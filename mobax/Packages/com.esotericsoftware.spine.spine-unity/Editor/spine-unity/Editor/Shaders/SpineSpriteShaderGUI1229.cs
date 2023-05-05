using UnityEngine;
using UnityEditor;
using Spine.Unity;

using SpineInspectorUtility = Spine.Unity.Editor.SpineInspectorUtility;

public class SpineSpriteShaderGUI1229 : SpineShaderWithOutlineGUI {
	static readonly string kShaderVertexLit = "Spine/Sprite/Vertex Lit";
	static readonly string kShaderPixelLit = "Spine/Sprite/Pixel Lit";
	static readonly string kShaderUnlit = "Spine/Sprite/Unlit";

	static readonly string kShaderVertexLitOutline = "Spine/Outline/Sprite/Vertex Lit";
	static readonly string kShaderPixelLitOutline = "Spine/Outline/Sprite/Pixel Lit";
	static readonly string kShaderUnlitOutline = "Spine/Outline/Sprite/Unlit";

	static readonly string kShaderLitLW = "Lightweight Render Pipeline/Spine/Sprite";
	static readonly string kShaderLitURP = "Universal Render Pipeline/Spine/Sprite";
	static readonly string kShaderLitURP2D = "Universal Render Pipeline/2D/Spine/Sprite";
	static readonly int kSolidQueue = 2000;
	static readonly int kAlphaTestQueue = 2450;
	static readonly int kTransparentQueue = 3000;

	private enum eBlendMode {
		PreMultipliedAlpha,
		StandardAlpha,
		Opaque,
		Additive,
		SoftAdditive,
		Multiply,
		Multiplyx2,
	};



	private enum eCulling {
		Off = 0,
		Front = 1,
		Back = 2,
	};

	private enum eNormalsMode {
		MeshNormals = -1,
		FixedNormalsViewSpace = 0,
		FixedNormalsModelSpace = 1,
		FixedNormalsWorldSpace = 2
	};

	MaterialProperty _mainTexture = null;
	MaterialProperty _color = null;

	MaterialProperty _tintColor = null;
	MaterialProperty _tintOffset = null;
	MaterialProperty _grayOffset = null;
	MaterialProperty _grayColorOffset = null;



	MaterialProperty _speOffset1 = null;
	MaterialProperty _speOffset2 = null;
	MaterialProperty _spePow = null;
	







	MaterialProperty _emitOffset = null;


	MaterialProperty _otherLightMax= null;





	MaterialProperty _maskTexture = null;

	MaterialProperty _pixelSnap = null;

	MaterialProperty _writeToDepth = null;
	MaterialProperty _depthAlphaCutoff = null;
	MaterialProperty _renderQueue = null;
	MaterialProperty _culling = null;
	MaterialProperty _customRenderQueue = null;












	MaterialProperty _MetalMap = null;

	MaterialProperty _LightDirAdjust = null;







	// MaterialProperty _RoleHeight = null;
	// MaterialProperty _RoleRoot = null;
	MaterialProperty _RootDarkLimit = null;


	private MaterialProperty _GuideH = null;

	bool _showOther = false;
	bool _showLight = false;
	bool _showTexture = false;
	bool _debugMode = false;
	private bool _speDebug = false;


	private MaterialProperty _EmmisionColor = null;




	static GUIContent _albedoText = new GUIContent("主纹理", "固有色 (RGB) ，透明度 (A)");
	static GUIContent _maskText = new GUIContent("Light Mask", "Light mask texture (secondary Sprite texture)");
	//static GUIContent _metallicMapText = new GUIContent("Metallic", "Metallic (R) and Smoothness (A)");
	//static GUIContent _subMapText = new GUIContent("副纹理", "遮蔽 (R)，高光（G） 自发光(B)");


	static GUIContent _metalTexToggleText = new GUIContent("附加纹理", "使用金属高光");
	//static GUIContent _subTexToggleText = new GUIContent("附加纹理", "使用遮蔽 (R)，高光（G） 自发光(B)");

	static GUIContent _metalMapText = new GUIContent("金属纹理", "");
	static GUIContent _depthText = new GUIContent("Write to Depth", "Write to Depth Buffer by clipping alpha.");
	static GUIContent _depthAlphaCutoffText = new GUIContent("Depth Alpha Cutoff", "Threshold for depth write alpha cutoff");

