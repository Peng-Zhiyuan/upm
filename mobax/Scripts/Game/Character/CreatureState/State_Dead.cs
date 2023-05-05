using System;
using System.Collections.Generic;
using UnityEngine;

public class State_Dead : CreatureState
{
    public override int GetStateID()
    {
        return (int)State.STATE_DEAD;
    }

    public override bool Condition(BaseState curState)
    {
        return _owner.mData.IsDead;
    }

    public override void OnStateChangeRequest(int newState, object param_UserData)
    {
        //switch (newState)
        //{
        //    case (int)State.STATE_MOVE:
        //        {
        //            StateMachine.ChangeState((int)State.STATE_MOVE, param_UserData);
        //        }
        //        break;
        //}
    }

    public override void OnEnter(object param_UserData)
    {
        base.OnEnter(param_UserData);
        int index = EffectManager.Instance.CreateBodyEffect("fx_dead_boss", _owner.GetBone("Bip001 Pelvis"), 10000, Vector3.zero, Vector3.one, Vector3.zero);
        float delay = 3;
        BattleTimer.Instance.DelayCall(delay, (param) =>
        {
            EffectManager.Instance.RemoveEffect(index);
            _owner.RemoveTag = true;
        });
        BattleTimer.Instance.DelayCall(1f, (param) =>
        {
            if (_owner.RoleRender != null)
                _owner.RoleRender.Dead();
        });

        // TODO: 这里可能右合并错误，懂的人看看
        _owner._cac.ChangeAction(CharacterActionConst.Dead);
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