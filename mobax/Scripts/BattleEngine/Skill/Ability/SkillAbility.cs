namespace BattleEngine.Logic
{
    using System.Collections.Generic;
    using UnityEngine;

    public class SkillInitData
    {
        public SkillConfigObject skillConfigObject;
        public SkillRow skillRow;
    }

    public partial class SkillAbility : AbilityEntity
    {
        public SkillConfigObject SkillConfigObject { get; set; }
        public SkillRow SkillBaseConfig;
        public Vector3 InputTargetPos;
        public bool Spelling { get; set; }
        public GameTimer CooldownTimer { get; set; }

        public float CDTime
        {
            get
            {
                float cdTime = SkillBaseConfig.Cd * 0.001f / Mathf.Max(0.5f, (1 + OwnerEntity.AttrData.GetValue(AttrType.HASTE) / BattleUtil.GetGlobalK(GlobalK.FAST_8)));
                return cdTime;
            }
        }

        private float _skillFrameRate = 0.0f;
        public float SkillFrameRate
        {
            get
            {
                if (_skillFrameRate == 0.0f)
                {
                    _skillFrameRate = Mathf.FloorToInt(1000f / SkillConfigObject.fps) * 0.001f;
                }
                return _skillFrameRate;
            }
        }

        private List<Effect> SkillAbilityEffect = new List<Effect>();
        private List<AddBuffEffect> buffEffectLst = new List<AddBuffEffect>();

        public override async void Awake(object _initData)
        {
            base.Awake(_initData);
            SkillInitData initData = _initData as SkillInitData;
            SkillConfigObject = initData.skillConfigObject;
            SkillBaseConfig = initData.skillRow;
            if (SkillConfigObject == null
                || SkillBaseConfig == null)
            {
                BattleLog.LogError("技能表配置出错");
                return;
            }
            buffEffectLst.Clear();
            SkillAbilityEffect.Clear();
            if (SkillBaseConfig.damType == (int)DamageType.Real
                || SkillBaseConfig.damType == (int)DamageType.Physic
                || SkillBaseConfig.damType == (int)DamageType.Mana)
            {
                DamageEffect damageEffect = new DamageEffect();
                damageEffect.IsSkillEffect = true;
                damageEffect.DamageValueFormula = "";
                damageEffect.AddSkillEffectTargetType = LOCK_DOWN_TARGET_TYPE.CurrentTarget;
                SkillAbilityEffect.Add(damageEffect);
            }
            else if (SkillBaseConfig.damType == (int)DamageType.Cure)
            {
                CureEffect cureEffect = new CureEffect();
                cureEffect.IsSkillEffect = true;
                cureEffect.CureValueFormula = "";
                cureEffect.ParticleEffect = "";
                cureEffect.AddSkillEffectTargetType = LOCK_DOWN_TARGET_TYPE.CurrentTarget;
                SkillAbilityEffect.Add(cureEffect);
            }
            for (int i = 0; i < SkillBaseConfig.BuffLsts.Count; i++)
            {
                if (SkillBaseConfig.BuffLsts[i] == null
                    || SkillBaseConfig.BuffLsts[i].buffId == 0)
                {
                    continue;
                }
                BuffTrigger buffTrigger = SkillBaseConfig.BuffLsts[i];
                ///测试触发类型
                ///可移到其他函数执行
                BuffRow row = BuffUtil.GetBuffRow(buffTrigger.buffId, buffTrigger.buffLv);
                if (row == null)
                {
                    BattleLog.LogError("Cant find buffid " + buffTrigger.buffId + "  buffLv " + buffTrigger.buffLv);
                }
                else
                {
                    AddBuffEffect addBuffeffect = new AddBuffEffect(row, (BUFF_EFFECT_TARGET)buffTrigger.buffTarget, buffTrigger);
                    addBuffeffect.buffTrigger = buffTrigger;
                    buffEffectLst.Add(addBuffeffect);
                }
            }
            for (int i = 0; i < SkillAbilityEffect.Count; i++)
            {
                if (SkillAbilityEffect[i] == null)
                    continue;
                if (SkillAbilityEffect[i] is DamageEffect damageEffect)
                {
                    damageEffect.DamageValueFormula = "";
                }
                else if (SkillAbilityEffect[i] is CureEffect cureEffect)
                {
                    cureEffect.CureValueFormula = "";
                }
            }
            RegisterActiviteBuffEvent();
        }

        //激活
        public override void ActivateAbility()
        {
            base.ActivateAbility();
        }

        //结束
        public override void EndAbility()
        {
            UnRegisterActiviteBuffEvent();
            base.EndAbility();
        }

        public override AbilityExecution CreateExecution()
        {
            var execution = CreateWithParent<SkillAbilityExecution>(OwnerEntity, this);
            return execution;
        }

        public override void ApplyAbilityEffectsTo(CombatActorEntity targetEntity, CombatUnitEntity partEntity = null, float hurtRatio = 1, bool isOneHitKill = false)
        {
            List<Effect> tempLst = new List<Effect>();
            tempLst.AddRange(SkillAbilityEffect);
            tempLst.AddRange(buffEffectLst);
            if (tempLst.Count == 0)
                return;
            DamageSource damageSource = DamageSource.Attack;
            if (SkillBaseConfig.skillType == (int)SKILL_TYPE.SSPMove
                || SkillBaseConfig.skillType == (int)SKILL_TYPE.SSP)
            {
                damageSource = DamageSource.Skill;
            }
            else if (SkillBaseConfig.skillType == (int)SKILL_TYPE.SPSKL)
            {
                damageSource = DamageSource.SPSkill;
            }
            for (int i = 0; i < tempLst.Count; i++)
            {
                if (tempLst[i] == null)
                    continue;
                if (tempLst[i] is AddBuffEffect) { }
                else
                {
                    if (tempLst[i].AddSkillEffectTargetType == LOCK_DOWN_TARGET_TYPE.CurrentTarget)
                    {
                        if (targetEntity.CurrentHealth.Value > 0)
                        {
                            ApplyEffectTo(targetEntity, tempLst[i], damageSource, partEntity, hurtRatio, isOneHitKill, SkillBaseConfig);
                        }
                    }
                    else if (OwnerEntity.CurrentHealth.Value > 0)
                    {
                        ApplyEffectTo(OwnerEntity, tempLst[i], damageSource, partEntity, hurtRatio, isOneHitKill, SkillBaseConfig);
                    }
                }
            }
        }

        /// <summary>
        /// 刷新技能CD
        /// </summary>
        public void ResetSkillCd()
        {
            if (CooldownTimer == null)
            {
                return;
            }
            float cdTime = CDTime;
#if !SERVER
            if (OwnerEntity != null
                && !OwnerEntity.isAtker
                && BattleLogicManager.Instance.IsDefAddSkillCD)
            {
                cdTime += BattleSBManager.Instance.GetSingleCDOffsetTime();
                //BattleLog.LogError(OwnerEntity.UID + " Execute Skill " + SkillBaseConfig.SkillID + " " + CooldownTimer.MaxTime + "  " + CooldownTimer.GetTime() + "  => " + cdTime);
            }
#endif
            CooldownTimer.MaxTime = cdTime;
            CooldownTimer.Reset();
        }
    }
}