namespace MR.SAASy.Contracts.Modules.Hosting;

public sealed record ModuleNavigationEntry(
    ModuleId ModuleId,
    string Key,
    string Label,
    string Route,
    int Order);
