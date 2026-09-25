using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Wissian.Core.Modularity;
using Wissian.Core.Organizations;
using Wissian.Host;
using Wissian.Host.Identity;

namespace Wissian.TestExtension;

// Test-only consumer; the production host does not reference this assembly.
public static class FoundationProbe
{
    public static ModuleManifest Manifest { get; } = new("foundation-probe", new(1, 0, 0), "Wissian tests",
        [new("core", new(1, 0, 0))], [new("foundation-probe.inspect", "foundation-probe.inspect")]);

    public static ModuleRegistration Registration { get; } = new(Manifest, endpoints =>
        endpoints.MapGet("/api/foundation-probe", () => Results.Ok(new
        {
            coreAssembly = typeof(Organization).Assembly.GetName().Name,
            contractVersion = Manifest.ContractVersion.ToString()
        })));

    public static ModuleRegistration SecuredRegistration { get; } = new(Manifest, endpoints =>
        endpoints.MapGet("/api/foundation-probe/{organizationId:guid}", async (Guid organizationId,
            HttpContext context, SessionAuthority authority) =>
            await authority.CanReadAsync(context.User, organizationId, Manifest.Id,
                "foundation-probe.inspect", context.RequestAborted)
                ? Results.Ok(new { organizationId }) : Results.NotFound())
            .RequireAuthorization());
}
