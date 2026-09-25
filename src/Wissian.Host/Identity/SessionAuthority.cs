using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Wissian.Core.Identity;
using Wissian.Core.Modularity;
using Wissian.Persistence;

namespace Wissian.Host.Identity;

public sealed record OrganizationChoice(Guid OrganizationId, Guid MembershipId, string DisplayName);
public sealed record ActiveContext(Guid OrganizationId, Guid MembershipId, string DisplayName,
    IReadOnlyList<string> Capabilities);
public sealed record SessionView(Guid SessionId, long Version, DateTimeOffset ExpiresAtUtc,
    string DisplayName, ActiveContext? Context, IReadOnlyList<OrganizationChoice> Organizations);

public sealed class SessionAuthority(WissianDbContext db, TimeProvider clock, ModuleCatalog catalog)
{
    internal static bool TryIds(ClaimsPrincipal principal, out Guid userId, out Guid sessionId)
    {
        userId = Guid.Empty;
        sessionId = Guid.Empty;
        return principal.Identity?.IsAuthenticated == true
            && Guid.TryParse(principal.FindFirstValue(ClaimTypes.NameIdentifier), out userId)
            && userId != Guid.Empty
            && Guid.TryParse(principal.FindFirstValue(IdentitySetup.SessionClaim), out sessionId)
            && sessionId != Guid.Empty;
    }

    // Must be called inside this user's actor-scoped transaction. No tracked or cached authority.
    internal async Task<HumanSession?> CurrentAsync(Guid userId, Guid sessionId, CancellationToken ct)
    {
        if (!await db.Users.AsNoTracking().AnyAsync(u => u.Id == userId && u.IsActive, ct)) return null;
        var session = await db.Sessions.AsNoTracking().SingleOrDefaultAsync(s => s.Id == sessionId && s.UserId == userId, ct);
        return session?.IsActiveAt(clock.GetUtcNow()) == true ? session : null;
    }

    public async Task<SessionView?> GetAsync(ClaimsPrincipal principal, CancellationToken ct)
    {
        if (!TryIds(principal, out var userId, out var sessionId)) return null;
        await using var transaction = await ScopedTransaction.BeginAsync(db, userId, cancellationToken: ct);
        var session = await CurrentAsync(userId, sessionId, ct);
        if (session is null) return null;
        var now = clock.GetUtcNow();
        var choices = await (from member in db.Memberships.AsNoTracking()
                             join organization in db.Organizations.AsNoTracking() on member.OrganizationId equals organization.Id
                             where member.UserId == userId && member.IsActive && organization.IsActive
                                 && member.CreatedAtUtc <= now && organization.CreatedAtUtc <= now
                             orderby organization.DisplayName
                             select new OrganizationChoice(organization.Id, member.Id, organization.DisplayName)).ToListAsync(ct);
        ActiveContext? context = null;
        if (session.OrganizationId is Guid organizationId)
        {
            await ScopedTransaction.SetOrganizationAsync(db, organizationId, ct);
            var facts = await LoadFacts(session, ct);
            if (facts is not null)
            {
                var capabilities = catalog.Modules.Values.SelectMany(m => m.Capabilities.Values
                    .Where(cap => OrganizationAccess.Allows(session, facts.Member, facts.Organization,
                        userId, true, now, catalog, m.Id, cap.Id, facts.Eligible, facts.Enabled, facts.Permissions))
                    .Select(cap => cap.Id)).Order(StringComparer.Ordinal).ToArray();
                context = new(organizationId, facts.Member.Id, facts.Organization.DisplayName, capabilities);
            }
        }
        var displayName = await db.Users.AsNoTracking().Where(u => u.Id == userId).Select(u => u.DisplayName).SingleAsync(ct);
        return new(session.Id, session.Version, session.ExpiresAtUtc, displayName, context, choices);
    }

    // Read-only endpoint check. Commands must keep their authorization and effect in one scoped transaction.
    public async Task<bool> CanReadAsync(ClaimsPrincipal principal, Guid resourceOrganization,
        string module, string capability, CancellationToken ct)
    {
        if (!TryIds(principal, out var userId, out var sessionId)) return false;
        await using var transaction = await ScopedTransaction.BeginAsync(db, userId, cancellationToken: ct);
        var session = await CurrentAsync(userId, sessionId, ct);
        if (session?.OrganizationId != resourceOrganization || resourceOrganization == Guid.Empty) return false;
        await ScopedTransaction.SetOrganizationAsync(db, resourceOrganization, ct);
        var facts = await LoadFacts(session, ct);
        return facts is not null && OrganizationAccess.Allows(session, facts.Member, facts.Organization,
            userId, true, clock.GetUtcNow(), catalog, module, capability, facts.Eligible, facts.Enabled, facts.Permissions);
    }

    private async Task<AccessFacts?> LoadFacts(HumanSession session, CancellationToken ct)
    {
        var member = await db.Memberships.AsNoTracking().SingleOrDefaultAsync(m => m.Id == session.MembershipId
            && m.OrganizationId == session.OrganizationId && m.UserId == session.UserId, ct);
        var organization = await db.Organizations.AsNoTracking().SingleOrDefaultAsync(o => o.Id == session.OrganizationId, ct);
        var now = clock.GetUtcNow();
        if (member is null || organization is null || !member.IsActive || !organization.IsActive
            || member.Version != session.MembershipVersion || member.CreatedAtUtc > now || organization.CreatedAtUtc > now) return null;
        var modules = await db.OrganizationModules.AsNoTracking().Where(m => m.OrganizationId == organization.Id).ToArrayAsync(ct);
        var permissions = await db.Permissions.AsNoTracking().Where(p => p.OrganizationId == organization.Id
            && p.MembershipId == member.Id && p.IsActive).Select(p => p.Permission).ToArrayAsync(ct);
        return new(member, organization,
            modules.Where(m => m.IsEligible).Select(m => m.ModuleId).ToHashSet(StringComparer.Ordinal),
            modules.Where(m => m.IsEnabled).Select(m => m.ModuleId).ToHashSet(StringComparer.Ordinal),
            permissions.ToHashSet(StringComparer.Ordinal));
    }

    private sealed record AccessFacts(Membership Member, Core.Organizations.Organization Organization,
        IReadOnlySet<string> Eligible, IReadOnlySet<string> Enabled, IReadOnlySet<string> Permissions);
}