	static GUIContent _sphericalHarmonicsText = new GUIContent("Light Probes & Ambient", "Enable to use spherical harmonics to aplpy ambient light and/or light probes. In vertex-lit mode this will be approximated from scenes ambient trilight settings.");
	static GUIContent _blendModeText = new GUIContent("Blend Mode", "Blend Mode");
	static GUIContent[] _blendModeOptions = {
		new GUIContent("Pre-Multiplied Alpha"),
		new GUIContent("Standard Alpha"),
		new GUIContent("Opaque"),
		new GUIContent("Additive"),
		new GUIContent("Soft Additive"),
		new GUIContent("Multiply"),
		new GUIContent("Multiply x2")
	};
	static GUIContent _rendererQueueText = new GUIContent("Render Queue Offset");
	static GUIContent _cullingModeText = new GUIContent("Culling Mode");
	static GUIContent[] _cullingModeOptions = { new GUIContent("Off"), new GUIContent("Front"), new GUIContent("Back") };
	static GUIContent _pixelSnapText = new GUIContent("Pixel Snap");
	static GUIContent _fogToggleText = new GUIContent("Fog", "Enable Fog rendering on this renderer.");



	static GUIContent _holyLightToggleText = new GUIContent("垂直压暗", "可实现从头到脚的渐暗");

	static GUIContent _debugToggleText = new GUIContent("Debug", "开启Debug模式");
	static GUIContent _speDebugText = new GUIContent("高光调试", "开启高光Debug模式");
	const string _depthLabelText = "Depth";
	const string _customRenderType = "Use Custom RenderType";

	#region ShaderGUI

	public override void OnGUI (MaterialEditor materialEditor, MaterialProperty[] properties) {
		FindProperties(properties); // MaterialProperties can be animated so we do not cache them but fetch them every event to ensure animated values are updated correctly
		_materialEditor = materialEditor;
		ShaderPropertiesGUI();
	}

	public override void AssignNewShaderToMaterial (Material material, Shader oldShader, Shader newShader) {
		base.AssignNewShaderToMaterial(material, oldShader, newShader);

		//If not originally a sprite shader set default keywords
		if (oldShader.name != kShaderVertexLit && oldShader.name != kShaderPixelLit && oldShader.name != kShaderUnlit &&
			oldShader.name != kShaderVertexLitOutline && oldShader.name != kShaderPixelLitOutline && oldShader.name != kShaderUnlitOutline &&
			oldShader.name != kShaderLitLW &&
			oldShader.name != kShaderLitURP &&
			oldShader.name != kShaderLitURP2D) {
			SetDefaultSpriteKeywords(material, newShader);
		}

		SetMaterialKeywords(material);
	}

	#endregion

	#region Virtual Interface

	protected override void FindProperties (MaterialProperty[] props) {
		base.FindProperties(props);

		_mainTexture = FindProperty("_MainTex", props);
		_maskTexture = FindProperty("_MaskTex", props, false);
		_color = FindProperty("_Color", props);

		_tintColor = FindProperty("_TintColor", props);
		_tintOffset = FindProperty("_TintOffset", props);
		_grayOffset = FindProperty("_GrayOffset", props);
		_grayColorOffset = FindProperty("_GrayColorOffset", props);


	

		_speOffset1 = FindProperty("_SpeOffset1", props);
		_speOffset2 = FindProperty("_SpeOffset2", props);
		_spePow = FindProperty("_SpePow", props);
		_emitOffset = FindProperty("_EmitOffset", props);
		_EmmisionColor = FindProperty("_EmmisionColor", props);



	


		_otherLightMax = FindProperty("_otherLightMax", props);

		
		_MetalMap = FindProperty("_MetalMap", props, false);


		_LightDirAdjust = FindProperty("_LightDirAdjust", props, false);




	



		// _RoleHeight = FindProperty("_RoleHeight", props, false);
		// _RoleRoot = FindProperty("_RoleRoot", props, false); 
		_RootDarkLimit = FindProperty("_RootDarkLimit", props, false);





		_pixelSnap = FindProperty("PixelSnap", props);

		_writeToDepth = FindProperty("_ZWrite", props, false);
		_depthAlphaCutoff = FindProperty("_Cutoff", props);
		_renderQueue = FindProperty("_RenderQueue", props);
		_culling = FindProperty("_Cull", props);
		_customRenderQueue = FindProperty("_CustomRenderQueue", props);






		_GuideH = FindProperty("_GuideH", props,false);












	}

