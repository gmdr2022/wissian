namespace ClubAL.Domain.Tenancy;

public sealed class Tenant
{
    private Tenant()
    {
    }

    public Tenant(
        Guid id,
        string slug,
        string displayName,
        DeploymentEnvironment environment,
        DateTimeOffset createdAtUtc)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Tenant requer identificador estável.", nameof(id));
        }

        Id = id;
        Slug = DomainText.Required(slug, 80, nameof(slug)).ToLowerInvariant();
        DisplayName = DomainText.Required(displayName, 160, nameof(displayName));
        Environment = environment;
        CreatedAtUtc = createdAtUtc;
        IsActive = true;
        Version = 1;
    }

    public Guid Id { get; private set; }

    public string Slug { get; private set; } = null!;

    public string DisplayName { get; private set; } = null!;

    public DeploymentEnvironment Environment { get; private set; }

    public bool IsActive { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public long Version { get; private set; }
}
