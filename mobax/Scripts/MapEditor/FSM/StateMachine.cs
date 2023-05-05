using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class StateMachine
{
    private Dictionary<int, BaseState> m_States;
    private BaseState m_CurrentState;

    public StateMachine()
    {
        m_States = new Dictionary<int, BaseState>();
        m_CurrentState = null;
    }

    public virtual void Update(float param_deltaTime)
    {
        //GameProfilerSample.BeginSample("StateMachine Update");
        if (m_CurrentState == null)
            return;

        if (m_CurrentState != null)
        {
            //GameProfilerSample.BeginSample("StateMachine CurrentState Update : "+ ((State)m_CurrentState.GetStateID()).ToString());
            m_CurrentState.Update(param_deltaTime);
           // //GameProfilerSample.EndSample();
        }
        foreach (KeyValuePair<int, BaseState> kv in m_States)
        {
            if (kv.Value.Condition(m_CurrentState))
            {
                ChangeState(kv.Value.GetStateID(), null);
                //如果condition满足就直接返回
                return;
            }
        }
        //GameProfilerSample.EndSample();
    }

    public virtual void FixedUpdate(float param_deltaTime)
    {
        if (m_CurrentState != null)
        {
            m_CurrentState.FixedUpdate(param_deltaTime);
        }
    }

    public virtual void LateUpdate(float param_deltaTime)
    {
        if (m_CurrentState != null)
        {
            m_CurrentState.LateUpdate(param_deltaTime);
        }
    }

    public void Start(int param_StateID, object param_Data)
    {
        BaseState tmp_state = GetRegistedState(param_StateID);
        if (tmp_state == null)
        {
            return;
        }

        m_CurrentState = tmp_state;
        m_CurrentState.OnEnter(param_Data);
    }

    public void Close()
    {
        if (m_CurrentState != null)
        {
            m_CurrentState.OnLeave();
            m_CurrentState = null;
        }

        foreach (var tmp_pair in m_States)
        {
            tmp_pair.Value.OnDestroy();
        }
        m_States.Clear();
    }

	public void StateDestroy()
	{
		foreach (var tmp_pair in m_States)
		{
			tmp_pair.Value.OnDestroy();
		}
	}

    public bool RegisterState(BaseState param_State, object owner)
    {
        if (param_State == null)
        {
            //Debug.LogError("StateMachine add null state");
            return false;
        }

        if (IsRegistedState(param_State.GetStateID()))
        {
            Debug.LogError("StateMachine has this state already " + param_State.GetStateID());
            return false;
        }

        m_States[param_State.GetStateID()] = param_State;
        param_State.OnInit(this, owner);

        return true;
    }

    //尝试转换状态, 但是需要在OnStateChangeRequest
    public void TryChangeState(int param_StateID, object param_Data)
    {
        if (m_CurrentState == null)
        {
            return;
        }

        m_CurrentState.OnStateChangeRequest(param_StateID, param_Data);
    }

    //真正的状态切换
    public void ChangeState(int param_StateID, object param_Data)
    {
        BaseState tmp_state = GetRegistedState(param_StateID);
        if (tmp_state == null)
        {
            Debug.LogError("change state, but target state is not existed " + param_StateID);
            return;
        }

        if (tmp_state == m_CurrentState)
        {
            return;
        }

        if (m_CurrentState != null)
        {

            //GameProfilerSample.BeginSample("CurrentState Leave : "+ m_CurrentState.GetStateID().ToString());
            m_CurrentState.OnLeave(tmp_state.GetStateID());
            m_CurrentState.OnLeave();
            //GameProfilerSample.EndSample();
            m_CurrentState.active = false;

            m_CurrentState = tmp_state;

            m_CurrentState.active = true;
            //GameProfilerSample.BeginSample("CurrentState Enter : "+m_CurrentState.GetStateID().ToString());
            m_CurrentState.OnEnter(param_Data);
            //GameProfilerSample.EndSample();
        }
    }

	public void LeaveState(int param_StateID)
	{
		if(m_CurrentState != null && m_CurrentState.GetStateID() == param_StateID)
		{
			m_CurrentState.active = false;
		}
	}

    public bool IsRegistedState(int param_StateID)
    {
        return m_States.ContainsKey(param_StateID);
    }

    public bool IsInState(int param_StateID)
    {
        if (m_CurrentState == null)
        {
            return false;
        }
        return m_CurrentState.GetStateID() == param_StateID;
    }

    public BaseState GetRegistedState(int param_StateID)
    {
        BaseState tmp_result = null;
        m_States.TryGetValue(param_StateID, out tmp_result);
        return tmp_result;
    }

    public BaseState CurrentState
    {
        get { return m_CurrentState; }
    }

    public int StateCount
    {
        get { return m_States.Count; }
    }
}
