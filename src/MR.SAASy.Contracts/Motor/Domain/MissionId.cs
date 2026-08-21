namespace MR.SAASy.Contracts.Motor.Domain;

/// <summary>Stable identity for one auditable unit of orchestration.</summary>
public readonly record struct MissionId(string Value)
{
    public override string ToString() => Value;
}
