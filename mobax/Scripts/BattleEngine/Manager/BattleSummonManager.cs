using BattleSystem.ProjectCore;

namespace BattleEngine.Logic
{
    using System.Collections.Generic;
    using UnityEngine;
    using System.Threading.Tasks;

    /// <summary>
    /// 怪物召唤管理
    /// </summary>
    public sealed class BattleSummonManager
    {
        public MapCatView mapCatView;

        public int SummonNum;

        public async Task SummonCatToSence()
        {
            if (!BattleLogicManager.Instance.IsOpenBattleViewLayer)
            {
                return;
            }
            if (mapCatView == null)
            {
                mapCatView = new MapCatView(null);
            }
            else
            {
                mapCatView.Clean();
            }
            var heros = BattleDataManager.Instance.GetList(MapUnitType.MainHero);
            var monsters = BattleDataManager.Instance.GetList(MapUnitType.Monster);
            Vector3 centerPos = Vector3Util.GetBetweenPoint(GetMapUnitCenter(heros), GetMapUnitCenter(monsters));
            List<StageCatInfo> modelSummonCat = new List<StageCatInfo>();
            for (int i = 0; i < 10; i++)
            {
                StageCatInfo stageCatInfo = new StageCatInfo();
                int CatIndex = Random.Range(0, StageCat.CatNickNameList.Count);
                stageCatInfo.catName = StageCat.CatNickNameList[CatIndex];
                stageCatInfo.sp = centerPos + new Vector3(-2, 0, -2);
                stageCatInfo.ep = centerPos + new Vector3(2, 0, 2);
                stageCatInfo.duration = Random.Range(5, 15);
                stageCatInfo.stayDurationMin = Random.Range(1, 3);
                stageCatInfo.stayDurationMin = Random.Range(6, 8);
                stageCatInfo.boxCollider = Vector3.zero;
                stageCatInfo.bornType = StageCat.BORNTYPE.FLY;
                modelSummonCat.Add(stageCatInfo);
            }
            int index = 3;
            for (int i = 0; i < modelSummonCat.Count; i++)
            {
                if (index > 3)
                {
                    await Task.Delay(500);
                    index = 0;
                }
                await mapCatView.InitCatActor(modelSummonCat[i]);
                index++;
            }
            List<CombatActorEntity> SelfTeam = BattleLogicManager.Instance.BattleData.atkActorLst;
            for (int i = 0; i < SelfTeam.Count; i++)
            {
                if (SelfTeam[i].CurrentHealth.Value <= 0)
                {
                    continue;
                }
                SelfTeam[i].AttachBuff((int)BUFF_COMMON_CONFIG_ID.CAT_ADD_ATTACK);
            }
        }

        public Vector3 GetMapUnitCenter(List<MapWaveModelData> list)
        {
            List<Vector3> lst = new List<Vector3>();
            foreach (var VARIABLE in list)
            {
                lst.Add(VARIABLE.Pos);
            }
            return Vector3Util.GetCenterPoint(lst);
        }

        public CombatActorEntity CreateActorEntity(int monsterID, ActorIFF iff)
        {
            BattleItemInfo itemInfo = BattleUtil.CreateActorItemInfo(monsterID, 1, true);
            return BattleUtil.CreateCombatActorUnit(itemInfo, iff);
        }

        public void CreateSummonMonster(BattleData battleData, int monsterID, ActorIFF iff)
        {
            SummonNum++;
            iff.PosIndex = BattleConst.SummonPosIndexStart + SummonNum;
            CombatActorEntity entity = CreateActorEntity(monsterID, iff);
            battleData.AddActorData(entity);
            if (BattleLogicManager.Instance.IsOpenBattleViewLayer)
            {
                EventManager.Instance.SendEvent<CombatActorEntity>("CreateEntity", entity);
            }
        }
    }

    public class BattleSummonData
    {
        public ItemInfo actor;
        public Vector3 position = Vector3.zero;
        public Vector3 foward = Vector3.zero;
        public float size = 1.0f;

        public bool SetMonsterData(int id, Vector3 targetPos, Vector3 targetForward)
        {
            BattleLog.LogError("召唤功能已暂停使用");
            MonsterRow row = StaticData.MonsterTable.TryGet(id, null);
            if (row == null)
            {
                BattleLog.LogError("Cant find the Monster : " + id);
                return false;
            }
            return true;
        }
    }
}