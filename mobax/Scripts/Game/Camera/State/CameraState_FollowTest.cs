using System.Collections;
using System.Collections.Generic;
using BattleSystem.Core;
using UnityEngine;

using UnityEngine.Rendering.Universal;


public class CameraState_FollowTest : BaseCameraState
{
    protected override CameraState GetState()
    {
        return CameraState.FollowTest;
    }

    private float m_HAngle = 135f;
    private float m_VAngle = 20f;
    private float m_Distance = 12f;
   
    protected override void OnStateEnter(object param_UserData)
    {
        Proxy.m_HAngle = this.m_HAngle;
        Proxy.m_VAngle = this.m_VAngle;
        Proxy.Distance = this.m_Distance;
        Proxy.m_SideOffset = -0.5f;

        CaculateDir();
        /*PostProcessHandler.SetPostEffectActive<DepthOfField>(CameraManager.Instance.MainCamera.gameObject, true);
        PostProcessHandler.SetPostEffectActive<Vignette>(CameraManager.Instance.MainCamera.gameObject, false);
        PostProcessHandler.SetPostEffectActive<Bloom>(CameraManager.Instance.RTCamera.gameObject, false);
        PostProcessHandler.SetPostEffectActive<Bloom>(CameraManager.Instance.MainCamera.gameObject, true);
        PostProcessHandler.SetPostEffectActive<MotionBlur>(CameraManager.Instance.MainCamera.gameObject, false);*/
       // var fowVolume =  PostProcessHandler.GetPostEffect<FoW.FogOfWarURP>(CameraManager.Instance.MainCamera.gameObject);
       // fowVolume.fogColor = new UnityEngine.Rendering.ColorParameter(new Color(0, 0, 0, 0.851f));

        //var blitFeature = RenderFeatureHandler.Ins.GetRenderFeatureByName("ColorBlendBlit") as Blit;
        //blitFeature.settings.blitMaterial.SetColor("_BlendColor", new Color(0, 0, 0, 0));
        
        GameEventCenter.AddListener(GameEvent.PlayerModeChanged, this, this.PlayerModeChanged);

        PlayerModeChanged(null);
    }
    protected override void OnStateLeave()
    {
        GameEventCenter.RemoveListener(GameEvent.PlayerModeChanged, this);
    }

    private void PlayerModeChanged(object[] data)
    {
        if(!GameObject.Find("Demo2Map"))
            return;

        //if (BattleCore.lastedInstance.playMode == PlayMode.BATTLE)
        //{
        //    Proxy.m_Distance = Proxy.m_Distance + 5;
        //}
        //else
        //{
        //    Proxy.m_Distance = 15f;
        //}
        Proxy.Distance = 15f;
    }

    public override void LateUpdate(float param_deltaTime)
    {
        Proxy.LateUpdate(param_deltaTime);
        
        OnPinch();
    }

    private void OnPinch()
    {
        float maxDistance = 30;
        float minDistance = 15;
        float speed = 77f;

        if (Input.GetAxis("Mouse ScrollWheel") < 0)
        {
            Proxy.Distance -= UnityEngine.Time.deltaTime*speed;;
            Proxy.m_VAngle -= UnityEngine.Time.deltaTime*speed * 0.2f;
        }
        //Zoom in
        if (Input.GetAxis("Mouse ScrollWheel") > 0)
        {
            Proxy.Distance += UnityEngine.Time.deltaTime*speed;;
        }
        Proxy.Distance = Mathf.Clamp(Proxy.Distance, minDistance, maxDistance);

        if (Input.GetAxis("Mouse Y") != 0)
        {
            //Proxy.m_VAngle += UnityEngine.Time.deltaTime*speed*Input.GetAxis("Mouse Y");
        }
        if (Input.GetKey(KeyCode.A))
        {
            Proxy.m_HAngle -= UnityEngine.Time.deltaTime*speed * 0.5f;
        }
        
        if (Input.GetKey(KeyCode.D))
        {
            Proxy.m_HAngle += UnityEngine.Time.deltaTime*speed * 0.5f;
        }
        
        if (Input.GetKey(KeyCode.UpArrow))
        {
            Proxy.m_VAngle -= UnityEngine.Time.deltaTime*speed * 0.2f;
        }
        
        if (Input.GetKey(KeyCode.DownArrow))
        {
            Proxy.m_VAngle += UnityEngine.Time.deltaTime*speed * 0.2f;
        }
    }
}
