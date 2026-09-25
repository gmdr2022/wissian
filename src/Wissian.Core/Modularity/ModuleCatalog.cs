using System.Collections.Frozen;

namespace Wissian.Core.Modularity;

public sealed class ModuleCatalog
{
    public static ModuleManifest CoreManifest { get; } = new("core", new(1, 0, 0), "Wissian", [], []);
    private readonly FrozenDictionary<string, ModuleManifest> modules;

    public ModuleCatalog(IEnumerable<ModuleManifest> extensions)
    {
        var manifests = new[] { CoreManifest }.Concat(extensions).ToArray();
        if (manifests.Select(m => m.Id).Distinct(StringComparer.Ordinal).Count() != manifests.Length)
            throw new InvalidOperationException("Composição contém ID repetido ou reservado.");
        modules = manifests.ToFrozenDictionary(m => m.Id, StringComparer.Ordinal);
        foreach (var manifest in manifests)
        foreach (var dependency in manifest.Dependencies)
        {
            if (!modules.TryGetValue(dependency.Id, out var installed)
                || installed.ContractVersion != dependency.ContractVersion)
                throw new InvalidOperationException("Dependência ausente ou contrato incompatível.");
        }
        var visited = new HashSet<string>(StringComparer.Ordinal);
        var visiting = new HashSet<string>(StringComparer.Ordinal);
        foreach (var manifest in manifests) Visit(manifest.Id, visited, visiting);
    }

    public IReadOnlyDictionary<string, ModuleManifest> Modules => modules;

    public bool IsOperational(string moduleId, IReadOnlySet<string> eligible, IReadOnlySet<string> enabled)
    {
        if (!modules.TryGetValue(moduleId, out var module)) return false;
        if (moduleId != "core" && (!eligible.Contains(moduleId) || !enabled.Contains(moduleId))) return false;
        return module.Dependencies.All(d => IsOperational(d.Id, eligible, enabled));
    }

    private void Visit(string id, HashSet<string> visited, HashSet<string> visiting)
    {
        if (visited.Contains(id)) return;
        if (!visiting.Add(id)) throw new InvalidOperationException("Dependências formam ciclo.");
        foreach (var dependency in modules[id].Dependencies) Visit(dependency.Id, visited, visiting);
        visiting.Remove(id);
        visited.Add(id);
    }
}
