using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public enum GuildCameraState
{
    Free = 0,
    Map = 1,
    Spring = 2,
    Timeline,
    Follow,
    SkillSelect,
    Setting,
    Front,
    FollowItem,
    Settlement,
    Fixed,
    FollowTest,
    Exchange,
    Battle,
    SpringPort,

    // 测试用
    None = 98,
}

public abstract class GuildBaseCameraState : BaseState
{
    public GuildCameraManager Owner;
    public CinemachineVirtualCamera CVCamera;

    public sealed override void OnInit(StateMachine param_StateMachine, object owner)
    {
        m_StateMachine = param_StateMachine;
        Owner = (GuildCameraManager)owner;
        if (CVCamera == null)
        {
            GameObject go = GameObject.Find(CameraName());
            if (go == null)
            {
                //throw new Exception("没找到虚拟相机:" + CameraName());
                return;
            }
            CVCamera = go.GetComponent<CinemachineVirtualCamera>();
            CVCamera.gameObject.SetActive(false);
        }
    }
    protected abstract GuildCameraState GetState();
    protected abstract string CameraName();
    public sealed override int GetStateID()
    {
        return (int)GetState();
    }

    public sealed override void OnEnter(object param_UserData)
    {
        CVCamera.gameObject.SetActive(true);
        
        AddListener(); OnStateEnter(param_UserData);
    }
    public sealed override void OnLeave() { CVCamera.gameObject.SetActive(false);RemoveListener(); OnStateLeave(); }

    protected abstract void OnStateEnter(object param_UserData);
    protected abstract void OnStateLeave();

    public override void OnDestroy()
    {
        Owner = null;
    }

    protected virtual void AddListener()
    {
     
    }
    protected virtual void RemoveListener()
    {
    }
    private void OnSwipe(object[] param_Objects)
    {
        Vector2 tmp_offset = (Vector2)param_Objects[0];
        OnSwipe(tmp_offset);
    }
    protected virtual void OnSwipe(Vector2 offset) { }
    private void OnPinch(object[] param_Objects)
    {
        float tmp_pinch = (float)param_Objects[0];
        OnPinch(tmp_pinch);
    }
    protected virtual void OnPinch(float pinch) { }
    public sealed override void Update(float param_deltaTime)
    {
    }
    public override void LateUpdate(float param_deltaTime)
    {
    }
    public sealed override bool Condition(BaseState curState)
    {
        return active;
    }
    
    public sealed override void FixedUpdate(float param_deltaTime) { }

    /*public sealed override Vector3 GetTargetPos()
    {
        return Proxy.Target.position;
    }*/
    

    protected void ExitState()
    {
    }
    public virtual void TryChangeSubState(int subState) { }
    
    public void SyncPosImmediate()
    {
        if(CVCamera == null)
            return;
        CVCamera.PreviousStateIsValid = false;
    }
}