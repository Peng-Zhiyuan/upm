using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor.Rendering.Universal;

namespace UnityEditor.Rendering.Universal.ShaderGUI
{
    internal class NGStylizedLitShader : BaseShaderGUI
    {
        // Properties
        private NGStylizedLitGUI.LitProperties litProperties;

        private string[] fogProperties = { "_FogColor", "_FogEmissionColor", "_FogMin", "_FogMax", "_FogEmissionPower", "_FogEmissionFalloff", "_FogFalloff", "_FogRelativeWorldOrLocal"};//, "_FogWaveSpeedX", "_FogWaveSpeedZ", "_FogWaveAmplitudeX", "_FogWaveAmplitudeZ", "_FogWaveFreqX", "_FogWaveFreqZ", "_ANIMATION" };// "_STANDARD_FOG", "_OVERRIDE_FOG_COLOR" };
        protected class StStyles       
        {
            public static readonly GUIContent stylizedDiffuseGUI = new GUIContent("Stylized Diffuse",
                "These settings describe the look and feel of the surface itself.");

            // Catergories
          
            public static readonly GUIContent medColorGUI = new GUIContent("Medium Color",
                "These settings describe the look and feel of the surface itself.");

            public static readonly GUIContent medThresholdGUI = new GUIContent("Medium Threshold",
                "These settings describe the look and feel of the surface itself.");

            public static readonly GUIContent medSmoothGUI = new GUIContent("Medium Smooth",
                "These settings describe the look and feel of the surface itself.");
      
            public static readonly GUIContent shadowColorGUI = new GUIContent("Shadow Color",
                "These settings describe the look and feel of the surface itself.");

            public static readonly GUIContent shadowThresholdGUI = new GUIContent("Shadow Threshold",
                "These settings describe the look and feel of the surface itself.");

            public static readonly GUIContent shadowSmoothGUI = new GUIContent("Shadow Smooth",
                "These settings describe the look and feel of the surface itself.");

            public static readonly GUIContent shadowBrushStrengthGUI = new GUIContent("Shadow Brush Strength",
                "These settings describe the look and feel of the surface itself.");

            public static readonly GUIContent reflColorGUI = new GUIContent("Reflect Color",
                "These settings describe the look and feel of the surface itself.");

            public static readonly GUIContent reflThresholdGUI = new GUIContent("Reflect Threshold",
                "These settings describe the look and feel of the surface itself.");

            public static readonly GUIContent reflSmoothGUI = new GUIContent("Reflect Smooth",
                "These settings describe the look and feel of the surface itself.");

            public static readonly GUIContent reflBrushStrengthGUI = new GUIContent("Reflect Brush Strength",
                "These settings describe the look and feel of the surface itself.");

            public static readonly GUIContent giIntensityGUI = new GUIContent("GI (indirect Diffuse) Intensity",
                "These settings describe the look and feel of the surface itself.");

            public static readonly GUIContent ggxSpecularGUI = new GUIContent("GGX Specular",
                "These settings describe the look and feel of the surface itself.");

            public static readonly GUIContent specularLightOffsetGUI = new GUIContent("Specular Light Offset",
                "These settings describe the look and feel of the surface itself.");

            public static readonly GUIContent specularThresholdGUI = new GUIContent("Specular Threshold",
                "These settings describe the look and feel of the surface itself.");

            public static readonly GUIContent specularSmoothGUI = new GUIContent("Specular Smooth",
                "These settings describe the look and feel of the surface itself.");

            public static readonly GUIContent specularIntensityGUI = new GUIContent("Specular Intensity",
                "These settings describe the look and feel of the surface itself.");
            public static readonly GUIContent directionalFresnelGUI = new GUIContent("Directional Fresnel",
                "These settings describe the look and feel of the surface itself.");

            public static readonly GUIContent fresnelThresholdGUI = new GUIContent("Fresnel Threshold",
                "These settings describe the look and feel of the surface itself.");

            public static readonly GUIContent fresnelSmoothGUI = new GUIContent("Fresnel Smooth",
                "These settings describe the look and feel of the surface itself.");

            public static readonly GUIContent fresnelIntensityGUI = new GUIContent("Fresnel Intensity",
                "These settings describe the look and feel of the surface itself.");

