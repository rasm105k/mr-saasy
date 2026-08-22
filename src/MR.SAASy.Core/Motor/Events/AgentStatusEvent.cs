namespace MR.SAASy.Core.Motor.Events;

public enum AgentStatus
{
    Planned,
    Running,
    Waiting,
    Blocked,
    Completed
}

public sealed record AgentStatusEvent(
    string AgentId,
    AgentStatus Status,
    DateTimeOffset OccurredAt,
    string? Reason = null);
