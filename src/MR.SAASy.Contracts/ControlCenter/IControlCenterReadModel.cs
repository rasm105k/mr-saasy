using MR.SAASy.Contracts.Application;

namespace MR.SAASy.Contracts.ControlCenter;

/// <summary>
/// Read-only aggregation/query boundary the Control Center BFF/UI consumes (ADR 0009). It exposes no
/// orchestration or mutation: re-run, deploy, merge and rollback stay in their owning systems. A
/// query for an application with no registered source returns an explicit Unknown projection, never
/// null and never a healthy default.
/// </summary>
public interface IControlCenterReadModel
{
    ValueTask<ControlCenterProjection> GetApplicationAsync(
        ApplicationIdentifier applicationId,
        ApplicationEnvironment environment,
        CancellationToken cancellationToken = default);

    ValueTask<IReadOnlyCollection<ControlCenterProjection>> GetAllAsync(
        ApplicationEnvironment environment,
        CancellationToken cancellationToken = default);
}
