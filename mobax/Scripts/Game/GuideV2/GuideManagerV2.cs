using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using CustomLitJson;
using System.Threading.Tasks;
using System.Text;
using System;
using System.Linq;

public class GuideManagerV2 : StuffObject<GuideManagerV2>
{
    public bool forceGuideEnabled = true;

    // 超过步骤直接就可以开启跳过了
    public int exceptionTouchStep = 10;

    // 记录当前点击屏蔽的
    public int curTouchStep = 0;

    TriggerEngine triggerEngine;

    void Awake()
    {
        this.triggerEngine = TriggerEngine.Create(this.transform);
        triggerEngine.OnExecuteCompelte += OnTriggerCompelte;
        NetworkManager.addExtraParamHandler += OnNetworkAddExtraParam;
    }

    void OnNetworkAddExtraParam(NetworkRoutine routing)
    {
        // 如果在强制引导中，如果有任何关键网路访问，强行附带那一步已完成
        var inForceGuide = this.IsExecutingForceGuide;
        var inTriggerdGuide = this.IsExecutingTriggredGuide;
        if (inForceGuide || inTriggerdGuide)
        {
            var api = routing.url;
            var needWriteGuide = NeedWriteGuide(api);
            if (needWriteGuide)
            {
                // 设置本地数据
                var id = this.ExecutingForceGuideId;

                if (inForceGuide)
                {
                    SetGuideValueToData("force", id, false);
                }

                if (inTriggerdGuide)
                {
                    var guideId = this.ExecutingTriggerdGuideId;
                    if (guideId != "guildGuide" && guideId != "guildGuideStart")
                    {
                        SetGuideValueToData(guideId, 1, false);
                    }
                }

                //Database.Stuff.roleDatabase.Me.SetGuide("force", id);

                // 通知服务器设置新手引导数据
                AddGuideParam(routing);
            }
        }
    }

    void AddGuideParam(NetworkRoutine routing)
    {
        var data = Database.Stuff.roleDatabase.Me.guide;
        if (data == null)
        {
            return;
        }

        var sb = new StringBuilder();
        var first = true;
        foreach (var kv in data)
        {
            if (first)
            {
                first = false;
            }
            else
            {
                sb.Append(",");
            }

            var key = kv.Key;
            var value = kv.Value;
            sb.Append(key);
            sb.Append(",");
            sb.Append(value);
        }

        var magicString = sb.ToString();
        routing.urlParam["guide"] = magicString;
    }

    bool NeedWriteGuide(string api)
    {
        if (api.EndsWith("battle/result"))
        {
            return true;
        }

        if (api.EndsWith("gacha/single"))
        {
            return true;
        }

        if (api.EndsWith("gacha/dozen"))
        {
            return true;
        }

        return false;
    }

    public static Action forceGuideOver;

    void OnTriggerCompelte(bool success, Trigger e)
    {
        if (success)
        {
            if (e.id == this.ExecutingForceGuideId.ToString())
            {
                if (forceGuideEnabled)
                {
                    // 是强制引导
                    this.SetGuideValueToData("force", this.ExecutingForceGuideId, true);
                    this.TryExecuteForceGuideByData();
                    if (this.ExecutingForceGuideId == 0)
                    {
                        forceGuideOver?.Invoke();
                    }
                }
            }
            else
            {
                // 是触发引导
                var guideId = e.id;
                // 这步不需要存
                if (guideId != "guildGuideStart")
                {
                    this.SetGuideValueToData(guideId, 1, true);
                }
            }
        }
    }

    Bucket Bucket
    {
        get { return BucketManager.Stuff.GetBucket("guideV2"); }
    }


    [ShowInInspector]
    public Dictionary<string, int> RoleGuideData
    {
        get
        {
            var ret = Database.Stuff.roleDatabase.Me?.guide;
            return ret;
        }
    }

