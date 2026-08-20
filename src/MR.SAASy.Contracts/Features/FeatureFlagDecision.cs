namespace MR.SAASy.Contracts.Features;

public sealed record FeatureFlagDecision(
    FeatureFlagKey Flag,
    FeatureFlagState State,
    FeatureFlagSource Source,
    string? Reason = null)
{
    public bool IsEnabled => State == FeatureFlagState.On;
}
