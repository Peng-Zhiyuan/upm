using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System.IO;
using System;
using System.Threading.Tasks;
public class ParticleBrushTool
{
	private static string TEXTURE_PATH = "Assets/ArtResources/Textures/";
	private static string MAT_PATH = "Assets/ArtResources/Materials/";
	private static string SHADER_PATH = "Assets/ArtResources/Shader/";
	private static string ANIM_PATH = "Assets/ArtResources/Anim/";
	private static string MODEL_PATH = "Assets/ArtResources/Models/";
	//[MenuItem ("Assets/CustomBrushTools/ParticlePrefabBrush")]
	static  void ParticlePrefabBrush() {

        UnityEngine.Object[] selection = Selection.GetFiltered (typeof (object), SelectionMode.DeepAssets);
	    UnityEngine.Object[] dependObjs = EditorUtility.CollectDependencies(selection);

		foreach (var obj in dependObjs)
		{
			string resPath = AssetDatabase.GetAssetPath(obj);
			if (resPath.EndsWith(".tga"))
			{
				CheckToMove(resPath, TEXTURE_PATH);
			}
			else if (resPath.EndsWith(".png"))
			{
				CheckToMove(resPath, TEXTURE_PATH);
			}
			else if (resPath.EndsWith(".jpg"))
			{
				CheckToMove(resPath, TEXTURE_PATH);
			}
			else if (resPath.EndsWith(".mat"))
			{
				CheckToMove(resPath, MAT_PATH);
			}
			else if (resPath.EndsWith(".shader"))
			{
				CheckToMove(resPath, SHADER_PATH);
			}
			else if (resPath.EndsWith(".anim"))
			{
				CheckToMove(resPath, ANIM_PATH);
			}
			else if (resPath.EndsWith(".FBX"))
			{
				CheckToMove(resPath, MODEL_PATH);
			}
			// Debug.Log("depend resPath:"+resPath);
		}
	
    

        foreach (UnityEngine.Object activeObject in selection) 
		{
			string resPath = AssetDatabase.GetAssetPath (activeObject);
			GameObject go = AssetDatabase.LoadAssetAtPath (resPath, typeof (GameObject)) as GameObject;
			if(go == null) 
			{
				CheckChangeMat(resPath);
				continue;
			}
			ParticleEffectUnit particleEffectUnit = go.GetOrAddComponent<ParticleEffectUnit>();
			// TODO: 这个东西不删掉了，需要的话可以拿回来
			//UIParticleScale particleScale = go.GetOrAddComponent<UIParticleScale>();

		    // var particles = go.GetComponentsInChildren<ParticleSystem> (true);
			// foreach(var particle in particles)
			// {
			// 	particle.
			// }
		
			var rendererList = go.GetComponentsInChildren<ParticleSystemRenderer> (true);
			foreach(var renderer in rendererList)
			{
				Mesh[] meshs = new Mesh[5];
				var count = renderer.GetMeshes(meshs);
				if (count > 0)
				{
					Mesh[] validMeshs = new Mesh[count];

					for (int i = 0; i < count; i++)
					{
						validMeshs[i] = meshs[i];
					}

					UnityEngine.Object[] depends = EditorUtility.CollectDependencies(validMeshs);
				
					if (validMeshs.Length == 1 && depends.Length == 1)
					{
						for (int i = 0; i < depends.Length; i++)
						{
							string meshDependPath = AssetDatabase.GetAssetPath(depends[i]);
							Debug.Log(depends[i].name + "==>meshDependPath:" + meshDependPath);
							if (meshDependPath.EndsWith(".FBX") && !meshDependPath.StartsWith(MODEL_PATH))
							{
								var fbxName = Path.GetFileName(meshDependPath);
								var fbx = AssetDatabase.LoadAssetAtPath<GameObject>(MODEL_PATH + fbxName);
								validMeshs[i] = fbx.GetComponent<MeshFilter>().sharedMesh;
							}
						}
						renderer.SetMeshes(validMeshs);
					}
					else
					{
						Debug.LogError("validMeshs.Length:" + validMeshs.Length + "   depends.Length:" + depends.Length);
					}

				}

				Material[] mats = renderer.sharedMaterials;
				foreach(var mat in mats)
				{
					if(mat == null) continue;
					var textureNames = mat.GetTexturePropertyNames();
					foreach(var textureName in textureNames)
					{
						var texture = mat.GetTexture(textureName);
						Debug.Log("==>depend textureName:" + textureName);
						if(texture == null || texture.name == "Default-Particle") continue;
						var tex = LoadTexture(texture.name + ".tga");
						if(tex == null) tex = LoadTexture(texture.name + ".png");
						if (tex == null) tex = LoadTexture(texture.name + ".jpg");
						if (tex != null)
						{
							//if (textureName.Contains("_Color"))
							//{
							//	Debug.Log("图片_Color:" + texture.name + go.name);
							//}
						    mat.SetTexture(textureName, tex);
						}
						else
						{
							Debug.LogError("图片不存在:" + texture.name);
						}
					}
				}
				//if(renderer.maskInteraction == SpriteMaskInteraction.None)
				{
					renderer.maskInteraction = SpriteMaskInteraction.None;
				}
			
				if(renderer.sortingOrder == 0) 
				{
					Debug.Log("renderQueue切换成功:"+renderer.sortingOrder +"=>"+"1");
					renderer.sortingOrder = 1;
				}
				else if(renderer.sortingOrder == 100)
				{
					Debug.Log("renderQueue切换成功:"+renderer.sortingOrder +"=>"+"101");
					renderer.sortingOrder = 101;
				}
			}
			var transformList = GetAllChildren (go.transform);
		
		
			go.layer = LayerMask.NameToLayer("UI");
			foreach (var trans in transformList)
			{
				trans.gameObject.layer = LayerMask.NameToLayer("UI"); 
			}
			// var mu = go.GetComponent<MapUnit> ();
			// if(mu != null) MonoBehaviour.DestroyImmediate(mu,true);
			
			EditorUtility.SetDirty (go);
		}
		AssetDatabase.SaveAssets ();
		AssetDatabase.Refresh ();
	}

