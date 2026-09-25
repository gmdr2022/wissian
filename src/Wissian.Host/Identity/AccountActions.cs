using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Wissian.Core.Audit;
using Wissian.Persistence;

namespace Wissian.Host.Identity;

public sealed record CompleteAccountAction(Guid UserId, string Token, string NewPassword);
public sealed record AccountActionLink(string Purpose, Guid UserId, string Token, DateTimeOffset ExpiresAtUtc);

public static class AccountLinks
{
    public const string ActivationPurpose = "wissian.activate-account.v1";
    public const string ProtectionName = "Wissian.AccountAction";
    public static readonly TimeSpan Lifetime = TimeSpan.FromHours(1);

    // Trusted local operator only. No endpoint is mapped for issuance and no email is sent.
    public static async Task<AccountActionLink?> IssueAsync(UserManager<WissianUser> users, Guid userId, string purpose)
    {
        if (userId == Guid.Empty || purpose is not ("activation" or "recovery")) return null;
        var user = await users.FindByIdAsync(userId.ToString());
        if (user is null) return null;
        var expires = DateTimeOffset.UtcNow.Add(Lifetime);
        if (purpose == "activation" && !user.IsActive && !user.EmailConfirmed && user.PasswordHash is null)
            return new(purpose, userId, await users.GenerateUserTokenAsync(user, TokenOptions.DefaultProvider, ActivationPurpose), expires);
        if (purpose == "recovery" && user.IsActive && user.EmailConfirmed && user.PasswordHash is not null)
            return new(purpose, userId, await users.GeneratePasswordResetTokenAsync(user), expires);
        return null;
    }
}

public sealed class AccountActions(WissianDbContext db, UserManager<WissianUser> users,
    SignInManager<WissianUser> signIn, TimeProvider clock, IOptions<IdentityOptions> options,
    ILogger<AccountActions> logger)
{
    public async Task<IResult> CompleteAsync(CompleteAccountAction request, bool activation, HttpContext context)
    {
        if (context.User.Identity?.IsAuthenticated == true) return Results.Conflict(new { code = "sign_out_required" });
        if (request.UserId == Guid.Empty || string.IsNullOrEmpty(request.Token) || request.Token.Length > 4096
            || string.IsNullOrEmpty(request.NewPassword) || request.NewPassword.Length > 1024) return InvalidLink(context);

        // A verified action token is the authentication proof for this endpoint, not an organization grant.
        await using var transaction = await ScopedTransaction.BeginAsync(db, request.UserId, cancellationToken: context.RequestAborted);
        var user = await db.Users.FromSqlInterpolated(
            $"SELECT * FROM wissian.identity_users WHERE \"Id\" = {request.UserId} FOR UPDATE")
            .SingleOrDefaultAsync(context.RequestAborted);
        if (user is null) return InvalidLink(context);
        if (activation ? user.IsActive || user.EmailConfirmed || user.PasswordHash is not null
            : !user.IsActive || !user.EmailConfirmed || user.PasswordHash is null) return InvalidLink(context);
        var provider = activation ? TokenOptions.DefaultProvider : options.Value.Tokens.PasswordResetTokenProvider;
        var purpose = activation ? AccountLinks.ActivationPurpose : UserManager<WissianUser>.ResetPasswordTokenPurpose;
        if (!await users.VerifyUserTokenAsync(user, provider, purpose, request.Token)) return InvalidLink(context);

        var result = activation ? await users.AddPasswordAsync(user, request.NewPassword)
            : await users.ResetPasswordAsync(user, request.Token, request.NewPassword);
        if (!result.Succeeded)
            return Results.BadRequest(new { code = "password_not_accepted", errors = result.Errors.Select(e => e.Code).ToArray() });
        user.EmailConfirmed = true;
        user.IsActive = true;
        user.AccessFailedCount = 0;
        user.LockoutEnd = null;
        if (!(await users.UpdateAsync(user)).Succeeded) return Results.Conflict(new { code = "identity_changed" });

        var now = clock.GetUtcNow();
        var sessions = await db.Sessions.FromSqlInterpolated(
            $"SELECT * FROM wissian.sessions WHERE \"UserId\" = {user.Id} AND \"RevokedAtUtc\" IS NULL FOR UPDATE")
            .ToListAsync(context.RequestAborted);
        foreach (var session in sessions) session.Revoke(now < session.CreatedAtUtc ? session.CreatedAtUtc : now);
        db.AuditEvents.Add(new AuditEvent(Guid.NewGuid(), now, user.Id, null,
            activation ? "identity.account_activated" : "identity.password_reset", "identity_user", user.Id.ToString(),
            "completed", context.TraceIdentifier, 1));
        await db.SaveChangesAsync(context.RequestAborted);
        await transaction.CommitAsync(context.RequestAborted);
        await signIn.SignOutAsync();
        return Results.Ok(new { code = activation ? "account_activated" : "password_updated" });
    }

    private IResult InvalidLink(HttpContext context)
    {
        logger.LogWarning("Account action rejected; correlation {CorrelationId}", context.TraceIdentifier);
        return Results.BadRequest(new { code = "access_link_invalid" });
    }
}
