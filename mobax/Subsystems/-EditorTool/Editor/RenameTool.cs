using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
public class RenameTool {

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

	private static string GetValidSpriteName(string spriteName)
	{
		var index = spriteName.IndexOf(" (");
		if(index >= 0)
		{
			spriteName = spriteName.Substring(0,index);
		}
		return spriteName;
	}

	[MenuItem ("Assets/RenameTool/Rename")]
	static void UIPrefabBrush () {

		// //收集游戏中正在使用的sprite
		// List<Sprite> spriteList = LoadAllAssetsAtPath<Sprite> (SPRITE_PATH);
		// //Debug.Log ("spriteList:" + spriteList.Count);
		// Dictionary<string, Sprite> spriteDic = new Dictionary<string, Sprite> ();
		// foreach (Sprite sp in spriteList) {
		// 	if (spriteDic.ContainsKey (sp.name)) {
		// 		Debug.LogError ("sp has same key:" + sp.name);
		// 		continue;
		// 	}
		// 	Debug.LogError ("sp.name:" + sp.name);
		// 	spriteDic.Add (sp.name, sp);
		// }
		// string fullPath = GetFullPath(SPRITE_PATH+"/Temp");
		// if(!Directory.Exists(fullPath))
		// {
		// 	Directory.CreateDirectory(fullPath);
		// }
	
		Object[] selection = Selection.GetFiltered (typeof (object), SelectionMode.DeepAssets);
		foreach (Object activeObject in selection) {
			string resPath = AssetDatabase.GetAssetPath (activeObject);
            Debug.LogError("resPath:"+resPath);
			// GameObject go = AssetDatabase.LoadAssetAtPath (resPath, typeof (GameObject)) as GameObject;
			// if(go == null) continue;
		
			// //收集新加的sprite
			// var imageList = go.GetComponentsInChildren<Image> (true);
			// foreach (Image image in imageList) {
			// 	if (image == null || image.sprite == null) continue;
			// 	var spriteName = GetValidSpriteName(image.sprite.name);
			// 	var sprite = image.sprite;
			// 	string spritePath = AssetDatabase.GetAssetPath (sprite);
			// 	if(!spriteDic.ContainsKey (spriteName))//!spritePath.StartsWith(SPRITE_PATH) &&
			// 	{
			// 		var fileExtension = Path.GetExtension(spritePath);
			// 		var fullSpriteName = spriteName + fileExtension;
			// 		//Debug.Log(spritePath+"=>"+SPRITE_PATH+"/Temp/"+fileName);
			// 		AssetDatabase.CopyAsset(spritePath, SPRITE_PATH+"/Temp/"+ fullSpriteName);
			// 		AssetDatabase.ImportAsset(SPRITE_PATH+"/Temp/"+ fullSpriteName);
            //         Debug.LogError("spriteName:" + spriteName + "   spritePath:" + spritePath);
			// 		var tarSprite = AssetDatabase.LoadAssetAtPath<Sprite> (SPRITE_PATH+"/Temp/"+ fullSpriteName);
			// 		Debug.LogError("   add tarSprite.name:"+ tarSprite.name);
			// 		spriteDic.Add(tarSprite.name, tarSprite);
			// 	}
			// }

			// //替换sprite引用，并修正填充方式
			// foreach (Image image in imageList) {
			// 	if (image == null || image.sprite == null) continue;
			// 	var spriteName = GetValidSpriteName(image.sprite.name);
			// 	//Debug.Log("spriteName=>"+spriteName);
			// 	if (spriteDic.ContainsKey (spriteName)) {
			// 		var targetSprite = spriteDic[spriteName];
			// 		if(targetSprite == null) 
			// 		{
			// 			Debug.LogError("sprite is null=>"+spriteName);
			// 			continue;
			// 		}
			// 		image.sprite = targetSprite;
				
			// 		if (targetSprite.border.x + targetSprite.border.y + targetSprite.border.z + targetSprite.border.w > 0) {
			// 			image.type = Image.Type.Sliced;
			// 		} else {
			// 			image.type = Image.Type.Simple;
			// 		}
			// 	}
			// }

			// //绑定UIButton
			// var transformList = GetAllChildren (go.transform);
			// foreach (Transform transform in transformList) {
			// 	if (!transform.name.EndsWith ("Button")) continue;
			// 	if (!transform.gameObject.name.StartsWith ("$")) {
			// 		transform.gameObject.name = "$" + transform.gameObject.name;
			// 	}
			// 	var button = transform.gameObject.GetOrAddComponent<Button> ();
			// 	var buttonSe = transform.gameObject.GetOrAddComponent<ButtonSe> ();
			// }
			// //绑定OutLine和UILocalize
			// var textList = go.GetComponentsInChildren<Text> (true);
			// foreach (Text text in textList) {
			// 	if (text == null) continue;
			// 	text.alignment = TextAnchor.MiddleCenter;
			// 	text.fontSize = (int)text.GetComponent<RectTransform>().rect.height;
			// 	var uiLocalize = text.gameObject.GetOrAddComponent<UILocalization> ();
			// 	var outLine = text.gameObject.GetOrAddComponent<Outline> ();
			// }

			// //绑定codelinker系统
			// var codeLinkerObject = go.GetOrAddComponent<CodeLinkerObject> ();
			// //ElementSystem.CodeLinkerEditorUtils.GenerateCodeForTree (codeLinkerObject);
			// EditorUtility.SetDirty (go);
			

		}
		AssetDatabase.SaveAssets ();
		AssetDatabase.Refresh ();
	}

