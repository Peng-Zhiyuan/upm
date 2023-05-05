using UnityEngine;
using UnityEditor;
using System;
using System.IO;

public class PunishingInspector : ShaderGUI
{
    RenderingOptions renderingOption;
    SpecularOptions specularOptions;
    RimOptions rimOptions;
    //BasicRimOptions basicRimOptions;
    VersionOptions versionOption;
    ColorPaletteOptions colorPaletteOptions;
    private string[] customPropertiesList = { "_UseLowQuality","_VersionMode", "_SrcBlendNG", "_DstBlendNG", "_Cull", "_UseShadowColorTexture", "_UseEmission", "_UseRampTexture", "_Albedo",
    "_BaseColor","_ShadowColorTexture","_Normal","_OcclusionMap","_Specular","_Emission","_Ramp","_Distort","_DistortSpeed","_SColor","_RampThresholdSub","_RampThreshold","_RampSmooth",
    "_OcclusionAddThreshold","_OcclusionAddIntensity","_OcclusionSubIntensity","_OcclusionShadowIntensity", "_OcclusionShadowThreshold","_Bias","_SpecularColor","_Glossiness","_Hardness","_SpecularIntensity","_FresnelThickness",
        "_ReflectionIntensity","_OutLineWidth","_OutLineColor","_CameraDisStrength","_OutLineZOffset", "_OutLineWidthOffset","_UseVertexColor","_OutlineUV2AsNormals","_EmissionColor","_EmissionAnim","_BloomFactor","_UseMatcapEffect","_UseTeleportEffect",
    "_OutLineCull","_UseMetallic","_Metallic","_MetallicIntensity","_MetallicMap","_GlossinessMap","_ColorPalette","_SSColor","_SRampThreshold","_SecondRampThresholdIntensity","_SecondRampVisableThreshold",
        "_UseReflectionMap","_ReflectionMap","_UseCartoonSpecular","nbSamples","_QualityMode","_Roughness","_LightMap","_OutLineMin","_OutLineMax","_OneMinusReflectivityIntensity",
        "_UseDitheredTransparentWithTexture","_Dither","_DitherScale","environment_rotation","_AnimationOffsetX","_AnimationOffsetY","_UseAnimation",
    "_FlipBackFaceNormal","_UseViewPortClip","_ReciveShadow", "_UseHSVShadowColor","_CartoonMetal","_UseCartoonMetal", "_UseMask", "_Use_SDF","_UseStock","_OffsetPosition", "_ShadowThreshold",
      "_OffsetProcess", "_Flip", "_UseDither", "_Eyebrow", "_RotateSpecular", "_QuiverSpecular", "_TearfulSpecular", "_FaceUp", "_FaceFront", "_RolePosition", "_RoleFront",  "_RoleUp", "_AsymmetricalSpecular"};



    private string[] liquidPropertiesList = {"_LiquidHeight","_SectionColor","_WaterColor", "_WaterColor2", "_FoamColor","_FoamHeight","_WaveHeight","_ForceDir","_WaterGradualRate","_CenterPosition","_HeightScale"};

    private string[] auraPropertiesList = { "_Use_Aura_Outline", "_AuraColor", "_AuraRimColor", "_AuraOutline", "_AuraOutlineZ", "_AuraNoiseTex", "_AuraNoiseScale", "_AuraSpeedX", "_AuraSpeedY", "_AuraOpacity", "_AuraBrightness", "_AuraEdge", "_AuraRimPower" };

    private string[] rimPropertiesList = { "_UseRim", "_RimColor", "_RimWidth", "_RimMin", "_RimMax", "_RimThreshold"};

     private string[] flowMapPropertiesList = { "_UseFlowMap", "_FlowMap", "_FlowMask", "_FlowSpeed", "_FlowColor", "_FlowStrength" };

    private string[] sssPropertiesList = { "_UseSSS", "_SSSColor", "_SSSIntensity", "_SSSAtten"};

    private string[] colorPalettePropertiesList = { "_UseColorPalette","_Region1Color", "_Region2Color", "_Region3Color","_Region4Color", "_Region2SColor",  "_Region3SColor",  "_Region4SColor", "_Region2SSColor", "_Region3SSColor", "_Region4SSColor" };

    private string[] stockingPropertiesList = { "_StockingGlossiness", "_StockingDenier", "_StockingDenierTex", "_StockingRimPower", "_StockingFresnelScale", "_StockingFresnelSmooth", "_StockingFresnelThreshould", "_StockingTint","_StockingFresnelMin", "_StockingSpecular", "_StockingSpecularSmooth", "_StockingSpecularThreshould", };

    private string[] ShadowColorHSVList = {"_ShadowThreshould",  "_ShadowColorH","_ShadowColorS", "_ShadowColorV","_ShadowThreshould2", "_ShadowColorH2", "_ShadowColorS2", "_ShadowColorV2" };
    
    private string[] CartoonMetalList = {"_CartoonMetalUVrotate"};//{ "_RoleEulerAnglesY", "_CartoonMetalUVrotate", "_OffsetPosition", "_OffsetProcess"};

    private string[] FresnelEffPropertiesList = {"_FresnelEffect", "_FresnelColor", "_FresnelStart" };
    private string[] DissolveEffPropertiesList = {"_DissolveEffect", "_DissolveColor",  "_DissolveRatio", "_DissolveHeight", "_DissolveRange" };
    //private string[] ExtendEffects = { "_FresnelEffect", "_FresnelColor", "_FresnelIntensity", "_DissolveEffect", "_DissolveIntensity", "_DissolveHeight" };
    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        Material targetMat = materialEditor.target as Material;
        versionOption = VersionOptions.Full;
        if (TryGetProperty(properties, "_VersionMode", out MaterialProperty VersionMode))
        {
            if (VersionMode.floatValue > 0)
            {
                versionOption = VersionOptions.Lite;
            }
            else
            {
                versionOption = VersionOptions.Full;
            }
            EditorGUI.BeginChangeCheck();
            versionOption = (VersionOptions)EditorGUILayout.EnumPopup(new GUIContent("Version Mode", "Lite模式为精简版参数，Full模式为完整版参数"), versionOption);
            if (EditorGUI.EndChangeCheck())
            {
                OnVersionModeChange(targetMat);
            }
        }
        specularOptions = 0;
        if (TryGetProperty(properties, "_UseMetallic", out MaterialProperty SpecularMode))
        {
            if (SpecularMode.floatValue > 0)
            {
                specularOptions = SpecularOptions.Metallic;
            }
            else
            {
                specularOptions = SpecularOptions.Specular;
            }
            EditorGUI.BeginChangeCheck();
            specularOptions = (SpecularOptions)EditorGUILayout.EnumPopup(new GUIContent("Specular Mode", "Specular为高光工作流，Metallic为金属工作流"), specularOptions);
            if (EditorGUI.EndChangeCheck())
            {
                OnSpecularModeChange(targetMat);
            }
        }
        renderingOption = RenderingOptions.Opaque;
        bool hasBlend = false;

