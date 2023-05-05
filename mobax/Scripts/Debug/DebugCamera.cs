using System;
using BattleSystem.Core;
using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugCamera : MonoBehaviour
{
    public CameraState CurCameraState = CameraState.Free;
    public CameraState CameraState = CameraState.Free;

    public bool VFollowTarget = true;

    public float HAngle;
    public float VAngle;
    public float Distance;
    public float SideOffsetX;
    public float SideOffsetY;
    public float FOV;
    public float LookY;
    public float MoveSpeed;
    public AnimationCurve Curve = AnimationCurve.Linear(0, 1, 1,3 );
    
    public void ReLoad()
    {
        if (!CameraManager.IsAccessable) return;
        BaseCameraState camerastate = CameraManager.Instance.GetState(CameraState) ;
        if(camerastate == null)
            return;
        var proxy = CameraManager.Instance.CameraProxy;
        VFollowTarget = proxy.VFollowTarget;
        VAngle = camerastate.VAngle;
        HAngle = camerastate.HAngle;
        Distance = camerastate.Distance;
        SideOffsetX = camerastate.XOffset;
        SideOffsetY = camerastate.YOffset;
        LookY = proxy.m_LookOffsetY;
        FOV = camerastate.FOV;
        MoveSpeed = proxy.OffsetSpeed;

        /*//相机参数
        var proxy = CameraManager.Instance.CameraProxy;
        HAngle = proxy.m_HAngle;
        VAngle = proxy.m_VAngle;
        Distance = proxy.m_Distance;
        SideOffsetX = proxy.m_SideOffset;
        SideOffsetY = proxy.m_LookOffsetY;
        FOV = proxy.m_FieldOfView;*/
    }

    public void UpdateData()

    {

        var isAccessable = CameraManager.IsAccessable;
        if(!isAccessable)
        {
            return;
        }
        BaseCameraState camerastate = CameraManager.Instance.GetState(CameraState) ;

        if(camerastate == null)
            return;
        /*camerastate.VAngle = VAngle;
        camerastate.HAngle = HAngle;
        camerastate.Distance = Distance;
        camerastate.FOV = FOV;
        camerastate.XOffset = SideOffsetX;
        camerastate.YOffset = SideOffsetY;
        CameraManager.Instance.ResetProxy();
        
        var proxy = CameraManager.Instance.CameraProxy;
        proxy.VFollowTarget = VFollowTarget;
        proxy.SetLookOffsetY(LookY);*/
        //var proxy = CameraManager.Instance.CameraProxy;
        //proxy.OffsetSpeed = MoveSpeed;
    }

    public void Update()
    {
        if(!CameraManager.IsAccessable)
        {
            return;
        }
        CurCameraState = (CameraState)CameraManager.Instance.GetCurrentStateID();
    }

    public void OnValidate()
    {
        UpdateData();
    }

    protected void OnDrawGizmos()
    {
        if(!CameraManager.IsAccessable)
        {
            return;
        }
        /*if (CameraManager.Instance.TargetRole != null && CameraManager.Instance.TargetRole.Target != null)
        {
            Creature _targetRole = CameraManager.Instance.TargetRole.Target as Creature;
            if (!_targetRole.ClientDead)
            {
                Transform _targetBone = _targetRole.GetBone("Bip001 Spine");
                Transform _fromBone = CameraManager.Instance.TargetRole.GetBone("Bip001 Spine");
                if(_targetBone != null && _fromBone != null)
                    Gizmos.DrawLine(_fromBone.position, _targetBone.position);
            }
        }*/
        
    }
    
    
}
