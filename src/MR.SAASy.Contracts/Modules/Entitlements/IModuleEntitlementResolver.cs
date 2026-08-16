namespace MR.SAASy.Contracts.Modules.Entitlements;

public interface IModuleEntitlementResolver
{
    ValueTask<ModuleEntitlementDecision> ResolveAsync(
        ModuleEntitlementQuery query,
        CancellationToken cancellationToken = default);
}
