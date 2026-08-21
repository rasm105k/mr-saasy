using MR.SAASy.Contracts.Application;

namespace MR.SAASy.Contracts.ControlCenter;

/// <summary>
/// Normalized projection of a single automation/CI run for an application and environment.
/// <see cref="RunId"/> is the exact source run identifier and <see cref="Workflow"/> the normalized
/// pipeline/workflow name. No provider payloads or logs are carried; <see cref="Evidence"/> points
/// back to the run in its owning system.
/// </summary>
public sealed record AutomationRun(
    ApplicationIdentifier ApplicationId,
    ApplicationEnvironment Environment,
    string RunId,
    string Workflow,
    AutomationRunState State,
    DateTimeOffset ObservedAt,
    DateTimeOffset RecordedAt,
    string? RevisionReference = null,
    EvidenceReference? Evidence = null,
    string? Reason = null);
