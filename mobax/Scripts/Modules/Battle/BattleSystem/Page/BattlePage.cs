using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using BattleEngine.Logic;
using BattleEngine.Manager;
using BattleEngine.View;
using DG.Tweening;
using BattleSystem.Core;
using BattleSystem.ProjectCore;
using Modules.Battle.BattleSystem.Page;
using Spine.Unity;
using SpineRegulate;
using BattleUtil = BattleEngine.Logic.BattleUtil;
using GlobalK = BattleEngine.Logic.GlobalK;
using BattleEngine.View;
using UnityEngine.EventSystems;

public class BufferUIItem
{
    public int UID;
    public int ConfigID;
    public GameObject go;
    public Image Icon;
    public Text Time;
    public float RemTime;
    public float StartTime;

    public BufferUIItem(Button click)
    {
        ButtonUtil.SetClick(click, delegate() { GameEventCenter.Broadcast(GameEvent.ShowBufferTips, ConfigID); });
    }
}

public class BufferUIShow
{
    public Transform Root;
    public GameObject Prefab;

    public List<BufferUIItem> Buffers = new List<BufferUIItem>();

    public BufferUIShow(Transform parent)
    {
        Root = parent;
        Prefab = Root.Find("Item").gameObject;
        Prefab.SetActive(false);
        TimerMgr.Instance.BattleSchedulerTimer(1f, delegate { Update(); }, true);
    }

    public async void AddBuffer(int uniqueId, int configID)
    {
        var BuffInfo = StaticData.BuffTable.TryGet(configID).Colls[0];
        var go = GameObject.Instantiate(Prefab, Root);
        go.SetActive(true);
        var temp_icon = go.transform.Find("Icon").GetComponent<Image>();
        temp_icon.sprite = null;
        var bucket = BucketManager.Stuff.Battle;
        var address = "zhuoshao" + ".png";
        if (configID == 3002)
        {
            address = "jiafangyu" + ".png";
        }
        var sprite = await bucket.GetOrAquireAsync<Sprite>(address);
        temp_icon.sprite = sprite;
        var temp_time = go.transform.Find("Time").GetComponent<Text>();
        var click = temp_icon.gameObject.AddComponent<Button>();
        Buffers.Add(new BufferUIItem(click)
                        {
                                        UID = uniqueId,
                                        ConfigID = configID,
                                        Icon = temp_icon,
                                        Time = temp_time,
                                        RemTime = BuffInfo.Time / 1000f,
                                        go = go,
                                        StartTime = Time.realtimeSinceStartup,
                        }
        );
    }

    public void RemoveBuffer(int uniqueId)
    {
        var buf = Buffers.Find(b => b.UID == uniqueId);
        if (buf != null)
        {
            GameObject.Destroy(buf.go);
            Buffers.Remove(buf);
        }
    }

    public void Update()
    {
        foreach (var VARIABLE in Buffers)
        {
            float t = (VARIABLE.RemTime - (Time.realtimeSinceStartup - VARIABLE.StartTime));
            VARIABLE.Time.text = string.Format("{0:F1}s", t);
        }
    }
}

public partial class BattlePage : Page
{
    public static float SuperSkillMoveOffset = 15f;

    public BufferBar BossBuffer;

    Image m_StartView;
    Button m_Setting;
    RectTransform m_RTShow;
    RawImage m_RTTextureShow;
    Transform m_RTName;

    private GameObject m_BufferRoot;
    private Text m_BufferName;
    private Text m_BufferDes;

    List<RectTransform> m_RTShows;
    List<RawImage> m_RTTextureShows;
    List<Transform> m_RTNames;

    private BufferUIShow BufferShow;

    public bool SuperGuided { get; set; }

    public string BossRootUID { get; set; }

    private Transform subCameraAnimator;
    public override async Task OnDismissAsync()
    {
    }
    void BindObject()
    {
        this.m_StartView = this.transform.Find("m_StartView").GetComponent<Image>();

        //this.m_Setting = this.transform.Find("Menu/m_Setting").GetComponent<Button>();
        this.m_RTShow = this.transform.Find("RTRoot/m_RTShow").GetComponent<RectTransform>();
        this.m_RTTextureShow = this.transform.Find("RTRoot/m_RTShow/m_RTTextureShow").GetComponent<RawImage>();
        this.m_RTName = this.transform.Find("RTRoot/m_RTShow/m_RTName").GetComponent<Transform>();
        this.m_BufferRoot = this.transform.Find("BufferTips").gameObject;
        this.m_BufferName = this.transform.Find("BufferTips/BufferName").GetComponent<Text>();
        this.m_BufferDes = this.transform.Find("BufferTips/BufferDes").GetComponent<Text>();
        this.BufferShow = new BufferUIShow(this.BufferRoot.transform);
    }

    List<int> events = new List<int>();
    private List<string> SuperList = new List<string>();

    void AddEvent(int evt, Action<object[]> handle)
    {
        GameEventCenter.AddListener(evt, this, handle);
        this.events.Add(evt);
    }

    void RemoveListener()
    {
        foreach (var e in this.events)
        {
            GameEventCenter.RemoveListener(e, this);
        }
        EventManager.Instance.RemoveListener<SkillAbilityExecution>("BattleSkillBeginExecution", OnSpellSkillPoint);
        EventManager.Instance.RemoveListener<CombatActorEntity>("BattleRefreshBuff", BufferRefresh);
        EventManager.Instance.RemoveListener<CombatActorEntity>("OnTriggerDeadPoint", CheckKillTargetEvent);
        EventManager.Instance.RemoveListener<SkillPreWarningActionViewTask>("BattleRefreshPreWarning", BattleRefreshPreWarning);
    }

    private void SuperSkillEnd(string uid)
    {
        foreach (var VARIABLE in this.SuperList)
        {
            if (VARIABLE == uid)
            {
                this.SuperList.Remove(VARIABLE);
                break;
            }
        }
    }

    void Awake()
    {
        this.BindObject();
        CreateHeadView();
        BossBuffer = new BufferBar(this.BossBufferRoot.transform, "BossBuffer.prefab");
        this.Focus.Init(this);

        SupperSkillEffect = this.HeroItemView_SuperSkill.transform;
        SuperSkillGraph = this.SpineGraphic;
        //StartGame();
    }

    private Transform SupperSkillEffect;
    private SkeletonGraphic SuperSkillGraph;
    public Transform GetSuperSkillEffect()
    {
        return SupperSkillEffect;
    }

    public SkeletonGraphic GetSuperSkillGraph()
    {
        return SuperSkillGraph;
    }
    

    public override void OnPush()
    {
        this.headViewCreated = false;
    }

    public override void OnPop() { }

    void AddListenner()
    {
        this.AddEvent(GameEvent.UpdateEnergy, this.UpdateEnergy);
        this.AddEvent(GameEvent.UpdateHpMp, this.UpdateHpMp);
        this.AddEvent(GameEvent.ShowSubCamera, this.ShowSubCamera);
        this.AddEvent(GameEvent.ClientBattleStart, this.ClientBattleStart);
        this.AddEvent(GameEvent.ClientBattleEnd, this.ClientBattleEnd);
        this.AddEvent(GameEvent.BattleHeroUpdate, this.ResetRoleList);
        this.AddEvent(GameEvent.AddBuffer, this.AddBuffer);
        this.AddEvent(GameEvent.RemoveBuffer, this.RemoveBuffer);
        this.AddEvent(GameEvent.ShowBufferTips, this.ShowBufferTips);
        this.AddEvent(GameEvent.ShowStartAnimtion, this.ShowStartAnimtion);
        this.AddEvent(GameEvent.TalkShow, this.TalkShow);
        this.AddEvent(GameEvent.ShowUI, this.ShowUI);
        this.AddEvent(GameEvent.RoleDie, this.RoleDie);
        this.AddEvent(GameEvent.SuperSkillStart, this.SuperSkillStart);
        this.AddEvent(GameEvent.ShowLive2D, this.ShowLive2D);
        this.AddEvent(GameEvent.ShowItemWords, this.ShowItemWords);
        this.AddEvent(GameEvent.ShowDamageBar, this.ShowDamageBar);
        this.AddEvent(GameEvent.CatEnter, this.CatEnter);
        this.AddEvent(GameEvent.WaveEnd, this.WaveEnd);
        this.AddEvent(GameEvent.PlayMood, this.PlayMood);
        this.AddEvent(GameEvent.DefenceWaveTips, this.WaveStart);
        this.AddEvent(GameEvent.DisableSiLaLi, this.DisableSiLaLi);
        this.AddEvent(GameEvent.SelectChanged, this.SelectChanged);
        this.AddEvent(GameEvent.TargetChanged, this.TargetChanged);
        this.AddEvent(GameEvent.EnemyComing, this.EnemyComing);
        this.AddEvent(GameEvent.MonsterPass, this.MonsterPass);
        this.AddEvent(GameEvent.ShowTalk, this.ShowTalkHandler);
        this.AddEvent(GameEvent.BreakQtePress, this.BreakQtePress);
        this.AddEvent(GameEvent.ShowRoleHp, this.ShowRoleHp);
        this.AddEvent(GameEvent.ShowBreakDamageChanged, this.ShowBreakDamageChanged);
        this.AddEvent(GameEvent.ShowBreakDamage, this.ShowBreakDamage);
        this.AddEvent(GameEvent.RefreshItemNum, this.RefreshItemNum);
        this.AddEvent(GameEvent.ShowTargetCamera, this.ShowTargetCamera);
        this.AddEvent(GameEvent.CloseTargetCamera, this.CloseTargetCamera);
        this.AddEvent(GameEvent.BreakDef, this.BreakQteShow);
        this.AddEvent(GameEvent.ActorDie, this.ActorDie);
        this.AddEvent(GameEvent.NextDefenceWave, WaveNextTips);
        this.AddEvent(GameEvent.FriendToBattle, this.FriendToBattle);
        //this.AddEvent(GameEvent.MonsterAppear, this.ActorDie);

        //EventManager.Instance.AddListener<string>("SuperSkillEnd", SuperSkillEnd);
        EventManager.Instance.AddListener<CombatActorEntity>("BattleRefreshBuff", BufferRefresh);
        EventManager.Instance.AddListener<SkillAbilityExecution>("BattleSkillBeginExecution", OnSpellSkillPoint);
        EventManager.Instance.AddListener<CombatActorEntity>("OnTriggerDeadPoint", CheckKillTargetEvent);
        EventManager.Instance.AddListener<SkillPreWarningActionViewTask>("BattleRefreshPreWarning", BattleRefreshPreWarning);
    }

