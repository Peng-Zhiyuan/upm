
using System;
using System.Collections.Generic;
using BattleSystem.Core;
using UnityEngine;

public class State_Leave : CreatureState
{
    private Creature _target = null;

    private float DurTime = 2f;

    private Vector3 targetPos = Vector3.zero;

    private int EffectID = 0;
    
    public override int GetStateID()
    {
        return (int)State.STATE_LEAVE;
    }

    public override bool Condition(BaseState curState)
    {
        return active;
    }

    public override void OnStateChangeRequest(int newState, object param_UserData)
    {
        
        /*switch (newState)
        {
            case (int)State.STATE_MOVE:
                {
                    StateMachine.ChangeState((int)State.STATE_MOVE, param_UserData);
                }
                break;
        }*/
    }

    public override void OnEnter(object param_UserData)
    {
        base.OnEnter(param_UserData);
        //离场状态
        targetPos = RaycastUtil.GetGroundPosition((Vector3)(param_UserData));
        DurTime = 1.5f;
        if (_owner._cac != null)
        {
            _owner._cac.ChangeAction(CharacterActionConst.Run, true);
        }
        _owner.CreatureLookAt(targetPos);
        _owner.HideWeapon();
        BattleTimer.Instance.DelayCall(1f, delegate(object[] objects)
        {
            //if(_owner.RoleRender != null)
               // _owner.RoleRender.Dead();
            
            //EffectID = EffectManager.Instance.CreateBodyEffect("fx_dead_boss", _owner.GetBone("Bip001 Pelvis"), 10000,Vector3.zero, Vector3.one, Vector3.zero);
        });

        //_owner.RoleRender.SetDissolve(0f);

    }

    public override void OnLeave()
    {
        base.OnLeave();
        _owner.HideWeapon();
        //_owner.LookAt(targetPos);
        _owner.SetPosition(targetPos);
        
        EffectManager.Instance.RemoveEffect(EffectID);
        _owner.RoleRender.SetWeaponDissolve(1f, Color.yellow, 0.2f,false);
        _owner.gameObject.SetActive(false);
    }

    public override void Update(float param_deltaTime)
    {
        DurTime -= param_deltaTime;
        if(DurTime <= 0)
        {
            active = false;
            StateMachine.ChangeState((int)State.STATE_SUBSTITUTE, null);
            _owner.SetPosition(targetPos);
            
            return;
        }
        
        //_owner.RoleRender.SetDissolve(1 - DurTime / 1f);
        
        _owner.transform.position = Vector3.MoveTowards(_owner.transform.position, targetPos, 6f * Time.deltaTime);
    }
    
    public override void OnDestroy()
    {
    }
}