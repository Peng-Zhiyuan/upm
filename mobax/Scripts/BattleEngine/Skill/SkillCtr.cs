namespace BattleEngine.Logic
{
    using UnityEngine;

#if !SERVER
    using DG.Tweening;
    using View;
#endif

    public class SkillCtr
    {
        public static void SkillActorMove(CombatActorEntity actorEntity, Vector3 moveDelta, float delta, float minY = -100.0f)
        {
            Vector3 endPos = actorEntity.GetPosition() + moveDelta;
            if (minY != -100.0f
                && endPos.y < minY)
            {
                endPos.y = minY;
            }
            actorEntity.SetPosition(endPos);
#if !SERVER
            if (BattleLogicManager.Instance.IsOpenBattleViewLayer)
            {
                Creature actor = BattleManager.Instance.ActorMgr.GetActor(actorEntity.UID);
                actor.SelfTrans.position = endPos;
            }
#endif
        }

        public static void SkillSetActorMove(CombatActorEntity actorEntity, Vector3 targetPos)
        {
            if (actorEntity == null)
            {
                return;
            }
            actorEntity.SetPosition(targetPos);
#if !SERVER
            if (BattleLogicManager.Instance.IsOpenBattleViewLayer)
            {
                Creature actor = BattleManager.Instance.ActorMgr.GetActor(actorEntity.UID);
                actor.transform.position = targetPos;
            }
#endif
        }

        public static void SkillActorBeatBack(CombatActorEntity actorEntity, Vector3 endPos, float duration, System.Action<CombatActorEntity> moveFinish)
        {
            actorEntity.SetPosition(endPos);
#if !SERVER
            if (BattleLogicManager.Instance.IsOpenBattleViewLayer)
            {
                Creature actor = BattleManager.Instance.ActorMgr.GetActor(actorEntity.UID);
                actor.transform.DOMove(endPos, duration).SetEase(Ease.OutQuint).onComplete = () => { moveFinish?.Invoke(actor.mData); };
            }
#endif
        }

        public static async void SkillCreatHitEffect(CombatActorEntity targetEntity, CreatHitEffectTaskData taskData)
        {
#if !SERVER
            if (!BattleLogicManager.Instance.IsOpenBattleViewLayer)
            {
                return;
            }
            string EffectPrefabName = taskData.hitFxPrefabName;
            Creature targetCreature = BattleManager.Instance.ActorMgr.GetActor(targetEntity.UID);
            Transform parentTrans = null;
            if (string.IsNullOrEmpty(taskData.attachPoint)
                || taskData.attachPoint.Equals("body_hit"))
            {
                parentTrans = targetCreature.GetBone("body_hit");
            }
            else
            {
                parentTrans = targetCreature.GetBone(taskData.attachPoint);
            }
            if (parentTrans == null)
            {
                BattleLog.LogWarning("Cant find the Hit Effect Parent Trans " + taskData.attachPoint);
                return;
            }
            GameObject fx = await BattleResManager.Instance.CreatorFx(EffectPrefabName, parentTrans, Vector3.zero);
            if (fx == null)
            {
                return;
            }
            fx.name = EffectPrefabName;
            fx.transform.localPosition = taskData.offset;
            fx.transform.localScale = taskData.fxScale;
            fx.transform.localRotation = Quaternion.Euler(taskData.euler);
            if (taskData.SkillID > 0)
            {
                var aa = StrBuild.Instance.ToStringAppend("skill_", taskData.SkillID.ToString());
                WwiseEventManager.SendEvent(TransformTable.SkillHit, aa, targetCreature.gameObject);
            }
            if (targetCreature.mData.HasBuffControlType((int)ACTION_CONTROL_TYPE.control_9))
            {
                targetCreature.PlayAnim("flyhit2");
            }
            else if (!string.IsNullOrEmpty(taskData.hitAnim)
                     && !targetCreature.mData.IsDead)
            {
                targetCreature.PlayAnim(taskData.hitAnim);
            }
            WwiseEventManager.SendEvent(TransformTable.HeroVoice_Hit, targetEntity.ConfigID.ToString());
#endif
        }
    }
}