using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections.Generic;

public class RoleRenderFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class RoleSettings
    {
        //public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingSkybox +1;
        public Material depthMaterial = null;
        public bool OpenRoleOutLine = true;
        /*
        public Material lutMaterial = null;
        public Material fadeMaterial;
        public Material colorAdjustMaterial;
        */
    }

    public class RoleRenderPass : ScriptableRenderPass
    {
        private ShaderTagId mClothBeforeLiquidShaderTag;
        private ShaderTagId mRoleLiquidShaderTag;
        private ShaderTagId mRoleForwardShaderTag;
        private ShaderTagId mRoleEyeShaderTag;
        private ShaderTagId mRoleHairShaderTag;
        private ShaderTagId mEyebrowShaderTag;
        private ShaderTagId mEyebrowGShaderTag;

        private ShaderTagId mClothShaderTag;
        private ShaderTagId mClothTransparentShaderTag;
        private ShaderTagId mOutlineShaderTag;
        //private ShaderTagId mFurShaderTag;
        //private ShaderTagId mPlaneShadowShaderTag;
        
        private List<ShaderTagId> mClothAndOutlineShaderTags;
        private List<ShaderTagId> mClothNoOutlineShaderTags;
        //private FilteringSettings mRoleShadowCasterFilter;
        private FilteringSettings mRoleAndRoleShadowFilter;
        private FilteringSettings mRoleFocusFilter;
        private RoleSettings mRoleSettings;
        //RenderTargetIdentifier m_Source;
        private NGRenderBuffer ngRenderBuffer;
        private NGGrabTexturePass ngGrabTexturePass;
        public void Setup(RenderTargetIdentifier source)
        {
            //m_Source = source;
            ngRenderBuffer.Setup(source);
            ngGrabTexturePass.Setup(ngRenderBuffer);
        }
     
        public RoleRenderPass(RoleSettings settings)
        {
           
            mClothBeforeLiquidShaderTag = new ShaderTagId("ClothBeforeLiquid");
            mRoleLiquidShaderTag = new ShaderTagId("RoleLiquid");
            mRoleForwardShaderTag = new ShaderTagId("RoleForward");
            mRoleEyeShaderTag = new ShaderTagId("RoleEye");
            mRoleHairShaderTag = new ShaderTagId("RoleHair");
            mEyebrowShaderTag = new ShaderTagId("Eyebrow");
            mEyebrowGShaderTag = new ShaderTagId("EyebrowG");
            //mEyeGShaderTag = new ShaderTagId("EyeG");
            mClothShaderTag = new ShaderTagId("Cloth");
            mClothTransparentShaderTag = new ShaderTagId("ClothTransparent");
            mOutlineShaderTag = new ShaderTagId("Outline");

            mClothAndOutlineShaderTags = new List<ShaderTagId>();
            mClothAndOutlineShaderTags.Add(mClothShaderTag);

            mClothAndOutlineShaderTags.Add(mRoleLiquidShaderTag);
            mClothAndOutlineShaderTags.Add(mClothTransparentShaderTag);
            mClothAndOutlineShaderTags.Add(mOutlineShaderTag);


            mClothNoOutlineShaderTags = new List<ShaderTagId>();
            mClothNoOutlineShaderTags.Add(mClothShaderTag);

            mClothNoOutlineShaderTags.Add(mRoleLiquidShaderTag);
            mClothNoOutlineShaderTags.Add(mClothTransparentShaderTag);
            //mPlaneShadowShaderTag = new ShaderTagId("PlaneShadow");
            //过滤设定
            RenderQueueRange queue = new RenderQueueRange();
            queue.lowerBound = 2000;
            queue.upperBound = 3000;
            //int roleLayer = 1 << LayerMask.NameToLayer("Role") | 1 << LayerMask.NameToLayer("Default") | 1 << LayerMask.NameToLayer("UI");
            int roleLayer = 1 << LayerMask.NameToLayer("Role") | 1 << LayerMask.NameToLayer("Default") | 1 << LayerMask.NameToLayer("UI") | 1 << LayerMask.NameToLayer("FOCUS");
            mRoleAndRoleShadowFilter = new FilteringSettings(queue, roleLayer);
            int focusLayer = 1 << LayerMask.NameToLayer("FOCUS");
            mRoleFocusFilter = new FilteringSettings(queue, focusLayer);

            mRoleSettings = settings;
            ngRenderBuffer = new NGRenderBuffer();
            ngGrabTexturePass = new NGGrabTexturePass();

        }

        CommandBuffer cmd;
        string mProfilerTag = "RoleRender";
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CameraCustomData customData = RenderFeatureCache.GetCustomData(renderingData.cameraData.camera);
           // CameraCustomData customData = renderingData.cameraData.camera.transform.GetComponent<CameraCustomData>();
            cmd = CommandBufferPool.Get(mProfilerTag);
            ngRenderBuffer.InitCustomSource(renderingData);
               
            /*
            if (customData != null && mRoleSettings.lutMaterial != null && customData.EnvLUT.Open)
            {

                //using的做法就是可以在FrameDebug上看到里面的所有渲染
                using (new ProfilingScope(cmd, m_EnvLutSampler))
                {
                    //创建一张RT
                    // RenderTextureDescriptor opaqueDesc = renderingData.cameraData.cameraTargetDescriptor;
                    // opaqueDesc.depthBufferBits = 0;
                    // cmd.GetTemporaryRT(m_TemporaryColorTexture.id, opaqueDesc, FilterMode.Bilinear);
                    int lut_height = 32;
                    int lut_width = 32 * 32;
                    mRoleSettings.lutMaterial.SetVector("_LUTParameters", new Vector3(1f / lut_width, 1f / lut_height, lut_height - 1f));
                    //mRoleSettings.lutMaterial.SetVector("_LUTParameters", new Vector4(lut_height, 0.5f/ lut_width, 0.5f/lut_height, lut_height /(lut_height - 1f)));
                    mRoleSettings.lutMaterial.SetFloat("_Strength", customData.EnvLUT.Strength);
                    mRoleSettings.lutMaterial.SetTexture("_LUT", customData.EnvLUT.LUTTexture);
                    cmd.Blit(ngRenderBuffer.GetSource(), ngRenderBuffer.GetTarget(), mRoleSettings.lutMaterial);
                }
                //执行
                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();
                ngRenderBuffer.SwapBuffer();
            }

            if (customData != null && customData.colorAdjustments.openColorAdjustments && mRoleSettings.colorAdjustMaterial != null)
            {
                using (new ProfilingScope(cmd, m_ColorAdjustSampler))
                {

                    customData.colorAdjustments.ColorFilter.a = 1 - customData.colorAdjustments.fadeScene;
                    mRoleSettings.colorAdjustMaterial.SetVector(this._ColorAdjustmentsId, new Vector4(Mathf.Pow(2f, customData.colorAdjustments.PostExposure),
                    customData.colorAdjustments.Contrast * 0.01f + 1f,
                    customData.colorAdjustments.HueShift * (1.0f / 360f),
                    customData.colorAdjustments.Saturation * 0.01f + 1f));
                    mRoleSettings.colorAdjustMaterial.SetColor(this._ColorFilterId, customData.colorAdjustments.ColorFilter.linear);
                    cmd.Blit(ngRenderBuffer.GetSource(), ngRenderBuffer.GetTarget(), mRoleSettings.colorAdjustMaterial, 0);

                  
                }
                //执行
                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();
                ngRenderBuffer.SwapBuffer();
            }
            ngRenderBuffer.ResetBuffer(context, cmd);
        */
            DrawRenders(context, ref renderingData, customData, false);
            /*
            if(customData != null && customData.Focus.showFocusLayer)
            {
                //using的做法就是可以在FrameDebug上看到里面的所有渲染
                mRoleSettings.fadeMaterial.SetFloat("_Val", 1 - customData.Focus.fadeBlack);
                using (new ProfilingScope(cmd, m_FadeBlackSampler))
                {
                    //创建一张RT
                    RenderTextureDescriptor opaqueDesc = renderingData.cameraData.cameraTargetDescriptor;
                    opaqueDesc.depthBufferBits = 0;
                    cmd.Blit(ngRenderBuffer.GetSource(), ngRenderBuffer.GetTarget(), mRoleSettings.fadeMaterial);
                }
                //执行
                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();
                ngRenderBuffer.SwapBuffer();
                //回收
                ngRenderBuffer.ResetBuffer(context, cmd);
                DrawRenders(context, ref renderingData, customData, true);
            }
            */
           // 
            ngRenderBuffer.FinalCopy(context, cmd);
           
            CommandBufferPool.Release(cmd);
            //ngGrabTexturePass.Execute(this, context, ref renderingData);

        }

        private void DrawRenders(ScriptableRenderContext context, ref RenderingData renderingData, CameraCustomData customData, bool focusLayer=false)
        {
            CameraCustomData.RoleSettingEnum roleSetting;
          
            if (customData == null || !customData.enabled)
            {
                roleSetting = CameraCustomData.RoleSettingEnum.MediumQuality;
            }
            else
            {
                roleSetting = customData.Role;
            }

            if(NGGlobalSettings.GlobalRoleState == NGGlobalSettings.RoleState.Depth)
            {
                if (mRoleSettings.depthMaterial != null)
                {
                    //DrawingSettings captureDepthDrawingSetting = CreateDrawingSettings(mPlaneShadowShaderTag, ref renderingData, SortingCriteria.CommonOpaque);
                    //CaptureDepthDrawingSetting.overrideMaterial = mRoleSettings.depthMaterial;
                    //ontext.DrawRenderers(renderingData.cullResults, ref captureDepthDrawingSetting, ref mRoleShadowCasterFilter);
                }
            }
            else
            {
                
                if (customData == null || customData.Outline == null)
                {
                    cmd.SetGlobalFloat("_OutLineWidth", 0.1f);
                    cmd.SetGlobalFloat("_CameraDisStrength", 0.2f);
                    cmd.SetGlobalFloat("_OutLineMin", 0.5f);
                    cmd.SetGlobalFloat("_OutLineMax", 2.0f);
                }
                else
                {
                    cmd.SetGlobalFloat("_OutLineWidth", customData.Outline.OutLineWidth);
                    cmd.SetGlobalFloat("_CameraDisStrength", customData.Outline.CameraDisStrength);
                    cmd.SetGlobalFloat("_OutLineMin", customData.Outline.OutLineMin);
                    cmd.SetGlobalFloat("_OutLineMax", customData.Outline.OutLineMax);
                }
                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();
                FilteringSettings filterSetting = focusLayer?mRoleFocusFilter:mRoleAndRoleShadowFilter;
                switch (roleSetting)
                {
                    default:
                    case CameraCustomData.RoleSettingEnum.HighQuality:
                   
                        {
                            //DrawingSettings clothBeforeLiquidDrawingSetting = CreateDrawingSettings(mClothBeforeLiquidShaderTag, ref renderingData, SortingCriteria.CommonOpaque);
                            //DrawingSettings roleLiquidDrawingSetting = CreateDrawingSettings(mRoleLiquidShaderTag, ref renderingData, SortingCriteria.CommonTransparent);
                            //DrawingSettings roleEyeGDrawingSetting = CreateDrawingSettings(mEyeGShaderTag, ref renderingData, SortingCriteria.CommonOpaque);

                            DrawingSettings roleForwardDrawingSetting = CreateDrawingSettings(mRoleForwardShaderTag, ref renderingData, SortingCriteria.CommonOpaque);
                            DrawingSettings roleEyeDrawingSetting = CreateDrawingSettings(mRoleEyeShaderTag, ref renderingData, SortingCriteria.CommonOpaque);
                            DrawingSettings roleHairDrawingSetting = CreateDrawingSettings(mRoleHairShaderTag, ref renderingData, SortingCriteria.CommonOpaque);
                            DrawingSettings eyebrowDrawingSetting = CreateDrawingSettings(mEyebrowShaderTag, ref renderingData, SortingCriteria.CommonOpaque);
                            DrawingSettings eyebrowGDrawingSetting = CreateDrawingSettings(mEyebrowGShaderTag, ref renderingData, SortingCriteria.CommonTransparent);
                            //context.DrawRenderers(renderingData.cullResults, ref clothBeforeLiquidDrawingSetting, ref filterSetting);
                            //context.DrawRenderers(renderingData.cullResults, ref roleLiquidDrawingSetting, ref filterSetting);
                            context.DrawRenderers(renderingData.cullResults, ref roleForwardDrawingSetting, ref filterSetting);
                            context.DrawRenderers(renderingData.cullResults, ref roleEyeDrawingSetting, ref filterSetting);
                            context.DrawRenderers(renderingData.cullResults, ref roleHairDrawingSetting, ref filterSetting);
                            context.DrawRenderers(renderingData.cullResults, ref eyebrowDrawingSetting, ref filterSetting);
                          
                            if (mRoleSettings.OpenRoleOutLine)
                            {
                                DrawingSettings clothAndOutlineDrawingSetting = CreateDrawingSettings(mClothAndOutlineShaderTags, ref renderingData, SortingCriteria.CommonTransparent);
                                context.DrawRenderers(renderingData.cullResults, ref clothAndOutlineDrawingSetting, ref filterSetting);
                            }
                            else 
                            {
                                DrawingSettings clothNoOutlineDrawingSetting = CreateDrawingSettings(mClothNoOutlineShaderTags, ref renderingData, SortingCriteria.CommonTransparent);
                                context.DrawRenderers(renderingData.cullResults, ref clothNoOutlineDrawingSetting, ref filterSetting);
                            }
                           // context.DrawRenderers(renderingData.cullResults, ref roleEyeGDrawingSetting, ref filterSetting);
                            context.DrawRenderers(renderingData.cullResults, ref eyebrowGDrawingSetting, ref filterSetting);
                        }
                        break;
                    case CameraCustomData.RoleSettingEnum.MediumQuality:
                        {
                            //DrawingSettings clothBeforeLiquidDrawingSetting = CreateDrawingSettings(mClothBeforeLiquidShaderTag, ref renderingData, SortingCriteria.CommonOpaque);
                            //DrawingSettings roleLiquidDrawingSetting = CreateDrawingSettings(mRoleLiquidShaderTag, ref renderingData, SortingCriteria.CommonTransparent);
                            //DrawingSettings roleEyeGDrawingSetting = CreateDrawingSettings(mEyeGShaderTag, ref renderingData, SortingCriteria.CommonOpaque);

                            DrawingSettings roleForwardDrawingSetting = CreateDrawingSettings(mRoleForwardShaderTag, ref renderingData, SortingCriteria.CommonOpaque);
                            DrawingSettings roleEyeDrawingSetting = CreateDrawingSettings(mRoleEyeShaderTag, ref renderingData, SortingCriteria.CommonOpaque);
                            DrawingSettings roleHairDrawingSetting = CreateDrawingSettings(mRoleHairShaderTag, ref renderingData, SortingCriteria.CommonOpaque);
                            DrawingSettings eyebrowGDrawingSetting = CreateDrawingSettings(mEyebrowGShaderTag, ref renderingData, SortingCriteria.CommonTransparent);
                            DrawingSettings eyebrowDrawingSetting = CreateDrawingSettings(mEyebrowShaderTag, ref renderingData, SortingCriteria.CommonOpaque);
                            //context.DrawRenderers(renderingData.cullResults, ref clothBeforeLiquidDrawingSetting, ref filterSetting);
                            //context.DrawRenderers(renderingData.cullResults, ref roleLiquidDrawingSetting, ref filterSetting);
       
                            context.DrawRenderers(renderingData.cullResults, ref roleForwardDrawingSetting, ref filterSetting);
                       
                            context.DrawRenderers(renderingData.cullResults, ref roleEyeDrawingSetting, ref filterSetting);
                            context.DrawRenderers(renderingData.cullResults, ref roleHairDrawingSetting, ref filterSetting);
       
                            context.DrawRenderers(renderingData.cullResults, ref eyebrowDrawingSetting, ref filterSetting);
                         
                            if (mRoleSettings.OpenRoleOutLine)
                            {
                                DrawingSettings clothAndOutlineDrawingSetting = CreateDrawingSettings(mClothAndOutlineShaderTags, ref renderingData, SortingCriteria.CommonTransparent);
                                context.DrawRenderers(renderingData.cullResults, ref clothAndOutlineDrawingSetting, ref filterSetting);
                            }
                            else
                            {
                                DrawingSettings clothNoOutlineDrawingSetting = CreateDrawingSettings(mClothNoOutlineShaderTags, ref renderingData, SortingCriteria.CommonTransparent);
                                context.DrawRenderers(renderingData.cullResults, ref clothNoOutlineDrawingSetting, ref filterSetting);
                            }
                            //context.DrawRenderers(renderingData.cullResults, ref roleEyeGDrawingSetting, ref filterSetting);
                            context.DrawRenderers(renderingData.cullResults, ref eyebrowGDrawingSetting, ref filterSetting);
                            break;
                        }
                    case CameraCustomData.RoleSettingEnum.LowQuality:
                        {
                            //DrawingSettings clothBeforeLiquidDrawingSetting = CreateDrawingSettings(mClothBeforeLiquidShaderTag, ref renderingData, SortingCriteria.CommonOpaque);
                            //DrawingSettings roleLiquidDrawingSetting = CreateDrawingSettings(mRoleLiquidShaderTag, ref renderingData, SortingCriteria.CommonTransparent);
                            DrawingSettings roleForwardDrawingSetting = CreateDrawingSettings(mRoleForwardShaderTag, ref renderingData, SortingCriteria.CommonOpaque);
                            //DrawingSettings roleEyeDrawingSetting = CreateDrawingSettings(mRoleEyeShaderTag, ref renderingData, SortingCriteria.CommonOpaque);
                            DrawingSettings roleHairDrawingSetting = CreateDrawingSettings(mRoleHairShaderTag, ref renderingData, SortingCriteria.CommonOpaque);
                           // DrawingSettings eyebrowGDrawingSetting = CreateDrawingSettings(mEyebrowGShaderTag, ref renderingData, SortingCriteria.CommonOpaque);
                           // DrawingSettings eyebrowDrawingSetting = CreateDrawingSettings(mEyebrowShaderTag, ref renderingData, SortingCriteria.CommonOpaque);
                            //DrawingSettings clothNoOutlineDrawingSetting = CreateDrawingSettings(mClothNoOutlineShaderTags, ref renderingData, SortingCriteria.CommonTransparent);
                            //DrawingSettings clothAndOutlineDrawingSetting = CreateDrawingSettings(mClothAndOutlineShaderTags, ref renderingData, SortingCriteria.CommonTransparent);
                            //context.DrawRenderers(renderingData.cullResults, ref clothBeforeLiquidDrawingSetting, ref filterSetting);
                            //context.DrawRenderers(renderingData.cullResults, ref roleLiquidDrawingSetting, ref filterSetting);
                            context.DrawRenderers(renderingData.cullResults, ref roleForwardDrawingSetting, ref filterSetting);
                            //context.DrawRenderers(renderingData.cullResults, ref roleEyeDrawingSetting, ref filterSetting);
                            context.DrawRenderers(renderingData.cullResults, ref roleHairDrawingSetting, ref filterSetting);
                            if (mRoleSettings.OpenRoleOutLine)
                            {
                                DrawingSettings clothAndOutlineDrawingSetting = CreateDrawingSettings(mClothAndOutlineShaderTags, ref renderingData, SortingCriteria.CommonTransparent);
                                context.DrawRenderers(renderingData.cullResults, ref clothAndOutlineDrawingSetting, ref filterSetting);
                            }
                            else
                            {
                                DrawingSettings clothNoOutlineDrawingSetting = CreateDrawingSettings(mClothNoOutlineShaderTags, ref renderingData, SortingCriteria.CommonTransparent);
                                context.DrawRenderers(renderingData.cullResults, ref clothNoOutlineDrawingSetting, ref filterSetting);
                            }
                            /*
                            DrawingSettings clothBeforeLiquidLowDrawingSetting;
                            //   DrawingSettings roleLiquidDrawingSetting;
                            DrawingSettings roleForwardLowDrawingSetting;
                            DrawingSettings eyebrowGLowDrawingSetting;
                            DrawingSettings clothLowDrawingSetting;
                            switch (FuntoyGlobalSettings.GlobalRoleState)
                            {
                                case FuntoyGlobalSettings.RoleState.BaseOnly:
                                    clothBeforeLiquidLowDrawingSetting = CreateDrawingSettings(mClothBeforeLiquidLowShaderTag, ref renderingData, SortingCriteria.CommonOpaque);
                                    //    roleLiquidDrawingSetting = CreateDrawingSettings(mRoleLiquidShaderTag, ref renderingData, SortingCriteria.CommonTransparent);
                                    roleForwardLowDrawingSetting = CreateDrawingSettings(mRoleForwardLowShaderTag, ref renderingData, SortingCriteria.CommonOpaque);
                                    eyebrowGLowDrawingSetting = CreateDrawingSettings(mEyebrowGLowShaderTag, ref renderingData, SortingCriteria.CommonOpaque);
                                    clothLowDrawingSetting = CreateDrawingSettings(mClothLowBaseOnlyShaderTags, ref renderingData, SortingCriteria.CommonTransparent);
                                    break;
                                case FuntoyGlobalSettings.RoleState.Teleport:
                                    clothBeforeLiquidLowDrawingSetting = CreateDrawingSettings(mClothBeforeLiquidTeleportShaderTag, ref renderingData, SortingCriteria.CommonOpaque);
                                    //   roleLiquidDrawingSetting = CreateDrawingSettings(mRoleLiquidTeleportShaderTag, ref renderingData, SortingCriteria.CommonTransparent);
                                    roleForwardLowDrawingSetting = CreateDrawingSettings(mRoleForwardTeleportShaderTag, ref renderingData, SortingCriteria.CommonOpaque);
                                    eyebrowGLowDrawingSetting = CreateDrawingSettings(mEyebrowGTeleportShaderTag, ref renderingData, SortingCriteria.CommonOpaque);
                                    clothLowDrawingSetting = CreateDrawingSettings(mClothTeleportAndOutlineShaderTags, ref renderingData, SortingCriteria.CommonTransparent);
                                    break;
                                case FuntoyGlobalSettings.RoleState.VertexOffset:
                                    clothBeforeLiquidLowDrawingSetting = CreateDrawingSettings(mClothBeforeLiquidVertexOffsetShaderTag, ref renderingData, SortingCriteria.CommonOpaque);
                                    //    roleLiquidDrawingSetting = CreateDrawingSettings(mRoleLiquidVertexOffsetShaderTag, ref renderingData, SortingCriteria.CommonTransparent);
                                    roleForwardLowDrawingSetting = CreateDrawingSettings(mRoleForwardVertexOffsetShaderTag, ref renderingData, SortingCriteria.CommonOpaque);
                                    eyebrowGLowDrawingSetting = CreateDrawingSettings(mEyebrowGVertexOffsetShaderTag, ref renderingData, SortingCriteria.CommonOpaque);
                                    clothLowDrawingSetting = CreateDrawingSettings(mClothVertexOffsetAndOutlineShaderTags, ref renderingData, SortingCriteria.CommonTransparent);
                                    break;
                                default:
                                    clothBeforeLiquidLowDrawingSetting = CreateDrawingSettings(mClothBeforeLiquidLowShaderTag, ref renderingData, SortingCriteria.CommonOpaque);
                                    //    roleLiquidDrawingSetting = CreateDrawingSettings(mRoleLiquidShaderTag, ref renderingData, SortingCriteria.CommonTransparent);
                                    roleForwardLowDrawingSetting = CreateDrawingSettings(mRoleForwardLowShaderTag, ref renderingData, SortingCriteria.CommonOpaque);
                                    eyebrowGLowDrawingSetting = CreateDrawingSettings(mEyebrowGLowShaderTag, ref renderingData, SortingCriteria.CommonOpaque);
                                    clothLowDrawingSetting = CreateDrawingSettings(mClothLowAndOutlineShaderTags, ref renderingData, SortingCriteria.CommonTransparent);
                                    break;
                            }
                            
                            //DrawingSettings planeShadowDrawingSetting = CreateDrawingSettings(mPlaneShadowShaderTag, ref renderingData, SortingCriteria.CommonOpaque);
                            context.DrawRenderers(renderingData.cullResults, ref clothBeforeLiquidLowDrawingSetting, ref mRoleAndRoleShadowFilter);
                            //  context.DrawRenderers(renderingData.cullResults, ref roleLiquidDrawingSetting, ref mRoleAndRoleShadowFilter);
                            context.DrawRenderers(renderingData.cullResults, ref roleForwardLowDrawingSetting, ref mRoleAndRoleShadowFilter);
                            context.DrawRenderers(renderingData.cullResults, ref eyebrowGLowDrawingSetting, ref mRoleAndRoleShadowFilter);
                            context.DrawRenderers(renderingData.cullResults, ref clothLowDrawingSetting, ref mRoleAndRoleShadowFilter);
                            
                            //if (FuntoyGlobalSettings.GlobalRoleState != FuntoyGlobalSettings.RoleState.BaseOnly)
                            //{
                            //    context.DrawRenderers(renderingData.cullResults, ref planeShadowDrawingSetting, ref mRoleAndRoleShadowFilter);
                            //}
                            */
                        }
                        break;
                        
                    }
              //  this.DrawRenders(customData, roleSetting,  renderingData, false);
              //  this.DrawRenders(customData, roleSetting, renderingData, true);
            }  
        }
    }

   
            
    

    public RoleSettings settings = new RoleSettings();
    RoleRenderPass m_ScriptablePass;
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        m_ScriptablePass.Setup(renderer.cameraColorTarget);
        renderer.EnqueuePass(m_ScriptablePass);
    }

    public override void Create()
    {
        m_ScriptablePass = new RoleRenderPass(settings);
        m_ScriptablePass.renderPassEvent = RenderPassEvent.AfterRenderingSkybox + 1;// settings.renderPassEvent;
    }
}