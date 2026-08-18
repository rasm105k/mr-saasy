namespace MR.SAASy.Contracts.Modules.Entitlements;

/// <summary>
/// Explicit module availability states. Only <see cref="Enabled"/> authorizes the module to run;
/// every other state fails closed. The default (0) value is non-authorizing (<see cref="Disabled"/>),
/// so a default-initialized decision never runs a module.
/// </summary>
public enum ModuleAvailabilityState
{
    Disabled = 0,
    BlockedDependency = 1,
    BlockedCapability = 2,
    UnsupportedVersion = 3,
    UnknownModule = 4,
    Enabled = 5
}
