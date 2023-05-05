
using System;
using System.Collections.Generic;
using Cinemachine;
using DG.Tweening;
using DigitalRubyShared;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.UI;

public class CameraPosData
{
    public float hAngle;
    public float vAngle;
    public float distance;
    public float sideOffset;
    public float sideOffsetY;
    public float fov;
    public float smoothscale;
}

public class CameraManager : BattleComponent<CameraManager>
{
    //摄像机相关数据逻辑控制
    private CameraProxy _proxy = new CameraProxy();
    private CameraCtrl_Shake _shake = new CameraCtrl_Shake();

    //摄像机状态机 主要依靠外部事件切换状态
    private StateMachine m_stateMachine = new StateMachine();

    private CameraState m_cahceState = CameraState.Disabled;
    private object m_cacheData = null;
    private PhysicsRaycaster m_physicsRayCaster;
    private Camera m_mainCamera = null;
    private Camera m_rtCamera = null;
    private Camera m_subCamera = null;

    public AnimationCurve TransitionCurve
    {
        get;
        set;
    } = null;

    //摄像机位置同步控制
    public bool Move
    {
        get;
        set;
    } = true;

    public float ScaleThreshold = 0.15f;
    public ScaleGestureRecognizer ScaleGesture { get; private set; }

    private float m_moveSpeed = 2.0f;
    public float MoveSpeed
    {
        get { return m_moveSpeed; }
        set { m_moveSpeed = value; }
    }

    public Creature FocusTarget
    {
        get;
        set;
    }

    //替换主相机
    public void ReplaceMainCamera(Camera camera)
    {
        if (m_mainCamera != null)
        {
            m_mainCamera.SetActive(false);
        }

        m_mainCamera = camera;
    }


    public void UpdateTarget(SceneObject obj)
    {
        //Debug.LogError("SetTarget----------------");
        if (obj == null || m_Target == null)
        {
            _proxy.HOffsetAngle = 0f;
            return;
        }
        
        float y = Vector3.Cross(obj.transform.position - m_Target.position, m_Target.forward).y;
        _proxy.HAngleSign = Mathf.Sign(y);
        //_proxy.HOffsetAngle = 30f;
    }
    public Camera MainCamera
    {
        get
        {
            if (m_subCamera != null)
                return m_subCamera;
            return m_mainCamera;
        }
    }
    
    public CinemachineVirtualCamera CVCamera
    {
        get;
        set;
    }

    public bool ShowTransition
    {
        get;
        set;
    }

    public Camera RTCamera
    {
        get
        {
            return m_rtCamera;
        }
    }
    
    public Vector3 TimeLineCameraPos
    {
        get;
        set;
    }
    
    public Vector3 TimeLineCameraEnler
    {
        get;
        set;
    }
    public void SetCameraData(Vector3 pos, Vector3 eulerAngles)
    {
        TimeLineCameraPos = pos;
        TimeLineCameraEnler = eulerAngles;
    }
    
    public void SetTimeLineCameraData(Transform camera)
    {
        TimeLineCameraPos = camera.position;
        TimeLineCameraEnler = camera.eulerAngles;
        
        CVCamera.transform.position = TimeLineCameraPos;
        CVCamera.transform.eulerAngles = TimeLineCameraEnler;
        MainCamera.transform.position = TimeLineCameraPos;
        MainCamera.transform.eulerAngles = TimeLineCameraEnler;
        TryChangeState(CameraState.Fixed);
        CameraProxy.TurnImmediate();
    }

    public void ShowTransitionBlack()
    {
        TransitionEffectController com =
            CameraManager.Instance.MainCamera.GetComponentInChildren<TransitionEffectController>();
        if (com != null)
        {
            com.ShowEffect();
            _proxy.DelayTime = 0.2f;
        }
    }

    public Canvas BattleCanvas
    {
        get;
        set;
    }

