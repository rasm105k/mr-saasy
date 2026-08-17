namespace MR.SAASy.Contracts.Audit;

/// <summary>
/// A single audit record. <see cref="Metadata"/> carries identifiers, field names, decision
/// states and reasons only — never customer field values. Receipt timestamping is the sink's
/// responsibility.
/// </summary>
public sealed record AuditEvent(
    string Name,
    IReadOnlyDictionary<string, string?> Metadata);
