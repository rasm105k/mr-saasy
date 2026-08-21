using MR.SAASy.Contracts.Application;

namespace MR.SAASy.Contracts.Help;

/// <summary>
/// Product-agnostic help copy. Workslip and later services register topics;
/// the UI agent only renders what the catalog returns.
/// </summary>
public interface IHelpCatalog
{
    ValueTask<HelpPrompt?> FindAsync(
        ApplicationIdentifier applicationId,
        HelpTopicKey topic,
        CancellationToken cancellationToken = default);
}
