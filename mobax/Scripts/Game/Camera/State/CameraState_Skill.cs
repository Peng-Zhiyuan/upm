using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

using UnityEngine.Rendering.Universal;

public class CameraState_Skill : BaseCameraState
{
    protected override CameraState GetState()
    {
        return CameraState.Skill;
    }

    /*private float m_HAngle = 0f;
    private float m_VAngle = 20f;
    private float m_Distance = 16f;*/

    private float m_LastDistance;
    private float m_LastVAngel;
    private float m_LastFOV;
    private float m_LastHAngle;

    private CameraPosData m_cameraData = new CameraPosData();

    protected override void OnStateEnter(object param_UserData)
    {

        CameraSetting.Ins.SetBindingMode(CinemachineTransposer.BindingMode.LockToTarget);
        m_LastDistance = Proxy.Distance;
        //CameraSetting.Ins.SetDistance(-2.5f);
        
        //m_LastHAngle = this.Proxy.m_HAngle;
        m_LastFOV = Proxy.FieldOfView;
        //CameraSetting.Ins.SetOffsetX(30);
        //CameraSetting.Ins.SetFOV(95);
        //m_LastVAngel = this.Proxy.m_VAngle;
        Proxy.SideOffsetY = 2.5f;

        //Proxy.ResetHAngle();
        
        Proxy.FieldOfView = 95;
        Proxy.m_HAngle = 40f;
        //Proxy.m_CurrentHAngle = -40f;
        Proxy.HAngleOffset = 0;
        

        int type = (int) param_UserData;
        if (type == 1)
        {
            Proxy.Distance = 3f;
        }
        else if (type == 2)
        {
            Proxy.Distance = 4f;
        }
        CameraSetting.Ins.CloseNoise();
        
        var target = SceneObjectManager.Instance.GetSelectPlayer();
        if (target != null)
        {
            CameraSetting.Ins.LookAt(target.GetBone("body_hit"));
            CameraSetting.Ins.SetFollow(target.GetBone("body_hit"));
        }
        
        if(true)
            return;
        
        var param = (CameraPosData) param_UserData;
        if(param == null)
            return;
        m_cameraData.smoothscale = this.Proxy.m_SmoothScale;
        Proxy.m_HAngle = param.hAngle;
        Proxy.m_VAngle = param.vAngle;
        Proxy.Distance = param.distance;
        Proxy.FieldOfView = param.fov;
        Proxy.m_SmoothScale = param.smoothscale;
        Proxy.m_SideOffset = param.sideOffset;

        //PostProcessHandler.SetPostEffectActive<DepthOfField>(CameraManager.Instance.MainCamera.gameObject, false);
        //MonoHelper.Ins.StartCoroutine(UnitySceneLoadCheck());
    }
    protected IEnumerator UnitySceneLoadCheck()
    {
        yield return null;
        
        while (Proxy.Distance >6f)
        {
            Proxy.Distance = Proxy.Distance - UnityEngine.Time.deltaTime * 4.0f;
            yield return null;
        }

        yield return new WaitForSeconds(2f);
        CameraManager.Instance.TryChangeState(CameraState.Free2);
    }

    protected override void OnStateLeave()
    {
        Proxy.Distance = m_LastDistance;
        Proxy.FieldOfView = m_LastFOV;
        Proxy.SideOffsetY = 2f;
        Proxy.m_HAngle = 0f;

        /*Proxy.m_HAngle = m_LastHAngle;
        Proxy.m_Distance = m_LastDistance;
        Proxy.m_FieldOfView = m_LastFOV;
        Proxy.m_VAngle = m_LastVAngel;
        Proxy.m_SmoothScale = m_cameraData.smoothscale;
        Proxy.m_SideOffset = 0f;*/

        //CameraManager.Instance.TryChangeState(CameraState.Free);

        //CameraSetting.Ins.SetFOV(m_LastFOV);
        //CameraSetting.Ins.SetDistance(m_LastDistance);
        //CameraSetting.Ins.SetOffsetX(0);
        CameraSetting.Ins.OpenNoise();
        
        TimerMgr.Instance.BattleSchedulerTimer(0.1f, delegate { GameEventCenter.Broadcast(GameEvent.CameraRoll, true); });
        
    }
    
    public override void OnStateChangeRequest(int newState, object param_UserData)
    {
        m_StateMachine.ChangeState(newState, param_UserData);
    }

    public override void LateUpdate(float param_deltaTime)
    {
        Proxy.LateUpdate(param_deltaTime);
    }

    public void SetParams(float distance, float fov, float vAngle, float hAngle)
    {
        Proxy.m_HAngle = hAngle;
        Proxy.Distance = distance;
        Proxy.FieldOfView = fov;
        Proxy.m_VAngle = vAngle;
    }
}
