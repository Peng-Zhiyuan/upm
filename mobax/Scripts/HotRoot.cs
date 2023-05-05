using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.ResourceManagement;
using System.Reflection;
using Plot.Runtime;
using System.Globalization;

/// <summary>
/// 热更新程序集的启动入口
/// </summary>
public class HotRoot : MonoBehaviour
{
    async void Start()
    {
        Debug.Log("[HotRoot] Start");

        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
        // address 找不到不需要打印
        //ResourceManager.ExceptionHandler = (a, b) =>
        //{
        //    // do notion
        //};

        // 获取启动器的 loading 页面
        var pageGo = GameObject.Find("LauncherLoadingPage");
        var page = pageGo.GetComponent<LauncherLoadingPage>();

        // 需要先加载资源
        // 因为 service 里可能会需要资源
        Debug.Log("[HotRoot] preload");
        var bucket = BucketManager.Stuff.Main;
        var taskList = new List<Task>();
        taskList.Add(bucket.AquireIfNeedAsync("BlockFloating.prefab")); // 这个没有对应名称的类， 所以就直接用字符串写
        taskList.Add(bucket.AquireIfNeedAsync($"{nameof(StatusBoxFloating)}.prefab"));
        taskList.Add(bucket.AquireIfNeedAsync($"{nameof(DialogFloating)}.prefab"));
        taskList.Add(bucket.AquireIfNeedAsync($"{nameof(ExceptionFloating)}.prefab"));
        taskList.Add(bucket.AquireIfNeedAsync($"{nameof(MaskFloating)}.prefab"));
        taskList.Add(bucket.AquireIfNeedAsync($"{nameof(TaskFloating)}.prefab"));
        taskList.Add(bucket.AquireIfNeedAsync($"{nameof(LoadingFloating)}.prefab"));
        taskList.Add(bucket.AquireIfNeedAsync($"{nameof(TopFloatingV2)}.prefab"));
        taskList.Add(bucket.AquireIfNeedAsync($"{nameof(ToastFloating)}.prefab"));
        taskList.Add(bucket.AquireIfNeedAsync($"{nameof(RewardFloating)}.prefab"));
        taskList.Add(bucket.AquireIfNeedAsync($"{nameof(GuideFloating)}.prefab"));
        taskList.Add(bucket.AquireIfNeedAsync($"{nameof(ItemTipFloating)}.prefab"));
        taskList.Add(bucket.AquireIfNeedAsync($"{nameof(ItemPackFloating)}.prefab"));
        taskList.Add(bucket.AquireIfNeedAsync($"{nameof(CircuitInfoFloating)}.prefab")); // 也是通用的
        taskList.Add(bucket.AquireIfNeedAsync($"{nameof(PowerChangeFloating)}.prefab"));
        taskList.Add(bucket.AquireIfNeedAsync($"{nameof(GuideBlockFloating)}.prefab"));
        taskList.Add(bucket.AquireIfNeedAsync($"{nameof(ConsoleFloating)}.prefab"));
        taskList.Add(bucket.AquireIfNeedAsync($"{nameof(PlotOptionsView)}.prefab"));
        taskList.Add(bucket.AquireIfNeedAsync($"{nameof(FullResDownloadingFloating)}.prefab"));
        taskList.Add(bucket.AquireIfNeedAsync($"{nameof(ItemInfoFloating)}.prefab"));
        taskList.Add(bucket.AquireIfNeedAsync($"{nameof(ShopPopEventGiftFloating)}.prefab"));
        
        taskList.Add(bucket.AquireIfNeedAsync("PageDisplayAnimator.controller"));
        taskList.Add(bucket.AquireIfNeedAsync("PopWindowAnimator.controller"));
        await Task.WhenAll(taskList);

        //var font = await BucketManager.Stuff.Main.GetOrAquireAsync<Font>("TsangerYuYangT-W05.ttf");
        //Debug.Log("font ： " + font);
        await BucketManager.Stuff.Font.AquireByLabelAsync<Font>("$font");

        // todo: service 需要一个异步生命周期
        await BucketManager.Stuff.Conf.AquireByLabelAsync<TextAsset>("$client_config_res");



        // 从热更程序集创建服务
        Debug.Log("[HotRoot] create services");
        var hotAssembly = typeof(HotRoot).Assembly;
        ServiceSystem.Stuff.Create(hotAssembly);
        WwiseService.Stuff.OnCreate();

        // 显示预设值面板
        if (DeveloperLocalSettings.IsDevelopmentMode)
        {
            await this.ShowPresetThenWaitCloseAsync();
        }

        // 加载静态数据
        Debug.Log("[HotRoot] load static data");
        await StaticDataRuntime.ReloadAsync();

        await QualitySetting.AutoSetQualityByConf();

        MemoryRelease.Stuff.Init();

        await LocalizationUtil.ProcessAsync();

        this.LoadPlotDataFromStaticData();


        // 屏蔽字
        Debug.Log("[HotRoot] init keyword data");
        {
            var rowList = StaticData.MaskwordTable.ElementList;
            foreach (var row in rowList)
            {
                Badword.Add(row.Des, MatchType.HoleWord);
            }
        }
        {
            var rowList = StaticData.MaskwordFuzzyTable.ElementList;
            foreach (var row in rowList)
            {
                Badword.Add(row.Des, MatchType.Fuzzy);
            }
        }

        // Util 注册翻译
        DateUtil.onGetText = (key) => { return LocalizationManager.Stuff.GetText(key); };


        var useWwise = DeveloperLocalSettings.IsUseWwise;
        if (useWwise)
        {
            //await WwiseManager.CreateWwiseGlobal();
            await WwiseManagerEx.GetInstance().CreateWwiseGlobal();
            Debug.Log("[HotRoot] init wwise");
            //await WwiseManager.CreateWwiseGlobal();

            // 测试加载 bank 与传递事件
            //await WwiseManager.TryLoadBankAsync("Effect");
            //WwiseManager.TryPostEvent("sound_boss_warn");
        }


        TrackManager.Initalize();


        // 解压视频
        //Debug.Log("[HotRoot] sync video");
        //VideoManager.Stuff.SyncFromAssetsToDiskAsync();


        page.UIProgressValue = 1;
        await page.WaitProcessBarFullAsync();

        await WaitGameCompleteAsync();
        await WaitLoadingPageHoldOn();


        //WwiseUtil.InitDevicesDetector();

        Debug.Log("[HotRoot] goto login page");
        UIEngine.Stuff.ForwardOrBackTo<LoginPage>();



        LauncherUiManager.Stuff.Remove<LauncherLoadingPage>();
        RemoveUI<LauncherGamePage>();
        RemoveUI("PuzzleGameInLoadingPage");
        LauncherUiManager.Stuff.gameObject.SetActive(false);

        var guideV2 = HotLocalSettings.IsGuideV2Enabled;
        if (guideV2)
        {
            await GuideManagerV2.Stuff.LoadForceGuideScript();
            await GuideManagerV2.Stuff.LoadTriggerdGuideScript();
        }

        if(IggSdkManager.Stuff.IsIggChannel)
        {
            TrackManager.SendGeneralEvents();
        }
    }

