// using UnityEngine;
// using System.Collections.Generic;
//
// namespace BattleEngine.Logic
// {
//     public class BattleSkillFindClass2TargetActionEvent : IBattleEvent
//     {
//         public string GetEventName()
//         {
//             return "SkillFindClass2TargetActionEvent";
//         }
//
//         public void Excute(BattleData battleData, object obj)
//         {
// //             BattleSkillEventData data = obj as BattleSkillEventData;
// //             if (!battleData.allActorDic.ContainsKey(data.actorUID))
// //             {
// //                 return;
// //             }
// //             CombatActorEntity actorEntity = battleData.allActorDic[data.actorUID];
// // #region 锁敌范围
// //             List<CombatActorEntity> enemys = new List<CombatActorEntity>();
// //             if (actorEntity.ReadySkill != null
// //                 && actorEntity.ReadySkill.SkillConfigObject.AffectTargetType == SKILL_AFFECT_TARGET_TYPE.Enemy)
// //             {
// //                 if (actorEntity.isLeader)
// //                 {
// //                     if (actorEntity.targetTeamKey == -1)
// //                     {
// //                         Dictionary<int, List<CombatActorEntity>> tempDic = new Dictionary<int, List<CombatActorEntity>>();
// //                         if (actorEntity.isAtker)
// //                         {
// //                             tempDic = battleData.defActorDic;
// //                         }
// //                         else
// //                         {
// //                             tempDic = battleData.atkActorDic;
// //                         }
// //                         var data1 = tempDic.GetEnumerator();
// //                         while (data1.MoveNext())
// //                         {
// //                             for (int i = 0; i < data1.Current.Value.Count; i++)
// //                             {
// //                                 if (data1.Current.Value[i] != null
// //                                     && data1.Current.Value[i].CurrentHealth.Value > 0)
// //                                 {
// //                                     enemys.Add(data1.Current.Value[i]);
// //                                 }
// //                             }
// //                         }
// //                     }
// //                     else
// //                     {
// //                         enemys = BattleUtil.GetEnemyList(battleData, actorEntity);
// //                     }
// //                 }
// //                 else
// //                 {
// //                     enemys = BattleUtil.GetEnemyList(battleData, actorEntity);
// //                 }
// //             }
// //             else
// //             {
// //                 BattleUtil.GetSelfCamp(battleData, actorEntity, out enemys);
// //             }
// // #endregion
// // #region 找目标 牧师>法师>射手>战士>坦克
// //             CombatActorEntity targetEntity = null;
// //             CombatActorPartEntity targetPartEntity = null;
// //             List<int> types = new List<int>()
// //             {
// //                             2
// //                             , 5
// //                             , 4
// //                             , 3
// //                             , 1
// //             };
// //             for (int i = 0; i < types.Count; i++)
// //             {
// //                 targetEntity = GetTarget(enemys, types[i]);
// //                 if (targetEntity != null)
// //                 {
// //                     break;
// //                 }
// //             }
// //             if (targetEntity != null)
// //             {
// //                 Vector3 forward = Vector3.zero;
// //                 string targetPartKey = "";
// //                 if (System.Object.ReferenceEquals(targetPartEntity, null))
// //                 {
// //                     forward = targetEntity.GetPositionXZ() - actorEntity.GetPositionXZ();
// //                 }
// //                 else
// //                 {
// //                     targetPartKey = targetPartEntity.partKey;
// //                     forward = targetPartEntity.GetPositionXZ() - actorEntity.GetPositionXZ();
// //                 }
// //                 actorEntity.KinematControl.Turn(forward.normalized, actorEntity.AngleSpeed);
// //                 actorEntity.SetSkillTargetInfos(new List<CombatActorEntity>() { targetEntity });
// //             }
// // #endregion
//         }
//
//         // private CombatActorEntity GetTarget(List<CombatActorEntity> enemys, int type)
//         // {
//         //     CombatActorEntity entity = null;
//         //     for (int i = 0; i < enemys.Count; i++)
//         //     {
//         //         if (enemys[i].IsCantSelect)
//         //         {
//         //             continue;
//         //         }
//         //         if (enemys[i].CurrentHealth.Value <= 0)
//         //         {
//         //             continue;
//         //         }
//         //         if (enemys[i].battleItemInfo.GetHeroRow().unitClass == type)
//         //         {
//         //             entity = enemys[i];
//         //             break;
//         //         }
//         //     }
//         //     return entity;
//         // }
//     }
// }

