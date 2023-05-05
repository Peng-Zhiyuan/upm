/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated January 1, 2020. Replaces all prior versions.
 *
 * Copyright (c) 2013-2020, Esoteric Software LLC
 *
 * Integration of the Spine Runtimes into software or otherwise creating
 * derivative works of the Spine Runtimes is permitted under the terms and
 * conditions of Section 2 of the Spine Editor License Agreement:
 * http://esotericsoftware.com/spine-editor-license
 *
 * Otherwise, it is permitted to integrate the Spine Runtimes into software
 * or otherwise create derivative works of the Spine Runtimes (collectively,
 * "Products"), provided that each user of the Products must obtain their own
 * Spine Editor license and redistribution of the Products in any form must
 * include this license and copyright notice.
 *
 * THE SPINE RUNTIMES ARE PROVIDED BY ESOTERIC SOFTWARE LLC "AS IS" AND ANY
 * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL ESOTERIC SOFTWARE LLC BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES,
 * BUSINESS INTERRUPTION, OR LOSS OF USE, DATA, OR PROFITS) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
 * THE SPINE RUNTIMES, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

using UnityEngine;
using UnityEditor;
using Spine.Unity;

using SpineInspectorUtility = Spine.Unity.Editor.SpineInspectorUtility;

public class SpineSpriteShaderGUI1227 : SpineShaderWithOutlineGUI {
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
	MaterialProperty _aoColor1 = null;
	//MaterialProperty _aoColor2 = null;
	MaterialProperty _aoOffset1 = null;
	//MaterialProperty _aoOffset2 = null;

	MaterialProperty _speOffset1 = null;
	//MaterialProperty _speOffset2 = null;




	MaterialProperty _AoLightOffset = null;
	MaterialProperty _NorLightOffset = null;
	MaterialProperty _HighLightOffset = null;


	MaterialProperty _AoLightMax = null;
	MaterialProperty _NorLightMax = null;
	MaterialProperty _HighLightMax = null;

	MaterialProperty _emitOffset = null;

	MaterialProperty _maskTexture = null;

	MaterialProperty _pixelSnap = null;

	MaterialProperty _writeToDepth = null;
	MaterialProperty _depthAlphaCutoff = null;
	MaterialProperty _shadowAlphaCutoff = null;
	MaterialProperty _renderQueue = null;
	MaterialProperty _culling = null;
	MaterialProperty _customRenderQueue = null;



	MaterialProperty _rimPower = null;
	MaterialProperty _rimColor = null;



	MaterialProperty _blendTexture = null;
	MaterialProperty _blendTextureLerp = null;




	MaterialProperty _metallicGlossMap = null;









	static GUIContent _albedoText = new GUIContent("MainTexture", "固有色 (RGB) ，透明度 (A)");
	static GUIContent _maskText = new GUIContent("Light Mask", "Light mask texture (secondary Sprite texture)");
	//static GUIContent _metallicMapText = new GUIContent("Metallic", "Metallic (R) and Smoothness (A)");
	static GUIContent _metallicMapText = new GUIContent("SubTexture", "遮蔽 (R)，高光（G） 自发光(B)");

	static GUIContent _diffuseRampText = new GUIContent("Diffuse Ramp", "A black and white gradient can be used to create a 'Toon Shading' effect.");
	static GUIContent _depthText = new GUIContent("Write to Depth", "Write to Depth Buffer by clipping alpha.");
	static GUIContent _depthAlphaCutoffText = new GUIContent("Depth Alpha Cutoff", "Threshold for depth write alpha cutoff");
	static GUIContent _shadowAlphaCutoffText = new GUIContent("Shadow Alpha Cutoff", "Threshold for shadow alpha cutoff");


	static GUIContent _receiveShadowsText = new GUIContent("Receive Shadows", "When enabled, other GameObjects can cast shadows onto this GameObject. 'Write to Depth' has to be enabled in Lightweight RP.");
	static GUIContent _fixedNormalText = new GUIContent("Fixed Normals", "If this is ticked instead of requiring mesh normals a Fixed Normal will be used instead (it's quicker and can result in better looking lighting effects on 2d objects).");
	static GUIContent _fixedNormalDirectionText = new GUIContent("Fixed Normal Direction", "Should normally be (0,0,1) if in view-space or (0,0,-1) if in model-space.");
	static GUIContent _adjustBackfaceTangentText = new GUIContent("Adjust Back-face Tangents", "Tick only if you are going to rotate the sprite to face away from the camera, the tangents will be flipped when this is the case to make lighting correct.");
	static GUIContent _sphericalHarmonicsText = new GUIContent("Light Probes & Ambient", "Enable to use spherical harmonics to aplpy ambient light and/or light probes. In vertex-lit mode this will be approximated from scenes ambient trilight settings.");
	static GUIContent _lightingModeText = new GUIContent("Lighting Mode", "Lighting Mode");
	static GUIContent[] _lightingModeOptions = {
		new GUIContent("Vertex Lit"),
	};
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
	//static GUIContent _customRenderTypetagsText = new GUIContent("Use Custom RenderType tags");
	static GUIContent _fixedNormalSpaceText = new GUIContent("Fixed Normal Space");
	static GUIContent[] _fixedNormalSpaceOptions = { new GUIContent("View-Space"), new GUIContent("Model-Space"), new GUIContent("World-Space") };
	static GUIContent _rimLightingToggleText = new GUIContent("Rim Lighting", "Enable Rim Lighting.");
	static GUIContent _rimColorText = new GUIContent("Rim Color");
	static GUIContent _rimPowerText = new GUIContent("Rim Power");
	static GUIContent _specularToggleText = new GUIContent("Specular", "Enable Specular.");
	static GUIContent _fogToggleText = new GUIContent("Fog", "Enable Fog rendering on this renderer.");
	static GUIContent _meshRequiresTangentsText = new GUIContent("Note: Material requires a mesh with tangents.");
	static GUIContent _meshRequiresNormalsText = new GUIContent("Note: Material requires a mesh with normals.");
	static GUIContent _meshRequiresNormalsAndTangentsText = new GUIContent("Note: Material requires a mesh with Normals and Tangents.");