            public static readonly GUIContent reflProbeIntensityGUI = new GUIContent("Non Metal Reflection Probe Intensity",
                "These settings describe the look and feel of the surface itself.");

            public static readonly GUIContent metalReflProbeIntensityGUI = new GUIContent("Metal Reflection Probe Intensity",
                "These settings describe the look and feel of the surface itself.");
            
            public static readonly GUIContent openFogGUI = new GUIContent("Height Fog",
              "These settings describe the open fog.");

            public static readonly GUIContent fogStateGUI = new GUIContent("Fog State",
            "These settings describe the fog.");
            public static readonly GUIContent fogColorGUI = new GUIContent("Fog Color",
            "These settings describe the fog color.");
            // collect properties from the material properties
        }

        public override void FindProperties(MaterialProperty[] properties)
        {
            base.FindProperties(properties);
            litProperties = new NGStylizedLitGUI.LitProperties(properties);
        }

        // material changed check
        public override void MaterialChanged(Material material)
        {
            if (material == null)
                throw new ArgumentNullException("material");

            SetMaterialKeywords(material, NGStylizedLitGUI.SetMaterialKeywords);
        }

        //
        public void DrawDitherProperty(Material material)
        {
           // EditorGUI.indentLevel++;
            EditorGUI.BeginChangeCheck();
            //if(litProperties.useDither != null) materialEditor.ShaderProperty(litProperties.useDither, NGStylizedLitGUI.Styles.useDitherText);
            // var dither = EditorGUILayout.PropertyField(useDither, "Use Dither");
            //if (EditorGUI.EndChangeCheck())
            //    MaterialChanged();
            if (EditorGUI.EndChangeCheck())
            {
                MaterialChanged(material);
            }
            //EditorGUI.indentLevel--;
        }

       public void DrawHeightMapProperty(Material material)
        {
            EditorGUI.BeginChangeCheck();
            materialEditor.ShaderProperty(litProperties.useHeightMap, NGStylizedLitGUI.Styles.useHeightMapText);
            float val = material.GetFloat("_UseHeightMap");
            if (val > 0)
            {
                materialEditor.TexturePropertySingleLine(NGStylizedLitGUI.Styles.heightMapText, litProperties.heightMap,null);
                EditorGUI.indentLevel++;
                //materialEditor.ShaderProperty(litProperties.heightMap, NGStylizedLitGUI.Styles.heightMapText);
                materialEditor.ShaderProperty(litProperties.heightScale, NGStylizedLitGUI.Styles.heightScaleText);
             
                EditorGUI.indentLevel--;
                 material.EnableKeyword("HEIGHT_MAP");
            }
            else
            {
                 material.DisableKeyword("HEIGHT_MAP");
            }
            
            // var dither = EditorGUILayout.PropertyField(useDither, "Use Dither");
            //if (EditorGUI.EndChangeCheck())
            //    MaterialChanged();
            if (EditorGUI.EndChangeCheck())
            {
                MaterialChanged(material);
            }
          
        }

        public void DrawReflectMapProperty(Material material)
        {
            //
            EditorGUI.BeginChangeCheck();
            //materialEditor.TexturePropertySingleLine(NGStylizedLitGUI.Styles.commonText, litProperties.useReflectMap, null);
            materialEditor.ShaderProperty(litProperties.useReflectMap, NGStylizedLitGUI.Styles.useReflectMapText);
            float val = material.GetFloat("_UseReflectMap");

            if (val > 0)
            {
                EditorGUI.indentLevel++;
                switch(val)
                {
                    case 1:
                        materialEditor.TexturePropertySingleLine(NGStylizedLitGUI.Styles.cubeMapText, litProperties.cubeMap, null);
                        material.EnableKeyword("CUBE_MAP");
                        break;
                    case 2:
                        materialEditor.TexturePropertySingleLine(NGStylizedLitGUI.Styles.matCapText, litProperties.matMap, null);
                        material.EnableKeyword("MAT_CAP");
                        break;
                    default:
                        break;
                }
               // materialEditor.TexturePropertySingleLine(NGStylizedLitGUI.Styles.reflectMaskText, litProperties.reflectMask, null);
                materialEditor.ShaderProperty(litProperties.reflectAmount, NGStylizedLitGUI.Styles._reflectAmountText);
                materialEditor.ShaderProperty(litProperties.mapColor, NGStylizedLitGUI.Styles._mapColorText);
                EditorGUI.indentLevel--;
            
            }
            else
            {
                material.DisableKeyword("REFLECT_MAP");
            }

            // var dither = EditorGUILayout.PropertyField(useDither, "Use Dither");
            //if (EditorGUI.EndChangeCheck())
            //    MaterialChanged();
            if (EditorGUI.EndChangeCheck())
            {
                MaterialChanged(material);
            }

        }


