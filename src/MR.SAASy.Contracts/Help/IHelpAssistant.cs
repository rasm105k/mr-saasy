namespace MR.SAASy.Contracts.Help;

/// <summary>
/// Entry point for help. Off-path must return null and must not expose catalog copy.
/// </summary>
public interface IHelpAssistant
{
    ValueTask<HelpPrompt?> TryGetPromptAsync(
        HelpRequest request,
        CancellationToken cancellationToken = default);
}
