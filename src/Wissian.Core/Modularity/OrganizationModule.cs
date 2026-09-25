namespace Wissian.Core.Modularity;

public sealed class OrganizationModule
{
    private OrganizationModule() { }
    public OrganizationModule(Guid organizationId, string moduleId, bool eligible, bool enabled)
    {
        if (organizationId == Guid.Empty) throw new ArgumentException("Organização obrigatória.");
        OrganizationId = organizationId;
        ModuleId = DomainText.Required(moduleId, 80, nameof(moduleId));
        IsEligible = eligible;
        IsEnabled = enabled;
    }
    public Guid OrganizationId { get; private set; }
    public string ModuleId { get; private set; } = null!;
    public bool IsEligible { get; private set; }
    public bool IsEnabled { get; private set; }
    public long Version { get; private set; } = 1;
    public void Suspend()
    {
        if (!IsEnabled) return;
        IsEnabled = false;
        Version++;
    }
}
