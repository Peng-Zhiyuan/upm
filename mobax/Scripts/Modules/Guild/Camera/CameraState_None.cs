public class GuildCameraState_None : GuildBaseCameraState
{
    protected override GuildCameraState GetState()
    {
        return GuildCameraState.None;
    }
    
    protected override string CameraName()
    {
        return "CMMain_None";
    }
    
    protected override void OnStateEnter(object param_UserData)
    {
        
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