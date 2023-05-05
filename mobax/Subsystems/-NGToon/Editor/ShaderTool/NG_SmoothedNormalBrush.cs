// NG Shader EditorV1.0
// (c) 2019-2020

using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

// Utility to generate meshes with encoded smoothed normals, to fix hard-edged broken outline

public class NG_SmoothedNormalBrush
{
	[MenuItem("Assets/CustomBrushTools/SmoothedNormalBrush")]
	static void SmoothedNormalBrush()
	{
		foreach (var o in Selection.objects)
		{
			DoSmoothedNormal(o);
		}
	}
	
	[MenuItem("Assets/CustomBrushTools/ClearBrushProgressBar")]
	static void ClearBrushProgressBar()
	{
		EditorUtility.ClearProgressBar();
	}


	[MenuItem("Assets/CustomBrushTools/ClearAttaches")]
	static void ClearAttaches()
	{
		Object[] selection = Selection.GetFiltered(typeof(object), SelectionMode.DeepAssets);
		foreach (Object activeObject in selection)
		{
			DoClearAttaches(activeObject);
		}
	}

	static void DoClearAttaches(Object o)
	{
		var path = AssetDatabase.GetAssetPath(o);
		var dir = Path.GetDirectoryName(path);
		if (o is GameObject)
		{
			var extendName = Path.GetExtension(path);
			bool isPrefab = extendName.ToLower() == ".prefab";
			if (isPrefab)
			{
				var go = PrefabUtility.LoadPrefabContents(path) as GameObject;
				AttachBone attachBone = go.GetComponent<AttachBone>();
				if (attachBone != null)
				{
					attachBone.Clear();
					PrefabUtility.SaveAsPrefabAssetAndConnect(go, path, InteractionMode.AutomatedAction);
					Debug.Log("Clear succes");
				}
				GameObject.DestroyImmediate(go);
				
			}
		}
	}

		static void DoSmoothedNormal(Object o)
		{
			var path = AssetDatabase.GetAssetPath(o);
			var dir = Path.GetDirectoryName(path);
			mCurrentDirectoryPath = dir;
			if (o is GameObject)
			{
				var extendName = Path.GetExtension(path);
				bool isPrefab = extendName.ToLower() == ".prefab";
				//bool isFBX = extendName.ToLower() == ".fbx";
				if (isPrefab)
				{
					//AttachBone.forbidAttach = true;
					//var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

					var go = PrefabUtility.LoadPrefabContents(path) as GameObject;
					/*
					var attachBone = go.GetComponent<AttachBone>();
					if (attachBone != null && attachBone.Part != null)
					{
						var part = go.transform.FindInChildren(attachBone.Part.name);
						if (part != null)
						{
							GameObject.DestroyImmediate(part);
						}
					}*/

					mMeshes = GetSelectedMeshes(go);
					CreateSmoothedMeshes(go.name);
					PrefabUtility.SaveAsPrefabAssetAndConnect(go, path, InteractionMode.AutomatedAction);
					GameObject.DestroyImmediate(go);
					//AttachBone.forbidAttach = false;
				}
				/*else if (isFBX)
				{
					var fbx = AssetDatabase.LoadAssetAtPath<GameObject>(path);
					mMeshes = GetSelectedMeshes(fbx);
					CreateSmoothedMeshes();
				}*/
				else
				{
					Debug.LogError("Not support type:" + extendName.ToLower());
				}

			}
			/*else if (o is Mesh)
			{
				mMeshes = new Dictionary<Mesh, SelectedMesh>();
				var sm = GetMeshToAdd(o as Mesh, true);
				if (sm != null)
				{
					mMeshes.Add(o as Mesh, sm);
				}
				CreateSmoothedMeshes();
			}*/
			else
			{
				Debug.LogError("Not support type:" + o.GetType().ToString());
			}


		}

		private static void CreateSmoothedMeshes(string output_folder)
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
				EditorUtility.DisplayProgressBar("Hold On", (mMeshes.Count > 1 ? "Generating Smoothed Meshes:\n" : "Generating Smoothed Mesh:\n") + sm.name, progress / total);
				progress++;
				var asset = CreateSmoothedMeshAsset(sm, output_folder);
				if (asset != null)
				{
					Debug.Log("smoothed:" + asset.name);
				}
			}
			EditorUtility.ClearProgressBar();
			Debug.Log("Total Smoothed Success!!!");
		}
	

	//--------------------------------------------------------------------------------------------------
	// INTERFACE

	private const string MESH_SUFFIX = "smoothed";//"[NG Smoothed]";
