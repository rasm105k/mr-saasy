namespace MR.SAASy.Contracts.Modules.Hosting;

/// <summary>
/// UI metadata for discovering a module. This descriptor is never an authorization decision.
/// </summary>
public sealed record ModuleNavigationDescriptor(
    string Key,
    string Label,
    string Route,
    int Order = 0);
