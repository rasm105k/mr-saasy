namespace MR.SAASy.Contracts.Motor.Domain;

public enum ExecutionOutcome
{
    Unknown = 0,
    Pending = 1,
    Succeeded = 2,
    Failed = 3,
    Denied = 4,
    Cancelled = 5,
}
