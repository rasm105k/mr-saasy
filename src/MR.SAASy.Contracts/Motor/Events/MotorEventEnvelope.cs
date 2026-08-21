using System.Text.Json;

namespace MR.SAASy.Contracts.Motor.Events;

/// <summary>Flattened transport envelope; the payload preserves the typed event contract.</summary>
public sealed record MotorEventEnvelope(
    string EventId,
    string EventType,
    int SchemaVersion,
    DateTimeOffset OccurredAt,
    string MissionId,
    string WorkspaceId,
    string ProjectId,
    string Environment,
    string? CustomerId,
    string CorrelationId,
    JsonElement Payload);
