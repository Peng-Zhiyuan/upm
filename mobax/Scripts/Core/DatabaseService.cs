using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DatabaseService : Service
{
    public override void OnCreate()
    {
        Database.RequestTimestap = RequestTimestamp;
    }

    long RequestTimestamp()
    {
        return Clock.TimestampMs;
    }
}
