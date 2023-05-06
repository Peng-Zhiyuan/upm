﻿using System;
using System.Collections.Generic;
using BattleSystem.Core;
using UnityEngine;

public class State_Victory : CreatureState
{
    private Creature _target = null;

    private float DurTime = 2f;
    

    public override int GetStateID()
    {
        return (int)State.STATE_VICTORY;
    }

    public override bool Condition(BaseState curState)
    {
        return active;
    }

    public override void OnStateChangeRequest(int newState, object param_UserData)
    {
        switch (newState)
        {
            case (int)State.STATE_MOVE:
            {
                StateMachine.ChangeState((int)State.STATE_MOVE, param_UserData);
            }
                break;
        }
    }

    public override void OnEnter(object param_UserData)
    {
        base.OnEnter(param_UserData);
        DurTime = (float)param_UserData;
        if (_owner._cac != null)
        {
            _owner._cac.ChangeAction(CharacterActionConst.Win, true);
            Transform head = _owner.GetBone("Bip001 Head");
            Animator anim = head.GetComponentInChildren<Animator>();
            if (anim != null)
            {
                anim.Play("face_win2");
            }
        }
        _owner.HideWeapon();
    }

    public override void OnLeave()
    {
        base.OnLeave();
    }

    public override void Update(float param_deltaTime)
    {
        /*DurTime -= param_deltaTime;
        if(DurTime <= 0)
        {
            //StateMachine.ChangeState((int)State.STATE_IDLE, null);
            active = false;
        }*/
    }

    public override void OnDestroy() { }
}