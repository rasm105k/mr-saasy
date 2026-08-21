using MR.SAASy.Contracts.Application;
using MR.SAASy.Contracts.Features;
using MR.SAASy.Contracts.Identity;
using MR.SAASy.Contracts.Tenant;
using MR.SAASy.Core.Features;
using Xunit;

namespace MR.SAASy.Core.Tests;

public sealed class InMemoryFeatureFlagEvaluatorTests
{
    private static readonly FeatureFlagKey Help = PlatformFeatureFlags.HelpWizard;
    private static readonly ApplicationIdentifier Workslip = new("workslip");
    private static readonly TenantId TenantA = new("tenant-a");
    private static readonly IdentityId UserA = new("user-a");

    [Fact]
    [Trait("Trait", "OffPath")]
    public async Task Unseeded_flag_is_off()
    {
        var evaluator = new InMemoryFeatureFlagEvaluator([]);

        var decision = await evaluator.EvaluateAsync(new FeatureFlagQuery(Help, Workslip));

        Assert.False(decision.IsEnabled);
        Assert.Equal(FeatureFlagState.Off, decision.State);
    }

    [Fact]
    [Trait("Trait", "OffPath")]
    public async Task Platform_kill_beats_identity_on()
    {
        var evaluator = new InMemoryFeatureFlagEvaluator(
        [
            new FeatureFlagAssignment(Help, FeatureFlagState.Off, FeatureFlagSource.PlatformKill),
            new FeatureFlagAssignment(Help, FeatureFlagState.On, FeatureFlagSource.Identity, IdentityId: UserA)
        ]);

        var decision = await evaluator.EvaluateAsync(new FeatureFlagQuery(Help, Workslip, TenantA, UserA));

        Assert.False(decision.IsEnabled);
        Assert.Equal(FeatureFlagSource.PlatformKill, decision.Source);
    }

    [Fact]
    [Trait("Trait", "OffPath")]
    public async Task Identity_can_turn_an_application_flag_off()
    {
        var evaluator = new InMemoryFeatureFlagEvaluator(
        [
            new FeatureFlagAssignment(Help, FeatureFlagState.On, FeatureFlagSource.Application, Workslip),
            new FeatureFlagAssignment(Help, FeatureFlagState.Off, FeatureFlagSource.Identity, IdentityId: UserA)
        ]);

        var decision = await evaluator.EvaluateAsync(new FeatureFlagQuery(Help, Workslip, TenantA, UserA));

        Assert.False(decision.IsEnabled);
        Assert.Equal(FeatureFlagSource.Identity, decision.Source);
    }

    [Fact]
    public async Task Application_on_enables_when_no_narrower_override()
    {
        var evaluator = new InMemoryFeatureFlagEvaluator(
        [
            new FeatureFlagAssignment(Help, FeatureFlagState.On, FeatureFlagSource.Application, Workslip)
        ]);

        var decision = await evaluator.EvaluateAsync(new FeatureFlagQuery(Help, Workslip, TenantA));

        Assert.True(decision.IsEnabled);
        Assert.Equal(FeatureFlagSource.Application, decision.Source);
    }
}
