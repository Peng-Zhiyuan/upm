using System.Collections.Generic;
using System.Threading.Tasks;
using BattleEngine.Logic;
using Cinemachine;
using UnityEngine;

public class GuildCameraManager : Singleton<GuildCameraManager>
{
    private StateMachine m_stateMachine;

    public Camera MainCamera
    {
        get;
        set;
    }
    public Transform CameraFollow
    {
        get;
        set;
    }
    
    private List<Transform> RoleCVCameras = new List<Transform>();

    public GameObject GetRoleCamera(int index)
    {
        return RoleCVCameras[index].gameObject;
    }

    public void ShowRoleCamera(int index)
    {
        for (int i = 0; i < RoleCVCameras.Count; i++)
        {
            RoleCVCameras[i].SetActive(index == i);
        }
        
        SetCameraFadeMode(CinemachineBlendDefinition.Style.Cut);
        TurnImmediate();
    }

    public void HideRoleCamera()
    {
        foreach (var VARIABLE in RoleCVCameras)
        {
            VARIABLE.SetActive(false);
        }
    }

    public void OnInit()
    {
        MainCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
        var go = new GameObject("CameraFollow");
        go.transform.SetParent(MainCamera.transform.parent);
        CameraFollow = go.transform;
        CameraFollow.transform.position = new Vector3(9f, 0, 8f);
        m_stateMachine = new StateMachine();
        m_stateMachine.RegisterState(new GuildCameraState_Free(), this);
        m_stateMachine.RegisterState(new GuildCameraState_Spring(), this);
        m_stateMachine.RegisterState(new GuildCameraState_Front(), this);
        m_stateMachine.RegisterState(new GuildCameraState_None(), this);
        m_stateMachine.RegisterState(new GuildCameraState_SpringPort(), this);
        
        m_stateMachine.Start((int)GuildCameraState.Free, null);
        
        var root = GameObject.Find("PlayerRoot");
        if (root != null)
        {
            for (int i = 0; i < 6; i++)
            {
                var cm = root.transform.Find($"Pos{i + 1}/CMMainPos");
                if (cm != null)
                {
                    RoleCVCameras.Add(cm);
                }
            }
        }
    }

    public void SetCameraFadeMode(CinemachineBlendDefinition.Style style, float durTime = 0.5f)
    {
        MainCamera.GetComponent<CinemachineBrain>().m_DefaultBlend = new CinemachineBlendDefinition(style, durTime);
    }

    public override void Init()
    {
        
    }

    public void LateUpdate()
    {
        if(m_stateMachine == null)
            return;
        m_stateMachine.LateUpdate(Time.deltaTime);
        
        if(MainCamera != null && WwiseManagerEx.GetInstance() != null)
            WwiseManagerEx.GetInstance().UpdateListenerParam(MainCamera.transform.position, MainCamera.transform.rotation);
    }

    public void TryChangeState(GuildCameraState state, object obj)
    {
        m_stateMachine.TryChangeState((int)state, obj);
    }

    public void SetTarget(GuildPlayer player)
    {
        if (m_stateMachine.CurrentState.GetStateID() == (int) GuildCameraState.Front)
        {
            var state = m_stateMachine.CurrentState as GuildCameraState_Front;
            state.SetTarget(player);
        }
    }
    
    public void SetOffset(float offset)
    {
        if (m_stateMachine.CurrentState.GetStateID() == (int) GuildCameraState.Spring)
        {
            var state = m_stateMachine.CurrentState as GuildCameraState_Spring;
            state.SetOffsetY(offset);
        }
    }

    public void ResetFov()
    {
        if (m_stateMachine.CurrentState.GetStateID() == (int) GuildCameraState.Spring)
        {
            var state = m_stateMachine.CurrentState as GuildCameraState_Spring;
            state.ResetCameraFov();
        }
    }

    public void ResetDistance(float dis)
    {
        float v = 1f;
        float scale = (Screen.width *1f / Screen.height);
        float stand = 1920 / 1080f;
        if (scale >= stand)
        {
            v = scale / stand;
        }
        else
        {
            v = stand / scale;
        }

        CinemachineBrain brain = GuildCameraManager.Instance.MainCamera.GetComponent<CinemachineBrain>();
        if (brain != null && brain.ActiveVirtualCamera != null)
        {
            var _CVCamer = brain.ActiveVirtualCamera.VirtualCameraGameObject.GetComponent<CinemachineVirtualCamera>();
            var _Transposer = _CVCamer.GetCinemachineComponent<CinemachineTransposer>();
            _Transposer.m_FollowOffset.y = dis * v;
        }
    }

    public void TurnImmediate()
    {
        CinemachineBrain brain = GuildCameraManager.Instance.MainCamera.GetComponent<CinemachineBrain>();
        if (brain != null && brain.ActiveVirtualCamera != null)
        {
            var _CVCamer = brain.ActiveVirtualCamera.VirtualCameraGameObject.GetComponent<CinemachineVirtualCamera>();
            _CVCamer.PreviousStateIsValid = false;
        }
    }

    public void ChangeBlend()
    {
        CinemachineBrain brain = GuildCameraManager.Instance.MainCamera.GetComponent<CinemachineBrain>();
        if (brain != null)
        {
            brain.m_DefaultBlend = new CinemachineBlendDefinition(CinemachineBlendDefinition.Style.EaseInOut, 1f);;
        }
    }

    public override void Dispose()
    {
        if(WwiseManagerEx.GetInstance() != null)
            WwiseManagerEx.GetInstance().UpdateListenerParam(Vector3.zero, Quaternion.identity);
    }
    
}