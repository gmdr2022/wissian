namespace Wissian.Core.Identity;

public sealed class Membership
{
    private Membership() { }

    public Membership(Guid id, Guid userId, Guid organizationId, DateTimeOffset createdAtUtc)
    {
        if (id == Guid.Empty || userId == Guid.Empty || organizationId == Guid.Empty)
            throw new ArgumentException("Vínculo requer identificadores estáveis.");
        Id = id;
        UserId = userId;
        OrganizationId = organizationId;
        CreatedAtUtc = createdAtUtc.ToUniversalTime();
    }

    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid OrganizationId { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public bool IsActive { get; private set; } = true;
    public long Version { get; private set; } = 1;

    public void Revoke()
    {
        if (!IsActive) return;
        IsActive = false;
        Version++;
    }
}
