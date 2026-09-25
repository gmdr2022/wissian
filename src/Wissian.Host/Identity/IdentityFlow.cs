using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Wissian.Core.Audit;
using Wissian.Core.Identity;
using Wissian.Persistence;

namespace Wissian.Host.Identity;

public sealed record PasswordLogin(string Identifier, string Password);
public sealed record AuthenticatorLogin(string Code);
public sealed record ContextSelection(Guid OrganizationId, Guid MembershipId, long ExpectedSessionVersion, Guid CommandId);

public sealed class IdentityFlow(UserManager<WissianUser> users, SignInManager<WissianUser> signIn,
    WissianDbContext db, TimeProvider clock, UnknownPasswordCheck unknown,
    ILogger<IdentityFlow> logger)
{
    private static IResult InvalidCredentials() => Results.Json(new { code = "invalid_credentials" }, statusCode: 401);

    public async Task<IResult> LoginAsync(PasswordLogin request, HttpContext context)
    {
        if (context.User.Identity?.IsAuthenticated == true) return Results.Conflict(new { code = "already_authenticated" });
        if (string.IsNullOrWhiteSpace(request.Identifier) || request.Identifier.Length > 256
            || string.IsNullOrEmpty(request.Password) || request.Password.Length > 1024) return InvalidCredentials();
        // Clear a prior unfinished challenge before checking another identity.
        await context.SignOutAsync(IdentityConstants.TwoFactorUserIdScheme);
        var identifier = request.Identifier.Trim();
        var user = identifier.Contains('@') ? await users.FindByEmailAsync(identifier) : await users.FindByNameAsync(identifier);
        if (user is null || !user.IsActive || !await users.IsEmailConfirmedAsync(user) || await users.IsLockedOutAsync(user))
        {
            unknown.Verify(request.Password);
            LogFailure(context);
            return InvalidCredentials();
        }
        var result = await signIn.PasswordSignInAsync(user, request.Password, isPersistent: false, lockoutOnFailure: true);
        if (result.RequiresTwoFactor) return Results.Json(new { code = "mfa_required" }, statusCode: 202);
        if (!result.Succeeded)
        {
            LogFailure(context);
            return InvalidCredentials();
        }
        return await CompleteAsync(user, context, "password");
    }

    public async Task<IResult> AuthenticatorAsync(AuthenticatorLogin request, HttpContext context)
    {
        if (context.User.Identity?.IsAuthenticated == true) return Results.Conflict(new { code = "already_authenticated" });
        if (string.IsNullOrWhiteSpace(request.Code) || request.Code.Length > 16) return InvalidCredentials();
        var user = await signIn.GetTwoFactorAuthenticationUserAsync();
        if (user is null || !user.IsActive) return InvalidCredentials();
        var result = await signIn.TwoFactorAuthenticatorSignInAsync(request.Code.Replace(" ", "").Replace("-", ""),
            isPersistent: false, rememberClient: false);
        if (!result.Succeeded) { LogFailure(context); return InvalidCredentials(); }
        return await CompleteAsync(user, context, "mfa");
    }

    public async Task<IResult> CancelAuthenticatorAsync(HttpContext context)
    {
        await context.SignOutAsync(IdentityConstants.TwoFactorUserIdScheme);
        return Results.NoContent();
    }

    private async Task<IResult> CompleteAsync(WissianUser user, HttpContext context, string method)
    {
        // The framework's preliminary ticket has no session_id and is rejected by our cookie validator.
        // Only the persisted session below produces an acceptable application ticket.
        try
        {
            var now = clock.GetUtcNow();
            var session = new HumanSession(Guid.NewGuid(), user.Id, now, now.Add(IdentitySetup.SessionLifetime));
            await using var transaction = await ScopedTransaction.BeginAsync(db, user.Id, cancellationToken: context.RequestAborted);
            db.Sessions.Add(session);
            Audit(session, null, "identity.login", method, context);
            await db.SaveChangesAsync(context.RequestAborted);
            await transaction.CommitAsync(context.RequestAborted);
            await signIn.SignInWithClaimsAsync(user, new AuthenticationProperties
            {
                IsPersistent = false, AllowRefresh = false, IssuedUtc = now, ExpiresUtc = session.ExpiresAtUtc
            }, [new Claim(IdentitySetup.SessionClaim, session.Id.ToString()), new Claim("amr", method)]);
            return Results.Ok(new { code = "authenticated" });
        }
        catch
        {
            await signIn.SignOutAsync();
            throw;
        }
    }

    public async Task<IResult> SelectAsync(ContextSelection request, HttpContext context)
    {
        if (!SessionAuthority.TryIds(context.User, out var userId, out var sessionId)) return Results.Unauthorized();
        if (request.OrganizationId == Guid.Empty || request.MembershipId == Guid.Empty
            || request.CommandId == Guid.Empty || request.ExpectedSessionVersion < 1)
            return Results.BadRequest(new { code = "invalid_context" });
        await using var transaction = await ScopedTransaction.BeginAsync(db, userId, cancellationToken: context.RequestAborted);
        // Serialize only commands of the same session, including retries after a lost response.
        var session = await db.Sessions.FromSqlInterpolated(
            $"SELECT * FROM wissian.sessions WHERE \"Id\" = {sessionId} AND \"UserId\" = {userId} FOR UPDATE")
            .AsNoTracking().SingleOrDefaultAsync(context.RequestAborted);
        if (session?.IsActiveAt(clock.GetUtcNow()) != true
            || !await db.Users.AsNoTracking().AnyAsync(u => u.Id == userId && u.IsActive, context.RequestAborted))
            return Results.Unauthorized();
        var now = clock.GetUtcNow();
        var membership = await db.Memberships.AsNoTracking().SingleOrDefaultAsync(m => m.OrganizationId == request.OrganizationId
            && m.Id == request.MembershipId && m.UserId == userId && m.IsActive && m.CreatedAtUtc <= now, context.RequestAborted);
        var organization = await db.Organizations.AsNoTracking().SingleOrDefaultAsync(o => o.Id == request.OrganizationId
            && o.IsActive && o.CreatedAtUtc <= now, context.RequestAborted);
        if (membership is null || organization is null) return Results.NotFound(new { code = "context_unavailable" });
        await ScopedTransaction.SetOrganizationAsync(db, organization.Id, context.RequestAborted);
        var requestHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(FormattableString.Invariant(
            $"v1|{request.OrganizationId:N}|{request.MembershipId:N}|{request.ExpectedSessionVersion}"))));
        // Authority above is checked before even looking for a replay receipt.
        var receipt = await db.ContextReceipts.AsNoTracking().SingleOrDefaultAsync(r => r.SessionId == sessionId
            && r.CommandId == request.CommandId, context.RequestAborted);
        if (receipt is not null)
        {
            if (receipt.RequestHash != requestHash) return Results.Conflict(new { code = "command_conflict" });
            if (session.Version != receipt.AppliedVersion || session.OrganizationId != receipt.OrganizationId
                || session.MembershipId != receipt.MembershipId || session.MembershipVersion != membership.Version
                || receipt.MembershipVersion != membership.Version)
                return Results.Conflict(new { code = "session_changed" });
            return SelectionResult(receipt);
        }
        if (session.Version != request.ExpectedSessionVersion) return Results.Conflict(new { code = "session_changed" });
        db.Sessions.Attach(session);
        var oldVersion = session.Version;
        session.SelectOrganization(membership, organization, now);
        if (session.Version != oldVersion)
        {
            Audit(session, organization.Id, "identity.context_selected", "allowed", context);
        }
        receipt = new(session.Id, request.CommandId, userId, organization.Id, membership.Id,
            membership.Version, requestHash, session.Version, now);
        try
        {
            // RLS on the receipt verifies the persisted selected session. Preserve this ordering
            // inside the same transaction, irrespective of EF's ordering of unrelated SQL batches.
            await db.SaveChangesAsync(context.RequestAborted);
            db.ContextReceipts.Add(receipt);
            await db.SaveChangesAsync(context.RequestAborted);
            await transaction.CommitAsync(context.RequestAborted);
        }
        catch (DbUpdateConcurrencyException) { return Results.Conflict(new { code = "session_changed" }); }
        catch (DbUpdateException error) when (error.InnerException is PostgresException { SqlState: "23505" })
        { return Results.Conflict(new { code = "command_conflict" }); }
        catch (DbUpdateException error) when (error.InnerException is PostgresException { SqlState: "42501" })
        { return Results.NotFound(new { code = "context_unavailable" }); }
        return SelectionResult(receipt);
    }

    public async Task<IResult> LogoutAsync(HttpContext context)
    {
        if (!SessionAuthority.TryIds(context.User, out var userId, out var sessionId)) return Results.Unauthorized();
        await using var transaction = await ScopedTransaction.BeginAsync(db, userId, cancellationToken: context.RequestAborted);
        var session = await db.Sessions.SingleOrDefaultAsync(s => s.Id == sessionId, context.RequestAborted);
        if (session?.IsActiveAt(clock.GetUtcNow()) == true)
        {
            session.Revoke(clock.GetUtcNow());
            Audit(session, null, "identity.logout", "revoked", context);
            try
            {
                await db.SaveChangesAsync(context.RequestAborted);
                await transaction.CommitAsync(context.RequestAborted);
            }
            catch (DbUpdateConcurrencyException) { return Results.Conflict(new { code = "session_changed" }); }
        }
        await signIn.SignOutAsync();
        return Results.NoContent();
    }

    private void Audit(HumanSession session, Guid? organizationId, string action, string outcome, HttpContext context) =>
        db.AuditEvents.Add(new AuditEvent(Guid.NewGuid(), clock.GetUtcNow(), session.UserId, organizationId,
            action, "human_session", session.Id.ToString(), outcome, context.TraceIdentifier, session.Version));

    private static IResult SelectionResult(ContextSelectionReceipt receipt) => Results.Ok(new
    {
        sessionId = receipt.SessionId, version = receipt.AppliedVersion,
        organizationId = receipt.OrganizationId, commandId = receipt.CommandId
    });

    // Anonymous failures are structured security events, never a submitted identifier or password.
    private void LogFailure(HttpContext context) =>
        logger.LogWarning("Identity authentication rejected; correlation {CorrelationId}", context.TraceIdentifier);
}
