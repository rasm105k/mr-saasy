namespace MR.SAASy.Contracts.Modules.Entitlements;

public sealed record ModuleDependencyFailure(
    ModuleId ModuleId,
    ModuleAvailabilityState State,
    ModuleVersion? MinimumVersion,
    ModuleVersion? ResolvedVersion,
    string Reason);