	private static Texture LoadTexture(string name)
	{
		string targetPath = TEXTURE_PATH + name;
		string fullPath = GetFullPath(targetPath);
		if (File.Exists(fullPath))
		{
			var tex = AssetDatabase.LoadAssetAtPath<Texture>(targetPath);
			return tex;
		}
		return null;
	}

	private static void CheckToMove(string resPath, string targetDir)
	{
		if(!resPath.StartsWith(targetDir))
		{
			var tarPath = targetDir + Path.GetFileName(resPath);
			if(!File.Exists(GetFullPath(tarPath)))
			{
				AssetDatabase.MoveAsset(resPath, tarPath);
				AssetDatabase.ImportAsset(tarPath);
				Debug.Log("迁移:"+resPath+"=>"+tarPath);
			}
			else
			{
				Debug.Log("已经存在:"+resPath+"=>"+tarPath);
			}
		}
	}

	private static void CheckChangeMat(string resPath)
	{
		var mat =  AssetDatabase.LoadAssetAtPath (resPath, typeof (Material)) as Material;
		if(mat == null) return;
		var textureNames = mat.GetTexturePropertyNames();
		foreach(var textureName in textureNames)
		{
			var texture = mat.GetTexture(textureName);
			Debug.Log("==>depend textureName:" + textureName);
			if(texture == null || texture.name == "Default-Particle") continue;
			var tex = LoadTexture(texture.name + ".tga");
			if(tex == null) tex = LoadTexture(texture.name + ".png");
			if (tex == null) tex = LoadTexture(texture.name + ".jpg");
			if (tex != null)
			{
				//if (textureName.Contains("_Color"))
				//{
				//	Debug.Log("图片_Color:" + texture.name + go.name);
				//}
				mat.SetTexture(textureName, tex);
			}
			else
			{
				Debug.LogError("图片不存在:" + texture.name);
			}
		}
	}
   private static List<T> LoadAllAssetsAtPath<T> (string path) where T : UnityEngine.Object {
		List<T> list = new List<T> ();
		path = path.Replace ("Assets", "");
		string[] spritesPath = Directory.GetFiles (Application.dataPath + path, "*", SearchOption.AllDirectories);
		//循环遍历每一个路径，单独加P
		foreach (string spritePath in spritesPath) {     //替换路径中的反斜杠为正斜杠       
			string tempPath = spritePath.Replace (@"\", "/");     //截取我们需要的路径
			tempPath = tempPath.Substring (tempPath.IndexOf ("Assets"));     //根据路径加载资源
			T obj = AssetDatabase.LoadAssetAtPath<T> (tempPath);    
			if (obj != null) list.Add (obj);
		}
		return list;
	}
    private static string GetFullPath(string path)
	{
		return Application.dataPath + path.Replace ("Assets", "");
	}
    private static List<Transform> GetAllChildren (Transform transform) {

		List<Transform> allChildren = new List<Transform> ();
		Queue<Transform> nextChildren = new Queue<Transform> ();
		nextChildren.Enqueue (transform);
		while (nextChildren.Count > 0) {
			transform = nextChildren.Dequeue ();
			var count = transform.childCount;
			for (var index = 0; index < count; index++) {
				var child = transform.GetChild (index);
				nextChildren.Enqueue (child);
				allChildren.Add (child);
			}
		}
		return allChildren;

	}
}
