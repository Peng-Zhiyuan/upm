/* Created:Loki Date:2023-01-30*/

namespace BattleEngine.Logic
{
    using System.Collections.Generic;

    public class BuffClearControlTypeLogic : BuffEffectLogic
    {
        public override void ExecuteLogic()
        {
            int removeType = buffEffect.Param1;
            int removeTypeNum = buffEffect.Param2;
            List<int> removeList = new List<int>();
            int needRemoveNum = 0;
            var data = buffAbility.SelfActorEntity.TypeIdBuffs.GetEnumerator();
            while (data.MoveNext())
            {
                BuffRow row = BuffUtil.GetBuffRow(data.Current.Key, 1);
                if (row == null)
                {
                    BattleLog.LogError("Cant find the Buff ID " + data.Current.Key + "  lv 1");
                    continue;
                }
                if (row.controlType == removeType)
                {
                    removeList.Add(data.Current.Key);
                    needRemoveNum += 1;
                }
                if (needRemoveNum >= removeTypeNum)
                {
                    break;
                }
            }
            for (int i = 0; i < removeList.Count; i++)
            {
                BuffAbility removeBuff = buffAbility.SelfActorEntity.GetBuff(removeList[i]);
                removeBuff.EndAbility();
            }
            buffAbility.EndAbility();
            BattleLog.LogWarning(StrBuild.Instance.ToStringAppend("[BUFF] 净化控制类型BUFF ", ((ACTION_CONTROL_TYPE)removeType).ToString()));
        }

        public override void EndLogic() { }
    }
}