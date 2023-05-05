using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using System.Linq;

public interface IAssetPostprocessor
{
	void PreprocessDefault(int maxSize, TextureImporterFormat textureFormat, bool halfSize = false);
	void PreprocessSprite(int maxSize, TextureImporterFormat textureFormat, bool halfSize = false);
	void PreprocessLightMap(int maxSize, TextureImporterFormat textureFormat, bool halfSize = false);
	void PreprocessNormalMap(int maxSize, TextureImporterFormat textureFormat, bool halfSize = false);
	void PreprocessCubeMap(int maxSize, TextureImporterFormat textureFormat, bool halfSize = false);
	
}

public class CustomAssetPostprocessor : AssetPostprocessor
{
	private List<IAssetPostprocessor> _postprocessors = new List<IAssetPostprocessor>();
	public CustomAssetPostprocessor()
	{
		_postprocessors.Add(new DefaultAssetPostprocessor(this));
		_postprocessors.Add(new AndroidAssetPostprocessor(this));
		_postprocessors.Add(new IosAssetPostprocessor(this));
	}
	/*
	void OnPreprocessAsset()
	{
		if (!PostProcessMenu.IsAssetPostProcessEnable) return;
		if (assetPath.EndsWith(".mask"))
		{
			AssetImportUtil.ImportAvatarMaskDelay((AssetImporter)assetImporter);
		}
	}
	*/

	public static void OnPostprocessAllAssets(string[] importedAsset, string[] deleteAsset, string[] movedAssets, string[] movedFromAssetPaths)
	{
		var isChnaged = false;
		foreach (var path in importedAsset)
		{
			var b = AssetImportUtil.TryPostprocessFx(path);
			if(b)
            {
				isChnaged = true;
            }
		}
		foreach (var path in movedAssets)
		{
			var b = AssetImportUtil.TryPostprocessFx(path);
			if(b)
            {
				isChnaged = true;
            }
		}

		if(isChnaged)
        {
			AssetDatabase.SaveAssets();
        }
	}



