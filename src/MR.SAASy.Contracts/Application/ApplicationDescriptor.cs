namespace MR.SAASy.Contracts.Application;

public sealed record ApplicationDescriptor(
    ApplicationId ApplicationId,
    string Name,
    string Version,
    ApplicationEnvironment Environment);
