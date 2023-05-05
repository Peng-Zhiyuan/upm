using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraState_Disabled : BaseCameraState
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
        return CameraState.Disabled;
    }
    protected override void AddListener()
    {
    }
    protected override void RemoveListener()
    {
    }

    protected override void OnStateEnter(object param_UserData)
    {
        CameraManager.Instance.CameraTransform.gameObject.SetActive(false);
    }

    protected override void OnStateLeave()
    {
        CameraManager.Instance.CameraTransform.gameObject.SetActive(true);
    }

    public override void LateUpdate(float param_deltaTime)
    {
        //if (getTargetLeave)
        //{
        //    GameEventCenter.Broadcast(GameEvent.CameraStateLeave,CameraState.Disabled);
        //}
    }
}