/*#if UNITY_EDITOR_WIN
	private const string OUTPUT_FOLDER = "\\SmoothedMeshes\\";
#else
	private const string OUTPUT_FOLDER = "/SmoothedMeshes/";
#endif*/

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


	//--------------------------------------------------------------------------------------------------
	/*
	private void LoadUserPrefs()
	{
		mAlwaysOverwrite = EditorPrefs.GetBool("NGSMU_mAlwaysOverwrite", false);
		mCustomDirectory = EditorPrefs.GetBool("NGSMU_mCustomDirectory", false);
		mCustomDirectoryPath = EditorPrefs.GetString("NGSMU_mCustomDirectoryPath", "/");
	}
	
	private void SaveUserPrefs()
	{
		EditorPrefs.SetBool("NGSMU_mAlwaysOverwrite", mAlwaysOverwrite);
		EditorPrefs.SetBool("NGSMU_mCustomDirectory", mCustomDirectory);
		EditorPrefs.SetString("NGSMU_mCustomDirectoryPath", mCustomDirectoryPath);
	}

	void OnEnable() { LoadUserPrefs(); }
	void OnDisable() { SaveUserPrefs(); }
		
	void OnFocus()
	{
		mMeshes = GetSelectedMeshes();
	}

	void OnSelectionChange()
	{
		mMeshes = GetSelectedMeshes();
	//	Repaint();
	}
	
	void OnGUI()
	{
		EditorGUILayout.BeginHorizontal();
		NG_GUI.HeaderBig("NG - SMOOTHED NORMALS UTILITY");
		NG_GUI.HelpButton("Smoothed Normals Utility");
		EditorGUILayout.EndHorizontal();
		NG_GUI.Separator();
		if(mMeshes != null && mMeshes.Count > 0)
		{
			GUILayout.Space(4);
			NG_GUI.Header("Meshes ready to be processed:", null, true);
			mScroll = EditorGUILayout.BeginScrollView(mScroll);
			NG_GUI.GUILine(Color.gray, 1);
			foreach(var sm in mMeshes.Values)
			{
				GUILayout.Space(2);
				GUILayout.BeginHorizontal();
				var label = sm.name;
				if(label.Contains(MESH_SUFFIX))
					label = label.Replace(MESH_SUFFIX, "\n" + MESH_SUFFIX);
				GUILayout.Label(label, EditorStyles.wordWrappedMiniLabel, GUILayout.Width(270));
				sm.isSkinned = GUILayout.Toggle(sm.isSkinned, new GUIContent("Skinned", "Should be checked if the mesh will be used on a SkinnedMeshRenderer"), EditorStyles.toolbarButton);
				GUILayout.Space(6);
				GUILayout.EndHorizontal();
				GUILayout.Space(2);
				NG_GUI.GUILine(Color.gray, 1);
			}
			EditorGUILayout.EndScrollView();

			GUILayout.FlexibleSpace();
			if(GUILayout.Button(mMeshes.Count == 1 ? "Generate Smoothed Mesh" : "Generate Smoothed Meshes", GUILayout.Height(30)))
			{
				var selection = new List<Object>();
				float progress = 1;
				float total = mMeshes.Count;
				foreach(var sm in mMeshes.Values)
				{
					if(sm == null)
						continue;

					EditorUtility.DisplayProgressBar("Hold On", (mMeshes.Count > 1 ? "Generating Smoothed Meshes:\n" : "Generating Smoothed Mesh:\n") + sm.name, progress/total);
					progress++;
					Object o = CreateSmoothedMeshAsset(sm);
					if(o != null)
						selection.Add(o);
				}
				EditorUtility.ClearProgressBar();
				Selection.objects = selection.ToArray();
			}
		}
		else
		{
			EditorGUILayout.HelpBox("Select one or multiple meshes to create a smoothed normals version.\n\nYou can also select models directly in the Scene, the new mesh will automatically be assigned.", MessageType.Info);
			GUILayout.FlexibleSpace();
			using(new EditorGUI.DisabledScope(true))
				GUILayout.Button("Generate Smoothed Mesh", GUILayout.Height(30));
		}

		NG_GUI.Separator();

		NG_GUI.Header("Store smoothed normals in:", "You will have to select the correct option in the Material Inspector when using outlines", true);
		
		var choice =	0;
		if(mTangents)	choice = 1;
		if(mUV2)		choice = 2;
		choice = NG_GUI.RadioChoice(choice, true, "Vertex Colors", "Tangents", "UV2");
		EditorGUILayout.HelpBox("Smoothed Normals for Skinned meshes will be stored in Tangents only. See Help to know why.", MessageType.Warning);
		
		mVColors	= (choice == 0);
		mTangents	= (choice == 1);
		mUV2		= (choice == 2);

		NG_GUI.Separator();

		NG_GUI.Header("Options", null, true);
		mAlwaysOverwrite = EditorGUILayout.Toggle(new GUIContent("Always Overwrite", "Will always overwrite existing [NG Smoothed] meshes"), mAlwaysOverwrite);
		mCustomDirectory = EditorGUILayout.Toggle(new GUIContent("Custom Output Directory", "Save the generated smoothed meshes in a custom directory"), mCustomDirectory);
		using(new EditorGUI.DisabledScope(!mCustomDirectory))
		{
			EditorGUILayout.BeginHorizontal();
			mCustomDirectoryPath = EditorGUILayout.TextField(GUIContent.none, mCustomDirectoryPath);
			if(GUILayout.Button("Select...", EditorStyles.miniButton, GUILayout.ExpandWidth(false)))
			{
				var outputPath = NG_Utils.OpenFolderPanel_ProjectPath("Choose custom output directory for generated smoothed meshes");
				if(!string.IsNullOrEmpty(outputPath))
				{
					mCustomDirectoryPath = outputPath;
				}
			}
			EditorGUILayout.EndHorizontal();
		};

		EditorGUILayout.BeginHorizontal();
		GUILayout.Label("In case of error:", GUILayout.Width(EditorGUIUtility.labelWidth));
		if(GUILayout.Button(new GUIContent("Clear Progress Bar", "Clears the progress bar if it's hanging on screen after an error."), EditorStyles.miniButton))
		{
			EditorUtility.ClearProgressBar();
		}
		EditorGUILayout.EndHorizontal();
	}
	*/
	//--------------------------------------------------------------------------------------------------

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

	private static Mesh CreateSmoothedMeshAsset(SelectedMesh originalMesh, string output_folder)
	{
		//Check if we are ok to overwrite
		var overwrite = true;

		var rootPath = mCustomDirectory ? Application.dataPath + "/" + mCustomDirectoryPath + "/" : mCurrentDirectoryPath + "/" + "SmoothedMeshes_" + output_folder ;// +OUTPUT_FOLDER;
		//Debug.Log("rootPath:"+ rootPath);
		if (!Directory.Exists(rootPath))
			Directory.CreateDirectory(rootPath);
		AssetDatabase.Refresh();
		/*
#if UNITY_EDITOR_WIN
		rootPath = rootPath.Replace(mCustomDirectory ? Application.dataPath : NG_Utils.UnityToSystemPath( Application.dataPath ), "").Replace(@"\", "/");
#else
		rootPath = rootPath.Replace(Application.dataPath, "");
#endif
		*/
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

		Mesh newMesh = null;
		/*if(originalMesh.isSkinned)
		{
			newMesh = NG_Utils.CreateSmoothedMesh(originalMesh.mesh, mFormat, false, true, false, !originalMesh.isAsset || (originalMesh.isAsset && assetExists));
		}
		else
		{*/
			newMesh = NG_Utils.CreateSmoothedMesh(originalMesh.mesh, mFormat, mVColors, mTangents, mUV2, !originalMesh.isAsset || (originalMesh.isAsset && assetExists));
	//	}

		if(newMesh == null)
		{
			Debug.LogError("Couldn't generate the mesh for:\n" + originalMesh.name);
			//ShowNotification(new GUIContent("Couldn't generate the mesh for:\n" + originalMesh.name));
		}
		else
		{
			if(originalMesh.associatedObjects != null)
			{
				Undo.RecordObjects(originalMesh.associatedObjects, "Assign NG Smoothed Mesh to Selection");

				foreach(var o in originalMesh.associatedObjects)
				{
					if(o is SkinnedMeshRenderer)
					{
						(o as SkinnedMeshRenderer).sharedMesh = newMesh;
					}
					else if(o is MeshFilter)
					{
						(o as MeshFilter).sharedMesh = newMesh;
					}
					else
					{
						Debug.LogWarning("[NG Smoothed Normals Utility] Unrecognized AssociatedObject: " + o + "\nType: " + o.GetType());
					}
					EditorUtility.SetDirty(o);
				}
			}

			if(originalMesh.isAsset)
			{
				if(overwrite && !assetExists)
				{
					AssetDatabase.CreateAsset(newMesh, assetPath);
				}
			}
			else
				return null;
		}

		return newMesh;
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
		//Debug.Log("mfilters.count:"+ mfilters.Length);
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
}
