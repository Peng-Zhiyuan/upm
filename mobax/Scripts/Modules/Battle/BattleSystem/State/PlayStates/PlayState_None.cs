using System.Collections;
using System.Collections.Generic;
using BattleSystem.ProjectCore;
using UnityEngine;
using UnityEngine.Analytics;

public class PlayState_None : BattleStateBase
{
    public override int GetStateID()
    {
        return (int) ePlayState.None;
    }

    public override void OnDestroy()
    {
    }

    public bool Condition(BaseState curState)
    {
        return false;
    }

    public override void OnStateChangeRequest(int newState, object param_UserData)
    {
    }
    
    public override void OnEnter(object param_UserData)
    {
        Debug.LogWarning("进入战斗空状态");
    }
    

    public override void OnLeave()
    {
    }

    public override void Update(float param_deltaTime)
    {
    }
}