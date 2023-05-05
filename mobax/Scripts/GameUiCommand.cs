using System.Collections;
using System.Collections.Generic;
using BattleEngine.Logic;
using UnityEngine;
using UnityEngine.Scripting;
using BattleSystem.ProjectCore;
using CustomLitJson;

[Preserve]
public static class GameUiCommand
{
    /// <summary>
    /// 跳过新手引导
    /// </summary>
    public static void Hierarchy()
    {
        UIEngine.Stuff.RemoveFloating<ConsoleFloating>();
        UIEngine.Stuff.ShowFloating<HierarchyFloating>();
    }

    private static string[] arr = {"movie_sea_monster_shelling", "movie_sea_monster_attack"};
    private static int movieIndex = 0;

    public static async void AttrCompare()
    {
        HeroInfo heroInfo = HeroDisplayer.Hero ?? HeroDisplayer.Default;
        var heroId = heroInfo.HeroId;
        var arg = new JsonData() {["id"] = heroId,};
        MainConsoleWind.ShowStringToCmd($"Id: {heroId}, Name: {LocalizationManager.Stuff.GetText(heroInfo.Name)}",
            Color.green);
        var serverHero = await NetworkManager.Stuff.CallAsync<BattleHero>(ServerType.Game, "debug/attr", arg);
        for (var i = 1; i < serverHero.Attr.Length; ++i)
        {
            var item = StaticData.HeroAttrTable.TryGet(i);
            var clientVal = heroInfo.GetAttribute((HeroAttr) item.Id);
            var serverVal = serverHero.Attr[item.Id];
            var color = clientVal == serverVal ? Color.green : Color.red;
            MainConsoleWind.ShowStringToCmd($"{item.Name} ->  本地: {clientVal}, 服务端: {serverVal}", color);
        }
    }
    
    public static async void TestMovie()
    {
        var movie = await UIEngine.Stuff.ShowFloatingAsync<SeaBattleMovieFloating>();
        if (movie is { }) movie.ChangeMovie($"{arr[movieIndex]}.mp4");

        movieIndex = (movieIndex + 1) % arr.Length;
    }

    public static void TestPuzzleGame()
    {
        UIEngine.Stuff.ForwardOrBackTo<PuzzleGamePage>(1);
    }

    public static void TestTransportGame()
    {
        UIEngine.Stuff.ForwardOrBackTo<TransportGamePage>(1);
    }

    public static async void TestEliminateGame()
    {
        UIEngine.Stuff.ForwardOrBackTo<DrawEliminationEntrance>();
    }

    public static async void AddGold(int count)
    {
        await DebugApi.AddItemAsync(ItemId.Gold, count);
        ToastManager.Show("success");
    }

    public static async void AddDiamond(int count)
    {
        await DebugApi.AddItemAsync(ItemId.FreeDiamond, count);
        ToastManager.Show("success");
    }

    public static async void AddItem(int id, int count)
    {
        await DebugApi.AddItemAsync(id, count);
        ToastManager.Show("success");
    }

    public static async void EnterLevel(int level)
    {
        BattleUtil.fast_enter = true;
        BattleUtil.EnterPveBattle(level);
    }

    /// <summary>
    /// 发放全英雄
    /// </summary>
    public static async void AllHero()
    {
        foreach (var heroRow in StaticData.HeroTable.ElementList)
        {
            if (heroRow.Special == 0 && heroRow.Show == 1)
            {
                await DebugApi.AddItemAsync(heroRow.Id, 1);
            }
        }

        ToastManager.Show("success");
    }

