namespace BattleEngine.Logic
{
    using System;
    using nobnak.Gist.ObjectExt;
    using Sirenix.Serialization;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using UnityEngine;

    public sealed partial class CombatActorEntity
    {
        public SpellSkillActionAbility SpellSkillActionAbility { get; private set; }

#region 技能信息
        //技能信息
        public Dictionary<uint, SkillAbility> SkillSlots { get; set; } = new Dictionary<uint, SkillAbility>();
        /// <summary>
        /// 大招-手动触发
        /// </summary>
        public SkillAbility SPSKL;
        /// <summary>
        /// 自动技能-AI触发
        /// </summary>
        public SkillAbility SSP
        {
            get { return GetSSP(); }
        }
        /// <summary>
        /// 自动技能-独立AI触发
        /// </summary>
        public SkillAbility SSPAI;
        /// <summary>
        /// 位移技能
        /// </summary>
        public SkillAbility SSPMove;
        /// <summary>
        /// 普通攻击
        /// </summary>
        private SkillAbility atk;
        public SkillAbility ATK
        {
            get
            {
                if (atk != null)
                {
                    return atk;
                }
                if (atk == null)
                {
                    atk = GetAtk();
                }
                return atk;
            }
            set { atk = value; }
        }

        /// <summary>
        /// 随机触发攻击
        /// </summary>
        public SkillAbility RandomATK
        {
            get { return GetAtk(); }
        }
        // /// <summary>
        // /// 被动技能——AI触发
        // /// </summary>
        // public SkillAbility PassiveAI;
        /// <summary>
        /// 准备释放技能
        /// </summary>
        public SkillAbility ReadySkill;
#endregion

        public List<SkillComboAbility> SkillCombos = new List<SkillComboAbility>();

        //常规技能执行体
        public SkillAbilityExecution CurrentSkillExecution { get; set; }

        //被动技能执行体7
        public SkillAbilityExecution PassiveSkillExecution { get; set; }
        public SkillComboAbilityExecution CurrentSkillComboExecution { get; set; }
        public List<SkillAbilityExecution> SkillExecutions { get; set; } = new List<SkillAbilityExecution>();
        public List<SkillAbilityExecution> RemoveSkillExecutions { get; set; } = new List<SkillAbilityExecution>();

        public async Task AttachSkill(SkillRow skillRow)
        {
            SkillConfigObject config = null;
#if !SERVER
            ScriptableObject scriptableObject = await BucketManager.Stuff.Battle.GetOrAquireAsync<ScriptableObject>(string.Format(AddressablePathConst.SkillConfPath, skillRow.skillData));
            config = scriptableObject as SkillConfigObject;
#else
            //TO DO STF BattleServer
            string dataJson = FileManager.ReadText(string.Format(AddressablePathConst.SkillConfServerPath, skillRow.skillData));
            config = ScriptableObject.CreateInstance<SkillConfigObject>();
            JsonUtility.FromJsonOverwrite(dataJson, config);
            Debug.LogWarning(config.Name);
            Debug.LogWarning(config.actionElements.Count);
#endif
            if (config == null)
            {
                BattleLog.LogError("技能表配置出错: " + skillRow.skillData);
            }
            SkillAbility abilityA = AttachSkill(config, skillRow);
#if !SERVER
            if (BattleLogicManager.Instance.IsOpenBattleViewLayer)
            {
                SkillActionElementItem skillItem = null;
                for (int i = 0; i < abilityA.SkillConfigObject.actionElements.Count; i++)
                {
                    skillItem = abilityA.SkillConfigObject.actionElements[i];
                    if (skillItem.type == SKILL_ACTION_ELEMENT_TYPE.CameraAni)
                    {
                        CameraAnimActionElement element = skillItem as CameraAnimActionElement;
                        string path = AddressablePathConst.SkillEditorPathParse(element.camPathPrefab);
                        await BucketManager.Stuff.Battle.GetOrAquireAsync<GameObject>(path);
                    }
                }
            }
#endif
        }

