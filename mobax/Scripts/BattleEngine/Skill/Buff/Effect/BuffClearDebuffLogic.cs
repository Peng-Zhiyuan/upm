/* Created:Loki Date:2022-08-31*/

namespace BattleEngine.Logic
{
    using System.Collections.Generic;

    /// <summary>
    /// 净化减益
    /// </summary>
    public class BuffClearDebuffLogic : BuffEffectLogic
    {
        public override void ExecuteLogic()
        {
            int removeID = buffEffect.Param1;
            int removeNum = buffEffect.Param2;
            if (removeID == 0)
            {
                List<int> removeList = new List<int>();
                int needRemoveNum = 0;
                var data = buffAbility.SelfActorEntity.TypeIdBuffs.GetEnumerator();
                while (data.MoveNext())
                {
                    BuffRow row = BuffUtil.GetBuffRow(data.Current.Key, 1);
                    if (row == null)
                    {
                        BattleLog.LogError("Cant find the Buff ID " + data.Current.Key);
                        continue;
                    }
                    if (row.Gain == 1)
                    {
                        removeList.Add(data.Current.Key);
                        needRemoveNum += 1;
                    }
                    if (needRemoveNum >= removeNum)
                    {
                        break;
                    }
                }
                for (int i = 0; i < removeList.Count; i++)
                {
                    BuffAbility removeBuff = buffAbility.SelfActorEntity.GetBuff(removeList[i]);
                    removeBuff.EndAbility();
                }
            }
            else
            {
                BuffAbility removeBuff = buffAbility.SelfActorEntity.GetBuff(removeID);
                removeBuff.EndAbility();
            }
            buffAbility.EndAbility();
            BattleLog.LogWarning("[BUFF] 净化减益BUFF ");
        }

        public override void EndLogic() { }
    }
}