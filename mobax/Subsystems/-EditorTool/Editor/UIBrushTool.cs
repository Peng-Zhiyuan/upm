using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using CustomLitJson;
using Sirenix.Utilities;


public class UIBrushTool {

	// [MenuItem ("Assets/BrushTools/ReplaceFontBrush")]
	// static void ReplaceFontBrush () {
	// 	Object[] selection = Selection.GetFiltered (typeof (object), SelectionMode.DeepAssets);
	// 	foreach (Object activeObject in selection) {
	// 		string resPath = AssetDatabase.GetAssetPath (activeObject);
	// 		GameObject go = AssetDatabase.LoadAssetAtPath (resPath, typeof (GameObject)) as GameObject;
	// 		if (go != null) {
	// 			Font font = Resources.GetBuiltinResource<Font> ("Arial.ttf");
	// 			if (go != null) {
	// 				EditorUtility.SetDirty (go);
	// 			}
	// 		}
	// 	}
	// 	AssetDatabase.SaveAssets ();
	// }

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

	public static void JsonLoop(JsonData jsonData)
	{
		if (jsonData.IsObject) {
			foreach (DictionaryEntry entry in ((IDictionary)jsonData))
			{
				var key = (string)entry.Key;
				var val = (JsonData)entry.Value;
				if (key == "textSize" && val.IsDouble)
				{
					var cache_val = val.ToDouble();
					var floatValue = (float)val.ToDouble();
					jsonData[key].SetJsonType(JsonType.Int);
					jsonData[key] = Mathf.RoundToInt(floatValue);
					Debug.Log(cache_val + "=>" + jsonData[key]);
					break;
				}
				else JsonLoop(val);
			}
		}
		else if(jsonData.IsArray)
		{
			for (var index = 0; index < jsonData.Count; index++)
			{
				JsonLoop(jsonData[index]);
			}
		}
	}


	public static void FixFont(string assetPath)
	{
		TextAsset textAsset = AssetDatabase.LoadAssetAtPath(assetPath, typeof(TextAsset)) as TextAsset;
		string text = textAsset.text;
		JsonData data = JsonMapper.Instance.ToObject(text);
		JsonLoop(data);
		var path = GetFullPath(assetPath);
		//Debug.Log(path+"=>"+JsonMapper.Instance.ToJson(data));
		File.WriteAllText(path, JsonMapper.Instance.ToJson(data));
		//AssetDatabase.SaveAssets();
		//AssetDatabase.Refresh();
	}

	/*
	[MenuItem("Assets/BrushTools/FontSizeFitBrush")]
	static void FontSizeFitBrush()
	{
		Object[] selection = Selection.GetFiltered(typeof(object), SelectionMode.DeepAssets);
		foreach (Object activeObject in selection)
		{
			string resPath = AssetDatabase.GetAssetPath(activeObject);
			FixFont(resPath);

			//TextAsset textAsset = AssetDatabase.LoadAssetAtPath(resPath, typeof(TextAsset)) as TextAsset;
			//string text = textAsset.text;
			//JsonData data = JsonMapper.Instance.ToObject(text);
			//JsonLoop(data);
			//var path = GetFullPath(resPath);
			//Debug.Log(path+"=>"+JsonMapper.Instance.ToJson(data));
			//File.WriteAllText(path, JsonMapper.Instance.ToJson(data));
		}
		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();
	}
	*/

	static List<T> GetSelectedAssets<T>() where T : UnityEngine.Object
    {
		Object[] selection = Selection.GetFiltered(typeof(object), SelectionMode.DeepAssets);
		var ret = new List<T>();
		foreach (var activeObject in selection)
		{
			string resPath = AssetDatabase.GetAssetPath(activeObject);
			var asset = AssetDatabase.LoadAssetAtPath<T>(resPath);
			if (asset == null)
			{
				continue;
			}
			ret.Add(asset);
		}
		return ret;

	}


	private const string SPRITE_LIBRARY_PATH = "Assets/UISprites";

	[MenuItem("Assets/BrushTools/UIPrefabBrush")]
	static void UIPrefabBrush()
	{
		UIPrefabPreset(false);
	}


	[MenuItem("Assets/BrushTools/UIPrefabSafeBrush")]
	static void UIPrefabSafeBrush()
	{
		UIPrefabPreset(true);
	}

	[MenuItem("Assets/BrushTools/ImportSprite")]
	static void ImportSprite()
	{
		var selection = Selection.GetFiltered(typeof(object), SelectionMode.DeepAssets);
		foreach (var activeObject in selection)
		{
			var prefab = activeObject as GameObject;
			if (prefab == null)
			{
				continue;
			}

			ProcessSpriteImport(prefab, SpriteOperation.none);
			EditorUtility.SetDirty(prefab);
		}
		
		AssetDatabase.SaveAssets();
	}

	[MenuItem("Assets/BrushTools/ImportSpriteForce")]
	static void ImportSpriteForce()
	{
		var selection = Selection.GetFiltered(typeof(object), SelectionMode.DeepAssets);
		foreach (var activeObject in selection)
		{
			var prefab = activeObject as GameObject;
			if (prefab == null)
			{
				continue;
			}

			ProcessSpriteImport(prefab, SpriteOperation.redirect);
			EditorUtility.SetDirty(prefab);
		}
		AssetDatabase.SaveAssets();
	}


	[MenuItem("Assets/BrushTools/TryGenSpriteMd5")]
	static void TryGenLibrarySpriteMd5()
	{
		var librarySpriteList = AssetNameToLibrarySpriteDic;
		foreach(var kv in librarySpriteList)
        {
			var spriteName = kv.Key;
			var sprite = kv.Value;
			var hasSliced = HasSliceInfo(sprite);
			if(hasSliced)
            {
				continue;
            }
			var path = AssetDatabase.GetAssetPath(sprite);
			var md5 = SecurityUtil.GetFileMd5(path);
			WriteMd5(spriteName, md5);
		}
	}

	static Dictionary<string, Sprite> LoadAllSpriteFromLibrary()
    {
		var dic = new Dictionary<string, Sprite>();
		var spriteList = LoadAllAssetsAtPath<Sprite>(SPRITE_LIBRARY_PATH);	
		foreach(var sprite in spriteList)
        {
			var assetName = sprite.name;
			dic[assetName] = sprite;
        }
		return dic;
	}

