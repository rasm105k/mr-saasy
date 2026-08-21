namespace MR.SAASy.Contracts.Motor.Domain;

/// <summary>Stable MOTOR-owned agent identity.</summary>
public readonly record struct AgentId(string Value)
{
    public override string ToString() => Value;
}
