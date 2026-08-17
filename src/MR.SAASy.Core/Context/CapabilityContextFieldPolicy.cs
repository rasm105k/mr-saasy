using MR.SAASy.Contracts.Capabilities;
using MR.SAASy.Contracts.Context;

namespace MR.SAASy.Core.Context;

/// <summary>
/// In-memory <see cref="IContextFieldPolicy"/> declaring the permitted and masked field sets
/// for each capability. Masked fields must be a subset of permitted fields; an unknown
/// capability exposes no fields.
/// </summary>
public sealed class CapabilityContextFieldPolicy : IContextFieldPolicy
{
    public sealed record CapabilityFields(
        IReadOnlyCollection<ContextFieldKey> Permitted,
        IReadOnlyCollection<ContextFieldKey> Masked);

    private readonly IReadOnlyDictionary<CapabilityKey, CapabilityFields> _byCapability;

    public CapabilityContextFieldPolicy(IReadOnlyDictionary<CapabilityKey, CapabilityFields> capabilities)
    {
        ArgumentNullException.ThrowIfNull(capabilities);

        foreach (var (capability, fields) in capabilities)
        {
            foreach (var masked in fields.Masked)
            {
                if (!fields.Permitted.Contains(masked))
                {
                    throw new ArgumentException(
                        $"Capability '{capability.Value}' masks '{masked.Value}', which is not one of its permitted fields.",
                        nameof(capabilities));
                }
            }
        }

        _byCapability = capabilities;
    }

    public bool Knows(CapabilityKey capability) => _byCapability.ContainsKey(capability);

    public IReadOnlyCollection<ContextFieldKey> PermittedFields(CapabilityKey capability) =>
        _byCapability.TryGetValue(capability, out var fields)
            ? fields.Permitted
            : Array.Empty<ContextFieldKey>();

    public IReadOnlyCollection<ContextFieldKey> MaskedFields(CapabilityKey capability) =>
        _byCapability.TryGetValue(capability, out var fields)
            ? fields.Masked
            : Array.Empty<ContextFieldKey>();
}
