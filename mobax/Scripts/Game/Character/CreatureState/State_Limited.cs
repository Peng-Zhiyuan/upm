
using System;
using System.Collections.Generic;
using UnityEngine;

public class State_Limited : CreatureState
{
    public override int GetStateID()
    {
        return (int)State.STATE_LIMITED;
    }

    public override bool Condition(BaseState curState)
    {
        //return _owner.IsLimited;
        return false;
    }

    public override void OnStateChangeRequest(int newState, object param_UserData)
    {
    }

    public override void OnEnter(object param_UserData)
    {
        base.OnEnter(param_UserData);
    }

    public override void OnLeave()
    {
        base.OnLeave();
    }

    public override void Update(float param_deltaTime)
    {
    }

    public override void OnDestroy()
    {
    }
}