    /// <summary>
    /// 发送这个变成超级账号
    /// </summary>
    public static async void Superman()
    {
        // 所有英雄
        foreach (var heroRow in StaticData.HeroTable.ElementList)
        {
            if (heroRow.Special == 0 && heroRow.Show == 1)
            {
                await DebugApi.AddItemAsync(heroRow.Id, 100);
            }
        }

        // 服装换色等
        foreach (var cloth in StaticData.ClothColorTable.ElementList)
        {
            await DebugApi.AddItemAsync(cloth.Id, 1);
        }

        foreach (var hair in StaticData.HairColorTable.ElementList)
        {
            await DebugApi.AddItemAsync(hair.Id, 1);
        }

        foreach (var avatar in StaticData.AvatarTable.ElementList)
        {
            await DebugApi.AddItemAsync(avatar.Id, 1);
        }

        // 基础道具
        await DebugApi.AddItemAsync(10005, 1000000);
        await DebugApi.AddItemAsync(10006, 1000000);
        await DebugApi.AddItemAsync(10007, 1000000);
        await DebugApi.AddItemAsync(10008, 1000000);
        await DebugApi.AddItemAsync(20201, 1000000);
        await DebugApi.AddItemAsync(20202, 1000000);
        await DebugApi.AddItemAsync(20203, 1000000);

        // 公会相关
        await DebugApi.AddItemAsync(10020, 1000000);
        await DebugApi.AddItemAsync(20101, 1000);
        await DebugApi.AddItemAsync(20102, 1000);
        await DebugApi.AddItemAsync(20103, 1000);
        await DebugApi.AddItemAsync(20104, 1000);

        // 魔械
        await DebugApi.AddItemAsync(21501, 1000);
        await DebugApi.AddItemAsync(21502, 1000);
        await DebugApi.AddItemAsync(21503, 1000);
        await DebugApi.AddItemAsync(21511, 1000);
        await DebugApi.AddItemAsync(21301, 1000);
        await DebugApi.AddItemAsync(21302, 1000);
        await DebugApi.AddItemAsync(21303, 1000);
        await DebugApi.AddItemAsync(21401, 1000);
        await DebugApi.AddItemAsync(21411, 1000);
        await DebugApi.AddItemAsync(21421, 1000);
        await DebugApi.AddItemAsync(21431, 1000);
        await DebugApi.AddItemAsync(21441, 1000);
        await DebugApi.AddItemAsync(21504, 1000);
        await DebugApi.AddItemAsync(21505, 1000);
        await DebugApi.AddItemAsync(21506, 1000);
        await DebugApi.AddItemAsync(21507, 1000);
        await DebugApi.AddItemAsync(21508, 1000);
        await DebugApi.AddItemAsync(21509, 1000);

        // 魔械合成材料
        await DebugApi.AddItemAsync(20301, 1000);
        await DebugApi.AddItemAsync(20302, 1000);
        await DebugApi.AddItemAsync(20303, 1000);
        await DebugApi.AddItemAsync(20304, 1000);
        await DebugApi.AddItemAsync(20305, 1000);
        await DebugApi.AddItemAsync(20306, 1000);
        await DebugApi.AddItemAsync(20307, 1000);
        await DebugApi.AddItemAsync(20308, 1000);
        await DebugApi.AddItemAsync(20309, 1000);
        await DebugApi.AddItemAsync(20310, 1000);
        await DebugApi.AddItemAsync(20311, 1000);
        await DebugApi.AddItemAsync(20312, 1000);
        await DebugApi.AddItemAsync(20313, 1000);
        await DebugApi.AddItemAsync(20314, 1000);
        await DebugApi.AddItemAsync(20315, 1000);
        await DebugApi.AddItemAsync(20316, 1000);
        await DebugApi.AddItemAsync(20317, 1000);

        ToastManager.Show("success");
    }

    public static void DevPage()
    {
        UIEngine.Stuff.ForwardOrBackTo("TempMainPage");
    }

    public static void Stage999()
    {
        BattleUtil.EnterPveBattle(999);
    }

    public static void Stage998()
    {
        UIEngine.Stuff.ForwardOrBackTo("FormationPage", 998);
    }

    public static void Stage2000()
    {
        BattleUtil.EnterPveBattle(2000, EFormationIndex.Normal1);
    }

    public static async void plotComics()
    {
        await PlotPipelineManager.Stuff.PlayPlotAsync(23107);
    }

    public static async void ResetGuildGuide()
    {
        var guideMap = Database.Stuff.roleDatabase.Me.guide ?? new Dictionary<string, int>();
        // 公会引导
        guideMap["guildGuide"] = 0;
        // guideMap["guildGuideStart"] = 1;
        // guideMap["guildGuideConfirm"] = 1;
        // guideMap["GuildGuideCompleted"] = 1;
        guideMap["MainPageHelp"] = 1;
        // 其他
        guideMap["shengji"] = 1;
        guideMap["dispatch"] = 1;
        guideMap["ziyuanguan"] = 1;
        // 强制引导到最后一步
        guideMap["force"] = 111;
        guideMap["111"] = 1;
        Database.Stuff.roleDatabase.Me.guide = guideMap;

        var guideDataJd = JsonUtil.ToJsonData(guideMap);
        var arg = new JsonData
        {
            ["guide"] = guideDataJd
        };
        await NetworkManager.Stuff.CallAsync(ServerType.Game, "guide", arg);
    }

    public static void SeaDebugAuto()
    {
        SeaBattleDebug.Auto = !SeaBattleDebug.Auto;

        var openStr = SeaBattleDebug.Auto ? "打开" : "关闭";
        MainConsoleWind.ShowStringToCmd($"【海怪】Debug自动模式：{openStr}", Color.green);
    }

    public static void SeaDebugLazy()
    {
        SeaBattleDebug.Lazy = !SeaBattleDebug.Lazy;

        var openStr = SeaBattleDebug.Lazy ? "打开" : "关闭";
        MainConsoleWind.ShowStringToCmd($"【海怪】Debug不干活模式：{openStr}", Color.green);
    }
}