        // material main surface options
        public override void DrawSurfaceOptions(Material material)
        {
            if (material == null)
                throw new ArgumentNullException("material");

            // Use default labelWidth
            EditorGUIUtility.labelWidth = 0f;

            // Detect any changes to the material
            EditorGUI.BeginChangeCheck();
            if (EditorGUI.EndChangeCheck())
            {
                foreach (var obj in blendModeProp.targets)
                    MaterialChanged((Material)obj);
            }
            base.DrawSurfaceOptions(material);
        }

        public void DrawStylizedInputs(Material material)    
        {
            if (litProperties.shadowColor == null) return;
            EditorGUILayout.HelpBox("Stylized Diffuse", MessageType.None);
            materialEditor.ShaderProperty(litProperties.medColor, StStyles.medColorGUI, 1);
            materialEditor.ShaderProperty(litProperties.medThreshold, StStyles.medThresholdGUI, 1);
            materialEditor.ShaderProperty(litProperties.medSmooth, StStyles.medSmoothGUI, 1);
            materialEditor.ShaderProperty(litProperties.shadowColor, StStyles.shadowColorGUI, 1);
            materialEditor.ShaderProperty(litProperties.shadowThreshold, StStyles.shadowThresholdGUI, 1);
            materialEditor.ShaderProperty(litProperties.shadowSmooth, StStyles.shadowSmoothGUI, 1);

            materialEditor.ShaderProperty(litProperties.reflColor, StStyles.reflColorGUI, 1);
            materialEditor.ShaderProperty(litProperties.reflThreshold, StStyles.reflThresholdGUI, 1);
            materialEditor.ShaderProperty(litProperties.reflSmooth, StStyles.reflSmoothGUI, 1);

            EditorGUILayout.Space();
            materialEditor.ShaderProperty(litProperties.giIntensity, StStyles.giIntensityGUI, 1);
               
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("Stylized Reflection", MessageType.None);

            materialEditor.ShaderProperty(litProperties.ggxSpecular, StStyles.ggxSpecularGUI, 1);
            materialEditor.ShaderProperty(litProperties.specularLightOffset, StStyles.specularLightOffsetGUI, 1);
            if (material.GetFloat("_GGXSpecular") == 0)
            {
                materialEditor.ShaderProperty(litProperties.specularThreshold, StStyles.specularThresholdGUI, 1);
                materialEditor.ShaderProperty(litProperties.specularSmooth, StStyles.specularSmoothGUI, 1);
            }
            materialEditor.ShaderProperty(litProperties.specularIntensity, StStyles.specularIntensityGUI, 1);

            materialEditor.ShaderProperty(litProperties.directionalFresnel, StStyles.directionalFresnelGUI, 1);
            materialEditor.ShaderProperty(litProperties.fresnelThreshold, StStyles.fresnelThresholdGUI, 1);
            materialEditor.ShaderProperty(litProperties.fresnelSmooth, StStyles.fresnelSmoothGUI, 1);
            materialEditor.ShaderProperty(litProperties.fresnelIntensity, StStyles.fresnelIntensityGUI, 1);
           
            EditorGUILayout.Space(10);

            materialEditor.ShaderProperty(litProperties.reflProbeIntensity, StStyles.reflProbeIntensityGUI, 1);
            materialEditor.ShaderProperty(litProperties.metalReflProbeIntensity, StStyles.metalReflProbeIntensityGUI, 1);

            EditorGUILayout.Space();
           
        }

