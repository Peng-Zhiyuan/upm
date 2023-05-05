using System.Collections;
using System.Collections.Generic;
using BattleSystem.Core;
using UnityEngine;

using UnityEngine.Rendering.Universal;


public class CameraState_Follow : BaseCameraState
{
    protected override CameraState GetState()
    {
        return CameraState.Follow;
    }

    private float m_HAngle = 0;
    private float SideOffsetY = 2f;
    private float m_Distance = 4f;
   
    protected override void OnStateEnter(object param_UserData)
    {
        Proxy.m_HAngle = this.m_HAngle;
        Proxy.Distance = this.m_Distance;
        Proxy.SideOffsetY = SideOffsetY;
        //Proxy.TurnImmediate();
    }
    protected override void OnStateLeave()
    {
    }
    
    public override void LateUpdate(float param_deltaTime)
    {
        Proxy.LateUpdate(param_deltaTime);
        
        Proxy.FollowSelectHero();
    }
}