	static bool BoldToggleField (GUIContent label, bool value) {
		FontStyle origFontStyle = EditorStyles.label.fontStyle;
		EditorStyles.label.fontStyle = FontStyle.Bold;
		value = EditorGUILayout.Toggle(label, value, EditorStyles.toggle);
		EditorStyles.label.fontStyle = origFontStyle;
		return value;
	}

	static bool ToggleField(GUIContent label, bool value)
	{
		FontStyle origFontStyle = EditorStyles.label.fontStyle;
		EditorStyles.label.fontStyle = FontStyle.Normal;
		value = EditorGUILayout.Toggle(label, value, EditorStyles.toggle);
		EditorStyles.label.fontStyle = origFontStyle;
		return value;
	}







	protected virtual void ShaderPropertiesGUI () {
		// Use default labelWidth
		EditorGUIUtility.labelWidth = 0f;



		// Detect any changes to the material
		bool dataChanged = false;


		//_showTexture = EditorGUILayout.Foldout(_showTexture, "贴图设置");

       // if (_showTexture)
        {
			//GUILayout.Label(_primaryMapsText, EditorStyles.boldLabel);
			{
				dataChanged |= RenderTextureProperties();
			}
			//GUILayout.Label(_subMapsText, EditorStyles.boldLabel);
			// {
			// 	dataChanged |= RenderSubMap();
			// }

			{
				dataChanged |= RenderMetalMap();
			}
		}





		//_showLight = EditorGUILayout.Foldout(_showLight, "光照设置");

        //if (_showLight)
        {
			_materialEditor.RangeProperty(_otherLightMax, "光照上限");
			//_materialEditor.RangeProperty(_LightDisOffset, "辅光源-辐射距离系数");

			{
				dataChanged |= RenderHolyLight();
			}
		}

		_showOther =EditorGUILayout.Foldout(_showOther,"其他设置");

        if (_showOther)
        {

	        _materialEditor.RangeProperty(_GuideH, "深度保护");
	        
			dataChanged = RenderModes();

			GUILayout.Label(_depthLabelText, EditorStyles.boldLabel);
			{
				dataChanged |= RenderDepthProperties();
			}

			{
				dataChanged |= RenderFogProperties();
			}

			{
				EditorGUILayout.Space();
				RenderStencilProperties();
			}

			{
				EditorGUILayout.Space();
				RenderOutlineProperties();
			}

			_debugMode = ToggleField(_debugToggleText, _debugMode);
			
			if (_debugMode)
			{
				_materialEditor.ColorProperty(_tintColor, "Tint颜色");
				_materialEditor.RangeProperty(_tintOffset, "Tint系数");
				_materialEditor.RangeProperty(_grayOffset, "gray系数");
				_materialEditor.RangeProperty(_grayColorOffset, "gray颜色系数");
			}

			bool mixEnable;
			_speDebug = IsKeywordEnabled(_materialEditor, "_SPE_DEBUG", out mixEnable);
			var sdebug = ToggleField(_speDebugText, _speDebug);
			if (_speDebug!=sdebug)
			{
				_speDebug = sdebug;
				SetKeyword(_materialEditor, "_SPE_DEBUG", _speDebug);
			}
        }




		if (dataChanged) {
			MaterialChanged(_materialEditor);
		}
	}

