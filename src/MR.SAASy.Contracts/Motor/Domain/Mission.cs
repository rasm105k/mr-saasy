namespace MR.SAASy.Contracts.Motor.Domain;

/// <summary>An immutable snapshot of a MOTOR mission.</summary>
public sealed record Mission(
    MissionId Id,
    MotorProjectContext Project,
    string Title,
    string Objective,
    MissionTaskType TaskType,
    RiskLevel Risk,
    ComplexityLevel Complexity,
    MissionState State,
    DateTimeOffset CreatedAt,
    DateTimeOffset? StartedAt = null,
    DateTimeOffset? CompletedAt = null);
