using MR.SAASy.Contracts.Tenant;

namespace MR.SAASy.Contracts.Modules.Hosting;

public interface IModuleHostComposer
{
    ValueTask<ModuleHostSnapshot> ComposeAsync(
        TenantId tenantId,
        ModuleHostDefinition hostDefinition,
        CancellationToken cancellationToken = default);
}
