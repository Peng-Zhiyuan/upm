/* Created:Loki Date:2023-01-17*/

using System;
using System.Threading.Tasks;
using BattleSystem.ProjectCore;
using System.Collections.Generic;
using BattleEngine.Logic;
using Unity.Mathematics;
using UnityEngine;

/// <summary>
/// 技能展示参数
/// </summary>
public class SkillViewModeParam
{
    public int HeroID;
    private int teamID = 1502014; //队友
    public int TeamID
    {
        get
        {
            teamID = HeroID == 1502014 ? 1501010 : 1502014;
            return teamID;
        }
    }
}

[BattleModeClass(BattleModeType.SkillView)]
public class SkillViewMode : Mode
{
    private SkillViewModeParam _SkillViewModeParam = null;
    private CombatActorEntity actorEntity;
    private CombatActorEntity enemyEntity;
    private CombatActorEntity friendEntity;
    private CombatActorEntity currentTargetEntity;
    private Vector3 actorInitPos = Vector3.zero;
    private Vector3 InitPos = Vector3.zero;
    private Vector3 hitePos = new Vector3(20000, 200000, 20000);
    private float currentATKDistance;

    public override string GetPageName()
    {
        return "HeroSkillViewPage";
    }

    public override BattleModeType ModeType
    {
        get { return BattleModeType.SkillView; }
    }

    public override void OnCreate(PveModeParam pveParam, object modeParam, Battle battle, CreateBattleResponse response)
    {
        base.OnCreate(pveParam, modeParam, battle, response);
        _SkillViewModeParam = modeParam as SkillViewModeParam;
    }

    public override Task<bool> PreloadRes(BattleCoreParam param, Action<float> onProgress, List<int> otherHero = null)
    {
        List<int> skillViewHero = new List<int>() { _SkillViewModeParam.HeroID, _SkillViewModeParam.TeamID };
        return base.PreloadRes(param, onProgress, skillViewHero);
    }

    public override async Task<BattlePlayer> RequestPlayerAsync()
    {
        return null;
    }

    public override async Task BattleReady()
    {
        await battle.SpawnMonsters(true);
        actorEntity = BattleLogicManager.Instance.BattleData.atkActorLst[0];
        friendEntity = BattleLogicManager.Instance.BattleData.atkActorLst[1];
        enemyEntity = BattleLogicManager.Instance.BattleData.defActorLst[0];
        actorInitPos = Vector3.zero;
        InitPos = Vector3.zero;
        HeroRow heroRow = StaticData.HeroTable[actorEntity.ConfigID];
        currentATKDistance = heroRow.Range;
        RefreshActorSenceInfo(enemyEntity);
        HeroSkillViewPage page = UIEngine.Stuff.FindPage("HeroSkillViewPage") as HeroSkillViewPage;
        page.delegatePlaySkill = (int skillID) => { PlaySkillID(actorEntity, skillID); };
        page.delegateModeBack = () => { QuitSkillViewMode(); };
        page.delegateGoToMainPage = () => { QuitSkillViewMode(true); };
        page.RefreshPage(_SkillViewModeParam.HeroID);
        GameObject skillViewCamera = GameObject.Find("SkillViewMainCamera");
        CameraManager.Instance.ReplaceMainCamera(skillViewCamera.GetComponent<Camera>());
        EventManager.Instance.SendEvent("ChangeBattleFixCamera", new CameraTransInfo() { pos = new Vector3(3, 2, -5), rotation = Quaternion.Euler(0, -25, 0) });
    }

    public override async Task OnWaveExectue()
    {
        List<CombatActorEntity> actorEntityLst = BattleLogicManager.Instance.BattleData.allActorLst;
        for (int i = 0; i < actorEntityLst.Count; i++)
        {
            actorEntityLst[i].isOpenAITree = false;
        }
        RefreshBattleSence();
    }

    private void RefreshBattleSence()
    {
        var actorLst = BattleLogicManager.Instance.BattleData.allActorLst;
        for (int i = 0; i < actorLst.Count; i++)
        {
            actorLst[i].ClearTargetInfo();
            actorLst[i].ClearAllOTNum();
            actorLst[i].OnClearBuff();
            actorLst[i].CurrentHealth.SetMaxValue(999999);
            actorLst[i].CurrentHealth.Reset();
            actorLst[i].CurrentVim.SetMaxValue(999999);
            actorLst[i].CurrentVim.Reset();
            actorLst[i].CurrentSkillExecution = null;
            actorLst[i].ReadySkill = null;
            actorLst[i].SetActionState(ACTOR_ACTION_STATE.Idle);
            var skillSlot = actorLst[i].SkillSlots.GetEnumerator();
            while (skillSlot.MoveNext())
            {
                skillSlot.Current.Value.CooldownTimer.MaxTime = 0;
                skillSlot.Current.Value.CooldownTimer.Reset();
            }
        }
        BattleTimeManager.Instance.ResetBattleTime();
        BattleLogicManager.Instance.CurrentFrame = 0;
    }

