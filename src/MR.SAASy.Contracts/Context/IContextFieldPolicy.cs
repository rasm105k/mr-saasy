using MR.SAASy.Contracts.Capabilities;

namespace MR.SAASy.Contracts.Context;

/// <summary>
/// Declares, per capability, which product-context fields are permitted and which of those
/// must be masked. Provider-neutral context-shaping policy; it never carries field values.
/// </summary>
public interface IContextFieldPolicy
{
    bool Knows(CapabilityKey capability);

    IReadOnlyCollection<ContextFieldKey> PermittedFields(CapabilityKey capability);

    IReadOnlyCollection<ContextFieldKey> MaskedFields(CapabilityKey capability);
}
