
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BattleSystem.Core;
using DG.Tweening;
using UnityEngine;

public class State_Join : CreatureState
{
    private Creature _target = null;

    private float DurTime = 2f;

    private Vector3 targetPos = Vector3.zero;

    private int effectID;
    
    public override int GetStateID()
    {
        return (int)State.STATE_JOIN;
    }

    public override bool Condition(BaseState curState)
    {
        return active;
    }

    public override void OnStateChangeRequest(int newState, object param_UserData)
    {
        
        switch (newState)
        {
            case (int)State.STATE_ATTACK:
                {
                    StateMachine.ChangeState((int)State.STATE_ATTACK, param_UserData);
                    active = false;
                }
                break;
        }
    }

    public override void OnEnter(object param_UserData)
    {
        base.OnEnter(param_UserData);
        targetPos = (Vector3)(param_UserData);
        DurTime = 1.21f;
        _owner.gameObject.SetActive(true);

        JoinFlow();
        /*if (_owner._cac != null)
        {
            _owner._cac.ChangeAction("assist1", true);
        }
        _owner.ShowWeapon();
        _owner.RoleRender.SetDissolve(0f);
        Transform bone = _owner.GetBone("Bip001");
        effectID = EffectManager.Instance.CreateBodyEffect("fx_assist1", bone, 10000,Vector3.zero, Vector3.one, new Vector3(0, -90f, -90f));

        BattleTimer.Instance.DelayCall(0.3f, delegate(object[] objects)
        {
            _owner._cac.ChangeAction("atk1 0", true);
            //EffectManager.Instance.RemoveEffect(effectID);
            //effectID = EffectManager.Instance.CreateBodyEffect("FX_Map_Teleportout_New", bone, 10000,Vector3.zero, Vector3.one, Vector3.zero);
        });


        //AudioEngine.Stuff.AquireClipIfNeedThenPlaySeAsync("sound_ruchang", 1, AudioEngine.Parameter.SingleTaskEarly);
        AudioManager.PlaySeInBackground("sound_ruchang");

        _owner.RoleRender.SetFresnel(true, Color.yellow);
       
        
        //_owner.SetPosition(targetPos);
        
        /*TimerManager.Instance.DelayCall(1.5f, delegate(object[] objects)
        {
            _owner.SetPosition(targetPos);
            ClientEngine.Instance.SendCommand(CmdType.UseSkill, _owner.ID, _owner.RoleItemData.SkillData.AssistSkill.ID);
        });#1#
        
        var tween = _owner.transform.DOLocalMove(targetPos, 0.3f).SetEase(Ease.Linear);*/
    }

    private async void JoinFlow()
    {
        _owner.RoleRender.SetWeaponDissolve(0f, Color.yellow, 0.2f, false);
        await Task.Delay(1000);
        //底层状态切换
        //播放开场技能
        
        if (_owner._cac != null)
        {
            _owner._cac.ChangeAction("support1", true);
        }
        _owner.ShowWeapon();
        
        Transform bone = _owner.GetBone("Bip001");
        effectID = EffectManager.Instance.CreateBodyEffect("fx_assist1", bone, 10000,Vector3.zero, Vector3.one, new Vector3(0, -90f, -90f));
        Vector3 nextpos = _owner.GetPosition() + (targetPos - _owner.GetPosition()).normalized * 1f;
        
        var tween = _owner.transform.DOLocalMove(nextpos, 0.2f).SetEase(Ease.Linear);
        //AudioManager.PlaySeInBackground("sound_ruchang");

        _owner.RoleRender.SetFresnel(true, Color.yellow);
        //await Task.Delay(267);
        var tween2 = _owner.transform.DOLocalMove(targetPos, 0.267f).SetEase(Ease.Linear);
        _owner._cac.ChangeAction("support1", true);
        await Task.Delay(267);
        //_owner._cac.ChangeAction("support2", true);
        
        active = false;
        
    }

    public override void OnLeave()
    {
        base.OnLeave();
        
        EffectManager.Instance.RemoveEffect(effectID);

        _owner.RoleRender.SetFresnel(false, Color.yellow);
        TimerMgr.Instance.BattleSchedulerTimer(0.01f, delegate
        {
            _owner.ShowSelectEffect(_owner.IsShowSelected);
        });
        
    }

    public override void Update(float param_deltaTime)
    {
        DurTime -= param_deltaTime;
        if(DurTime <= 0)
        {
            //active = false;
            //StateMachine.ChangeState((int)State.STATE_IDLE, null);
            //_owner.SetPosition(targetPos);
            //ClientEngine.Instance.SendCommand(CmdType.UseSkill, _owner.ID, _owner.RoleItemData.SkillData.AssistSkill.ID);
        }
    }
    
    public override void OnDestroy()
    {
    }
}