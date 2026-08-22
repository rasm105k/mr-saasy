namespace MR.SAASy.Core.Motor.Agents;

public interface IAgentExecutor
{
    Task<AgentExecutionResult> ExecuteAsync(
        AgentExecutionContext context,
        CancellationToken cancellationToken = default);
}

public sealed record AgentExecutionContext(
    Guid MissionId,
    Guid WorkflowId,
    string AgentId,
    IReadOnlyCollection<string> Capabilities);

public sealed record AgentExecutionResult(
    bool Success,
    string? Output,
    string? Error);
