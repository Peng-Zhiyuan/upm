using System.Collections;
using System.Collections.Generic;
using BattleSystem.Core;
using UnityEngine;
using BattleSystem.ProjectCore;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine.SceneManagement;
using Task = System.Threading.Tasks.Task;

public enum ePlayState
{
    None,
    Ready,
    Play,
    //Story,
}

public class BattleState_Play : BattleStateBase
{
    StateMachine _playStateMachine = new StateMachine();
    
    public override int GetStateID()
    {
        return (int) eBattleState.Play;
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

    public override void OnInit(StateMachine param_StateMachine, object owner)
    {
        base.OnInit(param_StateMachine, owner);
        
        _playStateMachine.RegisterState(new PlayState_Ready(), this.Owner);
        _playStateMachine.RegisterState(new PlayState_Play(), this.Owner);
        _playStateMachine.RegisterState(new PlayState_None(), this.Owner);
        _playStateMachine.Start((int)ePlayState.None, this);
    }

    public override void OnEnter(object param_UserData)
    {
        _playStateMachine.ChangeState((int)ePlayState.Ready, null);
    }

    public override async void OnLeave()
    {
        _playStateMachine.ChangeState((int)ePlayState.None, null);
    }

    public override void Update(float param_deltaTime)
    {
        //Util.CallLuaFuntion("BattleLogic", "Update");
        
        _playStateMachine.Update(param_deltaTime);
    }

    private void BattleResult(bool win)
    {
        BattleStateManager.Instance.GameResult = win ? eBattleResult.Victory : eBattleResult.Fail;
        CameraManager.Instance.CameraProxy.OffsetSpeed = 0;
        //CameraManager.Instance.ShowTransition = true;
        BattleTimer.Instance.DelayCall(2, delegate(object[] objects)
        {
            CameraManager.Instance.TryChangeState(CameraState.Free2, null);
            //coreService.Gather();
            
            //this.Focus();
            BattleStateManager.Instance.ChangeState(eBattleState.Move);
        });
    }

}