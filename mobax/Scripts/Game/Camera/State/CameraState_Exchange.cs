using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using BattleSystem.Core;
using UnityEngine;

using UnityEngine.Rendering.Universal;


public class CameraState_Exchange : BaseCameraState
{
    protected override CameraState GetState()
    {
        return CameraState.Exchange;
    }

    protected override void OnStateEnter(object param_UserData)
    {
        CameraSetting.Ins.CloseNoise();
        
        CameraSetting.Ins.SetFollow(null);
        CameraSetting.Ins.LookAt(null);
        
        Creature player = (Creature) param_UserData;
        
        
        // GameEventCenter.AddListener(GameEvent.PlayerModeChanged, this, this.PlayerModeChanged);

        //PlayerModeChanged(null);
        Flow(player);
    }

    private float m_LastDistance;
    private float m_LastVAngel;
    private float m_LastFOV;
    private float m_LastHAngle;
    private async void Flow(Creature player)
    {
        await Task.Delay(200);
        
        GameEventCenter.Broadcast(GameEvent.ShowLive2D, player);
        
        if (player != null)
        {
            //SceneObjectManager.Instance.SetSelectPlayer(player);
            CameraSetting.Ins.SetFollow(player.GetBone("body_hit"));
            CameraSetting.Ins.LookAt(player.GetBone("body_hit"));
        }
        
        Proxy.HAngleOffset = 0;
        Proxy.m_HAngleDamp = 0.2f;
        
        CameraSetting.Ins.SyncPosImmediate();
        
        Proxy.m_HAngle = -20f;
        
        m_LastDistance = Proxy.Distance;
        m_LastFOV = Proxy.FieldOfView;

        //Proxy.m_SideOffsetY = 2.5f;
        Proxy.Distance = 2.5f;
        Proxy.FieldOfView = 95;
        //Proxy.m_HAngle = -50f;

        //CameraSetting.Ins.LookAt(SceneObjectManager.Instance.GetSelectPlayer().transform);
        await Task.Delay(5000);
        
        CameraManager.Instance.TryChangeState(CameraState.Free2, null);
    }
    protected override void OnStateLeave()
    {
       // GameEventCenter.RemoveListener(GameEvent.PlayerModeChanged, this);
       Proxy.m_HAngle = 0f;
       Proxy.Distance = m_LastDistance;
       Proxy.FieldOfView = m_LastFOV;
       //Proxy.m_SideOffsetY = 1.5f;

       CameraSetting.Ins.OpenNoise();
    }
    

    public override void LateUpdate(float param_deltaTime)
    {
        Proxy.LateUpdate(param_deltaTime);
    }
    
    public override void OnStateChangeRequest(int newState, object param_UserData)
    {
        m_StateMachine.ChangeState(newState, param_UserData);
    }
}
