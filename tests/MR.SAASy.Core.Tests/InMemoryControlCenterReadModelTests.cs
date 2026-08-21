using MR.SAASy.Contracts.Application;
using MR.SAASy.Contracts.ControlCenter;
using MR.SAASy.Core.ControlCenter;
using Xunit;

namespace MR.SAASy.Core.Tests;

public sealed class InMemoryControlCenterReadModelTests
{
    private static readonly ApplicationIdentifier Workslip = new("workslip");
    private static readonly DateTimeOffset At = DateTimeOffset.UnixEpoch;

    private static ControlCenterProjection Projection(
        ApplicationIdentifier applicationId, ApplicationEnvironment environment, ObservationState state) =>
        new(
            applicationId,
            environment,
            new ApplicationHealthSnapshot(applicationId, environment, state, At, At),
            LatestDeployment: null,
            RecentRuns: []);

    [Fact]
    public async Task Returns_the_projection_from_a_registered_source()
    {
        var readModel = new InMemoryControlCenterReadModel(
            [new StaticControlCenterProjectionSource(
                Workslip, [Projection(Workslip, ApplicationEnvironment.Production, ObservationState.Healthy)])]);

        var projection = await readModel.GetApplicationAsync(Workslip, ApplicationEnvironment.Production);

        Assert.Equal(ObservationState.Healthy, projection.Health.State);
    }

    [Fact]
    public async Task Unknown_application_degrades_to_unknown_not_null()
    {
        var readModel = new InMemoryControlCenterReadModel([]);

        var projection = await readModel.GetApplicationAsync(Workslip, ApplicationEnvironment.Production);

        Assert.NotNull(projection);
        Assert.Equal(ObservationState.Unknown, projection.Health.State);
    }

    [Fact]
    public async Task Unknown_environment_on_a_known_source_degrades_to_unknown()
    {
        var readModel = new InMemoryControlCenterReadModel(
            [new StaticControlCenterProjectionSource(
                Workslip, [Projection(Workslip, ApplicationEnvironment.Production, ObservationState.Healthy)])]);

        var projection = await readModel.GetApplicationAsync(Workslip, ApplicationEnvironment.Staging);

        Assert.Equal(ObservationState.Unknown, projection.Health.State);
    }

    [Fact]
    public async Task GetAll_returns_one_projection_per_source_without_collapsing_state()
    {
        var marketing = new ApplicationIdentifier("marketing-site");
        var readModel = new InMemoryControlCenterReadModel(
        [
            new StaticControlCenterProjectionSource(
                Workslip, [Projection(Workslip, ApplicationEnvironment.Production, ObservationState.Healthy)]),
            new StaticControlCenterProjectionSource(
                marketing, [Projection(marketing, ApplicationEnvironment.Production, ObservationState.Degraded)]),
        ]);

        var all = await readModel.GetAllAsync(ApplicationEnvironment.Production);

        Assert.Equal(2, all.Count);
        Assert.Contains(all, p => p.ApplicationId == Workslip && p.Health.State == ObservationState.Healthy);
        Assert.Contains(all, p => p.ApplicationId == marketing && p.Health.State == ObservationState.Degraded);
    }

    [Fact]
    public void Source_rejects_a_projection_for_a_different_application()
    {
        var other = new ApplicationIdentifier("other");

        Assert.Throws<ArgumentException>(() =>
            new StaticControlCenterProjectionSource(
                Workslip, [Projection(other, ApplicationEnvironment.Production, ObservationState.Healthy)]));
    }
}
