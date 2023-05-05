using System.Collections;
using System.Collections.Generic;
using BattleSystem.Core;
using UnityEngine;

using UnityEngine.Rendering.Universal;


public class CameraState_SkillSelect : BaseCameraState
{
    protected override CameraState GetState()
    {
        return CameraState.SkillSelect;
    }

    private float m_HAngle = 0f;
    private float m_VAngle = 15.2f;
    private float m_Distance = 3.0f;
    private float m_FOV = 95f;
   
    protected override void OnStateEnter(object param_UserData)
    {
        Proxy.m_HAngle = this.m_HAngle;
        Proxy.m_VAngle = this.m_VAngle;
        Proxy.Distance = this.m_Distance;
        Proxy.FieldOfView = this.m_FOV;
        Proxy.HOffsetAngle = 25f;
        Proxy.ZOffset = 0f;
        Proxy.SetLookOffsetY(0f);
        Proxy.UseBone = true;
        Debug.LogError("技能相机");
        float offset = 30f;
        float y = (float) param_UserData;
        if (y > 0)
        {
            //Proxy.HOffset = -offset;
            //Proxy.m_SideOffset = 0.4f;
        }
            
        else
        {
            //Proxy.HOffset = offset;
            //Proxy.m_SideOffset = -0.4f;
        }

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
    }
    protected override void OnStateLeave()
    {
        //Proxy.ResetTransitionTime();
        //Proxy.HOffset = 0f;
        Proxy.UseBone = false;
    }
    
    public override void LateUpdate(float param_deltaTime)
    {
        Proxy.LateUpdate(param_deltaTime);
        
        //OnPinch();
    }
    
    public override void OnStateChangeRequest(int newState, object param_UserData)
    {
        m_StateMachine.ChangeState(newState, param_UserData);
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
