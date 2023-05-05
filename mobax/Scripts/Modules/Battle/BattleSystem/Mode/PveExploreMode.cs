using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BattleEngine.Logic;
using BattleSystem.Core;
using BattleSystem.ProjectCore;
using CustomLitJson;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 探索地区找猫猫
/// <para>禁止配置PVE地图模式</para>
/// </summary>
[BattleModeClass(BattleModeType.PveExplore)]
public class PveExploreMode : Mode
{
    //private AIUnit _heroAI;

    public override string GetSceneName()
    {
        var sceneName = "";
        switch (Battle.Instance.MapType)
        {
            // 禁止Pve模式
            case EPveMapType.Pve:
            case EPveMapType.MemoryMode:
                var battleMapId = Battle.Instance.PveBattleMapId;
                var battleMapRow = StaticData.BattleMapTable[battleMapId];
                sceneName = battleMapRow.unityScene;
                // sceneName = "Field";
                break;
            case EPveMapType.RogueLike:
            case EPveMapType.PveRogueLike:
                sceneName = "RoguelikeMap";
                break;
        }

        return sceneName;
    }

    public override string GetPageName()
    {
        return "BattlePage";
    }

    // 这里如果是roguelike模式则需要手动调用渲染完成
    public override Task OnSceneLoaded()
    {
        var tcs = new TaskCompletionSource<bool>();
        tcs.SetResult(true);
        if (Battle.Instance.MapType == EPveMapType.Pve || Battle.Instance.MapType == EPveMapType.MemoryMode)
            return tcs.Task;
        var scene = SceneManager.GetActiveScene();
        var roots = scene.GetRootGameObjects();
        var randomMapObj = Array.Find(roots, val => val.name.Equals("Parts"));
        if (randomMapObj == null)
        {
            Debug.LogError("加载pve肉鸽地图的时候出现了一个错误：RoguelikeMap场景内不存在Parts节点");
            return tcs.Task;
        }

        var randomMap = randomMapObj.GetComponent<RandomMap>();
        if (randomMap == null)
        {
            Debug.LogError("加载pve肉鸽地图的时候出现了一个错误：Parts节点上不存在脚本RandomMap");
            return tcs.Task;
        }

        randomMap.enabled = true;
        randomMap.Init();

        //return randomMap.BeginShowMap(stageInfo.CatInfo);
        return tcs.Task;
    }

    public override async Task OnPrechnagePage()
    {
        // var movieFLoating = await UIEngine.Stuff.ShowFloatingAsync("MovieFloating");
        //
        // var loadingPage = UIEngine.Stuff.FindPage<LoadingPage>();
        // loadingPage.gameObject.SetActive(false);
        // //AudioEngine.Stuff.PauseBgm();
        // AudioManager.PauseBgm();
        //
        // await (movieFLoating as MovieFloating).WaitResultAsync();
        //        Debug.LogError("MovieFloating loaded");
    }

    public override void OnUpdate()
    {
        /*if (this._heroAI == null) return;
        if (Input.GetMouseButtonDown(0))
        {
            if (Physics.Raycast(CameraManager.Instance.MainCamera.ScreenPointToRay(Input.mousePosition),
                out var hitInfo))
            {
                var data = BattleSubmitUtil.TakeInputObject();
                data.cmdType = CmdType.MoveTo;
                data._id = this._heroAI.uid;
                data.pos = hitInfo.point;
                BattleSubmitUtil.Submit(data);
            }
        }*/
    }

    public override async Task BattleReady()
    {
        Debug.LogError("先注释了，chenfei");
        /*var stageinfo = Battle.Instance.LevelInfo;

        battle.Wave = 1;
        int wave = 1;
        StageMonsterInfo info = StageBattleUtil.GetMonsterInfo(stageinfo.MonsterInfos, wave);
        if (info == null)
            info = StageBattleUtil.GetMonsterInfo(stageinfo.MonsterInfos, battle.Wave);
        var battleCore = this.battle.GetBattleComponent<ClientEngine>().battleCore;
        var coreService = battleCore.CoreService;
        MapUtil.RefreshMapTrigger(true);

        HeroFormationInfoWave stage = StageBattleUtil.GetFormationInfoByWave(stageinfo.formationInfos, battle.Wave);

        var heroInfo = stage.MainHeroInfoList.First();
        var dir = Quaternion.AngleAxis(heroInfo.HeroPoint.GetRotationY(), Vector3.up) * Vector3.forward;
        var pos = heroInfo.HeroPoint.pos;
        this._heroAI = coreService.CreateHero(heroInfo.HeroId, pos, dir, true, null,
            heroInfo.HeroPoint.heroType == StageHeroPoint.StageHeroType.Leader,
            heroInfo.HeroPoint.heroType == StageHeroPoint.StageHeroType.Sub,
            heroInfo.HeroPoint.heroType == StageHeroPoint.StageHeroType.Main, heroInfo.HeroPoint.index);
        this._heroAI.RoleData.IsAssitant = heroInfo.HeroPoint.heroType == StageHeroPoint.StageHeroType.Leader;
        this._heroAI.FightLock = true;
        this._heroAI.Slot = heroInfo.HeroPoint.index;
        this._heroAI.EnableBehaviourTree(false);

        coreService.storage.PlayerId = "hero";*/
        //battle.MoveToNext();
    }

    public async override Task BattleResult(bool isWin, List<BattleEngine.Logic.BattlePlayerRecord> myTeamRecrod,
        List<BattleEngine.Logic.BattlePlayerRecord> enemyTeamRecord, float duration)
    {
        var arg = new JsonData()
        {
            ["kill"] = BattleLogicManager.Instance.BattleData.KillNum,
            ["time"] = Mathf.FloorToInt(BattleTimeManager.Instance.CurrentBattleTime)
        };
        var battleInfo = new BattleInfo
        {
            win = (int) BattleStateManager.Instance.GameResult,
            id = Battle.Instance.CopyId,
            corps = Battle.Instance.FormationIndex, //TODO 编队 
            data = arg
        };
        StageManager.Stuff.ReqStageResult(battleInfo);
    }

    // public override async Task<bool> PreloadRes(BattleSystem.ProjectCore.BattleCoreParam param,
    //     Action<float> onProgress)
    // {
    //     var heroIdList = new List<int>();
    //     var heroIdArray = BattleDataManager.Instance.GetModels();
    //     heroIdList.AddRange(heroIdArray);
    //     onProgress.Invoke(0);
    //     await BattlePreloadUtil.PreLoadModel(heroIdList);
    //     onProgress.Invoke(0.6f);
    //     await BattlePreloadUtil.PreLoadSkillRes(heroIdList);
    //     onProgress.Invoke(0.9f);
    //     Debug.Log("complete!!");
    //     return true;
    // }
}