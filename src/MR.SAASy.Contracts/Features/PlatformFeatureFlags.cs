namespace MR.SAASy.Contracts.Features;

/// <summary>
/// Shared flags that products consume through the platform evaluator.
/// </summary>
public static class PlatformFeatureFlags
{
    public static FeatureFlagKey HelpWizard { get; } = new("platform.help-wizard");
}
