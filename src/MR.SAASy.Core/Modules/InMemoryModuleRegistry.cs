using MR.SAASy.Contracts.Modules;

namespace MR.SAASy.Core.Modules;

/// <summary>
/// Provider-neutral, in-memory <see cref="IModuleRegistry"/> seeded from explicit module
/// manifests, indexed by id. A platform default for local/dev and integration tests. A version
/// argument that does not match the seeded implementation version resolves to no manifest.
/// </summary>
public sealed class InMemoryModuleRegistry : IModuleRegistry
{
    private readonly IReadOnlyDictionary<ModuleId, ModuleManifest> _modules;

    public InMemoryModuleRegistry(IEnumerable<ModuleManifest> modules)
    {
        ArgumentNullException.ThrowIfNull(modules);

        var byId = new Dictionary<ModuleId, ModuleManifest>();
        foreach (var module in modules)
        {
            byId[module.ModuleId] = module;
        }

        _modules = byId;
    }

    public ValueTask<ModuleManifest?> FindAsync(
        ModuleId moduleId,
        ModuleVersion? version = null,
        CancellationToken cancellationToken = default)
    {
        if (!_modules.TryGetValue(moduleId, out var manifest))
        {
            return ValueTask.FromResult<ModuleManifest?>(null);
        }

        if (version is not null && manifest.ImplementationVersion != version.Value)
        {
            return ValueTask.FromResult<ModuleManifest?>(null);
        }

        return ValueTask.FromResult<ModuleManifest?>(manifest);
    }

    public ValueTask<IReadOnlyCollection<ModuleManifest>> ListAsync(CancellationToken cancellationToken = default) =>
        ValueTask.FromResult<IReadOnlyCollection<ModuleManifest>>(_modules.Values.ToArray());
}
