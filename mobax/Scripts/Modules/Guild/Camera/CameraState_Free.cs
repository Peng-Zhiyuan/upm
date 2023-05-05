using UnityEngine;

public class GuildCameraState_Free : GuildBaseCameraState
{
    protected override GuildCameraState GetState()
    {
        return GuildCameraState.Free;
    }
    
    protected override string CameraName()
    {
        return "CMMain_Free";
    }
    
    

    protected override void OnStateEnter(object param_UserData)
    {
        CVCamera.Follow = GuildLobbyManager.Instance.LocalPlayer.transform;
        CVCamera.LookAt = GuildLobbyManager.Instance.LocalPlayer.transform;
        GuildCameraManager.Instance.TurnImmediate();

        if (GuildLobbyManager.Instance.LastIsSpring)
        {
            var incamera = CVCamera.transform.parent.Find("CMMain_SpringPortOut");
            incamera.SetActive(true);
            TimerMgr.Instance.BattleSchedulerTimerDelay(1f, delegate
            {
                incamera.SetActive(false);
            });
        }
    }
    
    protected override void OnStateLeave()
    {
    }
    
    public override void LateUpdate(float param_deltaTime)
    {
    }
    
    public override void OnStateChangeRequest(int newState, object param_UserData)
    {
        m_StateMachine.ChangeState(newState, param_UserData);
    }
}