    private void MonsterPass(object[] data)
    {
        int rem = 3 - (int)data[0];
        rem = Mathf.Max(0, rem);
        PassMonsterValue.text = $"X {rem}";
        UiUtil.ScaleBling(PassMonsterValue.transform, 2.5f, 1f, 0.2f, 0.2f, null, "PassMonsterNum");
    }

    private void ShowTalkHandler(object[] data)
    {
        ShowTalk((string)data[0], 0, false, (string)data[1]);
    }

    private List<GameObject> BreakAnims = new List<GameObject>();

    private void BreakQtePress(object[] data)
    {
        //this.BreakAnim.gameObject.SetActive(false);
        //this.BreakAnim.gameObject.SetActive(true);
        var count = (int)data[0];
        if (count > 3)
        {
            count = 3;
        }
        this.BreakAnims[count - 1].gameObject.SetActive(true);
        TimerMgr.Instance.BattleSchedulerTimer(2f, delegate { this.BreakAnims[count - 1].gameObject.SetActive(false); });
        /*TimerMgr.Instance.BattleSchedulerTimerDelay(0.7f, delegate
        {
            EffectManager.Instance.CreateCameraEffect("fx_ui_breakbanner_suiping", 2f, Vector3.zero, Vector3.one);
        });*/
    }

    public void OnSpellSkillPoint(SkillAbilityExecution combatAction)
    {
        if (combatAction.SkillAbility.SkillBaseConfig.skillType == (int)SKILL_TYPE.SSP
            && combatAction.OwnerEntity.isAtker)
        {
            var list = BattleManager.Instance.ActorMgr.GetAllCamp(combatAction.OwnerEntity.CampID);
            foreach (var VARIABLE in list)
            {
                if (combatAction.OwnerEntity.UID == VARIABLE.mData.UID)
                {
                    VARIABLE.Trigger(EmojiEvent.CastSkill);
                    break;
                }
            }
        }
        
        if (combatAction.SkillAbility.SkillBaseConfig.skillType == (int)SKILL_TYPE.SPSKL && combatAction.OwnerEntity.isAtker)
        {
            foreach (var VARIABLE in role_items)
            {
                if (VARIABLE.Role != null && VARIABLE.Role.mData.UID == combatAction.OwnerEntity.UID)
                {
                    VARIABLE.PlaySuperSkillAnim();
                    if(!BattleDataManager.Instance.IsPlayCutScene)
                        this.AssistView.SetData(VARIABLE.Role.mData.ConfigID);
                    break;
                }
            }
        }
    }

    public void CheckKillTargetEvent(CombatActorEntity actor)
    {
        if (actor.UID == BossRootUID)
        {
            Boss_GuildRoot.gameObject.SetActive(false);
        }
        if (!actor.isAtker)
        {
            PlotPipelineControlManager.Stuff.StartPipelineAsync(Battle.Instance.CopyId, EPlotEventType.KillAppointMonsterId, new PlotParams() { MonsterId = actor.ConfigID });
        }
    }

    public void BufferRefresh(CombatActorEntity entity)
    {
        if (entity.battleItemInfo.isBoss)
        {
            BossBuffer.SetData(entity);
            return;
        }
        if (!entity.isAtker)
            return;
        foreach (var VARIABLE in this.role_items)
        {
            if (VARIABLE == null
                || VARIABLE.Role == null)
            {
                continue;
            }
            if (VARIABLE.Role.ID == entity.UID)
            {
                VARIABLE.RefreshBuffer();
                break;
            }
        }
    }

    private float WarningTotalTime = 1f;
    private float WarningRemTime = 1f;

    public void BattleRefreshPreWarning(SkillPreWarningActionViewTask Task)
    {
        if (Task.SkillAbilityExecution != null
            && Task.SkillAbilityExecution.OwnerEntity != null)
        {
            if (Task.SkillAbilityExecution.OwnerEntity.CampID == 0)
                return;
            WarningTotalTime = Task.TaskData.perDuration * (1f / 30);
            HudManager.Instance.ShowWarningBar(Task.SkillAbilityExecution.OwnerEntity.UID, WarningTotalTime);

            //Boss血条
            if (TargetRole != null)
            {
                if (TargetRole.mData == Task.SkillAbilityExecution.OwnerEntity)
                {
                    WarningRemTime = WarningTotalTime;
                    WarningBarRoot.gameObject.SetActive(true);
                }
            }
            Debug.Log("UID " + Task.SkillAbilityExecution.OwnerEntity.UID);
            Debug.Log("PreTime " + Task.TaskData.perDuration * (1f / 30));
        }
    }

    public async void WaveEnd(object[] data)
    {
        if (Battle.Instance.Wave != 1)
            return;
        ShowPlotEmoji(EPlotEventType.EndPoint1);
    }

    public async void WaveStart(object[] data)
    {
        this.m_StartView.transform.Find("words1").GetComponent<Image>().enabled = false;
        //this.m_StartView.gameObject.SetActive(false);
        this.m_StartView.gameObject.SetActive(true);
        LagacyUtil.PlayAnimation(this.gameObject, "BattlePage_animation");
        //this.WaveRoot.gameObject.SetActive(true);
        string des = "";
        if (Battle.Instance.Wave == 2)
        {
            des = LocalizationManager.Stuff.GetText("M10_defense_chat_005");
        }
        else if (Battle.Instance.Wave == 3)
        {
            des = LocalizationManager.Stuff.GetText("M10_defense_chat_006");
        }
        this.WaveTips.text = des;
        TimerMgr.Instance.BattleSchedulerTimer(3, delegate { this.m_StartView.gameObject.SetActive(false); });
    }

    

    public async void FriendToBattle(object[] data)
    {
        FriendRoot.SetActive(false);
    }

    public async void WaveNextTips(object[] data)
    {
        ShowMiddleTips(LocalizationManager.Stuff.GetText("m1_battlepage_monstercoming"));
        ResetDefenceInfo();
        this.Wave.text = Battle.Instance.Wave.ToString() + "/" + Battle.Instance.MaxWave.ToString();
        ShowDebuffMonsterTips();
    }

    private void ShowMiddleTips(string des)
    {
        this.m_StartView.transform.Find("words1").GetComponent<Image>().enabled = false;
        this.m_StartView.gameObject.SetActive(true);
        LagacyUtil.PlayAnimation(this.gameObject, "BattlePage_animation");
        this.WaveTips.text = des;
        TimerMgr.Instance.BattleSchedulerTimerDelay(3, delegate { this.m_StartView.gameObject.SetActive(false); });
    }

    /// <summary>
    /// 禁止斯拉李
    /// </summary>
    /// <param name="data"></param>
    public async void DisableSiLaLi(object[] data)
    {
        foreach (var VARIABLE in role_items)
        {
            if (VARIABLE.Role.ConfigID == BattleConst.SiLaLiID)
            {
                VARIABLE.Disable();
                break;
            }
        }
    }

    public void TargetChanged(object[] data)
    {
        var actor = data[0] as CombatActorEntity;
        if (SceneObjectManager.Instance.CurSelectHero == null
            || actor.UID != SceneObjectManager.Instance.CurSelectHero.mData.UID)
            return;
        SelectChanged(null);
    }

    public void SelectChanged(object[] data)
    {
        var role = SceneObjectManager.Instance.CurSelectHero;
        if (role == null)
            return;
        Creature target = null;
        if (StageBattleUtil.IsBossWave())
        {
            target = BossCreature;
        }
        else
        {
            target = SceneObjectManager.Instance.Find(role.mData.targetKey);
        }
        if (target != null)
        {
            //this.Boss.gameObject.SetActive(true);
            ShowBossPage(target);
        }
        else
        {
            if (Battle.Instance.GameMode.ModeType != BattleModeType.Guard)
                this.Boss_GuildRoot.gameObject.SetActive(false);
        }
        //CheckMiddle();
        this.JobAnti.SelectChange();
    }

    public async void ShowPlotEmoji(EPlotEventType evt)
    {
        BattleMono.Instance.StartCoroutine(ShowPlot(evt));
    }

    public IEnumerator ShowPlot(EPlotEventType evt)
    {
        List<BattlePlotChatInfo> list =
            PlotPipelineManager.Stuff.BattlePlotPipelineData(Battle.Instance.CopyId, evt);
        foreach (var VARIABLE in list)
        {
            yield return new WaitForSeconds(2f);
            ShowTalk(VARIABLE.Word, VARIABLE.HeroId);
        }
    }

    public async void SecondWaveStart()
    {
        if (Battle.Instance.Wave != 2)
            return;
        ShowPlotEmoji(EPlotEventType.StartPoint2);
    }

    bool pause;
    float CommonCD;

    void PauseBattle()
    {
        // 让战斗核心的帧驱动器暂停
        //Battle.Instance.GetBattleComponent<ClientEngine>().PauseDriver();
        // 停止动画表现
        //Time.timeScale = 0;
    }

    void ResumeBattle()
    {
        // 恢复战斗核心的帧驱动
        //Battle.Instance.GetBattleComponent<ClientEngine>().ResumeDriver();
        // 继续动画表现
        //Time.timeScale = 1;
    }

    private bool IsPause = false;
    async Task OnPuaseClicked()
    {
        if(IsPause)
            return;
        
        IsPause = true;
        BattleManager.Instance.BtnPause(true);
        BattleDataManager.Instance.TimeScale = 0f;
        //UIEngine.Stuff.Forward("BattleSettingPage");
        UIEngine.Stuff.ForwardOrBackTo<BattlePausePage>();
        await Task.Delay(2000);
        IsPause = false;
    }

    private bool isChangeTimeScalse = false;
    private float offsetValue = 0.1f;
    private float currentValue = 1.0f;
    public Text TempBattleScale;

    public void TempBtnChangeTimeScale()
    {
        currentValue += offsetValue;
        if (currentValue > 2.0f)
        {
            currentValue = 1.0f;
        }
        Time.timeScale = currentValue;
        TempBattleScale.text = (currentValue * 10).ToString("00");
    }
    
    protected override async Task LogicBackAsync()
    {
        this.OnPuaseClicked();
    }

