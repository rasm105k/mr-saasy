using MR.SAASy.Contracts.Modules.Entitlements;

namespace MR.SAASy.Contracts.Modules.Hosting;

/// <summary>
/// Backend enforcement boundary for module availability.
/// Product-domain authorization remains a separate product responsibility.
/// </summary>
public interface IModuleAccessGuard
{
    ValueTask<ModuleEntitlementDecision> EvaluateAsync(
        ModuleEntitlementQuery query,
        CancellationToken cancellationToken = default);

    ValueTask<ModuleEntitlementDecision> RequireEnabledAsync(
        ModuleEntitlementQuery query,
        CancellationToken cancellationToken = default);
}
