using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading.Tasks;
using BattleEngine.Logic;
using BattleSystem.ProjectCore;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;
using nobnak.Gist.ObjectExt;

public class Mode
{
    public Battle battle;
    public BattlePlayer AssistBattleInfo;
    public CreateBattleResponse Response;

    public float fPowerParam = 1f;

    public virtual BattleModeType ModeType
    {
        get { return BattleModeType.PveMode; }
    }

    private float _maxDistance = 1f;

    public float MaxDistance
    {
        get { return _maxDistance;}
        set
        {
            _maxDistance = value;
            _maxDistance = Mathf.Max(1, _maxDistance);
        }
    }

    public Vector3 LinePos { get; set; }

    public virtual void OnStart()
    {

    }

    public virtual void OnCreate(PveModeParam pveParam, object modeParam, Battle battle, CreateBattleResponse response)
    {
        this.battle = battle;
        Response = response;
    }

    public virtual string GetSceneName()
    {
        var pveModeParam = (PveModeParam)this.battle.param.pveParam;
        var sceneName = "";
        switch (pveModeParam.MapType)
        {
            case EPveMapType.Pve:
            case EPveMapType.MemoryMode:
                var battleMapId = pveModeParam.SceneId;
                var battleMapRow = StaticData.BattleMapTable[battleMapId];
                sceneName = battleMapRow.unityScene;
                // sceneName = "Field";
                break;
            case EPveMapType.RogueLike:
            case EPveMapType.PveRogueLike:
                var coreService = MapGenerateCore.Instance;
                sceneName = coreService.GetCurSceneName();
                break;
        }
        return sceneName;
    }

    public virtual string GetPageName()
    {
        return "BattlePage";
    }

    public virtual Task OnSceneLoaded()
    {
        var pveModeParam = (PveModeParam)this.battle.param.pveParam;
        var tcs = new TaskCompletionSource<bool>();
        tcs.SetResult(true);
        var scene = SceneManager.GetActiveScene();
        var roots = scene.GetRootGameObjects();
        if (pveModeParam.MapType == EPveMapType.Pve
            || pveModeParam.MapType == EPveMapType.MemoryMode)
        {
            var randomRoot = Array.Find(roots, val => val.GetComponent<RandomMap>() != null);
            if (randomRoot == null) return tcs.Task;
            var randomMapScript = randomRoot.GetComponent<RandomMap>();
            randomMapScript.InitMeshGrass();
            return tcs.Task;
        }
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
        return randomMap.BeginShowMap();
    }

    // 副本id
    public virtual int GetCopyId()
    {
        PveModeParam modeParam = (PveModeParam)this.battle.param.pveParam;
        return modeParam.CopyId;
    }

    // 战术道具获取
    public virtual List<int> GetBattleItems()
    {
        PveModeParam modeParam = (PveModeParam)this.battle.param.pveParam;
        return modeParam.Items;
    }

    // 获取波次信息
    public virtual List<MapWaveDataObject> GetWaveDataList()
    {
        if (battle.param.isBattleReport)
        {
            return battle.param.pveParam.WaveDataList;
        }
        PveModeParam modeParam = (PveModeParam)this.battle.param.pveParam;
        var stageRow = StaticData.StageTable.TryGet(this.GetCopyId());
        if (stageRow == null)
        {
            Debug.LogError("获取不到指定copyId =>" + this.GetCopyId());
            return new List<MapWaveDataObject>();
        }
        var stageType = (EPveMapType)stageRow.mapeType;
        if (stageType.Equals(EPveMapType.RogueLike))
        {
            var mapGenerator = MapGenerateCore.Instance;
            return mapGenerator.MapAgent.MapWaveDataList;
        }
        if (stageType.Equals(EPveMapType.MemoryMode))
        {
            return ModeUtil.ResetRandomMonster(modeParam);
        }
        return modeParam.WaveDataList;
    }
    

    public virtual void OnPlayBgm() { }

    public virtual Task OnPrechnagePage()
    {
        var tcs = new TaskCompletionSource<bool>();
        tcs.SetResult(true);
        return tcs.Task;
    }

    public virtual Task OnBattleDestroyed()
    {
        var tcs = new TaskCompletionSource<bool>();
        tcs.SetResult(true);
        return tcs.Task;
    }

    public virtual async Task<BattlePlayer> RequestPlayerAsync()
    {
        var data = await BattleApi.RequestPlayerAsync(Database.Stuff.roleDatabase.Me._id);
        return data;
    }