        if (TryGetProperty(properties, "_SrcBlendNG", out MaterialProperty SrcBlendNG))
        {

            if (TryGetProperty(properties, "_DstBlendNG", out MaterialProperty DstBlendNG))
            {
               // Debug.Log("_DstBlendNG");
                hasBlend = true;
                if (SrcBlendNG.floatValue == (int)UnityEngine.Rendering.BlendMode.SrcAlpha && DstBlendNG.floatValue == (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha)
                {
                    renderingOption = RenderingOptions.Transparent;
                }
            }
        }

        if (hasBlend)
        {

            EditorGUI.BeginChangeCheck();
            renderingOption = (RenderingOptions)EditorGUILayout.EnumPopup(new GUIContent("Rendering Mode", "Opaque为不透明，Transparent为透明"), renderingOption);
            if (EditorGUI.EndChangeCheck())
            {
                OnRenderingModeChange(targetMat);
            }
        }

        if (TryGetProperty(properties, "_Cull", out MaterialProperty Cull))
        {
            materialEditor.ShaderProperty(Cull, new GUIContent(Cull.displayName, "裁切模式，默认为Back表示不显示背面，Front则为不显示正面，Off则表示双面显示。"));
        }

        NG_GUI.Separator();
        EditorGUILayout.LabelField("Main Maps", new GUIStyle(EditorStyles.boldLabel));
        MaterialProperty UseLowQualityProperty;
        if (TryGetProperty(properties, "_UseLowQuality", out UseLowQualityProperty))
        {
            materialEditor.ShaderProperty(UseLowQualityProperty, new GUIContent(UseLowQualityProperty.displayName, "开启低画质模式"));
        }
        MaterialProperty UseShadowColorTextureProperty;
        if (TryGetProperty(properties, "_UseShadowColorTexture", out UseShadowColorTextureProperty))
        {
            materialEditor.ShaderProperty(UseShadowColorTextureProperty, new GUIContent(UseShadowColorTextureProperty.displayName, "开启是否使用阴影颜色贴图"));
        }
        MaterialProperty UseEmissionProperty;
        if (TryGetProperty(properties, "_UseEmission", out UseEmissionProperty))
        {
            materialEditor.ShaderProperty(UseEmissionProperty, new GUIContent(UseEmissionProperty.displayName, "开启是否使用自发光"));
        }
        MaterialProperty UseRampTextureProperty;
        if (TryGetProperty(properties, "_UseRampTexture", out UseRampTextureProperty))
        {
            materialEditor.ShaderProperty(UseRampTextureProperty, new GUIContent(UseRampTextureProperty.displayName, "开启是否使用Ramp贴图"));
        }
        MaterialProperty UseDitherProerty;
        if (TryGetProperty(properties, "_UseDither", out UseDitherProerty))
        {
            materialEditor.ShaderProperty(UseDitherProerty, new GUIContent(UseDitherProerty.displayName, "使用半透明裁剪"));
        }
        MaterialProperty FlipBackFaceNormalProperty;
        if (TryGetProperty(properties, "_FlipBackFaceNormal", out FlipBackFaceNormalProperty))
        {
            materialEditor.ShaderProperty(FlipBackFaceNormalProperty, new GUIContent(FlipBackFaceNormalProperty.displayName, "是否反转背面法线方向"));
        }
        if (TryGetProperty(properties, "_Albedo", out MaterialProperty AlbedoProperty))
        {
            EditorGUI.BeginChangeCheck();
            if (TryGetProperty(properties, "_BaseColor", out MaterialProperty BaseColorProperty))
            {
                materialEditor.TexturePropertyWithHDRColor(new GUIContent(AlbedoProperty.displayName, "基本色贴图"), AlbedoProperty, BaseColorProperty, true);
            }
            else
            {
                materialEditor.TexturePropertySingleLine(new GUIContent(AlbedoProperty.displayName, "基本色贴图"), AlbedoProperty);
            }
            if (EditorGUI.EndChangeCheck())
            {
                if (targetMat.shader.name.Contains("Face") || targetMat.shader.name.Contains("Body") || targetMat.shader.name.Contains("Eyebrow"))
                {
                    OnFixTextureFormat(targetMat, "_Albedo", 1024, true, true, false, true, TextureWrapMode.Repeat);
                }
                else
                {
                    OnFixTextureFormat(targetMat, "_Albedo", 1024, true, true, true, true, TextureWrapMode.Repeat);
                }
            }
            materialEditor.TextureScaleOffsetProperty(AlbedoProperty);
        }
        if (UseShadowColorTextureProperty != null)
        {
            if (UseShadowColorTextureProperty.floatValue > 0)
            {
                if (TryGetProperty(properties, "_ShadowColorTexture", out MaterialProperty ShadowColorTextureProperty))
                {
                    EditorGUI.BeginChangeCheck();
                    materialEditor.TexturePropertySingleLine(new GUIContent(ShadowColorTextureProperty.displayName, "阴影颜色贴图"), ShadowColorTextureProperty);
                    if (EditorGUI.EndChangeCheck())
                    {
                        OnFixTextureFormat(targetMat, "_ShadowColorTexture", 1024, true, true, true, false, TextureWrapMode.Repeat);
                    }
                }
                targetMat.EnableKeyword("_USE_SHADOW_COLOR_TEXTURE");
            }
            else
            {
                targetMat.DisableKeyword("_USE_SHADOW_COLOR_TEXTURE");
            }
        }
        /* MaterialProperty ColorPaletteProerty;
         if (TryGetProperty(properties, "_USE_HSV_SHADOW", out ColorPaletteProerty))
         {
             materialEditor.ShaderProperty(ColorPaletteProerty, new GUIContent(ColorPaletteProerty.displayName, "开启色相偏移"));
         }
         if (ColorPaletteProerty != null)
         {
             if (ColorPaletteProerty.floatValue > 0)
             {
                 if (TryGetProperty(properties, "_ShadowColorTexture", out MaterialProperty ShadowColorTextureProperty))
                 {
                     EditorGUI.BeginChangeCheck();
                     materialEditor.TexturePropertySingleLine(new GUIContent(ShadowColorTextureProperty.displayName, "阴影颜色贴图"), ShadowColorTextureProperty);
                     if (EditorGUI.EndChangeCheck())
                     {
                         OnFixTextureFormat(targetMat, "_ShadowColorTexture", 1024, true, true, true, false, TextureWrapMode.Repeat);
                     }
                 }
                 targetMat.EnableKeyword("_USE_SHADOW_COLOR_TEXTURE");
             }
             else
             {
                 targetMat.DisableKeyword("_USE_SHADOW_COLOR_TEXTURE");
             }
         }*/
        if (TryGetProperty(properties, "_Normal", out MaterialProperty NormalProperty))
        {
            EditorGUI.BeginChangeCheck();
            materialEditor.TexturePropertySingleLine(new GUIContent(NormalProperty.displayName, "法线贴图"), NormalProperty);
            if (EditorGUI.EndChangeCheck())
            {
                OnFixTextureFormat(targetMat, "_Normal", 512, false, false, true, false, TextureWrapMode.Repeat, true);
            }
        }
        MaterialProperty EyebrowProperty;
        if (TryGetProperty(properties, "_Eyebrow", out EyebrowProperty))
        {
            materialEditor.ShaderProperty(EyebrowProperty, new GUIContent(EyebrowProperty.displayName, "绘制眉毛"));
        }


        // if (UseLowQualityProperty == null || UseLowQualityProperty.floatValue == 0)
        // {
        if (TryGetProperty(properties, "_OcclusionMap", out MaterialProperty OcclusionMapProperty))
        {
            if (targetMat.shader.name.Contains("Face"))
            {
                MaterialProperty USESDFProperty;
                if (TryGetProperty(properties, "_Use_SDF", out USESDFProperty))
                {
                    materialEditor.ShaderProperty(USESDFProperty, new GUIContent(USESDFProperty.displayName, "有向距离场贴图"));
                }

                if (USESDFProperty.floatValue > 0)
                {
                    if (TryGetProperty(properties, "_Flip", out MaterialProperty USEFlipProperty))
                    {
                        materialEditor.ShaderProperty(USEFlipProperty, new GUIContent(USEFlipProperty.displayName, "_Flip"));
                    }
                }
            }
            EditorGUI.BeginChangeCheck();
            if (targetMat.shader.name.Contains("Cloth"))
            {
                materialEditor.TexturePropertySingleLine(new GUIContent(OcclusionMapProperty.displayName, "r为2阶色3阶色控制贴图，g为阴影修正贴图，b为自发光贴图,a为裁切遮罩"), OcclusionMapProperty);
            }
            else
            {
                materialEditor.TexturePropertySingleLine(new GUIContent(OcclusionMapProperty.displayName, "r为高光遮罩贴图，g为阴影修正贴图"), OcclusionMapProperty);
            }
            if (EditorGUI.EndChangeCheck())
            {
                OnFixTextureFormat(targetMat, "_OcclusionMap", 512, false, true, true, true, TextureWrapMode.Repeat);
            }


            /*
            if (TryGetProperty(properties, "_LightMap", out MaterialProperty LightMapProperty))
            {
                EditorGUI.BeginChangeCheck();
                materialEditor.TexturePropertySingleLine(new GUIContent(LightMapProperty.displayName, "r为高光遮罩贴图，g为阴影修正贴图"), LightMapProperty);
                if (EditorGUI.EndChangeCheck())
                {
                    OnFixTextureFormat(targetMat, "_LightMap", 512, false, true, true, true, TextureWrapMode.Repeat);
                }
            }
            */
        }
        //}


        switch (specularOptions)
        {
            case SpecularOptions.Specular:
                if (TryGetProperty(properties, "_Specular", out MaterialProperty SpecularProperty))
                {
                    EditorGUI.BeginChangeCheck();
                    materialEditor.TexturePropertySingleLine(new GUIContent(SpecularProperty.displayName, "高光贴图，rgb为高光颜色，a为光滑度"), SpecularProperty);
                    if (EditorGUI.EndChangeCheck())
                    {
                        OnFixTextureFormat(targetMat, "_Specular", 512, true, true, true, true, TextureWrapMode.Repeat);
                    }
                }
                break;
            case SpecularOptions.Metallic:
                {
                    if (TryGetProperty(properties, "_MetallicMap", out MaterialProperty MetallicMapProperty))
                    {
                        EditorGUI.BeginChangeCheck();
                        materialEditor.TexturePropertySingleLine(new GUIContent(MetallicMapProperty.displayName, "金属贴图，r为金属度，g为粗糙度"), MetallicMapProperty);
                        if (EditorGUI.EndChangeCheck())
                        {
                            OnFixTextureFormat(targetMat, "_MetallicMap", 512, false, true, true, true, TextureWrapMode.Repeat);
                        }
                    }
                }
                break;
            default:
                {
                    if (TryGetProperty(properties, "_MetallicMap", out MaterialProperty MetallicMapProperty))
                    {
                        EditorGUI.BeginChangeCheck();
                        materialEditor.TexturePropertySingleLine(new GUIContent(MetallicMapProperty.displayName, "金属贴图，r为金属度，g为粗糙度"), MetallicMapProperty);
                        if (EditorGUI.EndChangeCheck())
                        {
                            OnFixTextureFormat(targetMat, "_MetallicMap", 512, false, true, true, true, TextureWrapMode.Repeat);
                        }
                    }
                }
                break;
        }

        if (UseEmissionProperty != null)
        {
            if (UseEmissionProperty.floatValue > 0)
            {
                if (TryGetProperty(properties, "_Emission", out MaterialProperty EmissionProperty))
                {
                    EditorGUI.BeginChangeCheck();
                    materialEditor.TexturePropertySingleLine(new GUIContent(EmissionProperty.displayName, "r通道为自发光遮罩"), EmissionProperty);
                    if (EditorGUI.EndChangeCheck())
                    {
                        OnFixTextureFormat(targetMat, "_Emission", 512, false, true, true, false, TextureWrapMode.Repeat);
                    }
                }
                targetMat.EnableKeyword("_USE_EMISSION");
            }
            else
            {
                targetMat.DisableKeyword("_USE_EMISSION");
            }
        }
        if (UseRampTextureProperty == null || UseRampTextureProperty.floatValue > 0)
        {
            if (TryGetProperty(properties, "_Ramp", out MaterialProperty RampProperty))
            {
                EditorGUI.BeginChangeCheck();
                materialEditor.TexturePropertySingleLine(new GUIContent(RampProperty.displayName, "Ramp贴图"), RampProperty);
                if (EditorGUI.EndChangeCheck())
                {
                    OnFixTextureFormat(targetMat, "_Ramp", 512, false, true, true, false, TextureWrapMode.Clamp);
                }
            }
        }

        if (TryGetProperty(properties, "_UseAnimation", out MaterialProperty UseAnimationProperty))
        {
            NG_GUI.Separator();
            EditorGUILayout.LabelField("Animation", new GUIStyle(EditorStyles.boldLabel));
            materialEditor.ShaderProperty(UseAnimationProperty, new GUIContent(UseAnimationProperty.displayName, "开启关闭动画逐帧模式"));
            if (UseAnimationProperty.floatValue > 0)
            {
                if (TryGetProperty(properties, "_AnimationOffsetX", out MaterialProperty AnimationOffsetXProperty))
                {
                    materialEditor.ShaderProperty(AnimationOffsetXProperty, new GUIContent(AnimationOffsetXProperty.displayName, "动画每帧u偏移"));
                }
                if (TryGetProperty(properties, "_AnimationOffsetY", out MaterialProperty AnimationOffsetYProperty))
                {
                    materialEditor.ShaderProperty(AnimationOffsetYProperty, new GUIContent(AnimationOffsetYProperty.displayName, "动画每帧v偏移"));
                }
            }
        }

        if (TryGetProperty(properties, "_Distort", out MaterialProperty DistortProperty))
        {
            EditorGUI.BeginChangeCheck();
            materialEditor.TexturePropertySingleLine(new GUIContent(DistortProperty.displayName, "Distort贴图"), DistortProperty);
            if (EditorGUI.EndChangeCheck())
            {
                OnFixTextureFormat(targetMat, "_Distort", 512, true, true, true, true, TextureWrapMode.Repeat);
            }
        }

        NG_GUI.Separator();
        if (TryGetProperty(properties, "_DistortSpeed", out MaterialProperty DistortSpeedProperty))
        {
            EditorGUILayout.LabelField("Distort", new GUIStyle(EditorStyles.boldLabel));
            materialEditor.ShaderProperty(DistortSpeedProperty, new GUIContent(DistortSpeedProperty.displayName, "扭曲位移速度"));
            NG_GUI.Separator();
        }
        if (TryGetProperty(properties, "_LiquidHeight", out MaterialProperty LiquidHeightProperty))
        {
            EditorGUILayout.LabelField("Liquid Setting", new GUIStyle(EditorStyles.boldLabel));
            materialEditor.ShaderProperty(LiquidHeightProperty, new GUIContent(LiquidHeightProperty.displayName, "液体的高度"));

            if (TryGetProperty(properties, "_SectionColor", out MaterialProperty SectionColorProperty))
            {
                materialEditor.ShaderProperty(SectionColorProperty, new GUIContent(SectionColorProperty.displayName, "截面的颜色"));
            }

            if (TryGetProperty(properties, "_WaterColor", out MaterialProperty WaterColorProperty))
            {
                materialEditor.ShaderProperty(WaterColorProperty, new GUIContent(WaterColorProperty.displayName, "液体的颜色"));
            }

            if (TryGetProperty(properties, "_WaterColor2", out MaterialProperty WaterColor2Property))
            {
                materialEditor.ShaderProperty(WaterColor2Property, new GUIContent(WaterColor2Property.displayName, "液体的颜色2"));
            }

            if (TryGetProperty(properties, "_WaterGradualRate", out MaterialProperty WaterGradualRateProperty))
            {
                materialEditor.ShaderProperty(WaterGradualRateProperty, new GUIContent(WaterGradualRateProperty.displayName, "液体的颜色渐变阀值"));
            }

            if (TryGetProperty(properties, "_FoamColor", out MaterialProperty FoamColorProperty))
            {
                materialEditor.ShaderProperty(FoamColorProperty, new GUIContent(FoamColorProperty.displayName, "泡沫的颜色"));
            }

            if (TryGetProperty(properties, "_FoamHeight", out MaterialProperty FoamHeightProperty))
            {
                materialEditor.ShaderProperty(FoamHeightProperty, new GUIContent(FoamHeightProperty.displayName, "泡沫的高度"));
            }

            if (TryGetProperty(properties, "_WaveHeight", out MaterialProperty WaveHeightProperty))
            {
                materialEditor.ShaderProperty(WaveHeightProperty, new GUIContent(WaveHeightProperty.displayName, "波浪的高度"));
            }

            NG_GUI.Separator();
        }

        // if(renderingOption == RenderingOptions.Opaque)
        //  {
        if (TryGetProperty(properties, "_Dither", out MaterialProperty DitherProperty))
        {
            EditorGUILayout.LabelField("Dither", new GUIStyle(EditorStyles.boldLabel));
            EditorGUI.BeginChangeCheck();
            materialEditor.TexturePropertySingleLine(new GUIContent(DitherProperty.displayName, "羽化贴图"), DitherProperty);
            if (EditorGUI.EndChangeCheck())
            {
                OnFixTextureFormat(targetMat, "_Dither", 1024, false, false, false, false, TextureWrapMode.Repeat);
            }
            if (TryGetProperty(properties, "_DitherScale", out MaterialProperty DitherScaleProperty))
            {
                materialEditor.ShaderProperty(DitherScaleProperty, new GUIContent(DitherScaleProperty.displayName, "羽化贴图的UV缩放"));
            }
            NG_GUI.Separator();
        }

        //  }

        colorPaletteOptions = ColorPaletteOptions.Off;
        if (TryGetProperty(properties, "_UseHSVShadowColor", out MaterialProperty useHSVShadowColor))
        {
            EditorGUILayout.LabelField("Color Palette", new GUIStyle(EditorStyles.boldLabel));

            if (useHSVShadowColor.floatValue == 1)
            {
                colorPaletteOptions = ColorPaletteOptions.HSV;
            }
            else if (useHSVShadowColor.floatValue == 2)
            {
                colorPaletteOptions = ColorPaletteOptions.Normal;
            }
            else
            {
                colorPaletteOptions = ColorPaletteOptions.Off;
            }

            EditorGUI.BeginChangeCheck();
            colorPaletteOptions = (ColorPaletteOptions)EditorGUILayout.EnumPopup(new GUIContent("Color Palette Mode", "HSV为使用HSV阴影，Normal为常规阴影"), colorPaletteOptions);
            if (EditorGUI.EndChangeCheck())
            {
                OnColorPaletteModeChange(targetMat);
            }
            if (colorPaletteOptions != ColorPaletteOptions.Off)
            {
                if (TryGetProperty(properties, "_ColorPalette", out MaterialProperty ColorPaletteProperty))
                {
                    EditorGUI.BeginChangeCheck();
                    materialEditor.TexturePropertySingleLine(new GUIContent(ColorPaletteProperty.displayName, "调色板贴图"), ColorPaletteProperty);
                    if (EditorGUI.EndChangeCheck())
                    {
                        OnFixTextureFormat(targetMat, "_ColorPalette", 512, false, true, true, false, TextureWrapMode.Repeat);
                    }
                }

                if (TryGetProperty(properties, "_UseColorPalette", out MaterialProperty UseColorPaletteProperty))
                {
                    materialEditor.ShaderProperty(UseColorPaletteProperty, new GUIContent(UseColorPaletteProperty.displayName, "启用色调变化"));
                }

                if (TryGetProperty(properties, "_Region1Color", out MaterialProperty Region1ColorProperty))
                {
                    materialEditor.ShaderProperty(Region1ColorProperty, new GUIContent(Region1ColorProperty.displayName, "区域1颜色"));
                }
                if (TryGetProperty(properties, "_Region2Color", out MaterialProperty Region2ColorProperty))
                {
                    materialEditor.ShaderProperty(Region2ColorProperty, new GUIContent(Region2ColorProperty.displayName, "区域2颜色"));
                }
                if (TryGetProperty(properties, "_Region3Color", out MaterialProperty Region3ColorProperty))
                {
                    materialEditor.ShaderProperty(Region3ColorProperty, new GUIContent(Region3ColorProperty.displayName, "区域3颜色"));
                }
                if (TryGetProperty(properties, "_Region4Color", out MaterialProperty Region4ColorProperty))
                {
                    materialEditor.ShaderProperty(Region4ColorProperty, new GUIContent(Region4ColorProperty.displayName, "区域4颜色"));
                }
            }
            NG_GUI.Separator();
        }
        EditorGUILayout.LabelField("Shadow", new GUIStyle(EditorStyles.boldLabel));

        switch (colorPaletteOptions)
        {
            case ColorPaletteOptions.Off:
                {
                    if (TryGetProperty(properties, "_SColor", out MaterialProperty SColorProperty))
                    {
                        materialEditor.ShaderProperty(SColorProperty, new GUIContent(SColorProperty.displayName, "阴影颜色"));
                    }
                    if (TryGetProperty(properties, "_SSColor", out MaterialProperty SSColorProperty))
                    {
                        materialEditor.ShaderProperty(SSColorProperty, new GUIContent(SSColorProperty.displayName, "阴影颜色"));
                    }
                }
                break;
            case ColorPaletteOptions.HSV:
                ShowProperty("", materialEditor, properties, ShadowColorHSVList);
                break;
            case ColorPaletteOptions.Normal:
                {
                    if (TryGetProperty(properties, "_SColor", out MaterialProperty SColorProperty))
                    {
                        materialEditor.ShaderProperty(SColorProperty, new GUIContent("Region 1 First Shadow Color", "阴影颜色"));
                    }
                    if (TryGetProperty(properties, "_SSColor", out MaterialProperty SSColorProperty))
                    {
                        materialEditor.ShaderProperty(SColorProperty, new GUIContent("Region 1 Second Shadow Color", "阴影颜色"));
                    }
                    if (TryGetProperty(properties, "_Region2SColor", out MaterialProperty Region2SColorProperty))
                    {
                        materialEditor.ShaderProperty(Region2SColorProperty, new GUIContent(Region2SColorProperty.displayName, "区域2阴影颜色"));
                    }
                    if (TryGetProperty(properties, "_Region2SSColor", out MaterialProperty Region2SSColorProperty))
                    {
                        materialEditor.ShaderProperty(Region2SSColorProperty, new GUIContent(Region2SSColorProperty.displayName, "区域2阴影颜色"));
                    }
                    if (TryGetProperty(properties, "_Region3SColor", out MaterialProperty Region3SColorProperty))
                    {
                        materialEditor.ShaderProperty(Region3SColorProperty, new GUIContent(Region3SColorProperty.displayName, "区域3阴影颜色"));
                    }
                    if (TryGetProperty(properties, "_Region3SSColor", out MaterialProperty Region3SSColorProperty))
                    {
                        materialEditor.ShaderProperty(Region3SSColorProperty, new GUIContent(Region3SSColorProperty.displayName, "区域3阴影颜色"));
                    }
                    if (TryGetProperty(properties, "_Region4SColor", out MaterialProperty Region4SColorProperty))
                    {
                        materialEditor.ShaderProperty(Region4SColorProperty, new GUIContent(Region4SColorProperty.displayName, "区域4阴影颜色"));
                    }
                    if (TryGetProperty(properties, "_Region4SSColor", out MaterialProperty Region4SSColorProperty))
                    {
                        materialEditor.ShaderProperty(Region4SSColorProperty, new GUIContent(Region4SSColorProperty.displayName, "区域4阴影颜色"));
                    }
                }
                break;
        }
      
        if (TryGetProperty(properties, "_RampThreshold", out MaterialProperty RampThresholdProperty))
        {
            materialEditor.ShaderProperty(RampThresholdProperty, new GUIContent(RampThresholdProperty.displayName, "阴影的阀值，用来控制阴影的大小。"));
        }
        if (TryGetProperty(properties, "_FRampThreshold", out MaterialProperty FRampThresholdProperty))
        {
            materialEditor.ShaderProperty(FRampThresholdProperty, new GUIContent(FRampThresholdProperty.displayName, "第一层阴影的阀值，用来控制第一层阴影的大小。"));
        }
        if (TryGetProperty(properties, "_RampThresholdSub", out MaterialProperty RampThresholdSubProperty))
        {
            materialEditor.ShaderProperty(RampThresholdSubProperty, new GUIContent(RampThresholdSubProperty.displayName, "阴影递减值，用来控制暗部阴影的大小。"));
        }
        if (TryGetProperty(properties, "_SRampThreshold", out MaterialProperty SRampThresholdProperty))
        {
            materialEditor.ShaderProperty(SRampThresholdProperty, new GUIContent(SRampThresholdProperty.displayName, "第二层阴影的阀值，用来控制第二层阴影的大小。"));
        }
        if (versionOption == VersionOptions.Full)
        {
            if (TryGetProperty(properties, "_SecondRampVisableThreshold", out MaterialProperty SecondRampVisableThresholdProperty))
            {
                materialEditor.ShaderProperty(SecondRampVisableThresholdProperty, new GUIContent(SecondRampVisableThresholdProperty.displayName, "是控制第二层阴影是否显示的阀值，Second Ramp Mask贴图中的颜色大于这个值才会显示第二层阴影。"));
            }
            if (TryGetProperty(properties, "_RampSmooth", out MaterialProperty RampSmoothProperty))
            {
                materialEditor.ShaderProperty(RampSmoothProperty, new GUIContent(RampSmoothProperty.displayName, "阴影的平滑度"));
            }
            if (TryGetProperty(properties, "_OcclusionAddThreshold", out MaterialProperty OcclusionAddThresholdProperty))
            {
                materialEditor.ShaderProperty(OcclusionAddThresholdProperty, new GUIContent(OcclusionAddThresholdProperty.displayName, "修正阴影加强和减弱部分贴图的分隔阀值，低于阀值部分为加强阴影的修正贴图，高于阀值部分为减弱阴影的修正贴图。"));
            }
            if (TryGetProperty(properties, "_OcclusionAddIntensity", out MaterialProperty OcclusionAddIntensityProperty))
            {
                materialEditor.ShaderProperty(OcclusionAddIntensityProperty, new GUIContent(OcclusionAddIntensityProperty.displayName, "是阴影加强的强度，数值越大修正阴影贴图加强部分的效果越强，加强到高于1部分就会变成死阴影。"));
            }
            if (TryGetProperty(properties, "_OcclusionSubIntensity", out MaterialProperty OcclusionSubIntensityProperty))
            {
                materialEditor.ShaderProperty(OcclusionSubIntensityProperty, new GUIContent(OcclusionSubIntensityProperty.displayName, "是阴影减弱的强度，数值越大修正阴影贴图减弱部分的效果越强，加强到高于1部分就会永远不显示阴影。"));
            }

              if (TryGetProperty(properties, "_OcclusionShadowIntensity", out MaterialProperty OcclusionShadowIntensityyProperty))
            {
                materialEditor.ShaderProperty(OcclusionShadowIntensityyProperty, new GUIContent(OcclusionShadowIntensityyProperty.displayName, "暗部阴强度"));
            }
            if (TryGetProperty(properties, "_OcclusionShadowThreshold", out MaterialProperty OcclusionShadowThresholdProperty))
            {
                materialEditor.ShaderProperty(OcclusionShadowThresholdProperty, new GUIContent(OcclusionShadowThresholdProperty.displayName, "暗部阴影阈值"));
            }
        }
        else
        {
            if (TryGetProperty(properties, "_OcclusionAddIntensity", out MaterialProperty OcclusionAddIntensityProperty))
            {
                bool OcclusionAddOpen = OcclusionAddIntensityProperty.floatValue > 0;
                EditorGUI.BeginChangeCheck();
                OcclusionAddOpen = EditorGUILayout.Toggle(new GUIContent("Occlusion Add", "开关阴影修正贴图的加法功能，贴图中低于灰色部分（rgb(128,128,128)）越暗出现阴影越快"), OcclusionAddOpen);
                if (EditorGUI.EndChangeCheck())
                {
                    OnOcclusionAddIntensityToggle(targetMat, OcclusionAddOpen);
                }
            }
            if (TryGetProperty(properties, "_OcclusionSubIntensity", out MaterialProperty OcclusionSubIntensityProperty))
            {
                bool OcclusionSubOpen = OcclusionSubIntensityProperty.floatValue > 0;
                EditorGUI.BeginChangeCheck();
                OcclusionSubOpen = EditorGUILayout.Toggle(new GUIContent("Occlusion Sub", "开关阴影修正贴图的减法功能，贴图中高于灰色部分（rgb(128,128,128)）越亮出现阴影越慢"), OcclusionSubOpen);
                if (EditorGUI.EndChangeCheck())
                {
                    OnOcclusionSubIntensityToggle(targetMat, OcclusionSubOpen);
                }
            }
        }



        if (TryGetProperty(properties, "_ShadowThreshold", out MaterialProperty ShadowThresholdProperty))
        {
            materialEditor.ShaderProperty(ShadowThresholdProperty, new GUIContent(ShadowThresholdProperty.displayName, "面部阴影分界点阈值"));
        }

        if (TryGetProperty(properties, "_UseMask", out MaterialProperty UseMaskProperty))
        {
            materialEditor.ShaderProperty(UseMaskProperty, new GUIContent(UseMaskProperty.displayName, "面部阴影遮罩开关"));
        }
        NG_GUI.Separator();
        /*
        EditorGUILayout.LabelField("ReceiveShadow", new GUIStyle(EditorStyles.boldLabel));
        if (TryGetProperty(properties, "_ReciveShadow", out MaterialProperty ReciveShadowProperty))
        {
            materialEditor.ShaderProperty(ReciveShadowProperty, new GUIContent(ReciveShadowProperty.displayName, "是否接收阴影"));
        }
        if (TryGetProperty(properties, "_Bias", out MaterialProperty BiasProperty))
        {
            materialEditor.ShaderProperty(BiasProperty, new GUIContent(BiasProperty.displayName, "阴影偏移，深度差低于这个值的自阴影将被消除,主要用来修正阴影精度不够产生抖动问题"));
        }
        NG_GUI.Separator();
        */

        if (TryGetProperty(properties, "_SpecularColor", out MaterialProperty SpecularColorProperty))
        {
            EditorGUILayout.LabelField("Specular", new GUIStyle(EditorStyles.boldLabel));
            MaterialProperty UseStockProprity;
            if (TryGetProperty(properties, "_UseStock", out UseStockProprity))
            {
                //NG_GUI.Separator();
                //EditorGUILayout.LabelField("UseStock", new GUIStyle(EditorStyles.boldLabel));
                materialEditor.ShaderProperty(UseStockProprity, new GUIContent(UseStockProprity.displayName, "使用丝袜模式"));
                if (UseStockProprity.floatValue > 0)
                {
                    if (TryGetProperty(properties, "_StockingDenier", out MaterialProperty StockingDenierProperty))
                    {
                        EditorGUILayout.LabelField("Stocking", new GUIStyle(EditorStyles.boldLabel));
                        materialEditor.ShaderProperty(StockingDenierProperty, new GUIContent(StockingDenierProperty.displayName, "Denier属性"));
                        if (TryGetProperty(properties, "_StockingDenierTex", out MaterialProperty StockingDenierTexProperty))
                        {
                            EditorGUI.BeginChangeCheck();
                            materialEditor.TexturePropertySingleLine(new GUIContent(StockingDenierTexProperty.displayName, "Denier属性贴图"), StockingDenierTexProperty);
                            if (EditorGUI.EndChangeCheck())
                            {
                                OnFixTextureFormat(targetMat, "_StockingDenierTex", 512, false, true, true, false, TextureWrapMode.Repeat);
                            }
                        }

                        if (TryGetProperty(properties, "_StockingRimPower", out MaterialProperty StockingRimPowerProperty))
                        {
                            materialEditor.ShaderProperty(StockingRimPowerProperty, new GUIContent(StockingRimPowerProperty.displayName, "丝袜边缘光强度"));
                        }
                        if (TryGetProperty(properties, "_StockingFresnelScale", out MaterialProperty StockingFresnelScaleProperty))
                        {
                            materialEditor.ShaderProperty(StockingFresnelScaleProperty, new GUIContent(StockingFresnelScaleProperty.displayName, "丝袜菲涅尔强度"));
                        }

                        if (TryGetProperty(properties, "_StockingFresnelMin", out MaterialProperty StockingFresnelMinProperty))
                        {
                            materialEditor.ShaderProperty(StockingFresnelMinProperty, new GUIContent(StockingFresnelMinProperty.displayName, "丝袜菲涅尔强度最小值"));
                        }

                        if (TryGetProperty(properties, "_StockingFresnelThreshould", out MaterialProperty StockingFresnelThreshouldProperty))
                        {
                            materialEditor.ShaderProperty(StockingFresnelThreshouldProperty, new GUIContent(StockingFresnelThreshouldProperty.displayName, "丝袜菲涅尔点"));
                        }

                        if (TryGetProperty(properties, "_StockingFresnelSmooth", out MaterialProperty StockingFresnelSmoothProperty))
                        {
                            materialEditor.ShaderProperty(StockingFresnelSmoothProperty, new GUIContent(StockingFresnelSmoothProperty.displayName, "丝袜菲涅尔过渡"));
                        }

                        if (TryGetProperty(properties, "_StockingTint", out MaterialProperty StockingTintProperty))
                        {
                            materialEditor.ShaderProperty(StockingTintProperty, new GUIContent(StockingTintProperty.displayName, "丝袜颜色"));
                        }

                        if (TryGetProperty(properties, "_StockingGlossiness", out MaterialProperty StockingGlossinessProperty))
                        {
                            materialEditor.ShaderProperty(StockingGlossinessProperty, new GUIContent(StockingGlossinessProperty.displayName, "丝袜光滑度"));
                        }


                        if (TryGetProperty(properties, "_StockingSpecular", out MaterialProperty StockingSpecularProperty))
                        {
                            materialEditor.ShaderProperty(StockingSpecularProperty, new GUIContent(StockingSpecularProperty.displayName, "丝袜高光"));
                        }

                        if (TryGetProperty(properties, "_StockingSpecularThreshould", out MaterialProperty StockingSpecularThreshouldProperty))
                        {
                            materialEditor.ShaderProperty(StockingSpecularThreshouldProperty, new GUIContent(StockingSpecularThreshouldProperty.displayName, "丝袜高光点"));
                        }

                        if (TryGetProperty(properties, "_StockingSpecularSmooth", out MaterialProperty StockingSpecularSmoothProperty))
                        {
                            materialEditor.ShaderProperty(StockingSpecularSmoothProperty, new GUIContent(StockingSpecularSmoothProperty.displayName, "丝袜高光过渡"));
                        }
                       
                        NG_GUI.Separator();
                    }
                }
            }

            bool useStock = UseStockProprity != null && UseStockProprity.floatValue > 0;
            if (!useStock)
            {
                MaterialProperty UseCartoonMetalProperty;
                if (TryGetProperty(properties, "_UseCartoonMetal", out UseCartoonMetalProperty))
                {
                    materialEditor.ShaderProperty(UseCartoonMetalProperty, new GUIContent(UseCartoonMetalProperty.displayName, "是否使用卡通金属高光"));
                }
                switch (specularOptions)
                {
                    case SpecularOptions.Specular:
                        {
                            materialEditor.ShaderProperty(SpecularColorProperty, new GUIContent(SpecularColorProperty.displayName, "高光的颜色修正（与Specular贴图中的rgb相乘）"));
                            if (TryGetProperty(properties, "_Glossiness", out MaterialProperty GlossinessProperty))
                            {
                                materialEditor.ShaderProperty(GlossinessProperty, new GUIContent(GlossinessProperty.displayName, "高光的光滑度修正（于Specular贴图中的a相乘)"));
                            }
                            if (TryGetProperty(properties, "_Roughness", out MaterialProperty RoughnessProperty))
                            {
                                materialEditor.ShaderProperty(RoughnessProperty, new GUIContent(RoughnessProperty.displayName, "高光的光滑度修正（于Specular贴图中的a相乘)"));
                            }
                        }
                        break;
                    case SpecularOptions.Metallic:
                        {

                            materialEditor.ShaderProperty(SpecularColorProperty, new GUIContent(SpecularColorProperty.displayName, "高光的颜色修正（与Diffuse贴图中的rgb相乘）"));
                            if (TryGetProperty(properties, "_Metallic", out MaterialProperty MetallicProperty))
                            {
                                materialEditor.ShaderProperty(MetallicProperty, new GUIContent(MetallicProperty.displayName, "金属度修正（与Metallic贴图中的r相乘)"));
                            }
                            if (TryGetProperty(properties, "_MetallicIntensity", out MaterialProperty MetallicIntensityProperty))
                            {
                                materialEditor.ShaderProperty(MetallicIntensityProperty, new GUIContent(MetallicIntensityProperty.displayName, "金属度修正（与Metallic贴图中的r相乘)"));
                            }
                            if (TryGetProperty(properties, "_Glossiness", out MaterialProperty GlossinessProperty))
                            {
                                materialEditor.ShaderProperty(GlossinessProperty, new GUIContent("Roughness", "高光的粗糙度修正（与Metallic贴图中的g相乘)"));
                            }
                            if (TryGetProperty(properties, "_Roughness", out MaterialProperty RoughnessProperty))
                            {
                                materialEditor.ShaderProperty(RoughnessProperty, new GUIContent("Roughness", "高光的粗糙度修正（与Metallic贴图中的g相乘)"));
                            }
                        }
                        break;
                    default:
                        {
                            materialEditor.ShaderProperty(SpecularColorProperty, new GUIContent(SpecularColorProperty.displayName, "高光的颜色修正（与Diffuse贴图中的rgb相乘）"));
                            if (TryGetProperty(properties, "_Metallic", out MaterialProperty MetallicProperty))
                            {
                                materialEditor.ShaderProperty(MetallicProperty, new GUIContent(MetallicProperty.displayName, "金属度修正（与Metallic贴图中的r相乘)"));
                            }
                            if (TryGetProperty(properties, "_MetallicIntensity", out MaterialProperty MetallicIntensityProperty))
                            {
                                materialEditor.ShaderProperty(MetallicIntensityProperty, new GUIContent(MetallicIntensityProperty.displayName, "金属度修正（与Metallic贴图中的r相乘)"));
                            }
                            if (TryGetProperty(properties, "_Roughness", out MaterialProperty RoughnessProperty))
                            {
                                materialEditor.ShaderProperty(RoughnessProperty, new GUIContent("Roughness", "高光的粗糙度修正（与Metallic贴图中的g相乘)"));
                            }
                            if (TryGetProperty(properties, "_Glossiness", out MaterialProperty GlossinessProperty))
                            {
                                materialEditor.ShaderProperty(GlossinessProperty, new GUIContent(GlossinessProperty.displayName, "高光的模糊范围"));
                            }
                        }
                        break;
                }

                if (TryGetProperty(properties, "_Hardness", out MaterialProperty HardnessProperty))
                {
                    materialEditor.ShaderProperty(HardnessProperty, new GUIContent(HardnessProperty.displayName, "高光的硬度"));
                }

               /* if (TryGetProperty(properties, "_SpecularThreshold", out MaterialProperty SpecularThresholdProperty))
                {
                    materialEditor.ShaderProperty(SpecularThresholdProperty, new GUIContent(SpecularThresholdProperty.displayName, "高光卡通化算法中用来控制颜色溢出，控制高光的最大阀值"));
                }
*/
                if (TryGetProperty(properties, "_AsymmetricalSpecular", out MaterialProperty AsymmetricalSpecularProperty))
                {
                    materialEditor.ShaderProperty(AsymmetricalSpecularProperty, new GUIContent(AsymmetricalSpecularProperty.displayName, "非对称高光阀值"));
                }

                if (TryGetProperty(properties, "_SpecularIntensity", out MaterialProperty SpecularIntensityProperty))
                {
                    materialEditor.ShaderProperty(SpecularIntensityProperty, new GUIContent(SpecularIntensityProperty.displayName, "高光卡通化算法中用来强化高光亮度的参数"));
                }

                if (TryGetProperty(properties, "_OneMinusReflectivityIntensity", out MaterialProperty OneMinusReflectivityIntensityProperty))
                {
                    materialEditor.ShaderProperty(OneMinusReflectivityIntensityProperty, new GUIContent(OneMinusReflectivityIntensityProperty.displayName, "反射影响diffuse颜色的权重"));
                }
                MaterialProperty RotateSpecularProperty;
                if (TryGetProperty(properties, "_RotateSpecular", out RotateSpecularProperty))
                {
                    materialEditor.ShaderProperty(RotateSpecularProperty, new GUIContent(RotateSpecularProperty.displayName, "瞳孔旋转高光"));
                }
                if (TryGetProperty(properties, "_QuiverSpecular", out RotateSpecularProperty))
                {
                    materialEditor.ShaderProperty(RotateSpecularProperty, new GUIContent(RotateSpecularProperty.displayName, "瞳孔颤动高光"));
                }
                if (TryGetProperty(properties, "_TearfulSpecular", out RotateSpecularProperty))
                {
                    materialEditor.ShaderProperty(RotateSpecularProperty, new GUIContent(RotateSpecularProperty.displayName, "瞳孔常含泪水"));
                }

                if (UseCartoonMetalProperty != null && UseCartoonMetalProperty.floatValue > 0)
                {
                    if (TryGetProperty(properties, "_CartoonMetal", out MaterialProperty CartoonMetalProperty))
                    {
                        EditorGUI.BeginChangeCheck();
                        materialEditor.TexturePropertySingleLine(new GUIContent(CartoonMetalProperty.displayName, "卡通金属度贴图"), CartoonMetalProperty);
                        if (EditorGUI.EndChangeCheck())
                        {
                            OnFixTextureFormat(targetMat, "_CartoonMetal", 512, false, true, true, false, TextureWrapMode.Repeat);
                        }
                        materialEditor.TextureScaleOffsetProperty(CartoonMetalProperty);
                    }
                    ShowProperty("", materialEditor, properties, CartoonMetalList);
                }

                NG_GUI.Separator();
            }

            if (TryGetProperty(properties, "_ReflectionIntensity", out MaterialProperty ReflectionIntensityProperty))
            {
                EditorGUILayout.LabelField("Reflection", new GUIStyle(EditorStyles.boldLabel));
                MaterialProperty UseReflectionMapProperty;
                if (TryGetProperty(properties, "_UseReflectionMap", out UseReflectionMapProperty))
                {
                    materialEditor.ShaderProperty(UseReflectionMapProperty, new GUIContent(UseReflectionMapProperty.displayName, "开启是否使用环境反射贴图"));
                }
                if (TryGetProperty(properties, "_ReflectionMap", out MaterialProperty ReflectionMapProperty))
                {
                    materialEditor.ShaderProperty(ReflectionMapProperty, new GUIContent(ReflectionMapProperty.displayName, "环境反射贴图"));
                }
                if (UseReflectionMapProperty != null)
                {
                    if (UseReflectionMapProperty.floatValue > 0)
                    {
                        targetMat.EnableKeyword("_USE_REFLECTION_MAP");
                        if (versionOption == VersionOptions.Full)
                        {
                            if (TryGetProperty(properties, "nbSamples", out MaterialProperty nbSamplesProperty))
                            {
                                materialEditor.ShaderProperty(nbSamplesProperty, new GUIContent(nbSamplesProperty.displayName, "反射的采样次数"));
                            }
                            if (TryGetProperty(properties, "environment_rotation", out MaterialProperty environmentRotationProperty))
                            {
                                materialEditor.ShaderProperty(environmentRotationProperty, new GUIContent(environmentRotationProperty.displayName, "反射环境的旋转"));
                            }
                        }
                    }
                    else
                    {
                        targetMat.DisableKeyword("_USE_REFLECTION_MAP");
                    }
                }
                materialEditor.ShaderProperty(ReflectionIntensityProperty, new GUIContent(ReflectionIntensityProperty.displayName, "反射的强弱"));
                NG_GUI.Separator();
            }
        }

        /*    
        if (TryGetProperty(properties, "_FresnelThickness", out MaterialProperty FresnelThicknessProperty))
        {
            EditorGUILayout.LabelField("Fresnel", new GUIStyle(EditorStyles.boldLabel));
            materialEditor.ShaderProperty(FresnelThicknessProperty, new GUIContent(FresnelThicknessProperty.displayName, "Fresnel效果的强弱"));
            NG_GUI.Separator();
        }
        */

        if (TryGetProperty(properties, "_OutLineColor", out MaterialProperty OutLineColorProperty))
        {
            EditorGUILayout.LabelField("OutLine", new GUIStyle(EditorStyles.boldLabel));
            /*
            MaterialProperty UseVertexColorProperty;
            if (TryGetProperty(properties, "_UseVertexColor", out UseVertexColorProperty))
            {
                materialEditor.ShaderProperty(UseVertexColorProperty, new GUIContent(UseVertexColorProperty.displayName, "开关是否使用顶点色来细修描边线，其中顶点色中的b通道影响的是OutLine Z Offset的强弱，g通道影响的是Camera Strength的强弱，a通道影响的是描边的粗细。"));
                if (UseVertexColorProperty.floatValue > 0)
                {
                    targetMat.EnableKeyword("_OUTLINE_USE_VERTEXCOLOR");
                }
                else
                {
                    targetMat.DisableKeyword("_OUTLINE_USE_VERTEXCOLOR");
                }
            }
            if (TryGetProperty(properties, "_OutlineUV2AsNormals", out MaterialProperty OutlineUV2AsNormalsProperty))
            {
                materialEditor.ShaderProperty(OutlineUV2AsNormalsProperty, new GUIContent(OutlineUV2AsNormalsProperty.displayName, "开启是否使用UV2作为描边的法线"));
                if (OutlineUV2AsNormalsProperty.floatValue > 0)
                {
                    targetMat.EnableKeyword("_OUTLINE_UV2_AS_NORMALS");
                }
                else
                {
                    targetMat.DisableKeyword("_OUTLINE_UV2_AS_NORMALS");
                }
            }
            */

            materialEditor.ShaderProperty(OutLineColorProperty, new GUIContent(OutLineColorProperty.displayName, "描边线的颜色"));

            //if (UseVertexColorProperty.floatValue == 0)
            {
                if (TryGetProperty(properties, "_OutLineZOffset", out MaterialProperty OutLineZOffsetProperty))
                {
                    materialEditor.ShaderProperty(OutLineZOffsetProperty, new GUIContent(OutLineZOffsetProperty.displayName, "用来简易修正描边正面部分不需要显示的线，数值越大，正面的线消失的越多（根正面线本身的深度相关）"));
                }
                if (TryGetProperty(properties, "_OutLineWidthOffset", out MaterialProperty OutLineWidthOffsetProperty))
                {
                    materialEditor.ShaderProperty(OutLineWidthOffsetProperty, new GUIContent(OutLineWidthOffsetProperty.displayName, "描边线粗细调整"));
                }
            }


            NG_GUI.Separator();
        }

        rimOptions = RimOptions.Off;
        if (TryGetProperty(properties, "_UseRim", out MaterialProperty UseRimProperty))
        {
            EditorGUILayout.LabelField("Rim", new GUIStyle(EditorStyles.boldLabel));
            if (UseRimProperty.floatValue > 0)
            {
                if (UseRimProperty.floatValue == 1)
                {
                    rimOptions = RimOptions.NormalRim;
                }
                else
                {
                    rimOptions = RimOptions.DepthRim;
                }

            }

            EditorGUI.BeginChangeCheck();
            rimOptions = (RimOptions)EditorGUILayout.EnumPopup(new GUIContent("Rim Mode", "支持深度和法线两种模式"), rimOptions);
            if (EditorGUI.EndChangeCheck())
            {
                OnRimModeChange(targetMat);
            }
            if (UseRimProperty.floatValue > 0)
            {
                if (TryGetProperty(properties, "_RimColor", out MaterialProperty RimColorProperty))
                {
                    materialEditor.ShaderProperty(RimColorProperty, new GUIContent(RimColorProperty.displayName, "边缘光的颜色"));
                }

                if (UseRimProperty.floatValue == 1)
                {
                    //法线Rim
                    if (TryGetProperty(properties, "_RimMin", out MaterialProperty RimMinProperty))
                    {
                        materialEditor.ShaderProperty(RimMinProperty, new GUIContent(RimMinProperty.displayName, "边缘光阈值下限"));
                    }

                    if (TryGetProperty(properties, "_RimMax", out MaterialProperty RimMaxProperty))
                    {
                        materialEditor.ShaderProperty(RimMaxProperty, new GUIContent(RimMaxProperty.displayName, "边缘光阈值下限"));
                    }

                    /*   if (TryGetProperty(properties, "_RimSmooth", out MaterialProperty RimSmoothProperty))
                       {
                           materialEditor.ShaderProperty(RimSmoothProperty, new GUIContent(RimSmoothProperty.displayName, "边缘光平滑阈值"));
                       }*/
                }
                else
                {
                    //深度Rim
                    if (TryGetProperty(properties, "_RimWidth", out MaterialProperty RimWidthProperty))
                    {
                        materialEditor.ShaderProperty(RimWidthProperty, new GUIContent(RimWidthProperty.displayName, "边缘光的宽度"));
                    }
                    if (TryGetProperty(properties, "_RimThreshold", out MaterialProperty RimThresholdProperty))
                    {
                        materialEditor.ShaderProperty(RimThresholdProperty, new GUIContent(RimThresholdProperty.displayName, "光源的偏移"));
                    }
                }

                /* if (TryGetProperty(properties, "_RimPosition", out MaterialProperty RimPositionProperty))
                  {
                      materialEditor.ShaderProperty(RimPositionProperty, new GUIContent(RimPositionProperty.displayName, "模拟点光源的位置"));
                  }*/

            }
            NG_GUI.Separator();
        }
      
        if (TryGetProperty(properties, "_UseSSS", out MaterialProperty UseSSSProperty))
        {
            EditorGUILayout.LabelField("SSS", new GUIStyle(EditorStyles.boldLabel));
            materialEditor.ShaderProperty(UseSSSProperty, new GUIContent(UseSSSProperty.displayName, "次表面散射开关"));
            if (UseSSSProperty.floatValue > 0)
            {
                if (TryGetProperty(properties, "_SSSColor", out MaterialProperty SSSColorProperty))
                {
                    materialEditor.ShaderProperty(SSSColorProperty, new GUIContent(SSSColorProperty.displayName, "次表面散射光的颜色"));
                }
                if (TryGetProperty(properties, "_SSSAtten", out MaterialProperty SSSAttenProperty))
                {
                    materialEditor.ShaderProperty(SSSAttenProperty, new GUIContent(SSSAttenProperty.displayName, "次表面散射光衰减"));
                }

                if (TryGetProperty(properties, "_SSSAttenScale", out MaterialProperty SSSAttenScaleProperty))
                {
                    materialEditor.ShaderProperty(SSSAttenScaleProperty, new GUIContent(SSSAttenScaleProperty.displayName, "次表面散射光系数"));
                }
            }
            NG_GUI.Separator();
        }
      /*  basicRimOptions = BasicRimOptions.Off;
        if(TryGetProperty(properties, "_UseBasicRim", out MaterialProperty UseBasicRimProperty))
        {
            EditorGUILayout.LabelField("Rim", new GUIStyle(EditorStyles.boldLabel));
            if (UseBasicRimProperty.floatValue > 0)
            {
                basicRimOptions = BasicRimOptions.Normal;
            }
            EditorGUI.BeginChangeCheck();
            basicRimOptions = (BasicRimOptions)EditorGUILayout.EnumPopup(new GUIContent("Rim Mode", "Normal为标准边缘光"), basicRimOptions);
            if (EditorGUI.EndChangeCheck())
            {
                OnBasicRimModeChange(targetMat);
            }
            if(UseBasicRimProperty.floatValue>0)
            {
                if (TryGetProperty(properties, "_RimColor", out MaterialProperty RimColorProperty))
                {
                    materialEditor.ShaderProperty(RimColorProperty, new GUIContent(RimColorProperty.displayName, "边缘光的颜色"));
                }
                if (TryGetProperty(properties, "_RimMin", out MaterialProperty RimMinProperty))
                {
                    materialEditor.ShaderProperty(RimMinProperty, new GUIContent(RimMinProperty.displayName, "边缘光的最小阀值"));
                }
                if (TryGetProperty(properties, "_RimMax", out MaterialProperty RimMaxProperty))
                {
                    materialEditor.ShaderProperty(RimMaxProperty, new GUIContent(RimMaxProperty.displayName, "边缘光的最大阀值"));
                }
                if (TryGetProperty(properties, "_RimThreshold", out MaterialProperty RimThresholdProperty))
                {
                    materialEditor.ShaderProperty(RimThresholdProperty, new GUIContent(RimThresholdProperty.displayName, "边缘光的偏移"));
                }
            }
        }*/



        if (UseEmissionProperty == null || UseEmissionProperty.floatValue>0)
        {
            if (TryGetProperty(properties, "_EmissionColor", out MaterialProperty EmissionColorProperty))
            {
                EditorGUILayout.LabelField("Emission", new GUIStyle(EditorStyles.boldLabel));
                materialEditor.ShaderProperty(EmissionColorProperty, new GUIContent(EmissionColorProperty.displayName, "自发光的颜色，以及强度，主要是需要调HDR的强度，颜色一般还是由贴图本身决定。"));
                NG_GUI.Separator();
            }
            if (TryGetProperty(properties, "_EmissionAnim", out MaterialProperty EmissionAnimProperty))
            {
                materialEditor.ShaderProperty(EmissionAnimProperty, new GUIContent(EmissionAnimProperty.displayName, "自发光动画，呼吸效果"));
                NG_GUI.Separator();
            }
        }        

        if (TryGetProperty(properties, "_BloomFactor", out MaterialProperty BloomFactorProperty))
        {
            EditorGUILayout.LabelField("Bloom Setting", new GUIStyle(EditorStyles.boldLabel));
            materialEditor.ShaderProperty(BloomFactorProperty, new GUIContent(BloomFactorProperty.displayName, "和Bloom互动的参数，数值越大受到Bloom影响越大，注意如果开启透明模式不要修改这个值，应该让其保持为1，否则会影响透明度。"));
            NG_GUI.Separator();
        }

        if (TryGetProperty(properties, "_UseFlowMap", out MaterialProperty UseFlowMapProperty))
        {
            EditorGUILayout.LabelField("FlowMap", new GUIStyle(EditorStyles.boldLabel));
            materialEditor.ShaderProperty(UseFlowMapProperty, new GUIContent(UseFlowMapProperty.displayName, "溜光开关"));
            if (UseFlowMapProperty.floatValue > 0)
            {
                if (TryGetProperty(properties, "_FlowMap", out MaterialProperty FlowMapProperty))
                {
                    materialEditor.ShaderProperty(FlowMapProperty, new GUIContent(FlowMapProperty.displayName, "溜光贴图"));
                }
                if (TryGetProperty(properties, "_FlowMask", out MaterialProperty FlowMaskProperty))
                {
                    materialEditor.ShaderProperty(FlowMaskProperty, new GUIContent(FlowMaskProperty.displayName, "mask贴图"));
                }

                if (TryGetProperty(properties, "_FlowSpeed", out MaterialProperty FlowSpeedProperty))
                {
                    materialEditor.ShaderProperty(FlowSpeedProperty, new GUIContent(FlowSpeedProperty.displayName, "溜光速度"));
                }
                if (TryGetProperty(properties, "_FlowColor", out MaterialProperty FlowColorProperty))
                {
                    materialEditor.ShaderProperty(FlowColorProperty, new GUIContent(FlowColorProperty.displayName, "溜光颜色"));
                }
                if (TryGetProperty(properties, "_FlowStrength", out MaterialProperty FlowStrengthProperty))
                {
                    materialEditor.ShaderProperty(FlowStrengthProperty, new GUIContent(FlowStrengthProperty.displayName, "溜光强度"));
                }
            }
            NG_GUI.Separator();
        }

        EditorGUILayout.LabelField("Extend Effects", new GUIStyle(EditorStyles.boldLabel));
        if (TryGetProperty(properties, "_FresnelEffect", out MaterialProperty FresnelEffectProperty))
        {
            //EditorGUILayout.LabelField("Fresnel Effect", new GUIStyle(EditorStyles.boldLabel));
            materialEditor.ShaderProperty(FresnelEffectProperty, new GUIContent(FresnelEffectProperty.displayName, "菲涅尔特效开关"));
            NG_GUI.Separator();
            if (FresnelEffectProperty != null && FresnelEffectProperty.floatValue > 0)
            {
                targetMat.EnableKeyword("_FRESNEL_EFFECT");
                string[] filterList = { "_FresnelEffect" };
                ShowProperty("", materialEditor, properties, FresnelEffPropertiesList, filterList);
            }
            else
            {
                targetMat.DisableKeyword("_FRESNEL_EFFECT");
            }
        }

        if (TryGetProperty(properties, "_DissolveEffect", out MaterialProperty DissolveEffectProperty))
        {
            //EditorGUILayout.LabelField("Fresnel Effect", new GUIStyle(EditorStyles.boldLabel));
            materialEditor.ShaderProperty(DissolveEffectProperty, new GUIContent(DissolveEffectProperty.displayName, "溶解特效开关"));
            NG_GUI.Separator();
            if (DissolveEffectProperty != null && DissolveEffectProperty.floatValue > 0)
            {
                targetMat.EnableKeyword("_DISSOLVE_EFFECT");
                string[] filterList = { "_DissolveEffect" };
                ShowProperty("", materialEditor, properties, DissolveEffPropertiesList, filterList);
            }
            else
            {
                targetMat.DisableKeyword("_DISSOLVE_EFFECT");
            }
        }

        bool showOther = false;
        foreach(var p in properties)
        {
            if(!IsInCustomProperty(p, customPropertiesList, auraPropertiesList, rimPropertiesList, flowMapPropertiesList, colorPalettePropertiesList, stockingPropertiesList,ShadowColorHSVList, liquidPropertiesList, CartoonMetalList, FresnelEffPropertiesList, DissolveEffPropertiesList, sssPropertiesList))
            {
                if(!showOther)
                {
                    showOther = true;
                    EditorGUILayout.LabelField("Other Setting", new GUIStyle(EditorStyles.boldLabel));
                }
                materialEditor.ShaderProperty(p, new GUIContent(p.displayName));
            }
        }

        if(showOther)
        {
            NG_GUI.Separator();
        }

#if UNITY_5_5_OR_NEWER
        materialEditor.RenderQueueField();
#endif
    }

