namespace MR.SAASy.Core.Motor.Supervision;

public sealed class AgentSupervisor
{
    private readonly Dictionary<string, int> _failures = new();

    public bool ShouldRestart(string agentId, RestartPolicy policy)
    {
        return policy switch
        {
            RestartPolicy.Always => true,
            RestartPolicy.OnFailure => true,
            _ => false
        };
    }

    public void RecordFailure(string agentId)
    {
        _failures[agentId] = _failures.GetValueOrDefault(agentId) + 1;
    }

    public int FailureCount(string agentId)
    {
        return _failures.GetValueOrDefault(agentId);
    }
}
