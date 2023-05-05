using System;
using System.Collections.Generic;
using UnityEngine;

public class State_Idle : CreatureState
{
    public override int GetStateID()
    {
        return (int)State.STATE_IDLE;
    }

    public override bool Condition(BaseState curState)
    {
        return true;
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
        if (_owner._cac != null)
        {
            if (!_owner.WeaponState)
            {
                _owner._cac.ChangeAction(CharacterActionConst.Stand, true);
                //_owner.HideWeapon();
            }
            else
            {
                _owner._cac.ChangeAction(CharacterActionConst.Idle, true);
                _owner.ShowWeapon();
            }
        }
    }

    public override void OnLeave()
    {
        base.OnLeave();
    }

    public override void Update(float param_deltaTime)
    {
        //GameProfilerSample.BeginSample("State_Idle Update");
        //base.Update(param_deltaTime);

        //GameProfilerSample.EndSample();
    }

    public override void OnDestroy() { }
}