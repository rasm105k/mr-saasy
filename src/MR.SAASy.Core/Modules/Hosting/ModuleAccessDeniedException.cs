using MR.SAASy.Contracts.Modules.Entitlements;

namespace MR.SAASy.Core.Modules.Hosting;

public sealed class ModuleAccessDeniedException : Exception
{
    public ModuleAccessDeniedException(ModuleEntitlementDecision decision)
        : base(BuildMessage(decision))
    {
        Decision = decision ?? throw new ArgumentNullException(nameof(decision));
    }

    public ModuleEntitlementDecision Decision { get; }

    private static string BuildMessage(ModuleEntitlementDecision decision)
    {
        ArgumentNullException.ThrowIfNull(decision);
        return $"Module '{decision.ModuleId}' is not enabled for tenant '{decision.TenantId}' and application '{decision.ApplicationId}'. State: {decision.State}.";
    }
}
