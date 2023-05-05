// NG Shader EditorV1.0
// (c) 2019-2020

using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
// Utility to generate meshes with encoded smoothed normals, to fix hard-edged broken outline

public class CustomMeshTool
{
	[MenuItem("Assets/CustomBrushTools/LowMeshShadowBrush")]
	static void LowMeshShadowBrush()
	{
		Object[] selection = Selection.GetFiltered(typeof(object), SelectionMode.DeepAssets);
		foreach (Object activeObject in selection)
		{
			string resPath = AssetDatabase.GetAssetPath(activeObject);
			GameObject go = PrefabUtility.LoadPrefabContents(resPath);// AssetDatabase.LoadAssetAtPath(resPath, typeof(GameObject)) as GameObject;
			if (go == null) continue;
			var lodGroup = go.GetComponent<LODGroup>();

			var lods = lodGroup.GetLODs();
			if (lods.Length <= 1) continue;
			var lowMode = lods[lods.Length - 1];
			if (lowMode.renderers.Length == 0)
			{
				Debug.LogError("Not find render on Low mode.");
				continue;
			}
			for (int i = 0; i < lods.Length; i++)
			{
				var detailMode = lods[i];
				if (detailMode.renderers.Length == 0 || lowMode.renderers.Length == 0) continue;
				for (int j = 0; j < lowMode.renderers.Length; j++)
				{
					var render = detailMode.renderers[j];
					render.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
					if (render.transform.childCount > 0)
					{
						TransformUtil.RemoveAllChildren(render.transform);
					}
				}
			}
			for (int i = 0; i < lowMode.renderers.Length; i++)
			{
				var render = lowMode.renderers[i];
				var shadowObject = GameObject.Instantiate(render.gameObject, render.transform);
				shadowObject.name = shadowObject.name.Replace("(Clone)", "_SHADOW");
				shadowObject.transform.localScale = Vector3.one;
				shadowObject.transform.localPosition = Vector3.zero;
				shadowObject.transform.rotation = Quaternion.identity;
				var shadowRender = shadowObject.GetComponent<MeshRenderer>();
				shadowRender.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
				shadowRender.scaleInLightmap = 0;
				shadowRender.stitchLightmapSeams = false;
			}
			PrefabUtility.SaveAsPrefabAsset(go, resPath);
		}
	}
	/*
[MenuItem("Assets/CustomBrushTools/ReducedSmoothedBrush")]
static void PolyReducedSmoothedBrush()
{
	foreach (var o in Selection.objects)
	{
		DoReducedSmoothed(o);
	}
}

static private void DestroyAttachPart(GameObject go)
{

	var attachBone = go.GetComponent<AttachBone>();
	if (attachBone != null && attachBone.Part != null)
	{
		var part = go.transform.FindInChildren(attachBone.Part.name);
		if (part != null)
		{
			GameObject.DestroyImmediate(part);
		}
	}
}


static void DoReducedSmoothed(Object  o, bool newPolyPrefab = true)
{
	var path = AssetDatabase.GetAssetPath(o);
	var dir = Path.GetDirectoryName(path);
	mCurrentDirectoryPath = dir;
	if (o is GameObject)
	{
		var extendName = Path.GetExtension(path);
		bool isPrefab = extendName.ToLower() == ".prefab";
		bool isFBX = extendName.ToLower() == ".fbx";
		if (isPrefab)
		{ 

			//AttachBone.forbidAttach = true;
			//var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
			var go = PrefabUtility.LoadPrefabContents(path) as GameObject;
			if (newPolyPrefab)
			{
				if (path.EndsWith("_low.prefab"))
				{
					Debug.LogError("Already reduced:"+ path);
					return;
				}
				var newPath = path.Replace(".prefab", "_low.prefab");
				//var newGo = PrefabUtility.InstantiatePrefab(prefab) as GameObject;// GameObject.Instantiate(prefab);
				//bool suc;
				PrefabUtility.SaveAsPrefabAsset(go, newPath, out bool suc);
				GameObject.DestroyImmediate(go);
				if (!suc)
				{
					Debug.LogError("Poly prefab create fail:" + newPath);
					return;
				}
				//prefab = AssetDatabase.LoadAssetAtPath<GameObject>(newPath);
				go = PrefabUtility.LoadPrefabContents(newPath) as GameObject;
				path = newPath;


			}
			//GameObject go = PrefabUtility.InstantiatePrefab(prefab) as GameObject;



			mMeshes = GetSelectedMeshes(go);

			CreatePolySmoothedMeshes();
			PrefabUtility.SaveAsPrefabAssetAndConnect(go, path, InteractionMode.AutomatedAction);
			GameObject.DestroyImmediate(go);
			//AttachBone.forbidAttach = false;
		}
		else if (isFBX)
		{
			var fbx = AssetDatabase.LoadAssetAtPath<GameObject>(path);
			mMeshes = GetSelectedMeshes(fbx);
			CreatePolySmoothedMeshes();
		}
		else
		{
			Debug.LogError("Not support type:" + extendName.ToLower());
		}

	}
	else if (o is Mesh)
	{
		mMeshes = new Dictionary<Mesh, SelectedMesh>();
		var sm = GetMeshToAdd(o as Mesh, true);
		if (sm != null)
		{
			mMeshes.Add(o as Mesh, sm);
		}
		CreatePolySmoothedMeshes();
	}
	else
	{
		Debug.LogError("Not support type:" + o.GetType().ToString());
	}


}

private static void CreatePolySmoothedMeshes()
{
	Debug.Log("collect mesh count:" + mMeshes.Count);
	foreach (Mesh key in mMeshes.Keys)
	{
		Debug.Log("mesh name:" + key);
	}
	float progress = 1;
	float total = mMeshes.Count;
	foreach (var sm in mMeshes.Values)
	{
		if (sm == null)
			continue;
		EditorUtility.DisplayProgressBar("Hold On", (mMeshes.Count > 1 ? "Generating ReducedAndSmoothed Meshes:\n" : "Generating ReducedAndSmoothed Mesh:\n") + sm.name, progress / total);
		progress++;
		var asset = CreateSmoothedMeshAsset(sm);
		if (asset != null)
		{
			Debug.Log("reduced and smoothed:" + asset.name);
		}
	}
	EditorUtility.ClearProgressBar();
	Debug.Log("Total Reduced and Smoothed Success!!!");
}

//--------------------------------------------------------------------------------------------------
// INTERFACE

private const string MESH_SUFFIX = "reduced-smoothed";//"[NG Smoothed]";
#if UNITY_EDITOR_WIN
private const string OUTPUT_FOLDER = "\\ReducedSmoothedMeshes\\";
#else
private const string OUTPUT_FOLDER = "/ReducedSmoothedMeshes/";
#endif

private class SelectedMesh
{
	public SelectedMesh(Mesh _mesh, string _name, bool _isAsset, Object _assoObj = null, bool _skinned = false)
	{
		mesh = _mesh;
		name = _name;
		isAsset = _isAsset;
		AddAssociatedObject(_assoObj);

		isSkinned = _skinned;
		if(_assoObj != null && _assoObj is SkinnedMeshRenderer)
			isSkinned = true;
		else if(mesh != null && mesh.boneWeights != null && mesh.boneWeights.Length > 0)
			isSkinned = true;
	}

	public void AddAssociatedObject(Object _assoObj)
	{
		if(_assoObj != null)
		{
			_associatedObjects.Add(_assoObj);
		}
	}

	public Mesh mesh;
	public string name;
	public bool isAsset;
	public Object[] associatedObjects { get
	{
		if(_associatedObjects.Count == 0) return null;
		return _associatedObjects.ToArray();
	} } 	//can be SkinnedMeshRenderer or MeshFilter
	public bool isSkinned;

	private List<Object> _associatedObjects = new List<Object>();
}

private static Dictionary<Mesh, SelectedMesh> mMeshes;
private static string mFormat = "XYZ";
private static bool mVColors = false, mTangents = false, mUV2 = true;
//private Vector2 mScroll;

private static bool mAlwaysOverwrite = true;
private static bool mCustomDirectory = false;
private static string mCustomDirectoryPath = "smoothed-mesh";
private static string mCurrentDirectoryPath = "";

private static string GetSafeFilename(string name)
{
	var invalidChars = new List<char>(Path.GetInvalidFileNameChars());
	var newName = new List<char>(name.Length);
	foreach(var c in name)
	{
		if(!invalidChars.Contains(c))
			newName.Add(c);
	}

	return new string(newName.ToArray());
}

private static Mesh CreateSmoothedMeshAsset(SelectedMesh originalMesh)
{
	//Check if we are ok to overwrite
	var overwrite = true;

	var rootPath = mCustomDirectory ? Application.dataPath + "/" + mCustomDirectoryPath + "/" : mCurrentDirectoryPath + OUTPUT_FOLDER;
	//Debug.Log("rootPath:"+ rootPath);
	if (!Directory.Exists(rootPath))
		Directory.CreateDirectory(rootPath);
	AssetDatabase.Refresh();

#if UNITY_EDITOR_WIN
	//rootPath = rootPath.Replace(mCustomDirectory ? Application.dataPath : NG_Utils.UnityToSystemPath( Application.dataPath ), "").Replace(@"\", "/");
#else
	//rootPath = rootPath.Replace(Application.dataPath, "");
#endif

	//Debug.Log("Application.dataPath:" + Application.dataPath);
	var originalMeshName = GetSafeFilename(originalMesh.name);
	originalMeshName = originalMeshName.Remove(originalMeshName.Length - 6);//删除index
	//var assetPath = "Assets" + rootPath;
	var assetPath = rootPath;
	//Debug.Log("assetPath:"+ assetPath);
	var newAssetName = originalMeshName + "_" + MESH_SUFFIX + ".mesh";
	if(originalMeshName.Contains(MESH_SUFFIX))
	{
		newAssetName = originalMeshName + ".mesh";
	}
	assetPath += newAssetName;
	var existingAsset = AssetDatabase.LoadAssetAtPath(assetPath, typeof(Mesh)) as Mesh;
	var assetExists = (existingAsset != null) && originalMesh.isAsset;
	if(assetExists)
	{
		if(!mAlwaysOverwrite)
			overwrite = EditorUtility.DisplayDialog("NG : Smoothed Mesh", "The following smoothed mesh already exists:\n\n" + newAssetName + "\n\nOverwrite?", "Yes", "No");

		if(!overwrite)
		{
			return null;
		}

		originalMesh.mesh = existingAsset;
		originalMesh.name = existingAsset.name;
	}
	Mesh polyMesh = null;
	PolyfewRuntime.SimplificationOptions opt = new PolyfewRuntime.SimplificationOptions();
	Debug.LogError("originalMesh.name:"+ originalMesh.name);
	if (originalMesh.name.Contains("face"))
	{
		//减面
		opt.simplificationStrength = 0;
	}
	else
	{
		opt.simplificationStrength = 40;
	}

	opt.preserveBorderEdges = true;
	opt.regardCurvature = true;
	opt.preserveUVFoldoverEdges = true;
	var polyMeshes = PolyfewRuntime.SimplifyMeshes(new List<Mesh> { originalMesh.mesh }, opt, null);

	if (polyMeshes != null && polyMeshes.Count > 0)
	{
		polyMesh = polyMeshes[0];
	}
	//平滑法线
	Mesh smoothedMesh = null;
	if (polyMesh != null)
	{
		smoothedMesh = NG_Utils.CreateSmoothedMesh(polyMesh, mFormat, mVColors, mTangents, mUV2, !originalMesh.isAsset || (originalMesh.isAsset && assetExists));
		Debug.Log("simplify success!");
	}
	else
	{
		Debug.LogError("Couldn't simplify the mesh for:\n" + originalMesh.name);
	}

	if (smoothedMesh != null)
	{
		if (originalMesh.associatedObjects != null)
		{
			Undo.RecordObjects(originalMesh.associatedObjects, "Assign NG Smoothed Mesh to Selection");
			foreach (var o in originalMesh.associatedObjects)
			{
				if (o is SkinnedMeshRenderer)
				{
					(o as SkinnedMeshRenderer).sharedMesh = smoothedMesh;
				}
				else if (o is MeshFilter)
				{
					(o as MeshFilter).sharedMesh = smoothedMesh;
				}
				else
				{
					Debug.LogWarning("[NG Smoothed Normals Utility] Unrecognized AssociatedObject: " + o + "\nType: " + o.GetType());
				}
				EditorUtility.SetDirty(o);
			}
		}

		if (originalMesh.isAsset)
		{
			if (overwrite)// && !assetExists)
			{
				AssetDatabase.CreateAsset(smoothedMesh, assetPath);
				Debug.LogError("overwrite!");
			}
		}
		else return null;


	}
	else
	{
		Debug.LogError("Couldn't smooth the mesh for:\n" + originalMesh.name);
		//ShowNotification(new GUIContent("Couldn't generate the mesh for:\n" + originalMesh.name));
	}

	return smoothedMesh;
}

private static Dictionary<Mesh, SelectedMesh> GetSelectedMeshes(GameObject go)
{
	var meshDict = new Dictionary<Mesh, SelectedMesh>();
	var renderers = go.GetComponentsInChildren<SkinnedMeshRenderer>();
	Debug.Log("renderers.count:" + renderers.Length);
	foreach (var r in renderers)
	{
		if (r.sharedMesh != null)
		{
			if (meshDict.ContainsKey(r.sharedMesh))
			{
				var sm = meshDict[r.sharedMesh];
				sm.AddAssociatedObject(r);
			}
			else
			{
				if (r.sharedMesh.name.Contains(MESH_SUFFIX))
				{
					meshDict.Add(r.sharedMesh, new SelectedMesh(r.sharedMesh, r.sharedMesh.name, false));
				}
				else
				{
					if (r.sharedMesh != null)
					{
						var sm = GetMeshToAdd(r.sharedMesh, true, r);
						if (sm.mesh != null)
						{
							meshDict.Add(r.sharedMesh, sm);
						}

					}
				}
			}
		}
	}


	var mfilters = go.GetComponentsInChildren<MeshFilter>();
	foreach (var mf in mfilters)
	{
		if (mf.sharedMesh != null)
		{
			if (meshDict.ContainsKey(mf.sharedMesh))
			{
				var sm = meshDict[mf.sharedMesh];
				sm.AddAssociatedObject(mf);
			}
			else
			{
				if (mf.sharedMesh.name.Contains(MESH_SUFFIX))
				{
					meshDict.Add(mf.sharedMesh, new SelectedMesh(mf.sharedMesh, mf.sharedMesh.name, false));
				}
				else
				{
					if (mf.sharedMesh != null)
					{
						var sm = GetMeshToAdd(mf.sharedMesh, true, mf);
						if (sm.mesh != null)
							meshDict.Add(mf.sharedMesh, sm);
					}
				}
			}
		}
	}
	return meshDict;
}

private static SelectedMesh GetMeshToAdd(Mesh mesh, bool isProjectAsset, Object _assoObj = null)
{
	var meshPath = AssetDatabase.GetAssetPath(mesh);
	var meshAsset = AssetDatabase.LoadAssetAtPath(meshPath, typeof(Mesh)) as Mesh;
	//If null, it can be a built-in Unity mesh
	if(meshAsset == null)
	{
		return new SelectedMesh(mesh, mesh.name, isProjectAsset, _assoObj);
	}
	var meshName = mesh.name;
	if(!AssetDatabase.IsMainAsset(meshAsset))
	{

		var main = AssetDatabase.LoadMainAssetAtPath(meshPath);
		meshName = main.name + " - " + meshName + "_" + mesh.GetInstanceID();
		//Debug.LogError("meshName:"+ meshName   );
	}

	var sm = new SelectedMesh(mesh, meshName, isProjectAsset, _assoObj);
	return sm;
}
*/
}
