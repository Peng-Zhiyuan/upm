using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TriggerTimeline
{
    public TriggerShareData shareData;

    private List<BattleTrigger> _triggers = new List<BattleTrigger>();
    private bool _playing = false;
    private bool _end = false;
    private float _time;
    private int _completeCount;
    private float _lifeTime = 0f;
    public float LifeTime
    {
        set { _lifeTime = value; }
        get { return _lifeTime; }
    }

    //TriggerTimeline 所属的控制器
    private TriggerController _triggerController;
    public void SetController(TriggerController controller)
    {
        _triggerController = controller;
    }

    public TriggerTimeline()
    {
        shareData = new TriggerShareData();
        _triggers = new List<BattleTrigger>();
    }
    public bool IsEnd
    {
        get { return _end; }
    }

    public void AddTrigger(BattleTrigger trigger)
    {
        trigger.TimeLine = this;
        _triggers.Add(trigger);
    }


    public void Play()
    {
        _playing = true;
        _end = false;
    }

    public void Pause()
    {
        _playing = false;
    }

    public void Stop()
    {
        _end = true;
        Reset();
    }

    private void Reset()
    {
        _playing = false;
        _time = 0;
        _completeCount = 0;
    }

    public bool Playing
    {
        get { return _playing; }
    }

    public void Update()
    {
        //if (_playing)
        {
            _time += Time.deltaTime * 1;
            BattleTrigger trigger = null;
            bool isEnd = true;
            for (int i = 0; i < _triggers.Count; i++)
            {
                trigger = _triggers[i];
                trigger.Update(Time.deltaTime);
                if (!trigger.IsTriggered && _time >= trigger.config.FloatTriggerTime)
                {
                    trigger.DoTrigger();
                    _completeCount++;
                }
                if (!trigger.IsEnd)
                    isEnd = false;
            }
            if (_completeCount >= _triggers.Count && isEnd)
                Stop();
        }
    }


    public void Clear()
    {
        //shareData.RemoveEffects();

        _triggerController = null;
        if(shareData != null)
            shareData.Uninitialize();
        shareData = null;
        _triggers = null;

    }

}