    private Transform m_cameraTransform = null;
    public Transform CameraTransform
    {
        get
        {
            return m_cameraTransform;
        }
    }

    private Creature m_TargetRole = null;

    public Creature TargetRole
    {
        get { return m_TargetRole; }
    }
    
    public Transform ItemTarget
    {
        get;
        set;
    }

    public void SetItemTarget(Transform target, float time)
    {
        ItemTarget = target;
        
        float HOffsetAngle = CameraProxy.HOffsetAngle;
        float Dis = CameraProxy.Distance;
        float Fov = CameraProxy.FieldOfView;
            
        CameraProxy.HOffsetAngle = -5;
        CameraProxy.Distance = Dis * 0.5f;
        CameraProxy.FieldOfView = 95f;
        
        BattleTimer.Instance.DelayCall(time, delegate(object[] objects)
        {
            CameraManager.Instance.ItemTarget = null;
            CameraManager.Instance.CameraProxy.HOffsetAngle = HOffsetAngle;
            CameraManager.Instance.CameraProxy.Distance = Dis;
            CameraManager.Instance.CameraProxy.FieldOfView = Fov;
        });
    }

    public Transform GetTarget()
    {
        if (ItemTarget != null)
            return ItemTarget;

        if (TargetRole != null && TargetRole.Target != null)
        {
            Creature _targetRole = TargetRole.Target as Creature;
            if (!_targetRole.mData.IsDead)
            {
                // _targetRole.GetBone("body_hit");
                return null;
            }

            return null;
        }

        return null;
    }
    
    private Transform m_Target = null;
    public Transform Target
    {
        get
        {
            return m_Target;
        }
    }
    public bool Enable
    {
        get
        {
            if (m_mainCamera == null)
                return false;
            if (GetCurrentStateID().Equals((int)CameraState.Disabled))
                return false;
            return true;
        }
    }

    public Camera ModelCamera
    {
        get
        {
            return m_mainCamera;
        }
    }

    public GameObject FixCamera;

    public CameraCtrl_Shake ShakeCtrl
    {
        get { return _shake; }
    }

    public void SetTarget(Transform tmp, Creature role, bool immediate = false)
    {
        if (m_Target != null && !immediate)
        {
            _proxy.RegionPos = m_Target.position;
        }
        _proxy.RecoveHAngle();
        m_Target = tmp;
        _proxy.CaculateTransitionDis();
        //m_TargetRole = role;
    }
    
    public bool IsFree()
    {
        return GetCurrentStateID() == (int) CameraState.Free2;
    }

    public BlackTransition BlackTransition;
    public override void OnBattleCreate()
    {
        _proxy.Init(this);
        _shake.Init(this);
        RegisterState();

        m_rtCamera = GameObject.Find("char_throw").GetComponentInChildren<Camera>();
        CVCamera = GameObject.Find(("CMCamera")).GetComponent<CinemachineVirtualCamera>();
        FixCamera = GameObject.Find(("FixCamera")).gameObject;
        FixCamera.gameObject.SetActive(false);
        BattleCanvas = GameObject.Find("BattleCanvas").GetComponent<Canvas>();
        m_rtCamera.gameObject.SetActive(false);
        BlackTransition = GameObject.Find("BlackTransition").GetComponent<BlackTransition>();

        InitCamera();
    }

    public void ShowBlackTransition(Action action)
    {
        if (BlackTransition != null)
        {
            BlackTransition.ShowEffect(action);
        }
    }
    
    public void InitCamera()
    {
        GameObject go = GameObject.Find("BattleCamera");
        CameraInit(go);
        //WwiseManagerEx.GetInstance().SetListenerBindObj(go.transform);
    }

    public bool DefaultSet
    {
        get;
        set;
    }

