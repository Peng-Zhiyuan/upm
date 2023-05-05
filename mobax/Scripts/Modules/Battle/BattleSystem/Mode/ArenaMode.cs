using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using BattleSystem.ProjectCore;
using BattleEngine.Logic;

[BattleModeClass(BattleModeType.Arena)]
public class ArenaMode : Mode
{
    public ArenaModeParam param { get; set; }

    public override void OnCreate(PveModeParam pveParam, object modeParam, Battle battle, CreateBattleResponse response)
    {
        base.OnCreate(pveParam, modeParam, battle, response);
        this.param = modeParam as ArenaModeParam;
        this.battleInfo = response;
    }

    public override void OnStart()
    {

        var myTeamRecrod = BattleEngine.Logic.BattleLogicManager.Instance.BattleData.GetAtkRecordLst();
        var enemyTeamRecord = BattleEngine.Logic.BattleLogicManager.Instance.BattleData.GetDefRecordLst();
        // 本地记录战绩
        var sessionIndex = ArenaManager.Stuff.info.circle;
        var record = BattleEngineRecordToArenaRecord(myTeamRecrod, enemyTeamRecord);
        record.session = sessionIndex;
        record.isWin = false;
        record.scoreDelta = 0;
        record.postScore = ArenaManager.Stuff.info.score;
        record.battleId = this.battleInfo.id;
        record.power = this.param.enemyPower;
        record.enemyName = this.param.enemyName;
        record.timestampSec = Clock.TimestampSec;
        record.durationSec = (int)0;
        record.enemyScore = this.param.enemyScore;
        record.enemyUid = this.param.targetUid;
        ArenaRecordManager.Stuff.AddBattleInfo(record);
    }

    public override BattleModeType ModeType
    {
        get { return BattleModeType.Arena; }
    }

    public CreateBattleResponse battleInfo;
    BattlePlayer EnemyInfo;

    public override async Task OnPreCreateBattleGameObjectAsync()
    {
        var targetUid = param.targetUid;
        this.EnemyInfo = await BattleApi.RequestPlayerAsync(targetUid);
    }

    /// <summary>
    /// 战斗已请求到自己的玩家数据
    /// </summary>
    /// <returns></returns>
    public override async Task AfterLoadedPlayerInfoAsync()
    {
        var myInfo = this.battle.BattlePlayerData;
        var enemyInfo = this.EnemyInfo;
        var param = new VsPageParam();
        param.myInfo = myInfo;
        param.enemyInfo = enemyInfo;
        param.arenaMode = this;
        await UIEngine.Stuff.ForwardAsync<VsPage>(param);
        await UIEngine.Stuff.WaitPageBackAsync(nameof(VsPage));
    }

    public override string GetPageName()
    {
        return "BattlePage";
    }

    string LocalIsWinToWinnerUid(bool isWin)
    {
        if (isWin)
        {
            var myUid = Database.Stuff.roleDatabase.Me._id;
            return myUid;
        }
        else
        {
            var opponentUid = this.param.targetUid;
            return opponentUid;
        }
    }

    public static void RemoveUnrelatedPerson(List<BattlePlayerRecord> list)
    {
        for (int i = list.Count - 1; i >= 0; i--)
        {
            var one = list[i];
            var id = one.id;
            if (id == 1701000
                || id == 1701001)
            {
                list.RemoveAt(i);
            }
        }
    }

    public override async Task BattleResult(bool isWin, List<BattlePlayerRecord> myTeamRecrod, List<BattlePlayerRecord> enemyTeamRecord, float duration)
    {
        RemoveUnrelatedPerson(myTeamRecrod);
        RemoveUnrelatedPerson(enemyTeamRecord);
        var battleId = this.battleInfo.id;
        var winnerUid = LocalIsWinToWinnerUid(isWin);
        var myCamp = this.battleInfo.GetCampByUid(winnerUid);
        var ret = await BattleApi.ArenaResultAsync(battleId, myCamp);
        var newMyArenaInfo = ret.info;
        var cache = ret.cache;
        var beforeScore = ArenaManager.Stuff.info.score;
        ArenaManager.Stuff.info = newMyArenaInfo;
        var postScore = ArenaManager.Stuff.info.score;
        var scoreDelta = postScore - beforeScore;

        // 上报事件
        if (isWin)
        {
            var opponentId = this.param.targetUid;
            var row = ArenaUtil.GetArenaRowByScore(beforeScore);
            var rowId = row.Id;
            TrackManager.PvpWin(opponentId, rowId.ToString(), scoreDelta.ToString());
        }
        else
        {
            var opponentId = this.param.targetUid;
            var row = ArenaUtil.GetArenaRowByScore(beforeScore);
            var rowId = row.Id;
            TrackManager.PvpLose(opponentId, rowId.ToString(), scoreDelta.ToString());
        }

        // 本地记录战绩
        var sessionIndex = newMyArenaInfo.circle;
        var record = BattleEngineRecordToArenaRecord(myTeamRecrod, enemyTeamRecord);
        record.session = sessionIndex;
        record.isWin = isWin;
        record.scoreDelta = scoreDelta;
        record.postScore = postScore;
        record.battleId = battleId;
        record.power = this.param.enemyPower;
        record.enemyName = this.param.enemyName;
        record.timestampSec = Clock.TimestampSec;
        record.durationSec = (int)duration;
        record.enemyScore = this.param.enemyScore;
        record.enemyUid = this.param.targetUid;
        ArenaRecordManager.Stuff.AddBattleInfo(record);
        var param = new ArenaBattleResultPageParam();
        param.record = record;
        param.cache = cache;

        // 显示结算页面
        UIEngine.Stuff.ForwardOrBackTo<ArenaBattleResultPageV2>(param);
        UIEngine.Stuff.HookBack(nameof(ArenaBattleResultPageV2), async () =>
                        {
                            await ArenaManager.Stuff.SyncAllAsync();
                            await UIEngine.Stuff.RemoveFromStack<BattlePage>();
                            await Battle.Instance.DestroyBattleInstanceAsync();
                            UIEngine.Stuff.BackTo<ArenaPageV2>();
                        }
        );
    }

