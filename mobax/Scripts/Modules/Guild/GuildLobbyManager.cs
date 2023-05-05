using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BattleEngine.Logic;
using BattleEngine.View;
using Cinemachine;
using CustomLitJson;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

public enum GuildTask
{
    None,
    Lobby,
    Shop,
    Fight,
    Spring,
}

public class GuildLobbyManager : Singleton<GuildLobbyManager>
{
    private Vector3[] pos = new[]
    {
        new Vector3(-3f, 3, -1.7f), new Vector3(-3f, 3f, -1.7f), new Vector3(13f, 3f, -1.7f),
        new Vector3(-10.5f, 3f, -1.7f), new Vector3(16.8f, 3f, -1.7f),
    };

    public Vector3[] mypos = new[]
    {
        new Vector3(-3f, 3, -0.7f), new Vector3(-3f, 3f, -0.7f), new Vector3(13f, 3f, -0.7f),
        new Vector3(-10.5f, 3f, -0.7f), new Vector3(16.8f, 3f, -1.4f),
    };

    public Transform Root;
    public GameObject LobbyRoot;
    private Animator Animator;
    private Transform SpringPlayerRoot;

    private SceneInstance _sceneInstance;

    public GuildLocalPlayer LocalPlayer;
    public GuildPlayer SpringLocalPlayer;

    private Dictionary<string, GuildPlayer> AllPlayers = new Dictionary<string, GuildPlayer>();
    private Dictionary<int, GuildPlayer> SpringPlayers = new Dictionary<int, GuildPlayer>();
    private Dictionary<string, int> SpringUIDs = new Dictionary<string, int>();
    private Dictionary<string, GuildPlayerData> PlayerDatas = new Dictionary<string, GuildPlayerData>();

    public int ConfigID;

    public GuildTask CurTask = GuildTask.Lobby;

    private long LastUpdateTime = 0;

    public bool LastIsSpring = false;

    public GuildTask GuildTask
    {
        get { return CurTask; }
        set { CurTask = value; }
    }

    public int MyPos = 1;

    public List<Transform> SpringPlayerRoots = new List<Transform>();
    public List<Transform> FlashObjects = new List<Transform>();

    public int MaxSpringSlot = 6;

    public void SetTask(GuildTask task)
    {
        CurPos = (int) task;
        GuildTask = task;
    }

    public int CurPos = 1;

    private float IntervalTime = 20f;

    public int CurSpringSlot = 1;

    private GameObject TimeLine = null;

    /// <summary>
    /// 移动同步
    /// </summary>
    /// <returns></returns>
    public async Task<NetMsg<Dictionary<string, GuildState>>> RequestPositionSync(int index, int act = 0)
    {
        var jd = new JsonData();
        jd["update"] = (int) LastUpdateTime;
        jd["pos"] = index;
        jd["act"] = act;
        LastUpdateTime = Clock.TimestampSec;
        var ret = await NetworkManager.Stuff.CallAndGetMsgAsync<Dictionary<string, GuildState>>(ServerType.Game,
            "leaguegame/hall",
            jd, isBlock: false);
        return ret;
    }

    public bool FlashVis = false;

    public void HideFlashTag()
    {
        FlashVis = false;
        foreach (var VARIABLE in FlashObjects)
        {
            VARIABLE.SetActive(false);
        }
    }

    public void ShowFlashTag()
    {
        FlashVis = !FlashVis;
        if (!FlashVis)
        {
            HideFlashTag();
            return;
        }

        for (int i = 0; i < FlashObjects.Count; i++)
        {
            if (!SpringPlayers.ContainsKey(i + 1))
                FlashObjects[i].SetActive(true);
        }
    }

    public async Task UpdateSpringPos(int index)
    {
        NetMsg<Dictionary<string, GuildState>> info = await RequestPositionSync(index + 5);
        RefreshPos(info);
    }

    public async Task UpdateAct(SpringAct act)
    {
        var data = GetData(Database.Stuff.roleDatabase.Me._id);
        NetMsg<Dictionary<string, GuildState>> info = await RequestPositionSync(data.pos, (int) act);
        RefreshPos(info);
    }

    private async Task UpdatePosition()
    {
        NetMsg<Dictionary<string, GuildState>> info = await RequestPositionSync((int) CurTask);
        RefreshPos(info);
    }

    public void SimulateEnterSpring()
    {
        Dictionary<string, GuildState> data = new Dictionary<string, GuildState>();
        data.Add(Database.Stuff.roleDatabase.Me._id, new GuildState() {pos = 5});

        RefreshPos_Sub(data);
    }

