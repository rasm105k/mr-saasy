using MR.SAASy.Contracts.Motor.Domain;

namespace MR.SAASy.Contracts.Motor.Memory;

public sealed record SolutionMemory(
    MemoryRecordId Id,
    MissionId MissionId,
    string ProblemFingerprint,
    string SolutionSummary,
    IReadOnlyCollection<string> Tags,
    ExecutionOutcome Outcome,
    string EvidenceReference,
    DateTimeOffset CreatedAt);
