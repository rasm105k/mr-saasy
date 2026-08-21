namespace MR.SAASy.Contracts.Agents;

public sealed record AgentExecutionContext(
    string WorkspaceId,
    string ProjectId,
    string AgentId,
    string TaskId,
    IReadOnlyCollection<string> Capabilities,
    IReadOnlyDictionary<string, string> ContextValues);