    private void RegisterState()
    {
        //摄像机状态切换控制 无优先级概念
        m_stateMachine = new StateMachine();
        m_stateMachine.RegisterState(new CameraState_Settlement(), this);
        m_stateMachine.RegisterState(new CameraState_Front(), this);
        m_stateMachine.RegisterState(new CameraState_Free(), this);
        m_stateMachine.RegisterState(new CameraState_Free2(), this);
        m_stateMachine.RegisterState(new CameraState_Map(), this);
        m_stateMachine.RegisterState(new CameraState_Skill(), this);
        m_stateMachine.RegisterState(new CameraState_Disabled(), this);
        m_stateMachine.RegisterState(new CameraState_Timeline(), this);
        m_stateMachine.RegisterState(new CameraState_Follow(), this);
        m_stateMachine.RegisterState(new CameraState_SkillSelect(), this);
        m_stateMachine.RegisterState(new CameraState_Ready(), this);
        m_stateMachine.RegisterState(new CameraState_Fixed(), this);
        m_stateMachine.RegisterState(new CameraState_FollowTest(), this);
        m_stateMachine.RegisterState(new CameraState_Exchange(), this);
        m_stateMachine.RegisterState(new CameraState_LookAt(), this);
        m_stateMachine.RegisterState(new CameraState_Guard(), this);
        m_stateMachine.RegisterState(new CameraState_Defence(), this);
        m_stateMachine.RegisterState(new CameraState_Arena(), this);
        m_stateMachine.RegisterState(new CameraState_FixLookAt(), this);
    }

    public BaseCameraState GetState(CameraState state)
    {
        return m_stateMachine.GetRegistedState((int)state) as BaseCameraState;
    }

    public void ResetProxy()
    {
        var state = m_stateMachine.CurrentState as BaseCameraState;
        _proxy.m_VAngle = state.VAngle;
        _proxy.m_HAngle = state.HAngle;
        _proxy.Distance = state.Distance;
        _proxy.FieldOfView = state.FOV;
        _proxy.m_SideOffset = state.XOffset;
        _proxy.SideOffsetY = state.YOffset;
    }

    public void ResetHAngle()
    {
        _proxy.ResetHAngle();
    }

    public void TurnImmediate()
    {
        _proxy.TurnImmediate();
    }

    private void CameraInit(GameObject sceneCamera)
    {
        //GameObject.DontDestroyOnLoad(sceneCamera);
        sceneCamera.transform.localEulerAngles = Vector3.zero;
        sceneCamera.transform.localPosition = Vector3.zero;
        sceneCamera.transform.localScale = Vector3.one;
        m_cameraTransform = sceneCamera.transform;
        m_mainCamera = m_cameraTransform.GetComponent<Camera>();

        m_stateMachine.Start((int)m_cahceState, m_cacheData);

        //TransitionCurve = sceneCamera.GetComponent<DebugCamera>().Curve;

        //TryChangeState(CameraState.Disabled);
    }

    /// <summary>
    /// 显示主相机
    /// </summary>
    public void ShowCamera()
    {
        if(MainCamera == null)
            return;
        
        MainCamera.SetActive(true);
    }

    public void SetMainCamera(Transform cam)
    {
        if (cam == null)
        {
            m_subCamera = null;
            return;
        }
        
        var camera = cam.GetComponentInChildren<Camera>();
        if (camera != null)
        {
            m_subCamera = camera;
        }
    }
    
    /// <summary>
    /// 关闭主相机
    /// </summary>
    public void HideCamera()
    {
        if(MainCamera == null)
            return;
        
        MainCamera.SetActive(false);
    }

    public bool UpdateEnable
    {
        get;
        set;
    } = true;

    public void Update(float param_deltaTime)
    {
        if(UpdateEnable)
            m_stateMachine.Update(param_deltaTime);
    }
    
