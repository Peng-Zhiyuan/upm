using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
public class GameRoot : MonoBehaviour
{
   
    async void Start()
    {
        GameObject.DontDestroyOnLoad(GameObject.Find("EventSystem"));

        // 初始化服务系统
        // 所有服务对象会在这里创建
        ServiceSystem.Stuff.Create(null, "Assembly-CSharp");

        Debug.Log("[GameRoot] Post ServiceSystem Create");

        var isDebug = DeveloperLocalSettings.IsDevelopmentMode;
        if (isDebug)
        {
            await ShowPresetAndWaiteCloseAsync();
        }

        LauncherUiManager.Stuff.Show<LauncherLoadingPage>();
      
    }


    Task ShowPresetAndWaiteCloseAsync()
    {
        var tcs = new TaskCompletionSource<bool>();
        LauncherUiManager.Stuff.Show<PresetPage>();
        PresetPage.okHandler = () =>
        {
            LauncherUiManager.Stuff.Remove<PresetPage>();
            tcs.SetResult(true);
        };

        return tcs.Task;
    }
#if UNITY_EDITOR
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            Debug.LogError("UnloadUnusedAssets");
            Resources.UnloadUnusedAssets();
        }
    }
#endif

}
