using System;
using System.Collections.Generic;
using UnityEngine;

public class State_FightIdle : CreatureState
{
    private float _DurTime = 1.333f;

    public override int GetStateID()
    {
        return (int)State.STATE_FightIdle;
    }

    public override bool Condition(BaseState curState)
    {
        return active;
    }

    public override void OnStateChangeRequest(int newState, object param_UserData)
    {
        switch (newState)
        {
            case (int)State.STATE_MOVE:
            {
                StateMachine.ChangeState((int)State.STATE_MOVE, param_UserData);
            }
                break;
            case (int)State.STATE_ATTACK:
            {
                StateMachine.ChangeState((int)State.STATE_ATTACK, param_UserData);
            }
                break;
            case (int)State.STATE_HURT:
            {
                StateMachine.ChangeState((int)State.STATE_HURT, param_UserData);
            }
                break;
        }
    }

    public override void OnEnter(object param_UserData)
    {
        base.OnEnter(param_UserData);
        _DurTime = 3f;
        if (_owner._cac != null)
        {
            _owner._cac.ChangeAction(CharacterActionConst.FightIdle, true);
        }
    }

    public override void OnLeave()
    {
        base.OnLeave();
    }

    public override void Update(float param_deltaTime)
    {
        _DurTime -= param_deltaTime;
        if (_DurTime <= 0)
        {
            active = false;
            StateMachine.ChangeState((int)State.STATE_IDLE, null);
        }
    }

    public override void OnDestroy() { }
}