    public override void LateUpdate()
    {
        if(m_mainCamera == null)
            return;
        
        float param_deltaTime = Time.deltaTime;
        if (!UpdateEnable)
            return;
        
        //目标逻辑后续移除
        if (!Target)
        {
            if (SceneObjectManager.Instance.LocalPlayerCamera != null)
                m_Target = SceneObjectManager.Instance.LocalPlayerCamera.transform;
        }

        if (SceneObjectManager.Instance.LocalPlayerCamera != null)
        {
            //if(SceneObjectManager.Instance.LocalPlayer.fol)
        }
        if (Target)
        {
            //摄像机状态机
            if(Move)
                m_stateMachine.LateUpdate(param_deltaTime);
            _shake.Update(param_deltaTime);
        }
        
        if(MainCamera != null && WwiseManagerEx.GetInstance() != null)
            WwiseManagerEx.GetInstance().UpdateListenerParam(MainCamera.transform.position, MainCamera.transform.rotation);
    }
    public void FixedUpdate(float param_deltaTime) 
    { 
        m_stateMachine.FixedUpdate(param_deltaTime);
    }

    public void StartShake(float indesty, float durTime)
    {
        _shake.StartShake(indesty, durTime);
    }

    //尝试改变摄像机状态
    public void TryChangeState(CameraState state, object param_Data = null)
    {
        if(Battle.Instance.IsArenaMode && state == CameraState.Skill)
            return;
        
        m_stateMachine.TryChangeState((int)state, param_Data);
        //Debug.LogError("----state = " + state);
        /*if (m_stateMachine.CurrentState != null)
        {
            m_stateMachine.TryChangeState((int)state, param_Data);
        }
        else
        {
            m_cahceState = state;
            m_cacheData = param_Data;
        }*/
    }
    //尝试退出摄像机状态
    public void TryLeaveState(CameraState state)
    {
        if (!state.Equals((CameraState)GetCurrentStateID()))
            return;
        TryChangeState(CameraState.Free2);
    }
    //尝试改变摄像机子状态
    public void TryChangeSubState(int subState)
    {
        if (m_stateMachine.CurrentState != null)
        {
            BaseCameraState currentState = (BaseCameraState)m_stateMachine.CurrentState;
            currentState.TryChangeSubState(subState);
        }
    }

    public int GetCurrentStateID()
    {
        if (m_stateMachine.CurrentState != null)
        {
            return m_stateMachine.CurrentState.GetStateID();
        }
        return (int)m_cahceState;
    }

    public override void OnDestroy()
    {
        m_stateMachine.Close();
        if (_proxy != null)
        {
            _proxy.Dispose();
            _proxy = null;
        }
        
        //WwiseManagerEx.GetInstance().SetListenerBindObj();
        if(WwiseManagerEx.GetInstance() != null)
            WwiseManagerEx.GetInstance().UpdateListenerParam(Vector3.zero, Quaternion.identity);
    }

    
    public void CameraDelay(Vector3 destPos, float speed, float delayTime, float distance = 0f)
    {
        _proxy.DelaySync(destPos, speed, delayTime, distance);
    }

    #region 外部常用接口
    public CameraProxy CameraProxy
    {
        get { return _proxy; }
    }

    public float CameraFarClipPlane
    {
        get
        {
            if (MainCamera != null)
            {
                return MainCamera.farClipPlane;
            }
            return 0f;
        }
        set
        {
            if (MainCamera != null)
            {
                MainCamera.farClipPlane = value;
            }
        }
    }

    //获取当前视距
    public float GetCurrentDistance()
    {
        return _proxy.CurrentDistance;
    }

    public void ResetDistance()
    {
        _proxy.ResetCurrentDistance();
    }

    public void ResetOffset()
    {
        _proxy.CleanOffset();
    }

    public void EnableRenderFeature(string featureName, bool enabled = true)
    {
        var feature = RenderFeatureHandler.Ins.GetRenderFeatureByName(featureName);
        if (null != feature)
        {
            feature.SetActive(enabled);
        }
    }

