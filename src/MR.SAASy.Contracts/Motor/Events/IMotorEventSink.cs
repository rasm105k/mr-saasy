namespace MR.SAASy.Contracts.Motor.Events;

public interface IMotorEventSink
{
    ValueTask RecordAsync(
        MotorEvent motorEvent,
        CancellationToken cancellationToken = default);
}
