namespace Wissian.Core.Identity;

// Receipt for this concrete identity command, not a generic workflow or event store.
public sealed class ContextSelectionReceipt
{
    private ContextSelectionReceipt() { }
    public ContextSelectionReceipt(Guid sessionId, Guid commandId, Guid userId, Guid organizationId,
        Guid membershipId, long membershipVersion, string requestHash, long appliedVersion, DateTimeOffset createdAtUtc)
    {
        if (new[] { sessionId, commandId, userId, organizationId, membershipId }.Contains(Guid.Empty))
            throw new ArgumentException("Recibo requer identificadores válidos.");
        if (membershipVersion < 1 || appliedVersion < 1) throw new ArgumentOutOfRangeException(nameof(appliedVersion));
        if (string.IsNullOrEmpty(requestHash) || requestHash.Length != 64 || requestHash.Any(c => !Uri.IsHexDigit(c)))
            throw new ArgumentException("Resumo do comando inválido.", nameof(requestHash));
        SessionId = sessionId;
        CommandId = commandId;
        UserId = userId;
        OrganizationId = organizationId;
        MembershipId = membershipId;
        MembershipVersion = membershipVersion;
        RequestHash = requestHash;
        AppliedVersion = appliedVersion;
        CreatedAtUtc = createdAtUtc.ToUniversalTime();
    }
    public Guid SessionId { get; private set; }
    public Guid CommandId { get; private set; }
    public Guid UserId { get; private set; }
    public Guid OrganizationId { get; private set; }
    public Guid MembershipId { get; private set; }
    public long MembershipVersion { get; private set; }
    public string RequestHash { get; private set; } = null!;
    public long AppliedVersion { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
}