    void Init()
    {
        //autohide_lists.Add(BreakPropExpansion.gameObject);
        //autohide_lists.Add(FcousPropExpansion.gameObject);
        //autohide_lists.Add(AssistPropExpansion.gameObject);
        ButtonUtil.SetClick(this.PauseBtn, () =>
                        {
                            //Dialog.Confirm("设置", "暂未开放");
                            this.OnPuaseClicked();
                        }
        );
        ButtonUtil.SetClick(this.WeakButton, () =>
                        {
                            if (TargetRole != null)
                            {
                                string des = LocalizationManager.Stuff.GetText($"M9_SystemUI_{TargetRole.mData.Weak}");
                                this.WeakTipsDes.text = des;
                            }
                            else
                            {
                                return;
                            }
                            this.WeakTips.SetActive(true);
                            TimerMgr.Instance.BattleSchedulerTimerDelay(2f, delegate { this.WeakTips.SetActive(false); });
                        }
        );
        this.pause = false;
        CameraManager.Instance.EnableRenderFeature("NewGaussianBlurRenderPassFeature");

        //this.m_StartView.gameObject.SetActive(true);
        BattleTimer.Instance.DelayCall(2f, (param) =>
                        {
                            if (!CameraManager.IsAccessable)
                            {
                                return;
                            }
                            CameraManager.Instance.EnableRenderFeature("NewGaussianBlurRenderPassFeature", false);
                            this.m_RTShow.gameObject.SetActive(true);
                            //this.m_StartView.gameObject.SetActive(false);
                        }
        );
        LagacyUtil.SetRenderTexture(CameraManager.Instance.RTCamera.transform, this.m_RTTextureShow.transform);
        /*ButtonUtil.ResetListner(Auto0.GetComponent<Button>(), () =>
                        {
                            if (Battle.Instance.IsArenaMode)
                                return;
                            //Auto0.gameObject.SetActive(false);
                            //Auto1.gameObject.SetActive(true);
                            this.AutoArrow.GetComponent<Animation>().Play("AutoEnableAnim");
                            AutoSuperSkill = true;
                        }
        );*/
        ButtonUtil.SetClick(Auto1, () =>
                        {
                            if (Battle.Instance.IsArenaMode)
                                return;
                            BattleAutoFightManager.Instance.AutoState = (AutoState)(((int)BattleAutoFightManager.Instance.AutoState + 1) % 2);
                            BattleConst.AutoFight = BattleAutoFightManager.Instance.AutoState == AutoState.Auto;
                            BattleManager.Instance.BattleInfoRecord.isAutoBattle = BattleConst.AutoFight;
                            AutoSuperSkill = BattleConst.AutoFight;
                            PlayerPrefs.SetInt($"AutoState{Database.Stuff.roleDatabase.Me._id}", (int)BattleAutoFightManager.Instance.AutoState);
                            /*
                            if (AutoSuperSkill)
                            {
                                this.AutoArrow.GetComponent<Animation>().Play("AutoEnableAnim");
                            }
                            else
                            {
                                this.AutoArrow.GetComponent<Animation>().Play("AutoStopAnim");
                            }
                            */
                            RefreshAutoButton();
                            AutomaticTips.SetActive(true);
                            List<string> tips = new List<string>() { LocalizationManager.Stuff.GetText("m1_battlepage_autotips1"), LocalizationManager.Stuff.GetText("m1_battlepage_autotips2"), LocalizationManager.Stuff.GetText("m1_battlepage_autotips3") };
                            UiUtil.ShowRollText(AutoTips, tips[(int)BattleAutoFightManager.Instance.AutoState], false, true, 100f);
                            TimerMgr.Instance.Remove("autotips");
                            TimerMgr.Instance.BattleSchedulerTimerDelay(5f, delegate { AutomaticTips.SetActive(false); }, false, "autotips");
                        }
        );
        ButtonUtil.SetClick(FriendButton, () =>
        {
            if(FriendCDTime.enabled)
                return;
            
            SendFriendToBattle();
            FriendRoot.SetActive(false);
        });
        
        this.Auto1.SetActive(!Battle.Instance.IsArenaMode);

        ButtonUtil.SetClick(FocusMask, () => { this.ShowFocusVis(false); });
    }
    
    public void RefreshAutoButton()
    {
        DOTween.Kill("AutoRotate");
        this.AutoArrow.transform.localEulerAngles = Vector3.zero;
        if (AutoSuperSkill)
            this.AutoArrow.transform.DOLocalRotate(new Vector3(0, 0, -180), 1f, RotateMode.Fast).SetLoops(-1, LoopType.Restart).SetId("AutoRotate").SetEase(Ease.Linear);
        else
        {
            this.AutoArrow.transform.localEulerAngles = Vector3.zero;
        }
        List<Color> colors = new List<Color>() { ColorUtil.HexToColor("31B9E5"), Color.yellow, Color.red };
        //this.AutoWord.color = colors[(int)BattleAutoFightManager.Instance.AutoState];
        //this.AutoArrow.color = colors[(int)BattleAutoFightManager.Instance.AutoState];
    }

    public void ShowFocusVis(bool vis)
    {
        if (vis && this.DefenceInfo.gameObject.activeSelf)
            return;
        this.Focus.ShowOrderRoot(vis);
        //this.FocusMask.SetActive(vis);
        /*foreach (var VARIABLE in role_items)
        {
            VARIABLE.ShowFocus(vis);
        }*/
        /*if (vis)
        {
            BattleDataManager.Instance.TimeScale = 0.2f;
        }
        else
        {
            BattleDataManager.Instance.TimeScale = 1f;
        }*/
    }

    public void ShowBreakDamgeRoot()
    {
        var role = SceneObjectManager.Instance.Find(BossRootUID);
        if (role == null)
            return;
        BreakRoot.gameObject.SetActive(role.mData.BreakDefComponent.IsBreak);
        if (!role.mData.BreakDefComponent.IsBreak)
            return;
        BreakDamageValue.text = (int)(role.mData.BreakDefComponent.DamageParam * 100) + "%";
    }

    public void RefreshItemNum(object[] data)
    {
        /*BreakPropExpansion.RefreshData();
        FcousPropExpansion.RefreshData();
        AssistPropExpansion.RefreshData();*/
        var type = (int)data[0];
        if (type == 1)
        {
            BreakBattleItemView.RefreshNum();
        }
        else if (type == 3)
        {
            DefenceBattleItemView.RefreshNum();
        }
        else if (type == 4)
        {
            FocusBattleItemView.RefreshNum();
        }
    }
    
    public void ShowBreakDamage(object[] data)
    {
        var actor = data[0] as CombatActorEntity;
        if (BossRootUID != actor.UID)
            return;
        ShowBreakDamgeRoot();
    }

    public void ShowBreakDamageChanged(object[] data)
    {
        var actor = data[0] as CombatActorEntity;
        if (BossRootUID != actor.UID)
            return;
        ShowBreakDamageAnim();
    }

    public void ShowBreakDamageAnim()
    {
        ShowBreakDamgeRoot();
        BreakDamageAnim.SetActive(true);
        UiUtil.ScaleBling(BreakDamageValue.transform, 2.5f, 1f, 0.2f, 0.2f, delegate { BreakDamageAnim.SetActive(false); }, BossRootUID);
    }

    /*private void HideSubstitute(bool hide)
    {
        if (hide)
        {
            //this.Main.transform.DOLocalMoveY(-328f, 0.1f);
            this.HideButtonRoot.transform.DOScale(1f, 0.1f);
            this.ExchangeIn.gameObject.SetActive(true);
            this.ExchangeOut.gameObject.SetActive(false);
            //this.HideButton.transform.localScale = Vector3.one;
            //this.HideRoot.gameObject.SetActive(true);
        }
        else
        {
            this.HideRoot.gameObject.SetActive(false);
            this.ExchangeOut.gameObject.SetActive(false);
            this.ExchangeIn.gameObject.SetActive(true);
        }
    }*/

    private bool AutoSuperSkill = false;

    private float m_ComboTime = 0;

    public float FocusCDTime = 0;

    private void OnUpdateTime(float dt)
    {
        foreach (var VARIABLE in battle_items)
        {
            VARIABLE.UpdateCDTime(dt);
        }
    }
    
    // TODO:
    // 确定类型
    public List<HeroItemView> role_items = new List<HeroItemView>();
    //public List<SubstituteItem> substitute_items = new List<SubstituteItem>();
    public List<BattleItem> battle_items = new List<BattleItem>();

    void ClientBattleEnd(object[] param)
    {
        this.Role.gameObject.SetActive(false);
        this.Spine.gameObject.SetActive(false);
        CameraSetting.Ins.CloseNoise();
    }

    bool headViewCreated = false;

    // 这里有些问题
    // 在第二波怪开始时也会发送此消息
    void ClientBattleStart(object[] param)
    {
        CameraFollowTarget.Instance.SetDirection(SceneObjectManager.Instance.CurSelectHero.GetDirection());
        CameraSetting.Ins.OpenNoise();
        //AudioManager.PlayBgmInBackground("sound_fight_bgm");
        this.InitRoles();
        if (!headViewCreated)
        {
            // 头像一场战斗只创建一次
            //this.CreateHeadView();
            headViewCreated = true;
        }
        else
        {
            //this.m_StartView.gameObject.SetActive(false);
            //LagacyUtil.PlayAnimation(this.gameObject, "BattlePage_Enter");
        }
        this.Role.gameObject.SetActive(true);
        this.HeroRoot.gameObject.SetActive(true);
        this.HideRoot.gameObject.SetActive(true);
        this.UpdateHeads();
        //HideSubstitute(true);
        GameEventCenter.Broadcast(GameEvent.CameraRoll, true);
        var info = StaticData.StageTable.TryGet(Battle.Instance.CopyId);
        if (info == null)
            return;
        this.BloodNum.text = "";
        this.Stage.text = LocalizationManager.Stuff.GetText(info.desLv);
        this.Wave.text = Battle.Instance.Wave.ToString() + "/" + Battle.Instance.MaxWave.ToString();
        IsBoss = StageBattleUtil.IsBossWave();
        if (IsBoss)
        {
            foreach (var VARIABLE in BattleEngine.Logic.BattleManager.Instance.ActorMgr.GetCamp(1))
            {
                if (VARIABLE.mData.battleItemInfo.isBoss)
                {
                    BossCreature = VARIABLE;
                    break;
                }
            }
        }
        if (Battle.Instance.Wave == 1)
        {
            List<Creature> actors = BattleEngine.Logic.BattleManager.Instance.ActorMgr.GetAllActors();
            foreach (var VARIABLE in actors)
            {
                if (VARIABLE.mData.isAtker
                    && VARIABLE.IsMain)
                    VARIABLE.Trigger(EmojiEvent.EnterLevel);
            }
            ShowPlotChat();

            PassMonsterRoot.SetActive(Battle.Instance.GameMode.ModeType == BattleModeType.Defence && !Battle.Instance.IsArenaMode);
            this.ReportMask.enabled = BattleLogicManager.Instance.IsReport;
            
            SetAssistCD(6);
        }
        //ShowBossPage(IsBoss);
        SecondWaveStart();
        //this.Focus.CloseFocusList();
        //WaveStart();
        SelectChanged(null);
        
        ResetDefenceInfo();

        ShowDebuffMonsterTips();

        ShowFocusVis(true);
    }