	void OnPreprocessTexture()
	{
		if (!PostProcessMenu.IsAssetPostProcessEnable) return;
		TextureImporter importer = (TextureImporter)assetImporter;
		if (assetPath.Contains("/@"))
		{
			Debug.Log("import path:"+ assetPath);
			return;
		}

		

		if (assetPath.StartsWith("Assets/Arts/Models"))
		{
			Debug.Log("import path:" + assetPath);
			this.PreprocessDefault(1024, TextureImporterFormat.ASTC_4x4, false);
			return;
		}
			
		if (assetPath.StartsWith("Assets/Arts/Env"))
		{
			Debug.Log("import path:" + assetPath);
			this.PreprocessDefault(1024, TextureImporterFormat.ASTC_4x4, false);
			return;
		}
			

		if (assetPath.StartsWith("Assets/Arts/Roguelike"))
		{
			Debug.Log("import path:" + assetPath);
			this.PreprocessDefault(1024, TextureImporterFormat.ASTC_4x4, false);
			return;
		}

	
		if (assetPath.StartsWith("Assets/Arts/$skybox"))
		{
			Debug.Log("skip import path:" + assetPath);
			//this.PreprocessCubeMap(2048, TextureImporterFormat.ASTC_6x6, false);
			return;
		}


		if (assetPath.StartsWith("Assets/Arts/Spine"))
		{
			if (assetPath.EndsWith("_BG.png"))
			{
				this.PreprocessSprite(1024, TextureImporterFormat.ASTC_4x4, false);
				
			}
			else 
			{
				this.PreprocessDefault(2048, TextureImporterFormat.ASTC_6x6, false);
			}
			return;
		}


	/*	if (assetPath.StartsWith("Assets/Arts/FX/$ui_fx"))
		{
			Debug.Log("import path:" + assetPath);
			this.PreprocessSprite(512, TextureImporterFormat.ASTC_4x4, false);
			return;
		}
*/
		if (assetPath.StartsWith("Assets/Arts/FX"))
		{
			Debug.Log("import path:" + assetPath);
			this.PreprocessDefault(1024, TextureImporterFormat.ASTC_4x4, false);
			return;
		}


		if (assetPath.StartsWith("Assets/Arts/Bosstimeline"))
		{
			Debug.Log("import path:" + assetPath);
			this.PreprocessDefault(2048, TextureImporterFormat.ASTC_4x4, false);
			return;
		}

		if (assetPath.StartsWith("Assets/Arts/Weapon"))
		{
			Debug.Log("import path:" + assetPath);
			this.PreprocessDefault(1024, TextureImporterFormat.ASTC_4x4, false);
			return;
		}

		if (assetPath.StartsWith("Assets/Arts/Plots/Plot2D/$plots_Images/$plots_image_bubble"))
		{
			Debug.Log("import path:" + assetPath);
			this.PreprocessSprite(512, TextureImporterFormat.ASTC_4x4, true);
			return;
		}

		if (assetPath.StartsWith("Assets/Arts/Plots/Plot2D/$plots_Images/$plots_image_frame"))
		{
			Debug.Log("import path:" + assetPath);
			this.PreprocessSprite(512, TextureImporterFormat.ASTC_4x4, false);
			return;
		}

		if (assetPath.StartsWith("Assets/Arts/Plots/Plot2D/$plots_Images/$plots_image_mask"))
		{
			Debug.Log("import path:" + assetPath);
			this.PreprocessSprite(64, TextureImporterFormat.ASTC_4x4, false);
			return;
		}

		if (assetPath.StartsWith("Assets/Arts/Plots/Plot2D/$plots_Images/$plots_image_picture_pack"))
		{
			Debug.Log("import path:" + assetPath);
			this.PreprocessSprite(1024, TextureImporterFormat.ASTC_4x4, false);
			return;
		}

		if (assetPath.StartsWith("Assets/Arts/Hair"))
		{
			Debug.Log("import path:" + assetPath);
			this.PreprocessDefault(1024, TextureImporterFormat.ASTC_4x4, false);
			return;
		}
		if (assetPath.StartsWith("Assets/Arts/Mix"))
		{
			Debug.Log("import path:" + assetPath);
			this.PreprocessDefault(2048, TextureImporterFormat.ASTC_6x6, false);
			return;
		}


		if (assetPath.StartsWith("Assets/Arts/SpecialTimeline"))
		{
			Debug.Log("import path:" + assetPath);
			this.PreprocessDefault(1024, TextureImporterFormat.ASTC_4x4, false);
			return;
		}

		if (assetPath.StartsWith("Assets/Arts/$BattleFont"))
		{
			Debug.Log("import path:" + assetPath);
			this.PreprocessDefault(1024, TextureImporterFormat.ASTC_4x4, false);
			return;
		}

		if (assetPath.StartsWith("Assets/Arts/Logo"))
		{
			Debug.Log("import path:" + assetPath);
			this.PreprocessDefault(2048, TextureImporterFormat.RGB24, false);
			return;
		}

		if (assetPath.StartsWith("Assets/Arts/"))
		{
			Debug.Log("import path:" + assetPath);
			this.PreprocessDefault(1024, TextureImporterFormat.ASTC_4x4, false);
			return;
		}

		if (assetPath.StartsWith("Assets/UISprites/$ui_bg_loading"))
		{
			Debug.Log("import path:" + assetPath);
			this.PreprocessSprite(1024, TextureImporterFormat.ASTC_4x4, false);
			return;
		}


		if (assetPath.StartsWith("Assets/UISprites/$ui_bg"))
		{
			
			Debug.Log("import path:" + assetPath);
			this.PreprocessSprite(1024, TextureImporterFormat.ASTC_4x4, false);
			return;
		}

		if (assetPath.StartsWith("Assets/UISprites/$plot_bg"))
		{
			Debug.Log("import path:" + assetPath);
			this.PreprocessSprite(1024, TextureImporterFormat.ASTC_6x6, false);
			return;
		}

		

		if (assetPath.StartsWith("Assets/UISprites/$ui_texture") || assetPath.StartsWith("Assets/UISprites/$ui_single_texture"))
		{
			
			Debug.Log("import path:" + assetPath);
			this.PreprocessSprite(1024, TextureImporterFormat.ASTC_4x4, true);
			return;
		}

		if (assetPath.StartsWith("Assets/UISprites/$ui_itemicon"))
		{

			Debug.Log("import path:" + assetPath);
			this.PreprocessSprite(128, TextureImporterFormat.ASTC_4x4, true);
			return;
		}

		if (assetPath.StartsWith("Assets/UISprites/$ui_shop"))
		{

			Debug.Log("import path:" + assetPath);
			this.PreprocessSprite(256, TextureImporterFormat.ASTC_4x4, false);
			return;
		}

		if (assetPath.StartsWith("Assets/UISprites/$ui_stage"))
		{

			Debug.Log("import path:" + assetPath);
			this.PreprocessSprite(1024, TextureImporterFormat.ASTC_4x4, false);
			return;
		}


		if (assetPath.StartsWith("Assets/UISprites"))
		{

			Debug.Log("import path:" + assetPath);
			this.PreprocessSprite(2048, TextureImporterFormat.ASTC_4x4, false);
			return;
		}

		if (assetPath.StartsWith("Assets/Resources/LoadingBg"))
		{

			Debug.Log("import path:" + assetPath);
			this.PreprocessSprite(2048, TextureImporterFormat.ASTC_6x6, false);
			return;
		}


	}

