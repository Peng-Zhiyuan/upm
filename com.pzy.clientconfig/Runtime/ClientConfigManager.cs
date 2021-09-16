using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System;
using System.Threading.Tasks;

public class ClientConfigManager : StuffObject<ClientConfigManager>
{
    public Func<string, TextAsset> OnLoadTableTextAsset;

	public string LoadTableString(string name)
	{
        if(OnLoadTableTextAsset == null)
        { 
            throw new Exception("[ClientConfig] OnLoadTableTextAsset not set yet!");
        }


        var textAssets = OnLoadTableTextAsset.Invoke(name);
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
