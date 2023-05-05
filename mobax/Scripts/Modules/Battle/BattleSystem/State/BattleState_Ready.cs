using System.Collections;
using System.Collections.Generic;
using BattleSystem.ProjectCore;
using UnityEngine;
using UnityEngine.Analytics;

public class BattleState_Ready : BattleStateBase
{
    public override int GetStateID()
    {
        return (int)eBattleState.Ready;
    }

    public override void OnDestroy() { }

    public bool Condition(BaseState curState)
    {
        return false;
    }

    public override void OnStateChangeRequest(int newState, object param_UserData) { }

    public override async void OnEnter(object param_UserData)
    {
        Debug.LogWarning("Enter ready state!");
        Owner.GameResult = eBattleResult.None;
        BattleStateManager.Instance.ChangeState(eBattleState.Story);
    }

    public override void OnLeave() { }

    public override void Update(float param_deltaTime) { }
}