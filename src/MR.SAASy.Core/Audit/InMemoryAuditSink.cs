using MR.SAASy.Contracts.Audit;

namespace MR.SAASy.Core.Audit;

/// <summary>
/// In-memory <see cref="IAuditSink"/> that retains recorded events in order. A platform default
/// for local/dev and tests; it stores metadata only, never customer values.
/// </summary>
public sealed class InMemoryAuditSink : IAuditSink
{
    private readonly List<AuditEvent> _events = [];

    public IReadOnlyList<AuditEvent> Events => _events;

    public ValueTask RecordAsync(AuditEvent auditEvent, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(auditEvent);
        _events.Add(auditEvent);
        return ValueTask.CompletedTask;
    }
}
