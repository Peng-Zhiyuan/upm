using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading.Tasks;
using AK.Wwise.Unity.WwiseAddressables;

public static class WwiseManager 
{

    public static GameObject wwiseGlobal;
    
    public static async Task CreateWwiseGlobal()
    {
        return;
        if (Application.isPlaying)
        {
            var useWwise = DeveloperLocalSettings.IsUseWwise;
            if (!useWwise)
            {
                return;
            }
        }

        if (wwiseGlobal != null)
        {
            throw new Exception("[WwiseManager] WwiseGlobal already exists");
        }

        wwiseGlobal = new GameObject("WwiseGlobal");
        wwiseGlobal.SetActive(false);
        GameObject.DontDestroyOnLoad(wwiseGlobal);
        
        var settings = await BucketManager.Stuff.Main.GetOrAquireAsync<AkWwiseAddressablesInitializationSettings>("AkWwiseInitializationSettings.asset", true);
        if(settings == null)
        {
            throw new Exception("[WwiseManager] Could not find AkWwiseInitializationSettings.asset");
        }

        var initalizer = wwiseGlobal.AddComponent<AkInitializer>();
        initalizer.InitializationSettings = settings;

        var initAddressableBank = await BucketManager.Stuff.Main.GetOrAquireAsync<WwiseAddressableSoundBank>("Init.asset", true);
        if(initAddressableBank == null)
        {
            throw new Exception("[WwiseManager] Could not find addressable bank Init.asset");
        }

        var initBankHolder = wwiseGlobal.AddComponent<InitBankHolder>();
        initBankHolder.InitBank = initAddressableBank;

        wwiseGlobal.AddComponent<AkAudioListener>();

        wwiseGlobal.SetActive(true);

    }

    static void TryInitalizeInEditMode()
    {
#if UNITY_EDITOR
        if(!Application.isPlaying)
        {
            if(WwiseManager.wwiseGlobal == null)
            {
                InitalizeInEditMode();
            }
        }
#endif
    }

#if UNITY_EDITOR
    public static void InitalizeInEditMode()
    {
        if (Application.isPlaying)
        {
            throw new Exception("[WwiseManager] InitalizeInEditMode not acccessable in play mode");
        }

        if (WwiseManager.wwiseGlobal != null)
        {
            throw new Exception("[WwiseManager] WwiseGlobal already exists");
        }

        Debug.Log("[WwiseManager] Initalize In EditMode");

        WwiseManager.wwiseGlobal = new GameObject("WwiseGlobal");
        WwiseManager.wwiseGlobal.SetActive(false);
        GameObject.DontDestroyOnLoad(WwiseManager.wwiseGlobal);



        var settings = AssetDatabaseUtil.FindThenLoadAsset<AkWwiseAddressablesInitializationSettings>("AkWwiseInitializationSettings t:AkWwiseInitializationSettings");
        if (settings == null)
        {
            throw new Exception("[WwiseManager] Could not find AkWwiseInitializationSettings.asset");
        }

        var initalizer = WwiseManager.wwiseGlobal.AddComponent<AkInitializer>();
        initalizer.InitializationSettings = settings;

        var initAddressableBank = AssetDatabaseUtil.FindThenLoadAsset<WwiseAddressableSoundBank>("Init t:WwiseAddressableSoundBank");
        if (initAddressableBank == null)
        {
            throw new Exception("[WwiseManager] Could not find addressable bank Init.asset");
        }

        var initBankHolder = WwiseManager.wwiseGlobal.AddComponent<InitBankHolder>();
        initBankHolder.InitBank = initAddressableBank;

        WwiseManager.wwiseGlobal.AddComponent<AkAudioListener>();

        WwiseManager.wwiseGlobal.SetActive(true);

    }
#endif

    /// <summary>
    /// 加载一个 bank，wwise 会自己到文件系统或者 addressable 系统去加载
    /// 事件在 bank 中，在发送事件前需要加载所在 bank
    /// </summary>
    /// <param name="bankName"></param>
    public static async Task<bool> TryLoadBankAsync(string bankName)
    {
        return false;
        TryInitalizeInEditMode();
        if (Application.isPlaying)
        {
            var useWwise = DeveloperLocalSettings.IsUseWwise;
            if (!useWwise)
            {
                return false;
            }
        }

        var address = bankName + ".asset";
        var addressableBank = await BucketManager.Stuff.Main.GetOrAquireAsync<WwiseAddressableSoundBank>(address);
        AkAddressableBankManager.Instance.LoadBank(addressableBank);
        var result = await addressableBank.WaiteLoadResultAsync();
        Debug.Log($"[WwiseManager] try load bak '{bankName}': " + result);
        return result;
    }


    /// <summary>
    ///  卸载一个 bank
    /// </summary>
    /// <param name="bankName"></param>
    public static void UnloadBank(string bankName)
    {
        AkBankManager.UnloadBank(bankName);
    }

    /// <summary>
    /// 向 wwise 传递一个事件，wwise 会决定做什么
    /// </summary>
    /// <param name="eventName">事件id，由设计师给出</param>
    /// <param name="gameObject">3d音效的发声对象，如果填 null，会直接在接收器对象上播放</param>
    /// <param name="cb">事件结束的回调</param>
    public static void TryPostEvent(string eventName, GameObject gameObject = null, AkCallbackManager.EventCallback cb = null)
    {
        return;
        TryInitalizeInEditMode();
        if (Application.isPlaying)
        {
            var useWwise = DeveloperLocalSettings.IsUseWwise;
            if (!useWwise)
            {
                return;
            }
        }


        if (gameObject == null)
        {
            gameObject = wwiseGlobal;
        }
        Debug.Log("[WwiseManager] post event: " + eventName);
        AkSoundEngine.PostEvent(eventName, gameObject, (uint)AkCallbackType.AK_EndOfEvent, cb, null);
    }

    public static void TryPostEventList(List<string> eventList, GameObject gameObject = null)
    {
        foreach(var one in eventList)
        {
            TryPostEvent(one, gameObject);
        }
    }

    public static void SetSwitch(uint switchGroup, uint switchState, GameObject gameObject = null)
    {
        TryInitalizeInEditMode();
        if (Application.isPlaying)
        {
            var useWwise = DeveloperLocalSettings.IsUseWwise;
            if (!useWwise)
            {
                return;
            }
        }

        if (gameObject == null)
        {
            gameObject = wwiseGlobal;
        }
        AkSoundEngine.SetSwitch(switchGroup, switchState, gameObject);
    }

    public static void SetState(string stateGroup, string state)
    {
        TryInitalizeInEditMode();
        if(Application.isPlaying)
        {
            var useWwise = DeveloperLocalSettings.IsUseWwise;
            if (!useWwise)
            {
                return;
            }
        }


        AkSoundEngine.SetState(stateGroup, state);
    }

    public static void SetRTPC(string name, float value)
    {
        TryInitalizeInEditMode();
        if (Application.isPlaying)
        {
            var useWwise = DeveloperLocalSettings.IsUseWwise;
            if (!useWwise)
            {
                return;
            }
        }

        AkSoundEngine.SetRTPCValue(name, value);
    }
}
