namespace ClubAL.Domain.Audit;

public sealed class AuditEvent
{
    private AuditEvent()
    {
    }

    public AuditEvent(
        Guid id,
        DateTimeOffset occurredAtUtc,
        string actorType,
        Guid? actorId,
        string action,
        string resourceType,
        string outcome,
        string correlationId,
        Guid? tenantId = null,
        Guid? unitId = null,
        string? resourceId = null,
        string? summaryCode = null)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Evento de auditoria requer identificador.", nameof(id));
        }

        Id = id;
        OccurredAtUtc = occurredAtUtc;
        ActorType = DomainText.Required(actorType, 40, nameof(actorType));
        ActorId = actorId;
        Action = DomainText.Required(action, 120, nameof(action));
        ResourceType = DomainText.Required(resourceType, 80, nameof(resourceType));
        ResourceId = NormalizeOptional(resourceId, 120, nameof(resourceId));
        Outcome = DomainText.Required(outcome, 40, nameof(outcome));
        CorrelationId = DomainText.Required(correlationId, 120, nameof(correlationId));
        TenantId = tenantId;
        UnitId = unitId;
        SummaryCode = NormalizeOptional(summaryCode, 120, nameof(summaryCode));
    }

    public Guid Id { get; private set; }

    public DateTimeOffset OccurredAtUtc { get; private set; }

    public Guid? TenantId { get; private set; }

    public Guid? UnitId { get; private set; }

    public string ActorType { get; private set; } = null!;

    public Guid? ActorId { get; private set; }

    public string Action { get; private set; } = null!;

    public string ResourceType { get; private set; } = null!;

    public string? ResourceId { get; private set; }

    public string Outcome { get; private set; } = null!;

    public string CorrelationId { get; private set; } = null!;

    public string? SummaryCode { get; private set; }

    private static string? NormalizeOptional(
        string? value,
        int maximumLength,
        string parameterName)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : DomainText.Required(value, maximumLength, parameterName);
    }
}
