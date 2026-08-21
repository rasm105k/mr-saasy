using MR.SAASy.Contracts.Motor.Domain;
using MR.SAASy.Contracts.Motor.Memory;
using MR.SAASy.Contracts.Motor.Models;
using MR.SAASy.Core.Motor.Memory;
using Xunit;

namespace MR.SAASy.Core.Tests;

public sealed class InMemoryMotorMemoryStoreTests
{
    private static readonly DateTimeOffset Now = new(2026, 8, 21, 20, 0, 0, TimeSpan.Zero);
    private static readonly MissionId MissionId = new("mission-001");
    private static readonly AgentId AgentId = new("qa");

    [Fact]
    public async Task Snapshot_contains_each_memory_dimension_and_learning_feedback()
    {
        var store = new InMemoryMotorMemoryStore();
        await store.AddDecisionAsync(new DecisionMemory(
            Id("decision"), MissionId, AgentId, "Use What-If", "Prevents blind deployment",
            ExecutionOutcome.Succeeded, true, "azure://what-if/1", Now));
        await store.AddSolutionAsync(new SolutionMemory(
            Id("solution"), MissionId, "deploy-risk", "Gate deploy behind evidence",
            ["azure", "approval"], ExecutionOutcome.Succeeded, "docs://motor", Now));
        await store.AddAgentPerformanceAsync(new AgentPerformanceRecord(
            Id("agent"), AgentId, MissionTaskType.Testing, 10, 0.9m, 0.95m,
            TimeSpan.FromMinutes(2), Now));
        await store.AddModelPerformanceAsync(new ModelPerformanceRecord(
            Id("model"), MotorModelKeys.PremiumReasoning, MissionTaskType.Testing,
            10, 0.9m, 0.95m, 0.22m, TimeSpan.FromSeconds(30), Now));
        await store.AddBusinessImpactAsync(new BusinessImpactRecord(
            Id("impact"), MissionId, "escaped-defects", 4m, 1m, "count", 0.8m,
            "powerbi://dataset/1", Now));
        await store.AddLearningAsync(new LearningRecord(
            new LearningRecordId("learning"), MissionId, AgentId, MotorModelKeys.PremiumReasoning,
            "Validated deployment", ExecutionOutcome.Succeeded, true,
            "impact", "github://check/1", Now));

        var snapshot = await store.SnapshotAsync();

        Assert.Single(snapshot.Decisions);
        Assert.Single(snapshot.Solutions);
        Assert.Single(snapshot.AgentPerformance);
        Assert.Single(snapshot.ModelPerformance);
        Assert.Single(snapshot.BusinessImpact);
        Assert.Single(snapshot.Learning);
        Assert.True(snapshot.Learning.Single().HumanApproved);
    }

    [Fact]
    public async Task Append_only_store_rejects_duplicate_record_ids()
    {
        var store = new InMemoryMotorMemoryStore();
        var record = new DecisionMemory(
            Id("decision"), MissionId, AgentId, "Decision", "Reason",
            ExecutionOutcome.Pending, null, "evidence://1", Now);
        await store.AddDecisionAsync(record);

        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await store.AddDecisionAsync(record));
    }

    private static MemoryRecordId Id(string value) => new(value);
}
