namespace BattleEngine.Logic
{
    using System.Collections.Generic;
    using UnityEngine;

    public class DotCtr
    {
        public static void CreatDot(CreatDotTaskData taskData, CombatActorEntity owner, SKILL_AFFECT_TARGET_TYPE affectTargetType)
        {
            var dot = Entity.CreateWithParent<DotEntity>(MasterEntity.Instance, "");
            if (taskData.isAttachLookAt)
            {
                Vector3 pos = Quaternion.Euler(owner.GetEulerAngles()) * taskData.offset + owner.GetPosition();
                Vector3 rot = Quaternion.Euler(taskData.angleOffset).eulerAngles;
                dot.Born(pos, rot);
            }
            else
            {
                Vector3 pos = taskData.offset + owner.GetPosition();
                Vector3 rot = Quaternion.Euler(taskData.angleOffset).eulerAngles;
                dot.Born(pos, rot);
            }
            dot.Init(affectTargetType, taskData.scale, taskData.effectPrefab, taskData.durationTime, taskData.intervalTime, taskData.radius, taskData.height, taskData.Effects, owner, taskData.effectiveOnce, taskData.applyEffectDirect);
            dot.AddComponent<UpdateComponent>();
        }

        public static void CreatDot(Vector3 pos, Vector3 rot, Vector3 scale, string effectPrefab, int durationTime, int intervalTime, float radius, float height, List<Effect> Effects, CombatActorEntity owner, SKILL_AFFECT_TARGET_TYPE affectTargetType, bool effectiveOnce, bool applyEffectDirect)
        {
            var dot = Entity.CreateWithParent<DotEntity>(MasterEntity.Instance, "");
            dot.Born(pos, rot);
            dot.Init(affectTargetType, scale, effectPrefab, durationTime, intervalTime, radius, height, Effects, owner, effectiveOnce, applyEffectDirect);
            dot.AddComponent<UpdateComponent>();
        }
    }
}