    [ShowInInspector, ReadOnly] List<int> forceGuideIdList = new List<int>();

    public async Task LoadForceGuideScript()
    {
        var asset = await this.Bucket.GetOrAquireAsync<TextAsset>("force.txt");
        var script = asset.text;
        triggerEngine.Load(script, (List<Trigger> list) =>
        {
            forceGuideIdList.Clear();
            foreach (var trigger in list)
            {
                trigger.enable = false;
                trigger.when = null;
                trigger.headDic["force"] = ResolveResult.Create(true);
                var id = trigger.id;
                var b = int.TryParse(id, out var num);
                if (b)
                {
                    forceGuideIdList.Add(num);
                }
            }

            forceGuideIdList.Sort();
        });
    }

    public bool IsExecutingForceGuide
    {
        get
        {
            if (this.ExecutingForceGuideId == 0)
            {
                return false;
            }

            return true;
        }
    }


    [ShowInInspector, ReadOnly] int _executingForceGuideId;

    /// <summary>
    /// 0 表示没有执行引导
    /// </summary>
    public int ExecutingForceGuideId
    {
        get { return _executingForceGuideId; }
        set
        {
            if (value == _executingForceGuideId)
            {
                return;
            }

            _executingForceGuideId = value;
            if (ExecutingForceGuideId == 0)
            {
                return;
            }

            var id = value.ToString();
            this.triggerEngine.ForceExecute(id);
        }
    }

    [ShowInInspector]
    // 如果没有正在执行的步骤，设置当前启用的触发
    public void TryExecute(string guideName)
    {
        var intValue = TryParseInt(guideName, 0);
        if (this.ExecutingForceGuideId == 0)
        {
            var has = this.forceGuideIdList.Contains(intValue);
            if (has)
            {
                this.ExecutingForceGuideId = intValue;
                return;
            }
        }

        if (this.ExecutingTriggerdGuideId == null)
        {
            var has = this.triggerdGuideIdList.Contains(guideName);
            if (has)
            {
                this.triggerEngine.ForceExecute(guideName);
                return;
            }
        }
    }

    public static int TryParseInt(string text, int defaultValue)
    {
        var b = int.TryParse(text, out var value);
        if (!b)
        {
            return defaultValue;
        }

        return value;
    }


    public bool IsAnyGuideExecuting
    {
        get
        {
            if (this.IsExecutingForceGuide)
            {
                return true;
            }
            else if (this.IsExecutingTriggredGuide)
            {
                return true;
            }

            return false;
        }
    }

    /// <summary>
    /// 当没有要执行的步骤时，返回0
    /// </summary>
    public int NextForceGuideId
    {
        get
        {
            var isForceGuideEnabled = this.forceGuideEnabled;
            if (!isForceGuideEnabled)
            {
                return 0;
            }

            var gs = Database.Stuff.roleDatabase.Me.GetGuide("force", -1);
            if (gs == -1)
            {
                var ret = ListUtil.TryGet(forceGuideIdList, 0, 0);
                return ret;
            }
            else
            {
                var index = this.forceGuideIdList.IndexOf(gs);
                if (index == -1)
                {
                    // 找不到上一次完成的触发
                    return 0;
                }

                index++;
                if (index >= this.forceGuideIdList.Count)
                {
                    // 后面没有了
                    return 0;
                }

                var nextId = this.forceGuideIdList[index];
                return nextId;
            }
        }
    }

    /// <summary>
    /// 根据自己角色上的步骤信息，启用接下来一步的触发
    /// </summary>
    public void TryExecuteForceGuideByData()
    {
        var nextId = this.NextForceGuideId;
        this.ExecutingForceGuideId = nextId;
    }

