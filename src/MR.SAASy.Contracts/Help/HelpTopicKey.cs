namespace MR.SAASy.Contracts.Help;

/// <summary>
/// Stable help topic identifier. Example: workslip.jobs.create.
/// </summary>
public readonly record struct HelpTopicKey(string Value)
{
    public override string ToString() => Value;
}
