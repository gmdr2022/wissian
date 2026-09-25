namespace Wissian.Core.Identity;

public sealed class MembershipPermission
{
    private MembershipPermission() { }
    public MembershipPermission(Guid organizationId, Guid membershipId, string permission)
    {
        if (organizationId == Guid.Empty || membershipId == Guid.Empty)
            throw new ArgumentException("Escopo de permissão obrigatório.");
        OrganizationId = organizationId;
        MembershipId = membershipId;
        Permission = DomainText.Required(permission, 120, nameof(permission));
        if (Permission.Contains('*')) throw new ArgumentException("Permissão deve ser explícita.");
    }
    public Guid OrganizationId { get; private set; }
    public Guid MembershipId { get; private set; }
    public string Permission { get; private set; } = null!;
    public bool IsActive { get; private set; } = true;
    public long Version { get; private set; } = 1;
    public void Revoke()
    {
        if (!IsActive) return;
        IsActive = false;
        Version++;
    }
}
