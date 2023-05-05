using System;
using System.Collections.Generic;
using UnityEngine;

public class State_Hurt : CreatureState
{
    private float _durTime = 1.6f;

    public override int GetStateID()
    {
        return (int)State.STATE_HURT;
    }

    public override bool Condition(BaseState curState)
    {
        return active && !_owner.mData.IsDead;
    }

    public override void OnStateChangeRequest(int newState, object param_UserData)
    {
        switch (newState)
        {
            case (int)State.STATE_HURT:
            {
                _durTime = 1.6f;
                if (_owner._cac != null)
                {
                    _owner._cac.ChangeAction(CharacterActionConst.Hurt);
                }
            }
                break;
            case (int)State.STATE_ATTACK:
            {
                StateMachine.ChangeState((int)State.STATE_ATTACK, param_UserData);
            }
                break;
        }
    }

    public override void OnEnter(object param_UserData)
    {
        base.OnEnter(param_UserData);
        //Debug.LogError("进入受创状态");
        _durTime = 1.6f;
        if (_owner._cac != null)
        {
            _owner._cac.ChangeAction(CharacterActionConst.Hurt);
        }
    }

    public override void OnLeave()
    {
        base.OnLeave();
        //Debug.LogError("离开受创状态");
    }

    public override void Update(float param_deltaTime)
    {
        _durTime -= param_deltaTime;
        if (_durTime <= 0f)
        {
            active = false;
        }
    }

    public override void OnDestroy() { }
}