	// private static List<Transform> GetAllChildren (Transform transform) {

	// 	List<Transform> allChildren = new List<Transform> ();
	// 	Queue<Transform> nextChildren = new Queue<Transform> ();
	// 	nextChildren.Enqueue (transform);
	// 	while (nextChildren.Count > 0) {
	// 		transform = nextChildren.Dequeue ();
	// 		var count = transform.childCount;
	// 		for (var index = 0; index < count; index++) {
	// 			var child = transform.GetChild (index);
	// 			nextChildren.Enqueue (child);
	// 			allChildren.Add (child);
	// 		}
	// 	}
	// 	return allChildren;

	// }



	// [MenuItem ("Assets/BrushTools/TextureShrinkBrush")]
	// static void TextureShrinkBrush () {
	// 	Object[] selection = Selection.GetFiltered (typeof (object), SelectionMode.DeepAssets);
	// 	foreach (Object activeObject in selection) {
	// 		string resPath = AssetDatabase.GetAssetPath (activeObject);
	// 		TextureImporter ti = (TextureImporter) TextureImporter.GetAtPath (resPath);
	// 		ti.isReadable = true;
	// 		AssetDatabase.ImportAsset (resPath);
	// 		Texture2D originTexture = AssetDatabase.LoadAssetAtPath (resPath, typeof (Texture2D)) as Texture2D;
	// 		Sprite sp = AssetDatabase.LoadAssetAtPath (resPath, typeof (Sprite)) as Sprite;
	// 		if (sp != null) {
	// 			//Debug.Log ("originTex2d.width:" + originTexture.width + "	originTex2d.height:" + originTexture.height);		
	// 			int borderLeft = Mathf.RoundToInt (sp.border.x);
	// 			int borderRight = Mathf.RoundToInt (sp.border.z);
	// 			int borderTop = Mathf.RoundToInt (sp.border.y);
	// 			int borderBottom = Mathf.RoundToInt (sp.border.w);
	// 			int originXOffset = originTexture.width - borderRight - borderLeft;
	// 			if (borderRight == 0 && borderLeft == 0) {
	// 				originXOffset = 0;
	// 			}
	// 			int originYOffset = originTexture.height - borderTop - borderBottom;
	// 			if (borderTop == 0 && borderBottom == 0) {
	// 				originYOffset = 0;
	// 			}

	// 			if (originXOffset < 0 || originYOffset < 0) {
	// 				Debug.LogError ("border is invalid:" + resPath);
	// 				continue;
	// 			}
	// 			int newXOffset = Mathf.Min (originXOffset, 2);
	// 			int newYOffset = Mathf.Min (originYOffset, 2);

	// 			int newWidth = originTexture.width - originXOffset + newXOffset;
	// 			int newHeight = originTexture.height - originYOffset + newYOffset;
	// 			Texture2D newTexture = new Texture2D (newWidth, newHeight, TextureFormat.RGBA32, false);
	// 			for (int i = 0; i < newWidth; i++) {
	// 				for (int j = 0; j < newHeight; j++) {
	// 					int pi = i;
	// 					if (i >= borderLeft + newXOffset) {
	// 						pi = i + originXOffset - newXOffset;
	// 					}

	// 					int pj = j;
	// 					if (j >= borderTop + newYOffset) {
	// 						pj = j + originYOffset - newYOffset;
	// 					}
	// 					newTexture.SetPixel (i, j, originTexture.GetPixel (pi, pj));
	// 				}
	// 			}
	// 			byte[] imagebytes = newTexture.EncodeToPNG ();
	// 			WriteTexture (imagebytes, resPath);
	// 			// Debug.Log ("sp:" + sp.name);
	// 			// Debug.Log ("rect:" + sp.rect);
	// 			// Debug.Log ("border:" + sp.border);
	// 			// Debug.Log ("newXOffset:" + newXOffset + "newYOffset:" + newYOffset);
	// 			// Debug.Log ("newWidth:" + newWidth + "newHeight:" + newHeight);
	// 			//EditorUtility.SetDirty (sp);
	// 		}
	// 	}
	// 	AssetDatabase.SaveAssets ();
	// 	AssetDatabase.Refresh ();
	// }

