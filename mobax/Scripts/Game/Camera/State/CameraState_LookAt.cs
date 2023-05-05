/* Created:Loki Date:2022-10-08*/

public class CameraState_LookAt : BaseCameraState
{
    protected override CameraState GetState()
    {
        return CameraState.LookAt;
    }

    protected override void OnStateEnter(object param_UserData)
    {
        CameraSetting.Ins.SetDamping(0);
        CameraSetting.Ins.CloseNoise();
        CameraSetting.Ins.SetFollow(null);
        CameraSetting.Ins.LookAt(null);
        Creature player = (Creature)param_UserData;
        Flow(player);
    }

    private float m_LastDistance;
    private float m_LastVAngel;
    private float m_LastFOV;
    private float m_LastHAngle;

    private void Flow(Creature player)
    {
        if (player != null)
        {
            SceneObjectManager.Instance.SetSelectPlayer(player);
            CameraSetting.Ins.SetFollow(player.GetBone("body_hit"));
            CameraSetting.Ins.LookAt(player.GetBone("body_hit"));
        }
        Proxy.HAngleOffset = 0;
        Proxy.m_HAngleDamp = 0.2f;
        CameraSetting.Ins.SyncPosImmediate();
        Proxy.m_HAngle = -20f;
        m_LastDistance = Proxy.Distance;
        m_LastFOV = Proxy.FieldOfView;
        Proxy.Distance = 2.5f;
        Proxy.FieldOfView = 95;
    }

    protected override void OnStateLeave()
    {
        CameraSetting.Ins.SetDamping(5);
        Proxy.m_HAngle = 0f;
        Proxy.Distance = m_LastDistance;
        Proxy.FieldOfView = m_LastFOV;
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