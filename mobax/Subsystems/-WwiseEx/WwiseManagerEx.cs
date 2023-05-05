using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Threading.Tasks;
using AK.Wwise.Unity.WwiseAddressables;
using UnityEditor;

public class WwiseManagerEx : MonoBehaviour
{
    private static WwiseManagerEx m_Instance = null;
    public static WwiseManagerEx GetInstance()
    {
        if (Application.isPlaying)
        {
            if (m_Instance == null)
            {
                //m_Instance = GameObject.FindObjectOfType<WwiseManagerEx>();
                if (m_Instance == null)
                {
                    m_Instance = new GameObject(typeof(WwiseManagerEx).ToString(), typeof(WwiseManagerEx)).GetComponent<WwiseManagerEx>();
                    DontDestroyOnLoad(m_Instance);

                }
            }
        }
        else
        {
            m_Instance = GameObject.Find("WwiseManagerEx").GetComponent<WwiseManagerEx>();
        }
        return m_Instance;
    }

    //public WwiseManagerEx GetInstance()
    //{
    //    return m_Instance;
    //}

    private void Awake()
    {

        //if (m_Instance == null)
        //{
        //    m_Instance = this as WwiseManagerEx;
        //}
        //else
        //{
        //    DestroyImmediate(this);
        //}
    }

    //private void OnDestroy()
    //{
    //    Destroy();
    //}

    //protected virtual void Destroy()
    //{
    //}

    //------------------------------------------------------------------------------------------------------

    private GameObject _AudioListener; //??? 

    private WwiseSoundConfig _WwiseSoundConfig = new WwiseSoundConfig();

    private Vector3 ListenerPosition
    {
        get
        {
            if (_AudioListener == null)
                return Vector3.zero;
            return _AudioListener.transform.position;
        }
    }

    private GameObject wwiseGlobal = null;

    private bool _Inited = false;




    public float SfxVolume
    {
        get
        {
            //return _sfxVolume; 
            var ret = GetLocalSettingGroupVolume("sfx");
            return ret;
        }
        set
        {
            var old = GetLocalSettingGroupVolume("sfx");
            if (old != value)
            {
                SetLocalSettingGroupVolume("sfx", value);
                OnVolumeChanged();
            }
        }
    }

    public float MusicVolume
    {
        get
        {
            //return _sfxVolume; 
            var ret = GetLocalSettingGroupVolume("music");
            return ret;
        }
        set
        {
            var old = GetLocalSettingGroupVolume("music");
            if (old != value)
            {
                SetLocalSettingGroupVolume("music", value);
                OnVolumeChanged();
            }
        }
    }
    
    public float VoiceVolume
    {
        get
        {
            //return _sfxVolume; 
            var ret = GetLocalSettingGroupVolume("Volume_Voice");
            return ret;
        }
        set
        {
            var old = GetLocalSettingGroupVolume("Volume_Voice");
            if (old != value)
            {
                SetLocalSettingGroupVolume("Volume_Voice", value);
                OnVolumeChanged();
            }
        }
    }

    public void ResetVolume()
    {
        this.DeleteLocalSettingGroupVolume("sfx");
        this.DeleteLocalSettingGroupVolume("music");
    }

    Dictionary<string, float> _groupToLocalSettingVolumeDic = new Dictionary<string, float>();

    public float GetLocalSettingGroupVolume(string group)
    {
        var b = _groupToLocalSettingVolumeDic.TryGetValue(group, out var volume);
        if (b)
        {
            return volume;
        }
        else
        {
            var defaultValue = 1f;
            var v = PlayerPrefs.GetFloat($"Wwise.{group}.volume", defaultValue);
            _groupToLocalSettingVolumeDic[group] = v;
            return v;
        }
    }

    public void SetLocalSettingGroupVolume(string group, float volume)
    {
        _groupToLocalSettingVolumeDic[group] = volume;
        PlayerPrefs.SetFloat($"Wwise.{group}.volume", volume);
    }

