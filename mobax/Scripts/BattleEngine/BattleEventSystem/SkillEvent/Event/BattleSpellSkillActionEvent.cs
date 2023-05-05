using UnityEngine;

namespace BattleEngine.Logic
{
    public sealed class BattleSpellSkillActionEvent : IBattleEvent
    {
        public string GetEventName()
        {
            return "SpellSkillActionEvent";
        }

        public void Excute(BattleData battleData, object obj)
        {
            BattleSkillEventData data = obj as BattleSkillEventData;
            if (!battleData.allActorDic.ContainsKey(data.actorUID))
            {
                return;
            }
            CombatActorEntity actorEntity = battleData.allActorDic[data.actorUID];
            if (actorEntity.CurrentHealth.Value <= 0)
                return;
            CombatActorEntity targetEntity = BattleUtil.GetTargetActorEntity(battleData, actorEntity);
            if (targetEntity != null
                && targetEntity.CurrentHealth.Value > 0)
            {
                Vector3 forward = (targetEntity.GetPositionXZ() - actorEntity.GetPositionXZ()).normalized;
                actorEntity.SetForward(forward);
            }
            uint skillID = data.skillId;
            SkillAbility skillAbility = null;
            if (actorEntity.SkillSlots.ContainsKey(skillID))
            {
                skillAbility = actorEntity.SkillSlots[skillID];
            }
            if (skillAbility == null)
            {
                return;
            }
            actorEntity.SetActionState(ACTOR_ACTION_STATE.ATK);
            if (actorEntity.SpellSkillActionAbility.TryCreateAction(out var action))
            {
                action.SkillAbility = skillAbility;
                action.SkillAbilityExecution = action.SkillAbility.CreateExecution() as SkillAbilityExecution;
                if (action.SkillAbilityExecution == null)
                {
                    return;
                }
                action.SkillAbilityExecution.targetActorEntity = targetEntity;
                action.SkillAbilityExecution.AllTargetActorEntity = BattleUtil.GetSkillTargetsActorEntity(battleData, actorEntity);
                actorEntity.SetSkillTargetInfos(null);
                action.SpellSkill();
                action.SkillAbility.ResetSkillCd();
            }
            else
            {
                BattleLog.LogError("Skill Action Create Fail");
            }
            actorEntity.UpdateHeroInfoView();
        }
    }
}