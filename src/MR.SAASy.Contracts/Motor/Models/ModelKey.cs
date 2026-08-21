namespace MR.SAASy.Contracts.Motor.Models;

/// <summary>Logical model identity; concrete provider deployment names stay in configuration.</summary>
public readonly record struct ModelKey(string Value)
{
    public override string ToString() => Value;
}
