using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Wissian.Persistence;

namespace Wissian.Host.Identity;

public static class IdentitySetup
{
    public const string SessionClaim = "wissian.session_id";
    public const string CsrfHeader = "X-CSRF-TOKEN";
    public const string SessionCookie = "__Host-Wissian.Session";
    public static readonly TimeSpan SessionLifetime = TimeSpan.FromHours(8);

    public static void AddWissianIdentity(this WebApplicationBuilder builder)
    {
        var connection = builder.Configuration["Wissian:Database"];
        var keyDirectory = builder.Configuration["Wissian:KeyDirectory"];
        if (string.IsNullOrWhiteSpace(connection) || string.IsNullOrWhiteSpace(keyDirectory))
            throw new InvalidOperationException("Configurar Wissian:Database (runtime) e Wissian:KeyDirectory antes de iniciar.");
        if (!Path.IsPathFullyQualified(keyDirectory))
            throw new InvalidOperationException("O diretório exclusivo de chaves deve ser absoluto.");
        var keys = builder.Services.AddDataProtection().SetApplicationName("Wissian")
            .PersistKeysToFileSystem(new DirectoryInfo(keyDirectory));
        if (OperatingSystem.IsWindows()) keys.ProtectKeysWithDpapi();
        else throw new PlatformNotSupportedException("Proteção das chaves fora do Windows requer configuração operacional explícita.");

        builder.WebHost.ConfigureKestrel(options => options.Limits.MaxRequestBodySize = 32 * 1024);
        builder.Services.ConfigureHttpJsonOptions(options =>
            options.SerializerOptions.UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow);
        builder.Services.AddSingleton(TimeProvider.System);
        builder.Services.AddDbContext<WissianDbContext>(options => options.UseNpgsql(connection,
            postgres => postgres.SetPostgresVersion(18, 6).MigrationsHistoryTable("schema_history", "wissian")));
        builder.Services.AddHostedService<RuntimeSchemaCheck>();
        builder.Services.AddIdentityCore<WissianUser>(options =>
        {
            options.Password.RequiredLength = 12;
            options.Password.RequiredUniqueChars = 6;
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = true;
            options.User.RequireUniqueEmail = true;
            options.SignIn.RequireConfirmedEmail = true;
            options.Lockout.AllowedForNewUsers = true;
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
        }).AddSignInManager().AddDefaultTokenProviders().AddEntityFrameworkStores<WissianDbContext>();
        builder.Services.Configure<DataProtectionTokenProviderOptions>(options =>
        {
            options.Name = AccountLinks.ProtectionName;
            options.TokenLifespan = AccountLinks.Lifetime;
        });
        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
            options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
            options.DefaultSignInScheme = IdentityConstants.ApplicationScheme;
        }).AddIdentityCookies();
        builder.Services.AddAuthorization();
        ConfigureCookie(builder.Services, IdentityConstants.ApplicationScheme, SessionCookie, SessionLifetime);
        ConfigureCookie(builder.Services, IdentityConstants.TwoFactorUserIdScheme, "__Host-Wissian.TwoFactor", TimeSpan.FromMinutes(5));
        ConfigureCookie(builder.Services, IdentityConstants.TwoFactorRememberMeScheme, "__Host-Wissian.TwoFactorRemember", TimeSpan.FromMinutes(5));
        ConfigureCookie(builder.Services, IdentityConstants.ExternalScheme, "__Host-Wissian.External", TimeSpan.FromMinutes(5));
        builder.Services.Configure<CookieAuthenticationOptions>(IdentityConstants.ApplicationScheme,
            options => options.EventsType = typeof(SessionCookieEvents));
        builder.Services.AddScoped<SessionCookieEvents>();
        builder.Services.Configure<CookieAuthenticationOptions>(IdentityConstants.TwoFactorUserIdScheme,
            options => options.EventsType = typeof(MfaCookieEvents));
        builder.Services.AddScoped<MfaCookieEvents>();
        builder.Services.AddScoped<IdentityFlow>();
        builder.Services.AddScoped<AccountActions>();
        builder.Services.AddScoped<SessionAuthority>();
        builder.Services.AddSingleton<UnknownPasswordCheck>();
        builder.Services.AddAntiforgery(options =>
        {
            options.HeaderName = CsrfHeader;
            options.Cookie.Name = "__Host-Wissian.Csrf";
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
            options.Cookie.SameSite = SameSiteMode.Strict;
            options.Cookie.Path = "/";
        });
        builder.Services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.AddPolicy("identity", context => RateLimitPartition.GetFixedWindowLimiter(
                context.Connection.RemoteIpAddress?.ToString() ?? "unknown", _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 8, Window = TimeSpan.FromMinutes(1), QueueLimit = 0, AutoReplenishment = true
                }));
        });
    }

    private static void ConfigureCookie(IServiceCollection services, string scheme, string name, TimeSpan lifetime) =>
        services.Configure<CookieAuthenticationOptions>(scheme, options =>
        {
            options.Cookie.Name = name;
            options.Cookie.Path = "/";
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.Cookie.SameSite = SameSiteMode.Strict;
            options.ExpireTimeSpan = lifetime;
            options.SlidingExpiration = false;
            options.Events.OnRedirectToLogin = context => { context.Response.StatusCode = 401; return Task.CompletedTask; };
            options.Events.OnRedirectToAccessDenied = context => { context.Response.StatusCode = 403; return Task.CompletedTask; };
        });

    public static void UseWissianIdentity(this WebApplication app)
    {
        app.Use(async (context, next) =>
        {
            context.Response.Headers.CacheControl = "no-store";
            context.Response.Headers.XContentTypeOptions = "nosniff";
            // No forwarded-header trust is enabled. The operational slice must name any trusted proxy.
            if (!context.Request.IsHttps && context.Request.Path != "/health/live")
            {
                context.Response.StatusCode = 400;
                return;
            }
            await next(context);
        });
        app.UseRateLimiter();
        app.UseAuthentication();
        app.UseAuthorization();
        IdentityEndpoints.Map(app);
    }
}
