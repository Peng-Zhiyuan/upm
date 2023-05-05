using BattleSystem.ProjectCore;
using UnityEngine;

namespace BattleEngine.Logic
{
    public class BuffChangeAvatarLogic : BuffEffectLogic
    {
        public override void ExecuteLogic()
        {
            BattleLog.LogWarning("[BUFF] Change Avatar ");
#if !SERVER
            if (BattleLogicManager.Instance.IsOpenBattleViewLayer)
            {
                CombatActorEntity actorEntity = buffAbility.SelfActorEntity;
                Creature role = BattleManager.Instance.ActorMgr.GetActor(actorEntity.UID);
                if (role == null
                    || role.GetModelObject == null)
                {
                    return;
                }
                RoleSkillReplaceAvatar avatar = role.GetModelObject.GetComponent<RoleSkillReplaceAvatar>();
                if (avatar == null)
                {
                    return;
                }
                avatar.ExcuteAvatarChange();
            }
#endif
        }

        public override void EndLogic()
        {
            BattleLog.LogWarning("[BUFF] Reset Avatar ");
#if !SERVER
            if (BattleLogicManager.Instance.IsOpenBattleViewLayer)
            {
                CombatActorEntity actorEntity = buffAbility.SelfActorEntity;
                Creature role = BattleManager.Instance.ActorMgr.GetActor(actorEntity.UID);
                if (role == null
                    || role.GetModelObject == null)
                {
                    return;
                }
                RoleSkillReplaceAvatar avatar = role.GetModelObject.GetComponent<RoleSkillReplaceAvatar>();
                if (avatar == null)
                {
                    return;
                }
                avatar.ResetAvatarChange();
            }
#endif
        }
    }
}