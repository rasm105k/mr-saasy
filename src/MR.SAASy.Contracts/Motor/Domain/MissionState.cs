namespace MR.SAASy.Contracts.Motor.Domain;

public enum MissionState
{
    Planned = 0,
    Running = 1,
    WaitingForApproval = 2,
    Blocked = 3,
    Completed = 4,
    Failed = 5,
    Cancelled = 6,
}
