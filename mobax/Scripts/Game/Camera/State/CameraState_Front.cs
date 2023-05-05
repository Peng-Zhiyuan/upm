using System.Collections;
using System.Collections.Generic;
using BattleSystem.Core;
using Cinemachine;
using UnityEngine;

using UnityEngine.Rendering.Universal;

public class CameraState_Front : BaseCameraState
{
    protected override CameraState GetState()
    {
        return CameraState.Front;
    }

    private float m_HAngle = 90f;
    private float m_VAngle = 0f;
    private float m_Distance = 3f;
    private float m_FOV = 85f;
   
    protected override void OnStateEnter(object param_UserData)
    {
        CameraManager.Instance.Move = true;
        CameraSetting.Ins.SetDeadZone(0, 0);
        CameraSetting.Ins.SetBindingMode(CinemachineTransposer.BindingMode.LockToTarget);
        CameraSetting.Ins.SetMaxMinRot(180, -180);
        Proxy.m_HAngle = this.m_HAngle;
        Proxy.m_VAngle = this.m_VAngle;
        Proxy.Distance = this.m_Distance;
        Proxy.m_SideOffset = 0f;
        Proxy.HAngleOffset = 0f;
        Proxy.m_HAngleDamp = 0.4f;
        Proxy.m_DistanceOffset = 0f;
        Proxy.FieldOfView = this.m_FOV;
        Proxy.SetLookOffsetY(0.85f);
        Proxy.VFollowTarget = false;
        Proxy.Front();

        var role = param_UserData as Creature;
        if (role != null)
        {
            CameraSetting.Ins.SetFollow(role.GetBone("body_hit"));
            CameraSetting._ins.LookAt(role.GetBone("body_hit"));
            
            TimerMgr.Instance.BattleSchedulerTimer(0.2f, delegate
            {
                CameraSetting.Ins.SetFollow(role.GetBone("body_hit"));
                CameraSetting._ins.LookAt(role.GetBone("body_hit"));
            });
        }
        Proxy.Distance = 12.5f;
        Proxy.FieldOfView = 95;
        Proxy.SideOffsetY = 1f;
        Proxy.HAngleOffset = 0;
        CameraSetting.Ins.CloseNoise();
        CameraManager.Instance.CameraProxy.TurnImmediate();
    }
    protected override void OnStateLeave()
    {
        Proxy.VFollowTarget = true;
    }
    public override void LateUpdate(float param_deltaTime)
    {
        //if(CameraSetting._ins.IsUpdate)
        Proxy.LateUpdate(param_deltaTime);
        
        //OnPinch();
    }
}
