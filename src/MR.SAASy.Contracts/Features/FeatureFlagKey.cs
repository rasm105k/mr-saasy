namespace MR.SAASy.Contracts.Features;

/// <summary>
/// Stable namespaced identifier for an experimental or delight feature.
/// Example: platform.help-wizard.
/// </summary>
public readonly record struct FeatureFlagKey(string Value)
{
    public override string ToString() => Value;
}
