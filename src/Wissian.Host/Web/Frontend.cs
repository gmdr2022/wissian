using Microsoft.Extensions.FileProviders;

namespace Wissian.Host.Web;

public static class Frontend
{
    public static void MapWissianFrontend(this WebApplication app)
    {
        var configured = app.Configuration["Wissian:WebRoot"];
        var root = configured ?? Path.Combine(AppContext.BaseDirectory, "wwwroot");
        // API-only composition remains available. An explicitly configured UI must be complete.
        if (configured is null && !Directory.Exists(root)) return;
        var assets = Path.Combine(root, "assets");
        if (!Path.IsPathFullyQualified(root) || !File.Exists(Path.Combine(root, "index.html")) || !Directory.Exists(assets))
            throw new InvalidOperationException("Wissian:WebRoot deve indicar um build completo e um caminho absoluto.");
        var files = new PhysicalFileProvider(assets);
        app.Lifetime.ApplicationStopped.Register(files.Dispose);
        app.Use(async (context, next) =>
        {
            context.Response.Headers.ContentSecurityPolicy = "default-src 'none'; script-src 'self'; style-src 'self'; "
                + "img-src 'self' data:; font-src 'self'; connect-src 'self'; base-uri 'none'; form-action 'self'; "
                + "frame-ancestors 'none'; object-src 'none'";
            context.Response.Headers["Referrer-Policy"] = "no-referrer";
            context.Response.Headers.XFrameOptions = "DENY";
            context.Response.Headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";
            await next(context);
        });
        // Publish only the generated asset directory, never arbitrary files next to index.html.
        app.UseStaticFiles(new StaticFileOptions { FileProvider = files, RequestPath = "/assets" });
        var index = Path.Combine(root, "index.html");
        app.MapGet("/", () => Results.File(index, "text/html; charset=utf-8")).ExcludeFromDescription();
        app.MapGet("/access", () => Results.File(index, "text/html; charset=utf-8")).ExcludeFromDescription();
        app.MapGet("/m/{**route}", () => Results.File(index, "text/html; charset=utf-8")).ExcludeFromDescription();
    }
}
