using Wissian.Core.Modularity;
using Wissian.Core.Organizations;

namespace Wissian.Core.Identity;

// This policy operates on fresh server-loaded facts, never browser claims or cached grants.
// Resource classification and workflow-specific authorization remain the module's responsibility.
public static class OrganizationAccess
{
    public static bool Allows(HumanSession? session, Membership? membership, Organization? organization,
        Guid authenticatedUserId, bool userIsActive, DateTimeOffset nowUtc,
        ModuleCatalog catalog, string moduleId, string capabilityId,
        IReadOnlySet<string> eligibleModules, IReadOnlySet<string> enabledModules,
        IReadOnlySet<string> permissions)
    {
        if (!userIsActive || authenticatedUserId == Guid.Empty || session is null
            || membership is null || organization is null || !session.IsActiveAt(nowUtc)
            || session.UserId != authenticatedUserId || membership.UserId != authenticatedUserId
            || !membership.IsActive || !organization.IsActive
            || membership.CreatedAtUtc > nowUtc || organization.CreatedAtUtc > nowUtc
            || session.MembershipId != membership.Id || session.MembershipVersion != membership.Version
            || session.OrganizationId != organization.Id || membership.OrganizationId != organization.Id)
            return false;
        return catalog.Modules.TryGetValue(moduleId, out var manifest)
            && manifest.Capabilities.TryGetValue(capabilityId, out var capability)
            && catalog.IsOperational(moduleId, eligibleModules, enabledModules)
            && permissions.Contains(capability.Permission);
    }
}