	// [MenuItem ("Assets/BrushTools/TextureSmallerBrush")]
	// static void TextureSmallerBrush () {
	// 	Object[] selection = Selection.GetFiltered (typeof (object), SelectionMode.DeepAssets);
	// 	foreach (Object activeObject in selection) {
	// 		string resPath = AssetDatabase.GetAssetPath (activeObject);
	// 		TextureImporter ti = (TextureImporter) TextureImporter.GetAtPath (resPath);
	// 		ti.isReadable = true;
	// 		AssetDatabase.ImportAsset (resPath);
	// 		Texture2D originTexture = AssetDatabase.LoadAssetAtPath (resPath, typeof (Texture2D)) as Texture2D;
	// 		float scale = 0.7f;
	// 		int newWidth = Mathf.RoundToInt (originTexture.width * scale);
	// 		int newHeight = Mathf.RoundToInt (originTexture.height * scale);
	// 		if (originTexture != null) {
	// 			Texture2D newTexture = new Texture2D (newWidth, newHeight, TextureFormat.RGBA32, false);
	// 			for (int i = 0; i < newWidth; i++) {
	// 				for (int j = 0; j < newHeight; j++) {
	// 					var pixel = originTexture.GetPixelBilinear ((float) i / newWidth, (float) j / newHeight);
	// 					newTexture.SetPixel (i, j, pixel);
	// 				}
	// 			}
	// 			byte[] imagebytes = newTexture.EncodeToPNG ();
	// 			WriteTexture (imagebytes, resPath);
	// 		}
	// 	}
	// 	AssetDatabase.SaveAssets ();
	// 	AssetDatabase.Refresh ();
	// }

	// [MenuItem ("Assets/BrushTools/TextureGreyBrush")]
	// static void TextureGreyBrush () {
	// 	Object[] selection = Selection.GetFiltered (typeof (object), SelectionMode.DeepAssets);
	// 	foreach (Object activeObject in selection) {
	// 		string resPath = AssetDatabase.GetAssetPath (activeObject);
	// 		TextureImporter ti = (TextureImporter) TextureImporter.GetAtPath (resPath);
	// 		ti.isReadable = true;
	// 		AssetDatabase.ImportAsset (resPath);
	// 		Texture2D originTexture = AssetDatabase.LoadAssetAtPath (resPath, typeof (Texture2D)) as Texture2D;
	// 		if (originTexture != null) {
	// 			Texture2D newTexture = new Texture2D (originTexture.width, originTexture.height, TextureFormat.RGBA32, false);
	// 			for (int i = 0; i < originTexture.width; i++) {
	// 				for (int j = 0; j < originTexture.height; j++) {
	// 					var pixel = originTexture.GetPixel (i, j);
	// 					var grey = pixel.r * 0.299f + pixel.g * 0.587f + pixel.b * 0.114f;
	// 					newTexture.SetPixel (i, j, new Color (grey, grey, grey, pixel.a));
	// 				}
	// 			}
	// 			byte[] imagebytes = newTexture.EncodeToPNG ();
	// 			WriteTexture (imagebytes, resPath);
	// 		}
	// 	}
	// 	AssetDatabase.SaveAssets ();
	// 	AssetDatabase.Refresh ();
	// }

	// [MenuItem ("Assets/BrushTools/TextureAlphaBrush")]
	// static void TextureAlphaBrush () {
	// 	Object[] selection = Selection.GetFiltered (typeof (object), SelectionMode.DeepAssets);
	// 	foreach (Object activeObject in selection) {
	// 		string resPath = AssetDatabase.GetAssetPath (activeObject);
	// 		TextureImporter ti = (TextureImporter) TextureImporter.GetAtPath (resPath);
	// 		ti.isReadable = true;
	// 		AssetDatabase.ImportAsset (resPath);
	// 		Texture2D originTexture = AssetDatabase.LoadAssetAtPath (resPath, typeof (Texture2D)) as Texture2D;
	// 		if (originTexture != null) {
	// 			Texture2D newTexture = new Texture2D (originTexture.width, originTexture.height, TextureFormat.RGBA32, false);
	// 			for (int i = 0; i < originTexture.width; i++) {
	// 				for (int j = 0; j < originTexture.height; j++) {
	// 					var pixel = originTexture.GetPixel (i, j);
	// 					if(pixel.a > 0)
	// 					{
	// 						newTexture.SetPixel (i, j, new Color ( pixel.r,  pixel.g,  pixel.b, 1));
	// 					}
	// 				}
	// 			}
	// 			byte[] imagebytes = newTexture.EncodeToPNG ();
	// 			WriteTexture (imagebytes, resPath);
	// 		}
	// 	}
	// 	AssetDatabase.SaveAssets ();
	// 	AssetDatabase.Refresh ();
	// }

