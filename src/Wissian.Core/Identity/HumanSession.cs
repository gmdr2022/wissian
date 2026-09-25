using Wissian.Core.Organizations;

namespace Wissian.Core.Identity;

public sealed class HumanSession
{
    private HumanSession() { }

    public HumanSession(Guid id, Guid userId, DateTimeOffset createdAtUtc, DateTimeOffset expiresAtUtc)
    {
        if (id == Guid.Empty || userId == Guid.Empty)
            throw new ArgumentException("Sessão requer identificadores estáveis.");
        if (expiresAtUtc <= createdAtUtc)
            throw new ArgumentOutOfRangeException(nameof(expiresAtUtc));
        Id = id;
        UserId = userId;
        CreatedAtUtc = createdAtUtc.ToUniversalTime();
        ExpiresAtUtc = expiresAtUtc.ToUniversalTime();
    }

    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset ExpiresAtUtc { get; private set; }
    public DateTimeOffset? RevokedAtUtc { get; private set; }
    public Guid? OrganizationId { get; private set; }
    public Guid? MembershipId { get; private set; }
    public long? MembershipVersion { get; private set; }
    public long Version { get; private set; } = 1;

    public bool IsActiveAt(DateTimeOffset nowUtc) =>
        RevokedAtUtc is null && CreatedAtUtc <= nowUtc && nowUtc < ExpiresAtUtc;

    // The adapter must load current membership and organization from its trusted store.
    // This mutation does not authenticate a caller or grant authority to a request.
    public void SelectOrganization(Membership membership, Organization organization, DateTimeOffset nowUtc)
    {
        ArgumentNullException.ThrowIfNull(membership);
        ArgumentNullException.ThrowIfNull(organization);
        if (!IsActiveAt(nowUtc) || !membership.IsActive || !organization.IsActive
            || membership.CreatedAtUtc > nowUtc || organization.CreatedAtUtc > nowUtc
            || membership.UserId != UserId || membership.OrganizationId != organization.Id)
            throw new InvalidOperationException("Contexto indisponível.");
        if (OrganizationId == organization.Id && MembershipId == membership.Id
            && MembershipVersion == membership.Version) return;
        OrganizationId = organization.Id;
        MembershipId = membership.Id;
        MembershipVersion = membership.Version;
        Version++;
    }

    public void Revoke(DateTimeOffset nowUtc)
    {
        if (RevokedAtUtc is not null) return;
        if (nowUtc < CreatedAtUtc) throw new ArgumentOutOfRangeException(nameof(nowUtc));
        RevokedAtUtc = nowUtc.ToUniversalTime();
        OrganizationId = null;
        MembershipId = null;
        MembershipVersion = null;
        Version++;
    }
}
