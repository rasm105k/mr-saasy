namespace MR.SAASy.Contracts.Modules.Hosting;

public sealed record ModuleHostRegistration(
    ModuleId ModuleId,
    IReadOnlyCollection<ModuleNavigationDescriptor> Navigation);
