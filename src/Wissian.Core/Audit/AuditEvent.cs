namespace Wissian.Core.Audit;

public sealed class AuditEvent
{
    private AuditEvent() { }

    public AuditEvent(Guid id, DateTimeOffset occurredAtUtc, Guid actorId,
        Guid? organizationId, string action, string resourceType, string? resourceId,
        string outcome, string correlationId, long authorityVersion)
    {
        if (id == Guid.Empty || actorId == Guid.Empty || organizationId == Guid.Empty)
            throw new ArgumentException("Evento de auditoria requer identificadores estáveis.");
        if (authorityVersion < 1) throw new ArgumentOutOfRangeException(nameof(authorityVersion));
        Id = id;
        OccurredAtUtc = occurredAtUtc.ToUniversalTime();
        ActorId = actorId;
        OrganizationId = organizationId;
        Action = DomainText.Required(action, 120, nameof(action));
        ResourceType = DomainText.Required(resourceType, 80, nameof(resourceType));
        ResourceId = resourceId is null ? null : DomainText.Required(resourceId, 120, nameof(resourceId));
        Outcome = DomainText.Required(outcome, 40, nameof(outcome));
        CorrelationId = DomainText.Required(correlationId, 120, nameof(correlationId));
        AuthorityVersion = authorityVersion;
    }

    public Guid Id { get; private set; }
    public DateTimeOffset OccurredAtUtc { get; private set; }
    public Guid ActorId { get; private set; }
    public Guid? OrganizationId { get; private set; }
    public string Action { get; private set; } = null!;
    public string ResourceType { get; private set; } = null!;
    public string? ResourceId { get; private set; }
    public string Outcome { get; private set; } = null!;
    public string CorrelationId { get; private set; } = null!;
    public long AuthorityVersion { get; private set; }
    public int SchemaVersion { get; private set; } = 1;
}
