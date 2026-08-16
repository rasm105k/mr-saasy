namespace MR.SAASy.Contracts.Modules;

public sealed record ModuleDependency(
    ModuleId ModuleId,
    ModuleVersion MinimumVersion,
    bool Optional = false);
