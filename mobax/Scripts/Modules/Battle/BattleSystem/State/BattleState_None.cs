using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleState_None : BattleStateBase
{
    public override int GetStateID()
    {
        return (int)eBattleState.None;
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
        Debug.Log("进入非战斗状态");
    }

    public override void OnLeave()
    {
        Debug.Log("离开非战斗状态");
    }

    public override void Update(float param_deltaTime)
    {

    }
}
