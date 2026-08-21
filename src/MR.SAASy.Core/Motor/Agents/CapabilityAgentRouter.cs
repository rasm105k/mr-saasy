using MR.SAASy.Contracts.Motor.Agents;
using MR.SAASy.Contracts.Motor.Domain;
using MR.SAASy.Contracts.Motor.Events;

namespace MR.SAASy.Core.Motor.Agents;

/// <summary>
/// Deterministic mission-to-specialist router. Unknown and unrepresented work is assigned
/// to QA for evaluation and cannot silently gain a write-capable specialist.
/// </summary>
public sealed class CapabilityAgentRouter : IAgentRouter
{
    private static readonly AgentId QaAgentId = new("qa");
    private readonly IAgentRegistry _registry;
    private readonly IMotorEventSink _eventSink;
    private readonly TimeProvider _timeProvider;

    public CapabilityAgentRouter(
        IAgentRegistry registry,
        IMotorEventSink eventSink,
        TimeProvider timeProvider)
    {
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
        _eventSink = eventSink ?? throw new ArgumentNullException(nameof(eventSink));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
    }

    public async ValueTask<AgentAssignment> AssignAsync(
        AgentRouteRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var agents = await _registry.ListAsync(cancellationToken);
        var requiredCapability = RequiredCapability(request.Mission.TaskType);
        var requiresHumanReview = requiredCapability is null;
        var agent = requiredCapability is null
            ? agents.SingleOrDefault(candidate => candidate.Id == QaAgentId)
            : agents.FirstOrDefault(candidate => candidate.Capabilities.Contains(
                requiredCapability,
                StringComparer.Ordinal));

        if (agent is null)
        {
            agent = agents.SingleOrDefault(candidate => candidate.Id == QaAgentId);
            requiresHumanReview = true;
        }

        if (agent is null)
        {
            throw new InvalidOperationException("MOTOR cannot assign a mission without a matching agent or QA fallback.");
        }

        var reason = requiresHumanReview
            ? "No exact specialist route exists; QA must evaluate and re-route the mission."
            : $"Agent declares the required '{requiredCapability}' capability.";
        var assignment = new AgentAssignment(
            request.Mission.Id,
            agent.Id,
            agent.Role,
            reason,
            requiresHumanReview,
            _timeProvider.GetUtcNow());

        await _eventSink.RecordAsync(
            new AgentAssigned(
                new MotorEventId(Guid.NewGuid().ToString("N")),
                request.Mission.Id,
                request.Mission.Project,
                assignment.AssignedAt,
                request.CorrelationId,
                assignment.AgentId,
                assignment.Role,
                assignment.Reason),
            cancellationToken);

        return assignment;
    }

    private static string? RequiredCapability(MissionTaskType taskType) => taskType switch
    {
        MissionTaskType.Code => "development",
        MissionTaskType.Operations => "operations",
        MissionTaskType.Infrastructure => "infrastructure",
        MissionTaskType.Monitoring => "monitoring",
        MissionTaskType.Testing => "testing",
        MissionTaskType.Security => "security",
        MissionTaskType.Analytics => "analytics",
        MissionTaskType.Documentation => "documentation-hygiene",
        _ => null,
    };
}
