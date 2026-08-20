using MR.SAASy.Contracts.Application;
using MR.SAASy.Contracts.Help;
using MR.SAASy.Core.Help;
using Xunit;

namespace MR.SAASy.Core.Tests;

public sealed class InMemoryHelpCatalogTests
{
    private static readonly ApplicationIdentifier Workslip = new("workslip");
    private static readonly HelpTopicKey CreateJob = new("workslip.jobs.create");

    [Fact]
    public async Task Finds_seeded_prompt()
    {
        var catalog = new InMemoryHelpCatalog(
        [
            new HelpPrompt(CreateJob, Workslip, "Skal jeg hjælpe med jobbet?")
        ]);

        var prompt = await catalog.FindAsync(Workslip, CreateJob);

        Assert.NotNull(prompt);
        Assert.Equal("Skal jeg hjælpe med jobbet?", prompt.Text);
    }

    [Fact]
    public async Task Missing_topic_returns_null()
    {
        var catalog = new InMemoryHelpCatalog([]);

        var prompt = await catalog.FindAsync(Workslip, CreateJob);

        Assert.Null(prompt);
    }
}
