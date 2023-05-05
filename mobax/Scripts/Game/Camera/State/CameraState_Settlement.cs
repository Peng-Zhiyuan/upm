using System.Collections;
using System.Collections.Generic;
using BattleEngine.Logic;
using BattleSystem.Core;
using UnityEngine;

using UnityEngine.Rendering.Universal;

public class CameraState_Settlement : BaseCameraState
{
    protected override CameraState GetState()
    {
        return CameraState.Settlement;
    }

    private float m_HAngle = 90f;
    private float m_VAngle = 0f;
    private float m_Distance = 3f;
    private float m_FOV = 85f;
   
    protected override void OnStateEnter(object param_UserData)
    {
        CameraSetting.Ins.CloseNoise();
        CameraManager.Instance.Move = false;
        
        /*CameraSetting.Ins.SetMaxMinRot(180, -180);
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
        
        if (!BattleEngine.Logic.BattleLogicManager.Instance.ActorLogic.IsWin)
        {
            /*
            var list = BattleEngine.Logic.BattleManager.Instance.ActorMgr.GetCamp(1);
            foreach (var VARIABLE in list)
            if (!BattleResultManager.Instance.IsBattleWin)
            {
                if (!VARIABLE.mData.IsDead)
                {
                    CameraSetting.Ins.SetFollow(VARIABLE.transform);
                    CameraSetting._ins.LookAt(VARIABLE.GetBone("body_hit"));
                    break;  
                }
            }
            #1#
        }
        else
        {
            var role = SceneObjectManager.Instance.GetSelectPlayer();
            if (role != null)
            {
                CameraSetting.Ins.SetFollow(role.transform);
                CameraSetting._ins.LookAt(role.GetBone("body_hit"));
            }
            else
            {
                CameraManager.Instance.Move = false;
            }
        }
        Proxy.Distance = 2.5f;
        Proxy.FieldOfView = 95;
        Proxy.SideOffsetY = 1.5f;
        Proxy.HAngleOffset = 0;
        CameraSetting.Ins.CloseNoise();
        CameraManager.Instance.CameraProxy.TurnImmediate();*/
    }
    protected override void OnStateLeave()
    {
        Proxy.VFollowTarget = true;
    }
    public override void LateUpdate(float param_deltaTime)
    {
        //if(CameraSetting._ins.IsUpdate)
        //Proxy.LateUpdate(param_deltaTime);
        
        //OnPinch();
    }
}
