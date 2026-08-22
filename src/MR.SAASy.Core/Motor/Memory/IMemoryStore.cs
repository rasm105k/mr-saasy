namespace MR.SAASy.Core.Motor.Memory;

public interface IMemoryStore
{
    void Store(AgentMemory memory);

    IReadOnlyCollection<AgentMemory> Search(string agentId);
}
