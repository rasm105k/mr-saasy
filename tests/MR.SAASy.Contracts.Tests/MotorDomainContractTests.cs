using MR.SAASy.Contracts.Motor.Agents;
using MR.SAASy.Contracts.Motor.Domain;
using MR.SAASy.Contracts.Motor.Models;
using Xunit;

namespace MR.SAASy.Contracts.Tests;

public sealed class MotorDomainContractTests
{
    private static readonly DateTimeOffset Now = new(2026, 8, 21, 20, 0, 0, TimeSpan.Zero);
    private static readonly MotorProjectContext Project = new(
        "mr-software",
        "workslip",
        "Workslip",
        "test",
        "tenant-a");

    [Fact]
    public void Mission_is_bound_to_project_customer_and_risk()
    {
        var mission = new Mission(
            new MissionId("mission-001"),
            Project,
            "Validate release",
            "Prove the release is safe",
            MissionTaskType.Testing,
            RiskLevel.High,
            ComplexityLevel.Medium,
            MissionState.Planned,
            Now);

        Assert.Equal("workslip", mission.Project.ProjectId);
        Assert.Equal("tenant-a", mission.Project.CustomerId);
        Assert.Equal(RiskLevel.High, mission.Risk);
    }

    [Fact]
    public void Approval_fails_closed_unless_a_named_human_approved_it_and_it_is_current()
    {
        var pending = new Approval(
            new ApprovalId("approval-001"),
            new MissionId("mission-001"),
            Project,
            "azure.deployment.create",
            ApprovalState.Pending,
            "gordon",
            Now);
        var approved = pending with
        {
            State = ApprovalState.Approved,
            DecidedBy = "human-operator",
            DecidedAt = Now,
            ExpiresAt = Now.AddMinutes(30),
        };

        Assert.False(pending.IsApproved(Now));
        Assert.True(approved.IsApproved(Now));
        Assert.False(approved.IsApproved(Now.AddHours(1)));
    }

    [Fact]
    public void Learning_links_action_result_human_feedback_and_business_evidence()
    {
        var learning = new LearningRecord(
            new LearningRecordId("learning-001"),
            new MissionId("mission-001"),
            new AgentId("qa"),
            MotorModelKeys.PremiumReasoning,
            "Validated a high-risk release",
            ExecutionOutcome.Succeeded,
            true,
            "impact://release-defects",
            "github://checks/123",
            Now);

        Assert.True(learning.HumanApproved);
        Assert.Equal(ExecutionOutcome.Succeeded, learning.Outcome);
        Assert.NotNull(learning.BusinessImpactReference);
    }

    [Fact]
    public void Agent_contract_contains_explicit_permissions_and_approval_rules()
    {
        var agent = new AgentDefinition(
            new AgentId("example"),
            "Example",
            "Reviewer",
            AgentAccessMode.ReadOnly,
            ["review"],
            [new AgentPermission("github", "repository.read", PermissionLevel.Read)],
            [new AgentApprovalRequirement("github.*.write", RiskLevel.Low, "Human-owned mutation")]);

        Assert.DoesNotContain(agent.Permissions, permission => permission.Operation == "*");
        Assert.NotEmpty(agent.ApprovalRequirements);
    }
}
