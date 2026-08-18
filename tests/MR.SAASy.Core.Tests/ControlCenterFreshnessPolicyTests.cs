using MR.SAASy.Contracts.Application;
using MR.SAASy.Contracts.ControlCenter;
using MR.SAASy.Core.ControlCenter;
using Xunit;

namespace MR.SAASy.Core.Tests;

public sealed class ControlCenterFreshnessPolicyTests
{
    private static readonly ApplicationIdentifier Workslip = new("workslip");
    private static readonly DateTimeOffset Now = DateTimeOffset.UnixEpoch.AddHours(10);

    private static ApplicationHealthSnapshot Snapshot(ObservationState state, DateTimeOffset observedAt) =>
        new(Workslip, ApplicationEnvironment.Production, state, observedAt, observedAt);

    [Fact]
    public void Fresh_healthy_observation_is_unchanged()
    {
        var result = ControlCenterFreshnessPolicy.ApplyStaleness(
            Snapshot(ObservationState.Healthy, Now.AddMinutes(-1)), Now, TimeSpan.FromMinutes(5));

        Assert.Equal(ObservationState.Healthy, result.State);
    }

    [Fact]
    public void Old_healthy_observation_becomes_stale_with_a_reason()
    {
        var result = ControlCenterFreshnessPolicy.ApplyStaleness(
            Snapshot(ObservationState.Healthy, Now.AddMinutes(-30)), Now, TimeSpan.FromMinutes(5));

        Assert.Equal(ObservationState.Stale, result.State);
        Assert.NotNull(result.Reason);
    }

    [Theory]
    [InlineData(ObservationState.Unknown)]
    [InlineData(ObservationState.Blocked)]
    public void Unknown_and_blocked_are_never_reclassified_even_when_old(ObservationState state)
    {
        var result = ControlCenterFreshnessPolicy.ApplyStaleness(
            Snapshot(state, Now.AddDays(-1)), Now, TimeSpan.FromMinutes(5));

        Assert.Equal(state, result.State);
    }

    [Fact]
    public void Projection_overload_applies_staleness_to_health()
    {
        var projection = new ControlCenterProjection(
            Workslip,
            ApplicationEnvironment.Production,
            Snapshot(ObservationState.Healthy, Now.AddHours(-2)),
            LatestDeployment: null,
            RecentRuns: []);

        var result = ControlCenterFreshnessPolicy.ApplyStaleness(projection, Now, TimeSpan.FromMinutes(5));

        Assert.Equal(ObservationState.Stale, result.Health.State);
    }
}
