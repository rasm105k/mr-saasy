namespace MR.SAASy.Contracts.Access;

public readonly record struct AccessGrantId(string Value)
{
    public override string ToString() => Value;
}
