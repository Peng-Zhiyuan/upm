namespace BattleEngine.Logic
{
    using System.Collections.Generic;
    using UnityEngine;

    public class BattleAIPureLoveTargetActionEvent : IBattleEvent
    {
        public string GetEventName()
        {
            return "AIPureLoveTargetActionEvent";
        }

        public void Excute(BattleData battleData, object obj)
        {
            BattleAIOperaterData data = obj as BattleAIOperaterData;
            if (!battleData.allActorDic.ContainsKey(data.actorUID))
            {
                return;
            }
            CombatActorEntity actorEntity = battleData.allActorDic[data.actorUID];
#region 锁敌范围
            List<CombatActorEntity> enemys = battleData.GetEnemyLst(actorEntity);
#endregion
#region 找血量上限最低的目标
            CombatActorEntity targetEntity = null;
            int minHP = 0;
            for (int i = 0; i < enemys.Count; i++)
            {
                if (enemys[i].IsCantSelect)
                {
                    continue;
                }
                if (minHP > enemys[i].CurrentHealth.MaxValue
                    || targetEntity == null)
                {
                    minHP = enemys[i].CurrentHealth.MaxValue;
                    targetEntity = enemys[i];
                }
            }
            if (targetEntity != null)
            {
                Vector3 forward = (targetEntity.GetPositionXZ() - actorEntity.GetPositionXZ()).normalized;
                // actorEntity.KinematControl.Turn(forward.normalized, BattleConst.MoveAnagleOffset);
                actorEntity.SetForward(forward);
                actorEntity.SetInputTargetInfo(targetEntity);
            }
#endregion
        }
    }
}