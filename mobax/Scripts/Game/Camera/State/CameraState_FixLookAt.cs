using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using BattleEngine.Logic;
using BattleSystem.Core;
using Cinemachine;
using Unity.Mathematics;
using UnityEngine;

public class CameraTransInfo
{
    public Vector3 pos;
    public Quaternion rotation;
}

public class CameraState_FixLookAt : BaseCameraState
{
    protected override CameraState GetState()
    {
        return CameraState.FixLookAt;
    }

    protected override void OnStateEnter(object param_UserData)
    {
        EventManager.Instance.RemoveListener<CameraTransInfo>("ChangeBattleFixCamera", ChangeCameraLookAt);
        EventManager.Instance.AddListener<CameraTransInfo>("ChangeBattleFixCamera", ChangeCameraLookAt);
        CameraManager.Instance.CVCamera.SetActive(false);
        CameraManager.Instance.FixCamera.SetActive(true);
    }

    protected override void OnStateLeave()
    {
        EventManager.Instance.RemoveListener<CameraTransInfo>("ChangeBattleFixCamera", ChangeCameraLookAt);
    }

    public override void OnStateChangeRequest(int newState, object param_UserData)
    {
        m_StateMachine.ChangeState(newState, param_UserData);
    }

    public void ChangeCameraLookAt(CameraTransInfo val)
    {
        CameraManager.Instance.FixCamera.transform.localPosition = val.pos;
        CameraManager.Instance.FixCamera.transform.localRotation = val.rotation;
    }
}