    private bool IsBufferMonsterTips = false;
    private void ShowDebuffMonsterTips()
    {
        if (IsBufferMonsterTips)
        {
            return;
        }

        IsBufferMonsterTips = true;
        
        //Debuff怪出现提示
        TimerMgr.Instance.BattleSchedulerTimer(0.5f, delegate
            {
                foreach (var VARIABLE in BattleManager.Instance.ActorMgr.GetDefLst())
                {
                    var SkillPassiveAbility = VARIABLE.mData.GetStagePassiveBuff();
                    if (SkillPassiveAbility != null)
                    {
                        string des = LocalizationManager.Stuff.GetText("m1_battlepage_buffermonstertips");
                        ShowMiddleTips(des);

                        //小镜头照一下
                        GameEventCenter.Broadcast(GameEvent.EnemyComing, VARIABLE, 1.5f, 0f);
                        TimerMgr.Instance.BattleSchedulerTimerDelay(1.5f, delegate
                            {
                                des = LocalizationManager.Stuff.GetText(SkillPassiveAbility.SkillRowInfo.Desc);
                                ShowMiddleTips(des);
                            }
                        );
                        break;
                    }
                }
            }
        );
    }

    private async void ShowPlotChat()
    {
        BattleMono.Instance.StartCoroutine(ShowPlotChatCoroutine());
    }
    
    public IEnumerator ShowPlotChatCoroutine()
    {
        List<BattlePlotChatInfo> list = PlotPipelineManager.Stuff.BattlePlotPipelineData(Battle.Instance.CopyId, EPlotEventType.StartBattle);
        foreach (var VARIABLE in list)
        {
            yield return new WaitForSeconds(2f);
            ShowTalk(VARIABLE.Word, VARIABLE.HeroId);
        }
    }

    public Transform GetItemListTransform(int index = 0)
    {
        if (index == 0)
        {
            return BreakBattleItemView.transform;
        }
        if (index == 1)
        {
            return DefenceBattleItemView.transform;
        }
        if (index == 2)
        {
            return FocusBattleItemView.transform;
        }
        
        return BreakBattleItemView.transform;
    }
    
    // 获取放大招的那个节点
    public Transform GetRoleItemTransform(int index = 0)
    {
        if (role_items.Count <= index + 1)
            return null;
        var content = role_items[index].GetHeadClick();
        return content;
    }
    
    // 获取防御节点
    public Transform GetRoleDefenceTransform(int index = 0)
    {
        if (role_items.Count <= index + 1)
            return null;
        var content = role_items[index].GetDefenceButton();
        return content;
    }
    
    // 获取破防QTE节点
    public Transform GetBreakQteTransform()
    {
        var content = BreakDefUI.GetQTE();
        return content;
    } 
    
    // 获取集火按钮
    public Transform GetFocusTransform(int index)
    {
        var content = Focus.GetFocusItem(index);
        return content;
    } 
    
    // 获取集火确认按钮
    public Transform GetFocusConfirmTransform(int index)
    {
        var content = Focus.GetConfirmItem(index);
        return content;
    } 
    
    private int CD = 15;

    private int ItemRemTime = 15;

    private Creature BossCreature { get; set; }

    private bool IsBoss { get; set; }

    public void ShowBossPage(bool isBoss)
    {
        UiUtil.SetActive(this.NextBlood.gameObject, false);
        if (Battle.Instance.GameMode.ModeType != BattleModeType.Guard)
            this.Boss_GuildRoot.gameObject.SetActive(isBoss);
        if (isBoss)
        {
            if (BossCreature == null)
                return;
            ShowBossPage(BossCreature);
        }
    }

    public void ShowRoleHp(object[] data)
    {
        Creature role = data[0] as Creature;
        ShowBossPage(role);
    }

    private Creature TargetRole;

    private void ShowBossPage(Creature role)
    {
        if (role == null)
            return;
        if(BossRootUID == role.mData.UID)
            return;
        
        if (Battle.Instance.GameMode.ModeType == BattleModeType.Fixed
            && !role.mData.battleItemInfo.isBoss)
        {
            this.Boss_GuildRoot.gameObject.SetActive(false);
            return;
        }
        if (Battle.Instance.GameMode.ModeType == BattleModeType.Guard
            && role.mData.ConfigID != BattleConst.SiLaLiID)
        {
            this.Boss_GuildRoot.gameObject.SetActive(false);
            return;
        }
        else
        {
            this.Boss_GuildRoot.gameObject.SetActive(true);
        }
        WarningBarRoot.gameObject.SetActive(false);
        TargetRole = role;
        BossRootUID = role.mData.UID;
        var info = StaticData.HeroTable.TryGet(role.ConfigID);
        if (info != null)
        {
            UiUtil.SetSpriteInBackground(this.BossIcon, () => info.Head + ".png");
            UiUtil.ShowRollText(this.BossName, LocalizationManager.Stuff.GetText(info.Name), false, true);
            this.BossLevel.text = $"{role.mData.battleItemInfo.lv}";
            UiUtil.SetSpriteInBackground(this.BossJob, () => "Icon_occ" + info.Job + ".png");
        }
        this.BossElement.SetActive(role.mData.Weak != 0);
        UiUtil.SetSpriteInBackground(this.BossElement, () => "element_" + role.mData.Weak + ".png");
        this.RedBlood.fillAmount = role.mData.CurrentHealth.Percent();
        this.WhiteBlood.fillAmount = role.mData.CurrentVim.Percent();
        if (role.mData.IsDead)
        {
            if (Battle.Instance.GameMode.ModeType != BattleModeType.Guard)
                this.Boss_GuildRoot.gameObject.SetActive(false);
        }
        /*var select_hero = SceneObjectManager.Instance.CurSelectHero;
        if (select_hero != null)
        {
            this.Weak.gameObject.SetActive(select_hero.mData.isAnitJob(role.mData.Job));
        }*/
        //this.RedBlood.color = ColorUtil.HexToColor(colors[0]);
        ShowBreakDamgeRoot();
        //this.BloodNum.text = "";
        RefreshBossHpBar(role);
    }

    void ShowStartAnimtion(object[] param)
    {
        GuideManagerV2.Stuff.Notify("Battle.Init");
        
        if (this.BattleStart == null)
            return;
        this.BattleStart.gameObject.SetActive(true);
        //LagacyUtil.PlayAnimation(this.gameObject, "BattlePage_animation");
        Battle.Instance.GameMode.OnPlayBgm();
        //AudioManager.PlayBgmInBackground("sound_fight_bgm");
        TimerMgr.Instance.BattleSchedulerTimer(3f, delegate { this.BattleStart.SetActive(false); }, false, "StartAnim");
        RefreshFriendButton();
    }

    private Creature m_CurSelectedRole = null;
    private int m_CurComboNum = 0;

    void TalkShow(object[] param)
    {
        this.ShowTalk(param[0].ToString(), (int)param[1]);
    }

    void PlayMood(object[] param)
    {
        string anim = param[0].ToString();
        int id = (int)param[1];
        foreach (var VARIABLE in role_items)
        {
            if (VARIABLE.Role != null
                && VARIABLE.Role.ConfigID == id)
            {
                VARIABLE.PlayAnimtion(anim);
                return;
            }
        }
        /*foreach (var VARIABLE in substitute_items)
        {
            if (VARIABLE.Role != null
                && VARIABLE.Role.ConfigID == id)
            {
                VARIABLE.PlayAnimtion(anim);
                return;
            }
        }*/
    }

    private int UICount_Hide = 0;

    private GameObject UIRoot;
    private GameObject HudRoot;

    void ShowUI(object[] param)
    {
        bool vis = (bool)param[0];
        SetUIVis(vis);
    }

    void SetUIVis(bool vis)
    {
        if (UIRoot == null)
        {
            UIRoot = GameObject.Find("UIEngine");
        }
        UIRoot?.SetActive(vis);
        if (HudRoot == null)
        {
            HudRoot = GameObject.Find("BattleCanvas");
        }
        HudRoot?.SetActive(vis);
        this.BreakAnim.gameObject.SetActive(false);
    }

    void RoleDie(object[] param)
    {
        InitRoles();
        if (Focus.IsFocusShow())
        {
            if (BattleManager.Instance.ActorMgr.GetCamp(1).Count <= 0)
            {
                this.Focus.ShowFocus(false);
            }
        }
    }
    
    void SuperSkillStart(object[] param)
    {
        string uid = param[0] as string;
        foreach (var VARIABLE in CurShowItems)
        {
            VARIABLE.Hide();
            TimerMgr.Instance.Remove(VARIABLE.schedualname);
            TalkNum--;
            TalkItems.Add(VARIABLE);
        }
        CurShowItems.Clear();
    }

    int body_index;
    
    private GameObject MyPlayer;

    async void ShowSubCamera(object[] param)
    {
        var vis = (bool)param[0];
        this.m_RTShow.gameObject.SetActive(true);
        if (vis)
        {
            CameraManager.Instance.RTCamera.gameObject.gameObject.SetActive(true);
            this.m_RTShow.transform.DOLocalMoveX(-200f, 0.5f).SetEase(Ease.OutExpo).onComplete = delegate
            {
                //ItemEffect1.SetActive(true);
            };
            //var animator = GameObject.Find("char_throw").transform.GetComponent<Animator>();
            var heros = SceneObjectManager.Instance.GetAllPlayer();
            foreach (var hero in heros)
            {
                if (hero.IsPlayer)
                {
                    hero.GetModelObject.SetActive(true);
                    subCameraAnimator.transform.SetParent(hero.GetBone("root"));
                    subCameraAnimator.transform.localPosition = Vector3.zero;
                    MyPlayer = hero.GetModelObject;
                    //animator.transform.SetParent(hero.BoneRoot);
                    //animator.transform.eulerAngles = hero.transform.eulerAngles;
                    break;
                }
            }
            //animator.transform.localPosition = Vector3.zero;

            //animator.transform.localPosition = new Vector3(0, -0.59f, 0);
            //animator.Play("Char_throw", 0, 0);
            //animator.Update(0);
        }
        else
        {
            //ItemEffect1.SetActive(false);
            CameraManager.Instance.RTCamera.gameObject.SetActive(false);
            var tween = this.m_RTShow.transform.DOLocalMoveX(1200, 0.5f).SetEase(Ease.OutExpo);
            if (MyPlayer != null)
                MyPlayer.SetActive(false);
        }
    }
    
