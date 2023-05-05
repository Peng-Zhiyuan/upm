namespace BattleEngine.Logic
{
    using System.Collections.Generic;

    public sealed class BattleAIKeepSafeDisActionEvent : IBattleEvent
    {
        public string GetEventName()
        {
            return "AIKeepSafeDisActionEvent";
        }

        public void Excute(BattleData battleData, object obj)
        {
            BattleAIOperaterData data = obj as BattleAIOperaterData;
            if (!battleData.allActorDic.ContainsKey(data.actorUID))
            {
                return;
            }
            CombatActorEntity actorEntity = battleData.allActorDic[data.actorUID];
            List<CombatActorEntity> enemys = battleData.GetEnemyLst(actorEntity);
            float minDis = -1;
            int index = 0;
            float dis = 0.0f;
            for (int i = 0; i < enemys.Count; i++)
            {
                if (enemys[i].IsCantSelect)
                {
                    continue;
                }
                //如果是被这个敌人攻击的，直接逃离这个敌人
                if (enemys[i].targetKey == actorEntity.UID)
                {
                    index = i;
                    break;
                }
                //逃离最近的敌人
                dis = MathHelper.ActorDistance(actorEntity, enemys[i]);
                if (minDis > dis
                    || minDis < 0)
                {
                    minDis = dis;
                    index = i;
                }
            }
            var tarActor = enemys[index];
            UnityEngine.Vector3 pos = actorEntity.GetPosition() + 0.35f * (actorEntity.GetPositionXZ() - tarActor.GetPositionXZ()).normalized;
            actorEntity.SetTargetPos(pos);
            if (BattleTimeManager.Instance.NowTimestamp - actorEntity.beginKeepSafeTime >= BattleConst.keepSafeCD)
            {
                actorEntity.beginKeepSafeTime = BattleTimeManager.Instance.NowTimestamp;
            }
            actorEntity.SetActionState(ACTOR_ACTION_STATE.Move);
        }
    }
}