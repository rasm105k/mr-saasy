using MR.SAASy.Contracts.Application;
using MR.SAASy.Contracts.Features;
using MR.SAASy.Contracts.Identity;
using MR.SAASy.Contracts.Tenant;

namespace MR.SAASy.Core.Features;

/// <summary>
/// Fail-closed evaluator. Platform kill wins. Then identity, tenant, application.
/// Unseeded flags stay off.
/// </summary>
public sealed class InMemoryFeatureFlagEvaluator : IFeatureFlagEvaluator
{
    private readonly HashSet<FeatureFlagKey> _killed;
    private readonly Dictionary<FeatureFlagKey, FeatureFlagState> _platform;
    private readonly Dictionary<(FeatureFlagKey, ApplicationIdentifier), FeatureFlagState> _application;
    private readonly Dictionary<(FeatureFlagKey, TenantId), FeatureFlagState> _tenant;
    private readonly Dictionary<(FeatureFlagKey, IdentityId), FeatureFlagState> _identity;

    public InMemoryFeatureFlagEvaluator(IEnumerable<FeatureFlagAssignment> assignments)
    {
        ArgumentNullException.ThrowIfNull(assignments);

        _killed = [];
        _platform = [];
        _application = [];
        _tenant = [];
        _identity = [];

        foreach (var assignment in assignments)
        {
            if (assignment.State == FeatureFlagState.Killed || assignment.Source == FeatureFlagSource.PlatformKill)
            {
                _killed.Add(assignment.Flag);
                continue;
            }

            if (assignment.IdentityId is { } identityId)
            {
                _identity[(assignment.Flag, identityId)] = assignment.State;
                continue;
            }

            if (assignment.TenantId is { } tenantId)
            {
                _tenant[(assignment.Flag, tenantId)] = assignment.State;
                continue;
            }

            if (assignment.ApplicationId is { } applicationId)
            {
                _application[(assignment.Flag, applicationId)] = assignment.State;
                continue;
            }

            _platform[assignment.Flag] = assignment.State;
        }
    }

    public ValueTask<FeatureFlagDecision> EvaluateAsync(
        FeatureFlagQuery query,
        CancellationToken cancellationToken = default)
    {
        if (_killed.Contains(query.Flag))
        {
            return ValueTask.FromResult(new FeatureFlagDecision(
                query.Flag,
                FeatureFlagState.Killed,
                FeatureFlagSource.PlatformKill,
                "Platform kill switch."));
        }

        if (query.IdentityId is { } identityId &&
            _identity.TryGetValue((query.Flag, identityId), out var identityState))
        {
            return ValueTask.FromResult(Decision(query.Flag, identityState, FeatureFlagSource.Identity));
        }

        if (query.TenantId is { } tenantId &&
            _tenant.TryGetValue((query.Flag, tenantId), out var tenantState))
        {
            return ValueTask.FromResult(Decision(query.Flag, tenantState, FeatureFlagSource.Tenant));
        }

        if (_application.TryGetValue((query.Flag, query.ApplicationId), out var applicationState))
        {
            return ValueTask.FromResult(Decision(query.Flag, applicationState, FeatureFlagSource.Application));
        }

        if (_platform.TryGetValue(query.Flag, out var platformState))
        {
            return ValueTask.FromResult(Decision(query.Flag, platformState, FeatureFlagSource.DefaultOff));
        }

        return ValueTask.FromResult(new FeatureFlagDecision(
            query.Flag,
            FeatureFlagState.Off,
            FeatureFlagSource.DefaultOff,
            "Unconfigured flags stay off."));
    }

    private static FeatureFlagDecision Decision(
        FeatureFlagKey flag,
        FeatureFlagState state,
        FeatureFlagSource source) =>
        new(flag, state, source);
}