    Task ShowPresetThenWaitCloseAsync()
    {
        var tcs = new TaskCompletionSource<bool>();
        UIEngine.Stuff.Forward<HotPresetPage>();
        HotPresetPage.okHandler = () =>
        {
            UIEngine.Stuff.Back();
            tcs.SetResult(true);
        };
        return tcs.Task;
    }

    public async Task WaitLoadingPageHoldOn()
    {
        while(LauncherLoadingPage.holdon)
        {
            await Task.Delay(100);
        }
    }



    public async Task WaitGameCompleteAsync()
    {
        var page = GameObject.FindObjectOfType<LauncherGamePage>();
        if(page == null)
        {
            return;
        }
        else if(!page.gameObject.activeSelf)
        {
            return;
        }
        else
        {
            while(!LauncherGameProcesser.Finished)
            {
                await Task.Delay(100);
            }
        }
    }


    public void RemoveUI(string uiName)
    {
        var go = GameObject.Find(uiName);
        if(go != null)
        {
            GameObject.Destroy(go);
        }
    }

    public void RemoveUI<T>()
    {
        var prefabName = typeof(T).Name;
        var go = GameObject.Find(prefabName);
        if(go != null)
        {
            Destroy(go);
        }
    }

    void LoadPlotDataFromStaticData()
    {
        var type = typeof(StaticData);
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Static);
        foreach (var p in properties)
        {
            var n = p.Name;
            if (n.StartsWith("StoryEvent") && n.EndsWith("Table"))
            {
                var val = p.GetValue(null);
                var tt = val.GetType();
                var field = tt.GetField("table");
                var table = field.GetValue(val);
                var ttt = table.GetType();
                var pp = ttt.GetProperty("Dics");
                var dic = pp?.GetValue(table);
                var dataDic = dic as IDictionary;
                PlotDataManager.Stuff.Add2CurrentPlotData(dataDic);
            }

            if (n.StartsWith("StoryManage") && n.EndsWith("Table"))
            {
                var val = p.GetValue(null);
                var tt = val.GetType();
                var field = tt.GetField("table");
                var table = field.GetValue(val);
                var ttt = table.GetType();
                var pp = ttt.GetProperty("Dics");
                var dic = pp?.GetValue(table);
                var dataDic = dic as IDictionary;
                PlotDataManager.Stuff.Add2CurrentPlotManagerData(dataDic);
            }

            if (n.StartsWith("StoryChat") && n.EndsWith("Table"))
            {
                var val = p.GetValue(null);
                var tt = val.GetType();
                var field = tt.GetField("table");
                var table = field.GetValue(val);
                var ttt = table.GetType();
                var pp = ttt.GetProperty("Dics");
                var dic = pp?.GetValue(table);
                var dataDic = dic as IDictionary;
                PlotDataManager.Stuff.Add2CurrentPlotChatData(dataDic);
            }

            if (n.StartsWith("StoryChatChoice") && n.EndsWith("Table"))
            {
                var val = p.GetValue(null);
                var tt = val.GetType();
                var field = tt.GetField("table");
                var table = field.GetValue(val);
                var ttt = table.GetType();
                var pp = ttt.GetProperty("Dics");
                var dic = pp?.GetValue(table);
                var dataDic = dic as IDictionary;
                PlotDataManager.Stuff.Add2CurrentPlotChatChoiceData(dataDic);
            }
        }
    }
}