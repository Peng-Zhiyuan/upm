namespace BattleEngine.Logic
{
    using UnityEngine;

#if !SERVER
    using View;
#endif

    public class ShieldWallEntity : CombatUnitEntity
    {
        public float durationTime = 1;
        public int times = 1;
        public float radius = 1;
        public Vector3 size = Vector3.one;
        public CombatActorEntity Owner;
        private GameObject shieldWallEffect;
        public bool isAttachLookAt = true;
        public bool isAttachLock = false;
        public Vector3 offset = Vector3.one;
        public Vector3 angleOffset = Vector3.one;
        private float currentTime = 0;
        public SHIELD_WALL_TYPE wallTYpe = SHIELD_WALL_TYPE.Cube;
        string effectPrefab;
        string hitEffectPrefab;
        string destoryeDfectPrefab;

        public override void Awake(object initData) { }

        public override void Born(Vector3 pos, Vector3 rot, float _size = 1.0f)
        {
            base.Born(pos, rot, _size);
            SetLifeState(ACTOR_LIFE_STATE.Alive);
        }

        public void Init(CreatShieldWallData taskData, CombatActorEntity _Owner)
        {
            times = taskData.times;
            effectPrefab = taskData.effectPrefab;
            hitEffectPrefab = taskData.hitEffectPrefab;
            destoryeDfectPrefab = taskData.destoryEffectPrefab;
            wallTYpe = taskData.wallTYpe;
            durationTime = taskData.durationTime * 0.001f;
            radius = taskData.radius;
            size = taskData.size;
            Owner = _Owner;
            currentTime = 0.0f;
            isAttachLookAt = taskData.isAttachLookAt;
            isAttachLock = taskData.isAttachLock;
            offset = taskData.offset;
            angleOffset = taskData.angleOffset;
#if !SERVER
            if (BattleLogicManager.Instance.IsOpenBattleViewLayer)
            {
                CreatEffect(taskData.scale, taskData.effectPrefab);
            }
#endif
        }
#if !SERVER
        public async void CreatEffect(Vector3 scale, string effectPrefab)
        {
            if (Owner != null
                && Owner is CombatActorEntity)
            {
                Creature actor = BattleManager.Instance.ActorMgr.GetActor(Owner.UID);
                shieldWallEffect = await BattleResManager.Instance.CreatorFx(effectPrefab, actor.transform, Vector3.zero);
            }
            else
            {
                shieldWallEffect = await BattleResManager.Instance.CreatorFx(effectPrefab, this.GameObject.transform, Vector3.zero);
            }
            if (shieldWallEffect == null)
            {
                return;
            }
            shieldWallEffect.transform.localScale = scale;
        }
#endif

        public override void LogicUpdate(float deltaTime)
        {
            if (currentTime >= durationTime)
            {
                Destroy(this);
                return;
            }
            if (isAttachLock && Owner != null)
            {
                if (isAttachLookAt)
                {
                    Vector3 pos = Quaternion.Euler(Owner.GetEulerAngles()) * offset + Owner.GetPosition();
                    Vector3 rot = Quaternion.Euler(angleOffset).eulerAngles;
                    Born(pos, rot);
                }
                else
                {
                    Vector3 pos = offset + Owner.GetPosition();
                    Vector3 rot = Quaternion.Euler(angleOffset).eulerAngles;
                    Born(pos, rot);
                }
            }
            currentTime += deltaTime;
        }

        public override void OnDestroy()
        {
            SetLifeState(ACTOR_LIFE_STATE.Dead);
#if !SERVER
            if (shieldWallEffect != null
                && BattleLogicManager.Instance.IsOpenBattleViewLayer)
            {
                BattleResManager.Instance.RecycleEffect(effectPrefab, shieldWallEffect);
                CreatDestoryEffect(GetPosition());
            }
#endif
            base.OnDestroy();
        }

#if !SERVER
        public async void CreatDestoryEffect(Vector3 pos)
        {
            if (!string.IsNullOrEmpty(destoryeDfectPrefab))
            {
                GameObject destroyEffect = await BattleResManager.Instance.CreatorFx(destoryeDfectPrefab);
                if (destroyEffect == null)
                {
                    return;
                }
                destroyEffect.transform.position = pos;
                destroyEffect.transform.localScale = Vector3.one;
                GameObject.Destroy(destroyEffect, 10f);
                TimerMgr.Instance.BattleSchedulerTimer(5, () => { BattleResManager.Instance.ReleaseEffect(destoryeDfectPrefab); }, false, "CreatDestoryEffect");
            }
        }
#endif
        public async void CreatHitEffect(Vector3 pos)
        {
#if !SERVER
            if (!string.IsNullOrEmpty(hitEffectPrefab))
            {
                GameObject destroyEffect = await BattleResManager.Instance.CreatorFx(hitEffectPrefab);
                if (destroyEffect == null)
                {
                    return;
                }
                destroyEffect.transform.position = pos;
                destroyEffect.transform.localScale = Vector3.one;
            }
#endif
        }
    }
}