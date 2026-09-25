using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Wissian.Persistence;

namespace Wissian.Host.Identity;

public sealed class MfaCookieEvents(UserManager<WissianUser> users) : CookieAuthenticationEvents
{
    private const string StampClaim = "wissian.challenge_stamp";

    public override async Task SigningIn(CookieSigningInContext context)
    {
        // Identity's temporary challenge identifies its user through ClaimTypes.Name.
        var id = context.Principal?.FindFirstValue(ClaimTypes.Name);
        var user = id is null ? null : await users.FindByIdAsync(id);
        if (user?.IsActive != true || context.Principal?.Identity is not ClaimsIdentity identity)
            throw new InvalidOperationException("Desafio de identidade indisponível.");
        identity.AddClaim(new Claim(StampClaim, await users.GetSecurityStampAsync(user)));
    }

    public override async Task ValidatePrincipal(CookieValidatePrincipalContext context)
    {
        var id = context.Principal?.FindFirstValue(ClaimTypes.Name);
        var user = id is null ? null : await users.FindByIdAsync(id);
        if (user?.IsActive == true
            && context.Principal?.FindFirstValue(StampClaim) == await users.GetSecurityStampAsync(user)) return;
        context.RejectPrincipal();
        await context.HttpContext.SignOutAsync(IdentityConstants.TwoFactorUserIdScheme);
    }
}
