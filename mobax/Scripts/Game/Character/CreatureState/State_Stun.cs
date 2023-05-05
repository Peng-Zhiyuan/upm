using System;
using System.Collections.Generic;
using UnityEngine;

public class State_Stun : CreatureState
{
    private float _durTime = 1.6f;
    private int _effectID;

    public override int GetStateID()
    {
        return (int)State.STATE_STUN;
    }

    public override bool Condition(BaseState curState)
    {
        return _owner.LimitedTime > 0 && !_owner.mData.IsDead;
    }

    public override void OnStateChangeRequest(int newState, object param_UserData) { }

    public override void OnEnter(object param_UserData)
    {
        base.OnEnter(param_UserData);
        //Debug.LogError("进入眩晕状态");
        if (_owner._cac != null)
        {
            _owner._cac.ChangeAction(CharacterActionConst.Stun);
        }
        _effectID = EffectManager.Instance.CreateBodyEffect("fx_buff_coma", _owner.HeadBone, 20000f, new Vector3(0, -0.35f, 0), Vector3.one, Vector3.zero);
    }

    public override void OnLeave()
    {
        base.OnLeave();
        EffectManager.Instance.RemoveEffect(_effectID);
        //Debug.LogError("离开眩晕状态");
    }

    public override void Update(float param_deltaTime)
    {
        _owner.LimitedTime -= param_deltaTime;
        if (_owner.LimitedTime <= 0f)
        {
            active = false;
        }
    }

    public override void OnDestroy() { }
}