    public async void EnemyComing(object[] data)
    {
        var target = data[0] as Creature;
        var fTime = (float)data[1];
        var angle = (float)data[2];
        //int durT = 3000;
        BattleMono.Instance.StartCoroutine(ShowWaveComing(target, fTime, angle));
    }

    private IEnumerator ShowWaveComing(Creature target, float fTime, float angle)
    {
        this.m_RTShow.gameObject.SetActive(true);
        m_RTName.SetActive(false);
        CameraManager.Instance.RTCamera.gameObject.gameObject.SetActive(true);
        this.m_RTShow.transform.DOLocalMoveX(-200, 0.5f).SetEase(Ease.OutExpo);
        //var animator = subCameraAnimator;
        subCameraAnimator.transform.SetParent(target.GetBone("root"));
        subCameraAnimator.transform.localEulerAngles = new Vector3(0, angle, 0);
        if (target.ConfigID == BattleConst.SiLaLiID)
        {
            subCameraAnimator.transform.localPosition = new Vector3(0, 0.5f, -2f);
        }
        else
        {
            subCameraAnimator.transform.localPosition = Vector3.zero;
        }

        yield return new WaitForSeconds(3f);

        var tween = this.m_RTShow.transform.DOLocalMoveX(1200, 0.5f).SetEase(Ease.OutExpo);
        subCameraAnimator.transform.SetParent(SceneObjectManager.Instance.GetRootTransform);
        yield return new WaitForSeconds(0.5f);
        CameraManager.Instance.RTCamera.gameObject.SetActive(false);
    }

    public async void ShowTargetCamera(object[] data)
    {
        var target = data[0] as Creature;
        var angle = float.Parse(data[1].ToString());
        Transform RTShow = this.m_RTShow;
        Transform RTName = this.m_RTName;
        Transform Root = subCameraAnimator;
        GameObject Camera = CameraManager.Instance.RTCamera.gameObject;
        RTShow.gameObject.SetActive(true);
        RTName.SetActive(false);
        Camera.SetActive(true);
        RTShow.transform.DOLocalMoveX(-200, 0.5f).SetEase(Ease.OutExpo);
        //var animator = subCameraAnimator;
        Root.transform.SetParent(target.GetBone("root"));
        Root.transform.localEulerAngles = new Vector3(0, angle, 0);
        var bone1 = target.GetBone("root");
        var bone2 = target.HeadBone;
        float offset = bone2.position.y - bone1.position.y;
        Root.transform.localPosition = new Vector3(0, offset / 2, -2);
    }

    public async void CloseTargetCamera(object[] data)
    {
        var target = data[0] as Creature;
        Transform RTShow = this.m_RTShow;
        Transform Root = subCameraAnimator;
        GameObject Camera = CameraManager.Instance.RTCamera.gameObject;
        RTShow.DOLocalMoveX(1200, 0.5f).SetEase(Ease.OutExpo);
        Root.SetParent(SceneObjectManager.Instance.GetRootTransform);
        TimerMgr.Instance.BattleSchedulerTimerDelay(0.5f, delegate
        {
            Camera.SetActive(false);
        });
    }
    
    private void BreakQteShow(object[] data)
    {
        //ShowFocusVis(false);
    }

    private void ActorDie(object[] data)
    {
        ResetDefenceInfo();
    }

    private async void ShowDamageBar(object[] data)
    {
        string uid = data[0] as string;
        RecycledGameObject go = data[1] as RecycledGameObject;
        foreach (var VARIABLE in this.role_items)
        {
            if (VARIABLE.Role != null
                && VARIABLE.Role.mData.UID == uid)
            {
                //go.transform.position = VARIABLE.Cell.transform.position;
                go.transform.SetParent(VARIABLE.Cell.transform);
                go.transform.localPosition = new Vector3(0, -20, 0);
                go.transform.localScale = Vector3.one * 0.4f;
                break;
            }
        }
    }

    private async void ShowItemWords(object[] data)
    {
        var words = data[0] as string;
        var bucket = BucketManager.Stuff.Battle;
        var prefab = await bucket.GetOrAquireAsync<GameObject>(words + ".prefab");
        if (prefab == null)
            return;

        BattleMono.Instance.StartCoroutine(ShowWords(prefab));
    }

    private IEnumerator ShowWords(GameObject prefab)
    {
        yield return new WaitForSeconds(0.3f);
        var go = GameObject.Instantiate(prefab);
        go.transform.SetParent(this.m_RTShow.transform);
        go.transform.localPosition = new Vector3(-65, -169f, 0);
        go.transform.localScale = Vector3.one * 8f;
        go.transform.localEulerAngles = new Vector3(0, 0, 8f);
        yield return new WaitForSeconds(3f);
        GameObject.Destroy(go);
    }
    
    private Creature LocalPlayer = null;

    public List<int> item_events = new List<int> { 25900, 25901, 25902 };

    List<Creature> heros;

    //List<BattlePageRole> role_list = new List<BattlePageRole>();

    public Dictionary<int, Creature> MainHeros = new Dictionary<int, Creature>();
    public Dictionary<int, Creature> SubHeros = new Dictionary<int, Creature>();

    private BattleActorManager actorMgr = null;

    //public BattlePageRoleHeadItem FriendItem = null;

    void InitRoles()
    {
        actorMgr = BattleEngine.Logic.BattleManager.Instance.ActorMgr;
        this.heros = actorMgr.GetAllActors();
        MainHeros.Clear();
        SubHeros.Clear();
        var num = this.heros.Count;
        for (int i = 0; i <= this.heros.Count - 1; i++)
        {
            var hero = this.heros[i];
            //Debug.LogError(i + "_" + this.heros[i].RoleItemData.Slot);
            if (hero.IsHero)
            {
                if (this.heros[i].IsMain)
                {
                    if (this.MainHeros.ContainsKey(hero.mData.PosIndex))
                    {
                        this.MainHeros[hero.mData.PosIndex] = this.heros[i];
                    }
                    else
                    {
                        this.MainHeros.Add(hero.mData.PosIndex, this.heros[i]);
                    }
                }
                else if (this.heros[i].IsSub)
                {
                    var key = hero.mData.PosIndex - BattleConst.SubPosStart;
                    if (this.SubHeros.ContainsKey(key))
                    {
                        this.SubHeros[key] = this.heros[i];
                    }
                    else
                    {
                        this.SubHeros.Add(key, this.heros[i]);
                    }
                }
            }
        }
        this.ResetDatas();

        //this.RefreshSubstituteState();
        this.UpdateHeads();
    }

    public void ResetDatas()
    {
        for (int i = 0; i < 3; i++)
        {
            Creature role = null;
            MainHeros.TryGetValue(i, out role);
            this.role_items[i].ResetData(role);
        }
    }

    void CreateHeadView()
    {
        this.Role.gameObject.SetActive(false);
        this.HeroRoot.gameObject.SetActive(false);
        this.HideRoot.gameObject.SetActive(false);
        role_items.Add(HeroItemView1);
        role_items.Add(HeroItemView2);
        role_items.Add(HeroItemView3);

        int index = 0;
        foreach (var VARIABLE in role_items)
        {
            VARIABLE.Init(index, this);
            index++;
        }
        /*for (int i = 0; i < 3; i++)
        {
            var parent = this.Main.transform.Find("RoleItem" + (i + 1));
            var headGo = GameObject.Instantiate(this.RoleItem, parent).gameObject;
            headGo.gameObject.SetActive(true);
            headGo.transform.localPosition = Vector3.one;
            //var temp_head = new HeroItemView(headGo, i, this);
            var temp_head = new HeroItemView();
            this.role_items.Add(temp_head);

            var parent3 = this.Items.transform.Find("SubstitutionPanel" + (i + 1));
            var go3 = GameObject.Instantiate(this.Item.gameObject, parent3);
            go3.transform.localPosition = Vector3.zero;
            var item3 = new BattleItem(go3, this, i);
            //item3.SetData(i);
            battle_items.Add(item3);
        }*/
        /*{
            var parent = this.Main.transform.Find("RoleItem4");
            var headGo = GameObject.Instantiate(this.RoleItem, parent).gameObject;
            headGo.gameObject.SetActive(true);
            headGo.transform.localPosition = Vector3.one;
            FriendItem = new BattlePageRoleHeadItem(headGo, 3, this, true);
        }*/
    }

    public void ResetRoleList(object[] param)
    {
        InitRoles();
        if (this.last_camera_sel != null)
        {
            CameraClick(this.last_camera_sel);
            this.last_camera_sel = null;
        }
        this.UpdateHeads();
    }

    public SkeletonGraphic GetHeadSpineMove => HeadSpineMove;
    public Image GetHeadIconMove => HeadIconMove;
    public Mask GetSubstitutionMove => SubstitutionMove;

    public void PlaySpineAnimation(SkeletonGraphic graphic, int ConfigID, bool isJoin)
    {
        int index = ConfigID * 100;
        if (isJoin)
            index += 5;
        else
        {
            index += 6;
        }
        var data = StaticData.RolechatTable.TryGet(index);
        if (data == null)
            return;

        //graphic.AnimationState.SetAnimation(0, data.Mood, true);
        graphic.AnimationState.AddAnimation(0, "idle", true, 0);
    }

    public void StopSpineAnimation(SkeletonGraphic graphic)
    {
        if (graphic == null
            || graphic.AnimationState == null)
        {
            return;
        }
        graphic.AnimationState.SetEmptyAnimation(0, 0);
    }

    public void PlayAnimtion(SkeletonGraphic SpineGraphic, string anim, string name)
    {
        if (SpineGraphic == null
            || SpineGraphic.AnimationState == null)
            return;
        var animtion = SpineGraphic.SkeletonData.FindAnimation(anim);
        if (animtion == null)
        {
            //Debug.LogError(name + "缺少动画:" + anim);
            return;
        }
        SpineGraphic.AnimationState.AddAnimation(0, anim, true, 0);
    }

    public HeroItemView last_select1 = null;
    
    private GameObject last_camera_sel = null;

    void UpdateHeads()
    {
        for (int i = 0; i < 3; i++)
        {
            this.role_items[i].UpdateData();
        }
    }

    public void AddBuffer(object[] param)
    {
        AddBuffer((string)param[0], (int)param[1], (int)param[2]);
    }

    public void RemoveBuffer(object[] param)
    {
        RemoveBuffer((string)param[0], (int)param[1]);
    }

