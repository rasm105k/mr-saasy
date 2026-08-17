namespace MR.SAASy.Contracts.Audit;

/// <summary>
/// Sink for platform audit events. Implementations persist or forward event metadata only and
/// must never receive or store customer payload values.
/// </summary>
public interface IAuditSink
{
    ValueTask RecordAsync(AuditEvent auditEvent, CancellationToken cancellationToken = default);
}
