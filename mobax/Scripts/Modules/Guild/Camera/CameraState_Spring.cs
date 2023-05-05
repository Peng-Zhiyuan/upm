using Cinemachine;
using UnityEngine;

public class GuildCameraState_Spring : GuildBaseCameraState
{
    protected override GuildCameraState GetState()
    {
        return GuildCameraState.Spring;
    }
    
    protected override string CameraName()
    {
        return "CMMain_Spring";
    }
    
    

    protected override void OnStateEnter(object param_UserData)
    {
        //CVCamera.Follow = GuildLobbyManager.Instance.LocalPlayer.transform;
        //CVCamera.LookAt = GuildLobbyManager.Instance.LocalPlayer.transform;
        m_CurrentSideOffsetY = 0;
        m_SideOffsetY = 0;
        GuildCameraManager.Instance.SetCameraFadeMode(CinemachineBlendDefinition.Style.Cut);
        GuildCameraManager.Instance.TurnImmediate();
    }
    
    protected override void OnStateLeave()
    {
    }
    
    public float m_CurrentSideOffsetY = 0f;
    private float m_SideOffsetY = 0f;
    private float m_SideOffsetYSmooth = 0f;
    public float m_SideOffsetYDamp = 0.3f;
    public float m_SmoothScale = 1f;
    public override void LateUpdate(float param_deltaTime)
    {
        m_CurrentSideOffsetY = Mathf.SmoothDamp(m_CurrentSideOffsetY, m_SideOffsetY, ref m_SideOffsetYSmooth, m_SideOffsetYDamp * m_SmoothScale);
        
        var angeles = CVCamera.transform.localEulerAngles;
        angeles.y = 77.24f + m_CurrentSideOffsetY; 
        //CVCamera.transform.localEulerAngles = angeles;
        
        UpdateFingerInput();
    }

    public void SetOffsetY(float offset)
    {
        m_SideOffsetY += offset;
        m_SideOffsetY = Mathf.Clamp(m_SideOffsetY, -15f, 15f);
        //m_SideOffsetY = offset;
    }
    
    public override void OnStateChangeRequest(int newState, object param_UserData)
    {
        m_StateMachine.ChangeState(newState, param_UserData);
    }

    private Touch oldTouch1;  //上次触摸点1(手指1) 
    private Touch oldTouch2;  //上次触摸点2(手指2)
    public float farOffset = 0;
    public float nearOffset = 0;

    private float speed = 100f;
    
    private void UpdateFingerInput()
    {
        if (Input.touchCount > 1)//多点触碰
        {
            //if(cameraCtrl.m_SideZOffset > 1 || cameraCtrl.m_SideZOffset < 0.3) return;
            //多点触摸, 放大缩小
            Touch newTouch1 = Input.GetTouch(0);
            Touch newTouch2 = Input.GetTouch(1);
            //第2点刚开始接触屏幕, 只记录，不做处理
            if (newTouch2.phase == TouchPhase.Began)
            {
                oldTouch2 = newTouch2;
                oldTouch1 = newTouch1;
                return;
            }
            //计算老的两点距离和新的两点间距离，变大要放大模型，变小要缩放模型
            float oldDistance = Vector2.Distance(oldTouch1.position, oldTouch2.position);
            float newDistance = Vector2.Distance(newTouch1.position, newTouch2.position);
            //两个距离之差，为正表示放大手势， 为负表示缩小手势
            float offset = newDistance - oldDistance;
            //放大因子， 一个像素按 0.01倍来算(100可调整)
            float scaleFactor = -offset * 2 / 100f;
            //cameraCtrl.CameraBiasDelta(0, 0, scaleFactor);
            float zOffset = CVCamera.m_Lens.FieldOfView + scaleFactor;
            if (CVCamera != null)
            {
                CVCamera.m_Lens.FieldOfView = Mathf.Clamp(zOffset, 50f, 80f);
            }
            //记住最新的触摸点，下次使用
            oldTouch1 = newTouch1;
            oldTouch2 = newTouch2;
        }
        
        if (Input.GetKey(KeyCode.UpArrow))
        {
            CVCamera.m_Lens.FieldOfView -= UnityEngine.Time.deltaTime * speed;
        }
        //Zoom in
        if (Input.GetKey(KeyCode.DownArrow))
        {
            CVCamera.m_Lens.FieldOfView += UnityEngine.Time.deltaTime * speed;
        }
        CVCamera.m_Lens.FieldOfView = Mathf.Clamp(CVCamera.m_Lens.FieldOfView,  50f, 80);
        
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            m_SideOffsetY -= UnityEngine.Time.deltaTime * speed;
        }
        //Zoom in
        if (Input.GetKey(KeyCode.RightArrow))
        {
            m_SideOffsetY += UnityEngine.Time.deltaTime * speed;
        }
        m_SideOffsetY = Mathf.Clamp(m_SideOffsetY,  -15f, 15f);
    }

    public void ResetCameraFov()
    {
        CVCamera.m_Lens.FieldOfView = 72f;
        m_SideOffsetY = 0;
    }
}