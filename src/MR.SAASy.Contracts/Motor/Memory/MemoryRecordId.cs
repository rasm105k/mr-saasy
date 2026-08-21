namespace MR.SAASy.Contracts.Motor.Memory;

public readonly record struct MemoryRecordId(string Value)
{
    public override string ToString() => Value;
}
