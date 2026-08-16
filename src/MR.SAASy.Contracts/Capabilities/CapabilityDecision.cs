using MR.SAASy.Contracts.Tenant;

namespace MR.SAASy.Contracts.Capabilities;

public sealed record CapabilityDecision(
    TenantId TenantId,
    string ApplicationId,
    CapabilityKey CapabilityKey,
    CapabilityDecisionState State,
    CapabilityGrantSource Source = CapabilityGrantSource.Unknown,
    string? Reason = null)
{
    public bool IsEnabled => State == CapabilityDecisionState.Enabled;
}