    private async Task RefreshPos(NetMsg<Dictionary<string, GuildState>> info)
    {
        if (!info.IsSuccess)
        {
            return;
        }

        await RefreshPos_Sub(info.data);
    }

    private async Task RefreshPos_Sub(Dictionary<string, GuildState> data)
    {
        foreach (var VARIABLE in data)
        {
            //Debug.LogError($"{VARIABLE.Key}_{VARIABLE.Value}");
            var roleinfo = await Database.Stuff.roleDatabase.GetOrRequestIfNeedAsync(VARIABLE.Key);
            if (roleinfo == null)
            {
                continue;
            }

            var role = GetCreature(VARIABLE.Key, RoleUtil.GetKanBan(roleinfo.show));
            //var role = GetCreature(VARIABLE.Key, 1501008);
            if (role != null)
            {
                //温泉
                if (!SpringUIDs.ContainsKey(VARIABLE.Key))
                {
                    SpringUIDs.Add(VARIABLE.Key, VARIABLE.Value.pos);
                }
                else
                {
                    //bool bRefresh = false;
                    if (SpringUIDs[VARIABLE.Key] >= 5)
                    {
                        if (MyPos >= 5 && MyPos == SpringUIDs[VARIABLE.Key] && VARIABLE.Value.pos < 5)
                        {
                            //bRefresh = true;
                            LeaveSpring(VARIABLE.Key);
                        }
                    }
                    else
                    {
                        if (MyPos >= 5 && MyPos == VARIABLE.Value.pos)
                        {
                            //bRefresh = true; 
                            EnterSpringTask(VARIABLE.Key);
                        }
                    }

                    SpringUIDs[VARIABLE.Key] = VARIABLE.Value.pos;
                }

                if (VARIABLE.Value.pos >= 5)
                {
                }
                else if (!role.IsLocalPlayer())
                {
                    if (VARIABLE.Value.pos >= 0)
                    {
                        var target = pos[VARIABLE.Value.pos];
                        if (Vector3.Distance(target, role.transform.position) > 0.01f)
                            role.MoveTo(target);

                        Debug.LogError(VARIABLE.Key + "移动了" + VARIABLE.Value.pos);
                    }
                }

                if (role.IsLocalPlayer())
                {
                    bool isSpring = IsInSpring();

                    MyPos = VARIABLE.Value.pos;
                    if (IsInSpring())
                    {
                        if (!isSpring)
                            RefreshSpringScene();
                    }
                    else
                    {
                        if (isSpring)
                        {
                            RefreshLeaveSpringScene();
                        }
                    }
                }

                if (!PlayerDatas.TryGetValue(VARIABLE.Key, out var player))
                {
                    var playerdata = new GuildPlayerData();
                    playerdata.uid = VARIABLE.Key;
                    playerdata.pos = VARIABLE.Value.pos;
                    playerdata.SetAction((SpringAct) VARIABLE.Value.act);
                    PlayerDatas.Add(VARIABLE.Key, playerdata);
                }
                else
                {
                    player.pos = VARIABLE.Value.pos;
                    player.SetAction((SpringAct) VARIABLE.Value.act);
                    if (player.changed)
                    {
                        player.changed = false;
                        GameEventCenter.Broadcast(GameEvent.UpdateRoomItems);
                    }
                }

                if (VARIABLE.Value.pos == -1)
                {
                    RemoveScenePlayer(VARIABLE.Key);
                    LeaveSpring(VARIABLE.Key);
                }
            }
        }
    }

    public GuildPlayerData GetData(string uid)
    {
        PlayerDatas.TryGetValue(uid, out var player);
        return player;
    }

    private void RefreshSpringScene()
    {
        ReEnterSpring(delegate { RefreshSpring(); });
    }

    private void RefreshSpring()
    {
        HideFlashTag();
        RefreshLeaveSpringScene();
        var list = GetRoomPlayers(MyPos);
        list.Sort((string a, string b) =>
        {
            if (a == Database.Stuff.roleDatabase.Me._id)
                return -1;

            if (b == Database.Stuff.roleDatabase.Me._id)
                return 1;

            return 0;
        });
        foreach (var VARIABLE in list)
        {
            EnterSpringTask(VARIABLE);
        }
    }

