namespace MR.SAASy.Contracts.Features;

public interface IFeatureFlagEvaluator
{
    ValueTask<FeatureFlagDecision> EvaluateAsync(
        FeatureFlagQuery query,
        CancellationToken cancellationToken = default);
}
