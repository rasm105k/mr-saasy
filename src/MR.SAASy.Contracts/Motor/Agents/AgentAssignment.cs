using MR.SAASy.Contracts.Motor.Domain;

namespace MR.SAASy.Contracts.Motor.Agents;

public sealed record AgentAssignment(
    MissionId MissionId,
    AgentId AgentId,
    string Role,
    string Reason,
    bool RequiresHumanReview,
    DateTimeOffset AssignedAt);