    public virtual void OnUpdate()
    {
        //throw new Exception("[BattleModeInitalizeProvider] get page name not implement in current battle mode");
    }

    public virtual async Task CreateHeros()
    {
        await Battle.Instance.SpawnHeros();
    }

    //战斗前的准备工作
    public virtual async Task BattleReady()
    {
        Debug.LogWarning("~~~~~~~~~~~~~~~~~~~~~~~~~");
    }

    //战斗结算
    public virtual Task BattleResult(bool isWin, List<BattleEngine.Logic.BattlePlayerRecord> myTeamRecrod, List<BattleEngine.Logic.BattlePlayerRecord> enemyTeamRecord, float duration)
    {
        var tcs = new TaskCompletionSource<bool>();
        tcs.SetResult(true);
        return tcs.Task;
    }

    /// <summary>
    /// 提供模式独特的资源预加载处理
    /// 如果不提供，将使用通用的预加载处理
    /// </summary>
    /// <returns>该模式对象是否已提供自己的处理</returns>
    public async virtual Task<bool> PreloadRes(BattleSystem.ProjectCore.BattleCoreParam param, Action<float> onProgress,List<int> otherHero = null)
    {
        var heroIdList = new List<int>();
        if (otherHero != null)
        {
            heroIdList.AddRange(otherHero);
        }
        else
        {
            var mainheroList = BattleDataManager.Instance.GetList(MapUnitType.MainHero);
            for (int i = 0; i < mainheroList.Count; i++)
            {
                if (mainheroList[i].Id == 0
                    || heroIdList.Contains(mainheroList[i].Id))
                {
                    continue;
                }
                heroIdList.Add(mainheroList[i].Id);
            }    
        }
        List<MapWaveDataObject> dataList = GetWaveDataList();
        for (int i = 0; i < dataList.Count; i++)
        {
            for (int j = 0; j < dataList[i].MonsterList.Count; j++)
            {
                if (dataList[i].MonsterList[j] == null
                    || dataList[i].MonsterList[j].Id == 0
                    || heroIdList.Contains(dataList[i].MonsterList[j].Id))
                {
                    continue;
                }
                heroIdList.Add(dataList[i].MonsterList[j].Id);
            }
        }
        onProgress.Invoke(0);
        await BattlePreloadUtil.PreLoadModel(heroIdList);
        onProgress.Invoke(0.3f);
        await BattlePreloadUtil.PreLoadSkillRes(heroIdList);
        onProgress.Invoke(0.6f);
        await BattlePreloadUtil.PreLoadSkillTimeLineRes(heroIdList);
        onProgress.Invoke(0.9f);
        Debug.Log("complete!!");
        return true;
    }

    /// <summary>
    /// 在创建 Battle 根游戏对象前
    /// 为什么需要这个生命周期?
    /// 
    /// 某些战斗模式需要在最开始就访问服务器的接口。
    /// 例如，竞技场模式需要先创建战斗，然后拉取敌人英雄数据
    /// </summary>
    public virtual Task OnPreCreateBattleGameObjectAsync()
    {
        var tcs = new TaskCompletionSource<bool>();
        tcs.SetResult(false);
        return tcs.Task;
    }

    //一波战斗开始调用
    public virtual Task OnWaveStart()
    {
        var tcs = new TaskCompletionSource<bool>();
        tcs.SetResult(false);
        return tcs.Task;
    }

    //一波战斗开始调用
    public virtual Task OnWaveExectue()
    {
        var tcs = new TaskCompletionSource<bool>();
        tcs.SetResult(false);
        return tcs.Task;
    }

    //一波战斗结束后调用
    public virtual Task OnWaveEnd()
    {
        var tcs = new TaskCompletionSource<bool>();
        tcs.SetResult(false);
        return tcs.Task;
    }

