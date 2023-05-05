namespace BattleEngine.Logic
{
    /// <summary>
    /// 属性变化
    /// </summary>
    public class BuffChangeAttrLogic : BuffEffectLogic
    {
        private bool isChangeAttr = false;

        private int executeStackNum = 0;

        public override void InitData(BuffAbility _buffAbility, BuffEffect _buffEffect)
        {
            base.InitData(_buffAbility, _buffEffect);
            isChangeAttr = false;
        }

        public override void ExecuteLogic()
        {
            if (buffEffect == null)
            {
                return;
            }
            if (buffEffect.Param1 == (int)AttrType.RAGE)
            {
                buffAbility.SelfActorEntity.CurrentMp.Add(buffEffect.Param2);
            }
            else
            {
                isChangeAttr = true;
                executeStackNum = buffAbility.StackNum;
                if (buffEffect.Param3 > 0)
                {
                    BuffAbility targetBuffAbility = buffAbility.SelfActorEntity.GetBuff(buffEffect.Param3);
                    if (targetBuffAbility != null)
                    {
                        executeStackNum += targetBuffAbility.StackNum;
                    }
                }
                buffAbility.SelfActorEntity.AttrData.AddBuffAttr((AttrType)buffEffect.Param1, buffEffect.Param2 * executeStackNum);
            }
            BattleLog.LogWarning(StrBuild.Instance.ToStringAppend("[BUFF] 属性改变 开始：", buffAbility.SelfActorEntity.ConfigID.ToString(), " ", ((AttrType)buffEffect.Param1).ToString(), " ", (buffEffect.Param2 * executeStackNum).ToString()));
        }

        public override void EndLogic()
        {
            if (!isChangeAttr)
            {
                return;
            }
            isChangeAttr = false;
            buffAbility.SelfActorEntity.AttrData.AddBuffAttr((AttrType)buffEffect.Param1, buffEffect.Param2 * -1 * executeStackNum);
            BattleLog.LogWarning(StrBuild.Instance.ToStringAppend("[BUFF] 属性改变 结束：", buffAbility.SelfActorEntity.ConfigID.ToString(), " ", ((AttrType)buffEffect.Param1).ToString(), " ", (buffEffect.Param2 * -1 * executeStackNum).ToString()));
        }
    }
}