    public void BlurTo(float blurEnd, float duration = 0)
    {
        var blurFeature = RenderFeatureHandler.Ins.GetRenderFeatureByName("NewGaussianBlurRenderPassFeature") as GaussianBlurRenderPassFeature;
        if (null == blurFeature) return;

        if (blurEnd > 0 && !blurFeature.isActive)
        {
            blurFeature.SetActive(true);
        }

        if (duration > 0)
        {
            var currentBlur = blurFeature.blurSettings.intensity;
            DOTween.To(() => currentBlur, x => currentBlur = x, blurEnd, duration)
                .SetEase(Ease.OutCubic).OnUpdate(() =>
                {
                    blurFeature.blurSettings.intensity = currentBlur;
                    // EditorUtility.SetDirty(blurFeature);
                }).OnComplete(() =>
                {
                    if (blurEnd <= 0)
                    {
                        blurFeature.SetActive(false);
                    }
                });
        }
        else
        {
            // 如果等于0， 就直接赋值
            blurFeature.blurSettings.intensity = blurEnd;
            // EditorUtility.SetDirty(blurFeature);
        }

    }

    //获取开启的摄像机坐标
    public Vector3 GetActiveCameraPos()
    {
        if (MainCamera != null)
            return MainCamera.transform.position;
        return Vector3.zero;
    }
    //摄像机坐标方向
    public Vector3 GetCameraforward()
    {
        if (MainCamera != null)
        {
            return MainCamera.transform.forward;
        }
        return Vector3.zero;
    }
    //同步摄像机位置
    public void SyncCameraPosition(Vector3 pos, float syncTime = 0f)
    {
        _proxy.SyncPosition(pos, syncTime);
    }

    public void SyncCameraTransfrom(Vector3 pos, Vector3 rotation)
    {
        CameraTransform.position = pos;
        CameraTransform.rotation = Quaternion.Euler(rotation);
        MainCamera.transform.localPosition = Vector3.zero;
        MainCamera.transform.localRotation = Quaternion.identity;
    }

    public void SetCameraEnable(bool enable)
    {
        m_mainCamera.enabled = enable;
    }

    public void SetCameraFieldOfView(float fValue)
    {
        if (_proxy != null)
        {
            _proxy.SetCameraFieldOfView(fValue);
        }
    }

    public float GetCameraFieldOfView()
    {
        if (_proxy != null)
        {
            return _proxy.GetCameraFieldOfView();
        }

        return 45f;
    }

    public void SetDefaultCameraFieldOfView()
    {
        if (_proxy != null)
        {
            _proxy.SetDefaultCameraFieldOfView();
        }
    }


    public void BroadcastChangeSubState(int state)
    {

    }

    public Vector3 GetDir()
    {
        if (m_stateMachine.CurrentState != null)
        {
            var state = (BaseCameraState)m_stateMachine.CurrentState;
            return state.GetDirTargetToCamera();
        }

        return Vector3.zero;
    }

    private static Vector3 s_vTemp;
    public static Vector3 ScreenToWorldPoint(Camera cam, Vector3 screenPos)
    {
        if (cam != null)
        {
            s_vTemp = cam.ScreenToWorldPoint(screenPos);
        }
        else
        {
            s_vTemp = Vector3.zero;
        }

        return s_vTemp;
    }

    public static Vector3 WorldToScreenPoint(Camera cam, Vector3 worldPos)
    {
        if (cam != null)
        {
            s_vTemp = cam.WorldToScreenPoint(worldPos);
        }
        else
        {
            s_vTemp = Vector3.zero;
        }

        return s_vTemp;
    }   
    #endregion

    public void SwitchFOllow(Creature role)
    {
        var player = SceneObjectManager.Instance.GetSelectPlayer();
        if (player != null)
        {
            if (player == role)
                return;
            player.OnUnSelected();
        }
        CameraProxy.SetTarget(role.transform, role);
        SceneObjectManager.Instance.SetSelectPlayer(role);
    }
}