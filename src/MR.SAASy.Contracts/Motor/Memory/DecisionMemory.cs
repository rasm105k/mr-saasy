using MR.SAASy.Contracts.Motor.Domain;

namespace MR.SAASy.Contracts.Motor.Memory;

public sealed record DecisionMemory(
    MemoryRecordId Id,
    MissionId MissionId,
    AgentId? AgentId,
    string Summary,
    string Rationale,
    ExecutionOutcome Outcome,
    bool? HumanApproved,
    string EvidenceReference,
    DateTimeOffset CreatedAt);