    public void DeleteLocalSettingGroupVolume(string group)
    {
        _groupToLocalSettingVolumeDic.Remove(group);
        PlayerPrefs.DeleteKey($"Wwise.{group}.volume");
    }

    public async Task CreateWwiseGlobal()
    {
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
        //AkWwiseInitializationSettings settings = Resources.Load<AkWwiseInitializationSettings>("AkWwiseInitializationSettings");

        if (settings == null)
        {
            throw new Exception("[WwiseManager] Could not find AkWwiseInitializationSettings.asset");
        }

        var initalizer = wwiseGlobal.AddComponent<AkInitializer>();
        initalizer.InitializationSettings = settings;
        WwiseAddressableSoundBank initAddressableBank = null;

        var isUseExternalRes = IsWwiseUseExternalRes;
        if (Application.isEditor && isUseExternalRes)
        {
#if UNITY_EDITOR
            bool isSetWwisePath = AkBasePathGetter.IsSettingWwisePath();
            if (isSetWwisePath)
            {
                initAddressableBank = WwiseBankImporter.GenerateWwiseSoundBankAssetEditor("Init.bnk");
            }
            else
            {
                initAddressableBank = await BucketManager.Stuff.Main.GetOrAquireAsync<WwiseAddressableSoundBank>("Init.asset", true);
            }
#endif
        }
        else
        {
            initAddressableBank = await BucketManager.Stuff.Main.GetOrAquireAsync<WwiseAddressableSoundBank>("Init.asset", true);
        }

        if (initAddressableBank == null)
        {
            throw new Exception("[WwiseManager] Could not find addressable bank Init.asset");
        }

        var initBankHolder = wwiseGlobal.AddComponent<InitBankHolder>();
        initBankHolder.InitBank = initAddressableBank;

        //SetBasePath();
        await ParseXmlAsync();
        wwiseGlobal.SetActive(true);
        InitGlobalEvent();

        CheckAudioListener();
        InitLoadGlobalBank();
        //PostWwiseEvent("SE_MON_Robber_Water_ATK", null);
        //WwiseBankImporter.GenerateWwiseSoundBankAssetEditor("Hero_Awu.bnk");
        this.PushVolumToWwiseEngine();

    } 
        //PostWwiseEvent("SE_UI_Button", null);

    void TryInitalizeInEditMode()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            if (WwiseManager.wwiseGlobal == null)
            {
                InitalizeInEditMode();
                EditorApplication.update += UpdateEventAndMemory;
            }
        }
#endif
    }

#if UNITY_EDITOR
    public void InitalizeInEditMode()
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

        var settings = AssetDatabaseUtil.FindThenLoadAsset<AkWwiseAddressablesInitializationSettings>("AkWwiseInitializationSettings t:AkWwiseInitializationSettings");
        if (settings == null)
        {
            throw new Exception("[WwiseManager] Could not find AkWwiseInitializationSettings.asset");
        }

        var initalizer = WwiseManager.wwiseGlobal.AddComponent<AkInitializer>();
        initalizer.InitializationSettings = settings;

        //var initAddressableBank = AssetDatabaseUtil.FindThenLoadAsset<WwiseAddressableSoundBank>("Init t:WwiseAddressableSoundBank");
        //if (initAddressableBank == null)
        //{
        //    throw new Exception("[WwiseManager] Could not find addressable bank Init.asset");
        //}
        //AkAddressableBankManager.Instance.UnloadAllBanks();

        WwiseAddressableSoundBank initAddressableBank = null;
        bool isSetWwisePath = AkBasePathGetter.IsSettingWwisePath();
        if (isSetWwisePath)
        {
            initAddressableBank = WwiseBankImporter.GenerateWwiseSoundBankAssetEditor("Init.bnk");

        }
        else
        {

            return;
        }

        var initBankHolder = WwiseManager.wwiseGlobal.AddComponent<InitBankHolder>();

        initBankHolder.InitBank = initAddressableBank;
        AkAddressableBankManager.Instance.LoadBank(initAddressableBank);
        WwiseManager.wwiseGlobal.transform.SetParent(this.transform, false);
        ParseXmlEditor();
        InitGlobalEvent();
        WwiseManager.wwiseGlobal.AddComponent<AkAudioListener>();

        WwiseManager.wwiseGlobal.SetActive(true);
    }
