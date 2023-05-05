using UnityEngine;

public class GuildCameraState_Front : GuildBaseCameraState
{
    protected override GuildCameraState GetState()
    {
        return GuildCameraState.Front;
    }
    
    protected override string CameraName()
    {
        return "CMMain_Front";
    }
    
    

    protected override void OnStateEnter(object param_UserData)
    {
        GuildPlayer player = param_UserData as GuildPlayer;

        Transform bone = player.GetBone("body_hit");
        CVCamera.Follow = bone;
        CVCamera.LookAt = bone;
        //GuildCameraManager.Instance.TurnImmediate();
    }

    public void SetTarget(GuildPlayer player)
    {
        Transform bone = player.GetBone("body_hit");
        CVCamera.Follow = bone;
        CVCamera.LookAt = bone;
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