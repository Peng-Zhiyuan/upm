using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraState_Fixed : BaseCameraState
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
        return CameraState.Fixed;
    }
    protected override void AddListener()
    {
    }
    protected override void RemoveListener()
    {
    }

    protected override void OnStateEnter(object param_UserData)
    {
        /*if (Battle.Instance.param.coreParam.stageInfo.StageId == 101)
        {
            CameraManager.Instance.CVCamera.transform.position = new Vector3(2.4f, 4.13f, 29f);
            CameraManager.Instance.CVCamera.transform.eulerAngles = new Vector3(25, -90, 0);
        }
        else  if (Battle.Instance.param.coreParam.stageInfo.StageId == 103)
        {
            CameraManager.Instance.CVCamera.transform.position = new Vector3(-11.5f, 4.13f, 13.64f);
            CameraManager.Instance.CVCamera.transform.eulerAngles = new Vector3(25, -90, 0);
        }*/

        CameraManager.Instance.CVCamera.transform.position = CameraManager.Instance.TimeLineCameraPos;
        CameraManager.Instance.CVCamera.transform.eulerAngles = CameraManager.Instance.TimeLineCameraEnler;
        CameraManager.Instance.MainCamera.transform.position = CameraManager.Instance.TimeLineCameraPos;
        CameraManager.Instance.MainCamera.transform.eulerAngles = CameraManager.Instance.TimeLineCameraEnler;
    }

    protected override void OnStateLeave()
    {
        //CameraManager.Instance.CameraTransform.gameObject.SetActive(true);
    }

    public override void LateUpdate(float param_deltaTime)
    {
        //if (getTargetLeave)
        //{
        //    GameEventCenter.Broadcast(GameEvent.CameraStateLeave,CameraState.Disabled);
        //}
    }
}