	protected virtual bool RenderModes () {
		bool dataChanged = false;

		//Lighting Mode
		/*
		{
			EditorGUI.BeginChangeCheck();

			eLightMode lightMode = GetMaterialLightMode((Material)_materialEditor.target);
			EditorGUI.showMixedValue = false;
			foreach (Material material in _materialEditor.targets) {
				if (lightMode != GetMaterialLightMode(material)) {
					EditorGUI.showMixedValue = true;
					break;
				}
			}

			lightMode = (eLightMode)EditorGUILayout.Popup(_lightingModeText, (int)lightMode, _lightingModeOptions);
			if (EditorGUI.EndChangeCheck()) {
				foreach (Material material in _materialEditor.targets) {
					switch (lightMode) {
					case eLightMode.VertexLit:
						if (material.shader.name != kShaderVertexLit)
							_materialEditor.SetShader(Shader.Find(kShaderVertexLit), false);
						break;
					}
				}

				dataChanged = true;
			}
		}
		*/

		//Blend Mode
		{
			eBlendMode blendMode = GetMaterialBlendMode((Material)_materialEditor.target);
			EditorGUI.showMixedValue = false;
			foreach (Material material in _materialEditor.targets) {
				if (blendMode != GetMaterialBlendMode(material)) {
					EditorGUI.showMixedValue = true;
					break;
				}
			}

			EditorGUI.BeginChangeCheck();
			blendMode = (eBlendMode)EditorGUILayout.Popup(_blendModeText, (int)blendMode, _blendModeOptions);
			if (EditorGUI.EndChangeCheck()) {
				foreach (Material mat in _materialEditor.targets) {
					SetBlendMode(mat, blendMode);
				}

				dataChanged = true;
			}

			if (QualitySettings.activeColorSpace == ColorSpace.Linear &&
				!EditorGUI.showMixedValue && blendMode == eBlendMode.PreMultipliedAlpha) {
				EditorGUILayout.HelpBox(MaterialChecks.kPMANotSupportedLinearMessage, MessageType.Error, true);
			}
		}

		EditorGUI.BeginDisabledGroup(true);
		_materialEditor.RenderQueueField();
		EditorGUI.EndDisabledGroup();

		EditorGUI.BeginChangeCheck();
		EditorGUI.showMixedValue = _renderQueue.hasMixedValue;
		int renderQueue = EditorGUILayout.IntField(_rendererQueueText, (int)_renderQueue.floatValue);
		if (EditorGUI.EndChangeCheck()) {
			SetInt("_RenderQueue", renderQueue);
			dataChanged = true;
		}

		EditorGUI.BeginChangeCheck();
		var culling = (eCulling)Mathf.RoundToInt(_culling.floatValue);
		EditorGUI.showMixedValue = _culling.hasMixedValue;
		culling = (eCulling)EditorGUILayout.Popup(_cullingModeText, (int)culling, _cullingModeOptions);
		if (EditorGUI.EndChangeCheck()) {
			SetInt("_Cull", (int)culling);
			dataChanged = true;
		}

		EditorGUI.showMixedValue = false;

		EditorGUI.BeginChangeCheck();
		_materialEditor.ShaderProperty(_pixelSnap, _pixelSnapText);
		dataChanged |= EditorGUI.EndChangeCheck();

		return dataChanged;
	}

	protected virtual bool RenderTextureProperties () {
		bool dataChanged = false;

		EditorGUI.BeginChangeCheck();

		_materialEditor.TexturePropertySingleLine(_albedoText, _mainTexture, _color);


		if (_maskTexture != null)
			_materialEditor.TexturePropertySingleLine(_maskText, _maskTexture);



		dataChanged |= EditorGUI.EndChangeCheck();



		/*
		EditorGUI.BeginChangeCheck();
		_materialEditor.TextureScaleOffsetProperty(_mainTexture);
		dataChanged |= EditorGUI.EndChangeCheck();
		*/
		EditorGUI.showMixedValue = false;

		return dataChanged;
	}

