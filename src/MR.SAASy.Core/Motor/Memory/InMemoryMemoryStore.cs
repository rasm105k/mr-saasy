namespace MR.SAASy.Core.Motor.Memory;

public sealed class InMemoryMemoryStore : IMemoryStore
{
    private readonly List<AgentMemory> _memories = new();

    public void Store(AgentMemory memory)
    {
        _memories.Add(memory);
    }

    public IReadOnlyCollection<AgentMemory> Search(string agentId)
    {
        return _memories
            .Where(memory => memory.AgentId == agentId)
            .ToList();
    }
}
