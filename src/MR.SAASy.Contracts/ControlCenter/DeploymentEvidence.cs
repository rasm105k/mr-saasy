using MR.SAASy.Contracts.Application;

namespace MR.SAASy.Contracts.ControlCenter;

/// <summary>
/// Normalized record of the most recent deployment observed for an application and environment.
/// <see cref="RevisionReference"/> is the exact deployed revision; <see cref="RunId"/> optionally
/// links the automation run that produced it. <see cref="State"/> reuses <see cref="ObservationState"/>
/// so a failed or unknown deployment is never coerced to healthy.
/// </summary>
public sealed record DeploymentEvidence(
    ApplicationIdentifier ApplicationId,
    ApplicationEnvironment Environment,
    string RevisionReference,
    ObservationState State,
    DateTimeOffset ObservedAt,
    DateTimeOffset RecordedAt,
    string? RunId = null,
    EvidenceReference? Evidence = null,
    string? Reason = null);
