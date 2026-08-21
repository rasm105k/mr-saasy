using System.Text.Json;
using System.Text.Json.Serialization;
using MR.SAASy.Contracts.Motor.Events;

namespace MR.SAASy.Core.Motor.Events;

/// <summary>Creates a stable camel-case JSON envelope without provider-specific SDK types.</summary>
public sealed class SystemTextJsonMotorEventSerializer : IMotorEventSerializer
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false,
    };

    public MotorEventEnvelope CreateEnvelope(MotorEvent motorEvent)
    {
        ArgumentNullException.ThrowIfNull(motorEvent);

        return new MotorEventEnvelope(
            motorEvent.EventId.Value,
            motorEvent.EventType,
            motorEvent.SchemaVersion,
            motorEvent.OccurredAt,
            motorEvent.MissionId.Value,
            motorEvent.Project.WorkspaceId,
            motorEvent.Project.ProjectId,
            motorEvent.Project.Environment,
            motorEvent.Project.CustomerId,
            motorEvent.CorrelationId,
            JsonSerializer.SerializeToElement(motorEvent, motorEvent.GetType(), Options));
    }

    public string Serialize(MotorEvent motorEvent) =>
        JsonSerializer.Serialize(CreateEnvelope(motorEvent), Options);
}
