namespace MR.SAASy.Core.Motor.Learning;

public sealed record AgentExecutionSignal(
    string AgentId,
    bool Successful,
    TimeSpan Duration,
    decimal EstimatedCost,
    DateTime CreatedAt);
