
using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine.UI;
using System.IO;
public class GZipTool {
	[MenuItem ("Assets/gzip/compress")] 
	static void Compress ()  
	{         
		Object[] selection = Selection.GetFiltered(typeof(object), SelectionMode.DeepAssets);
		foreach(Object activeObject in selection)
		{
			string resPath = AssetDatabase.GetAssetPath(activeObject);
			TextAsset text= AssetDatabase.LoadAssetAtPath(resPath,typeof(TextAsset)) as TextAsset;
			if(text!=null)
			{
            
			   var bytes = Ionic.Zlib.GZipStream.CompressString (text.text);
               Debug.Log("resPath:"+resPath);
              var filePath = $"{FilePath}/{resPath}".Replace(".json",".byte").Replace(".txt",".byte");
              string path = WriteBytes(filePath,bytes);
              Debug.Log("compress to:"+path);
				
			}
            break;
		} 
	}

   

    public static string FilePath
    {
        get
        {
            return Application.dataPath.Replace("/Assets","");
        }
    }
    private static string Read(string filePath)
    {
        StreamReader _sw = new StreamReader (filePath);
        string text = _sw.ReadToEnd ();
        _sw.Close ();
        return text;
    }
    private static string Write(string filePath, string data)
    {
        StreamWriter _sw = new StreamWriter (filePath);
        _sw.Write (data);
        _sw.Close ();
        return filePath;
    }
    public static string WriteBytes(string filePath, byte[] bytes)
    {
        File.WriteAllBytes(filePath, bytes);
        return filePath;
    }
     private static byte[] ReadBytes(string filePath)
    {
        var bytes = File.ReadAllBytes(filePath);
        return bytes;
    }
    [MenuItem ("Assets/gzip/uncompress")] 
	static void UnCompress ()  
	{         
		Object[] selection = Selection.GetFiltered(typeof(object), SelectionMode.DeepAssets);
		foreach(Object activeObject in selection)
		{
			string resPath = AssetDatabase.GetAssetPath(activeObject);
            var filePath = $"{FilePath}/{resPath}";
		    var bytes = ReadBytes(filePath);
			if(bytes!=null)
			{
			   var data = Ionic.Zlib.GZipStream.UncompressString (bytes);
               string path = Write(resPath.Replace(".byte",".json"),data);
			}
            break;
		} 
	}
}