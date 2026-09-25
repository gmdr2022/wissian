using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Npgsql;
using Wissian.Core.Identity;
using Wissian.Core.Modularity;
using Wissian.Core.Organizations;
using Wissian.Host;
using Wissian.Host.Identity;
using Wissian.Persistence;
using Wissian.TestExtension;

namespace Wissian.Api.Tests;

public sealed record SyntheticPerson(Guid UserId, string Email, string Password, Guid OrganizationId, Guid MembershipId);
public sealed class LabClock : TimeProvider
{
    private TimeSpan offset;
    public override DateTimeOffset GetUtcNow() => DateTimeOffset.UtcNow.Add(offset);
    public void Advance(TimeSpan value) => offset += value;
    public void Reset() => offset = TimeSpan.Zero;
}

public sealed class ApiLab : IAsyncDisposable
{
    private readonly string adminConnection;
    private readonly string migratorPassword = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
    private readonly string runtimePassword = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
    private readonly X509Certificate2 certificate;
    public string KeyDirectory { get; } = Path.GetFullPath(Path.Combine(".local", "api-test-keys", Guid.NewGuid().ToString("N")));
    public LabClock Clock { get; } = new();
    public WebApplication App { get; private set; } = null!;
    public Uri Address { get; private set; } = null!;

    public ApiLab()
    {
        adminConnection = Environment.GetEnvironmentVariable("WISSIAN_TEST_ADMIN_CONNECTION")
            ?? throw new InvalidOperationException("Executar pelo laboratório sintético Test-Postgres.ps1 -Suite Api.");
        var options = new NpgsqlConnectionStringBuilder(adminConnection);
        if (options.Host != "127.0.0.1" || options.Username != "wissian_test_admin" || options.Database != "postgres")
            throw new InvalidOperationException("Apenas o cluster sintético novo em loopback é aceito.");
        using var key = RSA.Create(2048);
        var request = new CertificateRequest("CN=localhost", key, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        var san = new SubjectAlternativeNameBuilder();
        san.AddIpAddress(IPAddress.Loopback);
        request.CertificateExtensions.Add(san.Build());
        using var generated = request.CreateSelfSigned(DateTimeOffset.UtcNow.AddMinutes(-1), DateTimeOffset.UtcNow.AddDays(1));
        // Windows SChannel cannot serve a key from an ephemeral CNG handle. DefaultKeySet uses a
        // temporary user key container removed on certificate disposal; no certificate is trusted/installed.
        var pfx = generated.Export(X509ContentType.Pfx);
        try { certificate = X509CertificateLoader.LoadPkcs12(pfx, null, X509KeyStorageFlags.DefaultKeySet); }
        finally { CryptographicOperations.ZeroMemory(pfx); }
    }

    public string Connection(string role, string database = "wissian_api_synthetic") => new NpgsqlConnectionStringBuilder(adminConnection)
    {
        Database = database, Username = role,
        Password = role == "wissian_migrator" ? migratorPassword : runtimePassword,
        Pooling = true, MaxPoolSize = 4, MinPoolSize = 0, NoResetOnClose = false, IncludeErrorDetail = false
    }.ConnectionString;

    public WissianDbContext OwnerDb() => new(new DbContextOptionsBuilder<WissianDbContext>()
        .UseNpgsql(Connection("wissian_migrator"), p => p.SetPostgresVersion(18, 6)
            .MigrationsHistoryTable("schema_history", "wissian")).Options);

    public async Task StartAsync()
    {
        await using var admin = new NpgsqlConnection(adminConnection);
        await admin.OpenAsync();
        foreach (var sql in new[]
        {
            $"CREATE ROLE wissian_migrator LOGIN PASSWORD '{migratorPassword}' NOSUPERUSER NOCREATEDB NOCREATEROLE NOBYPASSRLS",
            $"CREATE ROLE wissian_runtime LOGIN PASSWORD '{runtimePassword}' NOSUPERUSER NOCREATEDB NOCREATEROLE NOINHERIT NOBYPASSRLS",
            "CREATE DATABASE wissian_api_synthetic OWNER wissian_migrator",
            "REVOKE ALL ON DATABASE wissian_api_synthetic FROM PUBLIC",
            "GRANT CONNECT ON DATABASE wissian_api_synthetic TO wissian_runtime"
        })
        {
            await using var command = new NpgsqlCommand(sql, admin);
            await command.ExecuteNonQueryAsync();
        }
        await using (var db = OwnerDb()) await db.Database.MigrateAsync();
        App = Build();
        await App.StartAsync();
        Address = new(App.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.Single());
    }

    public WebApplication Build(bool realRateLimit = false, bool ownerCredential = false,
        string database = "wissian_api_synthetic", bool persistentKeys = false, string? webRoot = null) => HostApplication.Build(
        ["--environment", "Testing", "--Logging:LogLevel:Default", "Critical",
         "--Wissian:Database", Connection(ownerCredential ? "wissian_migrator" : "wissian_runtime", database),
         "--Wissian:KeyDirectory", KeyDirectory], [FoundationProbe.SecuredRegistration], builder =>
        {
            if (webRoot is not null) builder.Configuration["Wissian:WebRoot"] = webRoot;
            builder.WebHost.ConfigureKestrel(server => server.Listen(IPAddress.Loopback, 0, endpoint => endpoint.UseHttps(certificate)));
            builder.Services.AddSingleton<TimeProvider>(Clock);
            if (!persistentKeys) builder.Services.AddSingleton<IDataProtectionProvider>(new EphemeralDataProtectionProvider());
            builder.Services.PostConfigureAll<CookieAuthenticationOptions>(options => options.TimeProvider = Clock);
            if (!realRateLimit)
            {
                // Separate test below exercises the real 8/minute policy; avoid making unrelated tests wait a minute.
                builder.Services.RemoveAll<IConfigureOptions<RateLimiterOptions>>();
                builder.Services.AddRateLimiter(options =>
                {
                    options.RejectionStatusCode = 429;
                    options.AddPolicy("identity", _ => RateLimitPartition.GetFixedWindowLimiter("synthetic-suite",
                        _ => new FixedWindowRateLimiterOptions { PermitLimit = 1000, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 }));
                });
            }
        });

    public async Task CreateOldSchemaAsync()
    {
        await using var admin = new NpgsqlConnection(adminConnection);
        await admin.OpenAsync();
        foreach (var sql in new[]
        {
            "CREATE DATABASE wissian_api_old_synthetic OWNER wissian_migrator",
            "REVOKE ALL ON DATABASE wissian_api_old_synthetic FROM PUBLIC",
            "GRANT CONNECT ON DATABASE wissian_api_old_synthetic TO wissian_runtime"
        })
        {
            await using var command = new NpgsqlCommand(sql, admin);
            await command.ExecuteNonQueryAsync();
        }
        await using var db = new WissianDbContext(new DbContextOptionsBuilder<WissianDbContext>()
            .UseNpgsql(Connection("wissian_migrator", "wissian_api_old_synthetic"), p =>
                p.SetPostgresVersion(18, 6).MigrationsHistoryTable("schema_history", "wissian")).Options);
        await db.GetService<IMigrator>().MigrateAsync("20260925154639_AuditMembershipBoundary");
        // Permit the version query without pretending the unapplied migration is recorded.
        await db.Database.ExecuteSqlRawAsync("GRANT SELECT ON wissian.schema_history TO wissian_runtime");
    }

    public LabClient Client(Uri? address = null) => new(address ?? Address, certificate.RawData);

    public async Task<SyntheticPerson> PersonAsync(bool membership = true, bool confirmed = true, bool pending = false,
        string organizationName = "Organização sintética")
    {
        var id = Guid.NewGuid();
        var email = id.ToString("N") + "@example.invalid";
        var password = "Aa1!" + Convert.ToHexString(RandomNumberGenerator.GetBytes(16));
        await using var scope = App.Services.CreateAsyncScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<WissianUser>>();
        var user = new WissianUser
        {
            Id = id, UserName = email, Email = email, EmailConfirmed = confirmed && !pending,
            IsActive = !pending, DisplayName = "Pessoa sintética", CreatedAtUtc = Clock.GetUtcNow()
        };
        Ensure((pending ? await users.CreateAsync(user) : await users.CreateAsync(user, password)).Succeeded, "Fixture de identidade inválida.");
        var organization = new Organization(Guid.NewGuid(), "example-" + id.ToString("N"), organizationName, Clock.GetUtcNow());
        var member = new Membership(Guid.NewGuid(), id, organization.Id, Clock.GetUtcNow());
        if (membership)
        {
            await using var db = OwnerDb();
            db.Organizations.Add(organization);
            db.Memberships.Add(member);
            db.OrganizationModules.Add(new OrganizationModule(organization.Id, "foundation-probe", true, true));
            db.Permissions.Add(new MembershipPermission(organization.Id, member.Id, "foundation-probe.inspect"));
            await db.SaveChangesAsync();
        }
        return new(id, email, password, organization.Id, member.Id);
    }

    public async Task ChangeAsync(Func<WissianDbContext, Task> change)
    {
        await using var db = OwnerDb();
        await change(db);
        await db.SaveChangesAsync();
    }

    public static void Ensure(bool condition, string message = "Oráculo não atendido")
    {
        if (!condition) throw new InvalidOperationException(message);
    }

    public async ValueTask DisposeAsync()
    {
        if (App is not null) { await App.StopAsync(); await App.DisposeAsync(); }
        NpgsqlConnection.ClearAllPools();
        certificate.Dispose();
    }
}

public sealed class LabClient : IDisposable
{
    public CookieContainer Cookies { get; } = new();
    public HttpClient Http { get; }
    public string? Csrf { get; private set; }
    public LabClient(Uri address, byte[] certificate)
    {
        var handler = new HttpClientHandler
        {
            CookieContainer = Cookies, AllowAutoRedirect = false,
            ServerCertificateCustomValidationCallback = (request, presented, _, _) => request.RequestUri!.IsLoopback
                && presented is not null && presented.RawData.AsSpan().SequenceEqual(certificate)
        };
        Http = new(handler) { BaseAddress = address, Timeout = TimeSpan.FromSeconds(20) };
    }
    public async Task RefreshCsrfAsync()
    {
        using var response = await Http.GetAsync("/api/identity/csrf");
        await Status(response, 200);
        Csrf = (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("token").GetString();
    }
    public Task<HttpResponseMessage> PostAsync(string path, object? payload, bool csrf = true)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, path);
        if (payload is not null) request.Content = JsonContent.Create(payload);
        if (csrf && Csrf is not null) request.Headers.Add(IdentitySetup.CsrfHeader, Csrf);
        return Http.SendAsync(request);
    }
    public async Task LoginAsync(SyntheticPerson person)
    {
        await RefreshCsrfAsync();
        using var response = await PostAsync("/api/identity/login", new { identifier = person.Email, password = person.Password });
        await Status(response, 200);
        await RefreshCsrfAsync();
    }
    public async Task<SessionView> SessionAsync()
    {
        using var response = await Http.GetAsync("/api/identity/session");
        await Status(response, 200);
        return (await response.Content.ReadFromJsonAsync<SessionView>())!;
    }
    public async Task SelectAsync(SyntheticPerson person)
    {
        var session = await SessionAsync();
        using var response = await PostAsync("/api/identity/context", new
        {
            organizationId = person.OrganizationId, membershipId = person.MembershipId, commandId = Guid.NewGuid(), expectedSessionVersion = session.Version
        });
        await Status(response, 200);
    }
    public void CopySessionFrom(LabClient source) => Cookies.Add(Http.BaseAddress!, new Cookie(IdentitySetup.SessionCookie,
        source.Cookies.GetCookies(source.Http.BaseAddress!)[IdentitySetup.SessionCookie]!.Value, "/") { Secure = true, HttpOnly = true });
    public static async Task Status(HttpResponseMessage response, int status)
    {
        if ((int)response.StatusCode != status)
            throw new InvalidOperationException($"HTTP esperado {status}, recebido {(int)response.StatusCode}: {await response.Content.ReadAsStringAsync()}");
    }
    public void Dispose() => Http.Dispose();
}