    private async Task EnterSpringTask(string uid)
    {
        var roleinfo = await Database.Stuff.roleDatabase.GetOrRequestIfNeedAsync(uid);
        if (roleinfo != null)
        {
            EnterSpring(uid, RoleUtil.GetKanBan(roleinfo.show), 0);
        }
    }


    private void RefreshLeaveSpringScene()
    {
        foreach (var VARIABLE in SpringPlayers)
        {
            GameObject.Destroy(VARIABLE.Value.gameObject);
        }

        SpringPlayers.Clear();
    }

    public bool IsInSpring()
    {
        return MyPos >= 5;
    }

    public Dictionary<int, List<string>> GetRoomPlayers()
    {
        Dictionary<int, List<string>> dic = new Dictionary<int, List<string>>();
        foreach (var VARIABLE in SpringUIDs)
        {
            if (VARIABLE.Value < 5)
                continue;

            if (!dic.ContainsKey(VARIABLE.Value))
            {
                dic.Add(VARIABLE.Value, new List<string>());
            }

            dic[VARIABLE.Value].Add(VARIABLE.Key);
        }

        return dic;
    }

    public List<string> GetRoomPlayers(int index)
    {
        var dic = GetRoomPlayers();
        ;
        int temp_index = index;
        List<string> list;
        if (!dic.TryGetValue(temp_index, out list))
        {
            return new List<string>();
        }
        else
        {
            return list;
        }
    }

    public async Task LoadAvatar()
    {
        /*HeroInfo heroInfo = HeroManager.Instance.GetHeroInfo(ConfigID);
        if (heroInfo == null)
        {
            Debug.LogError("没有找到该英雄");
            return;
        }

        var modelPath = RoleHelper.GetAvatarName(heroInfo);*/
        var address = $"{ConfigID}YP.prefab";

        CacheAvatar = await BattleResManager.Instance.LoadAvatarModel(address);
    }

    public bool IsFirst = false;
    public GameObject CacheAvatar = null;

    public async Task ShowTimeLine()
    {
        if (CacheAvatar == null)
        {
            return;
        }

        CacheAvatar.SetActive(true);
        await TimeLine.GetComponent<TimeLineExtend>().PlaySkillTimeLine(CacheAvatar);

        IsFirst = false;
        TimeLine.SetActive(true);
        var director = TimeLine.GetComponentInChildren<PlayableDirector>();
        await Task.Delay(2000);
        TimeLine.SetActive(false);
        CacheAvatar.SetActive(false);
    }

    public void EnterRoom(string uid, int room)
    {
    }

    public async Task Init(bool loadData = true)
    {
        //Root = new GameObject("PlayerRoot").transform;
        // 加载进度条
        var loadingFloating = await GameUtil.ShowLoadingFloatingAsync();

        //加载资源
        loadingFloating.Value = 0.5f;
        await LoadScence();

        //加载资源
        loadingFloating.Value = UnityEngine.Random.Range(0.76f, 0.79f);
        ;
        //创建预制件
        var bucket = BucketManager.Stuff.Battle;
        var battlePrefab = await bucket.GetOrAquireAsync<GameObject>("LobbyRoot.prefab");
        LobbyRoot = GameObject.Instantiate(battlePrefab);
        GameObject.DontDestroyOnLoad(LobbyRoot);

        loadingFloating.Value = UnityEngine.Random.Range(0.8f, 0.83f);
        ;
        Root = LobbyRoot.transform.Find("RoleRoot").transform;

        SpringPlayerRoot = LobbyRoot.transform.Find("PlayerRoot").transform;
        for (int i = 0; i < MaxSpringSlot; i++)
        {
            var trans = SpringPlayerRoot.Find($"Pos{i + 1}");
            SpringPlayerRoots.Add(trans);

            var obj = SpringPlayerRoot.Find($"Pos{i + 1}/PosFlash");
            obj.name = $"{i + 1}";
            obj.SetActive(false);
            FlashObjects.Add(obj);
        }

        // 先更新一次自己公会的数据
        if (loadData)
        {
            await GuildManager.Stuff.FetchMyGuildInfo();
        }
        //await LoadBone(1701001);
        loadingFloating.Value = 1f;
        await UIEngine.Stuff.ForwardOrBackToAsync<GuildMainPage>();

        //等待进度条加载完成
        await BattlePreloadUtil.FinishLoading();

        var me = Database.Stuff.roleDatabase.Me;
        ConfigID = RoleUtil.GetKanBan(me.show);
        var Player = new GuildLocalPlayer(me._id, ConfigID, true);
        AllPlayers.Add(Player.Uid, Player);
        Player.transform.position = new Vector3(-3f, 3, -0.7f);
        LocalPlayer = Player;
        //var Player2 = new GuildPlayer("2222", 1701001);
        //AllPlayers.Add("2222", Player2);
        //Player2.transform.position = new Vector3(-3.78f, 3, -4.43f);

        //LastUpdateTime = Clock.Timestamp;

        GuildCameraManager.Instance.OnInit();

        //Test();

        TimerMgr.Instance.ScheduleTimer(IntervalTime, delegate
        {
            if (!GuildManager.Stuff.InGuildGuide)
                UpdatePosition();
        }, true, "lobbytimer");

        EnvEffectGroup.Instance.OpenEffect(GuildCameraManager.Instance.MainCamera);
        
        /*TimerMgr.Instance.ScheduleTimerDelay(2, delegate
        {
            SimulateEnterSpring();
        });*/

        /*TimerMgr.Instance.ScheduleTimerDelay(10, delegate
        {
            SimulateEnterSpring();
        });*/
        
        // 先更新一次自己公会的数据
        /*if (loadData)
        {
            await GuildManager.Stuff.FetchMyGuildInfo();
        }*/
    }


