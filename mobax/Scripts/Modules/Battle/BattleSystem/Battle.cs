using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;
using System;
using System.Linq;
using BattleSystem.ProjectCore;
using BattleEngine.Logic;
using BattleEngine.View;
using UnityEngine.ResourceManagement.ResourceProviders;
using BattleUtil = BattleEngine.Logic.BattleUtil;

/// <summary>
/// 战斗本身
/// 每场战斗都是不同实例
/// </summary>
public class Battle : Singleton<Battle>
{
    //public static Battle Instance;

    public bool isEnterCompelted;
    public Dictionary<Type, IBattleComponent> typeToBattleSingleInstanceDic = new Dictionary<Type, IBattleComponent>();
    public List<IBattleComponent> componentList = new List<IBattleComponent>();

    private GameObject Root;

    public bool bBreakItemUse = true;
    public bool bFocusItemUse = true;
    public bool bFriendItemUse = true;

    private string battlePageName = "";

    public bool isReportBattle = false;

    public bool BreakItemUse
    {
        get { return bBreakItemUse; }
        set
        {
            bBreakItemUse = value;
            PlayerPrefs.SetInt("BreakItemUse", bBreakItemUse ? 1 : 0);
        }
    }

    public bool FocusItemUse
    {
        get { return bFocusItemUse; }
        set
        {
            bFocusItemUse = value;
            PlayerPrefs.SetInt("FocusItemUse", bFocusItemUse ? 1 : 0);
        }
    }

    public bool FriendItemUse
    {
        get { return bFriendItemUse; }
        set
        {
            bFriendItemUse = value;
            PlayerPrefs.SetInt("FriendItemUse", bFriendItemUse ? 1 : 0);
        }
    }

    public List<int> ExtBuffIDs = new List<int>();

    public T GetBattleComponent<T>() where T : IBattleComponent
    {
        var type = typeof(T);
        var comp = typeToBattleSingleInstanceDic[type];
        return (T)comp;
    }

    public void TryRefreshBattleRender()
    {
        if (IsFight)
        {
            var actors = BattleEngine.Logic.BattleManager.Instance.ActorMgr.GetAllActors();
            foreach (var actor in actors)
            {
                actor.RoleRender.RefreshRender();
            }
        }
    }

    public bool IsFight { get; private set; }

    public BattleCoreParam param;

    /*public LevelInfo LevelInfo
    {
        get;
        set;
    }*/

    public Mode GameMode { get; set; }

#region ---[Obsolete]---
    public int FormationIndex { get; set; }
    public EPveMapType MapType { get; set; }
    public int SceneId { get; set; }
    public List<MapPartConfig> MapParts { get; set; }
    public List<EnvironmentPartConfig> EnvironmentParts { get; set; }
    public List<MapEffectBase> MapEffects { get; set; }
#endregion
    public int CopyId { get; set; }
    //public List<int> Items { get; set; }
    //public List<MapWaveDataObject> WaveDataList { get; set; }
    public int PveBattleMapId { get; set; }

    public bool BattleStarted { get; set; }

    /// <summary>
    /// 战斗场景启用addressable加载场景
    /// </summary>
    private SceneInstance _sceneInstance;

#region HeroData
    public BattlePlayer BattlePlayerData = null;

    //设置battlepalyer数据
    public void SetBattlePlayerData(BattlePlayer data)
    {
        BattlePlayerData = data;
    }

    private async Task GetBattlePlayerData()
    {
        BattlePlayerData = await GameMode.RequestPlayerAsync();
    }

    //获取单个英雄的数据
    public BattleHero GetHeroData(int ConfigID)
    {
        if (BattlePlayerData == null)
            return null;
        foreach (var VARIABLE in BattlePlayerData.heroes)
        {
            if (VARIABLE.id == ConfigID)
                return VARIABLE;
        }
        return null;
    }

    public bool CheckHeroAttr(int ConfigID)
    {
        //临时代码
        if (ConfigID == 1701001)
            return true;
        var hero = HeroManager.Instance.GetHeroInfo(ConfigID);
        if (hero == null)
            return false;
        var battleHero = GetHeroData(ConfigID);
        if (battleHero == null)
            return false;
        for (int i = 1; i < (int)AttrType.MAXNUM; i++)
        {
            var val = hero.GetAttribute((HeroAttr)(i));
            var server_val = battleHero.Attr[i];
            if (val != server_val)
                return false;
        }
        return true;
    }
#endregion

