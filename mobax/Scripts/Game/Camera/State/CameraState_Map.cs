using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Rendering.Universal;

public class CameraState_Map : BaseCameraState
{
    protected override CameraState GetState()
    {
        return CameraState.Map;
    }

    private float m_HAngle = 0f;
    private float m_VAngle = 90f;
    private float m_Distance = 50f;

    protected override void OnStateEnter(object param_UserData)
    {
        Proxy.m_HAngle = this.m_HAngle;
        Proxy.m_VAngle = this.m_VAngle;
        Proxy.Distance = this.m_Distance;

        PostProcessHandler.SetPostEffectActive<DepthOfField>(CameraManager.Instance.MainCamera.gameObject, false);
        //PostProcessHandler.SetPostEffectActive<Bloom>(CameraManager.Instance.MainCamera.gameObject, false);
        PostProcessHandler.SetPostEffectActive<Vignette>(CameraManager.Instance.MainCamera.gameObject, true);
       // var fowVolume = PostProcessHandler.GetPostEffect<FoW.FogOfWarURP>(CameraManager.Instance.MainCamera.gameObject);
       // fowVolume.fogColor = new UnityEngine.Rendering.ColorParameter(new Color(0, 0, 0, 1f));
        GameEventCenter.AddListener(GameEvent.Camera_Offset, this, CameraOffset);
    }
	protected override void OnStateLeave()
    {
        Proxy.CleanOffset();
        GameEventCenter.RemoveListener(GameEvent.Camera_Offset, this);
    }

    public override void LateUpdate(float param_deltaTime)
    {
        Proxy.LateUpdate(param_deltaTime);
    }

    private void CameraOffset(object[] eventData)
    {
        Proxy.m_SideOffset -= (float)eventData[0];
        Proxy.SideOffsetY -= (float)eventData[1];
    }
}
