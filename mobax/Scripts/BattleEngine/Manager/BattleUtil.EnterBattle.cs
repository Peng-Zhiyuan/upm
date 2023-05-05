using System;
using System.Collections.Generic;
using behaviac;

namespace BattleEngine.Logic
{
    using System.Linq;
    using View;
    using BattleSystem.ProjectCore;

    /// <summary>
    /// 提供enterBattle相关接口
    /// </summary>
    public static partial class BattleUtil
    {
        public static bool fast_enter = false;

        /// <summary>
        /// 使用默认编队
        /// </summary>
        /// <param name="stageId"></param>
        public static async void EnterPveBattle(int stageId)
        {
            var formationIndex = FormationUtil.GetDefaultFormationIndex();
            EnterPveBattle(stageId, formationIndex);
        }

        public static async void EnterDefenceBattle(int stageId, EFormationIndex formationIndex)
        {
            EnterBattle(BattleModeType.Defence, stageId, null, formationIndex);
        }

        /// <summary>
        /// pve battle
        /// </summary>
        /// <param name="stageId">关卡id</param>o
        /// <param name="formationIndex">编队index</param>
        public static async void EnterPveBattle(int stageId, EFormationIndex formationIndex, EStageDifficult difficult = EStageDifficult.Easy)
        {
            BattleConst.Difficult = difficult;
            var stageRow = StaticData.StageTable.TryGet(stageId);
            var pveType = (EBattleType)stageRow.Types.First();
            if (pveType.Equals(EBattleType.GuardCopy))
            {
                EnterDefenceBattle(stageId, formationIndex: formationIndex);
            }
            else if (pveType.Equals(EBattleType.FixedDefence))
            {
                EnterBattle(BattleModeType.Fixed, stageId, null, formationIndex);
            }
            else if (pveType.Equals(EBattleType.Gold))
            {
                List<int> list = new List<int>();
                if (stageRow.mosterGroups != null)
                {
                    list = stageRow.mosterGroups.ToList();
                }
                var goldParam = new GoldCopyModeParam() { IsDouble = false, MonsterGroup = list, Difficult = EResourceCopyDifficult.Low };
                EnterGoldCopyBattle(stageId, goldParam, formationIndex: formationIndex);
            }
            else
            {
                EnterBattle(BattleModeType.PveMode, stageId, formationIndex: formationIndex);
            }
        }

        public static async void EnterDreamEscape(int copyId, EFormationIndex formationIndex, DreamscapeBattleParam modeParam, CreateBattleResponse response = null, Action<BattleServerResultData> delegateSkipAction = null)
        {
            EnterBattle(BattleModeType.Dreamscape, copyId, modeParam, formationIndex, BattleEnterSource.None, response, delegateSkipAction);
        }

        public static async void EnterGuardEscape(int copyId, GuardModeParam modeParam, BattleEnterSource source = BattleEnterSource.None)
        {
            var formationIndex = FormationUtil.GetDefaultFormationIndex();
            if (modeParam == null)
            {
                modeParam = new GuardModeParam();
                var heroes = FormationUtil.GetFormationHeroInfos(formationIndex);
                foreach (var VARIABLE in heroes)
                {
                    modeParam.heros.Add(VARIABLE);
                }
            }
            var hero = modeParam.heros.Find(hero => hero.MainHeroId == BattleConst.SiLaLiID);
            if (hero == null)
            {
                ToastManager.ShowLocalize("M4_cobattle_tip_noSpecifyHero");
                return;
            }
            EnterBattle(BattleModeType.Guard, copyId, modeParam, formationIndex, source);
        }

        public static async void EnterFixedMode(int copyId, GuardModeParam modeParam, BattleEnterSource source = BattleEnterSource.None)
        {
            var formationIndex = FormationUtil.GetDefaultFormationIndex();
            EnterBattle(BattleModeType.Fixed, copyId, null, formationIndex);
        }