        // material main surface inputs
        public override void DrawSurfaceInputs(Material material)
        {
            base.DrawSurfaceInputs(material);
            materialEditor.ShaderProperty(litProperties.xSpeed, NGStylizedLitGUI.Styles.useXSpeed);
            materialEditor.ShaderProperty(litProperties.ySpeed, NGStylizedLitGUI.Styles.useYSpeed);
           
            DrawDitherProperty(material);
            DrawHeightMapProperty(material);
            NGStylizedLitGUI.DoMetallicSpecularArea(litProperties, materialEditor, material);

            BaseShaderGUI.DrawNormalArea(materialEditor, litProperties.bumpMapProp, litProperties.bumpScaleProp);
            //NGStylizedLitGUI.DoMetallicSpecularArea(litProperties, materialEditor, material);
            //NGStylizedLitGUI.DoSmoothness(litProperties, material);
            //NGStylizedLitGUI.DoOcclusionStrenth(litProperties, material);
            DrawEmissionProperties(material, true);

            DrawTileOffset(materialEditor, baseMapProp);
        }
        bool TryGetProperty(MaterialProperty[] properties, string name, out MaterialProperty property)
        {
            foreach (var p in properties)
            {
                if (p.name == name)
                {
                    property = p;
                    return true;
                }
            }
            property = null;
            return false;
        }

        bool IsInCustomProperty(MaterialProperty property, params string[] propertiesList)
        {
            foreach (var name in propertiesList)
            {

                if (name == property.name)
                {
                    return true;
                }
                
            }
            return false;
        }

        public override void OnGUI(MaterialEditor materialEditorIn, MaterialProperty[] properties)
        {
            base.OnGUI(materialEditorIn, properties);
            //EditorGUILayout.HelpBox("Height Fog", MessageType.None);
            materialEditor.ShaderProperty(litProperties.openHeightFog, StStyles.openFogGUI, 0);
            if (TryGetProperty(properties, "_OpenHeightFog", out MaterialProperty OpenHeightFog))
            {
                if (OpenHeightFog.floatValue > 0)
                {
                    foreach (var p in properties)
                    {
                        if (IsInCustomProperty(p, fogProperties))
                        {
                            materialEditor.ShaderProperty(p, new GUIContent(p.displayName));
                        }
                    }
                }
            }
           
        }

        // material main advanced options
        public override void DrawAdvancedOptions(Material material)
        {
            //Stylized Lit
            EditorGUILayout.Space();
            DrawStylizedInputs(material);
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            DrawReflectMapProperty(material);
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("Other Option", MessageType.None);
            if (litProperties.reflections != null && litProperties.highlights != null)
            {
                EditorGUI.BeginChangeCheck();
                materialEditor.ShaderProperty(litProperties.highlights, NGStylizedLitGUI.Styles.highlightsText);
                materialEditor.ShaderProperty(litProperties.reflections, NGStylizedLitGUI.Styles.reflectionsText);
                if(EditorGUI.EndChangeCheck())
                {
                    MaterialChanged(material);
                }
            }
       
            base.DrawAdvancedOptions(material);
        }

        public override void AssignNewShaderToMaterial(Material material, Shader oldShader, Shader newShader)
        {
            if (material == null)
                throw new ArgumentNullException("material");

            base.AssignNewShaderToMaterial(material, oldShader, newShader);
            if (material.HasProperty("_Emission"))
            {
                material.SetColor("_EmissionColor", material.GetColor("_Emission"));
            }

            if (oldShader == null || !oldShader.name.Contains("Legacy Shaders/"))
            {
                SetupMaterialBlendMode(material);
                return;
            }

            SurfaceType surfaceType = SurfaceType.Opaque;
            BlendMode blendMode = BlendMode.Alpha;
            if (oldShader.name.Contains("/Transparent/Cutout/"))
            {
                surfaceType = SurfaceType.Opaque;
                material.SetFloat("_AlphaClip", 1);
            }
            else if (oldShader.name.Contains("/Transparent/"))
            {
                // NOTE: legacy shaders did not provide physically based transparency
                // therefore Fade mode
                surfaceType = SurfaceType.Transparent;
                blendMode = BlendMode.Alpha;
            }
            material.SetFloat("_Surface", (float)surfaceType);
            material.SetFloat("_Blend", (float)blendMode);
            Texture texture = material.GetTexture("_MetallicGlossMap");
            if (texture != null)
                material.SetTexture("_MetallicSpecGlossMap", texture);
            MaterialChanged(material);
        }
    }
}
