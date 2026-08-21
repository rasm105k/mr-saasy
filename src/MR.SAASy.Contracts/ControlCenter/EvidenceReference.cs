namespace MR.SAASy.Contracts.ControlCenter;

/// <summary>
/// A provenance pointer back to the owning source of truth for an observation. The read model keeps
/// references, never copies (ADR 0009): raw provider payloads, logs, traces, exception bodies,
/// secrets and customer PII must not be carried here. <see cref="Reference"/> is an opaque source
/// identifier (for example a workflow run id, deployment id or commit sha); <see cref="Uri"/> is an
/// optional human-navigable link to the same evidence.
/// </summary>
public sealed record EvidenceReference(
    string Source,
    string Kind,
    string Reference,
    string? Uri = null);