	protected virtual bool RenderDepthProperties () {
		bool dataChanged = false;

		EditorGUI.BeginChangeCheck();

		bool showDepthAlphaCutoff = true;
		// e.g. Pixel Lit shader always has ZWrite enabled
		if (_writeToDepth != null) {
			bool mixedValue = _writeToDepth.hasMixedValue;
			EditorGUI.showMixedValue = mixedValue;
			bool writeTodepth = EditorGUILayout.Toggle(_depthText, _writeToDepth.floatValue != 0.0f);

			if (EditorGUI.EndChangeCheck()) {
				SetInt("_ZWrite", writeTodepth ? 1 : 0);
				_depthAlphaCutoff.floatValue = writeTodepth ? 0.5f : 0.0f;
				mixedValue = false;
				dataChanged = true;
			}

			showDepthAlphaCutoff = writeTodepth && !mixedValue && GetMaterialBlendMode((Material)_materialEditor.target) != eBlendMode.Opaque;
		}
		if (showDepthAlphaCutoff) {
			EditorGUI.BeginChangeCheck();
			_materialEditor.RangeProperty(_depthAlphaCutoff, _depthAlphaCutoffText.text);
			dataChanged |= EditorGUI.EndChangeCheck();
		}

		{
			bool useCustomRenderType = _customRenderQueue.floatValue > 0.0f;
			EditorGUI.BeginChangeCheck();
			EditorGUI.showMixedValue = _customRenderQueue.hasMixedValue;
			useCustomRenderType = EditorGUILayout.Toggle(_customRenderType, useCustomRenderType);
			if (EditorGUI.EndChangeCheck()) {
				dataChanged = true;

				_customRenderQueue.floatValue = useCustomRenderType ? 1.0f : 0.0f;

				foreach (Material material in _materialEditor.targets) {
					eBlendMode blendMode = GetMaterialBlendMode(material);

					switch (blendMode) {
					case eBlendMode.Opaque:
						{
							SetRenderType(material, "Opaque", useCustomRenderType);
						}
						break;
					default:
						{
							bool zWrite = HasZWriteEnabled(material);
							SetRenderType(material, zWrite ? "TransparentCutout" : "Transparent", useCustomRenderType);
						}
						break;
					}
				}
			}
		}

		EditorGUI.showMixedValue = false;

		return dataChanged;
	}





	protected virtual bool RenderSphericalHarmonicsProperties () {

		bool areMixedShaders = false;
		bool isLWRPShader = IsLWRPShader(_materialEditor, out areMixedShaders);
		bool isURP3DShader = IsURP3DShader(_materialEditor, out areMixedShaders);
		bool isURP2DShader = IsURP2DShader(_materialEditor, out areMixedShaders);
		bool hasSHParameter = !(isLWRPShader || isURP3DShader || isURP2DShader);
		if (!hasSHParameter)
			return false;

		EditorGUI.BeginChangeCheck();
		bool mixedValue;
		bool enabled = IsKeywordEnabled(_materialEditor, "_SPHERICAL_HARMONICS", out mixedValue);
		EditorGUI.showMixedValue = mixedValue;
		enabled = BoldToggleField(_sphericalHarmonicsText, enabled);
		EditorGUI.showMixedValue = false;

		if (EditorGUI.EndChangeCheck()) {
			SetKeyword(_materialEditor, "_SPHERICAL_HARMONICS", enabled);
			return true;
		}

		return false;
	}

	protected virtual bool RenderFogProperties () {

		bool areMixedShaders = false;
		bool isURP2DShader = IsURP2DShader(_materialEditor, out areMixedShaders);

		if (isURP2DShader && !areMixedShaders)
			return false;

		EditorGUI.BeginChangeCheck();
		bool mixedValue;
		bool fog = IsKeywordEnabled(_materialEditor, "_FOG", out mixedValue);
		EditorGUI.showMixedValue = mixedValue;
		fog = BoldToggleField(_fogToggleText, fog);
		EditorGUI.showMixedValue = false;

		if (EditorGUI.EndChangeCheck()) {
			SetKeyword(_materialEditor, "_FOG", fog);
			return true;
		}

		return false;
	}