#endif
    
    /// <summary>
    /// ??????? bank??wwise ???????????????? addressable ???????
    /// ????? bank ?У????????????????????? bank
    /// </summary>
    /// <param name="bankName"></param>
    public async Task<bool> TryLoadBankAsync(string bankName, WwiseManagerEx.BankLoadedCallBack callback = null, BankHandle handle = null)
    {
        //TryInitalizeInEditMode();
        if (Application.isPlaying)
        {
            var useWwise = DeveloperLocalSettings.IsUseWwise;
            if (!useWwise)
            {
                return false;
            }
        }
        WwiseAddressableSoundBank addressableBank = null;

        var useExternlRes = IsWwiseUseExternalRes;
        if (Application.isEditor && useExternlRes)
        {
#if UNITY_EDITOR
            string name = bankName + ".bnk";
            addressableBank = WwiseBankImporter.GenerateWwiseSoundBankAssetEditor(name);
#endif
        }
        else
        {
            var address = bankName + ".asset";
            addressableBank = await BucketManager.Stuff.Main.GetOrAquireAsync<WwiseAddressableSoundBank>(address);
        }

        if (addressableBank == null)
        {
            throw new Exception("[WwiseManager] Could not find addressable bank " + bankName);
        }
        //var address = bankName + ".asset";
        //WwiseAddressableSoundBank addressableBank = await BucketManager.Stuff.Main.GetOrAquireAsync<WwiseAddressableSoundBank>(address);
        AkAddressableBankManager.Instance.LoadBank(addressableBank);
        var result = await addressableBank.WaiteLoadResultAsync();
        if (result)
        {
            handle.addressBank = addressableBank;
            callback();
        }
        else {
            Debug.LogError($"[WwiseManager] try load bak '{bankName}': " + "failed");

        }
        //AkSoundEngine.PostEvent("sound_boss_warn", this.globalEventRoot, (uint)AkCallbackType.AK_EndOfEvent, null, null);
        return result;
    }

    private void SetBasePath()
    {
#if UNITY_EDITOR
        string path = AkBasePathGetter.GetWwiseProjectBankPath();
        if (!Directory.Exists(path))
        {
            //???Res??
            path = AkBasePathGetter.GetGameProjectBankPath();
        }
        Debug.Log("wwise basePath   " + path);
        var result = AkSoundEngine.SetBasePath(path);
        if (result == AKRESULT.AK_Success)
        {
            Debug.Log("wwise basePath   " + path);
        }
        else
        {
            Debug.LogError($"???Wwise SetBasePath:{path}???????:{result}");
        }
#endif
    }
    public bool IsWwiseUseExternalRes

    {
        get
        {
            var ret = DeveloperLocalSettings.IsWwiseUseExternlRes;
            return ret;
        }
    }

    public async Task<string> ReadManifestAsync()
    {
        if(Application.isEditor && IsWwiseUseExternalRes)
        {
#if UNITY_EDITOR
            string path = AkBasePathGetter.GetWwiseProjectBankPath();
            if (!Directory.Exists(path))
            {
                path = AkBasePathGetter.GetGameProjectBankPath();
            }
            string filePath = Path.Combine(path, "SoundbanksInfo.xml");
            var cotnent = File.ReadAllText(filePath);
            return cotnent;
#else
            return "";
#endif
        }
        else
        {
            var wwisePlatform = AkBasePathGetter.GetPlatformName();
            var address = $"Assets/wwiseData/{wwisePlatform}/SoundbanksInfo.xml";
            var asset = await BucketManager.Stuff.Main.GetOrAquireAsync<TextAsset>(address);
            var content = asset.text;
            return content;
        }
    }


    private async Task ParseXmlAsync()
    {
        //#if UNITY_EDITOR
        //string path = AkBasePathGetter.GetWwiseProjectBankPath();
        //if (!Directory.Exists(path))
        //{
        //    //???Res??
        //    path = AkBasePathGetter.GetGameProjectBankPath();
        //}
        //string filePath = Path.Combine(path, "SoundbanksInfo.xml");
        //if (!File.Exists(filePath))
        //{
        //    Debug.LogError("WwiseBankXmlPath??????");
        //    return;
        //}
        var content = await ReadManifestAsync();

        _WwiseSoundConfig._dicBankAndEventID.Clear();
        //????ζ???????????·??
        //XElement xmlDoc = XElement.Load(filePath);

        XElement xmlDoc = XElement.Parse(content);
        IEnumerable<XElement> allSoundBank = xmlDoc.Descendants("SoundBank");
        if (allSoundBank == null)
        {
            Debug.LogError("SoundBank?б??????");
            return;
        }
        foreach (XElement bank in allSoundBank)
        {
            XElement bankName = bank.Element("ShortName");
            if (string.IsNullOrEmpty(bankName.Value))
            {
                Debug.LogError("null Bank" + bankName.Value);
            }
            XElement events = bank.Element("IncludedEvents");
            if (events == null)
            {
                continue;
            }
            IEnumerable<XElement> allEvent = events.Descendants("Event");
            if (allEvent == null)
            {
                continue;
            }
            foreach (XElement eventXml in allEvent)
            {
                XAttribute nameAttritub = eventXml.Attribute("Name");
                XAttribute id = eventXml.Attribute("Id");
                if (nameAttritub == null || id == null)
                {
                    continue;
                }
                string eventName = nameAttritub.Value;
                if (string.IsNullOrEmpty(eventName))
                {
                    Debug.LogError("null EventName in Bank:'" + bankName.Value);

                }
                if (_WwiseSoundConfig._dicBankAndEventID.ContainsKey(eventName))
                {
                    continue;
                }
                uint eventID = uint.Parse(id.Value);
                WwiseSoundConfig.BanknameAndEventID config = new WwiseSoundConfig.BanknameAndEventID();
                config.bankName = bankName.Value;
                config.eventID = eventID;
                _WwiseSoundConfig._dicBankAndEventID.Add(eventName, config);

            }
        }

        //foreach (var item in _dicBankAndEventID)
        //{
        //    Debug.Log("<color=#00FF00>" + item.Key + "    _bankName:" + item.Value.bankName + "    _ID:" + item.Value.eventID + "</color>");
        //}
//#endif
    }

    private  void ParseXmlEditor()
    {
#if UNITY_EDITOR
        string path = AkBasePathGetter.GetWwiseProjectBankPath();
        if (!Directory.Exists(path))
        {
            //???Res??
            path = AkBasePathGetter.GetGameProjectBankPath();
        }
        string filePath = Path.Combine(path, "SoundbanksInfo.xml");
        if (!File.Exists(filePath))
        {
            Debug.LogError("WwiseBankXmlPath??????");
            return;
        }

        _WwiseSoundConfig._dicBankAndEventID.Clear();
        //????ζ???????????·??
        XElement xmlDoc = XElement.Load(filePath);

        //XElement xmlDoc = XElement.Parse(content);
        IEnumerable<XElement> allSoundBank = xmlDoc.Descendants("SoundBank");
        if (allSoundBank == null)
        {
            Debug.LogError("SoundBank?б??????");
            return;
        }
        foreach (XElement bank in allSoundBank)
        {
            XElement bankName = bank.Element("ShortName");
            if (string.IsNullOrEmpty(bankName.Value))
            {
                Debug.LogError("null Bank" + bankName.Value);
            }
            XElement events = bank.Element("IncludedEvents");
            if (events == null)
            {
                continue;
            }
            IEnumerable<XElement> allEvent = events.Descendants("Event");
            if (allEvent == null)
            {
                continue;
            }
            foreach (XElement eventXml in allEvent)
            {
                XAttribute nameAttritub = eventXml.Attribute("Name");
                XAttribute id = eventXml.Attribute("Id");
                if (nameAttritub == null || id == null)
                {
                    continue;
                }
                string eventName = nameAttritub.Value;
                if (string.IsNullOrEmpty(eventName))
                {
                    Debug.LogError("null EventName in Bank:'" + bankName.Value);

                }
                if (_WwiseSoundConfig._dicBankAndEventID.ContainsKey(eventName))
                {
                    continue;
                }
                uint eventID = uint.Parse(id.Value);
                WwiseSoundConfig.BanknameAndEventID config = new WwiseSoundConfig.BanknameAndEventID();
                config.bankName = bankName.Value;
                config.eventID = eventID;
                _WwiseSoundConfig._dicBankAndEventID.Add(eventName, config);

            }
        }

        foreach (var item in _WwiseSoundConfig._dicBankAndEventID)
        {
            Debug.Log("<color=#00FF00>" + item.Key + "    _bankName:" + item.Value.bankName + "    _ID:" + item.Value.eventID + "</color>");
        }
#endif

    }

    private void InitGlobalEvent() 
    {
        if (this.globalEventRoot != null) 
        {
            Debug.LogError("this.globalEventRoot != null");
            return;
        }
        this.globalEventRoot = new GameObject("GlobalEventRoot");
        this.globalEventRoot.transform.SetParent(this.transform, false);

    }


    void OnVolumeChanged()
    {
        this.PushVolumToWwiseEngine();
    }

    void PushVolumToWwiseEngine()
    {
        var sfx = GetLocalSettingGroupVolume("sfx");
        var music = GetLocalSettingGroupVolume("music");
        var voice = GetLocalSettingGroupVolume("Volume_Voice");

        SetRTPCValue("Volume_Music", music * 100);
        SetRTPCValue("Volume_Effects", sfx * 100);
        SetRTPCValue("Volume_Voice", voice * 100);
    }


    private void CheckAudioListener()
    {
        if (_AudioListener == null)
        {
            _AudioListener = new GameObject("AkListener");
            _AudioListener.AddComponent<AkAudioListener>();
            _AudioListener.transform.SetParent(this.gameObject.transform, false);

        }
    }

    void OnDestroy()
    {
        DestoryWwise();
    }

    public void DeleteEffectEvent(string objName, GameObject objInstance)
    {
        var config = GetBankByEventName(objName);
        if (config == null)
        {
            return;
        }
        DeleteEvent(config.eventID, objInstance);
        
    }
    
    /// <summary>
    /// 刷新监听器的参数
    /// </summary>
    /// <param name="pos"></param>
    public void UpdateListenerParam(Vector3 pos, Quaternion rot)
    {
        if(_AudioListener == null)
            return;
        
        _AudioListener.transform.position = pos;
        _AudioListener.transform.rotation = rot;
    }

    public WwiseSoundConfig.BanknameAndEventID GetBankByEventName(string eventname)
    {
        if (_WwiseSoundConfig._dicBankAndEventID.TryGetValue(eventname, out var conf))
        {
            return conf;
        }
        if (!string.IsNullOrEmpty(eventname))
        {
            Debug.LogError("<color=#8B0000>" + "WWiseEventName:" + ":" + eventname + "    not bank" + "</color>");
        }
        return null;
    }

    public List<BankHandle> listBanksToUnLoad = new List<BankHandle>();

    private Queue<EventHandle> allEventHandles = new Queue<EventHandle>();

    public Dictionary<string, BankHandle> usedBankInfoOfBankName = new Dictionary<string, BankHandle>();

    public delegate void BankLoadedCallBack();

    public delegate void EndOfEventCallBack();

    private GameObject globalEventRoot = null;

    private ulong reqEventID = 0L;

    private int maxEventNumberOneFrame = 10;

    private delegate void delayCallBack();

    private delayCallBack delayCallBacks;

    public enum EventState
    {
        OnWait = 1,
        Playing = 2,
        EndOfPlaying = 3,
        Idle = 4
    }

    private void LateUpdate()
    {
        UpdateEventAndMemory();
    }

    private void UpdateEventAndMemory()
    {
        UpdateEvent();
        UnLoadBankFromMemory();
    }

    void UpdateEvent()
    {
        int curNumber = 0;
        while (curNumber < maxEventNumberOneFrame)
        {
            if ((this.allEventHandles.Count == 0))
            {
                break;
            }
            EventHandle curEventHandle = this.allEventHandles.Dequeue();
            if (curEventHandle.CanPlay())
            {
                DoCheckBank(curEventHandle);
            }
            curNumber++;

        }
    }

    private void DoCheckBank(EventHandle curEventHandle)
    {
        CheckBank(curEventHandle.BankName, true, delegate
        {
            AkSoundEngine.PostEvent(curEventHandle.EventName, curEventHandle.TargetGameobject, (uint)AkCallbackType.AK_EndOfEvent, PostEventCallBack, curEventHandle);
        });
    }
    
    private bool CheckWwiseEnvironment()
    {
        Transform globalEventTransform = this.gameObject.transform.Find("GlobalEventRoot");
        if (globalEventTransform == null)
        {
            return false;
        }
        return true;
    }

    private void PostEventCallBack(object in_cookie, AkCallbackType in_type, AkCallbackInfo in_info)
    {
        EventHandle handle = in_cookie as EventHandle;
        //Debug.Log("in_type:"+ in_type);
        ReduceBank(handle.BankName);
        if (handle.eventEndOfBack != null)
        {
            handle.eventEndOfBack();
        }
        //Debug.Log("<color=#00FF00>" + "??????name:" + (handle.EventName) + "</color>");
    }
    public void TryPostEventList(List<string> eventList, GameObject gameObject = null)
    {
        for (int i = 0; i < eventList.Count; i++)
        {
            PostWwiseEvent(eventList[i], gameObject);
        }
    }

    public void PostWwiseEvent(string _event, GameObject _targetObj = null, bool _isImmediately = false, EndOfEventCallBack _callBack = null, System.Action endCallback = null)
    {
        
        TryInitalizeInEditMode();

        if (_event == string.Empty)
        {
            return;
        }
        var config = GetBankByEventName(_event);
        if (config == null)
        {
            AkSoundEngine.PostEvent(_event, this.globalEventRoot);
            return;
        }
        uint _eventName = config.eventID;
        string _bankName = config.bankName;
        if (string.IsNullOrEmpty(_bankName))
        {
            //Debug.Log("<color=#8B0000>" + "WWise??Ч_eventName:" + _eventName + "____________bankName:" + _bankName + "</color>");
            return;
        }
        _targetObj = (_targetObj == null) ? this.globalEventRoot : _targetObj;
        this.reqEventID = this.reqEventID + 1;
        EventHandle handle = new EventHandle(this.reqEventID, Time.frameCount, _eventName, _targetObj, _callBack, _bankName);
        //Debug.Log("<color=#00FF00>" + "????Event???--obj????" + (_targetObj.name) + "----?????" + _eventName + "----event:" + _bankName +"---??????" + Time.frameCount+ "</color>");
        if (_isImmediately)
        {
            CheckBank(_bankName, true, delegate
            {
                uint test = (uint)(AkCallbackType.AK_EndOfEvent | AkCallbackType.AK_MusicSyncBeat);
                AkSoundEngine.PostEvent(_eventName, _targetObj, (uint)test, PostEventCallBack, handle);
            });
        }
        else
        {
            allEventHandles.Enqueue(handle);
        }
    }

    public void SetRTPCValue(string rtpcName, float value)
    {
        AKRESULT akresult = AkSoundEngine.SetRTPCValue(rtpcName, value);
        if (akresult != AKRESULT.AK_Success) 
        {
            Debug.LogError("SetRTPCValue akresult != AKRESULT.AK_Success");
        }
    }

    public void SetState(string stateGroup, string state)
    {
        AKRESULT akresult = AkSoundEngine.SetState(stateGroup, state);
        if (akresult != AKRESULT.AK_Success)
        {
            Debug.LogError("SetState akresult != AKRESULT.AK_Success");
        }
    }

    public void SetSwitch(string switchGroup, string switchState, GameObject gameObject = null)
    {
        AKRESULT akresult = AkSoundEngine.SetSwitch(switchGroup, switchState, gameObject ?? this.globalEventRoot);
        if (akresult != AKRESULT.AK_Success)
        {
            Debug.LogError("SetSwitch akresult != AKRESULT.AK_Success");
        }
    }

    public void DeleteEvent(uint eventName, GameObject game = null)
    {
        foreach (var item in allEventHandles)
        {
            if (item.EventName == eventName && item.StartFrame == Time.frameCount)
            {

                game = game == null ? this.globalEventRoot : game;
                if (game.GetInstanceID().Equals(item.TargetGameobject.GetInstanceID()))
                {
                    item.SetCanPlay(false);
                    //Debug.Log("<color=#228B22>" + "???eventName--obj????" + (game.name) + "----?????" + eventName + "---??????" + Time.frameCount + "</color>");
                }
            }
        }
    }

    private void CheckBank(string _bankName, bool isAsync, BankLoadedCallBack _callBack)
    {
        BankHandle handle = null;
        Dictionary<string, BankHandle> useBankInfoOfBankName = this.usedBankInfoOfBankName;
        lock (useBankInfoOfBankName)
        {
            if (this.usedBankInfoOfBankName.TryGetValue(_bankName, out handle))
            {
                if (handle != null)
                {
                    handle.IncreaseRef();
                    if (handle.IsLoaded)
                    {
                        if (_callBack != null)
                        {
                            _callBack();
                        }
                    }
                    else
                    {
                        //bank???????? ??μ????????????????????????????????
                        handle.taskList.Add(_callBack);
                        //Debug.Log("Bank????????δ????bank????" + _bankName);
                    }
                    return;
                }
            }
            handle = new BankHandle(_bankName);
            if (isAsync)
            {
                handle.taskList.Add(_callBack);
            }
            this.usedBankInfoOfBankName.Add(_bankName, handle);
            handle.LoadBank(isAsync, _callBack);
        }

    }

    private void ReduceBank(string bankName)
    {
        Dictionary<string, BankHandle> banks = this.usedBankInfoOfBankName;
        lock (banks)
        {
            BankHandle handle = null;
            if (this.usedBankInfoOfBankName.TryGetValue(bankName, out handle))
            {
                handle.DecreaseRef();
            }
        }
    }

    private void UnLoadBankFromMemory()
    {
        int unLoadMemory = -1;
        for (int i = 0; i < listBanksToUnLoad.Count; i++)
        {
            float duration = Time.realtimeSinceStartup - listBanksToUnLoad[i].LastSleepTime;
            if (duration < 5f)
            {
                break;
            }
            unLoadMemory = i;
        }
        if (unLoadMemory >= 0)
        {
            for (int j = unLoadMemory; j >= 0; j--)
            {
                if (this.listBanksToUnLoad[j] != null)
                {
                    this.listBanksToUnLoad[j].UnLoadBank();
                }
            }
        }

    }
    private void InitLoadGlobalBank()
    {
        _Inited = CheckWwiseEnvironment();

    }

    private void DestoryWwise()
    {
        AkBankManager.UnloadInitBank();
        AkBankManager.UnloadBank("Common.bnk");
        _Inited = false;
    }


}
public class BankHandle
{
    private readonly string mBankName;

