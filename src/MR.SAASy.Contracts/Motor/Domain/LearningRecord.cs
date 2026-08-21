using MR.SAASy.Contracts.Motor.Models;

namespace MR.SAASy.Contracts.Motor.Domain;

/// <summary>
/// Captures what MOTOR did, the observed result and whether a human accepted it.
/// Detailed customer data and model prompts do not belong in this record.
/// </summary>
public sealed record LearningRecord(
    LearningRecordId Id,
    MissionId MissionId,
    AgentId AgentId,
    ModelKey? Model,
    string ActionSummary,
    ExecutionOutcome Outcome,
    bool? HumanApproved,
    string? BusinessImpactReference,
    string EvidenceReference,
    DateTimeOffset CreatedAt);
