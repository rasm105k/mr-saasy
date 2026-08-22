namespace MR.SAASy.Core.Motor.Memory;

public sealed record AgentMemory(
    string AgentId,
    string MemoryType,
    string Content,
    DateTime CreatedAt);