    private void RefreshActorSenceInfo(CombatActorEntity targetEntity)
    {
        Creature actorCreature = BattleManager.Instance.ActorMgr.GetActor(actorEntity.UID);
        SceneObjectManager.Instance.SetSelectPlayer(actorCreature);
        actorEntity.SetPosition(actorInitPos);
        actorEntity.SetEulerAnglesY(0);
        actorCreature.SelfTrans.localPosition = actorInitPos;
        actorCreature.SelfTrans.localRotation = Quaternion.identity;
        actorCreature.mData.SetActionState(ACTOR_ACTION_STATE.Idle);
        actorCreature.PlayAnim("idle");
        Creature targetCreature = BattleManager.Instance.ActorMgr.GetActor(targetEntity.UID);
        InitPos = actorInitPos + actorEntity.GetForward() * (currentATKDistance + actorEntity.GetTouchRadiu() + enemyEntity.GetTouchRadiu());
        targetCreature.mData.SetPosition(InitPos);
        targetCreature.mData.SetEulerAnglesY(180);
        targetCreature.SelfTrans.position = InitPos;
        targetCreature.SelfTrans.localRotation = Quaternion.Euler(0, 180, 0);
        targetCreature.mData.SetActionState(ACTOR_ACTION_STATE.Idle);
        targetCreature.PlayAnim("idle");
        if (targetEntity != currentTargetEntity)
        {
            Creature hideCreature = targetEntity.UID == enemyEntity.UID ? BattleManager.Instance.ActorMgr.GetActor(friendEntity.UID) : BattleManager.Instance.ActorMgr.GetActor(enemyEntity.UID);
            hideCreature.mData.SetPosition(hitePos);
            hideCreature.SelfTrans.position = hitePos;
            currentTargetEntity = targetEntity;
        }
    }

    public async void PlaySkillID(CombatActorEntity actorEntity, int skillID)
    {
        SkillAbility skillAbility = null;
        if (actorEntity.SkillSlots.ContainsKey((uint)skillID))
        {
            skillAbility = actorEntity.SkillSlots[(uint)skillID];
        }
        if (skillAbility == null)
        {
            return;
        }
        if (actorEntity.CurrentSkillExecution != null)
        {
            return;
        }
        CombatActorEntity targetEntity = skillAbility.SkillConfigObject.AffectTargetType == SKILL_AFFECT_TARGET_TYPE.Team ? friendEntity : enemyEntity;
        float tempATKDistance = Mathf.Abs(skillAbility.SkillBaseConfig.Range * 0.01f - targetEntity.GetHitRadiu());
        tempATKDistance = Mathf.Min(3, tempATKDistance);
        if (currentATKDistance != tempATKDistance
            || targetEntity != currentTargetEntity)
        {
            currentATKDistance = tempATKDistance;
            RefreshActorSenceInfo(targetEntity);
            await Task.Delay(500);
        }
        actorEntity.SetAutoTargetInfo(targetEntity);
        actorEntity.SetActionState(ACTOR_ACTION_STATE.ATK);
        if (actorEntity.SpellSkillActionAbility.TryCreateAction(out var action))
        {
            action.SkillAbility = skillAbility;
            action.SkillAbilityExecution = action.SkillAbility.CreateExecution() as SkillAbilityExecution;
            if (action.SkillAbilityExecution == null)
            {
                return;
            }
            action.SkillAbilityExecution.isNeedCalTotalFrame = true;
            action.SkillAbilityExecution.targetActorEntity = targetEntity;
            action.SkillAbilityExecution.AllTargetActorEntity = BattleUtil.GetSkillTargetsActorEntity(BattleLogicManager.Instance.BattleData, actorEntity);
            actorEntity.SetSkillTargetInfos(null);
            action.SpellSkill();
            action.SkillAbility.ResetSkillCd();
            action.SkillAbilityExecution.onOver = () =>
            {
                RefreshBattleSence();
                RefreshActorSenceInfo(targetEntity);
                HeroSkillViewPage page = UIEngine.Stuff.FindPage("HeroSkillViewPage") as HeroSkillViewPage;
                page.ResetDamgeValue();
            };
        }
        else
        {
            BattleLog.LogError("Skill Action Create Fail");
        }
    }

    public override async Task CreateHeros()
    {
        BattleManager.Instance.Init();
        Battle.Instance.Wave = 1;
        MapUtil.RefreshMapTrigger(true);
        var hero = HeroManager.Instance.GetHeroInfo(_SkillViewModeParam.HeroID);
        var friend = HeroManager.Instance.GetHeroInfo(_SkillViewModeParam.TeamID);

       UnitFatory.CreateUnit(_SkillViewModeParam.HeroID, hero.Level, Vector3.zero, Vector3.zero, 0, true, BattleConst.ATKCampID, BattleConst.ATKTeamID, true, 1f, null, null);
       UnitFatory.CreateUnit(_SkillViewModeParam.TeamID, friend.Level, Vector3.zero, Vector3.zero, 1, true, BattleConst.ATKCampID, BattleConst.ATKTeamID, true, 1f, null, null);

       

    }

    public async void QuitSkillViewMode(bool isMainPage = false)
    {
        if (!Battle.HasInstance())
        {
            return;
        }
        BlockManager.Stuff.AddBlock("QuitSkillViewMode");
        await Battle.Instance.DestroyBattleInstanceAsync();
        if (isMainPage)
        {
            UiUtil.BackToMainGroupThenReplace<MainPage>();
        }
        else
        {
            await UIEngine.Stuff.ForwardOrBackToAsync<HeroPage>();
        }
        BlockManager.Stuff.RemoveBlock("QuitSkillViewMode");
    }
}