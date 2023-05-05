using System;
using UnityEngine;

public abstract class CreatureState : BaseState
{
    protected Creature _owner;

    public override void OnInit(StateMachine param_StateMachine, object owner)
    {
        m_StateMachine = param_StateMachine;
        _owner = (Creature)owner;
    }

    public override void OnDestroy() { }

    public override bool Condition(BaseState curState)
    {
        return false;
    }

    public override void OnStateChangeRequest(int newState, object param_UserData) { }

    public override void OnEnter(object param_UserData) { }

    public override void OnLeave() { }

    public override void Update(float param_deltaTime) { }

    public override void FixedUpdate(float param_deltaTime) { }

    public override void LateUpdate(float param_deltaTime) { }
}