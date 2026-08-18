using MR.SAASy.Contracts.Application;

namespace MR.SAASy.Contracts.ControlCenter;

/// <summary>
/// Provider/product-owned adapter port. Each registered application implements one source that
/// projects its own health, deployment and run state into provider-neutral contracts. Adding a new
/// product means adding a source, not changing the read model or its consumers (ADR 0009, ADR 0010).
/// Implementations must not leak product-domain types, raw provider payloads or secrets across this
/// boundary, and must map absent/unreachable state to explicit Unknown rather than a healthy value.
/// </summary>
public interface IControlCenterProjectionSource
{
    /// <summary>The stable platform application identity this source projects.</summary>
    ApplicationIdentifier ApplicationId { get; }

    ValueTask<ControlCenterProjection> GetProjectionAsync(
        ApplicationEnvironment environment,
        CancellationToken cancellationToken = default);
}