	private const string COMA = "@coma";
	private const string IDLE = "@idle";
	private const string RUN = "@run";
	private const string WALK = "@walk";
	private const string STAND = "@stand";
	private const string SKILL2 = "@skill2";

	/*
	void OnPostprocessModel(GameObject go)
	{

		Debug.LogError("OnPostprocessModel:" + assetPath);
		if (assetPath.StartsWith("Assets/Arts/Models"))
        {
			string fileName = System.IO.Path.GetFileName(assetPath);
			if (char.IsNumber(fileName[0]))
			{
				var sourceAvatarAnimator = go.GetComponentInChildren<Animator>();
				if (fileName.Contains("@"))
				{
					int index = assetPath.LastIndexOf("@");
					string path = assetPath.Substring(0, index) + ".fbx";
					sourceAvatarAnimator.avatar = GetSourceAvatar(path);
					Debug.LogError("sourceAvatarAnimator:" + path);
				}
				else if (fileName.Contains("_"))
				{
					int index = assetPath.LastIndexOf("_");
					string path = assetPath.Substring(0, index) + ".fbx";
					sourceAvatarAnimator.avatar = GetSourceAvatar(path);
					Debug.LogError("sourceAvatarAnimator:" + path);
				}
			}
		}
	}
	*/
	void OnPreprocessModel()
	{
		if (!PostProcessMenu.IsAssetPostProcessEnable) return;
		if (assetPath.Contains("/@"))
		{
			return;
		}
		if (assetPath.StartsWith("Assets/Arts/Env") || assetPath.StartsWith("Assets/Arts/Plots/$Plot3D"))
		{
			Debug.Log("PreprocessModel:" + assetPath);
			ModelImporter modelImporter = assetImporter as ModelImporter;
			string fileName = System.IO.Path.GetFileName(assetPath);
			//Materrials
			modelImporter.materialImportMode = ModelImporterMaterialImportMode.None;
			//Animation
			modelImporter.importConstraints = false;
			modelImporter.globalScale = 1;
			modelImporter.useFileUnits = true;
			modelImporter.bakeAxisConversion = false;
			modelImporter.importBlendShapes = false;
			modelImporter.importVisibility = false;
			modelImporter.importCameras = false;
			modelImporter.importLights = false;
			modelImporter.preserveHierarchy = false;
			modelImporter.sortHierarchyByName = true;
			modelImporter.meshCompression = ModelImporterMeshCompression.Off;
			modelImporter.isReadable = false;
			modelImporter.meshOptimizationFlags = MeshOptimizationFlags.Everything;
			modelImporter.addCollider = false;
			modelImporter.keepQuads = false;
			modelImporter.weldVertices = true;
			modelImporter.indexFormat = ModelImporterIndexFormat.Auto;
			modelImporter.importBlendShapes = false;
			modelImporter.importBlendShapeNormals = ModelImporterNormals.Import;
			modelImporter.normalCalculationMode = ModelImporterNormalCalculationMode.AreaAndAngleWeighted;
			modelImporter.normalSmoothingSource = ModelImporterNormalSmoothingSource.PreferSmoothingGroups;
			modelImporter.normalSmoothingAngle = 60.0f;
			modelImporter.importTangents = ModelImporterTangents.CalculateMikk;
			modelImporter.swapUVChannels = false;
			if (assetPath.StartsWith("Assets/Arts/Env"))
			{
				modelImporter.importAnimation = false;	
			}
			//modelImporter.generateSecondaryUV = false;
			Debug.Log("PreprocessModel Success:" + assetPath);
		}
		if (assetPath.StartsWith("Assets/Arts/Models"))// || assetPath.StartsWith("Assets/Arts/MixTemp"))
		{
			Debug.Log("PreprocessModel:" + assetPath);
			ModelImporter modelImporter = assetImporter as ModelImporter;
			string fileName = System.IO.Path.GetFileName(assetPath);
			//Materrials
			modelImporter.materialImportMode = ModelImporterMaterialImportMode.None;
			//Animation
			modelImporter.importConstraints = false;
			bool isAnimFbx = fileName.Contains("@");
			bool isFace = fileName.Contains("_face") || fileName.StartsWith("face_");
			bool isWeapon = fileName.Contains("_weapon") || fileName.StartsWith("wp_");
			bool isHair = fileName.Contains("_hair") || fileName.StartsWith("hair_");
			bool isMaskFbx = fileName.Contains("_mask");
			bool isRole = assetPath.Contains("/Role/");//|| assetPath.Contains("/Boss/") || assetPath.Contains("/Monster/");
			bool isAttachFbx = isFace || isWeapon || isHair;
			modelImporter.importAnimation = isAnimFbx;
			if (isAnimFbx)
			{
				if (assetPath.EndsWith(COMA) || assetPath.EndsWith(IDLE) || assetPath.EndsWith(RUN) || assetPath.EndsWith(WALK) || assetPath.EndsWith(STAND))
				{
					modelImporter.animationWrapMode = WrapMode.Loop;
				}
				else
				{
					modelImporter.animationWrapMode = WrapMode.Default;
				}

				if (!fileName.Contains(SKILL2))
				{
					//timeline skill
					modelImporter.animationCompression = ModelImporterAnimationCompression.Optimal;
					modelImporter.animationRotationError = 0.5f;
					modelImporter.animationPositionError = 0.5f;
					modelImporter.animationScaleError = 0.5f;
				}

				
				//int index = assetPath.LastIndexOf("@");
				//string path = assetPath.Substring(0, index).Replace("/fbx","") + ".mask";
				for (int i = 0; i < modelImporter.clipAnimations.Length; i++)
				{
					modelImporter.clipAnimations[i].lockRootHeightY = true;
					modelImporter.clipAnimations[i].keepOriginalPositionY = true;
					modelImporter.clipAnimations[i].heightOffset = 0;

					modelImporter.clipAnimations[i].lockRootPositionXZ = true;
					modelImporter.clipAnimations[i].keepOriginalPositionXZ = true;
					modelImporter.clipAnimations[i].cycleOffset = 0;

					modelImporter.clipAnimations[i].lockRootRotation = true;
					modelImporter.clipAnimations[i].rotationOffset = 0;
					modelImporter.clipAnimations[i].keepOriginalOrientation = true;
				}
			}

			if (isRole)
			{
				//Rig 
				if (isAttachFbx)
				{
					modelImporter.animationType = ModelImporterAnimationType.Generic;
					modelImporter.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
				}
				else
				{
					
					if (isAnimFbx)
					{
						//import path
						modelImporter.avatarSetup = ModelImporterAvatarSetup.CopyFromOther;
						int index = assetPath.LastIndexOf("@");
						string path = assetPath.Substring(0, index) + ".fbx";
						AssetImportUtil.TrySetSourceAvatar(modelImporter, path);
					}
					else if (isMaskFbx)
					{
						modelImporter.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
						modelImporter.autoGenerateAvatarMappingIfUnspecified = true;
					}
					else
					{
						modelImporter.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
						modelImporter.autoGenerateAvatarMappingIfUnspecified = true;
					}
				}
			}
			

			//Model
			modelImporter.globalScale = 1;
			modelImporter.useFileUnits = true;
			modelImporter.bakeAxisConversion = false;
			modelImporter.importBlendShapes = false;
			modelImporter.importVisibility = false;
			modelImporter.importCameras = false;
			modelImporter.importLights = false;
			modelImporter.preserveHierarchy = false;
			modelImporter.sortHierarchyByName = true;
			modelImporter.meshCompression = ModelImporterMeshCompression.Off;
			modelImporter.isReadable = false;
			modelImporter.meshOptimizationFlags = MeshOptimizationFlags.Everything;
			modelImporter.addCollider = false;
			modelImporter.keepQuads = false;
			modelImporter.weldVertices = true;
			modelImporter.indexFormat = ModelImporterIndexFormat.Auto;
			modelImporter.importBlendShapes = isFace;
			modelImporter.importBlendShapeNormals = ModelImporterNormals.Import;
			modelImporter.normalCalculationMode = ModelImporterNormalCalculationMode.AreaAndAngleWeighted;
			modelImporter.normalSmoothingSource = ModelImporterNormalSmoothingSource.PreferSmoothingGroups;
			modelImporter.normalSmoothingAngle = 60.0f;
			modelImporter.importTangents = ModelImporterTangents.CalculateMikk;
			modelImporter.swapUVChannels = false;
			modelImporter.generateSecondaryUV = false;
			Debug.Log("PreprocessModel Success:" + assetPath);
		}
	}

