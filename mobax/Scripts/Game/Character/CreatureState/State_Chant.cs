
using System;
using System.Collections.Generic;
using UnityEngine;

public class State_Chant : CreatureState
{
    private Creature _target = null;

    private float _AttackTime = 2f;
    public override int GetStateID()
    {
        return (int)State.STATE_CHANT;
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
        }
    }

    public void ResetAttackTime(float t)
    {
        _AttackTime = t;
    }

    public override void OnEnter(object param_UserData)
    {
        base.OnEnter(param_UserData);

        _AttackTime = (float)param_UserData;
    }

    public override void OnLeave()
    {
        base.OnLeave();

        //_owner.Target = null;
    }

    public override void Update(float param_deltaTime)
    {
        _AttackTime -= param_deltaTime;
        if(_AttackTime <= 0)
        {
            //StateMachine.ChangeState((int)State.STATE_IDLE, null);
            active = false;
        }
    }
    
    public override void OnDestroy()
    {
    }
}