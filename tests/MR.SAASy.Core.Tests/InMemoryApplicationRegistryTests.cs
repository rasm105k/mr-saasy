using MR.SAASy.Contracts.Application;
using MR.SAASy.Core.Application;
using Xunit;

namespace MR.SAASy.Core.Tests;

public sealed class InMemoryApplicationRegistryTests
{
    private static readonly ApplicationIdentifier Workslip = new("workslip");

    [Fact]
    public async Task Finds_a_seeded_application_for_its_environment()
    {
        var registry = new InMemoryApplicationRegistry(
            [new ApplicationDescriptor(Workslip, "Workslip", "1.0.0", ApplicationEnvironment.Production)]);

        var application = await registry.GetAsync(Workslip, ApplicationEnvironment.Production);

        Assert.NotNull(application);
        Assert.Equal("Workslip", application.Name);
    }

    [Fact]
    public async Task Returns_null_for_a_different_environment()
    {
        var registry = new InMemoryApplicationRegistry(
            [new ApplicationDescriptor(Workslip, "Workslip", "1.0.0", ApplicationEnvironment.Production)]);

        Assert.Null(await registry.GetAsync(Workslip, ApplicationEnvironment.Staging));
    }

    [Fact]
    public async Task Returns_null_for_unknown_application()
    {
        var registry = new InMemoryApplicationRegistry([]);

        Assert.Null(await registry.GetAsync(Workslip, ApplicationEnvironment.Production));
    }
}
