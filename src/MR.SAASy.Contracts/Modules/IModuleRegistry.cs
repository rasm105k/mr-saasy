namespace MR.SAASy.Contracts.Modules;

public interface IModuleRegistry
{
    ValueTask<ModuleManifest?> FindAsync(
        ModuleId moduleId,
        ModuleVersion? version = null,
        CancellationToken cancellationToken = default);

    ValueTask<IReadOnlyCollection<ModuleManifest>> ListAsync(
        CancellationToken cancellationToken = default);
}