    /// <summary>
    /// 将当前的步骤在数据层里标记为已完成
    /// </summary>
    /// <param name="requestServer"></param>
    public async void SetGuideValueToData(string key, int value, bool alsoSendToServer)
    {
        Database.Stuff.roleDatabase.Me.SetGuide(key, value);
        if (alsoSendToServer)
        {
            var guideDataDic = Database.Stuff.roleDatabase.Me.guide;
            var guideDataJd = JsonUtil.ToJsonData(guideDataDic);
            var arg = new JsonData();
            arg["guide"] = guideDataJd;
            await NetworkManager.Stuff.CallAsync(ServerType.Game, "guide", arg);
        }
    }

    /// <summary>
    /// Todo 临时处理， 设置所有guide完成
    /// 设置guide完成
    /// </summary>
    public async Task SetGuildGuideDone()
    {
        var guideMap = Database.Stuff.roleDatabase.Me.guide ?? new Dictionary<string, int>();
        // 公会引导
        guideMap["guildGuide"] = 1;
        guideMap["guildGuideStart"] = 1;
        guideMap["guildGuideConfirm"] = 1;
        guideMap["GuildGuideCompleted"] = 1;
        guideMap["MainPageHelp"] = 1;
        // 其他
        guideMap["shengji"] = 1;
        guideMap["dispatch"] = 1;
        guideMap["ziyuanguan"] = 1;
        // 强制引导到最后一步
        guideMap["force"] = 111;
        Database.Stuff.roleDatabase.Me.guide = guideMap;

        var guideDataJd = JsonUtil.ToJsonData(guideMap);
        var arg = new JsonData
        {
            ["guide"] = guideDataJd
        };
        await NetworkManager.Stuff.CallAsync(ServerType.Game, "guide", arg);
    }

    public async Task CheckNeedResetGuide()
    {
        DateTime regTime = Clock.ToDateTime(Database.Stuff.roleDatabase.Me.logon);
        DateTime dueTime = new DateTime(2023, 4, 6, 21, 0, 0);
        if (regTime >= dueTime) return;

        var isNeedReset = Database.Stuff.roleDatabase.Me.GetGuide("force", -1) < 111;
        if (!isNeedReset && GuildManager.Stuff.IsGuideCompleted()) return;
        await GuideManagerV2.Stuff.SetGuildGuideDone();
    }

    public async Task SetAllGuideValueToDataAndBackToMainPage()
    {
        this.forceGuideEnabled = false;
        this.ExecutingForceGuideId = 0;
        Database.Stuff.roleDatabase.Me.SetGuide("force", this.forceGuideIdList.Last());
        foreach (var guideId in this.forceGuideIdList)
        {
            Database.Stuff.roleDatabase.Me.SetGuide($"{guideId}", 1);
        }

        var guideDataDic = Database.Stuff.roleDatabase.Me.guide;
        var guideDataJd = JsonUtil.ToJsonData(guideDataDic);
        var arg = new JsonData();
        arg["guide"] = guideDataJd;
        await NetworkManager.Stuff.CallAsync(ServerType.Game, "guide", arg);

        UIEngine.Stuff.RemoveFloating<GuideFloating>();
        UIEngine.Stuff.RemoveFloating<GuideBlockFloating>();
        var top = UIEngine.Stuff.Top;
        // if (top.container.pageName == nameof(MainPage)) return;
        await UIEngine.Stuff.ForwardOrBackToAsync<MainPage>();
    }

    public async Task SetAllGuideValueToDataAndNotBackToMainPage()
    {
        this.forceGuideEnabled = false;
        this.ExecutingForceGuideId = 0;
        Database.Stuff.roleDatabase.Me.SetGuide("force", this.forceGuideIdList.Last());
        foreach (var guideId in this.forceGuideIdList)
        {
            Database.Stuff.roleDatabase.Me.SetGuide($"{guideId}", 1);
        }

        var guideDataDic = Database.Stuff.roleDatabase.Me.guide;
        var guideDataJd = JsonUtil.ToJsonData(guideDataDic);
        var arg = new JsonData();
        arg["guide"] = guideDataJd;
        await NetworkManager.Stuff.CallAsync(ServerType.Game, "guide", arg);

        UIEngine.Stuff.RemoveFloating<GuideFloating>();
        UIEngine.Stuff.RemoveFloating<GuideBlockFloating>();
    }

