using System.Collections.Frozen;
using System.Text.RegularExpressions;

namespace Wissian.Core.Modularity;

public sealed record ModuleDependency(string Id, Version ContractVersion);
public sealed record ModuleCapability(string Id, string Permission);

public sealed class ModuleManifest
{
    public ModuleManifest(string id, Version contractVersion, string owner,
        IEnumerable<ModuleDependency> dependencies, IEnumerable<ModuleCapability> capabilities)
    {
        Id = RequireId(id);
        ContractVersion = RequireVersion(contractVersion);
        Owner = DomainText.Required(owner, 120, nameof(owner));
        var dependencyList = dependencies.ToArray();
        foreach (var dependency in dependencyList)
        {
            RequireId(dependency.Id);
            RequireVersion(dependency.ContractVersion);
        }
        if (dependencyList.Select(d => d.Id).Distinct(StringComparer.Ordinal).Count() != dependencyList.Length)
            throw new ArgumentException("Dependência repetida.", nameof(dependencies));
        Dependencies = Array.AsReadOnly(dependencyList);
        var capabilityList = capabilities.ToArray();
        foreach (var capability in capabilityList)
        {
            RequireOwnedName(capability.Id);
            RequireOwnedName(capability.Permission);
        }
        if (capabilityList.Select(c => c.Id).Distinct(StringComparer.Ordinal).Count() != capabilityList.Length)
            throw new ArgumentException("Capacidade repetida.", nameof(capabilities));
        Capabilities = capabilityList.ToFrozenDictionary(c => c.Id, StringComparer.Ordinal);
    }

    public string Id { get; }
    public Version ContractVersion { get; }
    public string Owner { get; }
    public IReadOnlyList<ModuleDependency> Dependencies { get; }
    public IReadOnlyDictionary<string, ModuleCapability> Capabilities { get; }

    private void RequireOwnedName(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        if (name.Length > 120 || !name.StartsWith(Id + ".", StringComparison.Ordinal)
            || !Regex.IsMatch(name, "^[a-z][a-z0-9-]*(?:\\.[a-z][a-z0-9-]*)+$"))
            throw new ArgumentException("Capacidade e permissão requerem namespace do módulo.");
    }

    private static string RequireId(string id)
    {
        if (string.IsNullOrEmpty(id) || id.Length > 80 || !Regex.IsMatch(id, "^[a-z][a-z0-9]*(?:-[a-z0-9]+)*$"))
            throw new ArgumentException("ID de módulo inválido.", nameof(id));
        return id;
    }

    private static Version RequireVersion(Version version)
    {
        ArgumentNullException.ThrowIfNull(version);
        if (version.Major < 1 || version.Build < 0 || version.Revision != -1)
            throw new ArgumentException("Contrato requer versão estável major.minor.patch.", nameof(version));
        return version;
    }
}
