using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TriggerController
{
    private ulong _uid;
    private List<TriggerTimeline> _triggerTimelines;
    private TriggerShareData _shareData;

    //控制器生命周期
    private float _lifeTime;
    private bool _isTriggerEnd;
    private System.Action _endCallback;

    public float LifeTime
    {
        get { return _lifeTime; }
    }
    public TriggerShareData ShareData
    {
        get { return _shareData; }
        set
        {
            _shareData = value;
            for (int i = 0; i < _triggerTimelines.Count; i++)
            {
                TriggerTimeline timeline = _triggerTimelines[i];
                timeline.SetController(this);
                timeline.shareData = _shareData;
            }
        }
    }

    public TriggerController(ulong param_ID, float lifeTime = 1f)
    {
        _uid = param_ID;
        _triggerTimelines = new List<TriggerTimeline>();
        _lifeTime = lifeTime;
        _isTriggerEnd = false;
    }

    public ulong GetUID
    {
        get { return _uid; }
    }

    public bool IsEnd
    {
        get { return _isTriggerEnd && _lifeTime <= 0f; }
    }
    public void SetEndCallback(System.Action endCallback)
    {
        _endCallback = endCallback;
    }

    public void EndCallback()
    {
        if (_endCallback != null)
            _endCallback();
    }

    public void Play()
    {
        for (int i = 0; i < _triggerTimelines.Count; i++)
        {
            TriggerTimeline timeline = _triggerTimelines[i];
            timeline.Play();
        }
    }

    public void Pause()
    {
        for (int i = 0; i < _triggerTimelines.Count; i++)
        {
            TriggerTimeline timeline = _triggerTimelines[i];
            timeline.Pause();
        }
    }

    public void Stop()
    {
        if (!IsEnd)
        {
            _lifeTime = 0f;

            for (int i = 0; i < _triggerTimelines.Count; i++)
            {
                TriggerTimeline timeline = _triggerTimelines[i];
                timeline.Stop();
            }
        }
    }
    public void FixedUpdate(float param_deltaTime)
    {
        int endCount = 0;
        for (int i = 0; i < _triggerTimelines.Count; i++)
        {
            TriggerTimeline timeline = _triggerTimelines[i];
            timeline.Update();

            if (timeline.IsEnd)
            {
                endCount++;
            }
        }

        if (endCount >= _triggerTimelines.Count)
        {
            _isTriggerEnd = true;
        }

        _lifeTime = Mathf.Max(_lifeTime - param_deltaTime, 0f);
    }

    public void AddTimeline(TriggerTimeline timeline)
    {
        if (timeline.LifeTime > _lifeTime)
        {
            _lifeTime = timeline.LifeTime;
        }
        _triggerTimelines.Add(timeline);
    }
    public void Destroy()
    {
        Stop();

        if (_triggerTimelines != null)
        {
            for (int i = 0; i < _triggerTimelines.Count; i++)
            {
                TriggerTimeline timeline = _triggerTimelines[i];
                timeline.Clear();
            }

            _triggerTimelines.Clear();
            _triggerTimelines = null;
        }
    }
}