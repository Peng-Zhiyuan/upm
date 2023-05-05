using UnityEngine;
using System.Collections;

public enum eBattleState
{
    None,
    Ready,
    Play,
    Settlement,
    Move,
    Story,
    Explore,
}

public enum eBattleResult
{
    None = -1,    // 还没结束
    Victory = 1,    // 胜利
    Fail = 2, // 失败结算
}


public class BattleStateManager : BattleComponent<BattleStateManager>
{
    StateMachine _statemachine = new StateMachine();

    public bool GameOver => GameResult != eBattleResult.None;
    
    public eBattleResult GameResult
    {
        get;
        set;
    }

    public bool StoryPlay
    {
        get;
        set;
    }
    public override void OnBattleCreate()
    {
        this._statemachine = new StateMachine();
        _statemachine.RegisterState(new BattleState_Settlement(), this);
        _statemachine.RegisterState(new BattleState_Move(), this);
        _statemachine.RegisterState(new BattleState_Play(), this);
        _statemachine.RegisterState(new BattleState_Story(), this);
        _statemachine.RegisterState(new BattleState_Ready(), this);
        _statemachine.RegisterState(new BattleState_None(), this);
        _statemachine.RegisterState(new BattleState_Explore(), this);
        _statemachine.Start((int)eBattleState.None, this);
    }

    public void ChangeState(eBattleState param_Scene)
    {
        _statemachine.ChangeState((int)param_Scene, null);
    }

    public override void OnUpdate()
    {
        if(StoryPlay)
        
            return;
        var param_deltaTime = Time.deltaTime;
        _statemachine.Update(param_deltaTime);
    }

    public void FixedUpdate(float param_deltaTime)
    {
        _statemachine.FixedUpdate(param_deltaTime);
    }

    public override void LateUpdate()
    {
        float tmp_deltaTime = Time.deltaTime;
        _statemachine.LateUpdate(tmp_deltaTime);
    }

    public eBattleState GetCurrentState()
    {
        if (_statemachine.CurrentState != null)
        {
            return (eBattleState)(_statemachine.CurrentState.GetStateID());
        }

        return eBattleState.None;
    }

    public int GetCurrentStateID()
    {
        if (_statemachine.CurrentState != null)
        {
            return _statemachine.CurrentState.GetStateID();
        }

        return (int)eBattleState.None;
    }
}