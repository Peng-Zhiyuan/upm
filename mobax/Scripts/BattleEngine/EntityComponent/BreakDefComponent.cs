namespace BattleEngine.Logic
{
    public class BreakDefComponent : Component
    {
        public bool QteEnable { get; set; }
        public bool IsBreak { get; set; }

        private const float fWeakTime = 5f;
        private const float fDeductVal = 0.25f;

        public float DeductParam { get; private set; } = 1f;
        public float DamageParam { get; private set; }

        private CombatActorEntity Owner;

        public GameTimer breakTimer;

        public override void Setup()
        {
            base.Setup();
            Owner = Entity as CombatActorEntity;
            Subscribe<BreakDefEvent>(BreakDefEventHandler);
            Subscribe<BreakDefQteEvent>(BreakDefQteEventHandler);
            Enable = true;
        }

        private void BreakDefEventHandler(BreakDefEvent data)
        {
            QteEnable = false;
            IsBreak = true;
            DamageParam = BattleUtil.GetGlobalK(GlobalK.BreakDefDamage_41) * 0.001f;
            Owner.AttachBuff((int)BUFF_COMMON_CONFIG_ID.BREAK_VIM);
            Owner.TriggerActionPoint(ACTION_POINT_TYPE.PostBreakStatus);
            GameEventCenter.Broadcast(GameEvent.ShowBreakDamage, Owner, true);
            if (breakTimer == null)
            {
                breakTimer = new GameTimer(fWeakTime);
            }
            breakTimer.MaxTime = fWeakTime;
            breakTimer.Reset();
            if (Owner.CurrentSkillExecution != null)
            {
                Owner.CurrentSkillExecution.BreakActionsImmediate();
            }
        }

        private void BreakDefQteEventHandler(BreakDefQteEvent data)
        {
            QteEnable = true;
            breakTimer.MaxTime += 2.5f;
            DamageParam = BattleUtil.GetGlobalK(GlobalK.QTEBreakDefDamage_42) * 0.001f;
            GameEventCenter.Broadcast(GameEvent.ShowBreakDamageChanged, Owner, (int)(DamageParam * 100));
        }

        private void EndBreakState()
        {
            //qte影响随后的耐力值扣除
            DeductParam = QteEnable ? 0.75f : 1.25f;
            QteEnable = false;
            IsBreak = false;
            DamageParam = 1f;
            Owner.CurrentVim.Reset();
            Publish(new BreakStateEnd());
            Owner.OnBuffRemove((int)BUFF_COMMON_CONFIG_ID.BREAK_VIM);
            GameEventCenter.Broadcast(GameEvent.ShowBreakDamage, Owner, false);
        }

        public override void Dispose()
        {
            base.Dispose();
            UnSubscribe<BreakDefEvent>(BreakDefEventHandler);
            UnSubscribe<BreakDefQteEvent>(BreakDefQteEventHandler);
        }

        public void LogicUpdate(float delaTme)
        {
            if (breakTimer != null)
            {
                breakTimer.UpdateAsFinish(delaTme, EndBreakState);
            }
        }

        public void DeductVim(int val)
        {
            if (IsBreak)
                return;
            Owner.CurrentVim.Minus(val);
            if (!Owner.isAtker && Owner.CurrentVim.Value <= 0)
            {
                Publish(new BreakDefEvent() { uid = Owner.UID });
                GameEventCenter.Broadcast(GameEvent.BreakDef, Owner.UID);
            }
        }

        public bool CanBreak()
        {
            return Owner.CurrentVim.Percent() <= 0.1f;
        }

        public void TriggerBreakImmediate()
        {
            DeductVim(Owner.CurrentVim.Value);
        }

        private void SimulateDamageAction()
        {
            BDamageAction st = new BDamageAction();
            st.targetID = Owner.UID;
            //st.casterID = damageAction.Creator.UID;
            var hitdata = new BehitData();
            hitdata.SetState(HitType.Break);
            st.hitType = hitdata.hitType;
            //更新伤害事件
            Owner.SendDamageAction(st);
        }
    }
}