	static Dictionary<string, Sprite> _assetNameToLibrarySpriteDic;
	static Dictionary<string, Sprite> AssetNameToLibrarySpriteDic
    {
		get
        {
			if(_assetNameToLibrarySpriteDic == null)
            {
				_assetNameToLibrarySpriteDic = LoadAllSpriteFromLibrary();
			}
			return _assetNameToLibrarySpriteDic;
		}
    }

	static bool IsLibrarySprite(Sprite sprite)
    {
		var path = AssetDatabase.GetAssetPath(sprite);
		if(path.StartsWith(SPRITE_LIBRARY_PATH))
        {
			return true;
        }
		return false;
    }

	static List<Image> GetImageWhichUseNonLibrarySprite(GameObject prefab)
    {
		var ret = new List<Image>();
		var imageList = prefab.GetComponentsInChildren<Image>(true);
		foreach(var image in imageList)
        {
			var sprite = image.sprite;
			if(sprite != null)
            {
				var isLibrarySprite = IsLibrarySprite(sprite);
				if(!isLibrarySprite)
                {
					ret.Add(image);
                }
            }
        }
		return ret;
	}

	static bool IsLibraryHas(string spriteName)
    {
		if(AssetNameToLibrarySpriteDic.ContainsKey(spriteName))
        {
			return true;
        }
		return false;
    }

	static void WriteMd5(string spriteName, string md5)
    {
		var path = $"SpriteLibrary/{spriteName}";
		var dir = Path.GetDirectoryName(path);
		if(!Directory.Exists(dir))
        {
			Directory.CreateDirectory(dir);
        }
		File.WriteAllText(path, md5);
    }

	static string ReadMd5(string spriteName)
    {
		var path = $"SpriteLibrary/{spriteName}";
		var md5 = File.ReadAllText(path);
		return md5;
	}

	static bool IsMd5FileExists(string spriteName)
    {
		var path = $"SpriteLibrary/{spriteName}";
		var b = File.Exists(path);
		return b;
    }

	static void SupeDir(string path)
    {
		var dir = Path.GetDirectoryName(path);
		if(!Directory.Exists(dir))
        {
			Directory.CreateDirectory(dir);
        }
    }

	static Sprite Import(Sprite sprite)
    {
		var path = AssetDatabase.GetAssetPath(sprite);
		var spriteName = sprite.name;
		var validSpriteName = GetValidSpriteName(spriteName);
		var alreadyInLibrary = IsLibraryHas(validSpriteName);
		if(alreadyInLibrary)
        {
			throw new System.Exception(validSpriteName + " already exsits, cannot import");
        }
		var ext = Path.GetExtension(path);
		var to = $"{SPRITE_LIBRARY_PATH}/Temp/{validSpriteName}{ext}";
		SupeDir(to);
		AssetDatabase.CopyAsset(path, to);
		AssetDatabase.ImportAsset(to);
		var md5 = SecurityUtil.GetFileMd5(to);
		WriteMd5(validSpriteName, md5);
		var newImportedSprite = AssetDatabase.LoadAssetAtPath<Sprite>(to);
		AssetNameToLibrarySpriteDic[validSpriteName] = newImportedSprite;
		return newImportedSprite;
	}

	static bool Redirect(Image image, bool forceRedirect = false)
    {
		var sprite = image.sprite;
		var spriteName = sprite.name;
		var validSpriteName = GetValidSpriteName(spriteName);
		var alreadyInLibrary = IsLibraryHas(validSpriteName);
		if(!alreadyInLibrary)
        {
			throw new System.Exception(validSpriteName + " not exsits, cannot redirect to");
		}
		if(!forceRedirect)
        {
			var librarySpriteMd5Exists = IsMd5FileExists(validSpriteName);
			if(librarySpriteMd5Exists)
            {
				var librarySpriteMd5 = ReadMd5(validSpriteName);
				var spritePath = AssetDatabase.GetAssetPath(sprite);
				var md5 = SecurityUtil.GetFileMd5(spritePath);
				if (md5 != librarySpriteMd5)
				{
					Debug.Log($"skip redirect ' {spriteName} '. md5 not same.");
					return false;
				}
			}
		}
		var librarySprite = AssetNameToLibrarySpriteDic[validSpriteName];
		image.sprite = librarySprite;
		var hasSliceInfo = HasSliceInfo(librarySprite);
		
		if(image.type == Image.Type.Simple)
        {
			if (hasSliceInfo)
			{
				image.type = Image.Type.Sliced;
			}
		}

		return true;
	}

	static bool Replace(Image image, bool forceReplace= false)
	{
		var sprite = image.sprite;
		var spriteName = sprite.name;
		var validSpriteName = GetValidSpriteName(spriteName);
		var alreadyInLibrary = IsLibraryHas(validSpriteName);
		if (!alreadyInLibrary)
		{
			throw new System.Exception(validSpriteName + " not exsits, cannot redirect to");
		}
		if (!forceReplace)
		{
			var librarySpriteMd5Exists = IsMd5FileExists(validSpriteName);
			if (librarySpriteMd5Exists)
			{
				var librarySpriteMd5 = ReadMd5(validSpriteName);
				var spritePath = AssetDatabase.GetAssetPath(sprite);
				var md5 = SecurityUtil.GetFileMd5(spritePath);
				if (md5 != librarySpriteMd5)
				{
					Debug.Log($"skip redirect ' {spriteName} '. md5 not same.");
					return false;
				}
			}
		}
		var librarySprite = AssetNameToLibrarySpriteDic[validSpriteName];
		image.sprite = librarySprite;
		var hasSliceInfo = HasSliceInfo(librarySprite);

		if (image.type == Image.Type.Simple)
		{
			if (hasSliceInfo)
			{
				image.type = Image.Type.Sliced;
			}
		}

		return true;
	}

	static bool HasSliceInfo(Sprite sprite)
    {
		if (sprite.border.x + sprite.border.y + sprite.border.z + sprite.border.w > 0)
		{
			return true;
		}
		else
		{
			return false;
		}
	}


