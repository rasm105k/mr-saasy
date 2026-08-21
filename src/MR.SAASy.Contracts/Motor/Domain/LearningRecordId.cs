namespace MR.SAASy.Contracts.Motor.Domain;

/// <summary>Stable identity for an auditable learning record.</summary>
public readonly record struct LearningRecordId(string Value)
{
    public override string ToString() => Value;
}