    public void ShowBufferTips(object[] param)
    {
        this.m_BufferRoot.SetActive(true);
        this.m_BufferName.text = "灼烧";
        this.m_BufferDes.text = "目标受到灼烧效果，持续3秒";
        BattleTimer.Instance.DelayCall(2f, delegate(object[] objects) { this.m_BufferRoot.SetActive(false); });
    }

    public void AddBuffer(string roleid, int uid, int configid)
    {
        if (this.role_items == null)
        {
            return;
        }
    }

    public void RemoveBuffer(string roleid, int uid)
    {
        if (this.role_items == null)
        {
            return;
        }
    }
    
    public void CameraClick(GameObject go)
    {
        var index = int.Parse(go.transform.name);
        if (index >= this.MainHeros.Count)
            return;
        var role = this.MainHeros[index];
        if (role != null)
        {
            var player = SceneObjectManager.Instance.GetSelectPlayer();
            if (player != null)
            {
                if (player == role)
                    return;
                player.OnUnSelected();
            }
            CameraManager.Instance.CameraProxy.SetTarget(role.transform, role);
            SceneObjectManager.Instance.SetSelectPlayer(role);
        }
        this.UpdateHeads();
    }
    
    private int SuperCDTime = 0;

    public void CastSuperSkill(Creature player, bool bCheck = true)
    {
        if (SuperCDTime > 0
            || player.mData.IsWaitSPSkillState)
        {
            return;
        }
        if (player.mData.CurrentMp.Value < 200)
        {
            if (!bCheck)
                this.ShowTalk(LocalizationManager.Stuff.GetText("mana_not_enough"), player.ConfigID);
        }
        else
        {
            //这里先去掉cd
            SuperCDTime = 0;
            BattleManager.Instance.SendSpendSPSkill(player.ID);
        }
    }

    private int last_num = 0;

    void UpdateHpMp(object[] data)
    {
        this.UpdateHeads();
        var role = SceneObjectManager.Instance.GetSelectPlayer();
        if (role == null
            || role.mData.IsDead)
        {
            foreach (var VARIABLE in this.role_items)
            {
                if (VARIABLE.Role != null
                    && !VARIABLE.Role.mData.IsDead)
                {
                    this.CameraClick(VARIABLE.Cell);
                    break;
                }
            }
        }
        if (data == null)
        {
            return;
        }
        Creature target = data[0] as Creature;
        if (target.mData.UID == BossRootUID)
        {
            RefreshBossHpBar(target);
        }
        if (!target.mData.isAtker)
        {
            PlotPipelineControlManager.Stuff.StartPipelineAsync(Battle.Instance.CopyId, EPlotEventType.KillAppointMonsterId, new PlotParams() { MonsterId = target.mData.ConfigID, Percent = (int)(target.mData.CurrentHealth.Percent() * 100) });
        }

        Focus.UpdateHp(target);

        //更新守卫血量
        if (FixedModeHeroRoot.gameObject.activeSelf)
        {
            if (FixedModeTarget == target)
            {
                var percent = target.mData.CurrentHealth.Percent();
                if (percent <= 0.1f)
                {
                    if (FixedHeroHpBar.fillAmount > 0.1f)
                    {
                        ShowMiddleTips(LocalizationManager.Stuff.GetText("m1_battlepage_diamondhplow4"));
                    }
                }
                else if (percent <= 0.3f)
                {
                    if (FixedHeroHpBar.fillAmount > 0.3f)
                    {
                        ShowMiddleTips(LocalizationManager.Stuff.GetText("m1_battlepage_diamondhplow3"));
                    }
                }
                else if (percent <= 0.5f)
                {
                    if (FixedHeroHpBar.fillAmount > 0.5f)
                    {
                        ShowMiddleTips(LocalizationManager.Stuff.GetText("m1_battlepage_diamondhplow2"));
                    }
                }
                else if (percent <= 0.7f)
                {
                    if (FixedHeroHpBar.fillAmount > 0.7f)
                    {
                        ShowMiddleTips(LocalizationManager.Stuff.GetText("m1_battlepage_diamondhplow1"));
                    }
                }
                FixedHeroHpBar.fillAmount = percent;
                HPDes.text = $"{FixedModeTarget.mData.CurrentHealth.Value}/{FixedModeTarget.mData.CurrentHealth.MaxValue}";
            }
        }
    }

    void RefreshBossHpBar(Creature target)
    {
        int max_num = 1;
        MonsterRow monster = StaticData.MonsterTable.TryGet(target.mData.battleItemInfo.monsterid);
        if (monster != null)
        {
            if (monster.bloodNum != null
                && monster.bloodNum > 0)
            {
                max_num = monster.bloodNum;
            }
        }
        float interval = 1f / max_num;
        int cur_num = Mathf.CeilToInt(target.mData.CurrentHealth.Percent() / interval);
        float percent = (target.mData.CurrentHealth.Percent() - (cur_num - 1) * interval) / interval;
        this.RedBlood.fillAmount = (target.mData.CurrentHealth.Percent() - (cur_num - 1) * interval) / interval;
        this.WhiteBlood.fillAmount = target.mData.CurrentVim.Percent();
        if (target.mData.IsDead)
        {
            if (Battle.Instance.GameMode.ModeType != BattleModeType.Guard)
                this.Boss_GuildRoot.gameObject.SetActive(false);
        }
        //HudUtil.DoHpFade(target, this.FadeBlood, 1f);
        if (max_num > 1
            && cur_num != last_num)
        {
            this.BloodNum.text = $"X{cur_num}";
            last_num = cur_num;
            this.FadeBlood.fillAmount = 1f;

            if (cur_num <= 1)
            {
                UiUtil.SetActive(this.NextBlood.gameObject, false);
                this.RedBlood.color = ColorUtil.HexToColor(colors[0]);
            }
            else
            {
                UiUtil.SetActive(this.NextBlood.gameObject, true);
                int index = (cur_num - 1) % 5;
                this.NextBlood.color = ColorUtil.HexToColor(colors[(cur_num - 2) % 5]);
                this.RedBlood.color = ColorUtil.HexToColor(colors[index]);
            }
        }

        if (max_num == 1)
        {
            UiUtil.SetActive(this.NextBlood.gameObject, false);
            this.RedBlood.color = ColorUtil.HexToColor(colors[0]); 
        }
        HudUtil.DoHpFade(percent, this.FadeBlood, 1f, target.mData.UID);
    }

    private string[] colors = new[] {"d63837", "e75dff", "5d70ff", "32ddd5", "5dff90"};

    void UpdateEnergy(object[] data)
    {
        this.UpdateHeads();
    }

    public override void OnNavigatedFrom(PageNavigateInfo info)
    {
        //if (info.terminalOperation == NavigateOperation.Back)
        {
            //Leave();
        }
    }
    
    public Creature FixedModeTarget = null;

    private void ShowDefenceInfo()
    {
        if (Battle.Instance.mode.ModeType != BattleModeType.Defence
            && Battle.Instance.mode.ModeType != BattleModeType.Fixed)
        {
            this.DefenceInfo.SetActive(false);
        }
        if (Battle.Instance.mode.ModeType != BattleModeType.Fixed)
        {
            this.FixedModeHeroRoot.SetActive(false);
        }
        else
        {
            var list = SceneObjectManager.Instance.GetAllCreatures();
            foreach (var VARIABLE in list)
            {
                if (VARIABLE.IsDefenceTarget())
                {
                    this.FixedModeTarget = VARIABLE;
                    var info = StaticData.HeroTable.TryGet(VARIABLE.ConfigID);
                    if (info != null)
                    {
                        UiUtil.SetSpriteInBackground(FixedHeroIcon, () => info.Head + ".png");
                    }
                    
                    HPDes.text = $"{FixedModeTarget.mData.CurrentHealth.Value}/{FixedModeTarget.mData.CurrentHealth.MaxValue}";
                    break;
                }
            }
            this.FixedModeHeroRoot.SetActive(true);
        }
    }
    
    public List<DefFocusEnemyView> monsterHeads = new List<DefFocusEnemyView>();

    private void ResetDefenceInfo()
    {
        if (!this.DefenceInfo.gameObject.activeSelf)
            return;

        foreach (var VARIABLE in monsterHeads)
        {
            if (VARIABLE == DefFocusEnemyView.LastSelected)
            {
                if (VARIABLE.data != null && VARIABLE.data.mData.IsDead)
                {
                    DefFocusEnemyView.HideLastSelect();
                }
            }
            
            VARIABLE.SetActive(false);
        }
        this.MonsterRoot.transform.GetSizeDeltaEx(out float x, out float y);
        int index = 0;
        var list = BattleManager.Instance.ActorMgr.GetDefLst();
        bool allDie = true;
        foreach (var VARIABLE in list)
        {
            if (VARIABLE.mData.IsDead)
                continue;
            if (monsterHeads.Count < (index + 1))
            {
                var go = GameObject.Instantiate(MonsterHead.gameObject, MonsterHead.transform.parent);
                go.transform.SetAsLastSibling();
                var item = go.GetComponent<DefFocusEnemyView>();
                monsterHeads.Add(item);
            }
            var item_info = monsterHeads[index];
            item_info.SetData(VARIABLE);
            index++;

            allDie = false;
        }
        
        this.DefenceRootBar.SetActive(!allDie);
        
    }

    private void UpdateDefenceInfo()
    {
        if (!this.DefenceInfo.gameObject.activeSelf)
            return;
        this.MonsterRoot.transform.GetSizeDeltaEx(out float x, out float y);
        var mode = Battle.Instance.mode;
        foreach (var VARIABLE in monsterHeads)
        {
            if (VARIABLE.data == null || !VARIABLE.gameObject.activeSelf)
                continue;
            if (VARIABLE.data.mData.IsDead)
            {
                VARIABLE.SetActive(false);
                continue;
            }
            if (VARIABLE.gameObject == null)
                continue;
            var dis = Vector3.Distance(VARIABLE.data.mData.GetPosition(), mode.LinePos);
            var percent = 1 - dis / mode.MaxDistance;
            var pos = new Vector3(0, y * percent - 0.5f * y, 0);
            VARIABLE.transform.localPosition = pos;
        }
    }

    public void PreFocus(Creature data)
    {
        foreach (var VARIABLE in monsterHeads)
        {
            if (VARIABLE.data == data)
            {
                VARIABLE.Focus();
                break;
            }
        }
    }

    public void CleanFocus()
    {
        foreach (var VARIABLE in monsterHeads)
        {
            VARIABLE.NoFocus();
        }
    }

