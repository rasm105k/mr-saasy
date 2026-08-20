using MR.SAASy.Contracts.Application;
using MR.SAASy.Contracts.Help;

namespace MR.SAASy.Core.Help;

public sealed class InMemoryHelpCatalog : IHelpCatalog
{
    private readonly IReadOnlyDictionary<(ApplicationIdentifier, HelpTopicKey), HelpPrompt> _prompts;

    public InMemoryHelpCatalog(IEnumerable<HelpPrompt> prompts)
    {
        ArgumentNullException.ThrowIfNull(prompts);

        var byKey = new Dictionary<(ApplicationIdentifier, HelpTopicKey), HelpPrompt>();
        foreach (var prompt in prompts)
        {
            byKey[(prompt.ApplicationId, prompt.Topic)] = prompt;
        }

        _prompts = byKey;
    }

    public ValueTask<HelpPrompt?> FindAsync(
        ApplicationIdentifier applicationId,
        HelpTopicKey topic,
        CancellationToken cancellationToken = default) =>
        ValueTask.FromResult(
            _prompts.TryGetValue((applicationId, topic), out var prompt) ? prompt : null);
}
