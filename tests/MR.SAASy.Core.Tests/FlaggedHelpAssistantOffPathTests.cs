using MR.SAASy.Contracts.Application;
using MR.SAASy.Contracts.Features;
using MR.SAASy.Contracts.Help;
using MR.SAASy.Contracts.Identity;
using MR.SAASy.Contracts.Tenant;
using MR.SAASy.Core.Features;
using MR.SAASy.Core.Help;
using Xunit;

namespace MR.SAASy.Core.Tests;

public sealed class FlaggedHelpAssistantOffPathTests
{
    private static readonly ApplicationIdentifier Workslip = new("workslip");
    private static readonly TenantId TenantA = new("tenant-a");
    private static readonly IdentityId UserA = new("user-a");
    private static readonly HelpTopicKey CreateJob = new("workslip.jobs.create");
    private static readonly HelpPrompt Seeded = new(CreateJob, Workslip, "Skal jeg hjælpe med jobbet?");

    private static FlaggedHelpAssistant Assistant(params FeatureFlagAssignment[] assignments) =>
        new(
            new InMemoryFeatureFlagEvaluator(assignments),
            new RecordingHelpCatalog([Seeded]));

    [Fact]
    [Trait("Trait", "OffPath")]
    public async Task Unseeded_flag_returns_null_and_does_not_read_catalog()
    {
        var catalog = new RecordingHelpCatalog([Seeded]);
        var assistant = new FlaggedHelpAssistant(new InMemoryFeatureFlagEvaluator([]), catalog);

        var prompt = await assistant.TryGetPromptAsync(new HelpRequest(Workslip, CreateJob, TenantA, UserA));

        Assert.Null(prompt);
        Assert.Equal(0, catalog.FindCalls);
    }

    [Fact]
    [Trait("Trait", "OffPath")]
    public async Task Platform_kill_returns_null_even_when_identity_is_on()
    {
        var catalog = new RecordingHelpCatalog([Seeded]);
        var assistant = new FlaggedHelpAssistant(
            new InMemoryFeatureFlagEvaluator(
            [
                new FeatureFlagAssignment(PlatformFeatureFlags.HelpWizard, FeatureFlagState.Killed, FeatureFlagSource.PlatformKill),
                new FeatureFlagAssignment(PlatformFeatureFlags.HelpWizard, FeatureFlagState.On, FeatureFlagSource.Identity, IdentityId: UserA)
            ]),
            catalog);

        var prompt = await assistant.TryGetPromptAsync(new HelpRequest(Workslip, CreateJob, TenantA, UserA));

        Assert.Null(prompt);
        Assert.Equal(0, catalog.FindCalls);
    }

    [Fact]
    [Trait("Trait", "OffPath")]
    public async Task Identity_off_returns_null_when_application_is_on()
    {
        var catalog = new RecordingHelpCatalog([Seeded]);
        var assistant = new FlaggedHelpAssistant(
            new InMemoryFeatureFlagEvaluator(
            [
                new FeatureFlagAssignment(PlatformFeatureFlags.HelpWizard, FeatureFlagState.On, FeatureFlagSource.Application, Workslip),
                new FeatureFlagAssignment(PlatformFeatureFlags.HelpWizard, FeatureFlagState.Off, FeatureFlagSource.Identity, IdentityId: UserA)
            ]),
            catalog);

        var prompt = await assistant.TryGetPromptAsync(new HelpRequest(Workslip, CreateJob, TenantA, UserA));

        Assert.Null(prompt);
        Assert.Equal(0, catalog.FindCalls);
    }

    [Fact]
    [Trait("Trait", "OffPath")]
    public async Task Tenant_off_returns_null_when_application_is_on()
    {
        var assistant = Assistant(
            new FeatureFlagAssignment(PlatformFeatureFlags.HelpWizard, FeatureFlagState.On, FeatureFlagSource.Application, Workslip),
            new FeatureFlagAssignment(PlatformFeatureFlags.HelpWizard, FeatureFlagState.Off, FeatureFlagSource.Tenant, TenantId: TenantA));

        var prompt = await assistant.TryGetPromptAsync(new HelpRequest(Workslip, CreateJob, TenantA, UserA));

        Assert.Null(prompt);
    }

    [Fact]
    public async Task Enabled_flag_returns_catalog_prompt()
    {
        var catalog = new RecordingHelpCatalog([Seeded]);
        var assistant = new FlaggedHelpAssistant(
            new InMemoryFeatureFlagEvaluator(
            [
                new FeatureFlagAssignment(PlatformFeatureFlags.HelpWizard, FeatureFlagState.On, FeatureFlagSource.Application, Workslip)
            ]),
            catalog);

        var prompt = await assistant.TryGetPromptAsync(new HelpRequest(Workslip, CreateJob, TenantA));

        Assert.NotNull(prompt);
        Assert.Equal(Seeded.Text, prompt.Text);
        Assert.Equal(1, catalog.FindCalls);
    }

    private sealed class RecordingHelpCatalog : IHelpCatalog
    {
        private readonly InMemoryHelpCatalog _inner;

        public RecordingHelpCatalog(IEnumerable<HelpPrompt> prompts)
        {
            _inner = new InMemoryHelpCatalog(prompts);
        }

        public int FindCalls { get; private set; }

        public ValueTask<HelpPrompt?> FindAsync(
            ApplicationIdentifier applicationId,
            HelpTopicKey topic,
            CancellationToken cancellationToken = default)
        {
            FindCalls++;
            return _inner.FindAsync(applicationId, topic, cancellationToken);
        }
    }
}
