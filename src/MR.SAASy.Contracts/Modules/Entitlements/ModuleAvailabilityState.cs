namespace MR.SAASy.Contracts.Modules.Entitlements;

/// <summary>
/// Explicit module availability states. Only Enabled authorizes the module to run.
/// Every other state fails closed.
/// </summary>
public enum ModuleAvailabilityState
{
    Enabled = 0,
    Disabled = 1,
    BlockedDependency = 2,
    BlockedCapability = 3,
    UnsupportedVersion = 4,
    UnknownModule = 5
}
