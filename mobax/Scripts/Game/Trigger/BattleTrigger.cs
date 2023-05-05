using UnityEngine;
using System.Collections;
using BattleSystem.Core;
using Game.Skill;

public abstract class BattleTrigger {
    public bool _isTriggered = false;

    public EventConfig config = null;

    //是否完成
    private bool m_isEnd = false;

    public bool IsEnd {
        get { return m_isEnd; }
    }

    //是否触发
    public bool IsTriggered {
        get { return _isTriggered; }
    }

    //trigger所属timeline
    private TriggerTimeline _timeline;

    public TriggerTimeline TimeLine {
        set { _timeline = value; }
    }

    protected TriggerShareData ShareData {
        get {
            if (_timeline == null)
                _timeline = new TriggerTimeline();
            return _timeline.shareData;
        }
    }

    private void OnTriggerOver() {
        m_isEnd = true;
    }

    public void DoTrigger() {
        _isTriggered = true;

        bool isIgnore = false;
        if (config != null && config.IsOwner) {
            if (ShareData != null && ShareData.sceneObj != null) {
                var role = ShareData.sceneObj as Creature;
                if (role != null) {
                    isIgnore = !role.IsSelf();
                }
            }
        }

        if (config != null && config.IsOther) {
            if (ShareData != null && ShareData.sceneObj != null) {
                var role = ShareData.sceneObj as Creature;
                if (role != null) {
                    isIgnore = role.IsSelf();
                }
            }
        }

        if (!isIgnore && config.Enabled)
            OnTrigger();

        OnTriggerOver();
    }

    virtual public void OnTriggerEnd() { }
    abstract protected void OnTrigger();

    virtual public void Update(float delTime) { }

    //public ShareData LogicData { get; set; }
    public float DurTime { get; set; }

    public float TriggerTime { get; set; }
}