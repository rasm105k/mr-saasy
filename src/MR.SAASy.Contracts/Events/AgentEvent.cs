namespace MR.SAASy.Contracts.Events;

public sealed record AgentEvent(
    string EventType,
    string AgentId,
    string WorkspaceId,
    DateTimeOffset OccurredAt,
    IReadOnlyDictionary<string, string> Data);
