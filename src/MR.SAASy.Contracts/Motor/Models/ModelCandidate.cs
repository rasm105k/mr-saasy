using MR.SAASy.Contracts.Motor.Domain;

namespace MR.SAASy.Contracts.Motor.Models;

/// <summary>Runtime configuration for one available model route and its current price estimate.</summary>
public sealed record ModelCandidate(
    ModelKey Key,
    string Provider,
    ModelTier Tier,
    decimal InputCostPerMillionTokens,
    decimal OutputCostPerMillionTokens,
    IReadOnlyCollection<MissionTaskType> SupportedTasks,
    bool Enabled = true);
