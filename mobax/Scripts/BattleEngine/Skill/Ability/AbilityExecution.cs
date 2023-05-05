﻿namespace BattleEngine.Logic
{
    /// <summary>
    /// 能力执行体，能力执行体是实际创建执行能力表现，触发应用能力效果的地方
    /// 这里可以存一些表现执行相关的临时的状态数据
    /// </summary>
    public abstract class AbilityExecution : Entity
    {
        public AbilityEntity AbilityEntity { get; set; }
        private CombatActorEntity _ownerEntity;
        public CombatActorEntity OwnerEntity
        {
            get
            {
                if (_ownerEntity == null)
                {
                    _ownerEntity = GetParent<CombatActorEntity>();
                }
                return _ownerEntity;
            }
        }

        public override void Awake(object initData)
        {
            AbilityEntity = initData as AbilityEntity;
        }

        //开始执行
        public virtual void BeginExecute() { }

        //结束执行
        public virtual void EndExecute()
        {
            Destroy(this);
        }

        public T GetAbility<T>() where T : AbilityEntity
        {
            return AbilityEntity as T;
        }
    }
}