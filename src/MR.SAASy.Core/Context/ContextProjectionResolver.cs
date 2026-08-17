using MR.SAASy.Contracts.Capabilities;
using MR.SAASy.Contracts.Context;

namespace MR.SAASy.Core.Context;

/// <summary>
/// Fail-closed <see cref="IContextProjectionResolver"/>. Given a capability and the fields an
/// agent requested, it returns the plan the product applies: granted fields (requested ∩
/// permitted), the masked subset of those, and the denied remainder. An unknown capability
/// grants nothing. The resolver never sees or holds field values.
/// </summary>
public sealed class ContextProjectionResolver : IContextProjectionResolver
{
    private readonly IContextFieldPolicy _policy;

    public ContextProjectionResolver(IContextFieldPolicy policy)
    {
        _policy = policy ?? throw new ArgumentNullException(nameof(policy));
    }

    public ContextProjectionPlan Resolve(
        CapabilityKey capability,
        IReadOnlyCollection<ContextFieldKey> requestedFields)
    {
        ArgumentNullException.ThrowIfNull(requestedFields);

        if (!_policy.Knows(capability))
        {
            return new ContextProjectionPlan(
                capability,
                Array.Empty<ContextFieldKey>(),
                Array.Empty<ContextFieldKey>(),
                Deduplicate(requestedFields));
        }

        var permitted = _policy.PermittedFields(capability);
        var masked = _policy.MaskedFields(capability);

        var granted = new List<ContextFieldKey>();
        var maskedGranted = new List<ContextFieldKey>();
        var denied = new List<ContextFieldKey>();
        var seen = new HashSet<ContextFieldKey>();

        foreach (var field in requestedFields)
        {
            if (!seen.Add(field))
            {
                continue;
            }

            if (permitted.Contains(field))
            {
                granted.Add(field);
                if (masked.Contains(field))
                {
                    maskedGranted.Add(field);
                }
            }
            else
            {
                denied.Add(field);
            }
        }

        return new ContextProjectionPlan(capability, granted, maskedGranted, denied);
    }

    private static IReadOnlyCollection<ContextFieldKey> Deduplicate(IReadOnlyCollection<ContextFieldKey> fields)
    {
        var seen = new HashSet<ContextFieldKey>();
        var result = new List<ContextFieldKey>();
        foreach (var field in fields)
        {
            if (seen.Add(field))
            {
                result.Add(field);
            }
        }

        return result;
    }
}
