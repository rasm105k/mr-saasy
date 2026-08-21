namespace MR.SAASy.Contracts.Motor.Domain;

/// <summary>Human approval bound to one exact project and action reference.</summary>
public sealed record Approval(
    ApprovalId Id,
    MissionId MissionId,
    MotorProjectContext Project,
    string ActionReference,
    ApprovalState State,
    string RequestedBy,
    DateTimeOffset RequestedAt,
    string? DecidedBy = null,
    DateTimeOffset? DecidedAt = null,
    DateTimeOffset? ExpiresAt = null,
    string? Reason = null,
    string? EvidenceReference = null)
{
    public bool IsApproved(DateTimeOffset now) =>
        State == ApprovalState.Approved &&
        !string.IsNullOrWhiteSpace(DecidedBy) &&
        DecidedAt is { } decidedAt &&
        decidedAt <= now &&
        ExpiresAt is { } expiresAt &&
        expiresAt > now;
}
