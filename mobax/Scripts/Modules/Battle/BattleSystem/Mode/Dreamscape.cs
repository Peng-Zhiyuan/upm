using System.Collections.Generic;
using System.Threading.Tasks;
using BattleEngine.Logic;
using BattleSystem.ProjectCore;
using UnityEngine;

public class DreamscapeBattleParam : PveModeParam
{
    public int difficulty;
    public int buffId;
    public int heroid; //英雄id
    public int memMarkId;
    public List<int> heros = new List<int>();
}

[BattleModeClass(BattleModeType.Dreamscape)]
public class Dreamscape : PveMode
{
    public DreamscapeBattleParam _battleParam;

    public override BattleModeType ModeType
    {
        get { return BattleModeType.Dreamscape; }
    }

    public override void OnCreate(PveModeParam pveParam, object modeParam, Battle battle, CreateBattleResponse response)
    {
        base.OnCreate(pveParam, modeParam, battle, response);
        _battleParam = modeParam as DreamscapeBattleParam;
    }

    public override async Task OnPreCreateBattleGameObjectAsync() { }

    public float GetDifficult()
    {
        return _battleParam.difficulty;
    }

    public override async Task BattleResult(bool isWin, List<BattlePlayerRecord> myTeamRecrod, List<BattlePlayerRecord> enemyTeamRecord, float duration)
    {
        // 显示结算页面
        var win = BattleStateManager.Instance.GameResult == eBattleResult.Victory;
        var time = (int)Mathf.FloorToInt(BattleLogicManager.Instance.CurrentFrame * BattleLogicDefine.LogicSecTime);
        var killNum = BattleLogicManager.Instance.BattleData.KillNum;
        var deadNum = 0; // 战斗中挂掉的英雄个数
        var actorList = BattleManager.Instance.ActorMgr.GetAllActors();
        actorList.ForEach(actor =>
                        {
                            if (actor.mData.isAtker
                                && actor.mData.IsDead)
                            {
                                deadNum++;
                            }
                        }
        );

        // 1表示己方胜利，2表示对方胜利
        if (!BattleLogicManager.Instance.IsReport)
        {
            var score = win ? DreamscapeManager.CalculateScore(time, killNum, deadNum) : 0;
            var dreamscapeInfo = await DreamscapeMentorApi.BattleResult(Response.id, win ? 1 : 2, score);
            // if (win) DreamscapeManager.StageDone();
            DreamscapeManager.UpdateInfo(dreamscapeInfo);
        }
        if (win)
        {
            //await UIEngine.Stuff.ShowFloatingAsync(nameof(DreamscapeStageResultPop));
            await UIEngine.Stuff.ForwardOrBackToAsync<BattleResultWinPage>(new BattleWinPageParams() { LevelId = Battle.Instance.CopyId, RewardList = new List<ItemInfo>(), IsStory = false });
        }
        else
        {
            // 失败了
            await UIEngine.Stuff.ForwardOrBackToAsync<BattleResultFailPage>(new BattleFailPageParams { LevelId = Battle.Instance.CopyId, OnRestartAction = null });
            await DreamscapeManager.Exit(false);
            await UIEngine.Stuff.RemoveFromStack<DreamscapePage>();
        }
    }

    public override async Task BattleReady()
    {
        await this.battle.SpawnMonsters(true);
    }

    public override async Task CreateHeros()
    {
        this.SpawnHeros();
    }

    private async void SpawnHeros()
    {
        BattleManager.Instance.Init();
        Battle.Instance.Wave = 1;
        MapUtil.RefreshMapTrigger(true);
        int index = 0;
        int slot = 0;
        var heros = FormationUtil.ParseHeroIdList(_battleParam.heros);
        foreach (var VARIABLE in BattleDataManager.Instance.GetList(MapUnitType.MainHero))
        {
            var dir = Quaternion.AngleAxis(StageBattleUtil.GetMapRotationY(VARIABLE), Vector3.up) * Vector3.forward;
            var pos = VARIABLE.Pos;
            if (heros.Count < index + 1
                || heros[index].Count < 0)
                continue;
            var id = heros[index][0];
            if (id == 0) continue;
            var hero = HeroManager.Instance.GetHeroInfo(id);
            if (!hero.Unlocked)
                continue;
            UnitFatory.CreateUnit(id, hero.Level, pos, dir, index, true, BattleConst.ATKCampID, BattleConst.ATKTeamID, true, 1f, null, null);
            index++;
        }
        if (AssistBattleInfo != null
            && AssistBattleInfo.heroes != null
            && AssistBattleInfo.heroes.Count > 0)
        {
            var hero = AssistBattleInfo.heroes[0];
            var joinInfo = Battle.Instance.GetFrindJoinPosition();
            var dir = Quaternion.AngleAxis(StageBattleUtil.GetMapRotationY(joinInfo), Vector3.up) * Vector3.forward;
            UnitFatory.CreateUnit(hero.id, hero.lv, joinInfo.Pos, dir, BattleConst.FriendPosIndex, true, BattleConst.ATKCampID, BattleConst.ATKTeamID, true, 1f, hero);
        }
        await this.battle.SpawnLeader(false);
        BattleAutoFightManager.Instance.InitManager(BattleLogicManager.Instance.BattleData);
    }
}