namespace MR.SAASy.Core.Motor.Reconciliation;

/// <summary>
/// Represents the state MOTOR wants to achieve.
/// Inspired by Kubernetes controller desired-state patterns.
/// </summary>
public sealed record DesiredState(
    string MissionId,
    string Objective,
    IReadOnlyDictionary<string, string> Requirements);
