namespace BattleEngine.Logic
{
    using System.Collections.Generic;

    public class RoleData
    {
        public int Level;
        public int Star;
        public int Break;
    }

    public class RoleAttr
    {
        private BaseAttr BaseAttr = new BaseAttr();
        private Dictionary<AttrType, int> Attrs = new Dictionary<AttrType, int>();
        private List<int> ConfigAttrs = new List<int>();
        private Dictionary<AttrType, int> BufferAttrs = new Dictionary<AttrType, int>();
        private WeakType Weak;

        //loki 特定技能
        private Dictionary<int, Dictionary<SKILL_ATTR_TYPE, int>> SkillsAttrs = new Dictionary<int, Dictionary<SKILL_ATTR_TYPE, int>>();

        //private RoleData RoleData = new RoleData();
        public delegate void GetHeroDelegate(RoleData role);

        /*public int RoleID
        {
            get;
            set;
        }*/

        private HeroLevelRow info;

        public CombatActorEntity owner { get; set; }

        public float DifficultParam { get; set; } = 1f;

        //private int Level;
        private BattleItemInfo battleinfo;

        public RoleAttr(CombatActorEntity owner, BattleItemInfo battleinfo)
        {
            this.owner = owner;
            this.battleinfo = battleinfo;
            /*if (owner.isAtker)
            {
                var hero = HeroManager.Instance.GetHeroInfo(owner.ConfigID);
                if (hero.Unlocked)
                {
                    for (int i = 1; i < (int)AttrType.MAXNUM; i++)
                    {
                        var val = hero.GetAttribute((HeroAttr)(i));
                        ConfigAttrs.Add(val);
                    }
                }
                else
                {
                    foreach (var VARIABLE in battleinfo.att)
                    {
                        ConfigAttrs.Add((int)VARIABLE);
                    }
                }
            }
            else*/
            {
                foreach (var VARIABLE in battleinfo.att)
                {
                    ConfigAttrs.Add((int)VARIABLE);
                }
            }
            Caculate();
        }

        public void AddBaseAttr(AttrType type, int val)
        {
            BaseAttr.MergeAblity(type, val);
            RoleAttrHelper.RecalculateOneAttr(BaseAttr, type, ref Attrs);
            ///RoleAttrHelper.RecalculateOneAttr(BaseAttr, ref Attrs)
        }

        private void SetWeak(string weak)
        {
            if (string.IsNullOrEmpty(weak))
                return;
            /*string[] strs = weak.Split(',');
            foreach (var VARIABLE in strs)
            {
                FlagsHelper.Set(ref Weak, (WeakType)Int32.Parse(VARIABLE));
            }*/
        }

        public bool IsWeak(WeakType type)
        {
            return FlagsHelper.IsSet(Weak, type);
        }

        public void Init()
        {
            Caculate();
        }

        public void Recaculate()
        {
            BaseAttr.Clean();
            Caculate();
        }
 
        public void Caculate()
        {
            InitBaseAttr();
            //MergeLevelAttr();
            //MergeStarAttr();
            MeregeBufferAttr();
            /*if (owner.itemType == SceneItemType.Hero)
            {
                RefreshRoleData();
                MergeLevelAttr();
            }*/
            RoleAttrHelper.Recalculate(BaseAttr, ref Attrs);
            RoleAttrHelper.FixFinalAttr(DifficultParam, ref Attrs);
        }

        public void SetDifficult(float difficult)
        {
            DifficultParam = difficult;
            Recaculate();
            
            owner.CurrentHealth.SetMaxValue(GetValue(AttrType.HP));
            owner.CurrentHealth.Reset();
            owner.CurrentVim.SetMaxValue(GetValue(AttrType.VIM));
            owner.CurrentVim.Reset();
            owner.CurrentMp.Reset();
            owner.CurrentMp.SetMaxValue((int)BattleUtil.GetGlobalK(GlobalK.ENERGYMAX_30));
            owner.CurrentMp.Add((int)BattleUtil.GetGlobalK(GlobalK.ENERGYINIT_31));
        }

