namespace MR.SAASy.Contracts.Modules;

public sealed record ModuleManifest(
    ModuleId ModuleId,
    string DisplayName,
    ModuleVersion ImplementationVersion,
    ModuleContractVersion ContractVersion,
    IReadOnlyCollection<ModuleDependency> Dependencies,
    IReadOnlyCollection<RequiredCapability> RequiredCapabilities,
    IReadOnlyCollection<ProvidedCapability> ProvidedCapabilities,
    ModuleCompatibility Compatibility);
