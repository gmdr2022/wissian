using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Wissian.Core.Audit;
using Wissian.Persistence;

namespace Wissian.Host.Identity;

public sealed class SessionCookieEvents(SignInManager<WissianUser> signIn, WissianDbContext db,
    TimeProvider clock) : CookieAuthenticationEvents
{
    public override async Task ValidatePrincipal(CookieValidatePrincipalContext context)
    {
        var principal = context.Principal;
        if (!Guid.TryParse(principal?.FindFirstValue(ClaimTypes.NameIdentifier), out var userId)
            || userId == Guid.Empty
            || !Guid.TryParse(principal?.FindFirstValue(IdentitySetup.SessionClaim), out var sessionId) || sessionId == Guid.Empty)
        {
            await Reject(context);
            return;
        }
        var user = await signIn.ValidateSecurityStampAsync(principal);
        await using var transaction = await ScopedTransaction.BeginAsync(db, userId, cancellationToken: context.HttpContext.RequestAborted);
        var session = await db.Sessions.AsNoTracking().SingleOrDefaultAsync(s => s.Id == sessionId, context.HttpContext.RequestAborted);
        if (user?.IsActive == true && session?.IsActiveAt(clock.GetUtcNow()) == true) return;

        if (session?.IsActiveAt(clock.GetUtcNow()) == true)
        {
            db.Sessions.Attach(session);
            session.Revoke(clock.GetUtcNow());
            db.AuditEvents.Add(new AuditEvent(Guid.NewGuid(), clock.GetUtcNow(), userId, null,
                "identity.session_invalidated", "human_session", session.Id.ToString(), "revoked",
                context.HttpContext.TraceIdentifier, session.Version));
            try
            {
                await db.SaveChangesAsync(context.HttpContext.RequestAborted);
                await transaction.CommitAsync(context.HttpContext.RequestAborted);
            }
            catch (DbUpdateConcurrencyException) { /* Another revocation won; the cookie is rejected either way. */ }
        }
        await Reject(context);
    }

    private static async Task Reject(CookieValidatePrincipalContext context)
    {
        context.RejectPrincipal();
        await context.HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
    }

    public override Task RedirectToLogin(RedirectContext<CookieAuthenticationOptions> context)
    {
        context.Response.StatusCode = 401;
        return Task.CompletedTask;
    }
    public override Task RedirectToAccessDenied(RedirectContext<CookieAuthenticationOptions> context)
    {
        context.Response.StatusCode = 403;
        return Task.CompletedTask;
    }
}