    /// <summary>
    /// 功能引导的exception只会把步骤给存下来而已
    /// </summary>
    public async Task SetTriggerGuideValueToDataAndBackToMainPage()
    {
        this.SetTriggerGuideCompleteToData();

        UIEngine.Stuff.RemoveFloating<GuideFloating>();
        UIEngine.Stuff.RemoveFloating<GuideBlockFloating>();
        await UIEngine.Stuff.ForwardOrBackToAsync<MainPage>();

        // 处理一下如果是公会引导那么直接存储且把奖励派发
        await this.SubmitGuildReward();
    }

    /// <summary>
    /// 功能引导的exception只会把步骤给存下来而已
    /// </summary>
    public async Task SetTriggerGuideValueToDataAndNotBackToMainPage(string guideId)
    {
        this.SetTriggerGuideCompleteToData(guideId);
        await this.SubmitGuildReward(guideId);

        UIEngine.Stuff.RemoveFloating<GuideFloating>();
        UIEngine.Stuff.RemoveFloating<GuideBlockFloating>();
        // var top = UIEngine.Stuff.Top;
        // if (top.container.pageName == nameof(MainPage)) return;
        // await UIEngine.Stuff.ForwardOrBackToAsync<MainPage>();
    }

    public async Task SetTriggerGuideValueToDataAndNotBackToMainPage()
    {
        this.SetTriggerGuideCompleteToData();
        await this.SubmitGuildReward();

        UIEngine.Stuff.RemoveFloating<GuideFloating>();
        UIEngine.Stuff.RemoveFloating<GuideBlockFloating>();
        // var top = UIEngine.Stuff.Top;
        // if (top.container.pageName == nameof(MainPage)) return;
        // await UIEngine.Stuff.ForwardOrBackToAsync<MainPage>();
    }

    async Task SubmitGuildReward()
    {
        var guideId = this.ExecutingTriggerdGuideId;
        if (guideId == null || guideId != "guildGuide") return;

        await GuildManager.Stuff.SubmitGuildGuideWithNoRewardTipAsync();
    }

    async Task SubmitGuildReward(string guideId)
    {
        if (guideId == null || guideId != "guildGuide") return;

        await GuildManager.Stuff.SubmitGuildGuideWithNoRewardTipAsync();
    }

    public async Task SetGuideValueToDataAndBackToMainPage()
    {
        if (GuideManagerV2.Stuff.IsExecutingForceGuide)
        {
            await GuideManagerV2.Stuff.SetAllGuideValueToDataAndBackToMainPage();
        }
        else if (GuideManagerV2.Stuff.IsExecutingTriggredGuide)
        {
            await GuideManagerV2.Stuff.SetTriggerGuideValueToDataAndBackToMainPage();
        }
    }

    public async Task SetGuideValueToDataAndNotBackToMainPage()
    {
        if (GuideManagerV2.Stuff.IsExecutingForceGuide)
        {
            await GuideManagerV2.Stuff.SetAllGuideValueToDataAndNotBackToMainPage();
        }
        else if (GuideManagerV2.Stuff.IsExecutingTriggredGuide)
        {
            await GuideManagerV2.Stuff.SetTriggerGuideValueToDataAndNotBackToMainPage();
        }
    }

    public async Task SetGuideValueToDataAndNotBackToMainPage(string guideId)
    {
        if (GuideManagerV2.Stuff.IsExecutingForceGuide)
        {
            await GuideManagerV2.Stuff.SetAllGuideValueToDataAndNotBackToMainPage();
        }
        else if (GuideManagerV2.Stuff.IsExecutingTriggredGuide)
        {
            await GuideManagerV2.Stuff.SetTriggerGuideValueToDataAndNotBackToMainPage(guideId);
        }
    }

