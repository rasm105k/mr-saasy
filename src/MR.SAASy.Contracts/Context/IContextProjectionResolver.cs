using MR.SAASy.Contracts.Capabilities;

namespace MR.SAASy.Contracts.Context;

/// <summary>
/// Resolves a fail-closed <see cref="ContextProjectionPlan"/> for a capability and a set of
/// requested fields: minimization (only permitted fields are granted) and masking hooks,
/// without ever seeing field values.
/// </summary>
public interface IContextProjectionResolver
{
    ContextProjectionPlan Resolve(
        CapabilityKey capability,
        IReadOnlyCollection<ContextFieldKey> requestedFields);
}