	protected virtual bool RenderSubMap () {
		bool dataChanged = false;






		bool mixedValue;
		bool subTex = IsKeywordEnabled(_materialEditor, "_SUB_TEX", out mixedValue);
		EditorGUI.BeginChangeCheck();
		EditorGUI.showMixedValue = mixedValue;
		//subTex = ToggleField(_subTexToggleText, subTex);
		EditorGUI.showMixedValue = false;
		if (EditorGUI.EndChangeCheck())
		{
			SetKeyword(_materialEditor, "_SUB_TEX", subTex);
			mixedValue = false;
			dataChanged = true;
		}
		if (subTex && !mixedValue)
		{
			EditorGUI.BeginChangeCheck();

			//const int indentation = 2;

			//_materialEditor.TexturePropertySingleLine(_subMapText, _SubMap);

			//_materialEditor.ColorProperty(_aoColor1, "皮肤AO颜色");
			//_materialEditor.RangeProperty(_aoOffset1, "皮肤AO纹理系数");
			//_materialEditor.ColorProperty(_aoColor2, "AoOtherColor");
			//_materialEditor.RangeProperty(_aoOffset2, "AoOffset2");

			//_materialEditor.RangeProperty(_speOffset2, "SpecOffset2");

			
			//_materialEditor.RangeProperty(_skinLightOffset, "皮肤受光度");
			//_materialEditor.RangeProperty(_skinLightMax, "皮肤最大光");
			//_materialEditor.RangeProperty(_metalLightOffset, "金属受光度");
			//_materialEditor.RangeProperty(_metalLightMax, "金属最大光");
			//_materialEditor.RangeProperty(_otherLightOffset, "其他受光度");
			//_materialEditor.RangeProperty(_otherLightMax, "光照强度上限");


			dataChanged |= EditorGUI.EndChangeCheck();
		}





		return dataChanged;
	}


	protected virtual bool RenderMetalMap()
	{
		bool dataChanged = false;

		bool mixedValue;
		bool metalTex = IsKeywordEnabled(_materialEditor, "_METAL_TEX", out mixedValue);
		EditorGUI.BeginChangeCheck();
		EditorGUI.showMixedValue = mixedValue;
		metalTex = ToggleField(_metalTexToggleText, metalTex);
		EditorGUI.showMixedValue = false;
		if (EditorGUI.EndChangeCheck())
		{
			SetKeyword(_materialEditor, "_METAL_TEX", metalTex);
			mixedValue = false;
			dataChanged = true;
		}
		if (metalTex && !mixedValue)
        {
			EditorGUI.BeginChangeCheck();
			_materialEditor.TexturePropertySingleLine(_metalMapText, _MetalMap);
            if (_debugMode)
            {
				_materialEditor.VectorProperty(_LightDirAdjust, "光线矫正");
			}
            _materialEditor.RangeProperty(_emitOffset, "自发光强度");
            _materialEditor.ColorProperty(_EmmisionColor, "自发光颜色");
	        //_materialEditor.RangeProperty(_speOffset1, "主光源-高光强度");
            _materialEditor.RangeProperty(_speOffset2, "高光强度");
            _materialEditor.RangeProperty(_spePow, "高光范围");
			dataChanged |= EditorGUI.EndChangeCheck();
		}

		return dataChanged;
	}





	protected virtual bool RenderHolyLight()
	{
		bool dataChanged = false;

		bool mixedValue;
		bool holyLightEnable = IsKeywordEnabled(_materialEditor, "_HOLY_LIGHT", out mixedValue);
		EditorGUI.BeginChangeCheck();
		EditorGUI.showMixedValue = mixedValue;
		holyLightEnable = ToggleField(_holyLightToggleText, holyLightEnable);
		EditorGUI.showMixedValue = false;
		if (EditorGUI.EndChangeCheck())
		{
			SetKeyword(_materialEditor, "_HOLY_LIGHT", holyLightEnable);
			mixedValue = false;
			dataChanged = true;
		}
		if (holyLightEnable && !mixedValue)
		{
			EditorGUI.BeginChangeCheck();
			//EditorGUI.DrawRect(new Rect(0, 0, EditorGUIUtility.currentViewWidth, 300), Color.yellow);
			
			//_materialEditor.ColorProperty(_HolyLightColor, "颜色");
			//_materialEditor.RangeProperty(_HolyLightY, "偏移");
			//_materialEditor.RangeProperty(_HolyLightOffset, "强度");

			// _materialEditor.RangeProperty(_RoleHeight, "模型高度");
			// _materialEditor.RangeProperty(_RoleRoot, "模型底板");
			_materialEditor.RangeProperty(_RootDarkLimit, "压暗系数");

			dataChanged |= EditorGUI.EndChangeCheck();
		}

		return dataChanged;
	}





	#endregion

	#region Private Functions


	void SetInt (string propertyName, int value) {
		foreach (Material material in _materialEditor.targets) {
			material.SetInt(propertyName, value);
		}
	}

