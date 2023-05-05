using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Unity.Collections;
using Unity.Jobs;
using System.IO;
using Unity.Collections.LowLevel.Unsafe;
using System.Threading.Tasks;
public class PsdStructureImporter : AssetPostprocessor
{
    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        foreach (string str in importedAssets)
        {
            //OnPostprocessAsset(str);
            //Debug.Log("Reimported Asset: " + str);
        }
        /*
        foreach (string str in deletedAssets)
        {
            Debug.Log("Deleted Asset: " + str);
        }

        for (int i = 0; i < movedAssets.Length; i++)
        {
            Debug.Log("Moved Asset: " + movedAssets[i] + " from: " + movedFromAssetPaths[i]);
        }
        */
    }
    static void OnPostprocessAsset(string assetPath)
    {
        if (assetPath.Contains("UIPSD") && assetPath.EndsWith(".json"))
        {
            UIBrushTool.FixFont(assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Fix font suc:"+ assetPath);
        }

    }


}