	const string _primaryMapsText = "Main Maps";
	const string _subMapsText = "Sub Maps";
	const string _depthLabelText = "Depth";
	const string _shadowsText = "Shadows";
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
		_aoColor1 = FindProperty("_AoColor1", props);
		//_aoColor2 = FindProperty("_AoColor2", props);
		_aoOffset1 = FindProperty("_AoOffset1", props);
		//_aoOffset2 = FindProperty("_AoOffset2", props);
		_speOffset1 = FindProperty("_SpeOffset1", props);
		//_speOffset2 = FindProperty("_SpeOffset2", props);
		_emitOffset = FindProperty("_EmitOffset", props);




		_AoLightOffset = FindProperty("_AoLightOffset", props);
		_NorLightOffset = FindProperty("_NorLightOffset", props);
		_HighLightOffset = FindProperty("_HighLightOffset", props);


		_AoLightMax = FindProperty("_AoLightMax", props);
		_NorLightMax = FindProperty("_NorLightMax", props);
		_HighLightMax = FindProperty("_HighLightMax", props);






		_pixelSnap = FindProperty("PixelSnap", props);

		_writeToDepth = FindProperty("_ZWrite", props, false);
		_depthAlphaCutoff = FindProperty("_Cutoff", props);
		_shadowAlphaCutoff = FindProperty("_ShadowAlphaCutoff", props);
		_renderQueue = FindProperty("_RenderQueue", props);
		_culling = FindProperty("_Cull", props);
		_customRenderQueue = FindProperty("_CustomRenderQueue", props);


		_blendTexture = FindProperty("_BlendTex", props, false);
		_blendTextureLerp = FindProperty("_BlendAmount", props, false);


