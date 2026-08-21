using MR.SAASy.Contracts.Motor.Deployment;
using MR.SAASy.Contracts.Motor.Domain;

namespace MR.SAASy.Core.Motor.Deployment;

/// <summary>
/// Pure fail-closed readiness gate. It never deploys; a future Azure adapter may execute
/// only when this decision is ready and must then emit completion evidence.
/// </summary>
public sealed class SafeDeploymentGate : ISafeDeploymentGate
{
    private readonly TimeProvider _timeProvider;

    public SafeDeploymentGate(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
    }

    public DeploymentGateDecision Evaluate(
        SafeDeploymentRequest request,
        DeploymentReadinessEvidence evidence)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(evidence);

        if (string.IsNullOrWhiteSpace(request.TargetEnvironment) ||
            string.IsNullOrWhiteSpace(request.TemplatePath) ||
            string.IsNullOrWhiteSpace(request.ActionReference))
        {
            return Block("Target environment, template path and action reference are required.");
        }

        if (!string.Equals(request.TargetEnvironment, request.Project.Environment, StringComparison.Ordinal))
        {
            return Block("Deployment target must match the mission's project environment.");
        }

        if (request.MaximumApprovedMonthlyCost < 0m)
        {
            return Block("The approved monthly cost ceiling cannot be negative.");
        }

        if (!evidence.PlanCreated || string.IsNullOrWhiteSpace(evidence.PlanEvidenceReference))
        {
            return WaitAt(DeploymentStage.Plan, "A reviewable deployment plan is required.");
        }

        if (!evidence.ValidationSucceeded || string.IsNullOrWhiteSpace(evidence.ValidationEvidenceReference))
        {
            return WaitAt(DeploymentStage.Validate, "Template validation must succeed and retain evidence.");
        }

        if (!evidence.WhatIfSucceeded || string.IsNullOrWhiteSpace(evidence.WhatIfEvidenceReference))
        {
            return WaitAt(DeploymentStage.WhatIf, "Azure What-If must succeed and retain evidence.");
        }

        if (!evidence.CostCheckSucceeded ||
            evidence.EstimatedMonthlyCost is null ||
            string.IsNullOrWhiteSpace(evidence.CostEvidenceReference))
        {
            return WaitAt(DeploymentStage.CostCheck, "A bounded monthly cost estimate is required.");
        }

        if (evidence.EstimatedMonthlyCost < 0m)
        {
            return Block("The monthly cost estimate cannot be negative.");
        }

        if (evidence.EstimatedMonthlyCost > request.MaximumApprovedMonthlyCost)
        {
            return WaitAt(DeploymentStage.Approval,
                "The estimate exceeds the mission's approved monthly cost ceiling.");
        }

        if (evidence.Approval is not { } approval ||
            approval.MissionId != request.MissionId ||
            !SameProject(approval.Project, request.Project) ||
            !string.Equals(approval.ActionReference, request.ActionReference, StringComparison.Ordinal) ||
            !string.Equals(approval.EvidenceReference, evidence.WhatIfEvidenceReference, StringComparison.Ordinal) ||
            !approval.IsApproved(_timeProvider.GetUtcNow()))
        {
            var changeType = evidence.WhatIfContainsDestructiveChanges ? "destructive changes" : "cloud changes";
            return WaitAt(DeploymentStage.Approval,
                $"A current human approval bound to the exact What-If evidence and {changeType} is required.");
        }

        return new DeploymentGateDecision(
            true,
            DeploymentStage.ReadyToDeploy,
            ["Plan, validation, What-If, cost check and bound human approval all passed."]);
    }

    private static DeploymentGateDecision WaitAt(DeploymentStage stage, string reason) =>
        new(false, stage, [reason]);

    private static DeploymentGateDecision Block(string reason) =>
        new(false, DeploymentStage.Blocked, [reason]);

    private static bool SameProject(
        MotorProjectContext left,
        MotorProjectContext right) =>
        string.Equals(left.WorkspaceId, right.WorkspaceId, StringComparison.Ordinal) &&
        string.Equals(left.ProjectId, right.ProjectId, StringComparison.Ordinal) &&
        string.Equals(left.Environment, right.Environment, StringComparison.Ordinal) &&
        string.Equals(left.CustomerId, right.CustomerId, StringComparison.Ordinal);
}