        private void MergeStarAttr()
        {
            /*HeroLevelRow info = ProtoStaticData.HeroLevelTable.TryGet(owner.con.Level * 1000 + );
            BaseAttr.MergeAblity(AttrType.HP, info.levelAttrs[0]);
            BaseAttr.MergeAblity(AttrType.ATK, info.levelAttrs[1]);
            BaseAttr.MergeAblity(AttrType.DEF, info.levelAttrs[2]);*/

            //BaseAttr.MergeAblity(AttrType.HP, 10000);
        }

        private void MergeLevelAttr()
        {
            HeroRow hero = StaticData.HeroTable.TryGet(this.battleinfo.id);
            if (hero == null)
                return;
            HeroLevelRow info = StaticData.HeroLevelTable.TryGet(hero.lvGroup * 10000 + this.battleinfo.lv);
            if (info == null)
                return;
            int id = hero.lvGroup * 10000 + this.battleinfo.lv;
            //Debug.LogError($"id = {id}");
            BaseAttr.MergeAblity(AttrType.HP, info.levelAttrs?[1] ?? 0);
            BaseAttr.MergeAblity(AttrType.ATK, info.levelAttrs?[2] ?? 0);
            BaseAttr.MergeAblity(AttrType.DEF, info.levelAttrs?[3] ?? 0);

            //BaseAttr.MergeAblity(AttrType.HP, 10000);
        }

        public void MeregeBufferAttr()
        {
            foreach (var VARIABLE in BufferAttrs)
            {
                BaseAttr.MergeAblity(VARIABLE.Key, VARIABLE.Value);
            }
        }

        public int GetValue(AttrType type)
        {
            int val;
            Attrs.TryGetValue(type, out val);
            return val;
        }

        public void InitBaseAttr()
        {
            for (int i = 0; i < ConfigAttrs.Count; i++)
            {
                BaseAttr.MergeAblity((AttrType)i, ConfigAttrs[i]);
            }
        }

        public int GetBaseValue(AttrType type)
        {
            return BaseAttr.GetAttr(type).value;
        }

        public Dictionary<AttrType, int> GetAllAttr()
        {
            return Attrs;
        }

        public void AddValue(AttrType type, int val)
        {
            if (Attrs.ContainsKey(type))
            {
                Attrs[type] += val;
            }
            else
            {
                Attrs.Add(type, val);
            }
        }

        public void RefreshRoleData()
        {
            /*var jsStr = @$"
            if (typeof(HeroUtilsPackage) === 'undefined')
                HeroUtilsPackage = require('Game/Hero/HeroManager/HeroUtils');
            HeroUtilsPackage.default.GetHeroInfo({RoleID})";*/

            //Debug.LogError("RoleiD = " + RoleID);
            //var heroGetter = JsEngine.Stuff.jsEnv.Eval<GetHeroDelegate>(jsStr);
            //heroGetter(RoleData);

            //Debug.LogError($"RoleID = {RoleID}  level = {RoleData.Level}  {RoleData.Star} {RoleData.Break}");
        }

        //修改buffer影响属性
        public void AddBuffAttr(AttrType type, int val)
        {
            if (!BufferAttrs.ContainsKey(type))
            {
                BufferAttrs.Add(type, 0);
            }
            BufferAttrs[type] += val;
            Recaculate();
        }

        public void AddSkillAttr(int skillID, SKILL_ATTR_TYPE type, int val)
        {
            if (!SkillsAttrs.ContainsKey(skillID))
            {
                var temp = new Dictionary<SKILL_ATTR_TYPE, int>();
                temp.Add(type, val);
                SkillsAttrs.Add(skillID, temp);
                return;
            }
            if (!SkillsAttrs[skillID].ContainsKey(type))
            {
                SkillsAttrs[skillID].Add(type, val);
                return;
            }
            SkillsAttrs[skillID][type] += val;
        }

        public void RemoveSkillAttr(int skillID, SKILL_ATTR_TYPE type, int val)
        {
            if (!SkillsAttrs.ContainsKey(skillID)
                || !SkillsAttrs[skillID].ContainsKey(type))
                return;
            SkillsAttrs[skillID][type] -= val;
        }

        public int GetSkillAttr(int skillID, SKILL_ATTR_TYPE type)
        {
            if (SkillsAttrs.ContainsKey(skillID)
                && SkillsAttrs[skillID].ContainsKey(type))
            {
                return SkillsAttrs[skillID][type];
            }
            return 0;
        }
    }
}