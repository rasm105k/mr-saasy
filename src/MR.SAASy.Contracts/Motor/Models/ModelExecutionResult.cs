using MR.SAASy.Contracts.Motor.Domain;

namespace MR.SAASy.Contracts.Motor.Models;

/// <summary>Sanitized result metadata recorded after model execution.</summary>
public sealed record ModelExecutionResult(
    ModelSelectionId SelectionId,
    MissionId MissionId,
    ModelKey Model,
    ExecutionOutcome Outcome,
    decimal ActualCost,
    TimeSpan Duration,
    DateTimeOffset CompletedAt,
    string? ResultReference = null);
