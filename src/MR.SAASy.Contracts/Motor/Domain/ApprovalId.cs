namespace MR.SAASy.Contracts.Motor.Domain;

/// <summary>Stable identity for a human approval decision.</summary>
public readonly record struct ApprovalId(string Value)
{
    public override string ToString() => Value;
}
