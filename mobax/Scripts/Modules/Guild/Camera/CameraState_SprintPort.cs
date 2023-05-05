using Cinemachine;

public class GuildCameraState_SpringPort : GuildBaseCameraState
{
    protected override GuildCameraState GetState()
    {
        return GuildCameraState.SpringPort;
    }
    
    protected override string CameraName()
    {
        return "CMMain_SpringPort";
    }
    
    

    protected override void OnStateEnter(object param_UserData)
    {
        //CVCamera.Follow = GuildLobbyManager.Instance.LocalPlayer.transform;
        //CVCamera.LookAt = GuildLobbyManager.Instance.LocalPlayer.transform;
        
        GuildCameraManager.Instance.TurnImmediate();
    }
    
    protected override void OnStateLeave()
    {
        //GuildCameraManager.Instance.MainCamera.GetComponent<CinemachineBrain>().m_DefaultBlend = new CinemachineBlendDefinition(CinemachineBlendDefinition.Style.Cut, 2f);
    }
    
    public override void LateUpdate(float param_deltaTime)
    {
    }
    
    public override void OnStateChangeRequest(int newState, object param_UserData)
    {
        m_StateMachine.ChangeState(newState, param_UserData);
    }
}