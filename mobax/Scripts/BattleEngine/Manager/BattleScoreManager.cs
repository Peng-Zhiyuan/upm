// using System.Collections.Generic;
// using UnityEngine;
// using BattleEngine.Logic;
//
// namespace CodeHero {
//
//     public class BattleScoreManager {
//         public List<int> battleTimeLst = new List<int>();
//         public List<int> battleUseTimeLst = new List<int>();
//         public int BattleScore;
//         public int BattleUseTime;
//         //PVE战斗最佳分数
//         public int BattleBestScore {
//             get { return RoleLocalCache.GetInt("PVEBestScore", 0); }
//             set { RoleLocalCache.SetInt("PVEBestScore", value); }
//         }
//         //PVE战斗最少时间
//         public int BattleMinTime {
//             get { return RoleLocalCache.GetInt("PVEMinTime", 0); }
//             set { RoleLocalCache.SetInt("PVEMinTime", value); }
//         }
//
//         public void SaveBattleUseTime(int battleAllTime, int useTime) {
//             battleTimeLst.Add(battleAllTime);
//             battleUseTimeLst.Add(useTime);
//         }
//
//         public void ClearScoreData() {
//             battleTimeLst.Clear();
//             battleUseTimeLst.Clear();
//         }
//
//         public void CalculateBattleScore(BattleData data) {
//             ///每减少1秒+2分
//             int allTime = 0;
//             BattleUseTime = 0;
//             for (int i = 0; i < battleTimeLst.Count; i++) {
//                 allTime += battleTimeLst[i];
//                 BattleUseTime += battleUseTimeLst[i];
//             }
//             int timeScore = (allTime - BattleUseTime) * ProtoStaticData.UnitBaseTable["battleScoreTimeSecond"];
//             List<CombatActorEntity> actorLst = data.GetCampIDList(0);
//             int allHP = 0;
//             int remainHP = 0;
//             for (int i = 0; i < actorLst.Count; i++) {
//                 allHP += actorLst[i].CurrentHealth.MaxValue;
//                 remainHP += actorLst[i].CurrentHealth.Value;
//             }
//             float hpOffset = ((allHP - remainHP) / (float)allHP) * ProtoStaticData.UnitBaseTable["battleScoreHPMax"];
//             int hpScore = Mathf.CeilToInt(hpOffset / actorLst.Count);
//             BattleScore = timeScore + hpScore;
//             if (BattleBestScore < BattleScore || BattleBestScore == 0) {
//                 BattleBestScore = BattleScore;
//             }
//             if (BattleMinTime > BattleUseTime || BattleMinTime == 0) {
//                 BattleMinTime = BattleUseTime;
//             }
//             LuaHelper.CallLuaFunction("OnGetBattleScore", BattleScore, BattleBestScore);
//             LuaHelper.CallLuaFunction("OnGetBattleTime", BattleUseTime, BattleMinTime);
//         }
//     }
//
// }