    public WwiseAddressableSoundBank addressBank;

    public int RefCount { get; private set; }
    public float LastSleepTime { get; private set; }

    public List<WwiseManagerEx.BankLoadedCallBack> taskList = new List<WwiseManagerEx.BankLoadedCallBack>();

    public bool IsLoaded { get { return RefCount > 0; } }
    public BankHandle(string name)
    {
        this.mBankName = name;
    }

    public void Dispose()
    {
        this.RefCount = 0;
    }

    public void IncreaseRef()
    {
        if (this.RefCount == 0)
        {
            this.LastSleepTime = float.MaxValue;
            WwiseManagerEx.GetInstance().listBanksToUnLoad.Remove(this);
        }
        this.RefCount = this.RefCount + 1;
        //Debug.Log("<color=#D2691E>" + "bank????:" + this.mBankName+"-----???ü??? + 1 " + this.RefCount+"</color>");
    }

    public void DecreaseRef()
    {
        this.RefCount = this.RefCount - 1;
        if (this.RefCount == 0)
        {
            this.LastSleepTime = Time.realtimeSinceStartup;
            WwiseManagerEx.GetInstance().listBanksToUnLoad.Add(this);
        }
        //Debug.Log("<color=#D2691E>" + "bank????:" + this.mBankName + "-----???ü??? - 1 " + this.RefCount + "</color>");

    }

