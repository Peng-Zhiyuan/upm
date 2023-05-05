using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using System.Text.RegularExpressions;
using System.IO;
using System.Text;

public class ReimportAssemblyDll
{
    //[MenuItem("Assets/ReimportAssembly", false, 100)]
    //public static void ReimportAssembly()
    //{
    //    var path = Application.dataPath.Replace("/Assets", "") + "/Library/PlayerScriptAssemblies/{1}";
    //    var version = string.Empty;
    //    ReimportDll(string.Format(path, version, "Assembly-CSharp.dll"));
    //    var path2 = Application.dataPath.Replace("/Assets", "") + "/Library/ScriptAssemblies/{1}";
    //    ReimportDll(string.Format(path2, version, "Assembly-CSharp.dll"));
    //    //var path = EditorApplication.applicationContentsPath + "/UnityExtensions/Unity/GUISystem/{1}";
    //    //var version = string.Empty;
    //    //string engineDll = string.Format(path, version, "UnityEngine.UI.dll");
    //    //string editorDll = string.Format(path, version, "Editor/UnityEditor.UI.dll");
    //    //ReimportDll(engineDll);
    //    //ReimportDll(editorDll);

    //}
    //static void ReimportDll(string path)
    //{
    //    if (File.Exists(path))
    //        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate | ImportAssetOptions.DontDownloadFromCacheServer);
    //    else
    //        Debug.LogError(string.Format("DLL not found {0}", path));
    //}
}