    private int[] ids = new[] {1501008, 1501005, 1501011, 1501010, 1502014};

    public async Task Test()
    {
        for (int i = 0; i < 20; i++)
        {
            var temp = new GuildPlayer("2222" + i, ids[UnityEngine.Random.Range(0, ids.Length)], true);
            AllPlayers.Add(temp.Uid, temp);

            temp.transform.position = pos[UnityEngine.Random.Range(0, 5)];
            await Task.Delay(200);
            temp.MoveTo(pos[UnityEngine.Random.Range(0, 5)]);
            await Task.Delay(2000);
        }
    }

    public async Task LoadTimeLine()
    {
        if(TimeLine != null)
            return;
        
        var model = await BucketManager.Stuff.Battle.GetOrAquireAsync<GameObject>("TheBath_timeline.prefab");
        if (model == null)
        {
            BattleLog.LogWarning("Cant find TheBath_timeline  ");
            return;
        }
        TimeLine = GameObject.Instantiate(model);
        TimeLine.transform.SetParent(LobbyRoot.transform);
        TimeLine.transform.localPosition = new Vector3(25.707f, 3f, -5.01f);
        TimeLine.SetActive(false);
    }
    
    public async Task MoveTo(Vector3 pos)
    {
        /*_movePath.Clear();
        
        var pos  = Root.position;
        pos.x -= 20f * Mathf.Sign(dir);
        _movePath.AddLast(pos);
        
        SetState(State.STATE_MOVE);*/

        foreach (var VARIABLE in AllPlayers)
        {
            if (VARIABLE.Value.IsLocalPlayer())
            {
                if (CurTask != GuildTask.Spring)
                {
                    GuildCameraManager.Instance.TryChangeState(GuildCameraState.Free, null);
                    if (LastIsSpring)
                    {
                        GuildCameraManager.Instance.SetCameraFadeMode(CinemachineBlendDefinition.Style.EaseInOut, 1f);
                        await Task.Delay(1600);
                    }

                    //GuildCameraManager.Instance.MainCamera.GetComponent<CinemachineBrain>().m_DefaultBlend = new CinemachineBlendDefinition(CinemachineBlendDefinition.Style.EaseInOut, 0.5f);
                }

                if (Vector3.Distance(pos, VARIABLE.Value.transform.position) > 0.01f)
                    SyncPos(VARIABLE.Key, VARIABLE.Value.ConfigID, pos);
                else
                {
                    ArriveTarget();
                }

                break;
            }
        }

        /*foreach (var VARIABLE in AllPlayers)
        {
            if (!VARIABLE.Value.IsLocalPlayer())
            {
                SyncPos(VARIABLE.Key, dir);
            }
        }*/

        UpdatePosition();
    }

    public void ArriveTarget()
    {
        if (CurTask != GuildTask.None)
        {
            GameEventCenter.Broadcast(GameEvent.ArrivedTarget, CurTask);
            LastIsSpring = CurTask == GuildTask.Spring;
            CurTask = GuildTask.None;
        }
    }


    public async Task LoadScence()
    {
        _sceneInstance = await SceneUtil.AddressableLoadSceneAsync("gonghui", LoadSceneMode.Additive);
        var scene = _sceneInstance.Scene;
        SceneManager.SetActiveScene(scene);
    }