    public virtual void RegistCondition()
    {
        List<BattleResultCheckData> checkData = new List<BattleResultCheckData>();
        var stageinfo = StaticData.StageTable.TryGet(Battle.Instance.CopyId);
        if (stageinfo != null)
        {
            if (!string.IsNullOrEmpty(stageinfo.Condition))
            {
                string[] strs = stageinfo.Condition.Split(',');
                foreach (var VARIABLE in strs)
                {
                    string[] infos = VARIABLE.Split(':');
                    int type = Int32.Parse(infos[0]);
                    if (type == (int)BATTLE_RESULT_TYPE.DEFEND_TARGET)
                    {
                        var role = SceneObjectManager.Instance.FindCreatureByConfigID(Int32.Parse(infos[1]));
                        if (role != null)
                        {
                            checkData.Add(new BattleResultCheckData() { resultType = (BATTLE_RESULT_TYPE)type, Param = role.ID });
                        }
                    }
                    else if (type == (int)BATTLE_RESULT_TYPE.KILL_TARGET)
                    {
                        var role = SceneObjectManager.Instance.FindCreatureByConfigID(Int32.Parse(infos[1]));
                        if (role != null)
                        {
                            checkData.Add(new BattleResultCheckData() { resultType = (BATTLE_RESULT_TYPE)type, Param = role.ID });
                        }
                    }
                    else if (type == (int)BATTLE_RESULT_TYPE.DEFEND_TIME)
                    {
                        checkData.Add(new BattleResultCheckData() { resultType = BATTLE_RESULT_TYPE.DEFEND_TIME, Param = Int32.Parse(infos[1]) * 1000 });
                    }
                    else
                    {
                        checkData.Add(new BattleResultCheckData() { resultType = (BATTLE_RESULT_TYPE)(Int32.Parse((infos[0]))), Param = Int32.Parse(infos[1]) });
                    }
                }
            }
            checkData.Add(new BattleResultCheckData() { resultType = BATTLE_RESULT_TYPE.TEAM_DOOM, Param = BattleLogicManager.Instance.BattleData });
            checkData.Add(new BattleResultCheckData() { resultType = BATTLE_RESULT_TYPE.TIMEEND, Param = stageinfo.endTime * 1000 });
            checkData.Add(new BattleResultCheckData() { resultType = BATTLE_RESULT_TYPE.OUTCHECKRESULT, });
            BattleResultManager.Instance.CreateResultData(checkData);
        }
    }

    public virtual async Task SetReadyCamera()
    {
        if (Battle.Instance.Wave == 1)
        {
            //2.镜头切换
            Creature target = null;
            target = GetCenterRole();
            if (target == null)
            {
                Debug.LogError("------------------角色配置不对");
                return;
            }
            var dir = target.GetDirection();
            SceneObjectManager.Instance.LocalPlayerCamera.SetDirection(dir);
            CameraManager.Instance.TryChangeState(CameraState.Ready, GetCenterPosition());
           
            await Task.Delay(1000);
            if (target == null)
            {
                return;
            }
            SceneObjectManager.Instance.SetSelectPlayer(target);
            await ShowWeapon();
            if (target == null)
            {
                return;
            }
            //if(Battle.Instance.IsArenaMode)
            //SceneObjectManager.Instance.LocalPlayerCamera.transform.localPosition = new Vector3(0, 0, 2f);
            CameraManager.Instance.CameraProxy.SetTarget(target.transform, target, true);
            //SceneObjectManager.Instance.SetSelectPlayer(target);
            //BattleSubmitUtil.SubmitByNormalParam(CmdType.SelectRole, target.ID);
            if (Battle.Instance.IsArenaMode)
                CameraManager.Instance.TryChangeState(CameraState.Arena);
            else
            {
                CameraManager.Instance.TryChangeState(CameraState.Free2);
            }

            //CameraManager.Instance.CameraProxy.TurnImmediate();
            CameraManager.Instance.CameraProxy.OffsetSpeed = 3;

            //CameraManager.Instance.ShowTransitionBlack();
            RegistCondition();
        }

        //BattleLogicManager.Instance.ResetActorLogic(BattleEngine.Logic.BattleManager.Instance.mBattleData);
        //BattleManager.Instance.ActorMgr.OpenAI();
        BattleStateManager.Instance.ChangeState(eBattleState.Play);
    }

    public virtual async Task ShowWeapon(int delay = 1500)
    {
        if (Battle.Instance.Wave == 1)
        {
            List<Creature> roles = new List<Creature>();
            foreach (var VARIABLE in BattleEngine.Logic.BattleManager.Instance.ActorMgr.GetAllActors())
            {
                if (VARIABLE.mData.CampID == 0
                    && VARIABLE.mData.IsFighting)
                {
                    if (ModeType == BattleModeType.Guard
                        && VARIABLE.ConfigID == BattleConst.SiLaLiID)
                    {
                        continue;
                    }
                    VARIABLE.PlayAnim("pullarms");
                    VARIABLE.ShowWeapon();
                }
            }

            //Battle.Instance.OnWaveStart();
            await StageBattleUtil.TriggerStory(Battle.Instance.CopyId, EPlotEventType.StartBattle, null);
            GameEventCenter.Broadcast(GameEvent.ShowStartAnimtion);
            await Task.Delay(delay);
            if (!Battle.Instance.IsFight)
                return;
            Battle.Instance.BattleStarted = true;
            QualitySetting.TryAutoSetQualityByFrame();
        }
    }

