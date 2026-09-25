namespace ClubAL.Domain.IdentityAccess;

public sealed class Membership
{
    private Membership()
    {
    }

    public Membership(
        Guid id,
        Guid userId,
        Guid tenantId,
        DateTimeOffset createdAtUtc)
    {
        if (id == Guid.Empty || userId == Guid.Empty || tenantId == Guid.Empty)
        {
            throw new ArgumentException("Vínculo requer identificadores estáveis.");
        }

        Id = id;
        UserId = userId;
        TenantId = tenantId;
        CreatedAtUtc = createdAtUtc;
        IsActive = true;
        Version = 1;
    }

    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    public Guid TenantId { get; private set; }

    public bool IsActive { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public long Version { get; private set; }
}

public sealed class MembershipUnit
{
    private MembershipUnit()
    {
    }

    public MembershipUnit(Guid tenantId, Guid membershipId, Guid unitId)
    {
        if (tenantId == Guid.Empty || membershipId == Guid.Empty || unitId == Guid.Empty)
        {
            throw new ArgumentException("Escopo de unidade requer identificadores estáveis.");
        }

        TenantId = tenantId;
        MembershipId = membershipId;
        UnitId = unitId;
    }

    public Guid TenantId { get; private set; }

    public Guid MembershipId { get; private set; }

    public Guid UnitId { get; private set; }
}

public sealed class MembershipRole
{
    private MembershipRole()
    {
    }

    public MembershipRole(Guid tenantId, Guid membershipId, Guid roleId)
    {
        if (tenantId == Guid.Empty || membershipId == Guid.Empty || roleId == Guid.Empty)
        {
            throw new ArgumentException("Papel do vínculo requer identificadores estáveis.");
        }

        TenantId = tenantId;
        MembershipId = membershipId;
        RoleId = roleId;
    }

    public Guid TenantId { get; private set; }

    public Guid MembershipId { get; private set; }

    public Guid RoleId { get; private set; }
}