        /// <summary>
        /// 竞技场战斗
        /// </summary>
        /// <param name="arenaMode"></param>
        /// <param name="copyId"></param>
        public static void EnterArenaBattle(int copyId, ArenaModeParam arenaMode)
        {
            EnterBattle(BattleModeType.Arena, copyId, arenaMode, EFormationIndex.Normal1);
        }

        /// <summary>
        /// 守卫模式入口
        /// </summary>
        /// <param name="copyId"></param>
        public static async void EnterDefenceBattle(int copyId)
        {
            EnterBattle(BattleModeType.Defence, copyId);
        }

        /// <summary>
        /// 资源本入口 -- 金钱本
        /// </summary>
        public static void EnterGoldCopyBattle(int copyId, GoldCopyModeParam goldParam, EFormationIndex formationIndex)
        {
            EnterBattle(BattleModeType.Gold, copyId, goldParam, formationIndex);
        }

        /// <summary>
        /// 资源本入口 -- 角色本
        /// </summary>
        public static void EnterRoleCopyBattle(int copyId, RoleCopyModeParam goldParam, EFormationIndex formationIndex)
        {
            EnterBattle(BattleModeType.Role, copyId, goldParam, formationIndex);
        }

        /// <summary>
        /// 资源本入口 -- 道具本
        /// </summary>
        public static void EnterItemCopyBattle(int copyId, ItemCopyModeParam goldParam, EFormationIndex formationIndex)
        {
            EnterBattle(BattleModeType.Fixed, copyId, goldParam, formationIndex);
        }

        /// <summary>
        /// 技能展示场景
        /// </summary>
        public static void EnterSkillViewBattle(int copyId, SkillViewModeParam skillViewParam)
        {
            EnterSkillViewMode(BattleModeType.SkillView, copyId, skillViewParam);
        }

        /// <summary>
        /// 爬塔
        /// </summary>
        public static void EnterTowerBattle(TowerModeParam towerModeParam, EFormationIndex formationIndex, Action<BattleServerResultData> delegateSkipAction = null)
        {
            TowerRow towerRow = StaticData.TowerTable.TryGet(towerModeParam.TowerID);
            if (towerRow == null)
            {
                BattleLog.LogError("Cant find the Tower ID " + towerModeParam.TowerID);
                return;
            }
            var stageRow = StaticData.StageTable.TryGet(towerRow.Stage);
            var pveType = (EBattleType)stageRow.Types.First();
            if (pveType.Equals(EBattleType.TOWER_FIXED))
            {
                EnterBattle(BattleModeType.TowerFixed, towerRow.Stage, towerModeParam, formationIndex, BattleEnterSource.None, null, delegateSkipAction);
            }
            else
            {
                EnterBattle(BattleModeType.TowerNormal, towerRow.Stage, towerModeParam, formationIndex, BattleEnterSource.None, null, delegateSkipAction);
            }
        }

