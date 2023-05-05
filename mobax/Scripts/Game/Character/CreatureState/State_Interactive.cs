using UnityEngine;

public class State_Interactive : CreatureState
{
    public Creature _target = null;

    private float _interactiveTime = 5f;
    private float _remTime = 0f;

    public override int GetStateID()
    {
        return (int)State.STATE_INTERACTIVE;
    }

    public override bool Condition(BaseState curState)
    {
        return active;
    }

    public override void OnStateChangeRequest(int newState, object param_UserData)
    {
        switch (newState)
        {
            //case (int)State.STATE_SUPERJUMP:
            //    {
            //        StateMachine.ChangeState((int)State.STATE_SUPERJUMP, param_UserData);
            //    }
            //    break;
        }
    }

    public override void OnEnter(object param_UserData)
    {
        base.OnEnter(param_UserData);
        Debug.LogError("进入交互状态");
        _owner._cac.ChangeAction("Idle");
        _interactiveTime = 3f;
        _remTime = _interactiveTime;
    }

    private void TimeCall(object[] param)
    {
        Debug.LogError("交互中");
    }

    public override void OnLeave()
    {
        Debug.LogError("交互结束");
        //_owner.UpdateHPMP(10000, 0, _owner.mData.CurrentHealth.MaxValue, 0);
        base.OnLeave();
    }

    public override void Update(float param_deltaTime)
    {
        base.Update(param_deltaTime);
        _remTime -= param_deltaTime;
        if (_remTime <= 0)
        {
            active = false;
        }
    }
}