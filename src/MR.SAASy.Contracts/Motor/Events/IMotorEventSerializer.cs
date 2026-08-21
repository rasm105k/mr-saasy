namespace MR.SAASy.Contracts.Motor.Events;

public interface IMotorEventSerializer
{
    MotorEventEnvelope CreateEnvelope(MotorEvent motorEvent);

    string Serialize(MotorEvent motorEvent);
}