    private void OnFixTextureFormat(Material mat, string textureName, int maxTextureSize, bool sRGB, bool compress, bool useMipMap, bool hasAlpha, TextureWrapMode wrapMode, bool noramlMap = false)
    {
        var p = AssetDatabase.GetAssetPath(mat.GetTexture(textureName));
        if (p != "")
        {
            TextureImporter textureImporter = AssetImporter.GetAtPath(p) as TextureImporter;
            bool modify = false;
            if ((textureImporter.textureType != TextureImporterType.Default) != noramlMap)
            {
                if (noramlMap)
                {
                    textureImporter.textureType = TextureImporterType.NormalMap;
                }
                else
                {
                    textureImporter.textureType = TextureImporterType.Default;
                }
                modify = true;
            }
            if (textureImporter.sRGBTexture != sRGB)
            {
                textureImporter.sRGBTexture = sRGB;
                modify = true;
            }
            if (textureImporter.maxTextureSize != maxTextureSize)
            {
                textureImporter.maxTextureSize = maxTextureSize;
                modify = true;
            }
            if ((textureImporter.textureCompression != TextureImporterCompression.Uncompressed) != compress || ((textureImporter.textureCompression != TextureImporterCompression.Uncompressed) && textureImporter.textureCompression != TextureImporterCompression.CompressedHQ))
            {
                if (compress)
                {
                    textureImporter.textureCompression = TextureImporterCompression.CompressedHQ;
                }
                else
                {
                    textureImporter.textureCompression = TextureImporterCompression.Uncompressed;
                }
                modify = true;
            }
            if (textureImporter.mipmapEnabled != useMipMap)
            {
                textureImporter.mipmapEnabled = useMipMap;
                modify = true;
            }
            if(textureImporter.wrapMode != wrapMode)
            {
                textureImporter.wrapMode = wrapMode;
                modify = true;
            }
            
            bool iPhoneModify = false;
            var iPhoneSetting = textureImporter.GetPlatformTextureSettings("iPhone");
            if (!iPhoneSetting.overridden)
            {
                iPhoneSetting.overridden = true;
                iPhoneModify = true;
            }
            if (iPhoneSetting.maxTextureSize != maxTextureSize)
            {
                iPhoneSetting.maxTextureSize = maxTextureSize;
                iPhoneModify = true;
            }

            if (iPhoneSetting.format != TextureImporterFormat.ASTC_4x4)
            {
                iPhoneSetting.format = TextureImporterFormat.ASTC_4x4;
                iPhoneModify = true;
            }
            
            if (iPhoneSetting.compressionQuality != 100)
            {
                iPhoneSetting.compressionQuality = 100;
                iPhoneModify = true;
            }
            if (iPhoneModify)
            {
                textureImporter.SetPlatformTextureSettings(iPhoneSetting);
            }
            bool androidModify = false;
            var androidSetting = textureImporter.GetPlatformTextureSettings("Android");
            if (!androidSetting.overridden)
            {
                androidSetting.overridden = true;
                androidModify = true;
            }
            if (androidSetting.maxTextureSize != maxTextureSize)
            {
                androidSetting.maxTextureSize = maxTextureSize;
                androidModify = true;
            }
            if (textureImporter.textureCompression == TextureImporterCompression.Uncompressed)
            {
                if (androidSetting.format != TextureImporterFormat.ASTC_4x4)
                {
                    androidSetting.format = TextureImporterFormat.ASTC_4x4;
                    androidModify = true;
                }
            }
            else
            {
                if (androidSetting.format != TextureImporterFormat.ASTC_5x5)
                {
                    androidSetting.format = TextureImporterFormat.ASTC_5x5;
                    androidModify = true;
                }
            }
            if (androidSetting.compressionQuality != 100)
            {
                androidSetting.compressionQuality = 100;
                androidModify = true;
            }
            if (androidModify)
            {
                textureImporter.SetPlatformTextureSettings(androidSetting);
            }
            if (modify || androidModify || iPhoneModify)
            {
                AssetDatabase.ImportAsset(p);
            }
        }
    }

