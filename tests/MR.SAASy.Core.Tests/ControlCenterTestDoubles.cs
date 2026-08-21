using MR.SAASy.Contracts.Application;
using MR.SAASy.Contracts.ControlCenter;

namespace MR.SAASy.Core.Tests;

/// <summary>A deterministic clock for asserting recorded timestamps.</summary>
internal sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
{
    public override DateTimeOffset GetUtcNow() => now;
}

/// <summary>A projection source that always throws, standing in for an unreachable adapter.</summary>
internal sealed class ThrowingProjectionSource(ApplicationIdentifier applicationId) : IControlCenterProjectionSource
{
    public ApplicationIdentifier ApplicationId { get; } = applicationId;

    public ValueTask<ControlCenterProjection> GetProjectionAsync(
        ApplicationEnvironment environment,
        CancellationToken cancellationToken = default) =>
        throw new InvalidOperationException("source unavailable");
}