        /// <summary>
        /// 通用enter battle
        /// </summary>
        /// <param name="battleMode">模式</param>
        /// <param name="copyId"></param>
        /// <param name="modeParam">模式参数，需在模式内转换指定参数</param>
        public static async void EnterBattle(BattleModeType battleMode, int copyId, object modeParam = null, EFormationIndex formationIndex = EFormationIndex.None, BattleEnterSource source = BattleEnterSource.None, CreateBattleResponse response = null, Action<BattleServerResultData> delegateSkipCallBack = null)
        {
            EFormationIndex teamIndex = formationIndex;
            if (teamIndex == EFormationIndex.None)
            {
                teamIndex = FormationUtil.GetDefaultFormationIndex();
            }
            var formationInfo = FormationUtil.GetFormationHeroInfos(teamIndex);
            if (formationInfo == null
                || !FormationUtil.IsHeroExist(formationInfo))
            {
                Dialog.ConfirmWithNoButton("", LocalizationManager.Stuff.GetText("m3_noFormation"));
                return;
            }
            var pveParam = await StageManager.Stuff.GetPveModeParam(copyId, teamIndex);
            if (pveParam == null)
            {
                Debug.LogError("获取地图数据失败");
                return;
            }
            var param = new BattleCoreParam();
            param.mode = battleMode;
            param.pveParam = pveParam;
            param.modeParam = modeParam;
            param.source = source;
            param.memebers = formationInfo;
            if (delegateSkipCallBack != null)
            {
                BlockManager.Stuff.AddBlock("ExecureBattleCheck");
                // 请求create协议
                var createBattleResponse = await BattlePipline.RequestCreateBattleResponse(param);
                if (createBattleResponse == null)
                {
                    return;
                }
                var battlePlayerResponse = await BattleApi.RequestPlayerAsync(Database.Stuff.roleDatabase.Me._id);
                if (battleMode == BattleModeType.TowerFixed
                    || battleMode == BattleModeType.TowerNormal)
                {
                    BattleServerResultData resultData = new BattleServerResultData();
                    resultData.battleServerData = new BattleServerData();
                    resultData.battleServerData.BattleKey = createBattleResponse.id;
                    delegateSkipCallBack?.Invoke(resultData);
                }
                else
                {
                    BattleServerResultData resultData = await BattleServerEnter.ExecuteBattleCheck(param, createBattleResponse, battlePlayerResponse);
                    delegateSkipCallBack.Invoke(resultData);
                }
                BlockManager.Stuff.RemoveBlock("ExecureBattleCheck");
                return;
            }
            try
            {
                Battle.Instance.CreateBattleInstanceInBackground(param, response);
            }
            catch (Exception e)
            {
                await Battle.Instance.DestroyBattleInstanceAsync();
            }
        }

        public static async void EnterSkillViewMode(BattleModeType battleMode, int copyId, object modeParam = null, EFormationIndex formationIndex = EFormationIndex.None, BattleEnterSource source = BattleEnterSource.None, CreateBattleResponse response = null)
        {
            EFormationIndex teamIndex = formationIndex;
            if (teamIndex == EFormationIndex.None)
            {
                teamIndex = FormationUtil.GetDefaultFormationIndex();
            }
            var formationInfo = Database.Stuff.FormationDatabase.GetFormationInfo(teamIndex);
            if (formationInfo == null
                || !FormationUtil.IsHeroExist(formationInfo.heroes.ToList()))
            {
                Dialog.ConfirmWithNoButton("", LocalizationManager.Stuff.GetText("m3_noFormation"));
                return;
            }
            var pveParam = await StageManager.Stuff.GetPveModeParam(copyId, teamIndex);
            if (pveParam == null)
            {
                Debug.LogError("获取地图数据失败");
                return;
            }
            var param = new BattleCoreParam();
            param.mode = battleMode;
            param.pveParam = pveParam;
            param.modeParam = modeParam;
            param.source = source;
            try
            {
                Battle.Instance.CreateBattleInstanceInBackgroundSkillView(param);
            }
            catch (Exception e)
            {
                await Battle.Instance.DestroyBattleInstanceAsync();
            }
        }

#region ---Obsolete---
        public static Dictionary<int, int> HeroHps;

        /// <summary>
        /// 副本战斗的入口
        /// 注：调用需谨慎，接口太多，目前只针对Cookie开放
        /// </summary>
        /// <param name="stageid"></param>
        public static void EnterInstanceBattle(int stageid, EFormationIndex formationIndex, Dictionary<int, int> hpMap, BattleEnterSource source = BattleEnterSource.Zone)
        {
            HeroHps = hpMap;
            EBattleType type = StageManager.Stuff.GetBattleType(stageid);
            if (type == EBattleType.GuardNpc)
            {
                EnterBattle(BattleModeType.Guard, stageid, null, formationIndex, source);
            }
            else if (type == EBattleType.Normal)
            {
                EnterBattle(BattleModeType.PveMode, stageid, null, formationIndex, source);
            }
            else if (type == EBattleType.GuardCopy)
            {
                EnterBattle(BattleModeType.Defence, stageid, null, formationIndex, source);
            }
        }
#endregion
    }
}