    public CreateBattleResponse BattleResponse;

    //是否在战斗流程中
    public bool IsBattleServiceResponse = false;

    public Mode mode;

    public bool IsMainTask = false;

    //是否是资源关
    public bool IsResourceTask = false;

    public async void CreateBattleInstanceInBackground(BattleCoreParam param, CreateBattleResponse response)
    {
        if (IsBattleServiceResponse)
        {
            Debug.LogError("战斗流程中，请检查逻辑！！！！！");
            return;
        }
        IsBattleServiceResponse = true;
        if (response != null)
        {
            BattleResponse = response;
        }
        else
        {
            //副本战斗不需要
            if (param.source != BattleEnterSource.Zone)
            {
                // 请求create协议
                BattleResponse = await BattlePipline.RequestCreateBattleResponse(param);
                if (BattleResponse == null)
                {
                    IsBattleServiceResponse = false;
                    ToastManager.Show("[RequestBattle]->Create battle error,Please check!!!");
                    return;
                }
            }
        }
        try
        {
            // 创建游戏模式对象
            var modeType = param.mode;
            var pveParam = param.pveParam;
            var modeParam = param.modeParam;
            var thisMode = ModeUtil.Create(modeType, pveParam, modeParam, Battle.Instance, BattleResponse);
            mode = thisMode;
            Debug.Log($"[BattleContainer] mode: {mode.GetType().Name}");

            // 通知：在创建战斗游戏对象前
            // 这里会进行创建战斗对象前的网络请求等处理
            await mode.OnPreCreateBattleGameObjectAsync();
            await OnEnter(param, mode);
        }
        catch (Exception e)
        {
            IsBattleServiceResponse = false;
            throw;
        }
    }

    //战斗回放接口
    public async void CreateBattleInstanceInBackgroundRecord(BattleServerResultData recordData)
    {
        // 创建游戏模式对象
        this.param = recordData.battleCoreParam;
        this.param.isBattleReport = true;
        var modeType = recordData.battleCoreParam.mode;
        var pveParam = recordData.battleCoreParam.pveParam;
        var modeParam = recordData.battleCoreParam.modeParam;
        var thisMode = ModeUtil.Create(modeType, pveParam, modeParam, Battle.Instance, recordData.battleServerData.createBattleResponse);
        mode = thisMode;
        recordData.battleCoreParam.isBattleReport = true;
        Debug.Log($"[BattleContainer] mode: {mode.GetType().Name}");

        // 通知：在创建战斗游戏对象前
        // 这里会进行创建战斗对象前的网络请求等处理
        await mode.OnPreCreateBattleGameObjectAsync();
        await OnEnter(recordData.battleCoreParam, mode, recordData);
    }

    public async void CreateBattleInstanceInBackgroundSkillView(BattleCoreParam param)
    {
        if (IsBattleServiceResponse)
        {
            return;
        }
        IsBattleServiceResponse = true;
        try
        {
            // 创建游戏模式对象
            var modeType = param.mode;
            var pveParam = param.pveParam;
            var modeParam = param.modeParam;
            var thisMode = ModeUtil.Create(modeType, pveParam, modeParam, Battle.Instance, null);
            mode = thisMode;
            Debug.Log($"[BattleContainer] mode: {mode.GetType().Name}");

            // 通知：在创建战斗游戏对象前
            // 这里会进行创建战斗对象前的网络请求等处理
            //await mode.OnPreCreateBattleGameObjectAsync();
            await OnEnter(param, mode);
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }
    }

    public static event Action BattleExit;

    public async Task DestroyBattleInstanceAsync()
    {
        //Debug.LogError("离开了战斗");
        BattleConst.Difficult = EStageDifficult.Easy;
        IsMainTask = false;
        IsResourceTask = false;
        BattleUtil.fast_enter = false;
        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.QuitBattle();
        }
        await OnExit();

