namespace BattleEngine.Logic
{
    using System;

    public abstract class ConditionEntity : Entity
    {
        private CombatActorEntity _ownActorEntity;
        public CombatActorEntity OwnActorEntity
        {
            get
            {
                if (_ownActorEntity == null)
                {
                    _ownActorEntity = GetParent<CombatActorEntity>();
                }
                return _ownActorEntity;
            }
        }

        public abstract void StartListen(Action callBack);
    }
}