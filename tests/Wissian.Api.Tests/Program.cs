using System.Buffers.Binary;
using System.Net;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Wissian.Api.Tests;
using Wissian.Core.Identity;
using Wissian.Host;
using Wissian.Host.Identity;
using Wissian.Persistence;
using static Wissian.Api.Tests.ApiLab;
using static Wissian.Api.Tests.LabClient;

await using var lab = new ApiLab();
await lab.StartAsync();
if (args is ["--browser"]) return await BrowserTests.RunAsync(lab);
if (args.Length != 0) throw new ArgumentException("Modo de teste desconhecido.");
var tests = new List<(string Name, Func<Task> Run)>();
void Test(string name, Func<Task> run) => tests.Add((name, run));
string Resource(SyntheticPerson person) => "/api/foundation-probe/" + person.OrganizationId;
object Select(SyntheticPerson person, long version) => new
{
    organizationId = person.OrganizationId, membershipId = person.MembershipId, commandId = Guid.NewGuid(), expectedSessionVersion = version
};

Test("M1-S3A-01 unauthenticated routes and cleartext transport denied", async () =>
{
    using var client = lab.Client();
    await Status(await client.Http.GetAsync("/api/identity/session"), 401);
    await Status(await client.Http.GetAsync("/api/foundation-probe/" + Guid.NewGuid()), 401);
    // The server only listens on TLS in this laboratory; an HTTP request cannot reach an identity route.
    using var plain = new HttpClient { BaseAddress = new UriBuilder(lab.Address) { Scheme = "http" }.Uri };
    try
    {
        using var response = await plain.GetAsync("/api/identity/csrf");
        Ensure(!response.IsSuccessStatusCode && !response.Headers.Contains("Set-Cookie"));
    }
    catch (HttpRequestException) { }
});
Test("M1-S3A-02 login CSRF and own secure cookies", async () =>
{
    var person = await lab.PersonAsync();
    using var client = lab.Client();
    var payload = new { identifier = person.Email, password = person.Password };
    await Status(await client.PostAsync("/api/identity/login", payload, csrf: false), 400);
    await client.RefreshCsrfAsync();
    using var login = await client.PostAsync("/api/identity/login", payload);
    await Status(login, 200);
    var cookie = login.Headers.GetValues("Set-Cookie").Last(c => c.StartsWith(IdentitySetup.SessionCookie + "=", StringComparison.Ordinal));
    Ensure(cookie.Contains("secure", StringComparison.OrdinalIgnoreCase) && cookie.Contains("httponly", StringComparison.OrdinalIgnoreCase)
        && cookie.Contains("samesite=strict", StringComparison.OrdinalIgnoreCase) && !cookie.Contains("domain=", StringComparison.OrdinalIgnoreCase));
    var session = await client.SessionAsync();
    Ensure(session.Context is null && session.Organizations.Count == 1 && session.Organizations[0].OrganizationId == person.OrganizationId);
    Ensure(session.ExpiresAtUtc <= lab.Clock.GetUtcNow().AddHours(8));
    // Anonymous antiforgery tokens cannot be replayed after authentication.
    await Status(await client.PostAsync("/api/identity/context", Select(person, session.Version)), 400);
    await client.RefreshCsrfAsync();
    await client.SelectAsync(person);
});
Test("M1-S3A-03 two organizations, server-owned context and foreign resource denial", async () =>
{
    var a = await lab.PersonAsync();
    var b = await lab.PersonAsync();
    using var first = lab.Client();
    using var second = lab.Client();
    await first.LoginAsync(a);
    await second.LoginAsync(b);
    await Status(await first.Http.GetAsync(Resource(a)), 404);
    await first.SelectAsync(a);
    await second.SelectAsync(b);
    await Status(await first.Http.GetAsync(Resource(a)), 200);
    await Status(await second.Http.GetAsync(Resource(b)), 200);
    first.Http.DefaultRequestHeaders.Add("X-Organization-Id", b.OrganizationId.ToString());
    await Status(await first.Http.GetAsync(Resource(b)), 404);
    await Status(await second.Http.GetAsync(Resource(a)), 404);
    var session = await first.SessionAsync();
    using var foreign = await first.PostAsync("/api/identity/context", Select(b, session.Version));
    using var missing = await first.PostAsync("/api/identity/context", new { organizationId = Guid.NewGuid(), membershipId = Guid.NewGuid(), commandId = Guid.NewGuid(), expectedSessionVersion = session.Version });
    await Status(foreign, 404);
    await Status(missing, 404);
    Ensure(await foreign.Content.ReadAsStringAsync() == await missing.Content.ReadAsStringAsync(), "Negação revelou existência.");
    Ensure((await first.SessionAsync()).Context!.OrganizationId == a.OrganizationId);
});
Test("M1-S3A-04 platform label grants no operational organization", async () =>
{
    var admin = await lab.PersonAsync(membership: false);
    var target = await lab.PersonAsync();
    await using (var scope = lab.App.Services.CreateAsyncScope())
    {
        var users = scope.ServiceProvider.GetRequiredService<UserManager<WissianUser>>();
        var user = (await users.FindByIdAsync(admin.UserId.ToString()))!;
        Ensure((await users.AddClaimAsync(user, new("role", "platform-administrator"))).Succeeded);
    }
    using var client = lab.Client();
    await client.LoginAsync(admin);
    var session = await client.SessionAsync();
    Ensure(session.Organizations.Count == 0 && session.Context is null);
    await Status(await client.Http.GetAsync(Resource(target)), 404);
    await Status(await client.PostAsync("/api/identity/context", Select(target, session.Version)), 404);
});
Test("M1-S3A-05 permission revoked between HTTP calls", async () =>
{
    var person = await lab.PersonAsync();
    using var client = lab.Client();
    await client.LoginAsync(person);
    await client.SelectAsync(person);
    await Status(await client.Http.GetAsync(Resource(person)), 200);
    await lab.ChangeAsync(async db => (await db.Permissions.SingleAsync(p => p.MembershipId == person.MembershipId)).Revoke());
    await Status(await client.Http.GetAsync(Resource(person)), 404);
    Ensure((await client.SessionAsync()).Context!.Capabilities.Count == 0);
});
Test("M1-S3A-06 module disabled hides capability and denies direct URL", async () =>
{
    var person = await lab.PersonAsync();
    using var client = lab.Client();
    await client.LoginAsync(person);
    await client.SelectAsync(person);
    Ensure((await client.SessionAsync()).Context!.Capabilities.Contains("foundation-probe.inspect"));
    await lab.ChangeAsync(async db => (await db.OrganizationModules.SingleAsync(m => m.OrganizationId == person.OrganizationId)).Suspend());
    await Status(await client.Http.GetAsync(Resource(person)), 404);
    Ensure((await client.SessionAsync()).Context!.Capabilities.Count == 0);
});
Test("M1-S3A-07 membership revoked removes active context on next request", async () =>
{
    var person = await lab.PersonAsync();
    using var client = lab.Client();
    await client.LoginAsync(person);
    await client.SelectAsync(person);
    await lab.ChangeAsync(async db => (await db.Memberships.SingleAsync(m => m.Id == person.MembershipId)).Revoke());
    await Status(await client.Http.GetAsync(Resource(person)), 404);
    var session = await client.SessionAsync();
    Ensure(session.Context is null && session.Organizations.Count == 0);
    await Status(await client.PostAsync("/api/identity/context", Select(person, session.Version)), 404);
});
Test("M1-S3A-08 revoked session and copied cookie cannot regain access", async () =>
{
    var person = await lab.PersonAsync();
    using var client = lab.Client();
    using var copy = lab.Client();
    await client.LoginAsync(person);
    copy.CopySessionFrom(client);
    var session = await client.SessionAsync();
    await lab.ChangeAsync(async db => (await db.Sessions.SingleAsync(s => s.Id == session.SessionId)).Revoke(lab.Clock.GetUtcNow()));
    await Status(await client.Http.GetAsync("/api/identity/session"), 401);
    await Status(await copy.Http.GetAsync("/api/identity/session"), 401);
});
Test("M1-S3A-09 observed disabled user terminally revokes that session", async () =>
{
    var person = await lab.PersonAsync();
    using var client = lab.Client();
    using var copy = lab.Client();
    await client.LoginAsync(person);
    copy.CopySessionFrom(client);
    await lab.ChangeAsync(async db => (await db.Users.SingleAsync(u => u.Id == person.UserId)).IsActive = false);
    await Status(await client.Http.GetAsync("/api/identity/session"), 401);
    await lab.ChangeAsync(async db => (await db.Users.SingleAsync(u => u.Id == person.UserId)).IsActive = true);
    await Status(await copy.Http.GetAsync("/api/identity/session"), 401);
});
Test("M1-S3A-10 security stamp changed invalidates application cookie immediately", async () =>
{
    var person = await lab.PersonAsync();
    using var client = lab.Client();
    await client.LoginAsync(person);
    await using var scope = lab.App.Services.CreateAsyncScope();
    var users = scope.ServiceProvider.GetRequiredService<UserManager<WissianUser>>();
    Ensure((await users.UpdateSecurityStampAsync((await users.FindByIdAsync(person.UserId.ToString()))!)).Succeeded);
    await Status(await client.Http.GetAsync("/api/identity/session"), 401);
});
Test("M1-S3A-11 logout requires CSRF and revokes copied cookie", async () =>
{
    var person = await lab.PersonAsync();
    using var client = lab.Client();
    using var copy = lab.Client();
    await client.LoginAsync(person);
    await client.SelectAsync(person);
    copy.CopySessionFrom(client);
    await Status(await client.PostAsync("/api/identity/logout", null, csrf: false), 400);
    await Status(await client.Http.GetAsync("/api/identity/session"), 200);
    await Status(await client.Http.GetAsync("/api/identity/logout"), 405);
    await Status(await client.PostAsync("/api/identity/logout", null), 204);
    await Status(await copy.Http.GetAsync("/api/identity/session"), 401);
    await using var db = lab.OwnerDb();
    Ensure(await db.AuditEvents.CountAsync(a => a.ActorId == person.UserId && a.Action == "identity.logout") == 1);
});
Test("M1-S3A-12 absolute expiration is not renewed by activity", async () =>
{
    var person = await lab.PersonAsync();
    using var client = lab.Client();
    await client.LoginAsync(person);
    var expires = (await client.SessionAsync()).ExpiresAtUtc;
    try
    {
        lab.Clock.Advance(TimeSpan.FromHours(7));
        Ensure((await client.SessionAsync()).ExpiresAtUtc == expires);
        lab.Clock.Advance(TimeSpan.FromHours(1));
        await Status(await client.Http.GetAsync("/api/identity/session"), 401);
    }
    finally { lab.Clock.Reset(); }
});
Test("M1-S3A-13 MFA challenge cannot become a session without authenticator", async () =>
{
    var person = await lab.PersonAsync();
    string key;
    await using (var scope = lab.App.Services.CreateAsyncScope())
    {
        var users = scope.ServiceProvider.GetRequiredService<UserManager<WissianUser>>();
        var user = (await users.FindByIdAsync(person.UserId.ToString()))!;
        Ensure((await users.ResetAuthenticatorKeyAsync(user)).Succeeded);
        key = (await users.GetAuthenticatorKeyAsync(user))!;
        Ensure((await users.SetTwoFactorEnabledAsync(user, true)).Succeeded);
    }
    using var client = lab.Client();
    await client.RefreshCsrfAsync();
    await Status(await client.PostAsync("/api/identity/login", new { identifier = person.Email, password = person.Password }), 202);
    await Status(await client.Http.GetAsync("/api/identity/session"), 401);
    await Status(await client.PostAsync("/api/identity/mfa", new { code = "invalid" }), 401);
    await Status(await client.PostAsync("/api/identity/mfa", new { code = AuthenticatorCode(key) }), 200);
    await client.RefreshCsrfAsync();
    Ensure((await client.SessionAsync()).Context is null);
    await client.SelectAsync(person);
});
Test("M1-S3A-14 lockout, inactive, unconfirmed and unknown use generic rejection", async () =>
{
    var person = await lab.PersonAsync();
    var unconfirmed = await lab.PersonAsync(confirmed: false);
    var inactive = await lab.PersonAsync();
    await lab.ChangeAsync(async db => (await db.Users.SingleAsync(u => u.Id == inactive.UserId)).IsActive = false);
    using var client = lab.Client();
    await client.RefreshCsrfAsync();
    string? expected = null;
    for (var count = 0; count < 5; count++)
    {
        using var response = await client.PostAsync("/api/identity/login", new { identifier = person.Email, password = "Wrong1!password" });
        await Status(response, 401);
        expected = await response.Content.ReadAsStringAsync();
    }
    foreach (var credentials in new[] { (person.Email, person.Password), (unconfirmed.Email, unconfirmed.Password),
        (inactive.Email, inactive.Password), ("unknown@example.invalid", "Wrong1!password") })
    {
        using var response = await client.PostAsync("/api/identity/login", new { identifier = credentials.Item1, password = credentials.Item2 });
        await Status(response, 401);
        Ensure(await response.Content.ReadAsStringAsync() == expected);
    }
    await using var db = lab.OwnerDb();
    Ensure((await db.Users.SingleAsync(u => u.Id == person.UserId)).LockoutEnd > DateTimeOffset.UtcNow);
});
Test("M1-S3A-15 owner credential blocked before accepting traffic", async () =>
{
    await using var forbidden = lab.Build(ownerCredential: true);
    try { await forbidden.StartAsync(); }
    catch (InvalidOperationException error) when (error.Message.Contains("credencial runtime", StringComparison.Ordinal)) { return; }
    throw new InvalidOperationException("Host aceitou migrador.");
});
Test("M1-S3A-16 runtime configuration is mandatory", () =>
{
    try { HostApplication.Build([], []); }
    catch (InvalidOperationException error) when (error.Message.Contains("Wissian:Database", StringComparison.Ordinal)) { return Task.CompletedTask; }
    throw new InvalidOperationException("Host iniciou sem configuração.");
});
Test("M1-S3A-17 real per-IP limit rejects ninth authentication attempt", async () =>
{
    await using var app = lab.Build(realRateLimit: true);
    await app.StartAsync();
    var address = new Uri(app.Services.GetRequiredService<Microsoft.AspNetCore.Hosting.Server.IServer>()
        .Features.Get<Microsoft.AspNetCore.Hosting.Server.Features.IServerAddressesFeature>()!.Addresses.Single());
    using var client = lab.Client(address);
    await client.RefreshCsrfAsync();
    for (var i = 0; i < 8; i++)
        await Status(await client.PostAsync("/api/identity/login", new { identifier = "unknown@example.invalid", password = "Wrong1!password" }), 401);
    await Status(await client.PostAsync("/api/identity/login", new { identifier = "unknown@example.invalid", password = "Wrong1!password" }), 429);
    await app.StopAsync();
});
Test("M1-S3A-18 concurrent context selections expose conflict and one audited effect", async () =>
{
    var first = await lab.PersonAsync();
    var other = await lab.PersonAsync();
    var anotherMembership = Guid.NewGuid();
    await lab.ChangeAsync(db =>
    {
        db.Memberships.Add(new Membership(anotherMembership, first.UserId, other.OrganizationId, lab.Clock.GetUtcNow()));
        return Task.CompletedTask;
    });
    using var client = lab.Client();
    await client.LoginAsync(first);
    var session = await client.SessionAsync();
    var replies = await Task.WhenAll(client.PostAsync("/api/identity/context", Select(first, session.Version)),
        client.PostAsync("/api/identity/context", new { organizationId = other.OrganizationId, membershipId = anotherMembership, commandId = Guid.NewGuid(), expectedSessionVersion = session.Version }));
    Ensure(replies.Select(r => (int)r.StatusCode).Order().SequenceEqual([200, 409]), "Concorrência não produziu 200/409.");
    foreach (var reply in replies) reply.Dispose();
    await using var db = lab.OwnerDb();
    Ensure(await db.AuditEvents.CountAsync(a => a.ActorId == first.UserId && a.Action == "identity.context_selected") == 1);
});
Test("M1-S3A-19 unknown JSON fields and foreign membership pairing rejected", async () =>
{
    var person = await lab.PersonAsync();
    var other = await lab.PersonAsync();
    using var client = lab.Client();
    await client.RefreshCsrfAsync();
    await Status(await client.PostAsync("/api/identity/login", new { identifier = person.Email, password = person.Password, role = "administrator" }), 400);
    await client.LoginAsync(person);
    var session = await client.SessionAsync();
    await Status(await client.PostAsync("/api/identity/context", new { organizationId = person.OrganizationId, membershipId = other.MembershipId, commandId = Guid.NewGuid(), expectedSessionVersion = session.Version }), 404);
    Ensure((await client.SessionAsync()).Context is null);
});
Test("M1-S3A-20 outdated schema blocked without migration at host startup", async () =>
{
    await lab.CreateOldSchemaAsync();
    await using var outdated = lab.Build(database: "wissian_api_old_synthetic");
    try { await outdated.StartAsync(); }
    catch (InvalidOperationException error) when (error.Message.Contains("Schema incompatível", StringComparison.Ordinal)) { return; }
    throw new InvalidOperationException("Host aceitou schema antigo.");
});
Test("M1-S3A-21 protected keys survive restart and a different keyring rejects cookie", async () =>
{
    var person = await lab.PersonAsync();
    using var carried = lab.Client();
    await using (var app = lab.Build(persistentKeys: true))
    {
        await app.StartAsync();
        using var client = lab.Client(AppAddress(app));
        await client.LoginAsync(person);
        carried.CopySessionFrom(client);
        await app.StopAsync();
    }
    var files = Directory.GetFiles(lab.KeyDirectory, "key-*.xml");
    Ensure(files.Length > 0 && files.All(path => File.ReadAllText(path).Contains("encryptedSecret", StringComparison.Ordinal)),
        "Chaves persistidas sem proteção DPAPI.");
    await using (var restarted = lab.Build(persistentKeys: true))
    {
        await restarted.StartAsync();
        using var client = lab.Client(AppAddress(restarted));
        client.CopySessionFrom(carried);
        Ensure((await client.SessionAsync()).Organizations[0].OrganizationId == person.OrganizationId);
        await restarted.StopAsync();
    }
    // The main laboratory instance uses an independent keyring.
    await Status(await carried.Http.GetAsync("/api/identity/session"), 401);
});
Test("M1-S3A-22 organization suspension invalidates its context and capability", async () =>
{
    var person = await lab.PersonAsync();
    using var client = lab.Client();
    await client.LoginAsync(person);
    await client.SelectAsync(person);
    await lab.ChangeAsync(async db => (await db.Organizations.SingleAsync(o => o.Id == person.OrganizationId)).Suspend());
    await Status(await client.Http.GetAsync(Resource(person)), 404);
    var session = await client.SessionAsync();
    Ensure(session.Context is null && session.Organizations.Count == 0);
    await Status(await client.PostAsync("/api/identity/context", Select(person, session.Version)), 404);
});
Test("M1-S3A-23 modified ticket and oversized request cannot authenticate", async () =>
{
    var person = await lab.PersonAsync();
    using var client = lab.Client();
    await client.LoginAsync(person);
    var ticket = client.Cookies.GetCookies(client.Http.BaseAddress!)[IdentitySetup.SessionCookie]!;
    ticket.Value = "invalid" + ticket.Value[7..];
    client.Cookies.Add(client.Http.BaseAddress!, ticket);
    await Status(await client.Http.GetAsync("/api/identity/session"), 401);
    await client.RefreshCsrfAsync();
    await Status(await client.PostAsync("/api/identity/login", new { identifier = person.Email, password = new string('x', 40000) }), 413);
});
Test("M1-S3A-24 credential revocation invalidates a pending MFA challenge", async () =>
{
    var person = await lab.PersonAsync();
    var key = await EnableMfa(person);
    using var client = lab.Client();
    await client.RefreshCsrfAsync();
    await Status(await client.PostAsync("/api/identity/login", new { identifier = person.Email, password = person.Password }), 202);
    await using (var scope = lab.App.Services.CreateAsyncScope())
    {
        var users = scope.ServiceProvider.GetRequiredService<UserManager<WissianUser>>();
        Ensure((await users.UpdateSecurityStampAsync((await users.FindByIdAsync(person.UserId.ToString()))!)).Succeeded);
    }
    await Status(await client.PostAsync("/api/identity/mfa", new { code = AuthenticatorCode(key) }), 401);
    await Status(await client.Http.GetAsync("/api/identity/session"), 401);
});
Test("M1-S3A-25 MFA challenge expires in five minutes", async () =>
{
    var person = await lab.PersonAsync();
    var key = await EnableMfa(person);
    using var client = lab.Client();
    await client.RefreshCsrfAsync();
    await Status(await client.PostAsync("/api/identity/login", new { identifier = person.Email, password = person.Password }), 202);
    try
    {
        lab.Clock.Advance(TimeSpan.FromMinutes(5));
        await Status(await client.PostAsync("/api/identity/mfa", new { code = AuthenticatorCode(key) }), 401);
        await Status(await client.Http.GetAsync("/api/identity/session"), 401);
    }
    finally { lab.Clock.Reset(); }
});
Test("M1-S3A-26 suspend and reactivate without intervening request does not restore tickets", async () =>
{
    var person = await lab.PersonAsync();
    using var client = lab.Client();
    await client.LoginAsync(person);
    await lab.ChangeAsync(async db => (await db.Users.SingleAsync(u => u.Id == person.UserId)).IsActive = false);
    await lab.ChangeAsync(async db => (await db.Users.SingleAsync(u => u.Id == person.UserId)).IsActive = true);
    await Status(await client.Http.GetAsync("/api/identity/session"), 401);
    var mfaPerson = await lab.PersonAsync();
    var key = await EnableMfa(mfaPerson);
    using var pending = lab.Client();
    await pending.RefreshCsrfAsync();
    await Status(await pending.PostAsync("/api/identity/login", new { identifier = mfaPerson.Email, password = mfaPerson.Password }), 202);
    await lab.ChangeAsync(async db => (await db.Users.SingleAsync(u => u.Id == mfaPerson.UserId)).IsActive = false);
    await lab.ChangeAsync(async db => (await db.Users.SingleAsync(u => u.Id == mfaPerson.UserId)).IsActive = true);
    await Status(await pending.PostAsync("/api/identity/mfa", new { code = AuthenticatorCode(key) }), 401);
});

