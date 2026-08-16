using MR.SAASy.Contracts.Modules.Entitlements;
using MR.SAASy.Contracts.Modules.Hosting;

namespace MR.SAASy.Core.Modules.Hosting;

public sealed class ModuleAccessGuard : IModuleAccessGuard
{
    private readonly IModuleEntitlementResolver _resolver;

    public ModuleAccessGuard(IModuleEntitlementResolver resolver)
    {
        _resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
    }

    public ValueTask<ModuleEntitlementDecision> EvaluateAsync(
        ModuleEntitlementQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        return _resolver.ResolveAsync(query, cancellationToken);
    }

    public async ValueTask<ModuleEntitlementDecision> RequireEnabledAsync(
        ModuleEntitlementQuery query,
        CancellationToken cancellationToken = default)
    {
        var decision = await EvaluateAsync(query, cancellationToken);
        if (!decision.IsEnabled)
        {
            throw new ModuleAccessDeniedException(decision);
        }

        return decision;
    }
}