    public bool IsInBattle = false;

    public async Task GoBattle()
    {
        if (LobbyRoot != null)
        {
            LobbyRoot.SetActive(false);
            await SceneUtil.AddressableUnloadSceneAsync(this._sceneInstance);
            
            IsInBattle = true;
        }
    }

    public async Task ReturnSpring()
    {
        var loadingFloating = await GameUtil.ShowLoadingFloatingAsync();

        //加载资源
        loadingFloating.Value = 0.5f;
        IsInBattle = false;
        LobbyRoot.SetActive(true);
        await LoadScence();

        RefreshSpring();

        //等待进度条加载完成
        await BattlePreloadUtil.FinishLoading();
    }

    public async Task Destroy()
    {
        await SceneUtil.AddressableUnloadSceneAsync(this._sceneInstance);
        AllPlayers.Clear();
        SpringPlayers.Clear();
        GuildCameraManager.DestroyInstance();
        LocalPlayer = null;
        TimerMgr.Instance.Remove("lobbytimer");
        GameObject.Destroy(LobbyRoot);
        GameObject.Destroy(Root.gameObject);
        CurTask = GuildTask.None;
        RequestPositionSync((int) CurTask);
        LastIsSpring = false;
        CurPos = 0;
        SpringPlayerRoots.Clear();
        DestroyInstance();
        LastUpdateTime = 0;
        CacheAvatar = null;
        IsInBattle = false;
        TimeLine = null;
    }

    public bool IsInGuildScene()
    {
        return LobbyRoot != null && LobbyRoot.activeSelf;
    }


