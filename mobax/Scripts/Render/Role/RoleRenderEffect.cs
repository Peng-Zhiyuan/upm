using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.UI;


[ExecuteAlways]
[RequireComponent(typeof(RoleRender))]
public class RoleRenderEffect:MonoBehaviour
{
    static Color defaultLightColor = new Color(1, 1, 1, 0);
    static Color defaultHDRColor = Color.yellow;
    public LightSpace lightSpace = LightSpace.Local; 
    public float lightRotateY = 0;
/*    public float dissolveRatio = 0;
    public bool dissolveWeapon = false;*/
    public Color customLightColor = defaultLightColor;
    public float customLightAtten = 1;
    public bool fresnel = false;
    [ColorUsageAttribute(true, true)]
    public Color fresnelColor = defaultHDRColor;
    public float fresnelStartVal = 0;
    public float faceMask = 0;
/*  public bool eyeRotateSpecular = false;
    public bool eyeQuiverSpecular = false;
    public bool eyeTearfulSpecular = false;*/

  /*  public bool useMatCap;
    public Texture2D matCapTex;
    public Color matCapColor = defaultColor;
    public float matCapPower = 1;
    public float matCapAlphaPower = 1;
    public float matCapMultiply =1;
    public float matCapAdd = 1;*/

    private RoleRender render;
    private void Awake()
    {
        this.render = this.gameObject.GetOrAddComponent<RoleRender>();
        this.enabled = false;
    }
   
    public void Update()
    {
        if (!this.enabled) return;
        this.render.CustomLightAtten = customLightAtten;
        this.render.CustomLightColor  = customLightColor;
        this.render.UseFresnelEffect = fresnel;
        this.render.FresnelColor = fresnelColor;
        this.render.FresnelStart = fresnelStartVal;
      
        //this.render.SetWeaponDissolve(dissolveRatio, Color.yellow);
        this.render.CustomLightSpace = lightSpace;
        this.render.LightRotateY = lightRotateY;
       /* Vector4 lightDirection;
        if(lightSpace == LightSpace.Local)
        {
            lightDirection = Quaternion.Euler(0,lightRotateY * 30, 0) * this.transform.right;
        }
        else
        {
            lightDirection = Quaternion.Euler(0,lightRotateY * 30, 0) * Vector3.right;
        }
        lightDirection.w = lightRotateY > 0? 1: 0;
        this.render.SetCustomLightDir(lightDirection);*/
        this.render.UseFaceMask = faceMask;
        this.render.ApplyMPBlock();
        /*      this.render.SetEyeRotateSpecular(eyeRotateSpecular);
                this.render.SetEyeQuiverSpecular(eyeQuiverSpecular);
                this.render.SetTearfulSpecular(eyeTearfulSpecular);
                this.render.SetMatCap(useMatCap, matCapTex, matCapColor, matCapPower, matCapAlphaPower, matCapMultiply, matCapAdd);
        */
    }

    public void ResetEffect()
    {
        this.render.ResetProperties();
        lightRotateY = 0;
        //dissolveRatio = 0;
        customLightAtten = 1;
        fresnelStartVal = 0;
        fresnel = false;
        customLightColor = defaultLightColor;
        fresnelColor = defaultHDRColor;
        faceMask = 0;
        //useMatCap = false;
        //this.matCapTex = null;
       /* this.render.RefreshRenderList();
        this.render.SetFresnel(fresnel, fresnelColor, fresnelStartVal);
        this.render.SetWeaponDissolve(dissolveRatio,Color.yellow,  dissolveWeapon);
        this.render.SetCustomLightColor(customLightColor);
        this.render.SetCustomLightDir(Vector4.zero);
        this.render.SetMask(faceMask);
        this.render.SetCustomLightAtten(1);*/
        /* this.render.SetEyeRotateSpecular(eyeRotateSpecular);
         this.render.SetEyeQuiverSpecular(eyeQuiverSpecular);
         this.render.SetTearfulSpecular(eyeTearfulSpecular);
         eyeRotateSpecular = false;
         eyeQuiverSpecular = false;
         eyeTearfulSpecular = false;
         this.render.SetMatCap(useMatCap, matCapTex, matCapColor, matCapPower, matCapAlphaPower, matCapMultiply, matCapAdd);
        */

    }

    public void OnEnable()
    {
        this.ResetEffect();
    }

    public void OnDisable()
    {
        this.ResetEffect();
    }
}
