using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;
using System;
using System.Text;
using CustomLitJson;

[ConsoleCommands]
public class GameCommands
{
    public static void SetRenderScale(string vString)
    {
        var v = float.Parse(vString);
        v = Mathf.Clamp(v, 0.1f, 2);
        var urpAsset = GraphicsSettings.renderPipelineAsset as UniversalRenderPipelineAsset;
        urpAsset.renderScale = v;

        MainConsoleWind.ShowStringToCmd($"Universal Renderer Pipline Asset RenderScale has been set to : {v}",
            Color.green);
    }

    public static void block(string type)
    {
        BlockLevel? level = null;
        if(type == "none")
        {
            BlockManager.Stuff.RemoveBlock("test");
        }
        else if(type == "invisible")
        {
            BlockManager.Stuff.AddBlock("test", BlockLevel.Invisible);
        }
        else if (type == "visible")
        {
            BlockManager.Stuff.AddBlock("test", BlockLevel.Visible);
        }
        else if (type == "transaction")
        {
            BlockManager.Stuff.AddBlock("test", BlockLevel.Transaction);
        }

    }

    public static void CloseAutoRenderScale()
    {
        //urpAsset = GraphicsSettings.renderPipelineAsset as UniversalRenderPipelineAsset;
        if (AutoRenderScale.Ins.urpAsset == null)
        {
            Debug.LogError("AutoRenderScale.Ins.urpAsset is null");
            return;
        }

        AutoRenderScale.Ins.urpAsset.renderScale = 1;
        AutoRenderScale.Ins.autoScale = false;
        MainConsoleWind.ShowStringToCmd($"CloseAutoRenderScale suc!", Color.green);
    }

    public static void OpenAutoRenderScale(string minScale)
    {
        var v = float.Parse(minScale);
        v = Mathf.Clamp(v, 0.1f, 1);
        AutoRenderScale.Ins.minScale = v;
        AutoRenderScale.Ins.autoScale = true;
        MainConsoleWind.ShowStringToCmd($"OpenAutoRenderScale min scale : {v}", Color.green);
    }

    public static string Lang_Help = "切换语言";
    public static List<string> Lang_Alias = new List<string>() {"lang"};


    public static void chat(string msg)
    {
        GameConversationManager.Stuff.worldConversation.SendText(msg);
    }

    public static void invite()
    {
        GameConversationManager.Stuff.worldConversation.SendInvite(1, "111");
    }

    public static void Toast(string msg)
    {
        ToastManager.Show(msg);
    }

    public static void BackTo(string pageName)
    {
        UIEngine.Stuff.BackTo<LoginPage>(pageName);
    }

    public static async void TestConversation()
    {
        await UiUtil.ConversationAsync(10101);
    }

    public static void test()
    {
        UIEngine.Stuff.Forward<TestPage>();
    }

    public async static void sendDebugMail()
    {
        var timestamp = Clock.TimestampSec;
        await MailApi.DebugSend(2001, "测试邮件", "10005,100,10006,500", "这是内容", timestamp);
    }

    public static async void OpenPlot(string plotEventId)
    {
        var intValue = int.Parse(plotEventId);
        await PlotPipelineManager.Stuff.PlayPlotAsync(intValue);
    }

    public static async void OpenPlotSkip(string plotEventId)
    {
        var intValue = int.Parse(plotEventId);
        await PlotPipelineManager.Stuff.PlayPlotAsync(intValue, true);
    }

    public static async void PlotReview(string stageId)
    {
        await PlotPipelineManager.Stuff.StartPipelineReviewAsync(int.Parse(stageId));
    }

    public static async void SystemOpenDebugOn()
    {
        SystemOpenManager.Stuff.openDebug = true;
    }

    public static async void SystemOpenDebugOff()
    {
        SystemOpenManager.Stuff.openDebug = false;
    }

    public static async void syson()
    {
        SystemOpenManager.Stuff.openDebug = true;
    }

    public static async void sysoff()
    {
        SystemOpenManager.Stuff.openDebug = false;
    }

    public static async void timeoff()
    {
        PlotPipelineManager.Stuff.OpenRemainTime = false;
    }

    public static async void timeon()
    {
        PlotPipelineManager.Stuff.OpenRemainTime = true;
    }

    // 关闭编队：一键上阵 & 一键下阵
    public static void tmoff()
    {
        FormationOperateManager.openAutoFormation = false;
    }

    // 开启编队：一键上阵 & 一键下阵
    public static void tmon()
    {
        FormationOperateManager.openAutoFormation = true;
    }

    public static async void OpeningGameMovie()
    {
        UIEngine.Stuff.ForwardOrBackTo<MovieSubtitlePage>(1);
    }


