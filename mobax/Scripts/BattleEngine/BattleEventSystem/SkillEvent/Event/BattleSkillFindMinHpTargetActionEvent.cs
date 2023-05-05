// using UnityEngine;
// using System.Collections.Generic;
//
// namespace BattleEngine.Logic
// {
//     public class BattleSkillFindMinHpTargetActionEvent : IBattleEvent
//     {
//         public string GetEventName()
//         {
//             return "SkillFindMinHpTargetActionEvent";
//         }
//
//         public void Excute(BattleData battleData, object obj)
//         {
//             BattleSkillEventData data = obj as BattleSkillEventData;
//             if (!battleData.allActorDic.ContainsKey(data.actorUID))
//             {
//                 return;
//             }
//             CombatActorEntity actorEntity = battleData.allActorDic[data.actorUID];
// #region 锁敌范围
//             List<CombatActorEntity> enemys = new List<CombatActorEntity>();
//             if (actorEntity.ReadySkill != null
//                 && actorEntity.ReadySkill.SkillConfigObject.AffectTargetType == SKILL_AFFECT_TARGET_TYPE.Enemy)
//             {
//                 if (actorEntity.isLeader)
//                 {
//                     if (actorEntity.targetTeamKey == -1)
//                     {
//                         Dictionary<int, List<CombatActorEntity>> tempDic = new Dictionary<int, List<CombatActorEntity>>();
//                         if (actorEntity.isAtker)
//                         {
//                             tempDic = battleData.defActorDic;
//                         }
//                         else
//                         {
//                             tempDic = battleData.atkActorDic;
//                         }
//                         var data1 = tempDic.GetEnumerator();
//                         while (data1.MoveNext())
//                         {
//                             for (int i = 0; i < data1.Current.Value.Count; i++)
//                             {
//                                 if (data1.Current.Value[i] != null
//                                     && data1.Current.Value[i].CurrentHealth.Value > 0)
//                                 {
//                                     enemys.Add(data1.Current.Value[i]);
//                                 }
//                             }
//                         }
//                     }
//                     else
//                     {
//                         enemys = BattleUtil.GetEnemyList(battleData, actorEntity);
//                     }
//                 }
//                 else
//                 {
//                     enemys = BattleUtil.GetEnemyList(battleData, actorEntity);
//                 }
//             }
//             else
//             {
//                 BattleUtil.GetSelfCamp(battleData, actorEntity, out enemys);
//             }
//             enemys.Sort(delegate(CombatActorEntity x, CombatActorEntity y)
//                             {
//                                 if (x.CurrentHealth.Value > y.CurrentHealth.Value)
//                                     return 1;
//                                 else
//                                     return -1;
//                             }
//             );
// #endregion;
// #region 找血量最少的目标
//             CombatActorEntity targetEntity = null;
//             for (int i = 0; i < enemys.Count; i++)
//             {
//                 if (enemys[i].IsCantSelect)
//                 {
//                     continue;
//                     ;
//                 }
//                 targetEntity = enemys[i];
//                 break;
//             }
//             if (targetEntity != null)
//             {
//                 Vector3 forward = (targetEntity.GetPositionXZ() - actorEntity.GetPositionXZ()).normalized;
//                 actorEntity.SetForward(forward);
//                 List<CombatActorEntity> targets = new List<CombatActorEntity>();
//                 for (int i = 0; i < actorEntity.ReadySkill.SkillConfigObject.lockDownTargetNum && i < enemys.Count; i++)
//                 {
//                     if (enemys[i].IsCantSelect)
//                     {
//                         continue;
//                     }
//                     targets.Add(enemys[i]);
//                 }
//                 actorEntity.SetSkillTargetInfos(targets);
//             }
// #endregion
//         }
//     }
// }

