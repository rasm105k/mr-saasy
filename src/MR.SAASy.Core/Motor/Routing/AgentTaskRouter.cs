namespace MR.SAASy.Core.Motor.Routing;

/// <summary>
/// Minimal deterministic routing foundation for MOTOR-002.
/// Provider execution is intentionally outside this boundary.
/// </summary>
public sealed class AgentTaskRouter
{
    private readonly IReadOnlyList<AgentRoute> _routes;

    public AgentTaskRouter(IEnumerable<AgentRoute> routes)
    {
        _routes = routes.ToList();
    }

    public AgentRoute? Resolve(string capability)
    {
        return _routes.FirstOrDefault(x =>
            string.Equals(x.Capability, capability, StringComparison.OrdinalIgnoreCase));
    }
}

public sealed record AgentRoute(string AgentId, string Capability, int Priority);