async Task<string> EnableMfa(SyntheticPerson person)
{
    await using var scope = lab.App.Services.CreateAsyncScope();
    var users = scope.ServiceProvider.GetRequiredService<UserManager<WissianUser>>();
    var user = (await users.FindByIdAsync(person.UserId.ToString()))!;
    Ensure((await users.ResetAuthenticatorKeyAsync(user)).Succeeded);
    Ensure((await users.SetTwoFactorEnabledAsync(user, true)).Succeeded);
    return (await users.GetAuthenticatorKeyAsync(user))!;
}

AccountAndReplayTests.Register(Test, lab, AuthenticatorCode);
var failures = 0;
var filter = Environment.GetEnvironmentVariable("WISSIAN_TEST_CASE");
if (!string.IsNullOrEmpty(filter)) tests = tests.Where(t => t.Name.StartsWith(filter + " ", StringComparison.Ordinal)).ToList();
if (tests.Count == 0) throw new InvalidOperationException("Caso de teste não encontrado.");
foreach (var test in tests)
{
    try { await test.Run(); Console.WriteLine("PASS " + test.Name); }
    catch (Exception error) { failures++; Console.WriteLine($"FAIL {test.Name}: {error.GetType().Name}: {error.Message}; cause: {error.GetBaseException().Message}"); }
}
Console.WriteLine($"API: {tests.Count - failures}/{tests.Count} PASS; {failures} FAIL");
return failures == 0 ? 0 : 1;

