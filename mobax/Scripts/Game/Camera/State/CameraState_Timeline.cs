
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraState_Timeline : BaseCameraState
{
    protected override bool CanControl
    {
        get
        {
            return false;
        }
    }
    protected override CameraState GetState()
    {
        return CameraState.Timeline;
    }
    public override void OnStateChangeRequest(int newState, object param_UserData)
    {
        m_StateMachine.ChangeState(newState, param_UserData);
    }

    private Vector3 _enterPos;
    private Vector3 _leavePos;
    private Quaternion _leaveRotation;
    protected override void OnStateEnter(object param_UserData)
    {
        _enterPos = Proxy.CameraTransform.position;
        Proxy.CameraTransform.position = Vector3.zero;
        Proxy.CameraTransform.rotation = Quaternion.identity;
        Proxy.MainCamera.transform.localPosition = _enterPos;

        // TODO: pzy: UImanager 已被移除，需要修改为 UIEngine 的 API
        //UIManager.Instance.HideAllPanel();
        //UIManager.Instance.HideHud();
    }

    protected override void OnStateLeave()
    {
        if (Proxy.MainCamera == null)
            return;

        _leavePos = Proxy.MainCamera.transform.position;
        _leaveRotation = Proxy.MainCamera.transform.rotation;
        Proxy.MainCamera.transform.localPosition = Vector3.zero;
        Proxy.MainCamera.transform.localRotation = Quaternion.identity;
        Proxy.CameraTransform.position = _leavePos;
        Proxy.CameraTransform.rotation = _leaveRotation;
        //if (SceneObjectManager.Instance.LocalPlayer != null)                
        //{
        //    SceneObject myself = SceneObjectManager.Instance.LocalPlayer;
        //    if (myself != null)
        //    {
        //        Proxy.SetPosition(-SceneObjectManager.Instance.LocalPlayer.GetEulerAngles().y, 15f);
        //        Proxy.LateUpdate(0f);
        //    }
        //}
        //Proxy.SyncPosition(_leavePos, 1f);

        // TODO: UIManager 已被移除，需要替换成 UIEngine 的 API
        //UIManager.Instance.RecoverAllPanel();
        //UIManager.Instance.ShowHud();
    }
    
    public override void LateUpdate(float param_deltaTime)
    {
        //不执行原本摄像机逻辑 交给timeline处理
    }
}