    public void Leave()
    {
        ShowBossPage(false);
        this.AutoArrow.DOKill();
        this.m_RTShow.gameObject.SetActive(false);
        this.RemoveListener();
        this.Focus.OnDestroy();
        this.m_CurComboNum = 0;
        this.m_CurSelectedRole = null;
        TimerMgr.Instance.Remove("BattleTime");
        TimerMgr.Instance.Remove("BattleUpdate");
        TimerMgr.Instance.Remove("ItemCDTimer");
        TimerMgr.Instance.Remove("FocusCDTimer");
        TimerMgr.Instance.Remove("StartAnim");
    }

    public void StartGame()
    {
        Enter(null);
    }

    public void EndGame()
    {
        Leave();
    }

    private void InitItems()
    {
        var id = BattleDataManager.Instance.GetItemID(1);
        if (id == 0)
        {
            this.BreakBattleItemView.SetData(null);
        }
        else
        {
            this.BreakBattleItemView.SetData(new BattleItemData(){ID = id, type = 1});
        }
        
        id = BattleDataManager.Instance.GetItemID(3);
        if (id == 0)
        {
            this.DefenceBattleItemView.SetData(null);
        }
        else
        {
            this.DefenceBattleItemView.SetData(new BattleItemData(){ID = id, type = 3});
        }
        
        id = BattleDataManager.Instance.GetItemID(4);
        if (id == 0)
        {
            this.FocusBattleItemView.SetData(null);
        }
        else
        {
            this.FocusBattleItemView.SetData(new BattleItemData(){ID = id, type = 4});
        }
    }
    private async void Enter(PageNavigateInfo info)
    {
       InitItems();
        IsBufferMonsterTips = false;
        //this.UISpineUnitTest.Bind(1501010, ESpineTemplateType.HalfModel);
        this.m_StartView.transform.Find("words1").GetComponent<Image>().enabled = true;
        this.WaveTips.text = "";
        BreakAnims.Clear();
        BreakAnims.Add(BreakAnim1.gameObject);
        BreakAnims.Add(BreakAnim2.gameObject);
        BreakAnims.Add(BreakAnim3.gameObject);
        foreach (var VARIABLE in BreakAnims)
        {
            VARIABLE.SetActive(false);
        }
        //this.Bottom.transform.localPosition = new Vector3(0, 100, 0);
        this.UICount_Hide = 0;
        //this.HideButton.SetActive(!Battle.Instance.IsDreamEscapeMode);
        //this.Items.SetActive(!(Battle.Instance.IsDreamEscapeMode || Battle.Instance.IsArenaMode));
        PauseBtn.gameObject.SetActive(!GuideManagerV2.Stuff.IsExecutingForceGuide && !Battle.Instance.IsZoneBattle);
        actorMgr = BattleEngine.Logic.BattleManager.Instance.ActorMgr;
        this.Spine.gameObject.SetActive(false);
        this.Role.gameObject.SetActive(false);
        this.HeroRoot.gameObject.SetActive(false);
        this.HideRoot.gameObject.SetActive(false);
        BreakRoot.gameObject.SetActive(false);
        Boss_GuildRoot.gameObject.SetActive(false);

        if (Battle.Instance.IsArenaMode)
        {
            AutoSuperSkill = true;
        }
        else
        {
            AutoSuperSkill = BattleAutoFightManager.Instance.AutoState != AutoState.None;
        }
        RefreshAutoButton();
        this.DreamEscapeBg.SetActive(Battle.Instance.IsDreamEscapeMode);

        this.Init();
        this.AddListenner();
        this.SuperCDTime = 0;
        BossCreature = null;
        TimerMgr.Instance.BattleSchedulerTimer(0.1f, delegate
                        {
                            TimeSpan ts = TimeSpan.FromMilliseconds((int)((BattleTimeManager.Instance.BattleTime - BattleTimeManager.Instance.CurrentBattleTime) * 1000));
                            this.BattleTime.text = ts.ToMillionSecond();

                            if (SuperCDTime > 0)
                            {
                                SuperCDTime--;
                            }
                        }, true, "BattleTime"
        );
        this.Suspend.gameObject.SetActive(!GuideManagerV2.Stuff.IsExecutingForceGuide);
        for (int i = 0; i < BattleDataManager.Instance.Items.Count; i++)
        {
            if (i + 1 > battle_items.Count)
                break;
            battle_items[i].SetData(BattleDataManager.Instance.Items[i]);
        }
        TimerMgr.Instance.BattleSchedulerTimer(0.1f, delegate { OnUpdateTime(0.1f); }, true, "BattleUpdate");
        
        if (Battle.Instance.IsGoldMode)
        {
            GoldDropRoot.SetActive(true);
            GoldFXRoot.SetActive(true);
            RefreshGoldMode();
        }
        else
        {
            GoldDropRoot.SetActive(false);
            GoldFXRoot.SetActive(false);
        }
        subCameraAnimator = GameObject.Find("char_throw").transform;
        PassMonsterRoot.SetActive(false);
        var stage_info = StaticData.StageTable.TryGet(Battle.Instance.CopyId);
        if (stage_info == null)
            return;
        this.Stage.text = LocalizationManager.Stuff.GetText(stage_info.desLv);
        this.Wave.text = Battle.Instance.Wave.ToString() + "/" + Battle.Instance.MaxWave.ToString();
        Root.gameObject.SetActive(false);

        BattleMono.Instance.StartCoroutine(MyCoroutine());
    }
    
    private IEnumerator MyCoroutine()
    {
        // 等待2秒
        yield return new WaitForSeconds(0.2f);

        // 执行一些操作
        Root.gameObject.SetActive(true);
        ShowDefenceInfo();

        this.ItemRoot.SetActive(!Battle.Instance.IsArenaMode);
        this.Focus.SetActive(!Battle.Instance.IsArenaMode);
    }

    //private int DreamEscapEffectID;

    public override void OnNavigatedTo(PageNavigateInfo info)
    {
        if (info.terminalOperation == NavigateOperation.Back)
        {
            var lastPage = info.from.name;
            if (lastPage == "BattleSettingPage")
            {
                // 如果是从战斗设置页面返回，则战斗已经被暂停，需要被恢复
                this.ResumeBattle();
            }
        }
    }

    public void SetUIVis(List<EGuideBattleUIType> uiTypes, bool vis)
    {
        foreach (var VARIABLE in uiTypes)
        {
            switch (VARIABLE)
            {
                case EGuideBattleUIType.TacticItem1:
                {
                    this.BreakBattleItemView.SetActive(vis);
                    break;
                }
                case EGuideBattleUIType.TacticItem2:
                {
                    this.FocusBattleItemView.SetActive(vis);
                    break;
                }
                case EGuideBattleUIType.TacticItem3:
                {
                    this.DefenceBattleItemView.SetActive(vis);
                    break;
                }
                case EGuideBattleUIType.BackBtn:
                {
                    this.PauseBtn.SetActive(vis);
                    break;
                }
                case EGuideBattleUIType.FocusMonsters:
                {
                    this.DefenceInfo.SetActive(vis);
                    this.Focus.SetActive(vis);
                    break;
                }
                case EGuideBattleUIType.DefenceBtn1:
                {
                    role_items[0].SetDefenceVis(vis);
                    break;
                }
                case EGuideBattleUIType.DefenceBtn2:
                {
                    role_items[1].SetDefenceVis(vis);
                    break;
                }
                case EGuideBattleUIType.DefenceBtn3:
                {
                    role_items[2].SetDefenceVis(vis);
                    break;
                }
                case EGuideBattleUIType.AutoBtn:
                {
                    Auto1.SetActive(vis);
                    break;
                }
                case EGuideBattleUIType.JobIntro:
                {
                    JobAnti.SetActive(vis);
                    break;
                }
            }
        }
    }

    /// <summary>
    /// 切换到某个英雄
    /// </summary>
    /// <param name="index">0， 1， 2</param>
    public void ChangeSelect(int index)
    {
        foreach (var VARIABLE in role_items)
        {
            if (index == VARIABLE.index)
            {
                if(VARIABLE.Role != null)
                    SceneObjectManager.Instance.SetSelectPlayer(VARIABLE.Role);
                break;
            }
        }
    }

    
    private List<GameObject> autohide_lists = new List<GameObject>();

    private void Update()
    {
        UpdateDefenceInfo();
        if (WarningBarRoot.gameObject.activeSelf)
        {
            if (WarningRemTime > 0)
            {
                WarningRemTime -= Time.deltaTime;
                if (WarningRemTime <= 0)
                {
                    WarningBarRoot.gameObject.SetActive(false);
                }
                WarningBar.fillAmount = WarningRemTime / WarningTotalTime;
            }
        }
        if (InputProxy.TouchDown())
        {
            foreach (var VARIABLE in autohide_lists)
            {
                if (VARIABLE.activeSelf
                    && !HitTest(VARIABLE.transform))
                {
                    TimerMgr.Instance.BattleSchedulerTimerDelay(0.2f, delegate { VARIABLE.SetActive(false); });
                }
            }

            if (FocusEnemyView.LastSelected != null && !HitTest(FocusEnemyView.LastSelected.transform))
            {
                FocusEnemyView.HideLastSelect();

                if (!HitTest(this.Focus.transform))
                {
                    DamageManager.Instance.SetVisible(true);
                    HudManager.Instance.SetVis(true);
                    BattleSpecialEventManager.Instance.CloseDark(CameraManager.Instance.MainCamera);
                }
            }

            if (DefFocusEnemyView.LastSelected != null && !HitTest(DefFocusEnemyView.LastSelected.transform))
            {
                DefFocusEnemyView.HideLastSelect();
                if (!HitTest(this.DefenceInfo.transform))
                {
                    DamageManager.Instance.SetVisible(true);
                    HudManager.Instance.SetVis(true);
                    BattleSpecialEventManager.Instance.CloseDark(CameraManager.Instance.MainCamera);
                }
            }
        }
    }

    /** 是否点到目标物 */
    public bool HitTest(Transform tf)
    {
        List<RaycastResult> raycastResults = new List<RaycastResult>();
        PointerEventData p = new PointerEventData(EventSystem.current);
        p.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        EventSystem.current.RaycastAll(p, raycastResults);
        foreach (var raycastResult in raycastResults)
        {
            var node = raycastResult.gameObject.transform;
            while (node != null)
            {
                if (node == tf)
                {
                    return true;
                }
                node = node.parent;
            }
        }
        return false;
    }
    
