using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ConsoleManager : StuffObject<ConsoleManager>
{

	private int timezone = 8;

    public int MaxCount = 1000;

    public int Count = 0;

    public Queue<GameUnityLog> logs = new Queue<GameUnityLog>();

    void Awake()
    {
        Application.logMessageReceived += LogCallback;
    }

    public Action<GameUnityLog> onRefresh; 


    public void LogCallback(string condition, string stackTrace, LogType type){
        var log = new GameUnityLog();
        log.condition = condition;
        log.stack = stackTrace;
        log.type = type;

        
        var startZeroTime = new DateTime(1970, 1, 1, timezone, 0, 0);
        var time = startZeroTime.AddSeconds(Time.realtimeSinceStartup);
        log.time = time;

        logs.Enqueue(log);
        while(logs.Count > MaxCount){
            logs.Dequeue();
        }
        Count = logs.Count;
        onRefresh?.Invoke(log);
    }
    
    void Update()
	{
        if (Input.GetKeyDown(KeyCode.BackQuote))
        {
            var visible = UIEngine.Stuff.IsFloatingExists<GameDashboardFloating>();
            if (!visible)
            {
                UIEngine.Stuff.ShowFloating<GameDashboardFloating>();
            }
            else
            {
                UIEngine.Stuff.RemoveFloating<GameDashboardFloating>();
            }
        }
    }
}

public struct GameUnityLog{
    public string condition;

    public string stack;

    public LogType type;

    public DateTime time;
}