using MR.SAASy.Contracts.Application;

namespace MR.SAASy.Contracts.Modules.Hosting;

public sealed record ModuleHostDefinition(
    ApplicationIdentifier ApplicationId,
    ModuleContractVersion HostContractVersion,
    IReadOnlyCollection<ModuleHostRegistration> Modules);