    public int TalkNum = 0;
    public List<TalkItemInfo> TalkItems = new List<TalkItemInfo>();
    public TalkItemInfo LastTalk = null;
    public TalkItemInfo FirstTalk = null;
    public int count = 0;
    public List<TalkItemInfo> CurShowItems = new List<TalkItemInfo>();

    public void ShowTalk(string des, int ConfigID, bool isPlot = false, string Head = "")
    {
        if (ConfigID == 0
            && string.IsNullOrEmpty(Head))
            return;
        if (TalkNum >= 2
            && FirstTalk != null)
        {
            if (FirstTalk.isPlot)
                return;
            FirstTalk.Hide();
            FirstTalk.isUp = false;
            TalkNum--;
            TalkItems.Add(FirstTalk);
            TimerMgr.Instance.Remove(FirstTalk.schedualname);
        }
        bool bHold = false;
        if (CurShowItems.Count > 0)
        {
            foreach (var VARIABLE in CurShowItems)
            {
                if (VARIABLE.ConfigID == ConfigID)
                {
                    VARIABLE.hold = true;
                    VARIABLE.SetContent(des, ConfigID, isPlot);
                    //LastTalk = VARIABLE;
                    return;
                }
            }
        }
        if (TalkItems.Count == 0)
        {
            var talkitem_temp = new TalkItemInfo();
            var go = GameObject.Instantiate(this.TalkItem.gameObject);
            go.transform.SetParent(this.TalkItem.transform.parent);
            talkitem_temp.Init(go);
            TalkItems.Add(talkitem_temp);
        }
        var talkitem = TalkItems[0];
        TalkItems.RemoveAt(0);
        talkitem.Show();
        talkitem.isUp = true;
        //talkitem.isPlot = false;
        if (string.IsNullOrEmpty(Head))
            talkitem.SetContent(des, ConfigID, isPlot);
        else
        {
            talkitem.SetContentTalk(des, Head, isPlot);
        }

        foreach (var VARIABLE in CurShowItems)
        {
            if (talkitem != VARIABLE)
            {
                if (VARIABLE.isUp)
                {
                    LastTalk.Down();
                    LastTalk.isUp = false;
                }
            }
        }
        TalkNum++;
        LastTalk = talkitem;
        FirstTalk = LastTalk;
        count++;
        string schedualname = talkitem.go.name + count;
        talkitem.schedualname = schedualname;
        CurShowItems.Add(talkitem);
        TimerMgr.Instance.BattleSchedulerTimer(3f, delegate
                        {
                            if(!Battle.HasInstance())
                                return;
                            if (talkitem.hold)
                            {
                                TimerMgr.Instance.BattleSchedulerTimer(3f, delegate
                                                {
                                                    if(!Battle.HasInstance() || talkitem == null)
                                                        return;
                                                    CurShowItems.Remove(talkitem);
                                                    TalkNum--;
                                                    talkitem.Back();
                                                    talkitem.hold = false;
                                                    talkitem.isUp = false;
                                                    TalkItems.Add(talkitem);
                                                    if (FirstTalk == talkitem)
                                                        FirstTalk = null;
                                                }
                                );
                                return;
                            }
                            CurShowItems.Remove(talkitem);
                            TalkNum--;
                            talkitem.Back();
                            talkitem.isUp = false;
                            TalkItems.Add(talkitem);
                            if (FirstTalk == talkitem)
                                FirstTalk = null;
                        }, false, schedualname
        );
    }

    public void ShowLive2D(object[] data)
    {
        var creature = data[0] as Creature;
        this.Role.gameObject.SetActive(false);
        this.HeroRoot.gameObject.SetActive(false);
        this.HideRoot.gameObject.SetActive(false);
        TimerMgr.Instance.BattleSchedulerTimer(2f, delegate
                        {
                            this.Role.gameObject.SetActive(true);
                            this.HeroRoot.gameObject.SetActive(true);
                            this.HideRoot.gameObject.SetActive(true);
                            //HideSubstitute(true);
                        }
        );
        this.UISpineUnit.Set(creature.ConfigID, ESpineTemplateType.HalfModel);
        //this.UISpineUnit.Bind(creature.ConfigID);
        TimerMgr.Instance.BattleSchedulerTimer(0.4f, delegate
                        {
                            this.Spine.gameObject.SetActive(true);
                            LagacyUtil.PlayAnimation(this.gameObject, "BattlePageroledraw_insert");
                        }
        );

        TimerMgr.Instance.BattleSchedulerTimer(1.4f, delegate
        {
            this.Spine.SetActive(false);
        });
    }

    public void ShowOrderDefencePanels(bool vis)
    {
        this.HeroRoot.SetActive(vis);
        this.HideRoot.SetActive(vis);
    }

    private void CatEnter(object[] data)
    {
        foreach (var VARIABLE in BattleEngine.Logic.BattleManager.Instance.ActorMgr.GetAllActors())
        {
            if (VARIABLE.IsMain
                && !VARIABLE.mData.IsDead)
                VARIABLE.Trigger(EmojiEvent.CatEnter);
        }
    }

    public void UseItemTalk()
    {
        foreach (var VARIABLE in BattleEngine.Logic.BattleManager.Instance.ActorMgr.GetAllActors())
        {
            if (VARIABLE.IsMain
                && !VARIABLE.mData.IsDead)
                VARIABLE.Trigger(EmojiEvent.UseItem);
        }
    }

#region 好友助战
    private void RefreshFriendButton()
    {
        if (!Battle.Instance.IsFight
            || Battle.Instance.GameMode.AssistBattleInfo == null
            || Battle.Instance.GameMode.AssistBattleInfo.heroes == null)
        {
            FriendRoot.SetActive(false);
            return;
        }

        FriendRoot.SetActive(true);
        Button btn = FriendButton.GetComponent<Button>();
        btn.enabled = true;
        FriendCD.SetActive(false);
        CombatActorEntity friendActor = null;
        List<CombatActorEntity> teamList = BattleLogicManager.Instance.BattleData.GetTeamList(BattleConst.ATKTeamID);
        for (int i = 0; i < teamList.Count; i++)
        {
            if (teamList[i].PosIndex != BattleConst.FriendPosIndex
                || teamList[i].IsDead)
            {
                continue;
            }
            friendActor = teamList[i];
        }
        if (friendActor == null)
            return;
        var role = SceneObjectManager.Instance.Find(friendActor.UID);
        if (role != null)
        {
            //FriendItem.ResetData(role); 
            UiUtil.SetSpriteInBackground(this.FriendHead, () => "Icon_" + friendActor.ConfigID + ".png");
        }
    }

    public void SendFriendToBattle()
    {
        if (BattleAutoFightManager.Instance.FriendIsUsed)
        {
            //ToastManager.Show("已用过");
            return;
        }

        BattleAutoFightManager.Instance.FriendIsUsed = true;
        Button btn = FriendButton.GetComponent<Button>();
        btn.enabled = false;
        BattleManager.Instance.ExecuteFreindToBattle();
        //SetAssistCD(BattleUtil.GetGlobalK(GlobalK.Friend_To_Battle_38));
        FriendCDTime.enabled = true;
        FriendCD.gameObject.SetActive(true);
        FriendCDTime.text = "";
    }

    public async Task ShowItemPanel()
    {
        this.ItemRoot.gameObject.SetActive(true);
    }
    
    public async Task HideItemPanel()
    {
        this.ItemRoot.gameObject.SetActive(false);
    }

    public void SetAssistCD(float cd)
    {
        FriendCD.gameObject.SetActive(true);
        FriendCDTime.enabled = true;
        int waitTime = (int)cd;
        FriendCDTime.text = waitTime.ToString() + "s";
        TimerMgr.Instance.BattleSchedulerTimer(1, () =>
                        {
                            waitTime -= 1;
                            FriendCDTime.text = LocalizationManager.Stuff.GetText("m1_battlePage_Assist") + " :" + StrBuild.Instance.ToStringAppend(Mathf.Max(0, waitTime).ToString(), "s");
                            if (waitTime <= 0)
                            {
                                FriendCD.enabled = false;
                                FriendCDTime.enabled = false;
                                TimerMgr.Instance.Remove("RefreshAssistFriendTime");
                            }
                        }, true, "RefreshAssistFriendTime"
        );
    }

    public void CloseFriendTime()
    {
        TimerMgr.Instance.Remove("RefreshAssistFriendTime");
        FriendCDTime.text = "";
    }
#endregion

#region 金币副本
    private int _currentGold = 0;
    public List<Bezier> goldFlyLst = new List<Bezier>();
    public GameObject GetGoldRoot
    {
        get { return GoldDrop.gameObject; }
    }

    private void RefreshGoldMode()
    {
        PickUpFlyUnit.SetActive(false);
        _currentGold = 0;
        GoldValue.text = "0";
        EventManager.Instance.RemoveListener<int>("GoldModeGetGold", RefreshGoldValue);
        EventManager.Instance.AddListener<int>("GoldModeGetGold", RefreshGoldValue);
    }

    private void RefreshGoldValue(int value)
    {
        GoldValueLabel.DOKill();
        GoldValue.DOKill();
        GoldValue.transform.localScale = Vector3.one;
        GoldValue.text = _currentGold.ToString();
        GoldValue.DOCounter(_currentGold, value, 0.5f);
        var sequence = DOTween.Sequence().Append(GoldValueLabel.transform.DOScale(1.3f, 0.25f)).Append(GoldValueLabel.transform.DOScale(1.0f, 0.25f));
        sequence.Play();
        _currentGold = value;
        Fx_ui_gold_get.GetComponent<UIFxSimple>().Play();
    }

    public void ExecuteGetGoldFx(Vector3 originPos)
    {
        Bezier tempPickFly = null;
        for (int i = 0; i < goldFlyLst.Count; i++)
        {
            if (goldFlyLst[i].isFlying)
            {
                continue;
            }
            tempPickFly = goldFlyLst[i];
        }
        if (tempPickFly == null)
        {
            GameObject obj = Instantiate(PickUpFlyUnit.gameObject);
            TransformUtil.InitTransformInfo(obj.gameObject, GoldFXRoot);
            tempPickFly = obj.GetComponent<Bezier>();
            goldFlyLst.Add(tempPickFly);
        }
        tempPickFly.SetActive(true);
        Vector3 beginPos = new Vector3(originPos.x - Screen.width * 0.5f, originPos.y - Screen.height * 0.5f, 0);
        Vector3 endPos = new Vector3(GoldImage.transform.position.x - Screen.width * 0.5f, GoldImage.transform.position.y - Screen.height * 0.5f, 0);
        tempPickFly.ExerciseToTargetPos(beginPos, endPos);
    }
    #endregion
}