    /// <summary>
    /// 显示或设置 fps
    /// </summary>
    /// <param name="value"></param>
    public static void fps(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            var fps = Application.targetFrameRate;
            MainConsoleWind.ShowStringToCmd(fps.ToString(), Color.green);
        }
        else
        {
            var v = int.Parse(value);
            Application.targetFrameRate = v;
        }
    }

    /// <summary>
    /// 调用网络接口
    /// </summary>
    /// <param name="path"></param>
    public static async void api(string path)
    {
        await NetworkManager.Stuff.CallAsync(ServerType.Game, path);
    }

    public static async void lang(string lang)
    {
        LanguageManager.Language = lang;
        await LocalizationUtil.ProcessAsync();
        MainConsoleWind.ShowStringToCmd($"Set language to : {lang}", Color.green);
    }

    public static void user()
    {
        GPC.Helper.Jingwei.Script.Account.AccountUIHelper.SharedInstance().ShowProfileManagementPanel();
    }

    public static void tsh()
    {
        IggSdkManager.Stuff.TshUnreadCount.Value = 2;
    }

    public static void noti()
    {
        var now = DateTime.Now;
        var dateTime = now.AddMinutes(1);
        LocalNotification.Set(1, "title", "text", dateTime);
    }

    public static async void fh()
    {
        await OrderUtil.TrySubmitAllOrderIfNeedAsync();
    }

    public static string attach_Help = "查看物品 attach";

    public static void attach(string itemInstanceId)
    {
        var info = Database.Stuff.itemDatabase.GetItemInfoByInstanceId(itemInstanceId);
        var attach = info.attach;
        var json = CustomLitJson.JsonMapper.Instance.ToJson(attach);
        MainConsoleWind.ShowStringToCmd($"{json}", Color.green);
    }



    public static string sd_Help = "s(datic)d(ata) 搜索指定 rowId 的静态数据信息";

    public static void sd(string rowId)
    {
        var b = int.TryParse(rowId, out var intId);
        if (!b)
        {
            MainConsoleWind.ShowStringToCmd($"id must be a int", Color.red);
        }

        var request = new SearchRequest();
        request.id = intId;
        //request.needField = needPropertyName;
        var info = StaticDataRuntime.SearchOrGetFormCache(request);
        if (info.row == null)
        {
            MainConsoleWind.ShowStringToCmd($"not found", Color.green);
        }
        else
        {
            var row = info.row;
            var tableName = info.tableName;
            var rowJson = CustomLitJson.JsonMapper.Instance.ToJson(row);
            var tableItype = StaticDataRuntime.GetMetadata(tableName, "itype");
            var msg = "";
            if (tableItype != "")
            {
                msg = $"table: {tableName} (itype: {tableItype})\n{rowJson}\n";
            }
            else
            {
                msg = $"table: {tableName}\n{rowJson}\n";
            }

            var name = StaticDataUtil.GetAnyFieldOfAnyRow<string>(intId, "Name").Localize();
            msg += $"name: {name}\n";

            MainConsoleWind.ShowStringToCmd(msg, Color.green);
        }
    }

    public static string env_Help = "打印环境信息";

    public static void env()
    {
        var env = EnvManager.OriginEnv;
        var sb = new StringBuilder();
        sb.Append($"env: {env}");
        var isConnected = Remote.Stuff.isConnected;
        if (isConnected)
        {
            var subEnv = Remote.Stuff.subEnv;
            sb.Append($".{subEnv}");
        }
        else
        {
            sb.Append($" (not connected)");
        }

        sb.AppendLine();
        var localConfig = EnvManager.LocalConfig;
        var localJson = JsonMapper.Instance.ToJson(localConfig);
        sb.AppendLine("local: \n" + localJson);
        if (isConnected)
        {
            var remoteConfig = EnvManager.RemoteConfig;
            var remoteJson = JsonMapper.Instance.ToJson(remoteConfig);
            sb.AppendLine("remote: \n" + remoteJson);
        }

        MainConsoleWind.ShowStringToCmd(sb.ToString(), Color.green);
    }

    public static async void connect()
    {
        await SocketManager.Stuff.ConnectAsync(EndpointManager.SocketEndpoint);
    }

    public static void item(string id)
    {
        var intId = int.Parse(id);
        var info = Database.Stuff.itemDatabase.GetFirstItemInfoOfRowId(intId);
        if(info == null)
        {
            MainConsoleWind.ShowStringToCmd("not hold", Color.green);
            return;
        }
        var attach = info.attach;
        var json = JsonMapper.Instance.ToJson(attach);
        MainConsoleWind.ShowStringToCmd("attach: " + json, Color.green);

    }

    public static void rate()
    {
        IggSdkManager.Stuff.GameRating();
    }

    public static async void notice()
    {
        //SdkNoticeManager.Stuff.FetchInBackground();
    }

    public static void arena()
    {
        UIEngine.Stuff.ForwardOrBackTo<ArenaPageV2>();
    }
}