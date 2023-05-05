using System;

[Serializable]
public enum TimerType
{
    Null = 0,
    Main,
    Battle
}

[Serializable]
public class Timer
{
    public TimerType type = TimerType.Null;
    public string name;
    public Action triggerHandler;
    public float triggerTime;
    public float delay;
    public bool loop;
    public bool isPuased;
    public bool isRemoved;
}