	// [MenuItem ("Assets/BrushTools/TextureBlackBrush")]
	// static void TextureBlackBrush () {
	// 	Object[] selection = Selection.GetFiltered (typeof (object), SelectionMode.DeepAssets);
	// 	foreach (Object activeObject in selection) {
	// 		string resPath = AssetDatabase.GetAssetPath (activeObject);
	// 		TextureImporter ti = (TextureImporter) TextureImporter.GetAtPath (resPath);
	// 		ti.isReadable = true;
	// 		AssetDatabase.ImportAsset (resPath);
	// 		Texture2D originTexture = AssetDatabase.LoadAssetAtPath (resPath, typeof (Texture2D)) as Texture2D;
	// 		if (originTexture != null) {
	// 			Texture2D newTexture = new Texture2D (originTexture.width, originTexture.height, TextureFormat.RGBA32, false);
	// 			for (int i = 0; i < originTexture.width; i++) {
	// 				for (int j = 0; j < originTexture.height; j++) {
	// 					var pixel = originTexture.GetPixel (i, j);
	// 					if(pixel.a > 0)
	// 					{
	// 						newTexture.SetPixel (i, j, new Color ( 0, 0, 0, pixel.a));
	// 					}
	// 				}
	// 			}
	// 			byte[] imagebytes = newTexture.EncodeToPNG ();
	// 			WriteTexture (imagebytes, resPath);
	// 		}
	// 	}
	// 	AssetDatabase.SaveAssets ();
	// 	AssetDatabase.Refresh ();
	// }

	// [MenuItem ("Assets/BrushTools/TextureWhiteBrush")]
	// static void TextureWhiteBrush () {
	// 	Object[] selection = Selection.GetFiltered (typeof (object), SelectionMode.DeepAssets);
	// 	foreach (Object activeObject in selection) {
	// 		string resPath = AssetDatabase.GetAssetPath (activeObject);
	// 		TextureImporter ti = (TextureImporter) TextureImporter.GetAtPath (resPath);
	// 		ti.isReadable = true;
	// 		AssetDatabase.ImportAsset (resPath);
	// 		Texture2D originTexture = AssetDatabase.LoadAssetAtPath (resPath, typeof (Texture2D)) as Texture2D;
	// 		if (originTexture != null) {
	// 			Texture2D newTexture = new Texture2D (originTexture.width, originTexture.height, TextureFormat.RGBA32, false);
	// 			for (int i = 0; i < originTexture.width; i++) {
	// 				for (int j = 0; j < originTexture.height; j++) {
	// 					var pixel = originTexture.GetPixel (i, j);
	// 					if(pixel.a > 0)
	// 					{
	// 						newTexture.SetPixel (i, j, new Color (255, 255, 255, pixel.a));
	// 					}
	// 				}
	// 			}
	// 			byte[] imagebytes = newTexture.EncodeToPNG ();
	// 			WriteTexture (imagebytes, resPath);
	// 		}
	// 	}
	// 	AssetDatabase.SaveAssets ();
	// 	AssetDatabase.Refresh ();
	// }
	// private static void WriteTexture (byte[] bytes, string filePath) {
	// 	string fileName = Path.GetFileName (filePath);
	// 	string directoryName = Path.GetDirectoryName (filePath);
	// 	if (!Directory.Exists (directoryName)) {
	// 		Directory.CreateDirectory (directoryName);
	// 	}
	// 	string extension = Path.GetExtension (filePath);
	// 	using (FileStream fs = File.OpenWrite (filePath)) {
	// 		fs.Write (bytes, 0, bytes.Length);
	// 		fs.Close ();
	// 		fs.Dispose ();
	// 	}
	// }
	// /*
	// 	[MenuItem("Assets/ResizeSpriteBrush")]
	//     static void ResizeSpriteBrush()
	//     {
	//         Object[] selection = Selection.GetFiltered(typeof(object), SelectionMode.DeepAssets);
	// 		foreach(Object activeObject in selection)
	// 		{
	// 			string resPath = AssetDatabase.GetAssetPath(activeObject);
	// 			GameObject go = AssetDatabase.LoadAssetAtPath(resPath,typeof(GameObject)) as GameObject;
	// 			if(go!=null)
	// 			{
	// 				//TODO
	// 				if(go!=null)
	// 				{
	// 					EditorUtility.SetDirty(go);
	// 				}
	// 			}
	// 		} 
	// 		AssetDatabase.SaveAssets();
	//     }
	// */

}