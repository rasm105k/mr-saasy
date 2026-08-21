namespace MR.SAASy.Contracts.Motor.Domain;

/// <summary>Stable identity for a model-routing decision.</summary>
public readonly record struct ModelSelectionId(string Value)
{
    public override string ToString() => Value;
}
