namespace MR.SAASy.Contracts.Application;

public sealed record ApplicationDescriptor(
    ApplicationIdentifier ApplicationId,
    string Name,
    string Version,
    ApplicationEnvironment Environment);