static Uri AppAddress(Microsoft.AspNetCore.Builder.WebApplication app) => new(app.Services
    .GetRequiredService<Microsoft.AspNetCore.Hosting.Server.IServer>()
    .Features.Get<Microsoft.AspNetCore.Hosting.Server.Features.IServerAddressesFeature>()!.Addresses.Single());

// RFC 6238 fixture only. The production endpoint uses ASP.NET Identity's authenticator provider.
static string AuthenticatorCode(string key)
{
    const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
    var bytes = new List<byte>();
    var buffer = 0;
    var bits = 0;
    foreach (var character in key.TrimEnd('=').ToUpperInvariant())
    {
        buffer = (buffer << 5) | alphabet.IndexOf(character);
        bits += 5;
        if (bits >= 8) { bits -= 8; bytes.Add((byte)(buffer >> bits)); }
    }
    Span<byte> counter = stackalloc byte[8];
    BinaryPrimitives.WriteInt64BigEndian(counter, DateTimeOffset.UtcNow.ToUnixTimeSeconds() / 30);
    var hash = HMACSHA1.HashData(bytes.ToArray(), counter);
    var offset = hash[^1] & 15;
    var number = BinaryPrimitives.ReadInt32BigEndian(hash.AsSpan(offset, 4)) & 0x7fffffff;
    return (number % 1000000).ToString("D6", System.Globalization.CultureInfo.InvariantCulture);
}
