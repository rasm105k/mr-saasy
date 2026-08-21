using MR.SAASy.Contracts.Motor.Domain;

namespace MR.SAASy.Contracts.Motor.Models;

/// <summary>An auditable model-routing decision made before any provider is invoked.</summary>
public sealed record ModelSelection(
    ModelSelectionId Id,
    MissionId MissionId,
    ModelKey Model,
    string Provider,
    ModelTier Tier,
    string Reason,
    decimal EstimatedCost,
    bool EvaluationRequired,
    DateTimeOffset SelectedAt);
