using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CameraState
{
    Free = 0,
    Free2,
    Map,
    Skill,
    Timeline,
    Follow,
    SkillSelect,
    Ready,
    Front,
    FollowItem,
    Settlement,
    Fixed,
    FollowTest,
    Exchange,
    LookAt,
    Guard,
    Arena,
    FixLookAt,
    Defence,

    // 测试用
    Disabled = 98,
}

public abstract class BaseCameraState : BaseState
{
    protected virtual bool CanControl { get { return true; } }
    private Vector3 _dirTargetToCamera = Vector3.zero;
    private CameraManager _owner;
    public CameraManager Owner
    {
        get { return _owner; }
        set { _owner = value; }
    }
    
    public float HAngle
    {
        get;
        set;
    }
    
    public float VAngle
    {
        get;
        set;
    }
    public float Distance
    {
        get;
        set;
    }
    
    public float FOV
    {
        get;
        set;
    }
    
    public float XOffset
    {
        get;
        set;
    }
    
    public float YOffset
    {
        get;
        set;
    }
    
    public CameraProxy Proxy
    {
        get { return Owner.CameraProxy; }
    }
    private float _deltaTime;
    protected float DeltaTime
    {
        get { return _deltaTime; }
    }
    private float _time;
    protected float Time
    {
        get { return _time; }
    }
    public override void FixedUpdate(float param_deltaTime) { }
    public override void OnStateChangeRequest(int newState, object param_UserData)
    {
        m_StateMachine.ChangeState(newState, param_UserData);
    }
    public override void OnInit(StateMachine param_StateMachine, object owner)
    {
        m_StateMachine = param_StateMachine;
        _owner = (CameraManager)owner;
        _time = 0f;
    }
    protected abstract CameraState GetState();
    public sealed override int GetStateID()
    {
        return (int)GetState();
    }

    public sealed override void OnEnter(object param_UserData) { AddListener(); OnStateEnter(param_UserData); }
    public sealed override void OnLeave() { RemoveListener(); OnStateLeave(); }

    protected abstract void OnStateEnter(object param_UserData);
    protected abstract void OnStateLeave();

    public override void OnDestroy()
    {
        _owner = null;
    }

    protected virtual void AddListener()
    {
        if (CanControl)
        {
            //GameEventCenter.AddListener(GameEvent.OnGestureSwipe, this, OnSwipe);
            //GameEventCenter.AddListener(GameEvent.OnGesturePinch, this, OnPinch);
        }
    }
    protected virtual void RemoveListener()
    {
        if (CanControl)
        {
            //GameEventCenter.RemoveListener(GameEvent.OnGestureSwipe, this, OnSwipe);
            //GameEventCenter.RemoveListener(GameEvent.OnGesturePinch, this, OnPinch);
        }
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
        _deltaTime = param_deltaTime;
        _time += _deltaTime;
    }
    public override void LateUpdate(float param_deltaTime)
    {
        Proxy.LateUpdate(param_deltaTime);
    }
    public sealed override bool Condition(BaseState curState)
    {
        return active;
    }

    /*public sealed override Vector3 GetTargetPos()
    {
        return Proxy.Target.position;
    }*/
    

    protected void ExitState()
    {
        CameraManager.Instance.TryLeaveState(GetState());
    }
    public virtual void TryChangeSubState(int subState) { }

    public Vector3 GetDirTargetToCamera()
    {
        if (_dirTargetToCamera == Vector3.zero)
            CaculateDir();
        return _dirTargetToCamera;
    }

    public void CaculateDir()
    {
        _dirTargetToCamera = Vector3.zero;
        if (_owner.Target != null)
        {
            Vector3 cameraPos = CameraManager.Instance.ModelCamera.transform.position;
            cameraPos.y = 0f;
            _dirTargetToCamera = cameraPos - _owner.Target.position;
        }
    }
}