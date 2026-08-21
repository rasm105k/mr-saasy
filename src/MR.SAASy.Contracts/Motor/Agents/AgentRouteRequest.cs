using MR.SAASy.Contracts.Motor.Domain;

namespace MR.SAASy.Contracts.Motor.Agents;

public sealed record AgentRouteRequest(
    Mission Mission,
    string CorrelationId);