    public async Task ShowPlot() { }

    public virtual async Task ShowSomething()
    {
        await Task.CompletedTask;
    }

    public Vector3 GetCenterPosition()
    {
        List<Creature> roles = new List<Creature>();
        foreach (var VARIABLE in BattleEngine.Logic.BattleManager.Instance.ActorMgr.GetCamp(0))
        {
            if (VARIABLE.mData.CampID == 0
                && VARIABLE.IsMain)
                roles.Add(VARIABLE);
        }
        Vector3 pos = Vector3.zero;
        foreach (var VARIABLE in roles)
        {
            pos += VARIABLE.GetPosition();
        }
        return pos / roles.Count;
    }

    public Creature GetCenterRole()
    {
        Vector3 mid = GetCenterPosition();
        float dis = 100000;
        Creature Role = null;
        foreach (var VARIABLE in BattleEngine.Logic.BattleManager.Instance.ActorMgr.GetCamp(0))
        {
            if (VARIABLE.mData.CampID == 0
                && VARIABLE.IsMain)
            {
                var temp_dis = Vector3.Distance(mid, VARIABLE.GetPosition());
                if (temp_dis < dis)
                {
                    Role = VARIABLE;
                    dis = temp_dis;
                }
            }
        }
        return Role;
    }

    public virtual async Task BattleEnd(bool win)
    {
        await Task.CompletedTask;
    }

    public virtual void SendZoneResult() { }

    public void SetStartCamera()
    {
        Creature target = null;
        target = GetCenterRole();
        var dir = target.GetDirection();
        var center = GetCenterPosition();
        SceneObjectManager.Instance.LocalPlayerCamera.SetDirection(dir);
        CameraManager.Instance.TryChangeState(CameraState.Ready, center);
    }

    public List<MapWaveDataObject> ReplaceMonsterInfo(List<MapWaveDataObject> mapWaveDataObjects, List<int> mosterGroups)
    {
        List<List<int>> monsterGroupID = new List<List<int>>();
        for (int i = 0; i < mosterGroups.Count; i++)
        {
            if (mosterGroups[i] == 0)
            {
                continue;
            }
            var monsterGroupRow = StaticData.PortMonsterTable.TryGet(mosterGroups[i]);
            List<int> groupList = new List<int>();
            if (monsterGroupRow.Boss != 0)
            {
                groupList.Add(monsterGroupRow.Boss);
            }
            if (monsterGroupRow.Monsters != null)
            {
                for (int j = 0; j < monsterGroupRow.Monsters.Length; j++)
                {
                    if (monsterGroupRow.Monsters[j] == 0)
                    {
                        continue;
                    }
                    groupList.Add(monsterGroupRow.Monsters[j]);
                }
            }
            if (groupList.Count == 0)
            {
                break;
            }
            monsterGroupID.Add(groupList);
        }
        List<MapWaveDataObject> newWaveDataObjects = new List<MapWaveDataObject>();
        for (int i = 0; i < monsterGroupID.Count; i++)
        {
            if (i >= mapWaveDataObjects.Count)
            {
                break;
            }
            MapWaveDataObject newData = new MapWaveDataObject();
            newData = mapWaveDataObjects[i].DeepCopy();
            newData.MonsterList.Clear();
            for (int j = 0; j < monsterGroupID[i].Count; j++)
            {
                if (j >= mapWaveDataObjects[i].MonsterList.Count)
                {
                    break;
                }
                MapWaveModelData newModeData = mapWaveDataObjects[i].MonsterList[j].DeepCopy();
                newModeData.Id = monsterGroupID[i][j];
                newData.MonsterList.Add(newModeData);
            }
            newWaveDataObjects.Add(newData);
        }
        return newWaveDataObjects;
    }

    /// <summary>
    /// 自己的玩家信息由战斗获取
    /// 当战斗获取到了自己的信息后
    /// </summary>
    public virtual async Task AfterLoadedPlayerInfoAsync() { }
}