    private void LoadCallBack()
    {
        this.IncreaseRef();
        for (int i = 0; i < taskList.Count; i++)
        {
            taskList[i]();
        }
    }

    public void LoadBank(bool isAsync, WwiseManagerEx.BankLoadedCallBack _callBack)
    {

        if ((this.RefCount == 0))
        {
            this.HandleLoadBank(isAsync, _callBack);
        }

    }

    public async void HandleLoadBank(bool _isAsync, WwiseManagerEx.BankLoadedCallBack callBack)
    {
        if (_isAsync)
        {
            //AkBankManager.LoadBankAsync(this.mBankName, LoadCallBack);
            await WwiseManagerEx.GetInstance().TryLoadBankAsync(this.mBankName, LoadCallBack,this);
        }
        else
        {
            AkBankManager.LoadBank(this.mBankName, false, false);
            if (callBack != null)
            {
                callBack();
            }
            this.IncreaseRef();
        }
    }

    public void UnLoadBank()
    {
        //AkBankManager.UnloadBank(this.mBankName);
        AkAddressableBankManager.Instance.UnloadBank(this.addressBank);
        WwiseManagerEx.GetInstance().listBanksToUnLoad.Remove(this);
        WwiseManagerEx.GetInstance().usedBankInfoOfBankName.Remove(this.mBankName);
    }

}

