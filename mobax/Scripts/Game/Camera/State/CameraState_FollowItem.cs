
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraState_FollowItem : BaseCameraState
{
    protected override CameraState GetState()
    {
        return CameraState.FollowItem;
    }
    public override void OnStateChangeRequest(int newState, object param_UserData)
    {
        m_StateMachine.ChangeState(newState, param_UserData);
    }

    //private float ZOffset;
    private float Dis;
    private float Fov;
    private float HAngle;
    protected override void OnStateEnter(object param_UserData)
    {
        //ZOffset = CameraManager.Instance.CameraProxy.ZOffset;
        Dis = CameraManager.Instance.CameraProxy.Distance;
        Fov = CameraManager.Instance.CameraProxy.FieldOfView;
        HAngle = CameraManager.Instance.CameraProxy.HOffsetAngle;
        
        CameraManager.Instance.CameraProxy.HOffsetAngle = -5;
        CameraManager.Instance.CameraProxy.Distance = CameraManager.Instance.CameraProxy.Distance*0.5f;
        CameraManager.Instance.CameraProxy.FieldOfView = 95f;
    }

    protected override void OnStateLeave()
    {
        CameraManager.Instance.CameraProxy.HOffsetAngle = HAngle;
        CameraManager.Instance.CameraProxy.Distance = Dis;
        CameraManager.Instance.CameraProxy.FieldOfView = Fov;
    }
    
    public override void LateUpdate(float param_deltaTime)
    {
        
    }
}
