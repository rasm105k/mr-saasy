using MR.SAASy.Contracts.Features;
using MR.SAASy.Contracts.Help;

namespace MR.SAASy.Core.Help;

/// <summary>
/// Single gate for the help-wizard feature. Off and kill return null without reading the catalog.
/// </summary>
public sealed class FlaggedHelpAssistant : IHelpAssistant
{
    private readonly IFeatureFlagEvaluator _flags;
    private readonly IHelpCatalog _catalog;

    public FlaggedHelpAssistant(IFeatureFlagEvaluator flags, IHelpCatalog catalog)
    {
        ArgumentNullException.ThrowIfNull(flags);
        ArgumentNullException.ThrowIfNull(catalog);
        _flags = flags;
        _catalog = catalog;
    }

    public async ValueTask<HelpPrompt?> TryGetPromptAsync(
        HelpRequest request,
        CancellationToken cancellationToken = default)
    {
        var decision = await _flags.EvaluateAsync(
            new FeatureFlagQuery(
                PlatformFeatureFlags.HelpWizard,
                request.ApplicationId,
                request.TenantId,
                request.IdentityId),
            cancellationToken);

        if (!decision.IsEnabled)
        {
            return null;
        }

        return await _catalog.FindAsync(request.ApplicationId, request.Topic, cancellationToken);
    }
}
