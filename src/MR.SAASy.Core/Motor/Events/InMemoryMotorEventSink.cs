using MR.SAASy.Contracts.Motor.Events;

namespace MR.SAASy.Core.Motor.Events;

/// <summary>Ordered single-process event sink for tests and local composition.</summary>
public sealed class InMemoryMotorEventSink : IMotorEventSink
{
    private readonly object _sync = new();
    private readonly List<MotorEvent> _events = [];

    public IReadOnlyCollection<MotorEvent> Events
    {
        get
        {
            lock (_sync)
            {
                return _events.ToArray();
            }
        }
    }

    public ValueTask RecordAsync(MotorEvent motorEvent, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(motorEvent);
        lock (_sync)
        {
            _events.Add(motorEvent);
        }

        return ValueTask.CompletedTask;
    }
}