    private void OnRenderingModeChange(Material mat)
    {
        if(renderingOption == RenderingOptions.Transparent)
        {
            mat.SetFloat("_SrcBlendNG", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetFloat("_DstBlendNG", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);                    
        }
        else
        {
            mat.SetFloat("_SrcBlendNG", (float)UnityEngine.Rendering.BlendMode.One);
            mat.SetFloat("_DstBlendNG", (float)UnityEngine.Rendering.BlendMode.Zero);
        }
    }

    private void OnColorPaletteModeChange(Material mat)
    {
        mat.SetFloat("_UseHSVShadowColor", (float)colorPaletteOptions);
        switch(colorPaletteOptions)
        {
            case ColorPaletteOptions.Off:
                mat.DisableKeyword("_USE_HSV_SHADOW");
                mat.DisableKeyword("_USE_NORMAL_SHADOW");
                break;
            case ColorPaletteOptions.HSV:
                mat.EnableKeyword("_USE_HSV_SHADOW");
                mat.DisableKeyword("_USE_NORMAL_SHADOW");
                break;
            case ColorPaletteOptions.Normal:
                mat.EnableKeyword("_USE_NORMAL_SHADOW");
                mat.DisableKeyword("_USE_HSV_SHADOW");
                break;
        }
    }

    private void OnVersionModeChange(Material mat)
    {
        mat.SetFloat("_VersionMode", (float)versionOption);
    }

    private void OnRimModeChange(Material mat)
    {
        switch(rimOptions)
        {
            case RimOptions.NormalRim:
                mat.EnableKeyword("_USE_RIM_NORMAL");
                mat.DisableKeyword("_USE_RIM_DEPTH");
                mat.SetFloat("_UseRim", 1);
                break;
            case RimOptions.DepthRim:
                mat.EnableKeyword("_USE_RIM_DEPTH");
                mat.DisableKeyword("_USE_RIM_NORMAL");
                mat.SetFloat("_UseRim", 2);
                break;
            case RimOptions.Off:
                mat.DisableKeyword("_USE_RIM_NORMAL");
                mat.DisableKeyword("_USE_RIM_DEPTH");
                mat.SetFloat("_UseRim", 0);
                break;
        }
    }

/*    private void OnBasicRimModeChange(Material mat)
    {
        switch (basicRimOptions)
        {
            case BasicRimOptions.Normal:
                mat.EnableKeyword("_USE_RIM");
                mat.SetFloat("_UseBasicRim", 1);
                break;
            case BasicRimOptions.Off:
                mat.DisableKeyword("_USE_RIM");
                mat.SetFloat("_UseBasicRim", 0);
                break;
        }
    }*/

    private void OnSpecularModeChange(Material mat)
    {
        switch(specularOptions)
        {
            case SpecularOptions.Specular:
                mat.SetFloat("_UseMetallic", 0);
                mat.DisableKeyword("_USE_METALLIC");
                break;
            case SpecularOptions.Metallic:
                mat.SetFloat("_UseMetallic", 1);
                mat.EnableKeyword("_USE_METALLIC");
                break;
        }
    }

    private void OnOcclusionAddIntensityToggle(Material mat,bool check)
    {
        if(check)
        {
            mat.SetFloat("_OcclusionAddIntensity", 4);
        }
        else
        {
            mat.SetFloat("_OcclusionAddIntensity", 0);
        }
    }

    private void OnOcclusionSubIntensityToggle(Material mat, bool check)
    {
        if (check)
        {
            mat.SetFloat("_OcclusionSubIntensity", 4);
        }
        else
        {
            mat.SetFloat("_OcclusionSubIntensity", 0);
        }
    }

    private void OnUseDiffuseLightToggle(Material mat, bool check)
    {
        if (check)
        {
            mat.SetFloat("_UseDiffuseLight", 1);
        }
        else
        {
            mat.SetFloat("_UseDiffuseLight", 0);
        }
    }

    bool TryGetProperty(MaterialProperty[] properties,string name,out MaterialProperty property)
    {
        foreach(var p in properties)
        {
            if(p.name == name)
            {
                property = p;
                return true;
            }
        }
        property = null;
        return false;
    }

    bool IsInCustomProperty(MaterialProperty property, params string[][] propertiesList)
    {
        foreach(var list in propertiesList)
        {
            foreach(var name in list)
            {
                if (name == property.name)
                {
                    return true;
                }
            }            
        }
        return false;
    }

    bool IsInFilterList(MaterialProperty property, params string[] propertiesList)
    {
        if (propertiesList == null) return false;
        foreach (var name in propertiesList)
        {
            if (name == property.name)
            {
                return true;
            }
        }
        return false;
    }

    void ShowProperty(string name,MaterialEditor materialEditor, MaterialProperty[] properties, string[] propertiesList, params string[] filterList)
    {
        if(!String.IsNullOrEmpty(name))
        {
            EditorGUILayout.LabelField(name, new GUIStyle(EditorStyles.boldLabel));
        }        
        foreach (var p in properties)
        {
            if (this.IsInFilterList(p,filterList))
            {
                continue;
            }
            if (IsInCustomProperty(p, propertiesList))
            {
                materialEditor.ShaderProperty(p, new GUIContent(p.displayName));
            }
        }
    }
    
}