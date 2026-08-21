using MR.SAASy.Contracts.Motor.Domain;

namespace MR.SAASy.Contracts.Motor.Agents;

/// <summary>Declares actions an agent may only propose until a human approves.</summary>
public sealed record AgentApprovalRequirement(
    string ActionPattern,
    RiskLevel MinimumRisk,
    string Reason);
