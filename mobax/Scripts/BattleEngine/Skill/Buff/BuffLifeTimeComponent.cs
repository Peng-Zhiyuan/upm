namespace BattleEngine.Logic
{
    using UnityEngine;

    /// <summary>
    /// 状态的生命周期组件
    /// </summary>
    public class BuffLifeTimeComponent : Component
    {
        public override bool Enable { get; set; } = true;
        public GameTimer LifeTimer { get; set; }
        private BuffAbility selfBuffAbility;

        public BuffAbility SelfBuffAbility
        {
            get
            {
                if (selfBuffAbility == null)
                {
                    selfBuffAbility = GetEntity<BuffAbility>();
                }
                return selfBuffAbility;
            }
        }

        public override void Setup()
        {
            BuffAbility buff = Entity as BuffAbility;
            float lifeTime = 0.0f;
            if (buff.buffRow != null)
            {
                lifeTime = buff.buffRow.Time * 0.001f;
            }
            else
            {
                SelfBuffAbility.EndAbility();
                return;
            }
            if (lifeTime > 0)
            {
                LifeTimer = new GameTimer(lifeTime);
            }
            else
            {
                LifeTimer = null;
            }
        }

        public void LogicUpdate(float deltaTime)
        {
            if (LifeTimer == null)
            {
                return;
            }
            if (LifeTimer.IsRunning)
            {
                LifeTimer.UpdateAsFinish(deltaTime, SelfBuffAbility.EndAbility);
            }
        }
    }
}