    public void Update()
    {
        foreach (var VARIABLE in AllPlayers)
        {
            VARIABLE.Value.Update();
        }

        if (IsInBattle)
            return;

        if (IsTouchDown())
        {
            if (GuildCameraManager.Instance.MainCamera == null)
                return;
            Ray ray = GuildCameraManager.Instance.MainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out var hitInfo)) //如果碰撞检测到物体
            {
                if (!hitInfo.collider.gameObject.name.Equals("Flash")) return;
                // this._clickAction?.Invoke();
                //Debug.LogError(hitInfo.collider.gameObject.transform.parent.name + " clicked");

                ChangeSlot(Int32.Parse(hitInfo.collider.transform.parent.name));
            }
        }

        if (Input.GetKeyDown(KeyCode.M))
        {
            UpdateAct(SpringAct.JoinBattle);
        }

        if (Input.GetKeyDown(KeyCode.K))
        {
            UpdateAct(SpringAct.LeaveBattle);
        }
    }

    public bool IsTouchDown()
    {
#if ((UNITY_ANDROID || UNITY_IOS || UNITY_BLACKBERRY || UNITY_TVOS) && !UNITY_EDITOR)
			return Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began;
#else
        return Input.GetMouseButtonDown(0);
#endif
    }

    private GuildPlayer GetCreature(string uid, int configID = 0)
    {
        GuildPlayer role = null;
        if (AllPlayers.TryGetValue(uid, out role))
        {
            return role;
        }

        if (configID == 0)
            return null;

        role = new GuildPlayer(uid, configID);
        AllPlayers.Add(uid, role);
        return role;
    }

    //同步坐标到服务器
    public void SyncPos(string uid, int configID, Vector3 target)
    {
        GuildPlayer player = GetCreature(uid, configID);
        if (player == null)
            return;

        player.MoveTo(target);
    }

    //更新坐标
    public void UpdatePos(string uid, int configID, Vector3 target)
    {
        var player = GetCreature(uid, configID);
        if (player == null)
            return;

        player.MoveTo(target);
    }

    private Vector3[] SprintSlots = new[]
    {
        new Vector3(35.12f, 3.5f, -4.7f),
        new Vector3(34.75f, 0.5f, -11f),
        new Vector3(34.75f, 0.5f, -13f),
        new Vector3(34.75f, 0.5f, -10f),
        new Vector3(34.75f, 0.5f, -14f),
        new Vector3(34.75f, 0.5f, -9.28f),
        new Vector3(35.4f, 0.5f, -12.5f),
        new Vector3(35.4f, 0.5f, -10.37f),
        new Vector3(35.4f, 0.5f, -13.8f),
        new Vector3(35.4f, 0.5f, -9.2f),
    };

    public int GetEmptypSlot(bool isSelef)
    {
        if (isSelef)
        {
            return CurSpringSlot;
        }

        for (int i = 1; i <= MaxSpringSlot; i++)
        {
            if (!SpringPlayers.ContainsKey(i))
                return i;
        }

        return -1;
    }

    public GuildPlayer GetSprintPlayer(string uid)
    {
        foreach (var VARIABLE in SpringPlayers.Values)
        {
            if (VARIABLE.Uid.Equals(uid))
                return VARIABLE;
        }

        return null;
    }

    private string[] anims = new[] {"sit03", "sit06", "sit01", "sit04", "sit05", "sit02", "sit07", "sit08",};

    private string[] helloanims = new[]
        {"mutual03", "mutual06", "mutual01", "mutual04", "mutual05", "mutual02", "sit07", "sit08",};

    public void EnterSpring(string uid, int configID, int slot)
    {
        if (configID == 1701001)
        {
            configID = ids[UnityEngine.Random.Range(0, ids.Length)];
        }

        //configID = 10000;
        //ConfigID = 10000;
        GuildPlayer player = GetSprintPlayer(uid);
        if (player == null)
        {
            if (uid == Database.Stuff.roleDatabase.Me._id)
            {
                player = new GuildLocalPlayer(uid, ConfigID, true, true);
                SpringLocalPlayer = player;

                TrackManager.CustomReport("Guild_Pool_Chat");
            }
            else
            {
                player = new GuildPlayer(uid, configID, true, true);
            }
        }

        int index = GetEmptypSlot(uid == Database.Stuff.roleDatabase.Me._id);
        if (index == -1)
        {
            Debug.LogError("没有多余的空位了");
            return;
        }

        player.transform.SetParent(SpringPlayerRoots[index - 1]);
        player.transform.localPosition = Vector3.zero;
        player.transform.localRotation = Quaternion.identity;
        player.SpringIndex = index;

        SpringPlayers.Add(index, player);

        player.EnterSpring(anims[index - 1]);

        /*if (uid == Database.Stuff.roleDatabase.Me._id)
        {
            ReEnterSpring();
        }*/
    }

    public void SayHello(string uid)
    {
        GuildPlayer player = GetSprintPlayer(uid);
        if (player == null)
        {
            return;
        }

        player.SayHello(helloanims[player.SpringIndex - 1], anims[player.SpringIndex - 1]);
    }

    //player.transform.position = SprintSlots[index-1];


    /*public void ChangeNextSlot()
    {
        if(GetEmptypSlot())
    }*/
    public void ChangeSlot(int slot)
    {
        CurSpringSlot = slot;

        RefreshSpringScene();
    }

    private async Task ReEnterSpring(Action callback)
    {
        var page = UIEngine.Stuff.FindPage<GuildMainPage>();
        if (page != null)
        {
            await page.ReEnterSpring(callback);
        }
    }

    public GuildPlayer GetNextSpringPlayer(int index)
    {
        if (SpringPlayers.Values.Count < index)
            return null;

        return SpringPlayers.Values.ToArray()[index - 1];
    }

    public void LeaveSpring(string uid)
    {
        foreach (var VARIABLE in SpringPlayers)
        {
            if (VARIABLE.Value.Uid.Equals(uid))
            {
                SpringPlayers.Remove(VARIABLE.Key);

                /*if (VARIABLE.Value.IsLocalPlayer())
                {
                    
                }*/
                GameObject.Destroy(VARIABLE.Value.gameObject);
                break;
            }
        }

        CurSpringSlot = 1;
        /*
        foreach (var VARIABLE in SpringPlayers)
        {
            GameObject.Destroy(VARIABLE.Value.gameObject);
        }
        SpringPlayers.Clear();*/
    }

    public void RemoveScenePlayer(string uid)
    {
        var player = GetCreature(uid);
        if (player != null)
        {
            GameObject.Destroy(player.gameObject);

            AllPlayers.Remove(uid);
        }
    }

    public void RemoveOne()
    {
        foreach (var VARIABLE in SpringPlayers)
        {
            if (!VARIABLE.Value.IsLocalPlayer())
            {
                SpringPlayers.Remove(VARIABLE.Key);
                GameObject.Destroy(VARIABLE.Value.gameObject);
                break;
            }
        }
    }

    public void UpdateRoomInfo()
    {
    }

    public GuildPlayer GetSpringPlayerByIndex(int index)
    {
        GuildPlayer player = null;
        SpringPlayers.TryGetValue(index, out player);
        return player;
    }
}