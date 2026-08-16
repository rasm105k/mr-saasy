namespace MR.SAASy.Contracts.Modules;

/// <summary>
/// Declares which host contract versions a module supports. The host must reject incompatible modules rather than guessing compatibility.
/// </summary>
public sealed record ModuleCompatibility(
    ModuleContractVersion MinimumHostContractVersion,
    ModuleContractVersion MaximumHostContractVersion);
