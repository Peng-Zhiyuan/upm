using UnityEngine;

public class ReminderNodeView : MonoBehaviour, IReminderNodeView
{
    public Transform reminder;

    public Transform Reminder
    {
        get => reminder;
        set => reminder = value;
    }
}