    public void Notify(string msg)
    {
        this.triggerEngine.Notify(msg);
    }

    public bool IsTriggerEngineEnbaled
    {
        get { return this.triggerEngine.IsOn; }
        set { this.triggerEngine.IsOn = value; }
    }


    [ShowInInspector, ReadOnly, PropertyOrder(100)]
    List<string> triggerdGuideIdList = new List<string>();

    public async Task LoadTriggerdGuideScript()
    {
        var asset = await this.Bucket.GetOrAquireAsync<TextAsset>("triggered.txt");
        var script = asset.text;
        triggerEngine.Load(script, (List<Trigger> list) =>
        {
            triggerdGuideIdList.Clear();
            foreach (var trigger in list)
            {
                var id = trigger.id;
                if (string.IsNullOrEmpty(id))
                {
                    continue;
                }

                triggerdGuideIdList.Add(id);
                if (id == "force")
                {
                    throw new System.Exception("[GuideManager] trigger id cannot be 'force'.");
                }
            }
        });
    }

    [ShowInInspector, PropertyOrder(101)]
    public string ExecutingTriggerdGuideId
    {
        get
        {
            var list = this.triggerEngine.executingIdList;
            if (list == null)
            {
                return null;
            }

            if (list.Count == 0)
            {
                return null;
            }

            var id = list[0];
            if (this.triggerdGuideIdList.Contains(id))
            {
                return id;
            }

            return null;
        }
    }

    public bool IsExecutingTriggredGuide
    {
        get
        {
            var id = this.ExecutingTriggerdGuideId;
            if (id != null)
            {
                return true;
            }

            return false;
        }
    }

    // 仅仅只是为了设置trigger guide完成
    public void SetTriggerGuideCompleteToData()
    {
        var guideId = this.ExecutingTriggerdGuideId;
        if (guideId == null) return;
        this.SetGuideValueToData(guideId, 1, true);
    }

    public void SetTriggerGuideCompleteToData(string guideId)
    {
        this.SetGuideValueToData(guideId, 1, true);
    }

    public void SetForceStepGuideCompleteToData()
    {
        var guideId = this.ExecutingForceGuideId;
        Database.Stuff.roleDatabase.Me.SetGuide("force", guideId);
        this.SetGuideValueToData($"{guideId}", 1, true);
    }

    /// <summary>
    /// 根据角色数据设置各个功能引导是否开启
    /// </summary>
    public void AutoSetTriggeredGuideEnableByRoleData()
    {
        var roleData = Database.Stuff.roleDatabase.Me;
        foreach (var id in this.triggerdGuideIdList)
        {
            if (id == "guildGuideStart") continue;
            var alreadyPlayed = roleData.GetGuide(id, 0) == 1;
            if (alreadyPlayed)
            {
                this.triggerEngine.TrySetTriggerEnable(id, false);
            }
        }
    }

    [ShowIf(nameof(IsAnyGuideExecuting))]
    [ShowInInspector]
    public bool HasNeedGmFlag
    {
        get
        {
            var list = this.triggerEngine.executingIdList;
            foreach (var one in list)
            {
                var info = this.triggerEngine.GetEventById(one);
                var b = info.GetHeadValue("needGm", false);
                if (b)
                {
                    return true;
                }
            }

            return false;
        }
    }


    [ShowIf(nameof(IsAnyGuideExecuting))]
    [ShowInInspector]
    public bool HasNeedGmFollowFlag
    {
        get
        {
            var list = this.triggerEngine.executingIdList;
            foreach (var one in list)
            {
                var info = this.triggerEngine.GetEventById(one);
                var b = info.GetHeadValue("needGmFollow", false);
                if (b)
                {
                    return true;
                }
            }

            return false;
        }
    }
}