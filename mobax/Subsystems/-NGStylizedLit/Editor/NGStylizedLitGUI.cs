using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Scripting.APIUpdating;

namespace UnityEditor.Rendering.Universal.ShaderGUI
{
    [MovedFrom("UnityEditor.Rendering.LWRP.ShaderGUI")] public static class NGStylizedLitGUI
    {
        public static class Styles
        {
            public static GUIContent metallicMapText =
                new GUIContent("Metallic Map", "Sets and configures the map for the Metallic workflow.");

            public static GUIContent smoothnessText = new GUIContent("Smoothness",
                "Controls the spread of highlights and reflections on the surface.");

            public static GUIContent highlightsText = new GUIContent("Specular Highlights",
                "When enabled, the Material reflects the shine from direct lighting.");

           /* public static GUIContent useDitherText =
              new GUIContent("Use Dither", "Set Dither");*/
            public static GUIContent useYSpeed = 
                new GUIContent("Y Speed", "Set UV Y Speed");
            public static GUIContent useXSpeed = 
                new GUIContent("X Speed", "Set UV X Speed");
            public static GUIContent useHeightMapText =
              new GUIContent("Height Map", "Use Height Map");
           public static GUIContent heightMapText =
              new GUIContent("Height Map", "Set Height Map");
            public static GUIContent heightScaleText =
              new GUIContent("Height Scale", "Set Height Scale");
            public static GUIContent reflectionsText =
                new GUIContent("Environment Reflections",
                    "When enabled, the Material samples reflections from the nearest Reflection Probes or Lighting Probe.");

            public static GUIContent occlusionStrengthText = new GUIContent("Occlusion Strength",
                "Sets an occlusion strength to set simulate shadowing from ambient lighting.");
            public static GUIContent useReflectMapText =
             new GUIContent("ReflectMap", "Set Property useReflectMapText");
            public static GUIContent cubeMapText =
             new GUIContent("cubeMap", "Set Property cubeMap");
            public static GUIContent matCapText =
              new GUIContent("matCap", "Set Property matCap");
            public static GUIContent reflectMaskText =
              new GUIContent("reflectMask", "Set Property reflectMask");
            public static GUIContent _reflectAmountText =
             new GUIContent("reflectAmount", "Set Property reflectAmount");
            public static GUIContent _mapColorText =
              new GUIContent("mapColor", "Set Property mapColor");
        }

        public struct LitProperties
        {

            // Surface Input Props
            public MaterialProperty metallic;
            public MaterialProperty specColor;
            public MaterialProperty metallicGlossMap;
            public MaterialProperty specGlossMap;
            public MaterialProperty smoothness;
            public MaterialProperty bumpMapProp;
            public MaterialProperty bumpScaleProp;
            public MaterialProperty occlusionStrength;
            //public MaterialProperty useDither;
            public MaterialProperty ySpeed;
            public MaterialProperty xSpeed;

            //StylizedLit
            public MaterialProperty medColor;
            public MaterialProperty medThreshold;
            public MaterialProperty medSmooth;

            public MaterialProperty shadowColor;
            public MaterialProperty shadowThreshold;
            public MaterialProperty shadowSmooth;
            public MaterialProperty shadowBrushStrength;

            public MaterialProperty giIntensity;
            public MaterialProperty reflColor;
            public MaterialProperty reflThreshold;
            public MaterialProperty reflSmooth;
            public MaterialProperty reflBrushStrength;
            public MaterialProperty ggxSpecular;
            public MaterialProperty specularLightOffset;
            public MaterialProperty specularThreshold;
            public MaterialProperty specularSmooth;
            public MaterialProperty specularIntensity;
            public MaterialProperty directionalFresnel;
            public MaterialProperty fresnelThreshold;
            public MaterialProperty fresnelSmooth;
            public MaterialProperty fresnelIntensity;
            public MaterialProperty reflProbeIntensity;
            public MaterialProperty metalReflProbeIntensity;
            public MaterialProperty reflProbeRotation;
            public MaterialProperty openHeightFog;
            //public MaterialProperty fogColor;

            // Advanced Props
            public MaterialProperty highlights;
            public MaterialProperty reflections;

            public MaterialProperty heightMap;
            public MaterialProperty heightScale;
            public MaterialProperty useHeightMap;


            public MaterialProperty useReflectMap;
            public MaterialProperty cubeMap;
            public MaterialProperty matMap;
            public MaterialProperty reflectMask;
            public MaterialProperty reflectAmount;
            public MaterialProperty mapColor;
            public LitProperties(MaterialProperty[] properties)
            {
                // Surface Option Props
                // Surface Input Props
                metallic = BaseShaderGUI.FindProperty("_Metallic", properties);
                specColor = BaseShaderGUI.FindProperty("_SpecColor", properties, false);
                metallicGlossMap = BaseShaderGUI.FindProperty("_MetallicGlossMap", properties);
                specGlossMap = BaseShaderGUI.FindProperty("_SpecGlossMap", properties, false);
                smoothness = BaseShaderGUI.FindProperty("_Smoothness", properties, false);
                ySpeed  = BaseShaderGUI.FindProperty("_YSpeed", properties);
                xSpeed  = BaseShaderGUI.FindProperty("_XSpeed", properties);
                //useDither = BaseShaderGUI.FindProperty("_UseDither", properties);
                
                bumpMapProp = BaseShaderGUI.FindProperty("_BumpMap", properties, false);
                bumpScaleProp = BaseShaderGUI.FindProperty("_BumpScale", properties, false);
                occlusionStrength = BaseShaderGUI.FindProperty("_OcclusionStrength", properties, false);
                // Advanced Props
                highlights = BaseShaderGUI.FindProperty("_SpecularHighlights", properties, false);
                reflections = BaseShaderGUI.FindProperty("_EnvironmentReflections", properties, false);

                //stylized Lit
                medColor = BaseShaderGUI.FindProperty("_MedColor", properties, false);
                medThreshold = BaseShaderGUI.FindProperty("_MedThreshold", properties, false);
                medSmooth = BaseShaderGUI.FindProperty("_MedSmooth", properties, false);
                shadowColor = BaseShaderGUI.FindProperty("_ShadowColor", properties, false);
                shadowThreshold = BaseShaderGUI.FindProperty("_ShadowThreshold", properties, false);
                shadowSmooth = BaseShaderGUI.FindProperty("_ShadowSmooth", properties, false);
                shadowBrushStrength = BaseShaderGUI.FindProperty("_ShadowBrushStrength", properties, false);
                reflColor = BaseShaderGUI.FindProperty("_ReflectColor", properties, false);
                reflThreshold = BaseShaderGUI.FindProperty("_ReflectThreshold", properties, false);
                reflSmooth = BaseShaderGUI.FindProperty("_ReflectSmooth", properties, false);
                reflBrushStrength = BaseShaderGUI.FindProperty("_ReflBrushStrength", properties, false);
                giIntensity = BaseShaderGUI.FindProperty("_GIIntensity", properties, false);
                ggxSpecular = BaseShaderGUI.FindProperty("_GGXSpecular", properties, false);
                specularLightOffset = BaseShaderGUI.FindProperty("_SpecularLightOffset", properties, false);
                specularThreshold = BaseShaderGUI.FindProperty("_SpecularThreshold", properties, false);
                specularSmooth = BaseShaderGUI.FindProperty("_SpecularSmooth", properties, false);
                specularIntensity = BaseShaderGUI.FindProperty("_SpecularIntensity", properties, false);
                directionalFresnel = BaseShaderGUI.FindProperty("_DirectionalFresnel", properties, false);
                fresnelThreshold = BaseShaderGUI.FindProperty("_FresnelThreshold", properties, false);
                fresnelSmooth  = BaseShaderGUI.FindProperty("_FresnelSmooth", properties, false);
                fresnelIntensity = BaseShaderGUI.FindProperty("_FresnelIntensity", properties, false);
                reflProbeIntensity = BaseShaderGUI.FindProperty("_ReflProbeIntensity", properties, false);
                metalReflProbeIntensity = BaseShaderGUI.FindProperty("_MetalReflProbeIntensity", properties, false);
                reflProbeRotation = BaseShaderGUI.FindProperty("_ReflProbeRotation", properties, false);
                useHeightMap = BaseShaderGUI.FindProperty("_UseHeightMap", properties);
                heightMap = BaseShaderGUI.FindProperty("_HeightMap", properties);
                heightScale = BaseShaderGUI.FindProperty("_HeightScale", properties, false);
                openHeightFog = BaseShaderGUI.FindProperty("_OpenHeightFog", properties, false);


                useReflectMap = BaseShaderGUI.FindProperty("_UseReflectMap", properties, false);
                reflectMask = BaseShaderGUI.FindProperty("_ReflectMask", properties, false);
                cubeMap = BaseShaderGUI.FindProperty("_CubeMap", properties, false);
                matMap = BaseShaderGUI.FindProperty("_MatMap", properties, false);
                reflectAmount = BaseShaderGUI.FindProperty("_ReflectAmount", properties, false);
                mapColor = BaseShaderGUI.FindProperty("_MapColor", properties, false);
                //fogColor = BaseShaderGUI.FindProperty("_FogColor", properties, false);
            }
        }

       // public static void Inputs(LitProperties properties, MaterialEditor materialEditor, Material material)
       // {
       //     DoMetallicSpecularArea(properties, materialEditor, material);
       // }

        public static void DoOcclusionStrenth(LitProperties properties, Material material)
        {
            EditorGUI.indentLevel++;
            EditorGUI.BeginChangeCheck();
            EditorGUI.showMixedValue = properties.occlusionStrength.hasMixedValue;
            var strength = EditorGUILayout.Slider(Styles.occlusionStrengthText, properties.occlusionStrength.floatValue, 0f, 1f);
            if (EditorGUI.EndChangeCheck())
                properties.occlusionStrength.floatValue = strength;
            EditorGUI.showMixedValue = false;
            EditorGUI.indentLevel--;
        }
     
        public static void DoMetallicSpecularArea(LitProperties properties, MaterialEditor materialEditor, Material material)
        {
            bool hasGlossMap = properties.metallicGlossMap.textureValue != null;
            materialEditor.TexturePropertySingleLine(Styles.metallicMapText, properties.metallicGlossMap,
            hasGlossMap ? null : properties.metallic);
            EditorGUI.indentLevel++;
            DoSmoothness(properties, material);
            DoOcclusionStrenth(properties, material);
            //EditorGUI.DoDither(properties, material);
            EditorGUI.indentLevel--;
        }

        public static void DoSmoothness(LitProperties properties, Material material)
        {
            EditorGUI.indentLevel++;
            EditorGUI.BeginChangeCheck();
            EditorGUI.showMixedValue = properties.smoothness.hasMixedValue;
            var smoothness = EditorGUILayout.Slider(Styles.smoothnessText, properties.smoothness.floatValue, 0f, 1f);
            if (EditorGUI.EndChangeCheck())
                properties.smoothness.floatValue = smoothness;
            EditorGUI.showMixedValue = false;
            EditorGUI.indentLevel--;
        }

        public static void SetMaterialKeywords(Material material)
        {
            // Note: keywords must be based on Material value not on MaterialProperty due to multi-edit & material animation
            // (MaterialProperty value might come from renderer material property block)
            var hasGlossMap = material.GetTexture("_MetallicGlossMap") != null;
            var opaque = ((BaseShaderGUI.SurfaceType)material.GetFloat("_Surface") ==
                        BaseShaderGUI.SurfaceType.Opaque);
            CoreUtils.SetKeyword(material, "_METALLICSPECGLOSSMAP", hasGlossMap);
            if (material.HasProperty("_SpecularHighlights"))
                CoreUtils.SetKeyword(material, "_SPECULARHIGHLIGHTS_OFF",
                    material.GetFloat("_SpecularHighlights") == 0.0f);
            if (material.HasProperty("_EnvironmentReflections"))
                CoreUtils.SetKeyword(material, "_ENVIRONMENTREFLECTIONS_OFF",
                    material.GetFloat("_EnvironmentReflections") == 0.0f);
/*            if (material.HasProperty("_OpenHeightFog"))
                CoreUtils.SetKeyword(material, "HEIGHT_FOG",
                    material.GetFloat("_OpenHeightFog") == 1.0f);*/
         /*   if (material.HasProperty("_UseDither"))
                CoreUtils.SetKeyword(material, "_USE_DITHER",
                    material.GetFloat("_UseDither") == 1.0f);*/
        }
    }
}
