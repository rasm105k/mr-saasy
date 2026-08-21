namespace MR.SAASy.Contracts.Motor.Domain;

/// <summary>Globally unique identity for a MOTOR event.</summary>
public readonly record struct MotorEventId(string Value)
{
    public override string ToString() => Value;
}
