using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System;
using System.Threading.Tasks;

public class ClientConfigManager
{
    public static Func<string, TextAsset> OnLoadTableTextAsset;

    public static Func<string, TextAsset> OnEditorModeLoadTableTextAsset;

	public static string LoadTableString(string name)
    {

        TextAsset textAssets;
        if (Application.isPlaying)
        {
            if (OnLoadTableTextAsset == null)
            {
                throw new Exception("[ClientConfig] OnLoadTableTextAsset not set yet!");
            }
            textAssets = OnLoadTableTextAsset.Invoke(name);
        }
        else
        {
            if (OnEditorModeLoadTableTextAsset == null)
            {
                throw new Exception("[ClientConfig] OnEditorModeLoadTableTextAsset not set yet!");
            }
            textAssets = OnEditorModeLoadTableTextAsset.Invoke(name);
        }


        var tableRes = textAssets.text;
        return tableRes;
    }


    //private void CleanAllTableObject()
    //{
    //    var type = typeof(ClientConfig);
    //    var fieldList = type.GetFields();
    //    foreach (var f in fieldList)
    //    {
    //        var name = f.Name;
    //        var isTable = name.EndsWith("Table") && name.StartsWith("_");
    //        if (isTable)
    //        {
    //            f.SetValue(ClientConfig.Stuff, null);
    //        }
    //    }
    //}


}
