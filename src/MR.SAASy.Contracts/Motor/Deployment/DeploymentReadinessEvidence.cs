using MR.SAASy.Contracts.Motor.Domain;

namespace MR.SAASy.Contracts.Motor.Deployment;

/// <summary>References evidence; raw What-If output and secrets remain in their owning systems.</summary>
public sealed record DeploymentReadinessEvidence(
    bool PlanCreated,
    bool ValidationSucceeded,
    bool WhatIfSucceeded,
    bool WhatIfContainsDestructiveChanges,
    bool CostCheckSucceeded,
    decimal? EstimatedMonthlyCost,
    string? PlanEvidenceReference,
    string? ValidationEvidenceReference,
    string? WhatIfEvidenceReference,
    string? CostEvidenceReference,
    Approval? Approval);
