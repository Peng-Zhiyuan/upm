namespace BattleEngine.Logic
{
    using System.Collections.Generic;
    using UnityEngine;

    public sealed class RemoveBuffEvent
    {
        public CombatActorEntity CombatEntity { get; set; }
        public BuffAbility Buff { get; set; }
        public long BuffId { get; set; }
    }

    public sealed class TriggerBuffEvent
    {
        public CombatActorEntity CombatEntity { get; set; }
        public BuffAbility Buff { get; set; }
        public long BuffId { get; set; }
    }

    public sealed partial class CombatActorEntity
    {
        /// <summary>
        /// buffID,List<BuffAbility>
        /// </summary>
        public Dictionary<int, BuffAbility> TypeIdBuffs { get; set; } = new Dictionary<int, BuffAbility>();

        public BuffAbility AttachBuff(int buffID, int buffLv = 1, CombatActorEntity createActorEntity = null)
        {
            BuffRow row = BuffUtil.GetBuffRow(buffID, buffLv);
            if (row == null)
            {
                BattleLog.LogError("Buff Row is invalid! " + buffID + " lv" + buffLv);
                return null;
            }
            if (row.controlType != (int)ACTION_CONTROL_TYPE.None)
            {
                bool isSuccess = BattleControlUtil.RefreshBuffControlState(this, (ACTION_CONTROL_TYPE)row.controlType);
                if (!isSuccess)
                {
                    BattleLog.Log("Buff is invalid!");
                    return null;
                }
            }
            if (AssignEffectActionAbility.TryCreateAction(out var action))
            {
                action.Creator = createActorEntity == null ? this : createActorEntity;
                action.Target = this;
                action.Effect = new AddBuffEffect(row);
                action.ApplyAssignEffect();
                if (action.Buff == null
                    || action.Buff.buffRow == null
                    || IsSuccessForbidDebuff(action.Buff))
                {
                    action.OnDestroy();
                    return null;
                }
                TypeIdBuffs[buffID] = action.Buff;
                EventManager.Instance.SendEvent<BuffAbility>("AttachBuffSuccess", action.Buff);
                return action.Buff;
            }
            return null;
        }

        public T AttachBuff<T>(object configObject) where T : BuffAbility, new()
        {
            var buff = AttachAbility<T>(configObject);
            if (buff.buffRow == null)
            {
                BattleLog.Log("Buff Row is invalid!");
                return null;
            }
            if (buff.buffRow.controlType != (int)ACTION_CONTROL_TYPE.None)
            {
                bool isSuccess = BattleControlUtil.RefreshBuffControlState(this, (ACTION_CONTROL_TYPE)buff.buffRow.controlType);
                if (!isSuccess)
                {
                    BattleLog.Log("Buff is invalid!");
                    buff.OnDestroy();
                    return null;
                }
            }
            if (IsSuccessForbidDebuff(buff))
            {
                buff.OnDestroy();
                return null;
            }
            TypeIdBuffs[buff.buffRow.BuffID] = buff;
            EventManager.Instance.SendEvent<BuffAbility>("AttachBuffSuccess", buff);
            return buff;
        }

        public bool HasBuff(int buffID)
        {
            return TypeIdBuffs.ContainsKey(buffID);
        }

        public bool HasBuffControlType(int buffControlType)
        {
            var data = TypeIdBuffs.GetEnumerator();
            while (data.MoveNext())
            {
                if (data.Current.Value.buffRow.controlType == buffControlType)
                {
                    return true;
                }
            }
            return false;
        }

        public bool HasBuffControlType()
        {
            var data = TypeIdBuffs.GetEnumerator();
            while (data.MoveNext())
            {
                if (data.Current.Value.buffRow.controlType != (int)ACTION_CONTROL_TYPE.None)
                {
                    return true;
                }
            }
            return false;
        }
        

        public BuffAbility GetBuff(int buffID)
        {
            if (TypeIdBuffs.ContainsKey(buffID))
            {
                return TypeIdBuffs[buffID];
            }
            return null;
        }

        public void OnBuffRemove(int buffID)
        {
            if (!TypeIdBuffs.ContainsKey(buffID))
            {
                return;
            }
            if (TypeIdBuffs[buffID] == null)
            {
                TypeIdBuffs.Remove(buffID);
                return;
            }
            OnBuffRemove(TypeIdBuffs[buffID]);
        }

        public void OnBuffRemove(BuffAbility buffAbility)
        {
            if (!TypeIdBuffs.ContainsKey(buffAbility.buffRow.BuffID))
            {
                BattleLog.LogError("Cant find the Buff in actor entity " + buffAbility.buffRow.BuffID + "  " + ConfigID);
                return;
            }
            Publish(new RemoveBuffEvent() { CombatEntity = this, Buff = buffAbility, BuffId = buffAbility.Id });
            TypeIdBuffs.Remove(buffAbility.buffRow.BuffID);
            EventManager.Instance.SendEvent<CombatActorEntity>("BattleRefreshBuff", this);
            BattleLog.LogWarning("Remove Buff " + buffAbility.Name);
        }

        public void OnBuffTrigger(BuffAbility buffAbility)
        {
            this.Publish(new TriggerBuffEvent() { CombatEntity = this, Buff = buffAbility, BuffId = buffAbility.Id });
        }

        public void OnClearBuff()
        {
            var data = TypeIdBuffs.GetEnumerator();
            List<int> removeLst = new List<int>();
            while (data.MoveNext())
            {
                removeLst.Add(data.Current.Key);
            }
            for (int i = 0; i < removeLst.Count; i++)
            {
                BuffAbility buffAbility = GetBuff(removeLst[i]);
                buffAbility.EndAbility();
            }
            TypeIdBuffs.Clear();
        }
        public void OnClearDeBuff()
        {
            var data = TypeIdBuffs.GetEnumerator();
            List<int> removeLst = new List<int>();
            while (data.MoveNext())
            {
                if (data.Current.Value.buffRow.Gain == 1)
                {
                    removeLst.Add(data.Current.Key);    
                }
            }
            for (int i = 0; i < removeLst.Count; i++)
            {
                BuffAbility buffAbility = GetBuff(removeLst[i]);
                buffAbility.EndAbility();
            }
            TypeIdBuffs.Clear();
        }

        /// <summary>
        /// 抵御DEBUFF效果
        /// </summary>
        /// <param name="buff"></param>
        /// <returns></returns>
        private bool IsSuccessForbidDebuff(BuffAbility buff)
        {
            BuffAbility forbidDebuff = IsHaveForbidDebuff();
            if (buff.buffRow.Gain == 1
                && forbidDebuff != null)
            {
                int deBuffNum = 0;
                if (forbidDebuff.GetForbidDeBuffNum() > 1)
                {
                    forbidDebuff.SubForbidDeBuffNum(1);
                    deBuffNum = forbidDebuff.GetForbidDeBuffNum();
                }
                else
                {
                    forbidDebuff.EndAbility();
                }
                BattleLog.LogWarning(StrBuild.Instance.ToStringAppend("[BUFF] 抵御DEBUFF效果 还剩余:", deBuffNum.ToString()));
                return true;
            }
            return false;
        }

        public BuffAbility IsHaveForbidDebuff()
        {
            var data = TypeIdBuffs.GetEnumerator();
            while (data.MoveNext())
            {
                if (data.Current.Value.IsCanForbidDeBuff())
                {
                    return data.Current.Value;
                }
            }
            return null;
        }

        public BuffAbility IsHaveReBornBuff()
        {
            var data = TypeIdBuffs.GetEnumerator();
            while (data.MoveNext())
            {
                if (data.Current.Value.IsCanReborn() != null)
                {
                    return data.Current.Value;
                }
            }
            return null;
        }

        /// <summary>
        /// 执行重生操作
        /// </summary>
        /// <returns></returns>
        public bool ExectureReBornBuff()
        {
            var reBornBuff = IsHaveReBornBuff();
            if (CurrentHealth.Value <= 0
                && reBornBuff != null)
            {
                BuffEffect ReBornEffectBuff = null;
                for (int i = 0; i < reBornBuff.BuffEffectLogicLst.Count; i++)
                {
                    if (reBornBuff.BuffEffectLogicLst[i].GetBuffEffectType() == BUFF_EFFECT_TYPE.BE_REBORN)
                    {
                        ReBornEffectBuff = reBornBuff.BuffEffectLogicLst[i].GetBuffEffect;
                        break;
                    }
                }
                if (ReBornEffectBuff != null)
                {
                    int reBornHP = Mathf.FloorToInt((CurrentHealth.MaxValue * ReBornEffectBuff.Param1 * 0.001f + ReBornEffectBuff.Param2) * (1 + AttrData.GetValue(AttrType.CURE) * 0.001f));
                    AddHp(reBornHP);
                    reBornBuff.EndAbility();
                    BattleLog.LogWarning(StrBuild.Instance.ToStringAppend("[BUFF] BUFF重生 ", ConfigID.ToString()));
                    return true;
                }
            }
            return false;
        }
    }
}