        public SkillAbility AttachSkill(SkillConfigObject _configObject, SkillRow _skillRow)
        {
            if (_skillRow.SkillID != _configObject.ID)
            {
                Debug.LogError("The SkillEditorID is error " + _skillRow.SkillID + "~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
            }
            
            SkillInitData skillInitData = new SkillInitData() { skillConfigObject = _configObject, skillRow = _skillRow };
            var skill = AttachAbility<SkillAbility>(skillInitData);
            if (!SkillSlots.ContainsKey((uint)_skillRow.SkillID))
            {
                SkillSlots.Add((uint)_skillRow.SkillID, skill);
            }
            else
            {
                SkillSlots[(uint)_skillRow.SkillID] = skill;
            }
            if (skill.SkillBaseConfig.preTime > 0)
            {
                skill.CooldownTimer = new GameTimer(skill.SkillBaseConfig.preTime * 0.001f);
            }
            else
            {
                skill.CooldownTimer = new GameTimer(skill.CDTime);
            }
            if (_skillRow.skillType == (int)SKILL_TYPE.SPSKL)
            {
                // 装载 大招-手动触发
                SPSKL = skill;
            }
            else if (_skillRow.skillType == (int)SKILL_TYPE.SSPMove)
            {
                // 装载 位移技能-手动触发
                SSPMove = skill;
            }
            else
            {
                ReadySkill = skill;
            }
            return skill;
        }

        public void AttachSkillGroup()
        {
            SkillCombos = GetSkillCombos();
        }

        public SkillAbility GetSkill(uint skillId)
        {
            if (SkillSlots.ContainsKey(skillId))
                return SkillSlots[skillId];
            return null;
        }

        public bool isSpellingSkill()
        {
            if (CurrentSkillExecution == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public bool isSpellingSkillCombo()
        {
            if (CurrentSkillComboExecution == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public bool isSSPMoveCoolDown()
        {
            if (SSPMove == null)
            {
                return false;
            }
            else
            {
                return SSPMove.CooldownTimer.IsFinished;
            }
        }

        /// <summary>
        /// 大招技能
        /// </summary>
        public void SpellDriveSkill()
        {
            var maxMP = (int)BattleUtil.GetGlobalK(GlobalK.ENERGYMAX_30);
            if (CurrentMp.Value < maxMP)
            {
                return;
            }
            if (CurrentSkillExecution != null)
            {
                CurrentSkillExecution.BreakActions(SKILL_BREAK_CAUSE.UltraSkill, true);
            }
            SetTargetPos(GetPosition());
            ReadySkill = SPSKL;
        }

        /// <summary>
        /// 位移技能
        /// </summary>
        /// <param name="sspmoveTargetPos">移动目标点</param>
        /// <returns></returns>
        public void SpellMoveSkill(Vector3 sspmoveTargetPos)
        {
            if (SSPMove == null)
            {
                BattleLog.LogWarning("Cant find Move Skill");
                return;
            }
            if (!SSPMove.CooldownTimer.IsFinished)
            {
                return;
            }
            if (CurrentSkillExecution != null)
            {
                CurrentSkillExecution.BreakActions(SKILL_BREAK_CAUSE.UltraSkill, true);
            }
            SSPMove.InputTargetPos = sspmoveTargetPos;
            ReadySkill = SSPMove;
        }

        public void SpellItemSkill(uint skillID)
        {
            if (!SkillSlots.ContainsKey(skillID))
            {
                BattleLog.LogError("Cant find the skillID " + skillID);
                return;
            }
            SkillAbility targetSkill = GetSkill(skillID);
            if (CurrentSkillExecution != null)
            {
                CurrentSkillExecution.BreakActions(SKILL_BREAK_CAUSE.ManualAttack, true);
            }
            ReadySkill = targetSkill;
        }

        public BulletEntity CreatBullet(CastTrajectoryTaskData taskData, CombatActorEntity target, Vector3 pos, Vector3 rot, Vector3 targetPos, SkillAbility skillAbility)
        {
            var bullet = Entity.CreateWithParent<BulletEntity>(this, taskData);
            bullet.endPos = targetPos;
            bullet.skillAbility = skillAbility;
            bullet.target = target;
            bullet.Born(pos, rot, 1.0f);
            bullet.AddComponent<UpdateComponent>();
            return bullet;
        }

        public void BreakCurentSkillImmediate(System.Action _onBreak = null)
        {
            if (CurrentSkillExecution == null)
            {
                _onBreak?.Invoke();
                return;
            }
            CurrentSkillExecution.BreakActionsImmediate(_onBreak);
        }

        public void BreakCurentSkill(SKILL_BREAK_CAUSE bREAK_CAUSE, bool immediate = true, System.Action _onBreak = null)
        {
            if (CurrentSkillExecution == null)
            {
                _onBreak?.Invoke();
                return;
            }
            CurrentSkillExecution.BreakActions(bREAK_CAUSE, immediate, _onBreak);
        }

        public void BreakAllSkill(System.Action _onBreak = null)
        {
            if (PassiveSkillExecution != null)
            {
                PassiveSkillExecution.BreakActionsImmediate();
            }
            for (int i = 0; i < SkillExecutions.Count; i++)
            {
                SkillExecutions[i].BreakActionsImmediate();
            }
            SkillExecutions.Clear();
            _onBreak?.Invoke();
        }

        private SkillAbility currentChooseSkill = null;

        public SkillAbility GetAtk()
        {
            var skills = SkillSlots.GetEnumerator();
            currentChooseSkill = null;
            while (skills.MoveNext())
            {
                if (!skills.Current.Value.CooldownTimer.IsFinished)
                {
                    continue;
                }
                if (skills.Current.Value.SkillBaseConfig.skillType == (int)SKILL_TYPE.ATK
                    || skills.Current.Value.SkillBaseConfig.skillType == (int)SKILL_TYPE.SSP)
                {
                    if (currentChooseSkill != null
                        && currentChooseSkill.SkillBaseConfig.skillType > skills.Current.Value.SkillBaseConfig.skillType)
                    {
                        continue;
                    }
                    currentChooseSkill = skills.Current.Value;
                }
            }
            return currentChooseSkill;
        }

        public SkillAbility GetNormalAttack()
        {
            var skills = SkillSlots.GetEnumerator();
            while (skills.MoveNext())
            {
                if ((skills.Current.Value.SkillBaseConfig.skillType == (int)SKILL_TYPE.ATK))
                {
                    return skills.Current.Value;
                }
            }
            return null;
        }

        public SkillAbility GetSSP()
        {
            var skills = SkillSlots.GetEnumerator();
            List<SkillAbility> lst = new List<SkillAbility>();
            while (skills.MoveNext())
            {
                if (skills.Current.Value.SkillBaseConfig.skillType != (int)SKILL_TYPE.SSP)
                {
                    continue;
                }
                if (!skills.Current.Value.CooldownTimer.IsFinished)
                {
                    continue;
                }
                lst.Add(skills.Current.Value);
            }
            if (lst.Count <= 0)
            {
                return null;
            }
            if (lst.Count == 1)
            {
                return lst[0];
            }
            int index = BattleLogicManager.Instance.Rand.RandomVaule(0, lst.Count - 1);
            return lst[index];
        }

        public List<SkillComboAbility> GetSkillCombos()
        {
            List<SkillComboAbility> lst = new List<SkillComboAbility>();
            HeroRow heroRow = StaticData.HeroTable.TryGet(battleItemInfo.id);
            if (heroRow == null)
            {
                return lst;
            }
            UnitComboRow comboRow = StaticData.UnitComboTable.TryGet(heroRow.combID);
            if (comboRow == null)
            {
                return lst;
            }
            SkillComboAbility ability = AttachAbility<SkillComboAbility>(comboRow);
            lst.Add(ability);
            return lst;
        }

        public SkillComboAbility GetFirstSkillCombo()
        {
            UnitComboRow comboRow = null;
            var comboLst = StaticData.UnitComboTable.ElementList;
            for (int i = 0; i < comboLst.Count; i++)
            {
                if (comboLst[i].heroID == battleItemInfo.id)
                {
                    comboRow = comboLst[i];
                    SkillComboAbility ability = AttachAbility<SkillComboAbility>(comboRow);
                    return ability;
                }
            }
            return null;
        }

        public Dictionary<string, string> GetAllSkillAnim()
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            var skills = SkillSlots.GetEnumerator();
            List<SkillAbility> lst = new List<SkillAbility>();
            while (skills.MoveNext())
            {
                List<SkillActionElementItem> saeis = skills.Current.Value.SkillConfigObject.actionElements;
                for (int i = 0; i < saeis.Count; i++)
                {
                    if (saeis[i].type == SKILL_ACTION_ELEMENT_TYPE.Animation)
                    {
                        if (!dic.ContainsKey((saeis[i] as PlayAnimActionElement).anim))
                        {
                            dic.Add((saeis[i] as PlayAnimActionElement).anim, (saeis[i] as PlayAnimActionElement).animClipName);
                        }
                    }
                }
            }
            return dic;
        }
    }
}