		_rimPower = FindProperty("_RimPower", props, false);
		_rimColor = FindProperty("_RimColor", props, false);


		
		_metallicGlossMap = FindProperty("_MetallicGlossMap", props, false);




	}

	static bool BoldToggleField (GUIContent label, bool value) {
		FontStyle origFontStyle = EditorStyles.label.fontStyle;
		EditorStyles.label.fontStyle = FontStyle.Bold;
		value = EditorGUILayout.Toggle(label, value, EditorStyles.toggle);
		EditorStyles.label.fontStyle = origFontStyle;
		return value;
	}

	protected virtual void ShaderPropertiesGUI () {
		// Use default labelWidth
		EditorGUIUtility.labelWidth = 0f;



		// Detect any changes to the material
		bool dataChanged = false;

		//GUILayout.Label(_primaryMapsText, EditorStyles.boldLabel);
		{
			dataChanged |= RenderTextureProperties();
		}
		//GUILayout.Label(_subMapsText, EditorStyles.boldLabel);
		{
			dataChanged |= RenderSpecularProperties();
		}





		EditorGUILayout.Space();
		EditorGUILayout.Space();
		GUILayout.Label("---------------------------Other---------------------------");


		dataChanged = RenderModes();

		GUILayout.Label(_depthLabelText, EditorStyles.boldLabel);
		{
			dataChanged |= RenderDepthProperties();
		}

		GUILayout.Label(_shadowsText, EditorStyles.boldLabel);
		{
			dataChanged |= RenderShadowsProperties();
		}

		

		






		{
			dataChanged |= RenderFogProperties();
		}



		if (_rimColor != null) {
			dataChanged |= RenderRimLightingProperties();
		}

		{
			EditorGUILayout.Space();
			RenderStencilProperties();
		}

		{
			EditorGUILayout.Space();
			RenderOutlineProperties();
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

		if (_blendTexture != null) {
			EditorGUI.BeginChangeCheck();
			//_materialEditor.TexturePropertySingleLine(_altAlbedoText, _blendTexture, _blendTextureLerp);
			if (EditorGUI.EndChangeCheck()) {
				SetKeyword(_materialEditor, "_TEXTURE_BLEND", _blendTexture != null);
				dataChanged = true;
			}
		}

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



	protected virtual bool RenderShadowsProperties () {
		bool dataChanged = false;

		EditorGUI.BeginChangeCheck();
		_materialEditor.RangeProperty(_shadowAlphaCutoff, _shadowAlphaCutoffText.text);
		dataChanged = EditorGUI.EndChangeCheck();
		bool areMixedShaders = false;
		bool hasReceiveShadowsParameter = IsLWRPShader(_materialEditor, out areMixedShaders) ||
			IsURP3DShader(_materialEditor, out areMixedShaders);

		if (hasReceiveShadowsParameter) {
			bool forceDisableReceiveShadows = !_writeToDepth.hasMixedValue && _writeToDepth.floatValue == 0;
			EditorGUI.BeginDisabledGroup(forceDisableReceiveShadows);

			EditorGUI.BeginChangeCheck();
			bool mixedValue;
			bool enableReceive = !IsKeywordEnabled(_materialEditor, "_RECEIVE_SHADOWS_OFF", out mixedValue);
			EditorGUI.showMixedValue = mixedValue;
			enableReceive = EditorGUILayout.Toggle(_receiveShadowsText, enableReceive);

			EditorGUI.showMixedValue = false;

			if (EditorGUI.EndChangeCheck() || forceDisableReceiveShadows) {
				SetKeyword(_materialEditor, "_RECEIVE_SHADOWS_OFF", !enableReceive || forceDisableReceiveShadows);
				dataChanged = true;
			}
			EditorGUI.EndDisabledGroup(); // forceDisableReceiveShadows
		}

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



	protected virtual bool RenderSpecularProperties () {
		bool dataChanged = false;

		/*
		bool mixedSpecularValue;
		bool mixedSpecularGlossMapValue;
		bool specularGlossMap = IsKeywordEnabled(_materialEditor, "_SPECULAR_GLOSSMAP", out mixedSpecularGlossMapValue);
		bool mixedValue = mixedSpecularValue || mixedSpecularGlossMapValue;

		EditorGUI.BeginChangeCheck();
		EditorGUI.showMixedValue = mixedValue;
		bool specularEnabled = BoldToggleField(_specularToggleText, specular || specularGlossMap);
		EditorGUI.showMixedValue = false;
		if (EditorGUI.EndChangeCheck()) {
			foreach (Material material in _materialEditor.targets) {
				bool hasGlossMap = material.GetTexture("_MetallicGlossMap") != null;
				SetKeyword(material, "_SPECULAR", specularEnabled && !hasGlossMap);
				SetKeyword(material, "_SPECULAR_GLOSSMAP", specularEnabled && hasGlossMap);
			}

			mixedValue = false;
			dataChanged = true;
		}
		*/

			EditorGUI.BeginChangeCheck();
			bool hasGlossMap = _metallicGlossMap.textureValue != null;
			_materialEditor.TexturePropertySingleLine(_metallicMapText, _metallicGlossMap);
			if (EditorGUI.EndChangeCheck()) {
				hasGlossMap = _metallicGlossMap.textureValue != null;
				SetKeyword(_materialEditor, "_SPECULAR_GLOSSMAP", hasGlossMap);

				dataChanged = true;
			}

		//const int indentation = 2;

		_materialEditor.ColorProperty(_aoColor1, "AoSkinColor");
		_materialEditor.RangeProperty(_aoOffset1, "AoOffset1");
		//_materialEditor.ColorProperty(_aoColor2, "AoOtherColor");
		//_materialEditor.RangeProperty(_aoOffset2, "AoOffset2");
		_materialEditor.RangeProperty(_speOffset1, "SpecOffset1");
		//_materialEditor.RangeProperty(_speOffset2, "SpecOffset2");
		_materialEditor.RangeProperty(_emitOffset, "EmitOffset");


		_materialEditor.RangeProperty(_AoLightOffset, "_AoLightOffset");
		_materialEditor.RangeProperty(_NorLightOffset, "_NorLightOffset");
		_materialEditor.RangeProperty(_HighLightOffset, "_HighLightOffset");
		_materialEditor.RangeProperty(_AoLightMax, "_AoLightMax");
		_materialEditor.RangeProperty(_NorLightMax, "_NorLightMax");
		_materialEditor.RangeProperty(_HighLightMax, "_HighLightMax");


		return dataChanged;
	}



	protected virtual bool RenderRimLightingProperties () {
		bool dataChanged = false;

		bool mixedValue;
		bool rimLighting = IsKeywordEnabled(_materialEditor, "_RIM_LIGHTING", out mixedValue);

		EditorGUI.BeginChangeCheck();
		EditorGUI.showMixedValue = mixedValue;
		rimLighting = BoldToggleField(_rimLightingToggleText, rimLighting);
		EditorGUI.showMixedValue = false;
		if (EditorGUI.EndChangeCheck()) {
			SetKeyword(_materialEditor, "_RIM_LIGHTING", rimLighting);
			mixedValue = false;
			dataChanged = true;
		}

		if (rimLighting && !mixedValue) {
			EditorGUI.BeginChangeCheck();
			_materialEditor.ColorProperty(_rimColor, _rimColorText.text);
			_materialEditor.FloatProperty(_rimPower, _rimPowerText.text);
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
