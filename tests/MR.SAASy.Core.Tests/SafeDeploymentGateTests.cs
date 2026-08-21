using MR.SAASy.Contracts.Motor.Deployment;
using MR.SAASy.Contracts.Motor.Domain;
using MR.SAASy.Core.Motor.Deployment;
using Xunit;

namespace MR.SAASy.Core.Tests;

public sealed class SafeDeploymentGateTests
{
    private static readonly DateTimeOffset Now = new(2026, 8, 21, 20, 0, 0, TimeSpan.Zero);
    private static readonly MissionId MissionId = new("mission-001");
    private static readonly MotorProjectContext Project = new("workspace", "motor", "MOTOR", "test");
    private static readonly SafeDeploymentRequest Request = new(
        MissionId,
        Project,
        "test",
        "infrastructure/bicep/main.bicep",
        "azure.deployment.motor-test",
        RiskLevel.High,
        100m);

    [Fact]
    public void Missing_what_if_stops_before_cost_and_approval()
    {
        var gate = new SafeDeploymentGate(new FixedTimeProvider(Now));
        var evidence = ReadyEvidence() with
        {
            WhatIfSucceeded = false,
            WhatIfEvidenceReference = null,
        };

        var decision = gate.Evaluate(Request, evidence);

        Assert.False(decision.IsReadyToDeploy);
        Assert.Equal(DeploymentStage.WhatIf, decision.Stage);
    }

    [Fact]
    public void Cost_above_approved_ceiling_requires_new_approval()
    {
        var gate = new SafeDeploymentGate(new FixedTimeProvider(Now));
        var evidence = ReadyEvidence() with { EstimatedMonthlyCost = 101m };

        var decision = gate.Evaluate(Request, evidence);

        Assert.False(decision.IsReadyToDeploy);
        Assert.Equal(DeploymentStage.Approval, decision.Stage);
    }

    [Fact]
    public void Approval_for_another_action_fails_closed()
    {
        var gate = new SafeDeploymentGate(new FixedTimeProvider(Now));
        var evidence = ReadyEvidence() with
        {
            Approval = Approved() with { ActionReference = "azure.deployment.other" },
        };

        var decision = gate.Evaluate(Request, evidence);

        Assert.False(decision.IsReadyToDeploy);
        Assert.Equal(DeploymentStage.Approval, decision.Stage);
    }

    [Fact]
    public void Complete_evidence_and_bound_current_approval_is_ready_but_does_not_deploy()
    {
        var gate = new SafeDeploymentGate(new FixedTimeProvider(Now));

        var decision = gate.Evaluate(Request, ReadyEvidence());

        Assert.True(decision.IsReadyToDeploy);
        Assert.Equal(DeploymentStage.ReadyToDeploy, decision.Stage);
    }

    private static DeploymentReadinessEvidence ReadyEvidence() =>
        new(
            true,
            true,
            true,
            true,
            true,
            42m,
            "docs://plan/1",
            "azure://validate/1",
            "azure://what-if/1",
            "azure://cost/1",
            Approved());

    private static Approval Approved() =>
        new(
            new ApprovalId("approval-001"),
            MissionId,
            Project,
            Request.ActionReference,
            ApprovalState.Approved,
            "gordon",
            Now.AddMinutes(-5),
            "human-operator",
            Now.AddMinutes(-1),
            Now.AddMinutes(30),
            EvidenceReference: "azure://what-if/1");
}