        //先清理战斗界面
        var page = UIEngine.Stuff.FindPage("BattlePage");
        if (page != null)
            (page as BattlePage).EndGame();
        if (!string.IsNullOrEmpty(battlePageName))
            await UIEngine.Stuff.RemoveFromStackAsync(battlePageName);
        IsBattleServiceResponse = false;
        BattleExit?.Invoke();
        ExtBuffIDs.Clear();
        DestroyInstance();
        Resources.UnloadUnusedAssets();
        GC.Collect();
    }

    /// <summary>
    /// 当战斗是实例被创建时
    /// </summary>
    public async Task OnEnter(BattleCoreParam param, Mode mode, BattleServerResultData recordData = null)
    {
        await GuildLobbyManager.Instance.GoBattle();
        IsFight = true;
        bDestroyAfterLoaded = false;
        battlePageName = "";

        //初始化道具
        var itemlist = new List<int>();
        var formationItems = FormationUtil.ParseFormationItems(param.pveParam.FormationIndex);
        foreach (var VARIABLE in formationItems)
        {
            itemlist.Add(VARIABLE.ItemId);
        }
        BattleDataManager.Instance.InitItem(itemlist);
        this.param = param;
        GameMode = mode;
        if (recordData != null)
        {
            isReportBattle = true;
            BattlePlayerData = recordData.battleServerData.battlePlayer;
            PathFindingManager.Instance.AstarPathCore.LoadGraphAdditional(recordData.battleServerData.pathData.graphAssetList, recordData.battleServerData.pathData.offsetList, recordData.battleServerData.pathData.rotationList);
        }
        else
        {
            isReportBattle = false;

            // pzy:
            // 为什么资源区战斗不需要获取玩家数据？
            if (!IsZoneBattle)
            {
                await GetBattlePlayerData();

                // 竞技场需要使用一个额外的准备界面，用来显示双方的数据
                await mode.AfterLoadedPlayerInfoAsync();
            }
            else
            {
                if (BattlePlayerData == null)
                {
                    Debug.LogError("BattlePlayerData is null !!!!!!!!!");
                }
            }
            await ProjectBattleCoreUtil.Create(param);
        }
        if (IsArenaMode)
        {
            BattleAutoFightManager.Instance.AutoState = AutoState.Auto;
        }
        else
        {
            BattleAutoFightManager.Instance.AutoState = (AutoState)PlayerPrefs.GetInt($"AutoState{Database.Stuff.roleDatabase.Me._id}", 0);
        }
        //创建预制件
        var bucket = BucketManager.Stuff.Battle;
        var battlePrefab = await bucket.GetOrAquireAsync<GameObject>("Battle.prefab");
        Root = GameObject.Instantiate(battlePrefab);
        GameObject.DontDestroyOnLoad(Root);

        //var battle = battleGo.GetComponent<Battle>();

        //加载助战信息
        if (IsMainTask && recordData == null)
        {
            if (!string.IsNullOrEmpty(param.pveParam.AssistUID))
            {
                // 代表的是机器人
                if (RobotUtil.IsRobot(param.pveParam.AssistUID))
                {
                    var player = BattleUtil.CreateTestHero(Database.Stuff.roleDatabase.GM.assist);
                    GameMode.AssistBattleInfo = player;
                }
                else
                {
                    var targetUid = param.pveParam.AssistUID;
                    GameMode.AssistBattleInfo = await BattleApi.RequestFriendAssistAsync(targetUid);
                }
            }
        }
        else if (GameMode.ModeType == BattleModeType.Dreamscape
                 && recordData == null)
        {
            Dreamscape dreamscape = GameMode as Dreamscape;
            GameMode.AssistBattleInfo = await BattleApi.RequestDreamscapeAssistAsync(dreamscape._battleParam.heroid);
        }

        // 加载战斗通用资源
        DebugTaskManager.Add("Battle", "PreloadBattleAssets");

        // 加载进度条
        var loadingFloating = await GameUtil.ShowLoadingFloatingAsync();

        // 获取战斗组件类型
        var battleComponentTypeList = GetBattleComponentTypeList();

        // 创建战斗组件
        foreach (var type in battleComponentTypeList)
        {
            var instance = Activator.CreateInstance(type);
            var component = instance as IBattleComponent;
            component.Setup(this);
            componentList.Add(component);
            typeToBattleSingleInstanceDic[type] = component;
        }
        var count = componentList.Count;
        Debug.Log($"[Battle] total {count} battle component created");
        foreach (var comp in componentList)
        {
            comp.OnBattleCreate();
        }

        // 组件加载资源
        // TODO：可以修改为并发
        foreach (var comp in componentList)
        {
            await comp.OnLoadResourcesAsync();
        }

        // 启动战斗核心
        DebugTaskManager.Add("Battle", "ClientEngine.Instance.InitAsync");
        //await ClientEngine.Instance.InitAsync(param);

        // 组件加载资源
        foreach (var comp in componentList)
        {
            comp.OnCoreCreated();
        }

        //加载死亡资源
        //await MatsPool.Instance.CacheMaterialAsync("Dead");

        //this.WaveDataList = mode.GetWaveDataList();
        //this.Items = mode.GetBattleItems();
        this.CopyId = mode.GetCopyId();
        BattleDataManager.Instance.SetData(mode);
        var isProcessed = await mode.PreloadRes(param, progress => { loadingFloating.Value = progress; });
        Debug.Log($"[Battle] battle core started");

        // 获得场景名称
        var sceneName = mode.GetSceneName();
        Debug.Log($"[Battle] scene: {sceneName}");
        if (!string.IsNullOrEmpty(sceneName))
        {
            // 载入场景
            DebugTaskManager.Add("Battle", "LoadSceneAsync");
            var curScene = SceneManager.GetActiveScene();
            if (!curScene.name.Equals(sceneName))
            {
                this._sceneInstance = await SceneUtil.AddressableLoadSceneAsync(sceneName, LoadSceneMode.Additive);
                var scene = this._sceneInstance.Scene;
                SceneManager.SetActiveScene(scene);
            }
            Debug.Log($"[Battle] scene loaded");
            await mode.OnSceneLoaded();
            // 通知：场景加载完毕
            DebugTaskManager.Add("Battle", "OnSceneLoaded");
        }

        //卸载主界面数据
        TimelineManager.Stuff.ReleaseSkillTimelineUnit();
        Resources.UnloadUnusedAssets();
        GC.Collect();

        //初始化场景数据
        //SceneData.CoreStuff.Init();

        // 获得页面名称
        var pageName = mode.GetPageName();
        Debug.Log($"[Battle] pageName: {pageName}");

        // 跳转页面
        DebugTaskManager.Add("Battle", "ForwardAsync");
        if (pageName.Equals("HeroSkillViewPage"))
        {
            await UIEngine.Stuff.ForwardOrBackToAsync(pageName);
            battlePageName = "HeroSkillViewPage";
        }
        else
        {
            var battlePage = await UIEngine.Stuff.ForwardOrBackToAsync<BattlePage>();
            battlePage.StartGame();
            battlePageName = "BattlePage";
        }

        //await UIEngine.Stuff.ForwardAsync(pageName);
        Debug.Log($"加载镜头效果[OpenEffect]");
        EnvEffectGroup.Instance.OpenEffect(CameraManager.Instance.MainCamera);
        Debug.Log($"[Battle] complete");

        //创建英雄
        await mode.CreateHeros();

        //设置镜头
        mode.SetStartCamera();
        Battle.Instance.CloseLoading();
        this.isEnterCompelted = true;
        if (bDestroyAfterLoaded)
        {
            await DestroyBattleInstanceAsync();
            return;
        }

        //return;
        //开启战斗逻辑
        BattleStateManager.Instance.ChangeState(eBattleState.Ready);
        mode.OnStart();
    }

    public void CloseLoading()
    {
        BattlePreloadUtil.FinishLoading();
        DebugTaskManager.Remove("Battle");
    }

    /// <summary>
    /// 获取所有战斗组件的类型
    /// </summary>
    /// <returns></returns>
    static List<Type> GetBattleComponentTypeList()
    {
        var assembly = typeof(Battle).Assembly;
        var battleComponentTypeList = ReflectionUtil.GetSubClasses<IBattleComponent>(assembly);
        var list = new List<Type>();
        foreach (var type in battleComponentTypeList)
        {
            if (type == typeof(BattleComponent<>))
            {
                continue;
            }

            //Debug.Log($"[Battle] process battle component {type.Name}");
            list.Add(type);
        }
        return list;
    }

    public async Task OnExit()
    {
        Wave = 1;
        IsFight = false;
        isEnterCompelted = false;
        // 战斗力可能设置了动画播放倍速
        // 在战斗结束时统一还原
        BattleDataManager.Instance.TimeScale = 1.0f;
        Time.timeScale = 1.0f;
        var battleRoot = Root.gameObject;
        battleRoot.gameObject.SetActive(false);
        GameObject.Destroy(battleRoot);
        GameObject.DestroyImmediate(BattleMono.Instance);
        //CameraManager.Instance.ResetHAngle();
        //HudManager.Instance.Clear();
        foreach (var comp in this.componentList)
        {
            comp.OnDestroy();
        }
        componentList.Clear();
        CoreEngine.lastestInstance.DestoryAllObject();
        await SceneUtil.AddressableUnloadSceneAsync(this._sceneInstance);
        //GameEventCenter.CleanAll();
        BattleUtil.HeroHps = null;
        PathFindingManager.Instance.Destroy();
        BattleStarted = false;
    }

    public void Update()
    {
        if (this.isEnterCompelted)
        {
            foreach (var comp in this.componentList)
            {
                comp.OnUpdate();
            }
        }
        if (this.GameMode != null)
        {
            GameMode.OnUpdate();
        }
    }

    public void LateUpdate()
    {
        if (this.isEnterCompelted)
        {
            foreach (var comp in this.componentList)
            {
                comp.LateUpdate();
            }
        }
    }

    public void FixedUpdate()
    {
        if (this.isEnterCompelted)
        {
            foreach (var comp in this.componentList)
            {
                comp.FixedUpdate();
            }
        }
    }

    public int Wave { get; set; } = 1;

    public bool NextWaveAvailable()
    {
        return Wave < BattleDataManager.Instance.WaveDataList.Count;
    }

    public int MaxWave
    {
        get { return BattleDataManager.Instance.WaveDataList.Count; }
    }

    /// <summary>
    /// 到下一个位置
    /// </summary>
    public float MoveToNext()
    {
        if (!NextWaveAvailable()) return 0;
        Wave++;
        if (GameMode.ModeType == BattleModeType.Fixed
            || GameMode.ModeType == BattleModeType.Guard
            || GameMode.ModeType == BattleModeType.Defence)
            return 0;
        CameraManager.Instance.TryChangeState(CameraState.Follow);
        float min = float.PositiveInfinity;
        MapUtil.RefreshMapTrigger(true);
        int index = 0;
        foreach (var VARIABLE in BattleDataManager.Instance.GetList(MapUnitType.MainHero))
        {
            var dir = Quaternion.AngleAxis(StageBattleUtil.GetMapRotationY(VARIABLE), Vector3.up) * Vector3.forward;
            var pos = VARIABLE.Pos;
            var creature = BattleEngine.Logic.BattleManager.Instance.ActorMgr.GetActorByIndex(index);
            if (creature != null)
            {
                float t = creature.MoveTo(pos);
                min = Mathf.Min(min, t);
            }
            index++;
        }
        return min;
    }

    /// <summary>
    /// 生成英雄和替补，主角
    /// </summary>
    public async Task SpawnHeros()
    {
        BattleManager.Instance.Init();
        Wave = 1;
        MapUtil.RefreshMapTrigger(true);
        int index = 0;
        foreach (var VARIABLE in BattleDataManager.Instance.GetList(MapUnitType.MainHero))
        {
            var dir = Quaternion.AngleAxis(StageBattleUtil.GetMapRotationY(VARIABLE), Vector3.up) * Vector3.forward;
            var pos = VARIABLE.Pos;
            var hero = HeroManager.Instance.GetHeroInfo(VARIABLE.Id);
            if (!hero.Unlocked)
                continue;
            UnitFatory.CreateUnit(VARIABLE.Id, hero.Level, pos, dir, index, true, BattleConst.ATKCampID, BattleConst.ATKTeamID, true, 1f, null, null);
            index++;
        }
        if (GameMode.AssistBattleInfo != null
            && GameMode.AssistBattleInfo.heroes != null
            && GameMode.AssistBattleInfo.heroes.Count > 0)
        {
            var hero = GameMode.AssistBattleInfo.heroes[0];
            var joinInfo = GetFrindJoinPosition();
            var dir = Quaternion.AngleAxis(StageBattleUtil.GetMapRotationY(joinInfo), Vector3.up) * Vector3.forward;
            UnitFatory.CreateUnit(hero.id, hero.lv, joinInfo.Pos, dir, BattleConst.FriendPosIndex, true, BattleConst.ATKCampID, BattleConst.ATKTeamID, true, 1f, hero);
        }
        await SpawnLeader(false);
        BattleAutoFightManager.Instance.InitManager(BattleLogicManager.Instance.BattleData);
    }

    /// <summary>
    /// 又造一波新怪
    /// </summary>
    public async Task SpawnMonsters(bool IsReady = false)
    {
        var list = BattleDataManager.Instance.GetList(MapUnitType.Monster);
        if (list != null)
        {
            float difficult = 1f;
            if (IsDreamEscapeMode)
            {
                Dreamscape mode = GameMode as Dreamscape;
                difficult = mode.GetDifficult() * 0.001f;
            }
            foreach (var VARIABLE in list)
            {
                var dir = Quaternion.AngleAxis(StageBattleUtil.GetMapRotationY(VARIABLE), Vector3.up) * Vector3.forward;
                var pos = VARIABLE.Pos;
                CombatActorEntity enemyEntity = UnitFatory.CreateUnit(VARIABLE.Id, 1, pos, dir, 0, false, BattleConst.DEFCampID, BattleConst.DEFTeamID, IsReady, difficult);
                AddDebuffSkill(enemyEntity, VARIABLE.Index);
            }
        }
    }

    public void AddDebuffSkill(CombatActorEntity enemyEntity, int index)
    {
        MapWaveModelData stageBuffModelData = BattleDataManager.Instance.GetWaveMonsterData(Wave);
        if (stageBuffModelData != null
            && stageBuffModelData.Index == index)
        {
            WavePassiveSkill passiveSkillLst = BattleDataManager.Instance.GetWavePassiveSkill(Wave);
            if (passiveSkillLst != null)
            {
                for (int i = 0; i < passiveSkillLst.PassiveSkillID.Count; i++)
                {
                    SkillRow row = SkillUtil.GetSkillItem(passiveSkillLst.PassiveSkillID[i], 1);
                    if (row == null)
                    {
                        continue;
                    }
                    enemyEntity.AddPassiveSkill(row);
                }
            }
        }
    }

    public async Task SpawnLeader(bool isResetPos = false)
    {
        //临时记一下替补的位置，没有主角的位置时放置主角
        Vector3 TempDir = Vector3.zero;
        Vector3 TempPos = Vector3.zero;
        var list = BattleDataManager.Instance.GetList(MapUnitType.Player);
        if (list.Count > 0
            && list[0] != null)
        {
            var VARIABLE = list[0];
            if (VARIABLE != null)
            {
                TempDir = Quaternion.AngleAxis(StageBattleUtil.GetMapRotationY(VARIABLE), Vector3.up) * Vector3.forward;
                TempPos = PlayerData.Pos;
            }
        }
        else
        {
            List<Vector3> PlayVec3 = new List<Vector3>();
            List<MapWaveModelData> MainHeroLst = BattleDataManager.Instance.GetList(MapUnitType.MainHero);
            for (int i = 0; i < MainHeroLst.Count; i++)
            {
                PlayVec3.Add(MainHeroLst[i].Pos);
            }
            List<MapWaveModelData> MonsterLst = BattleDataManager.Instance.GetList(MapUnitType.Monster);
            List<Vector3> MonsterVec3 = new List<Vector3>();
            for (int i = 0; i < MonsterLst.Count; i++)
            {
                MonsterVec3.Add(MonsterLst[i].Pos);
            }
            Vector3 leadDir = Vector3.zero;
            if (PlayVec3.Count > 2)
            {
                leadDir = (PlayVec3[1] - PlayVec3[0]).normalized;
            }
            else
            {
                Vector3 MainCenterPos = Vector3Util.GetCenterPoint(PlayVec3);
                Vector3 MonsterCenterPos = Vector3Util.GetCenterPoint(MonsterVec3);
                leadDir = Vector3.Cross(MainCenterPos, MonsterCenterPos).normalized;
            }
            PlayVec3.AddRange(MonsterVec3);
            Vector3 centerVector3 = Vector3Util.GetCenterPoint(PlayVec3);
            TempPos = centerVector3 + leadDir * 4;
            TempDir = (centerVector3 - TempPos).normalized;
        }
        if (!isResetPos)
        {
            CombatActorEntity leaderActor = UnitFatory.CreateUnit(1701001, 1, TempPos, TempDir, BattleConst.PlayerPosIndex, true, BattleConst.ATKCampID, BattleConst.ATKTeamID, true);
            List<int> itemSkillLst = BattleDataManager.Instance.GetItemSkillLst();
            for (int i = 0; i < itemSkillLst.Count; i++)
            {
                SkillRow sr = SkillUtil.GetSkillItem(itemSkillLst[i], 1);
                if (sr == null)
                {
                    continue;
                }
                leaderActor.AttachSkill(sr);
            }
        }
        else
        {
            Creature leaderPlay = BattleManager.Instance.ActorMgr.GetPlayer();
            leaderPlay.mData.SetPosition(TempPos);
            leaderPlay.mData.SetForward(TempDir);
            leaderPlay.SelfTrans.localPosition = TempPos;
            leaderPlay.SelfTrans.localRotation = Quaternion.Euler(TempDir);
        }
    }

    public MapWaveModelData GetSubJoinPosition()
    {
        var list = BattleDataManager.Instance.GetList(MapUnitType.SubHero);
        if (list.Count == 0)
            return new MapWaveModelData();
        return list[0];
    }

    public MapWaveModelData GetFrindJoinPosition()
    {
        var list = BattleDataManager.Instance.GetList(MapUnitType.MainHero);
        if (list.Count == 0)
        {
            Debug.LogError("Cant find MainHero MapWaveModelData");
            return new MapWaveModelData();
        }
        return list[0];
    }

    private int currentIndex = 0;

    public MapWaveModelData GetLinkJoinPosition()
    {
        var list = BattleDataManager.Instance.GetList(MapUnitType.MainHero);
        if (list.Count == 0)
        {
            Debug.LogError("Cant find MainHero MapWaveModelData");
            return new MapWaveModelData();
        }
        if (currentIndex >= list.Count)
        {
            currentIndex = 0;
        }
        var temp = list[currentIndex];
        currentIndex = 0;
        return temp;
    }

    public bool IsDreamEscapeMode => param != null && param.mode == BattleModeType.Dreamscape;
    public bool IsArenaMode => param != null && param.mode == BattleModeType.Arena;
    public bool IsGoldMode => param != null && param.mode == BattleModeType.Gold;

    public MapWaveModelData PlayerData
    {
        get
        {
            var Wave = Battle.Instance.Wave;
            if (BattleDataManager.Instance.WaveDataList == null
                || Wave > BattleDataManager.Instance.WaveDataList.Count)
            {
                return null;
            }
            return BattleDataManager.Instance.WaveDataList[Wave - 1].LeadData;
        }
    }

    public async Task OnWaveStart()
    {
        if (Wave == 1)
        {
            if (!GuideManagerV2.Stuff.IsExecutingForceGuide)
            {
                CameraManager.Instance.Move = false;
                await PlotPipelineControlManager.Stuff.StartPipelineAsync(CopyId, EPlotEventType.StartPoint1, null);
                CameraManager.Instance.Move = true;
            }
        }
        if (Wave == 2)
        {
            if (!GuideManagerV2.Stuff.IsExecutingForceGuide)
            {
                CameraManager.Instance.Move = false;
                await PlotPipelineControlManager.Stuff.StartPipelineAsync(CopyId, EPlotEventType.StartPoint2, null);
                CameraManager.Instance.Move = true;
            }
        }
        GameMode.OnWaveStart();
    }

    public void OnWaveEnd()
    {
        GameMode.OnWaveEnd();
    }

    public bool IsZoneBattle
    {
        get { return param.source == BattleEnterSource.Zone; }
    }

    public void SendBattleResult()
    {
        if (!IsZoneBattle)
            return;
        if (BattleUtil.HeroHps != null)
        {
            var heroIds = BattleUtil.HeroHps.Keys.ToArray();
            for (int i = 0; i < heroIds.Length; i++)
            {
                var role = BattleManager.Instance.ActorMgr.GetActorByConfigID(heroIds[i]);
                if (role != null)
                {
                    BattleUtil.HeroHps[heroIds[i]] = role.mData.CurrentHealth.Value;
                }
            }
        }
        GameMode.SendZoneResult();
    }

    public static bool CheckBattleBegin
    {
        get
        {
            if (Instance == null
                || !Instance.IsFight
                || (int)BattleStateManager.Instance.GetCurrentState() <= (int)eBattleState.Ready)
            {
                return false;
            }
            return true;
        }
    }

    private bool bDestroyAfterLoaded = false;

    /// <summary>
    /// 退出战斗
    /// </summary>
    public async Task QuitBattle()
    {
        BattlePipline.SetBackPage(null);
        if (isEnterCompelted)
        {
            await DestroyBattleInstanceAsync();
        }
        else
        {
            bDestroyAfterLoaded = true;
        }
    }
}