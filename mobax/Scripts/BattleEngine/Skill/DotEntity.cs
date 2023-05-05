namespace BattleEngine.Logic
{
    using System.Collections.Generic;
    using UnityEngine;

#if !SERVER
    using View;
#endif

    public class DotEntity : CombatUnitEntity
    {
        public int durationTime = 1;
        public int intervalTime = 1;
        public float radius = 1;
        public float height = 1;
        public List<Effect> Effects = new List<Effect>();
        public CombatActorEntity Owner;
        public SKILL_AFFECT_TARGET_TYPE AffectTargetType;
        private List<CombatUnitEntity> effectiveEntities = new List<CombatUnitEntity>();
#if !SERVER
        private GameObject dotEffect;
#endif

        private long time = 0;
        private long finalTime = 0;
        private bool effectiveOnce = false;
        private bool applyEffectDirect = false;

        public override void Awake(object initData)
        {
            effectiveEntities.Clear();
        }

        public override void Born(Vector3 pos, Vector3 rot, float _size = 1.0f)
        {
            effectiveEntities.Clear();
            base.Born(pos, rot, _size);
            SetLifeState(ACTOR_LIFE_STATE.Alive);
#if !SERVER
            if (BattleLogicManager.Instance.IsOpenBattleViewLayer)
            {
                this.GameObject.transform.position = pos;
            }
#endif
        }

        public void Init(SKILL_AFFECT_TARGET_TYPE affectTargetType, Vector3 scale, string _effectPrefab, int _durationTime, int _intervalTime, float _radius, float _height, List<Effect> _Effects, CombatActorEntity _Owner, bool _effectiveOnce, bool _applyEffectDirect)
        {
            CreatEffect(scale, _effectPrefab);
            AffectTargetType = affectTargetType;
            effectPrefab = _effectPrefab;
            durationTime = _durationTime;
            intervalTime = _intervalTime;
            radius = _radius;
            height = _height;
            Effects = _Effects;
            Owner = _Owner;
            effectiveOnce = _effectiveOnce;
            applyEffectDirect = _applyEffectDirect;
            time = BattleTimeManager.Instance.NowTimestamp + intervalTime;
            finalTime = BattleTimeManager.Instance.NowTimestamp + durationTime;
            if (applyEffectDirect)
            {
                for (int i = 0; i < Effects.Count; i++)
                {
                    ApplyEffectTo(null, Effects[i]);
                }
            }
        }

        string effectPrefab;

        public async void CreatEffect(Vector3 scale, string effectPrefab)
        {
#if !SERVER
            if (string.IsNullOrEmpty(effectPrefab)
                || !BattleLogicManager.Instance.IsOpenBattleViewLayer)
            {
                return;
            }
            dotEffect = await BattleResManager.Instance.CreatorFx(effectPrefab, this.GameObject.transform, Vector3.zero);
            if (dotEffect == null)
            {
                return;
            }
            dotEffect.transform.localScale = scale;
            dotEffect.SetActive(true);
#endif
        }

        public override void LogicUpdate(float deltaTime)
        {
            if (applyEffectDirect)
            {
                Destroy(this);
                return;
            }
            if (BattleTimeManager.Instance.NowTimestamp <= time)
            {
                TriggerCheck();
            }
            else
            {
                time = time + intervalTime;
                effectiveEntities.Clear();
            }
            if (finalTime <= BattleTimeManager.Instance.NowTimestamp)
            {
                Destroy(this);
            }
        }

        private void TriggerCheck()
        {
            if (effectiveOnce && effectiveEntities.Count > 0)
            {
                return;
            }
            var data = BattleLogicManager.Instance.BattleData.allActorDic.GetEnumerator();
            while (data.MoveNext())
            {
                CombatActorEntity enemy = data.Current.Value;
                if (AffectTargetType == SKILL_AFFECT_TARGET_TYPE.Enemy)
                {
                    if (enemy.isAtker == Owner.isAtker)
                    {
                        continue;
                    }
                }
                else if (AffectTargetType == SKILL_AFFECT_TARGET_TYPE.Team)
                {
                    if (enemy.isAtker != Owner.isAtker)
                    {
                        continue;
                    }
                }
                if (enemy.IsCantSelect)
                {
                    continue;
                }
                Vector3 unitCenter = enemy.GetPositionXZ() + enemy.GetCenter();
                Vector3 center = GetPositionXZ();
                float curRadius = radius + enemy.GetHitRadiu();
                if (Mathf.Max(unitCenter.y + enemy.GetTouchHight() / 2, center.y + height / 2) - Mathf.Min(unitCenter.y - enemy.GetTouchHight() / 2, center.y - height / 2) < height + enemy.GetTouchHight())
                {
                    if (MathHelper.DoubleDistanceVect3(GetPositionXZ(), enemy.GetPositionXZ()) <= curRadius * curRadius)
                    {
                        effectiveEntities.Add(enemy);
                        OnTriggerEnter(enemy, null);
                        if (effectiveOnce)
                        {
                            break;
                        }
                    }
                }
            }
        }

        private void OnTriggerEnter(CombatActorEntity enemy, CombatUnitEntity enemyParty)
        {
            for (int i = 0; i < Effects.Count; i++)
            {
                ApplyEffectTo(enemy, Effects[i]);
            }
        }

        public void ApplyEffectTo(CombatActorEntity targetEntity, Effect effectItem)
        {
            if (effectItem is DamageEffect damageEffect)
            {
                var action = Owner.CreateCombatAction<DamageAction>();
                action.Target = targetEntity;
                action.partEntity = null;
                action.DamageSource = DamageSource.Attack;
                action.DamageEffect = damageEffect;
                action.attackDamage = false;
                action.ApplyDamage();
            }
            else if (effectItem is CureEffect cureEffect)
            {
                var action = Owner.CreateCombatAction<CureAction>();
                action.Target = targetEntity;
                action.CureEffect = cureEffect;
                action.ApplyCure();
            }
            else if (effectItem is TriggerSkillEffect triggerSkillEffect)
            {
                var action = Owner.CreateCombatAction<TriggerSkillAction>();
                action.Target = targetEntity;
                action.TriggerSkillEffect = triggerSkillEffect;
                action.InputPoint = GetPosition();
                action.ApplyTrigger();
            }
        }

        public override void OnDestroy()
        {
            SetLifeState(ACTOR_LIFE_STATE.Dead);
#if !SERVER
            if (BattleLogicManager.Instance.IsOpenBattleViewLayer
                && dotEffect != null)
            {
                BattleResManager.Instance.RecycleEffect(effectPrefab, dotEffect);
            }
#endif
            base.OnDestroy();
        }
    }
}