public class EventHandle
{
    private ulong reqEventID;
    public ulong ReqEventID { get { return reqEventID; } set { value = reqEventID; } }

    private float startFrame;
    public float StartFrame { get { return startFrame; } set { value = startFrame; } }

    private uint eventName;
    public uint EventName { get { return eventName; } set { value = eventName; } }

    private string bankName;
    public string BankName { get { return bankName; } set { value = bankName; } }

    private GameObject targetGameobject;
    public GameObject TargetGameobject { get { return targetGameobject; } set { value = targetGameobject; } }

    WwiseManagerEx.EventState mEventState;

    public WwiseManagerEx.EventState MEventState { get { return mEventState; } set { value = mEventState; } }

    public WwiseManagerEx.EndOfEventCallBack eventEndOfBack;

    public bool isPlayOrNot = true;

    public EventHandle(ulong ID, float startFrame, uint _eventName, GameObject target, WwiseManagerEx.EndOfEventCallBack back, string bankName)
    {
        this.reqEventID = ID;
        this.eventName = _eventName;
        this.targetGameobject = target;
        this.bankName = bankName;
        this.eventEndOfBack = back;
        this.startFrame = startFrame;
        this.isPlayOrNot = true;
    }

    public bool IsHandleEventCommon(EventHandle a, EventHandle b)
    {
        if (a.targetGameobject != b.targetGameobject)
        {
            return false;
        }
        if (a.eventName != b.eventName)
        {
            return false;
        }
        if (a.startFrame != b.startFrame)
        {
            return false;
        }
        return true;
    }

    public void SetCanPlay(bool isPlay)
    {
        this.isPlayOrNot = isPlay;
    }

    public bool CanPlay()
    {
        return isPlayOrNot == true;
    }


}
public class WwiseSoundConfig
{
    public Dictionary<string, BanknameAndEventID> _dicBankAndEventID = new Dictionary<string, BanknameAndEventID>();

    public class BanknameAndEventID
    {
        public string bankName;
        public uint eventID;

    }
}