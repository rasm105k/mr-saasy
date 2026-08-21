namespace MR.SAASy.Contracts.Projects;

public sealed record ProjectContext(
    string WorkspaceId,
    string ProjectId,
    string Name,
    IReadOnlyDictionary<string, string> Metadata);
