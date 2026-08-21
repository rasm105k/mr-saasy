namespace MR.SAASy.Contracts.AgentOrchestration;

public enum AgentStatus
{
    Planned,
    Running,
    Waiting,
    Blocked,
    Completed
}

public sealed record AgentDescriptor(
    string Id,
    string Name,
    string Purpose,
    AgentStatus Status,
    DateTimeOffset LastActivity,
    IReadOnlyCollection<string> Integrations);

public sealed record AgentTaskReference(
    string AgentId,
    string TaskId,
    string? LinearIssueId,
    string? GitHubReference);

public enum AgentEventType
{
    TaskStarted,
    AnalysisCompleted,
    PullRequestCreated,
    DeploymentStarted,
    TestFailed,
    ApprovalRequired,
    TaskCompleted
}

public sealed record AgentActivityEvent(
    string AgentId,
    AgentEventType Type,
    string Description,
    DateTimeOffset CreatedAt);

public sealed record ApprovalGate(
    string Id,
    string Action,
    bool RequiresApproval,
    string? ApprovedBy,
    DateTimeOffset? ApprovedAt);