	void SetDefaultSpriteKeywords (Material material, Shader shader) {
		//Disable emission by default (is set on by default in standard shader)

		//Start with preMultiply alpha by default
		SetBlendMode(material, eBlendMode.PreMultipliedAlpha);
		//Start with mesh normals by default

		//Start with spherical harmonics disabled?
		SetKeyword(material, "_SPHERICAL_HARMONICS", false);
		//Start with specular disabled
		SetKeyword(material, "_SPECULAR", false);
		SetKeyword(material, "_SPECULAR_GLOSSMAP", false);
		//Start with Culling disabled
		material.SetInt("_Cull", (int)eCulling.Off);
		//Start with Z writing disabled
		if (material.HasProperty("_ZWrite"))
			material.SetInt("_ZWrite", 0);
	}

	//Z write is on then

	static void SetRenderType (Material material, string renderType, bool useCustomRenderQueue) {
		//Want a check box to say if should use Sprite render queue (for custom writing depth and normals)
		bool zWrite = HasZWriteEnabled(material);

		renderType = "SpriteViewSpaceFixedNormal";
		//If we don't write to depth set tag so custom shaders can write to depth themselves
		material.SetOverrideTag("AlphaDepth", zWrite ? "False" : "True");

		material.SetOverrideTag("RenderType", renderType);
	}

	static void SetMaterialKeywords (Material material) {
		eBlendMode blendMode = GetMaterialBlendMode(material);
		SetBlendMode(material, blendMode);

		bool zWrite = HasZWriteEnabled(material);
		bool clipAlpha = zWrite && blendMode != eBlendMode.Opaque && material.GetFloat("_Cutoff") > 0.0f;
		SetKeyword(material, "_ALPHA_CLIP", clipAlpha);

		bool normalMap = material.HasProperty("_BumpMap") && material.GetTexture("_BumpMap") != null;
		SetKeyword(material, "_NORMALMAP", normalMap);

		bool diffuseRamp = material.HasProperty("_DiffuseRamp") && material.GetTexture("_DiffuseRamp") != null;
		SetKeyword(material, "_DIFFUSE_RAMP", diffuseRamp);

		bool blendTexture = material.HasProperty("_BlendTex") && material.GetTexture("_BlendTex") != null;
		SetKeyword(material, "_TEXTURE_BLEND", blendTexture);
	}

	static void MaterialChanged (MaterialEditor materialEditor) {
		foreach (Material material in materialEditor.targets)
			SetMaterialKeywords(material);
	}

	static void SetKeyword (MaterialEditor m, string keyword, bool state) {
		foreach (Material material in m.targets) {
			SetKeyword(material, keyword, state);
		}
	}

	static void SetKeyword (Material m, string keyword, bool state) {
		if (state)
			m.EnableKeyword(keyword);
		else
			m.DisableKeyword(keyword);
	}

	static bool IsLWRPShader (MaterialEditor editor, out bool mixedValue) {
		return IsShaderType(kShaderLitLW, editor, out mixedValue);
	}

	static bool IsURP3DShader (MaterialEditor editor, out bool mixedValue) {
		return IsShaderType(kShaderLitURP, editor, out mixedValue);
	}

	static bool IsURP2DShader (MaterialEditor editor, out bool mixedValue) {
		return IsShaderType(kShaderLitURP2D, editor, out mixedValue);
	}

	static bool IsShaderType (string shaderType, MaterialEditor editor, out bool mixedValue) {

		mixedValue = false;
		bool isAnyTargetTypeShader = false;
		foreach (Material material in editor.targets) {
			if (material.shader.name == shaderType) {
				isAnyTargetTypeShader = true;
			}
			else if (isAnyTargetTypeShader) {
				mixedValue = true;
			}
		}
		return isAnyTargetTypeShader;
	}

	static bool IsKeywordEnabled (MaterialEditor editor, string keyword, out bool mixedValue) {
		bool keywordEnabled = ((Material)editor.target).IsKeywordEnabled(keyword);
		mixedValue = false;

		foreach (Material material in editor.targets) {
			if (material.IsKeywordEnabled(keyword) != keywordEnabled) {
				mixedValue = true;
				break;
			}
		}

		return keywordEnabled;
	}



