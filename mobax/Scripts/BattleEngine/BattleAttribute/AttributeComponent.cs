using UnityEngine;

namespace BattleEngine.Logic
{
    /// <summary>
    /// 战斗属性数值组件，在这里管理所有角色战斗属性数值的存储、变更、刷新等
    /// </summary>
    public sealed class AttributeComponent : Component
    {
        private RoleAttr Attrs;

        public void SetDifficult(float diff)
        {
            Attrs.SetDifficult(diff);
        }

        //public readonly List<FloatNumeric> attributeNumerics = new List<FloatNumeric>();
        public int Att_Attack
        {
            get { return GetValue(AttrType.ATK); }
        }

        public int GetValue(AttrType type)
        {
            return Attrs.GetValue(type);
        }

        public int GetBaseValue(AttrType type)
        {
            return Attrs.GetBaseValue(type);
        }

        public int GetSkillValue(int skillID, SKILL_ATTR_TYPE type)
        {
            return Attrs.GetSkillAttr(skillID, type);
        }

        public int Att_Defence
        {
            get { return GetValue(AttrType.DEF); }
        }
        public float Att_Move
        {
            get
            {
                if (GetValue(AttrType.MOVESPEED) == 0)
                {
                    return 1.0f;
                }
                return GetValue(AttrType.MOVESPEED) * 0.01f;
            }
        }
        public int Att_AttackRange
        {
            get { return GetValue(AttrType.RAGE); }
        }
        public float Att_AttackSpeed
        {
            get
            {
                if (GetValue(AttrType.ATKSPEED) == 0)
                {
                    return 1.0f;
                }
                return GetValue(AttrType.ATKSPEED) * 0.01f;
            }
        }

        public void Initialize(BattleItemInfo battleinfo, CombatActorEntity owner)
        {
            Attrs = new RoleAttr(owner, battleinfo);
        }

        //修改buffer影响属性
        public void AddBuffAttr(AttrType type, int val)
        {
            Attrs.AddBuffAttr(type, val);
        }

        public void AddSkillAttr(int skillID, SKILL_ATTR_TYPE type, int val)
        {
            Attrs.AddSkillAttr(skillID, type, val);
        }

        public void RemoveSkillAttr(int skillID, SKILL_ATTR_TYPE type, int val)
        {
            Attrs.RemoveSkillAttr(skillID, type, val);
        }

        public int ATT_MANA_ATK_VALUE(MANA_TYPE type)
        {
            switch (type)
            {
                case MANA_TYPE.FIRE:
                    return GetValue(AttrType.FIREATK);
                case MANA_TYPE.ELEC:
                    return GetValue(AttrType.ELECATK);
                case MANA_TYPE.WATER:
                    return GetValue(AttrType.WATERATK);
                case MANA_TYPE.WIND:
                    return GetValue(AttrType.WINDATK);
                case MANA_TYPE.LIGHE:
                    return GetValue(AttrType.LIGHEATK);
                case MANA_TYPE.DARK:
                    return GetValue(AttrType.DARKATK);
            }
            return 0;
        }

        public int ATT_MANA_DEF_VALUE(MANA_TYPE type)
        {
            switch (type)
            {
                case MANA_TYPE.FIRE:
                    return GetValue(AttrType.FIREDEF);
                case MANA_TYPE.ELEC:
                    return GetValue(AttrType.ELECDEF);
                case MANA_TYPE.WATER:
                    return GetValue(AttrType.WATERDEF);
                case MANA_TYPE.WIND:
                    return GetValue(AttrType.WINDDEF);
                case MANA_TYPE.LIGHE:
                    return GetValue(AttrType.LIGHEDEF);
                case MANA_TYPE.DARK:
                    return GetValue(AttrType.DARKDEF);
            }
            return 0;
        }
    }
}