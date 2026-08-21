using MR.SAASy.Contracts.Motor.Domain;
using MR.SAASy.Contracts.Motor.Events;
using MR.SAASy.Contracts.Motor.Models;
using MR.SAASy.Core.Motor.Events;
using MR.SAASy.Core.Motor.Models;
using Xunit;

namespace MR.SAASy.Core.Tests;

public sealed class PolicyModelRouterTests
{
    private static readonly DateTimeOffset Now = new(2026, 8, 21, 20, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task High_risk_routes_to_premium_and_logs_reason_and_cost()
    {
        var log = new InMemoryModelSelectionLog();
        var events = new InMemoryMotorEventSink();
        var router = new PolicyModelRouter(log, events, new FixedTimeProvider(Now));
        var request = Request(
            MissionTaskType.Operations,
            RiskLevel.High,
            candidates: [Premium(), OpenCode(), Economy()]);

        var selection = await router.SelectAsync(request);

        Assert.Equal(MotorModelKeys.PremiumReasoning, selection.Model);
        Assert.Contains("High-risk", selection.Reason, StringComparison.Ordinal);
        Assert.True(selection.EstimatedCost > 0m);
        Assert.Single(log.Selections);
        Assert.Equal(selection, log.Selections.Single());
        Assert.Equal(selection.Model, Assert.IsType<ModelSelected>(Assert.Single(events.Events)).Model);
    }

    [Fact]
    public async Task Code_routes_to_opencode_zen_when_available()
    {
        var log = new InMemoryModelSelectionLog();
        var router = Router(log);

        var selection = await router.SelectAsync(Request(
            MissionTaskType.Code,
            RiskLevel.Medium,
            candidates: [Premium(), OpenCode(), Economy()]));

        Assert.Equal(MotorModelKeys.OpenCodeZen, selection.Model);
        Assert.Equal(ModelTier.CodeSpecialist, selection.Tier);
    }

    [Fact]
    public async Task Bulk_routing_prefers_lower_estimated_cost_within_economy_tier()
    {
        var log = new InMemoryModelSelectionLog();
        var router = Router(log);
        var expensive = Economy(new ModelKey("economy-expensive"), 2m, 4m);
        var cheap = Economy(new ModelKey("economy-cheap"), 0.2m, 0.4m);

        var selection = await router.SelectAsync(Request(
            MissionTaskType.Bulk,
            RiskLevel.Low,
            isBulk: true,
            candidates: [expensive, cheap, Premium()]));

        Assert.Equal(cheap.Key, selection.Model);
    }

    [Fact]
    public async Task Historical_quality_breaks_ties_for_quality_first_route()
    {
        var log = new InMemoryModelSelectionLog();
        var router = Router(log);
        var modelA = Premium(new ModelKey("premium-a"));
        var modelB = Premium(new ModelKey("premium-b"));
        var history = new[]
        {
            new ModelPerformanceSnapshot(modelA.Key, MissionTaskType.Security, 0.95m, 0.97m, 1m, 20),
            new ModelPerformanceSnapshot(modelB.Key, MissionTaskType.Security, 0.80m, 0.82m, 1m, 20),
        };

        var selection = await router.SelectAsync(Request(
            MissionTaskType.Security,
            RiskLevel.High,
            candidates: [modelB, modelA],
            history: history));

        Assert.Equal(modelA.Key, selection.Model);
    }

    [Fact]
    public async Task Unknown_routes_to_premium_and_requires_evaluation()
    {
        var log = new InMemoryModelSelectionLog();
        var router = Router(log);

        var selection = await router.SelectAsync(Request(
            MissionTaskType.Unknown,
            RiskLevel.Unknown,
            candidates: [Premium(), Economy()]));

        Assert.Equal(ModelTier.PremiumReasoning, selection.Tier);
        Assert.True(selection.EvaluationRequired);
    }

    [Fact]
    public async Task Execution_result_is_linked_to_an_existing_logged_selection()
    {
        var log = new InMemoryModelSelectionLog();
        var router = Router(log);
        var selection = await router.SelectAsync(Request(
            MissionTaskType.Code,
            RiskLevel.Medium,
            candidates: [OpenCode()]));
        var result = new ModelExecutionResult(
            selection.Id,
            selection.MissionId,
            selection.Model,
            ExecutionOutcome.Succeeded,
            0.11m,
            TimeSpan.FromSeconds(12),
            Now.AddSeconds(12),
            "github://commit/abc");

        await log.RecordResultAsync(result);

        Assert.Equal(result, log.Results.Single());
    }

    [Fact]
    public async Task Candidate_that_does_not_support_the_task_is_never_selected()
    {
        var log = new InMemoryModelSelectionLog();
        var router = Router(log);
        var unsupported = new ModelCandidate(
            MotorModelKeys.PremiumReasoning,
            "premium-provider",
            ModelTier.PremiumReasoning,
            10m,
            30m,
            [MissionTaskType.Analytics]);

        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await router.SelectAsync(Request(
                MissionTaskType.Security,
                RiskLevel.High,
                candidates: [unsupported])));
    }

    private static ModelRouteRequest Request(
        MissionTaskType taskType,
        RiskLevel risk,
        bool isBulk = false,
        IReadOnlyCollection<ModelCandidate>? candidates = null,
        IReadOnlyCollection<ModelPerformanceSnapshot>? history = null) =>
        new(
            new MissionId("mission-001"),
            new MotorProjectContext("workspace", "motor", "MOTOR", "test"),
            "corr-001",
            taskType,
            risk,
            ComplexityLevel.Medium,
            isBulk,
            false,
            10_000,
            2_000,
            candidates ?? [Premium(), OpenCode(), Economy()],
            history ?? []);

    private static PolicyModelRouter Router(InMemoryModelSelectionLog log) =>
        new(log, new InMemoryMotorEventSink(), new FixedTimeProvider(Now));

    private static ModelCandidate Premium(ModelKey? key = null) =>
        new(key ?? MotorModelKeys.PremiumReasoning, "premium-provider", ModelTier.PremiumReasoning,
            10m, 30m, Enum.GetValues<MissionTaskType>());

    private static ModelCandidate OpenCode() =>
        new(MotorModelKeys.OpenCodeZen, "opencode", ModelTier.CodeSpecialist,
            2m, 8m, [MissionTaskType.Code]);

    private static ModelCandidate Economy(ModelKey? key = null, decimal inputCost = 0.5m, decimal outputCost = 1m) =>
        new(key ?? MotorModelKeys.EconomyBulk, "economy-provider", ModelTier.Economy,
            inputCost, outputCost, Enum.GetValues<MissionTaskType>());
}