	static void ProcessSpriteImport(GameObject prefab, SpriteOperation operation = SpriteOperation.redirect)
    {
		if(prefab == null)
        {
			return;
        }

		var nonLibrarySpriteImageList = GetImageWhichUseNonLibrarySprite(prefab);
		var importedCount = 0;
		var redirectCount = 0;
		var replaceCount = 0;
		var skipedCount = 0;
		foreach (var noneLibraryImage in nonLibrarySpriteImageList)
		{
			var spriteName = GetValidSpriteName(noneLibraryImage.sprite.name);
			var sprite = noneLibraryImage.sprite;
			string spritePath = AssetDatabase.GetAssetPath(sprite);
			if (spritePath.Contains("unity_builtin_extra"))
			{
				continue;
			}
			if (!IsLibraryHas(spriteName))
			{
				var newImportedSprite = Import(sprite);
				noneLibraryImage.sprite = newImportedSprite;
				importedCount++;
			}
			else
            {
	
				if (operation == SpriteOperation.redirect)
				{
					var b = Redirect(noneLibraryImage, true);
					if (b)
					{
						redirectCount++;
					}
					else
					{
						skipedCount++;
					}

				}
				else if (operation == SpriteOperation.replace)
				{
					var b = Replace(noneLibraryImage, true);
					if (b)
					{
						replaceCount++;
					}
					else
					{
						skipedCount++;
					}
				}
			}
		}
		Debug.Log($" imported: {importedCount}, redirect: {redirectCount}, replace: {replaceCount}, skiped: {skipedCount}");
	}
	public enum SpriteOperation
	{
		none,
		redirect,
		replace,
	}
	static void UIPrefabPreset(bool safe = false, bool replaceSprite = false) 
	{
		string fullPath = GetFullPath(SPRITE_LIBRARY_PATH+"/Temp");
		if(!Directory.Exists(fullPath))
		{
			Directory.CreateDirectory(fullPath);
		}
	
		var selection = Selection.GetFiltered (typeof (object), SelectionMode.DeepAssets);
		foreach (var activeObject in selection) 
		{
			var prefab = activeObject as GameObject;
			if (prefab == null)
			{
				continue;
			}
		
			ProcessSpriteImport(prefab, replaceSprite?SpriteOperation.replace:SpriteOperation.redirect);

			// pzy:
			// 问题：通过 Button 来判断是否是按钮并不准确
			// 多数情况下这会造成麻烦

			////绑定UIButton
			var transformList = GetAllChildren (prefab.transform);
			foreach (Transform transform in transformList) 
			{
				var button = transform.gameObject.GetComponent<Button>();
				if (button == null) continue;
			
				if (!transform.gameObject.name.StartsWith ("$")) {
                	transform.gameObject.name = "$" + transform.gameObject.name;
                }

				// 关闭动画
				//if (transform.gameObject.GetComponent<Animator>() == null)
				//{
				//	var animator = transform.gameObject.AddComponent<Animator>();
				//	var animCtrl = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/UIAnim/BtnClick.controller");
				//	animator.runtimeAnimatorController = animCtrl;
				//}
				var buttonSe = transform.gameObject.GetOrAddComponent<ButtonExtra>();

                //// 增加缩放动画
                var buttonAnim = transform.gameObject.GetOrAddComponent<ButtonScaleAnim>();
            }

	

			//绑定OutLine和UILocalize
			var textList = prefab.GetComponentsInChildren<Text> (true);
			foreach (Text text in textList) 
			{
				if (text == null) continue;

				//Add FontLocalizer 
				var fontLocalizer = text.GetComponent<FontLocalizer>();
				if (fontLocalizer == null)
				{
					text.gameObject.AddComponent<FontLocalizer>();
				}

				if (!safe)
				{
					text.alignment = TextAnchor.MiddleCenter;
					text.supportRichText = false;
					// 猜测 psd 里有描边
					if (text.GetComponent<RectTransform>().rect.height > text.fontSize)
					{
						var outLine = text.gameObject.GetOrAddComponent<Outline>();
					}
				}

				

				var lagacy = text.gameObject.GetComponent<UILocalization>();
				if (lagacy != null)
				{
					if (lagacy.KeyString != null)
					{
						var localizer = text.gameObject.GetOrAddComponent<TextLocalizer>();
						localizer.key = lagacy.KeyString;
					}
					GameObject.DestroyImmediate(lagacy, true);
				}
				else
				{
					text.gameObject.GetOrAddComponent<TextLocalizer>();
				}
				text.gameObject.GetOrAddComponent<FontLocalizer>();
			}


			//绑定codelinker系统

			var codeLinkerObject = prefab.GetOrAddComponent<CodeLinkerObject> ();
			EditorUtility.SetDirty (prefab);
			

		}
		AssetDatabase.SaveAssets ();
		AssetDatabase.Refresh ();
	}


	[MenuItem("Assets/BrushTools/ReplaceSprite")]
	static void UIReplaceSprite()
	{
		string fullPath = GetFullPath(SPRITE_LIBRARY_PATH + "/Temp");
		if (!Directory.Exists(fullPath))
		{
			Directory.CreateDirectory(fullPath);
		}
		var selection = Selection.GetFiltered(typeof(object), SelectionMode.DeepAssets);
		foreach (var activeObject in selection)
		{
			var prefab = activeObject as GameObject;
			if (prefab == null)
			{
				continue;
			}

			ProcessSpriteImport(prefab, SpriteOperation.redirect);
			EditorUtility.SetDirty(prefab);
		}
		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();
	}