	#region ICustomAssetPostprocessor implementation

	void PreprocessDefault(int maxSize, TextureImporterFormat textureFormat, bool halfSize = false)
	{
		for (int i = 0; i < _postprocessors.Count; i++)
		{
			if (_postprocessors[i] != null)
				_postprocessors[i].PreprocessDefault(maxSize, textureFormat, halfSize);
		}
	}
	void PreprocessSprite(int maxSize, TextureImporterFormat textureFormat, bool halfSize = false)
	{
		for (int i = 0; i < _postprocessors.Count; i++)
		{
			if (_postprocessors[i] != null)
				_postprocessors[i].PreprocessSprite(maxSize, textureFormat, halfSize);
		}
	}
	void PreprocessLightMap(int maxSize, TextureImporterFormat textureFormat, bool halfSize = false)
	{
		for (int i = 0; i < _postprocessors.Count; i++)
		{
			if (_postprocessors[i] != null)
				_postprocessors[i].PreprocessLightMap(maxSize, textureFormat, halfSize);
		}
	}
	void PreprocessNormalMap(int maxSize, TextureImporterFormat textureFormat, bool halfSize = false)
    {
		for (int i = 0; i < _postprocessors.Count; i++)
		{
			if (_postprocessors[i] != null)
				_postprocessors[i].PreprocessNormalMap(maxSize, textureFormat, halfSize);
		}
	}

	void PreprocessCubeMap(int maxSize, TextureImporterFormat textureFormat, bool halfSize = false)
	{
		for (int i = 0; i < _postprocessors.Count; i++)
		{
			if (_postprocessors[i] != null)
				_postprocessors[i].PreprocessCubeMap(maxSize, textureFormat, halfSize);
		}
	}

	#endregion
}



/*
public class DisableMaterialImport :AssetPostprocessor
{
	public void OnPreprocessModel () 
	{
		if (assetPath.Contains("Skin"))
		{
			ModelImporter _mi = (ModelImporter)assetImporter;
			_mi.animationType = ModelImporterAnimationType.Legacy;
			_mi.importAnimation = false;
			_mi.isReadable = false;
		}
	}
}
*/
