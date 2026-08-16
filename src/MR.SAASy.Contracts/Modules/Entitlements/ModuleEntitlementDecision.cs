using MR.SAASy.Contracts.Application;
using MR.SAASy.Contracts.Capabilities;
using MR.SAASy.Contracts.Tenant;

namespace MR.SAASy.Contracts.Modules.Entitlements;

public sealed record ModuleEntitlementDecision(
    TenantId TenantId,
    ApplicationIdentifier ApplicationId,
    ModuleId ModuleId,
    ModuleAvailabilityState State,
    ModuleVersion? ResolvedVersion,
    ModuleContractVersion? ResolvedContractVersion,
    IReadOnlyCollection<ModuleDependencyFailure> DependencyFailures,
    IReadOnlyCollection<CapabilityDecision> CapabilityDecisions,
    string? Reason = null)
{
    public bool IsEnabled => State == ModuleAvailabilityState.Enabled;
}
