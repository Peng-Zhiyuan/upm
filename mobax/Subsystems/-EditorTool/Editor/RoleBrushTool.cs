using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System.IO;
using System;
using System.Threading.Tasks;
public class RoleBrushTool
{
   
    private const string ROLE_PATH = "Assets/RoleSprites";
	//[MenuItem ("Assets/CustomBrushTools/RolePrefabBrush")]
	static async void RolePrefabBrush () {

		//收集游戏中正在使用的sprite
		//List<Sprite> spriteList = LoadAllAssetsAtPath<Sprite> (MAP_PATH);

		//Dictionary<string, Sprite> spriteDic = new Dictionary<string, Sprite> ();
        //foreach (Sprite sp in spriteList)
        //{
        //    if (spriteDic.ContainsKey(sp.name))
        //    {
        //        Debug.LogError("sp has same key:" + sp.name);
        //        continue;
        //    }
        //    spriteDic.Add(sp.name, sp);
        //}



        UnityEngine.Object[] selection = Selection.GetFiltered (typeof (object), SelectionMode.DeepAssets);
		foreach (UnityEngine.Object activeObject in selection) {
			string resPath = AssetDatabase.GetAssetPath (activeObject);
			GameObject go = AssetDatabase.LoadAssetAtPath (resPath, typeof (GameObject)) as GameObject;
			if(go == null) continue;
			string dirName = go.name;
			string fullPath = GetFullPath(ROLE_PATH + "/"+ dirName);
			if (!Directory.Exists(fullPath))
			{
				Directory.CreateDirectory(fullPath);
			}
			RectTransform rectTransform = go.GetComponent<RectTransform>();
			rectTransform.sizeDelta.Set(960, 1280);
			//收集新加的sprite
			var imageList = go.GetComponentsInChildren<Image> (true);
			foreach (Image image in imageList) {
				if (image == null || image.sprite == null) continue;
				var spriteName = GetValidSpriteName(image.sprite.name);
				var sprite = image.sprite;
				string spritePath = AssetDatabase.GetAssetPath (sprite);
				if(!spritePath.StartsWith(ROLE_PATH))
				{
					var fileExtension = Path.GetExtension(spritePath);
					var fullSpriteName = spriteName + fileExtension;
					var tarPath =  ROLE_PATH + "/" + dirName  + "/" + fullSpriteName;
					Debug.Log(spritePath+"=>"+tarPath);
					AssetDatabase.DeleteAsset(tarPath);
					AssetDatabase.MoveAsset(spritePath, tarPath);
					AssetDatabase.ImportAsset(tarPath);
				}
			}

			var transformList = GetAllChildren (go.transform);
		
			var temp = GameObject.Instantiate(go);
			var canvas = temp.gameObject.GetOrAddComponent<Canvas>();
			canvas.renderMode = RenderMode.ScreenSpaceOverlay;
			//canvas.
			await Task.Delay(500);
			if (canvas.gameObject.activeInHierarchy && canvas.enabled)
			{
				canvas.overrideSorting = true;
				canvas.sortingOrder = -1;
			}
			else
			{
				Debug.LogError("Cannot change Canvas overrideSorting");
			}

			// if(transformList.Count > 1 && transformList[1].name != "BattleLayer")
			// {
			// 	var layer = MonoUtil.CreateGameobjectWithComponent<RectTransform>("BattleLayer");
			// 	layer.transform.SetParent(temp.transform);
			// 	layer.transform.SetSiblingIndex(1);
			// 	layer.localScale = Vector3.one;
			// 	layer.localPosition = Vector3.zero;
			// 	layer.sizeDelta = new Vector2(720,1280);
			// 	layer.gameObject.layer = LayerMask.NameToLayer("UI");
			// }
			PrefabUtility.SaveAsPrefabAsset (temp, resPath);
			MonoBehaviour.DestroyImmediate(temp);
			
		
			go.layer = LayerMask.NameToLayer("UI");
			foreach (var trans in transformList)
			{
				trans.gameObject.layer = LayerMask.NameToLayer("UI"); 
			}
			
			// foreach (Transform transform in transformList) {

			// 	// if (!transform.name.EndsWith ("Button")) continue;
			// 	if (!transform.gameObject.name.StartsWith ("$")) {
			// 		transform.gameObject.name = "$" + transform.gameObject.name;
			// 	}
			// }

			//绑定codelinker系统
			//var codeLinkerObject = go.GetOrAddComponent<CodeLinkerObject> ();
			//var mu = go.GetOrAddComponent<RoleUnit> ();
			//ElementSystem.CodeLinkerEditorUtils.GenerateCodeForTree (codeLinkerObject);
			EditorUtility.SetDirty (go);
		}
		AssetDatabase.SaveAssets ();
		AssetDatabase.Refresh ();
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
	private static string GetValidSpriteName(string spriteName)
	{
		var index = spriteName.IndexOf(" (");
		if(index >= 0)
		{
			spriteName = spriteName.Substring(0,index);
		}
		return spriteName;
	}
}
