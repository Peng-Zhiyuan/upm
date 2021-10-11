using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;

public class AddressableCmds : GameConsoleCmds
{
    
    public static string LoadAsset_Help = "显示所有指令的帮助信息";
    public static List<string> LoadAsset_Alias = new List<string>(){"loadasset"};

    public async static void LoadAsset(string address){
        var a = await AddressableManager.LoadAssetAsync<UnityEngine.Object>(address);
        MainConsoleWind.ShowStringToCmd(a.GetType().ToString(), Color.green);
        // Application.
    }
    
    public static void GetAsset(string address){
        var a = AddressableManager.GetAsset<UnityEngine.Object>(address);
        MainConsoleWind.ShowStringToCmd(a.GetType().ToString(), Color.green);
    }
    

    public static void UnloadAsset(string address){
        // var go = AddressableManager.GetAsset<Object>(address);
        // AddressableManager.ReleaseAsset(go);
        AddressableManager.ReleaseAsset(address);
    }

    
    public static void PreloadAssetsOfLabel(string label){
        AddressableManager.LoadAssetsByLabelAsync(label);
    }
    
    public static void UnloadAssetsOfLabel(string label){
        AddressableManager.UnloadAssetByLabelAsync(label);
    }
    
    public static void IsAssetsLoaded(string address){
        var result = AddressableManager.IsAssetLoaded(address);
        MainConsoleWind.ShowStringToCmd(result.ToString(), Color.green);
    }
    
    public async static void AllAtlas(){
        var sp = await AddressableManager.LoadAssetAsync<Sprite>("Environment1.png[Environment1_0]");
        Debug.Log(sp.name);
        // Environment1.png[Environment1_0]
    }
    
}
