using MR.SAASy.Contracts.Audit;
using MR.SAASy.Core.Audit;
using Xunit;

namespace MR.SAASy.Core.Tests;

public sealed class InMemoryAuditSinkTests
{
    [Fact]
    public async Task Retains_recorded_events_in_order()
    {
        var sink = new InMemoryAuditSink();

        await sink.RecordAsync(new AuditEvent("first", new Dictionary<string, string?> { ["k"] = "1" }));
        await sink.RecordAsync(new AuditEvent("second", new Dictionary<string, string?> { ["k"] = "2" }));

        Assert.Equal(new[] { "first", "second" }, sink.Events.Select(e => e.Name).ToArray());
        Assert.Equal("2", sink.Events[1].Metadata["k"]);
    }
}