	private static bool IsButtonImage(MaskableGraphic graphic, int deep = 3)
	{
		if(graphic is Image || graphic is RawImage) 
		{
			var tr = graphic.transform;
			while(tr != null && deep > 0 )
			{
				if(tr.GetComponent<Button>() != null)
				{
					return true;
				}
				tr = tr.parent;
				deep--;
			}
		}
		return false;
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



	[MenuItem ("Assets/BrushTools/TextureShrinkBrush")]
	static void TextureShrinkBrush () {
		Object[] selection = Selection.GetFiltered (typeof (object), SelectionMode.DeepAssets);
		foreach (Object activeObject in selection) {
			string resPath = AssetDatabase.GetAssetPath (activeObject);
			if (!(TextureImporter.GetAtPath(resPath) is TextureImporter)) continue;
			TextureImporter ti = (TextureImporter) TextureImporter.GetAtPath (resPath);
			if (ti == null) continue;
			ti.isReadable = true;
			AssetDatabase.ImportAsset (resPath);
			Texture2D originTexture = AssetDatabase.LoadAssetAtPath (resPath, typeof (Texture2D)) as Texture2D;
	
			Sprite sp = AssetDatabase.LoadAssetAtPath (resPath, typeof (Sprite)) as Sprite;
			if (sp != null) {
				//Debug.Log ("originTex2d.width:" + originTexture.width + "	originTex2d.height:" + originTexture.height);		
				int borderLeft = Mathf.RoundToInt (sp.border.x);
				int borderRight = Mathf.RoundToInt (sp.border.z);
				int borderTop = Mathf.RoundToInt (sp.border.y);
				int borderBottom = Mathf.RoundToInt (sp.border.w);
			
				int originXOffset = originTexture.width - borderRight - borderLeft;
				if (borderRight == 0 && borderLeft == 0) {
					originXOffset = 0;
				}
				int originYOffset = originTexture.height - borderTop - borderBottom;
				if (borderTop == 0 && borderBottom == 0) {
					originYOffset = 0;
				}
				int newXOffset = Mathf.Min(originXOffset, 2);
				int newYOffset = Mathf.Min(originYOffset, 2);

				if (originXOffset < 0 || originYOffset < 0 || originXOffset + originYOffset == 0|| (newXOffset >= originXOffset && newYOffset >= originYOffset)) {
					Debug.LogError ("border is invalid:" + resPath);
					ti.isReadable = false;
					ti.SaveAndReimport();
					continue;
				}
			

				int newWidth = originTexture.width - originXOffset + newXOffset;
				int newHeight = originTexture.height - originYOffset + newYOffset;
				Texture2D newTexture = new Texture2D (newWidth, newHeight, TextureFormat.RGBA32, false);
				for (int i = 0; i < newWidth; i++) {
					for (int j = 0; j < newHeight; j++) {
						int pi = i;
						if (i >= borderLeft + newXOffset) {
							pi = i + originXOffset - newXOffset;
						}

						int pj = j;
						if (j >= borderTop + newYOffset) {
							pj = j + originYOffset - newYOffset;
						}
						newTexture.SetPixel (i, j, originTexture.GetPixel (pi, pj));
					}
				}
				byte[] imagebytes = newTexture.EncodeToPNG ();
				WriteTexture (imagebytes, resPath);
				ti.isReadable = false;
				ti.SaveAndReimport();
				// Debug.Log ("sp:" + sp.name);
				// Debug.Log ("rect:" + sp.rect);
				// Debug.Log ("border:" + sp.border);
				// Debug.Log ("newXOffset:" + newXOffset + "newYOffset:" + newYOffset);
				// Debug.Log ("newWidth:" + newWidth + "newHeight:" + newHeight);
				//EditorUtility.SetDirty (sp);
			}
		}
		//AssetDatabase.SaveAssets ();
		AssetDatabase.Refresh ();
	}




	[MenuItem ("Assets/BrushTools/TextureSmallerBrush")]
	static void TextureSmallerBrush () {
		Object[] selection = Selection.GetFiltered (typeof (object), SelectionMode.DeepAssets);
		foreach (Object activeObject in selection) {
			string resPath = AssetDatabase.GetAssetPath (activeObject);
			TextureImporter ti = (TextureImporter) TextureImporter.GetAtPath (resPath);
			ti.isReadable = true;
			AssetDatabase.ImportAsset (resPath);
			Texture2D originTexture = AssetDatabase.LoadAssetAtPath (resPath, typeof (Texture2D)) as Texture2D;
			float scale = 0.1f;
			int newWidth = Mathf.RoundToInt (originTexture.width * scale);
			int newHeight = Mathf.RoundToInt (originTexture.height * scale);
			if (originTexture != null) {
				Texture2D newTexture = new Texture2D (newWidth, newHeight, TextureFormat.RGBA32, false);
				for (int i = 0; i < newWidth; i++) {
					for (int j = 0; j < newHeight; j++) {
						var pixel = originTexture.GetPixelBilinear ((float) i / newWidth, (float) j / newHeight);
						newTexture.SetPixel (i, j, pixel);
					}
				}
				byte[] imagebytes = newTexture.EncodeToPNG ();
				WriteTexture (imagebytes, resPath);
			}
		}
		AssetDatabase.SaveAssets ();
		AssetDatabase.Refresh ();
	}

	[MenuItem ("Assets/BrushTools/TextureGreyBrush")]
	static void TextureGreyBrush () {
		Object[] selection = Selection.GetFiltered (typeof (object), SelectionMode.DeepAssets);
		foreach (Object activeObject in selection) {
			string resPath = AssetDatabase.GetAssetPath (activeObject);
			TextureImporter ti = (TextureImporter) TextureImporter.GetAtPath (resPath);
			ti.isReadable = true;
			AssetDatabase.ImportAsset (resPath);
			Texture2D originTexture = AssetDatabase.LoadAssetAtPath (resPath, typeof (Texture2D)) as Texture2D;
			if (originTexture != null) {
				Texture2D newTexture = new Texture2D (originTexture.width, originTexture.height, TextureFormat.RGBA32, false);
				for (int i = 0; i < originTexture.width; i++) {
					for (int j = 0; j < originTexture.height; j++) {
						var pixel = originTexture.GetPixel (i, j);
						var grey = pixel.r * 0.299f + pixel.g * 0.587f + pixel.b * 0.114f;
						newTexture.SetPixel (i, j, new Color (grey, grey, grey, pixel.a));
					}
				}
				byte[] imagebytes = newTexture.EncodeToPNG ();
				WriteTexture (imagebytes, resPath);
			}
		}
		AssetDatabase.SaveAssets ();
		AssetDatabase.Refresh ();
	}

	[MenuItem ("Assets/BrushTools/TextureAlphaBrush")]
	static void TextureAlphaBrush () {
		Object[] selection = Selection.GetFiltered (typeof (object), SelectionMode.DeepAssets);
		foreach (Object activeObject in selection) {
			string resPath = AssetDatabase.GetAssetPath (activeObject);
			TextureImporter ti = (TextureImporter) TextureImporter.GetAtPath (resPath);
			ti.isReadable = true;
			AssetDatabase.ImportAsset (resPath);
			Texture2D originTexture = AssetDatabase.LoadAssetAtPath (resPath, typeof (Texture2D)) as Texture2D;
			if (originTexture != null) {
				Texture2D newTexture = new Texture2D (originTexture.width, originTexture.height, TextureFormat.RGBA32, false);
				for (int i = 0; i < originTexture.width; i++) {
					for (int j = 0; j < originTexture.height; j++) {
						var pixel = originTexture.GetPixel (i, j);
						if(pixel.a > 0)
						{
							newTexture.SetPixel (i, j, new Color ( pixel.r,  pixel.g,  pixel.b, 1));
						}
						else
						{
							newTexture.SetPixel (i, j, new Color (255, 255, 255, 0));
						}
					}
				}
				byte[] imagebytes = newTexture.EncodeToPNG ();
				WriteTexture (imagebytes, resPath);
			}
		}
		AssetDatabase.SaveAssets ();
		AssetDatabase.Refresh ();
	}

	[MenuItem ("Assets/BrushTools/TextureBlackBrush")]
	static void TextureBlackBrush () {
		Object[] selection = Selection.GetFiltered (typeof (object), SelectionMode.DeepAssets);
		foreach (Object activeObject in selection) {
			string resPath = AssetDatabase.GetAssetPath (activeObject);
			TextureImporter ti = (TextureImporter) TextureImporter.GetAtPath (resPath);
			ti.isReadable = true;
			AssetDatabase.ImportAsset (resPath);
			Texture2D originTexture = AssetDatabase.LoadAssetAtPath (resPath, typeof (Texture2D)) as Texture2D;
			if (originTexture != null) {
				Texture2D newTexture = new Texture2D (originTexture.width, originTexture.height, TextureFormat.RGBA32, false);
				for (int i = 0; i < originTexture.width; i++) {
					for (int j = 0; j < originTexture.height; j++) {
						var pixel = originTexture.GetPixel (i, j);
						if(pixel.a > 0)
						{
							newTexture.SetPixel (i, j, new Color ( 0, 0, 0, pixel.a));
						}
						else
						{
							newTexture.SetPixel (i, j, new Color (255, 255, 255, 0));
						}
					}
				}
				byte[] imagebytes = newTexture.EncodeToPNG ();
				WriteTexture (imagebytes, resPath);
			}
		}
		AssetDatabase.SaveAssets ();
		AssetDatabase.Refresh ();
	}

	[MenuItem ("Assets/BrushTools/TextureWhiteBrush")]
	static void TextureWhiteBrush () {
		Object[] selection = Selection.GetFiltered (typeof (object), SelectionMode.DeepAssets);
		foreach (Object activeObject in selection) {
			string resPath = AssetDatabase.GetAssetPath (activeObject);
			TextureImporter ti = (TextureImporter) TextureImporter.GetAtPath (resPath);
			ti.isReadable = true;
			AssetDatabase.ImportAsset (resPath);
			Texture2D originTexture = AssetDatabase.LoadAssetAtPath (resPath, typeof (Texture2D)) as Texture2D;
			if (originTexture != null) {
				Texture2D newTexture = new Texture2D (originTexture.width, originTexture.height, TextureFormat.RGBA32, false);
				for (int i = 0; i < originTexture.width; i++) {
					for (int j = 0; j < originTexture.height; j++) {
						var pixel = originTexture.GetPixel (i, j);
						if(pixel.a > 0)
						{
							newTexture.SetPixel (i, j, new Color (255, 255, 255, pixel.a));
						}
						else
						{
							newTexture.SetPixel (i, j, new Color (255, 255, 255, 0));
						}

					}
				}
				byte[] imagebytes = newTexture.EncodeToPNG ();
				WriteTexture (imagebytes, resPath);
			}
		}
		AssetDatabase.SaveAssets ();
		AssetDatabase.Refresh ();
	}
	private static void WriteTexture (byte[] bytes, string filePath) {
		string fileName = Path.GetFileName (filePath);
		string directoryName = Path.GetDirectoryName (filePath);
		if (!Directory.Exists (directoryName)) {
			Directory.CreateDirectory (directoryName);
		}
		string extension = Path.GetExtension (filePath);
		using (FileStream fs = File.OpenWrite (filePath)) {
			fs.Write (bytes, 0, bytes.Length);
			fs.Close ();
			fs.Dispose ();
		}
	}


	[MenuItem("Assets/BrushTools/TextureCutBrush")]
	static void TextureCutBrush()
	{
		Object[] selection = Selection.GetFiltered(typeof(object), SelectionMode.DeepAssets);
		foreach (Object activeObject in selection)
		{
			string resPath = AssetDatabase.GetAssetPath(activeObject);
			TextureImporter ti = (TextureImporter)TextureImporter.GetAtPath(resPath);
			ti.isReadable = true;
			AssetDatabase.ImportAsset(resPath);
			Texture2D originTexture = AssetDatabase.LoadAssetAtPath(resPath, typeof(Texture2D)) as Texture2D;
			Sprite sp = AssetDatabase.LoadAssetAtPath(resPath, typeof(Sprite)) as Sprite;
			if (sp != null)
			{
				//Debug.Log ("originTex2d.width:" + originTexture.width + "	originTex2d.height:" + originTexture.height);		
				int borderLeft = Mathf.RoundToInt(sp.border.x);
				int borderRight = Mathf.RoundToInt(sp.border.z);
				int borderTop = Mathf.RoundToInt(sp.border.y);
				int borderBottom = Mathf.RoundToInt(sp.border.w);
				int newWidth = originTexture.width - borderLeft - borderRight;
				int newHeight = originTexture.height - borderTop - borderBottom;
				Texture2D newTexture = new Texture2D(newWidth, newHeight, TextureFormat.RGBA32, false);
				for (int i = 0; i < newWidth; i++)
				{
					for (int j = 0; j < newHeight; j++)
					{
						int pi = borderLeft + i;
						int pj = borderTop + j;
						newTexture.SetPixel(i, j, originTexture.GetPixel(pi, pj));
					}
				}
				sp.border.Set(0, 0, 0, 0);
				EditorUtility.SetDirty(sp);
				byte[] imagebytes = newTexture.EncodeToPNG();
				WriteTexture(imagebytes, resPath);
			}
		}
		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();
	}

	[MenuItem ("Assets/BrushTools/TextureAutoCutBrush")]
	static void TextureAutoCutBrush () {
		Object[] selection = Selection.GetFiltered (typeof (object), SelectionMode.DeepAssets);
		foreach (Object activeObject in selection) {
			string resPath = AssetDatabase.GetAssetPath (activeObject);
			TextureImporter ti = (TextureImporter) TextureImporter.GetAtPath (resPath);
			ti.isReadable = true;
			AssetDatabase.ImportAsset (resPath);
			Texture2D originTexture = AssetDatabase.LoadAssetAtPath (resPath, typeof (Texture2D)) as Texture2D;
			int borderLeft = 0;
			int borderRight = 0;
			int borderTop = 0;
			int borderBottom = 0;
			for (int i = 0; i < originTexture.width; i++)
			{
				borderLeft = i;
				bool finished = false; 
				for (int j = 0; j < originTexture.height; j++)
				{
					var p = originTexture.GetPixel(i, j);
					if (p.a > 0.1)
					{
						finished = true;
						break;
					}
				}

				if (finished)
				{
					break;
				}
			}

			for (int i = originTexture.width - 1; i >= 0; i--)
			{
				
				borderRight = originTexture.width - 1 - i;
				bool finished = false;
				for (int j = 0; j < originTexture.height; j++)
				{
					var p = originTexture.GetPixel(i, j);
					if (p.a > 0.1)
					{
						finished = true;
						break;
					}
				}
				if (finished)
				{
					break;
				}
			}

			for (int i = originTexture.height - 1; i >= 0; i--)
			{
				borderBottom = originTexture.height - 1 - i;
				bool finished = false;
				for (int j = 0; j < originTexture.width; j++)
				{
					var p = originTexture.GetPixel(j, i);
					if (p.a > 0.1)
					{
						finished = true;
						break;
					}
				}
				if (finished)
				{
					break;
				}
			}

			for (int i = 0; i < originTexture.height; i++)
			{
				borderTop = i;
				bool finished = false;
				for (int j = 0; j < originTexture.width; j++)
				{
					var p = originTexture.GetPixel(j, i);
					if (p.a > 0.1)
					{
						finished = true;
						break;
					}
				}
				if (finished)
				{
					break;
				}
			}
			if (borderTop + borderBottom + borderLeft + borderRight == 0)
			{
				Debug.LogError("无需裁剪:"+ resPath);
				continue;
			}
			//Sprite sp = AssetDatabase.LoadAssetAtPath (resPath, typeof (Sprite)) as Sprite;
			//if (sp != null) {
			//Debug.Log ("originTex2d.width:" + originTexture.width + "	originTex2d.height:" + originTexture.height);		
			    Debug.LogError("borderLeft:"+ borderLeft+ "  borderRight:"+ borderRight+ "  borderTop:"+ borderTop+ "   borderBottom:"+ borderBottom);
			    int newWidth = originTexture.width - borderLeft - borderRight;
				int newHeight = originTexture.height - borderTop - borderBottom;
				Texture2D newTexture = new Texture2D (newWidth, newHeight, TextureFormat.RGBA32, false);
				for (int i = 0; i < newWidth; i++) {
					for (int j = 0; j < newHeight; j++) {
						int pi = borderLeft + i;
						int pj = borderTop + j;
						newTexture.SetPixel (i, j, originTexture.GetPixel (pi, pj));
					}
				}
				//sp.border.Set(0,0,0,0);
			    //EditorUtility.SetDirty(sp);
				byte[] imagebytes = newTexture.EncodeToPNG ();
				WriteTexture (imagebytes, resPath);
			//}
		}
		//AssetDatabase.SaveAssets ();
		AssetDatabase.Refresh ();
	}


	[MenuItem ("Assets/BrushTools/PrefabRaycastBrush")]
	static void PrefabRaycastBrush () {
	
		Object[] selection = Selection.GetFiltered (typeof (object), SelectionMode.DeepAssets);
		foreach (Object activeObject in selection) {
			string resPath = AssetDatabase.GetAssetPath (activeObject);
			GameObject go = AssetDatabase.LoadAssetAtPath (resPath, typeof (GameObject)) as GameObject;
			if(go == null) continue;
			var graphicList = go.GetComponentsInChildren<MaskableGraphic> (true);
			foreach (MaskableGraphic graphic in graphicList) {

				if (graphic == null) continue;
				if (IsButtonImage(graphic))
				{
					if (!graphic.raycastTarget) graphic.raycastTarget = true;
				}
				else
				{
					if (graphic.raycastTarget)  graphic.raycastTarget = false;
				}
				
			}
			EditorUtility.SetDirty (go);
		}
		AssetDatabase.SaveAssets ();
		AssetDatabase.Refresh ();
	}

	[MenuItem("Assets/BrushTools/PrefabButtonBrush")]
	static void PrefabButtonBrush()
	{

		Object[] selection = Selection.GetFiltered(typeof(object), SelectionMode.DeepAssets);
		foreach (Object activeObject in selection)
		{
			string resPath = AssetDatabase.GetAssetPath(activeObject);
			GameObject go = AssetDatabase.LoadAssetAtPath(resPath, typeof(GameObject)) as GameObject;
			if (go == null) continue;
			//绑定UIButton
			var transformList = GetAllChildren(go.transform);
			foreach (Transform transform in transformList)
			{
				var button = transform.gameObject.GetComponent<Button>();
				if (button == null) continue;

				// pzy:
				// 不需要为按钮生成代码绑定
				//if (!transform.gameObject.name.StartsWith("$"))
				//{
				//	transform.gameObject.name = "$" + transform.gameObject.name;
				//}
				var anim = transform.gameObject.GetComponent<Animator>();
				if (anim != null) MonoBehaviour.DestroyImmediate(anim, true);
				var buttonSe = transform.gameObject.GetOrAddComponent<ButtonExtra>();
				var buttonAnim = transform.gameObject.GetOrAddComponent<ButtonScaleAnim>();
			}
			EditorUtility.SetDirty(go);
		}
		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();
	}

	[MenuItem("Assets/BrushTools/FixUIPrefab")]
	static void FixUIPrefab()
	{

		Object[] selection = Selection.GetFiltered(typeof(object), SelectionMode.DeepAssets);
		foreach (Object activeObject in selection)
		{
			string resPath = AssetDatabase.GetAssetPath(activeObject);
			GameObject prefab = AssetDatabase.LoadAssetAtPath(resPath, typeof(GameObject)) as GameObject;
			if (prefab == null)
			{
				continue;
			}
			FixUIPrefab(prefab);
		}
		AssetDatabase.SaveAssets();
	}

	static void FixUIPrefab(GameObject prefab)
	{
		var isChnaged = false;
		var buttonList = prefab.GetComponentsInChildren<Button>();
		foreach (var button in buttonList)
		{
			var comp = button.GetComponent<ButtonExtra>();
			if (comp == null)
			{
				button.gameObject.AddComponent<ButtonExtra>();
				EditorUtility.SetDirty(prefab);
				isChnaged = true;
			}
		}
		if(isChnaged)
        {
			Debug.Log("add ButtonSe to prefab: " + prefab);
		}
	}

	[MenuItem("Assets/BrushTools/PrefabFontBrush")]
	static void PrefabFontBrush()
	{

		Object[] selection = Selection.GetFiltered(typeof(object), SelectionMode.DeepAssets);
		foreach (Object activeObject in selection)
		{
			string resPath = AssetDatabase.GetAssetPath(activeObject);
			if (resPath.Contains("/Plot/") && !resPath.Contains("/PlotPage"))
			{
				Debug.LogError("skip:"+ resPath);
				continue;
			}
			GameObject go = AssetDatabase.LoadAssetAtPath(resPath, typeof(GameObject)) as GameObject;
			if (go == null) continue;
			var textList = go.GetComponentsInChildren<Text>(true);

			//替换sprite引用，并修正填充方式
			foreach (Text text in textList)
			{
				var localization = text.gameObject.GetComponent<FontLocalizer>();
				if(localization == null) text.gameObject.AddComponent<FontLocalizer>();
				EditorUtility.SetDirty(go);
			}
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
		}
	}


	[MenuItem ("Assets/BrushTools/PrefabSliceBrush")]
	static void PrefabSliceBrush () {
	
		Object[] selection = Selection.GetFiltered (typeof (object), SelectionMode.DeepAssets);
		foreach (Object activeObject in selection) {
			string resPath = AssetDatabase.GetAssetPath (activeObject);
			GameObject go = AssetDatabase.LoadAssetAtPath (resPath, typeof (GameObject)) as GameObject;
			if(go == null) continue;
			var imageList = go.GetComponentsInChildren<Image> (true);
            
			//替换sprite引用，并修正填充方式
			foreach (Image image in imageList) {
				if (image == null || image.sprite == null) continue;
				var targetSprite = image.sprite;
				if (targetSprite.border.x + targetSprite.border.y + targetSprite.border.z + targetSprite.border.w > 0)
				{
					if (image.type == Image.Type.Simple)
					{
						image.type = Image.Type.Sliced;
					}
				} 
				else 
				{
					if (image.type == Image.Type.Sliced)
					{
						image.type = Image.Type.Simple;
					}
				}
				EditorUtility.SetDirty(go);
			}
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
		}
	}
	/*
		[MenuItem("Assets/ResizeSpriteBrush")]
	    static void ResizeSpriteBrush()
	    {
	        Object[] selection = Selection.GetFiltered(typeof(object), SelectionMode.DeepAssets);
			foreach(Object activeObject in selection)
			{
				string resPath = AssetDatabase.GetAssetPath(activeObject);
				GameObject go = AssetDatabase.LoadAssetAtPath(resPath,typeof(GameObject)) as GameObject;
				if(go!=null)
				{
					//TODO
					if(go!=null)
					{
						EditorUtility.SetDirty(go);
					}
				}
			To
			AssetDatabase.SaveAssets()d
	    }
	*/
	[MenuItem ("Assets/BrushTools/TexturePreMultipliedAlphaFix")]
	static void TexturePreMultipliedAlphaFix() {
		Object[] selection = Selection.GetFiltered (typeof (object), SelectionMode.DeepAssets);
		foreach (Object activeObject in selection) {
			string resPath = AssetDatabase.GetAssetPath (activeObject);
            TextureImporter ti = (TextureImporter)TextureImporter.GetAtPath(resPath);
			//TextureImporterSettings setting = new TextureImporterSettings();
			ti.isReadable = true;
			//var cacheFormat = setting.textureFormat;
			//var cacheAlpha = setting.alphaIsTransparency;
			//setting.textureFormat = TextureImporterFormat.RGBA32;
			//setting.alphaIsTransparency = true;
			//ti.SetTextureSettings(setting);
			//AssetDatabase.SaveAssets();
			//AssetDatabase.Refresh();
			AssetDatabase.ImportAsset (resPath);
			Texture2D originTexture = AssetDatabase.LoadAssetAtPath (resPath, typeof (Texture2D)) as Texture2D;
			if (originTexture != null) {
				
				int count1 = 0;
				int count2 = 0;
				int count3 = 0;
				//originTexture = new Texture2D(originTexture.width, originTexture.height, TextureFormat.RGBA32, false);
				Texture2D newTexture = new Texture2D (originTexture.width, originTexture.height, TextureFormat.RGBA32, false);
				for (int i = 0; i < originTexture.width; i++) {
					for (int j = 0; j < originTexture.height; j++) {
						var pixel = originTexture.GetPixel(i, j);
						if (pixel.a > 0 && pixel.a <= 1)
						{
							count1++;
							newTexture.SetPixel(i, j, new Color(pixel.r, pixel.g, pixel.b, pixel.a));
						}
						else if (pixel.a > 1)
						{
							count2++;
						}
						else
						{
							count3++;
							newTexture.SetPixel(i, j, new Color(0, 0, 0, 0));
						}

					}
				}
				Debug.LogError("count1:" + count1);
				Debug.LogError("count2:" + count2);
				Debug.LogError("count3:" + count3);
				byte[] imagebytes = newTexture.EncodeToPNG();
                WriteTexture(imagebytes, resPath);
			}
		}
	
		AssetDatabase.SaveAssets ();
		AssetDatabase.Refresh ();
	}
	[MenuItem("Assets/BrushTools/TextureAlphaFix")]
	static void TextureAlphaFix()
	{
		Object[] selection = Selection.GetFiltered(typeof(object), SelectionMode.DeepAssets);
		foreach (Object activeObject in selection)
		{
			string resPath = AssetDatabase.GetAssetPath(activeObject);
			TextureImporter ti = (TextureImporter)TextureImporter.GetAtPath(resPath);
			ti.isReadable = true;
	
			AssetDatabase.ImportAsset(resPath);
			Texture2D originTexture = AssetDatabase.LoadAssetAtPath(resPath, typeof(Texture2D)) as Texture2D;
			if (originTexture != null)
			{
				Texture2D newTexture = new Texture2D(originTexture.width, originTexture.height, TextureFormat.RGBA32, false);
				for (int i = 0; i < originTexture.width; i++)
				{
					for (int j = 0; j < originTexture.height; j++)
					{
						var pixel = originTexture.GetPixel(i, j);
						if (pixel.a > 0 && pixel.a < 1 )
						{
							if (pixel.r + pixel.g + pixel.b == 0)
							{
								newTexture.SetPixel(i, j, new Color(pixel.r, pixel.g, pixel.b, pixel.a * 1.2f));
							}
							else if (pixel.r + pixel.g + pixel.b == 3)
							{
								newTexture.SetPixel(i, j, new Color(pixel.r, pixel.g, pixel.b, pixel.a * 0.3f));
							}
						}
						else if (pixel.a > 1)
						{
	
						}
						else
						{
							newTexture.SetPixel(i, j, new Color(0, 0, 0, 0));
						}
					}
				}
				byte[] imagebytes = newTexture.EncodeToPNG();
				WriteTexture(imagebytes, resPath);
			}
		}

		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();
	}

	[MenuItem ("Assets/BrushTools/TextureAndroidIconBrush")]
	static void TextureAndroidIconBrush () {
		Object[] selection = Selection.GetFiltered (typeof (object), SelectionMode.DeepAssets);
		List<int> sizeList = new List<int>();
		sizeList.Add(192);
		sizeList.Add(144);
		sizeList.Add(96);
		sizeList.Add(72);
		sizeList.Add(48);
		sizeList.Add(36);
		foreach (Object activeObject in selection) {
			string resPath = AssetDatabase.GetAssetPath (activeObject);
			TextureImporter ti = (TextureImporter) TextureImporter.GetAtPath (resPath);
			ti.isReadable = true;
			AssetDatabase.ImportAsset (resPath);
			Texture2D originTexture = AssetDatabase.LoadAssetAtPath (resPath, typeof (Texture2D)) as Texture2D;
			foreach(int size in sizeList)
			{
				int newWidth = size;
				int newHeight = size;
				if (originTexture != null) {
					Texture2D newTexture = new Texture2D (newWidth, newHeight, TextureFormat.RGBA32, false);
					for (int i = 0; i < newWidth; i++) {
						for (int j = 0; j < newHeight; j++) {
							var pixel = originTexture.GetPixelBilinear ((float) i / newWidth, (float) j / newHeight);
							newTexture.SetPixel (i, j, pixel);
						}
					}
					string targetPath = Path.GetDirectoryName(resPath)+"/icon"+size+"x"+size+".png";
					byte[] imagebytes = newTexture.EncodeToPNG ();
					Debug.Log("targetPath:"+targetPath);
					WriteTexture (imagebytes, targetPath);
				}
			}
			
		}
		AssetDatabase.SaveAssets ();
		AssetDatabase.Refresh ();
	}


	[MenuItem("Assets/BrushTools/ImageReplaceBrush", true)]
	static bool ReplaceImagesValidation()
	{
		foreach (var obj in Selection.objects)
		{
			if (!(obj is DefaultAsset) && !(obj is Texture2D))
				return false;
		}

		return true;
	}

	[MenuItem("Assets/BrushTools/ImageReplaceBrush", false, 2001)]
	static void ReplaceImagesHandler()
	{
		Dictionary<string, Sprite> spriteDic = new Dictionary<string, Sprite> ();
		List<Sprite> spriteList = LoadAllAssetsAtPath<Sprite> (SPRITE_LIBRARY_PATH);
		Dictionary<string, List<string>> duplicatedMap = new Dictionary<string, List<string>>();
		foreach (Sprite sp in spriteList) {
			if (spriteDic.TryGetValue(sp.name, out var prevSp)) {
				if (!duplicatedMap.TryGetValue(sp.name, out var list))
				{
					list = duplicatedMap[sp.name] = new List<string>();
					list.Add(AssetDatabase.GetAssetPath(prevSp));
				}

				list.Add(AssetDatabase.GetAssetPath(sp));
				continue;
			}
			spriteDic.Add (sp.name, sp);
		}
		string tempFolder = GetFullPath(SPRITE_LIBRARY_PATH + "/Temp");
		if(!Directory.Exists(tempFolder))
		{
			Directory.CreateDirectory(tempFolder);
		}

		Selection.objects.ForEach(obj =>
		{
			if (obj is DefaultAsset assetFolder)
			{
				var assetPath = AssetDatabase.GetAssetPath(assetFolder);
				List<Texture2D> textureList = LoadAllAssetsAtPath<Texture2D> (assetPath);
				Debug.Log(textureList);
				
				foreach (var texture2D in textureList)
				{
					HandleTexture(texture2D);
				}
			}
			else if (obj is Texture2D texture2D)
			{
				HandleTexture(texture2D);
			}
		});
		Debug.Log("【图片替换】完成！");

		void HandleTexture(Texture2D texture2D)
		{
			var fromPath = AssetDatabase.GetAssetPath(texture2D);
			if (spriteDic.TryGetValue(texture2D.name, out var sp))
			{
				// 如果是重复的，就不替换， 先处理掉再替换吧
				if (duplicatedMap.TryGetValue(texture2D.name, out var list))
				{
					Debug.LogError($"{texture2D.name}取消替换，因为有重复：{string.Join(",", list)}");
				}
				else
				{
					// 找到的图片直接替换
					var destPath = AssetDatabase.GetAssetPath(sp);
					File.Copy(fromPath, destPath, true);
					AssetDatabase.ImportAsset(destPath);
				}
			}
			else
			{
				// 没有找到的统一扔到Temp目录下
				var fileName = Path.GetFileName(fromPath);
				var destPath = $"{tempFolder}/{fileName}";
				AssetDatabase.CopyAsset(fromPath, destPath);
				// AssetDatabase.ImportAsset(destPath);
				// var asset = AssetDatabase.LoadAssetAtPath<Object>(destPath);
				// Debug.Log(asset);
			}
		}
	}

	[MenuItem("Assets/BrushTools/PageOptimizationBrush")]
	static void UIPageOptimizationBrush()
	{
		Object[] selection = Selection.GetFiltered(typeof(object), SelectionMode.DeepAssets);
		foreach (Object activeObject in selection)
		{
			string resPath = AssetDatabase.GetAssetPath(activeObject);
			GameObject go = AssetDatabase.LoadAssetAtPath(resPath, typeof(GameObject)) as GameObject;
			if (go == null) continue;
			var page = go.GetComponent<Page>();
			if (page == null)
			{
				page = go.GetComponentInChildren<Page>(true);
			}
			if (page != null)
			{
				if (page.optimization != PageOptimization.DestroyOnDisabled)
				{
					page.optimization = PageOptimization.OneTime;
					Debug.LogError("Set Page OneTime Suc:" + page.name);
				}
			}
			EditorUtility.SetDirty(go);

		}
		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();
	}

	[MenuItem("Assets/BrushTools/AddFontLocalizerIfNeed")]
	static void AddFontLocalizerIfNeed()
	{
		var assetList = GetSelectedAssets<GameObject>();
		foreach (var asset in assetList)
		{
			var textList = asset.GetComponentsInChildren<Text>(true);
			var isChanged = false;
			foreach (var text in textList)
			{
				var fontLocalizer = text.GetComponent<FontLocalizer>();
				if (fontLocalizer == null)
				{
					text.gameObject.AddComponent<FontLocalizer>();
					isChanged = true;

				}
			}
			if (isChanged)
			{
				EditorUtility.SetDirty(asset.gameObject);
			}
		}
		AssetDatabase.SaveAssets();
		Debug.Log("compelte");
	}


}