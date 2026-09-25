using Wissian.Core.Modularity;
using Wissian.Host.Identity;
using Wissian.Host.Web;

namespace Wissian.Host;

public sealed record ModuleRegistration(ModuleManifest Manifest, Action<IEndpointRouteBuilder> MapEndpoints,
    Action<WebApplicationBuilder>? Configure = null);

public static class HostApplication
{
    public static WebApplication Build(string[] args, IReadOnlyList<ModuleRegistration> extensions,
        Action<WebApplicationBuilder>? configure = null) => Compose(args, extensions, configure, true);

    // Explicit composition fixture: no identity routes or business endpoints are installed here.
    public static WebApplication BuildNeutral(string[] args, IReadOnlyList<ModuleRegistration> extensions) =>
        Compose(args, extensions, null, false);

    private static WebApplication Compose(string[] args, IReadOnlyList<ModuleRegistration> extensions,
        Action<WebApplicationBuilder>? configure, bool identity)
    {
        // Only compiled, reviewed composition supplies registrations. Configuration cannot load code.
        var registrations = extensions.ToArray();
        var catalog = new ModuleCatalog(registrations.Select(e => e.Manifest));
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddSingleton(catalog);
        builder.Services.AddProblemDetails();
        if (identity) builder.AddWissianIdentity();
        if (identity) foreach (var registration in registrations) registration.Configure?.Invoke(builder);
        configure?.Invoke(builder);
        var app = builder.Build();
        app.UseExceptionHandler();
        if (!app.Environment.IsDevelopment()) app.UseHsts();
        if (identity)
        {
            app.UseWissianIdentity();
            app.MapWissianFrontend();
        }
        app.MapGet("/health/live", () => Results.Ok(new { status = "alive" }));
        foreach (var registration in registrations) registration.MapEndpoints(app);
        return app;
    }
}
