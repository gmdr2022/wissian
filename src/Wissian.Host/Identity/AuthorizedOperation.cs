using Microsoft.EntityFrameworkCore;
using Npgsql;
using Wissian.Core.Audit;
using Wissian.Core.Identity;
using Wissian.Persistence;

namespace Wissian.Host.Identity;

public sealed record AuthorizedScope(Guid ActorId, Guid OrganizationId, long SessionVersion);

public sealed partial class SessionAuthority
{
    // A scoped operation holds fresh authority through its effect and audit commit. The callback
    // is trusted compiled host code, never a client-supplied handler or SQL statement.
    public async Task<IResult> RunAsync(HttpContext context, Guid organization, string module, string capability,
        long? expectedSessionVersion, Func<AuthorizedScope, Task<IResult>> operation)
    {
        if (!TryIds(context.User, out var actor, out var sessionId)) return Results.Unauthorized();
        if (organization == Guid.Empty) return Results.NotFound();
        var ct = context.RequestAborted;
        await using var transaction = await ScopedTransaction.BeginAsync(db, actor, organization, ct);
        try
        {
            var sessionText = sessionId.ToString();
            await db.Database.ExecuteSqlInterpolatedAsync($"SELECT set_config('wissian.session_id', {sessionText}, true), set_config('lock_timeout', '5000', true)", ct);
            await db.Database.ExecuteSqlRawAsync("SELECT wissian.lock_current_authority()", ct);
            var session = await CurrentAsync(actor, sessionId, ct);
            if (session is null) return Results.Unauthorized();
            if (session.OrganizationId != organization) return Results.NotFound();
            var facts = await LoadFacts(session, ct);
            if (facts is null || !OrganizationAccess.Allows(session, facts.Member, facts.Organization, actor, true,
                clock.GetUtcNow(), catalog, module, capability, facts.Eligible, facts.Enabled, facts.Permissions))
                return Results.NotFound();
            if (expectedSessionVersion is not null && expectedSessionVersion != session.Version)
                return Results.Conflict(new { code = "session_changed" });
            var result = await operation(new(actor, organization, session.Version));
            if (result is IStatusCodeHttpResult { StatusCode: >= 400 }) return result;
            await db.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
            return result;
        }
        catch (DbUpdateConcurrencyException) { return Results.Conflict(new { code = "resource_changed" }); }
        catch (DbUpdateException e) when (e.InnerException is PostgresException { SqlState: "23505" })
        { return Results.Conflict(new { code = "command_conflict" }); }
        catch (DbUpdateException e) when (e.InnerException is PostgresException { SqlState: "42501" })
        { return Results.NotFound(); }
        catch (PostgresException e) when (e.SqlState is "55P03" or "40P01")
        { return Results.Json(new { code = "retry_request" }, statusCode: 503); }
    }

    public void Record(AuthorizedScope scope, string action, string resourceType, Guid resource, HttpContext context)
    {
        if (db.Database.CurrentTransaction is null) throw new InvalidOperationException("Auditoria requer transação.");
        db.AuditEvents.Add(new AuditEvent(Guid.NewGuid(), clock.GetUtcNow(), scope.ActorId, scope.OrganizationId,
            action, resourceType, resource.ToString(), "applied", context.TraceIdentifier, scope.SessionVersion));
    }
}
