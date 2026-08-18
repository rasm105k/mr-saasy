using MR.SAASy.Contracts.Application;
using MR.SAASy.Contracts.ControlCenter;
using Xunit;

namespace MR.SAASy.Contracts.Tests;

public sealed class ControlCenterReadModelContractTests
{
    private static readonly ApplicationIdentifier Workslip = new("workslip");

    [Fact]
    public void Unknown_is_the_default_state_for_both_observation_and_run()
    {
        Assert.Equal(ObservationState.Unknown, default(ObservationState));
        Assert.Equal(AutomationRunState.Unknown, default(AutomationRunState));
    }

    [Fact]
    public void Unknown_projection_is_not_null_and_not_healthy()
    {
        var projection = ControlCenterProjection.Unknown(
            Workslip, ApplicationEnvironment.Production, DateTimeOffset.UnixEpoch);

        Assert.Equal(ObservationState.Unknown, projection.Health.State);
        Assert.False(projection.Health.IsHealthy);
        Assert.Null(projection.LatestDeployment);
        Assert.Empty(projection.RecentRuns);
    }

    [Fact]
    public void Health_snapshot_reports_healthy_only_for_healthy_state()
    {
        var healthy = new ApplicationHealthSnapshot(
            Workslip, ApplicationEnvironment.Production, ObservationState.Healthy,
            DateTimeOffset.UnixEpoch, DateTimeOffset.UnixEpoch);

        Assert.True(healthy.IsHealthy);
        Assert.False((healthy with { State = ObservationState.Degraded }).IsHealthy);
        Assert.False((healthy with { State = ObservationState.Blocked }).IsHealthy);
    }
}
