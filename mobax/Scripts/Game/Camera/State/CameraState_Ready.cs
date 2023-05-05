using System.Collections;
using System.Collections.Generic;
using BattleSystem.Core;
using behaviac;
using Cinemachine;
using UnityEngine;

using UnityEngine.Rendering.Universal;

public class CameraState_Ready : BaseCameraState
{
    public GameObject ReadyCamera = null;
    protected override CameraState GetState()
    {
        return CameraState.Ready;
    }

    //private float m_HAngle = 0f;
    private float m_VAngle = 25f;
    private float m_Distance = 9.9f;
    private float m_FOV = 85f;
    
    public override void OnInit(StateMachine param_StateMachine, object owner)
    {
        base.OnInit(param_StateMachine, owner);
        ReadyCamera = GameObject.Find("CMCamera_Ready");
        if(ReadyCamera != null)
            ReadyCamera.SetActive(false);
    }
   
    protected override void OnStateEnter(object param_UserData)
    {
        //ReadyCamera.SetActive(true);
        CameraManager.Instance.CameraProxy.CleanOffset();
        CameraSetting.Ins.SetBindingMode(CinemachineTransposer.BindingMode.LockToTarget);
        /*if (Battle.Instance.IsArenaMode)
        {
            Proxy.m_HAngle = 30f;
        }
        else*/
        {
            Proxy.m_HAngle = 0;
        }
        
        Proxy.m_VAngle = 0;
        Proxy.Distance = this.m_Distance;
        Proxy.m_SideOffset = 0f;
        Proxy.FieldOfView = this.m_FOV;
        
        Proxy.HAngleOffset = 0f;
        Proxy.m_CurrentHAngle = 0f;
        Proxy.m_DistanceOffset = 0f;
        //if(Battle.Instance.IsArenaMode)
        Proxy.Distance = 10;
        Proxy.SideOffsetY = 4.35f;
        var pos = (Vector3) param_UserData;
        pos = pos + SceneObjectManager.Instance.LocalPlayerCamera.GetDirection().normalized * 4f;
        if(Battle.Instance.IsArenaMode)
            SceneObjectManager.Instance.LocalPlayerCamera.transform.position = Vector3.zero;
        else
        {
            SceneObjectManager.Instance.LocalPlayerCamera.transform.position = pos;
        }
        
        CameraSetting.Ins.SetFollow(SceneObjectManager.Instance.LocalPlayerCamera.transform);
        CameraSetting.Ins.LookAt(SceneObjectManager.Instance.LocalPlayerCamera.transform);

        //CameraManager.Instance.CameraProxy.CleanOffset();
        if(CameraManager.Instance.CameraProxy.ChangeImmedate)
            CameraManager.Instance.CameraProxy.TurnImmediate();
        else
        {
            CameraManager.Instance.CameraProxy.ChangeImmedate = true;
        }
        //CameraManager.Instance.Move = false;

        //CaculateDir();
    }
    protected override void OnStateLeave()
    {
        //ReadyCamera.SetActive(false);
        CameraManager.Instance.Move = true;
        //GameEventCenter.RemoveListener(GameEvent.PlayerModeChanged, this, this.PlayerModeChanged);
    }

    public void FollowSelectHero()
    {
        Creature follow = null;
        Creature look = null;
        if (CameraManager.Instance.FocusTarget != null)
        {
            if (CameraManager.Instance.FocusTarget.mData.IsDead)
            {
                CameraManager.Instance.FocusTarget = null;
                look = SceneObjectManager.Instance.GetSelectPlayer();
            }
            else
                look = CameraManager.Instance.FocusTarget;
        }
        else
        {
            look = SceneObjectManager.Instance.GetSelectPlayer();
        }

        if (look != null)
        {
            CameraSetting.Ins.LookAt(look.CameraPoint);
        }
        
        follow = SceneObjectManager.Instance.GetSelectPlayer();
        if (follow != null)
        {
            CameraSetting.Ins.SetFollow(follow.GetBone("body_hit"));
            CameraSetting.Ins.LookAt(follow.GetBone("body_hit"));
        }
        else
        {
            
        }
    }

    public override void LateUpdate(float param_deltaTime)
    {
        //FollowSelectHero();
        Proxy.LateUpdate(param_deltaTime);
        
        //OnPinch();
    }
}
