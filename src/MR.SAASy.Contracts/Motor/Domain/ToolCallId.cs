namespace MR.SAASy.Contracts.Motor.Domain;

/// <summary>Stable identity for one requested tool invocation.</summary>
public readonly record struct ToolCallId(string Value)
{
    public override string ToString() => Value;
}
