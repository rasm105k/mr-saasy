using MR.SAASy.Contracts.Application;
using MR.SAASy.Contracts.ControlCenter;
using MR.SAASy.Core.ControlCenter;
using Xunit;

namespace MR.SAASy.Core.Tests;

public sealed class InMemoryControlCenterReadModelResilienceTests
{
    private static readonly ApplicationIdentifier Workslip = new("workslip");
    private static readonly ApplicationIdentifier Marketing = new("marketing-site");
    private static readonly DateTimeOffset At = DateTimeOffset.UnixEpoch;

    private static ControlCenterProjection Healthy(ApplicationIdentifier applicationId) =>
        new(
            applicationId,
            ApplicationEnvironment.Production,
            new ApplicationHealthSnapshot(applicationId, ApplicationEnvironment.Production, ObservationState.Healthy, At, At),
            LatestDeployment: null,
            RecentRuns: []);

    [Fact]
    public async Task GetAll_degrades_a_throwing_source_to_unknown_without_collapsing_others()
    {
        var readModel = new InMemoryControlCenterReadModel(
        [
            new StaticControlCenterProjectionSource(Workslip, [Healthy(Workslip)]),
            new ThrowingProjectionSource(Marketing),
        ]);

        var all = await readModel.GetAllAsync(ApplicationEnvironment.Production);

        Assert.Equal(2, all.Count);
        Assert.Contains(all, p => p.ApplicationId == Workslip && p.Health.State == ObservationState.Healthy);
        Assert.Contains(all, p => p.ApplicationId == Marketing && p.Health.State == ObservationState.Unknown);
    }

    [Fact]
    public async Task GetApplication_degrades_a_throwing_source_to_unknown()
    {
        var readModel = new InMemoryControlCenterReadModel([new ThrowingProjectionSource(Workslip)]);

        var projection = await readModel.GetApplicationAsync(Workslip, ApplicationEnvironment.Production);

        Assert.Equal(ObservationState.Unknown, projection.Health.State);
    }

    [Fact]
    public async Task Unknown_projection_uses_the_injected_clock_for_recorded_at()
    {
        var now = DateTimeOffset.UnixEpoch.AddDays(3);
        var readModel = new InMemoryControlCenterReadModel([], new FixedTimeProvider(now));

        var projection = await readModel.GetApplicationAsync(Workslip, ApplicationEnvironment.Production);

        Assert.Equal(ObservationState.Unknown, projection.Health.State);
        Assert.Equal(now, projection.Health.RecordedAt);
    }
}
