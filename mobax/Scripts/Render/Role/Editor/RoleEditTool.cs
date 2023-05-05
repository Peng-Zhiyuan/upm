using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
public class RoleEditTool
{

    [MenuItem("Tools/GenerateModelList")]
    // Update is called once per frame
    public static void GenerateModelList()
    {

        List<string> roleList = new List<string>();
        string[] models = AssetDatabase.FindAssets("t:prefab", new[] { "Assets/Arts/Models" });
        var count = models.Length;
        for (var i = 0; i < count; ++i)
        {
            string path = AssetDatabase.GUIDToAssetPath(models[i]);
          /*
            string[] pathParts = path.Split('/');
            if (pathParts.Length == 0)
            {
                Debug.LogError("invalid path:" + path);
                continue;
            }
            var modelName = pathParts[pathParts.Length - 1];*/
            //Debug.LogError("modelName:"+ modelName);
            //var roleModel = StaticData.RoleModelTable.GetValueByIndex(i);
            roleList.Add(path);
            var content = string.Join(",", roleList.ToArray());
            if (!Directory.Exists(Application.dataPath + "/Arts/Models/$ModelList"))
            {
                Directory.CreateDirectory(Application.dataPath + "/Arts/Models/$ModelList");
            }
            var path2 = "Assets/Arts/Models/$ModelList/ModelList.txt";

            File.WriteAllText(path2, content);
            AssetDatabase.Refresh();
            //AssetDatabase.CreateAsset(asset, "Assets/Arts/Models/$ModelList/ModelList.txt");
            //heroIdList.Add(id);
            //roleDropDown.options.Add(new UnityEngine.UI.Dropdown.OptionData(modelName));
            //roleDataDic.Add(id, modelName);
        }
    }
}
