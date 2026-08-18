using MR.SAASy.Contracts.Application;
using MR.SAASy.Contracts.Capabilities;
using MR.SAASy.Contracts.Tenant;

namespace MR.SAASy.Core.Capabilities;

/// <summary>
/// Provider-neutral, in-memory <see cref="ICapabilityRegistry"/> seeded from explicit capability
/// descriptors and per-(tenant, application, capability) decisions. A platform default for
/// local/dev and integration tests. A target with no seeded decision resolves fail-closed to
/// <see cref="CapabilityDecisionState.Unknown"/>.
/// </summary>
public sealed class InMemoryCapabilityRegistry : ICapabilityRegistry
{
    private readonly IReadOnlyDictionary<CapabilityKey, CapabilityDescriptor> _descriptors;
    private readonly IReadOnlyDictionary<(TenantId Tenant, ApplicationIdentifier Application, CapabilityKey Capability), CapabilityDecision> _decisions;

    public InMemoryCapabilityRegistry(
        IEnumerable<CapabilityDescriptor> descriptors,
        IEnumerable<CapabilityDecision> decisions)
    {
        ArgumentNullException.ThrowIfNull(descriptors);
        ArgumentNullException.ThrowIfNull(decisions);

        var byKey = new Dictionary<CapabilityKey, CapabilityDescriptor>();
        foreach (var descriptor in descriptors)
        {
            byKey[descriptor.Key] = descriptor;
        }

        _descriptors = byKey;

        var byTarget = new Dictionary<(TenantId, ApplicationIdentifier, CapabilityKey), CapabilityDecision>();
        foreach (var decision in decisions)
        {
            byTarget[(decision.TenantId, decision.ApplicationId, decision.CapabilityKey)] = decision;
        }

        _decisions = byTarget;
    }

    public ValueTask<CapabilityDescriptor?> FindAsync(
        CapabilityKey capabilityKey,
        CancellationToken cancellationToken = default) =>
        ValueTask.FromResult(_descriptors.TryGetValue(capabilityKey, out var descriptor) ? descriptor : null);

    public ValueTask<CapabilityDecision> ResolveAsync(
        TenantId tenantId,
        ApplicationIdentifier applicationId,
        CapabilityKey capabilityKey,
        CancellationToken cancellationToken = default) =>
        ValueTask.FromResult(
            _decisions.TryGetValue((tenantId, applicationId, capabilityKey), out var decision)
                ? decision
                : new CapabilityDecision(
                    tenantId,
                    applicationId,
                    capabilityKey,
                    CapabilityDecisionState.Unknown,
                    Reason: "No capability decision configured."));
}