    public static ArenaRecordBattle BattleEngineRecordToArenaRecord(List<BattlePlayerRecord> myTeamRecrod, List<BattlePlayerRecord> enemyTeamRecord)
    {
        var info = new ArenaRecordBattle();
        foreach (var battleRecord in myTeamRecrod)
        {
            var record = new ArenaRecordHero();
            record.heroRowId = battleRecord.id;
            record.camp = ArenaBattleHeroCamp.Self;
            record.attack = battleRecord.OP_AttackValue;
            record.heal = battleRecord.OP_CureValue;
            record.defence = battleRecord.ReceiveDamageValue;
            info.heroInfoList.Add(record);
        }
        foreach (var battleRecord in enemyTeamRecord)
        {
            var record = new ArenaRecordHero();
            record.heroRowId = battleRecord.id;
            record.camp = ArenaBattleHeroCamp.Opponent;
            record.attack = battleRecord.OP_AttackValue;
            record.heal = battleRecord.OP_CureValue;
            record.defence = battleRecord.ReceiveDamageValue;
            info.heroInfoList.Add(record);
        }
        return info;
    }

    public override async Task BattleReady()
    {
        await this.SpawnMonster();
        if (!Battle.Instance.IsFight)
            return;
        if (ArenaManager.Stuff.info.score < StaticData.BaseTable["arenaBuffRange"])
        {
            List<CombatActorEntity> selfList = BattleLogicManager.Instance.BattleData.atkActorLst;
            for (int i = 0; i < selfList.Count; i++)
            {
                if (selfList[i].IsCantSelect)
                {
                    continue;
                }
                selfList[i].AttachBuff(StaticData.BaseTable["arenaBuffAddition"]);
            }
        }
    }

    private async Task SpawnMonster()
    {
        List<List<int>> heroIDList = FormationUtil.ParseHeroIdList(param.enemyHeroList);
        var list = BattleDataManager.Instance.GetList(MapUnitType.Monster);
        if (list != null
            && list.Count > 0)
        {
            int index = 0;
            for (int i = 0; i < heroIDList.Count; i++)
            {
                List<int> mainHeroLst = heroIDList[i];
                if (mainHeroLst.Count == 0)
                {
                    continue;
                }
                int heroID = mainHeroLst[0];
                BattleHero hero = GetEnemy(heroID);
                if (hero == null)
                {
                    continue;
                }
                var pos = list[index].Pos;
                var dir = Quaternion.AngleAxis(StageBattleUtil.GetMapRotationY(list[index]), Vector3.up) * Vector3.forward;
                var slot = index;
                UnitFatory.CreateUnit(hero.id, hero.lv, pos, dir, slot, true, BattleConst.DEFCampID, BattleConst.DEFTeamID, false, 1f, hero);
                index++;
            }
        }
    }

    public BattleHero GetEnemy(int heroID)
    {
        foreach (var VARIABLE in EnemyInfo.heroes)
        {
            if (VARIABLE.id == heroID)
                return VARIABLE;
        }
        return null;
    }

    public override void OnUpdate() { }

    public override async Task SetReadyCamera()
    {
        await base.SetReadyCamera();
        if (!Battle.Instance.IsFight)
            return;
        //SceneObjectManager.Instance.LocalPlayerCamera.transform.DOMove(Vector3.zero, 0.2f).SetEase(Ease.OutCirc);
        SceneObjectManager.Instance.LocalPlayerCamera.transform.localPosition = Vector3.zero;
        await Task.Delay(200);
        if (!Battle.Instance.IsFight)
            return;
        CameraManager.Instance.TryChangeState(CameraState.Arena, null);
    }
}