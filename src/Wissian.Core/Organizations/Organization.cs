namespace Wissian.Core.Organizations;

public sealed class Organization
{
    private Organization() { }

    public Organization(Guid id, string slug, string displayName, DateTimeOffset createdAtUtc)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Organização requer identificador estável.", nameof(id));
        Id = id;
        Slug = DomainText.Required(slug, 80, nameof(slug)).ToLowerInvariant();
        if (!System.Text.RegularExpressions.Regex.IsMatch(Slug, "^[a-z0-9]+(?:-[a-z0-9]+)*$"))
            throw new ArgumentException("Identificador de organização inválido.", nameof(slug));
        DisplayName = DomainText.Required(displayName, 160, nameof(displayName));
        CreatedAtUtc = createdAtUtc.ToUniversalTime();
    }

    public Guid Id { get; private set; }
    public string Slug { get; private set; } = null!;
    public string DisplayName { get; private set; } = null!;
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public bool IsActive { get; private set; } = true;
    public long Version { get; private set; } = 1;

    public void Suspend()
    {
        if (!IsActive) return;
        IsActive = false;
        Version++;
    }
}