	static eBlendMode GetMaterialBlendMode (Material material) {
		if (material.IsKeywordEnabled("_ALPHABLEND_ON"))
			return eBlendMode.StandardAlpha;
		if (material.IsKeywordEnabled("_ALPHAPREMULTIPLY_ON"))
			return eBlendMode.PreMultipliedAlpha;
		if (material.IsKeywordEnabled("_MULTIPLYBLEND"))
			return eBlendMode.Multiply;
		if (material.IsKeywordEnabled("_MULTIPLYBLEND_X2"))
			return eBlendMode.Multiplyx2;
		if (material.IsKeywordEnabled("_ADDITIVEBLEND"))
			return eBlendMode.Additive;
		if (material.IsKeywordEnabled("_ADDITIVEBLEND_SOFT"))
			return eBlendMode.SoftAdditive;

		return eBlendMode.Opaque;
	}

	static void SetBlendMode (Material material, eBlendMode blendMode) {
		SetKeyword(material, "_ALPHABLEND_ON", blendMode == eBlendMode.StandardAlpha);
		SetKeyword(material, "_ALPHAPREMULTIPLY_ON", blendMode == eBlendMode.PreMultipliedAlpha);
		SetKeyword(material, "_MULTIPLYBLEND", blendMode == eBlendMode.Multiply);
		SetKeyword(material, "_MULTIPLYBLEND_X2", blendMode == eBlendMode.Multiplyx2);
		SetKeyword(material, "_ADDITIVEBLEND", blendMode == eBlendMode.Additive);
		SetKeyword(material, "_ADDITIVEBLEND_SOFT", blendMode == eBlendMode.SoftAdditive);

		int renderQueue;
		bool useCustomRenderQueue = material.GetFloat("_CustomRenderQueue") > 0.0f;

		switch (blendMode) {
		case eBlendMode.Opaque:
			{
				material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
				material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
				SetRenderType(material, "Opaque", useCustomRenderQueue);
				renderQueue = kSolidQueue;
			}
			break;
		case eBlendMode.Additive:
			{
				material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
				material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
				bool zWrite = HasZWriteEnabled(material);
				SetRenderType(material, zWrite ? "TransparentCutout" : "Transparent", useCustomRenderQueue);
				renderQueue = zWrite ? kAlphaTestQueue : kTransparentQueue;
			}
			break;
		case eBlendMode.SoftAdditive:
			{
				material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
				material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcColor);
				bool zWrite = HasZWriteEnabled(material);
				SetRenderType(material, zWrite ? "TransparentCutout" : "Transparent", useCustomRenderQueue);
				renderQueue = zWrite ? kAlphaTestQueue : kTransparentQueue;
			}
			break;
		case eBlendMode.Multiply:
			{
				material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
				material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.SrcColor);
				bool zWrite = HasZWriteEnabled(material);
				SetRenderType(material, zWrite ? "TransparentCutout" : "Transparent", useCustomRenderQueue);
				renderQueue = zWrite ? kAlphaTestQueue : kTransparentQueue;
			}
			break;
		case eBlendMode.Multiplyx2:
			{
				material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.DstColor);
				material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.SrcColor);
				bool zWrite = HasZWriteEnabled(material);
				SetRenderType(material, zWrite ? "TransparentCutout" : "Transparent", useCustomRenderQueue);
				renderQueue = zWrite ? kAlphaTestQueue : kTransparentQueue;
			}
			break;
		default:
			{
				material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
				material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
				bool zWrite = HasZWriteEnabled(material);
				SetRenderType(material, zWrite ? "TransparentCutout" : "Transparent", useCustomRenderQueue);
				renderQueue = zWrite ? kAlphaTestQueue : kTransparentQueue;
			}
			break;
		}

		material.renderQueue = renderQueue + material.GetInt("_RenderQueue");
		material.SetOverrideTag("IgnoreProjector", blendMode == eBlendMode.Opaque ? "False" : "True");
	}


	static bool HasZWriteEnabled (Material material) {
		if (material.HasProperty("_ZWrite")) {
			return material.GetFloat("_ZWrite") > 0.0f;
		}
		else return true; // Pixel Lit shader always has _ZWrite enabled.
	}
	#endregion
}
