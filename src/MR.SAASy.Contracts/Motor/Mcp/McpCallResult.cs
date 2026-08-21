using MR.SAASy.Contracts.Motor.Domain;

namespace MR.SAASy.Contracts.Motor.Mcp;

public sealed record McpCallResult(
    ToolCallId ToolCallId,
    ExecutionOutcome Outcome,
    string Reason,
    TimeSpan Duration,
    string? ResultReference = null);
