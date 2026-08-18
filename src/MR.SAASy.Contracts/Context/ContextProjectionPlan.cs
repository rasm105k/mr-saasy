using MR.SAASy.Contracts.Capabilities;

namespace MR.SAASy.Contracts.Context;

/// <summary>
/// The field-shaping plan a product applies before handing context to an agent.
/// The platform decides which fields may be seen and which must be masked; the product
/// owns the data and applies the plan, so no customer values enter the platform.
/// </summary>
/// <param name="Capability">The capability the plan was resolved for.</param>
/// <param name="GrantedFields">Fields the product may include (both plaintext and masked).</param>
/// <param name="MaskedFields">Subset of <paramref name="GrantedFields"/> whose values must be masked.</param>
/// <param name="DeniedFields">Requested fields the product must omit entirely.</param>
public sealed record ContextProjectionPlan(
    CapabilityKey Capability,
    IReadOnlyCollection<ContextFieldKey> GrantedFields,
    IReadOnlyCollection<ContextFieldKey> MaskedFields,
    IReadOnlyCollection<ContextFieldKey> DeniedFields)
{
    /// <summary>
    /// Granted fields that are not masked — i.e. <see cref="GrantedFields"/> minus
    /// <see cref="MaskedFields"/>. This is the set a product may safely return as raw values;
    /// prefer it over <see cref="GrantedFields"/> when emitting plaintext, so masked fields are
    /// never leaked by returning "granted" verbatim.
    /// </summary>
    public IReadOnlyCollection<ContextFieldKey> PlaintextFields =>
        GrantedFields.Where(field => !MaskedFields.Contains(field)).ToArray();
}
