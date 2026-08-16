namespace MR.SAASy.Contracts.Application;

public sealed record ApplicationDescriptor(
    string ApplicationId,
    string Name,
    string Version,
    ApplicationEnvironment Environment);
