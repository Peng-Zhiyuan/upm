namespace BattleEngine.Logic
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using behaviac;

    /// <summary>
    /// 战斗实体
    /// </summary>
    public sealed partial class CombatActorEntity : CombatUnitEntity
    {
#region Var
        private int campID; //阵营
        public int CampID
        {
            get { return campID; }
        }

        private int teamID = -1; //队伍
        public int TeamID
        {
            get { return teamID; }
        }

        public int TeamKey //队伍唯一索引
        {
            get { return 1000 * campID + teamID; }
        }

        public void SetTeamInfo(int campID, int teamID)
        {
            this.campID = campID;
            this.teamID = teamID;
        }

        public string UID
        {
            get { return battleItemInfo._id; }
        }

        public int ConfigID
        {
            get { return battleItemInfo.id; }
        }

        //阵容站位 0,1,2(替补位置 10,11,12)
        public int PosIndex { get; set; }

        /// <summary>
        /// 战斗配置 and 数据
        /// </summary>
        public BattleItemInfo battleItemInfo;

        //public RoleItemInfo roleItemInfo;

        public HealthPoint CurrentHealth { get; private set; } = new HealthPoint();
        public VimPoint CurrentVim { get; private set; } = new VimPoint();
        public MagicPoint CurrentMp { get; private set; } = new MagicPoint();
        private GameTimer mpAdCD;
        /// <summary>
        /// 是否集火中
        /// </summary>
        //private GameTimer ATKFocusCD; //集火CD
        // public bool IsATKFocus
        // {
        //     get { return ATKFocusCD != null; }
        // }
        private string atkFocusUID = "";
        public string ATKFoucusUID
        {
            get
            {
                if (!string.IsNullOrEmpty(atkFocusUID))
                {
                    CombatActorEntity foucusActor = BattleLogicManager.Instance.BattleData.GetActorEntity(atkFocusUID);
                    if (foucusActor == null
                        || foucusActor.IsDead)
                    {
                        atkFocusUID = "";
                    }
                }
                return atkFocusUID;
            }
            set { atkFocusUID = value; }
        }
        /// <summary>
        /// 是否保护中
        /// </summary>
        private GameTimer DEFFocusCD; //保护CD
        public bool IsDefFocus
        {
            get { return DEFFocusCD != null; }
        }
        private string defendTargetUID = "";
        public string DefendTargetUID
        {
            get { return defendTargetUID; }
        }

        public Dictionary<Type, ActionAbility> TypeActions { get; set; } = new Dictionary<Type, ActionAbility>();
        public DamageActionAbility DamageActionAbility { get; private set; }
        public CureActionAbility CureActionAbility { get; private set; }
        public AssignEffectActionAbility AssignEffectActionAbility { get; private set; }

        //破防组件
        public BreakDefComponent BreakDefComponent { get; private set; }

        //Combo信息
        public CombatContext CombatContext { get; set; }

        //运动控制器
        public KinematControl KinematControl { get; set; }

        //怒气
        public int Energy { get; set; }

        //是否死亡
        public bool IsDead
        {
            get { return CurrentHealth.Value <= 0; }
        }

        private INPUT_OPERATE_TYPE mAction_inputOperateType = INPUT_OPERATE_TYPE.None;

        public INPUT_OPERATE_TYPE Action_inputOperateType
        {
            set { mAction_inputOperateType = value; }
            get { return mAction_inputOperateType; }
        }

        private ACTOR_ACTION_STATE mAction_actorActionState = ACTOR_ACTION_STATE.None;

        public ACTOR_ACTION_STATE Action_actorActionState
        {
            set { mAction_actorActionState = value; }
            get { return mAction_actorActionState; }
        }

        //是否处于等待大招状态——零时添加
        private bool isWaitSPSkillState = false;

        public bool IsWaitSPSkillState
        {
            get { return isWaitSPSkillState; }
        }

        private long mbeginKeepSafeTime = 0;

        public long beginKeepSafeTime
        {
            get { return mbeginKeepSafeTime; }
            set { mbeginKeepSafeTime = value; }
        }

        /// <summary>
        /// 千分数 1000 = 100%
        /// 默认角速度拉满
        /// </summary>
        public float AngleSpeed = 1000F;

        //警戒范围
        public int alertRange = 0;

        //目标坐标
        private Vector3 mTargetPos;

        public Vector3 targetPos
        {
            get { return mTargetPos; }
        }

        private Vector3 _targetPosXZ = Vector3.zero;
        public Vector3 targetPosXZ
        {
            get { return _targetPosXZ; }
        }

        private void SetTargetPosXZ(Vector3 pos)
        {
            _targetPosXZ.x = pos.x;
            _targetPosXZ.y = 0;
            _targetPosXZ.z = pos.z;
        }

        public void SetTargetPos(Vector3 pos, bool isNeedKC = true)
        {
            mTargetPos = pos;
            if (isNeedKC)
            {
                SetTargetPosXZ(pos);
                Vector3 forward = (_targetPosXZ - GetPositionXZ()).normalized;
                forward.y = 0;
                KinematControl.Move(forward * AttrData.Att_Move);
            }
        }

        //是否是进攻方
        public bool isAtker
        {
            get { return TeamKey < BattleConst.DEFTeamKeyIndex; }
        }

        //是否是队长
        public string CommanderID = "";

        public bool isLeader
        {
            get { return battleItemInfo._id == CommanderID; }
        }

        //目标信息
        private int _targetTeamKey = -1;

        public int targetTeamKey
        {
            get { return _targetTeamKey; }
        }

        private string _targetKey = "";

        public string targetKey
        {
            get { return _targetKey; }
        }

        private List<string> _skilltTrgetKeys = new List<string>();

        public List<string> skillTargetKeys
        {
            get
            {
                if (_skilltTrgetKeys == null) _skilltTrgetKeys = new List<string>();
                return _skilltTrgetKeys;
            }
        }

        private string _targetPartkey = "";

        public string targetPartkey
        {
            get { return _targetPartkey; }
        }

        private long lastInputMoveTime = 0;

        public long LastInputMoveTime
        {
            get { return lastInputMoveTime; }
            set { lastInputMoveTime = value; }
        }

        public AttributeComponent AttrData;

        public ActionPointManageComponent mActionPointManageComponent;

        public ConditionManageComponent mConditionManageComponent;

        public CombatActorEntity()
        {
            SetLifeState(ACTOR_LIFE_STATE.Born);
        }

        //是否不可控制操作
        public bool IsNotOperable
        {
            get
            {
                if (CurrentLifeState == ACTOR_LIFE_STATE.Born
                    || CurrentLifeState == ACTOR_LIFE_STATE.Dead
                    || CurrentLifeState == ACTOR_LIFE_STATE.StopLogic
                    || CurrentLifeState == ACTOR_LIFE_STATE.Substitut
                    || CurrentLifeState == ACTOR_LIFE_STATE.Assist
                    || CurrentLifeState == ACTOR_LIFE_STATE.Guard
                    || CurrentHealth.Value <= 0)
                {
                    return true;
                }
                return false;
            }
        }

        //是否不能被选中
        public bool IsCantSelect
        {
            get
            {
                if (IsDead
                    || CurrentLifeState == ACTOR_LIFE_STATE.Born
                    || CurrentLifeState == ACTOR_LIFE_STATE.Dead
                    || CurrentLifeState == ACTOR_LIFE_STATE.God
                    || CurrentLifeState == ACTOR_LIFE_STATE.Substitut
                    || CurrentLifeState == ACTOR_LIFE_STATE.LookAt
                    || CurrentLifeState == ACTOR_LIFE_STATE.Assist)
                {
                    return true;
                }
                return false;
            }
        }
        /// <summary>
        /// 是否正在战斗中
        /// </summary>
        public bool IsFighting
        {
            get
            {
                if (CurrentLifeState == ACTOR_LIFE_STATE.Alive
                    || CurrentLifeState == ACTOR_LIFE_STATE.StopLogic
                    || CurrentLifeState == ACTOR_LIFE_STATE.God
                    || CurrentLifeState == ACTOR_LIFE_STATE.Guard)
                {
                    return true;
                }
                return false;
            }
        }

        private AgentActor agentActor = null;

        public AgentActor GetAI()
        {
            return agentActor;
        }

        private SkillAgent skillAgent = null;

        public bool isAnitJob(Job job)
        {
            return AnitJob == job;
        }

        public Job AnitJob { get; set; }

        public Job Job { get; set; }

        public void SetJob()
        {
            HeroRow unit = battleItemInfo.GetHeroRow();
            Job = (Job)unit.Job;
            if (unit.Job == (int)Job.JieWei)
            {
                AnitJob = Job.ZhiYuan;
            }
            else if (unit.Job == (int)Job.ZhiYuan)
            {
                AnitJob = Job.JieWei;
            }
            else if (unit.Job == (int)Job.YouJi)
            {
                AnitJob = Job.ZhenDi;
            }
            else if (unit.Job == (int)Job.ZhenDi)
            {
                AnitJob = Job.TuXi;
            }
            else if (unit.Job == (int)Job.TuXi)
            {
                AnitJob = Job.YouJi;
            }
        }

        public int Weak { get; set; }

        //怪物属性表中的字段
        public int Sort { get; set; }

        //警戒范围
        private int _warningRange = -1;
        public int WarningRange
        {
            get { return _warningRange; }
        }

        /// <summary>
        /// 自身携带的链接者UID
        /// </summary>
        public List<string> LinkerUIDLst = new List<string>();

        public Vector3 BornPos = Vector3.zero;
        public Vector3 BornRot = Vector3.zero;

        public bool isOpenAITree = true;
#endregion

#region Method
        public override void Awake()
        {
            AttrData = AddComponent<AttributeComponent>();
            mActionPointManageComponent = AddComponent<ActionPointManageComponent>();
            mConditionManageComponent = AddComponent<ConditionManageComponent>();
            SpellSkillActionAbility = AttachActionAbility<SpellSkillActionAbility>();
            DamageActionAbility = AttachActionAbility<DamageActionAbility>();
            CureActionAbility = AttachActionAbility<CureActionAbility>();
            AssignEffectActionAbility = AttachActionAbility<AssignEffectActionAbility>();
            CombatContext = (CombatContext)Master.GetTypeChildren<CombatContext>()[0];
            KinematControl = new KinematControl(this);
            BreakDefComponent = AddComponent<BreakDefComponent>();
        }

        public override void BornCharacters(Vector3 pos, Vector3 rot, float _size, HeroRow row)
        {
            base.BornCharacters(pos, rot, _size, row);
            BornPos = pos;
            BornRot = rot;
        }

        public void ResetInfoData()
        {
            _targetKey = "";
            _targetTeamKey = -1;
            CommanderID = "";
            ReadySkill = null;
            if (CurrentLifeState == ACTOR_LIFE_STATE.Born)
            {
                SetActionState(ACTOR_ACTION_STATE.Born);
            }
            else
            {
                SetActionState(ACTOR_ACTION_STATE.Idle);
            }
            Action_inputOperateType = INPUT_OPERATE_TYPE.None;
            BreakCurentSkillImmediate();
            ClearTargetInfo();
            ClearOverTauntedList();
            KinematControl.Init();
            lastInputMoveTime = 0;
            mpAdCD = new GameTimer(1);
            //ATKFocusCD = null;
            DEFFocusCD = null;
            defendTargetUID = "";
            ATKFoucusUID = "";
        }

        public void InitBattleInfo(ItemInfo info, int[] attr)
        {
            battleItemInfo = new BattleItemInfo(info, attr);
            InitBattleInfo(battleItemInfo);
        }

        public void InitBattleInfo(BattleItemInfo _battleItemInfo)
        {
            try
            {
                battleItemInfo = _battleItemInfo;
                AttrData.Initialize(battleItemInfo, this);
                SetJob();
                CurrentHealth.SetMaxValue(AttrData.GetValue(AttrType.HP));
                CurrentHealth.Reset();
                CurrentVim.SetMaxValue(AttrData.GetValue(AttrType.VIM));
                CurrentVim.Reset();
                CurrentMp.Reset();
                CurrentMp.SetMaxValue((int)BattleUtil.GetGlobalK(GlobalK.ENERGYMAX_30));
                CurrentMp.Add((int)BattleUtil.GetGlobalK(GlobalK.ENERGYINIT_31));
                mpAdCD = new GameTimer(1);
                ActiviteEntity();
                _warningRange = battleItemInfo.warnRange;
                InitBtree();
                InitSkillBtree();
            }
            catch (Exception ex)
            {
                BattleLog.LogError($"-------------- {ex.Message}");
            }
        }

        public void ActiviteEntity()
        {
            SetLifeState(ACTOR_LIFE_STATE.Alive);
            SetActionState(ACTOR_ACTION_STATE.Idle);
            InitBuffAbility();
        }

        public async void InitBtree()
        {
            if (agentActor != null)
            {
                return;
            }
            HeroRow row = battleItemInfo.GetHeroRow();
            agentActor = new AgentActor();
            // 这里先暂时使用中文作为映射键
            // 组装行动节点
            ACxData aCxData = new ACxData();
            aCxData.SetData(this, BattleLogicManager.Instance.BattleData);
            List<string> aCxFunc = await AIManager.GetAIExecuteFunc(row.Tree);
            List<ACx> actionLst = new List<ACx>();
            int index = 1000;
            foreach (string vValue in aCxFunc)
            {
                UnitAIConfigRow vInfo = StaticData.UnitAIConfigTable[int.Parse(vValue)];
                if (vInfo == null) continue;
                if (!AIManager.mAIDelegateObjectDic.ContainsKey(vInfo.Name))
                {
                    BattleLog.LogError("Cant find the AI Execute " + vInfo.Name);
                    continue;
                }
                Type executeFunc = AIManager.mAIDelegateObjectDic[vInfo.Name];
                object obj = Activator.CreateInstance(executeFunc, vInfo.Name, index);
                actionLst.Add(obj as ACx);
                index++;
            }
            agentActor.Register(aCxData, actionLst);
            string behavicPath = string.Format(AddressablePathConst.BehaviorsBsonPath, row.Tree);
            await BucketManager.Stuff.Battle.GetOrAquireAsync<TextAsset>(behavicPath);
            agentActor.btload(row.Tree);
            agentActor.btsetcurrent(row.Tree);
            if (agentActor == null)
            {
                BattleLog.LogError("agnetActor is null");
            }
        }

        public async void InitSkillBtree()
        {
            if (skillAgent != null)
            {
                return;
            }
            HeroRow row = battleItemInfo.GetHeroRow();
            skillAgent = new SkillAgent();
            // 这里先暂时使用中文作为映射键
            // 组装行动节点
            ACxData aCxData = new ACxData();
            aCxData.SetData(this, BattleLogicManager.Instance.BattleData);
            List<string> aCxFunc = await AIManager.GetAIExecuteFunc(row.skillTree);
            List<ACx> actionLst = new List<ACx>();
            int index = 1000;
            foreach (string vValue in aCxFunc)
            {
                UnitAIConfigRow vInfo = StaticData.UnitAIConfigTable[int.Parse(vValue)];
                if (vInfo == null) continue;
                if (!AIManager.mAIDelegateObjectDic.ContainsKey(vInfo.Name))
                {
                    BattleLog.LogError("Cant find the AI Execute " + vInfo.Name);
                    continue;
                }
                Type executeFunc = AIManager.mAIDelegateObjectDic[vInfo.Name];
                object obj = Activator.CreateInstance(executeFunc, vInfo.Name, index);
                actionLst.Add(obj as ACx);
                index++;
            }
            skillAgent.Register(aCxData, actionLst, true);
            string behavicPath = string.Format(AddressablePathConst.BehaviorsBsonPath, row.skillTree);
            //TextAsset behavicText = await AddressableRes.loadAddressableResAsync<TextAsset>(behavicPath);
            await BucketManager.Stuff.Battle.GetOrAquireAsync<TextAsset>(behavicPath);
            skillAgent.btload(row.skillTree);
            skillAgent.btsetcurrent(row.skillTree);
            if (skillAgent == null)
            {
                BattleLog.LogError("agnetActor is null");
            }
        }

        public void InitBuffAbility()
        {
            // if (battleItemInfo.bornRow == null || battleItemInfo.bornRow.extraBuff.Count == 0)
            //  {
            //      return;
            //  }
            //  for (int i = 0; i < battleItemInfo.bornRow.extraBuff.Count; i++)
            //  {
            //      if (battleItemInfo.bornRow.extraBuff[i] == 0 || !ProtoStaticData.BuffTable.ContainsKey(battleItemInfo.bornRow.extraBuff[i]))
            //          continue;
            //      AttachBuff(battleItemInfo.bornRow.extraBuff[i], this);
            //  }
        }

        public T CreateCombatAction<T>() where T : ActionExecution
        {
            if (CombatContext == null
                || CombatContext.GetComponent<CombatActionManageComponent>() == null)
            {
                return null;
            }
            var action = CombatContext.GetComponent<CombatActionManageComponent>().CreateAction<T>(this);
            return action;
        }

        /// <summary>
        /// 挂载能力，技能、被动、buff都通过这个接口挂载
        /// </summary>
        /// <param name="configObject"></param>
        public T AttachAbility<T>(object configObject) where T : AbilityEntity
        {
            var ability = Entity.CreateWithParent<T>(this, configObject);
            ability.OnSetParent(this);
            return ability;
        }

        public T AttachActionAbility<T>() where T : ActionAbility
        {
            var action = AttachAbility<T>(null);
            TypeActions.Add(typeof(T), action);
            return action;
        }

        public void SetActionState(ACTOR_ACTION_STATE state)
        {
            Action_actorActionState = state;
        }

        public override void OnUpdate(int currentFrame)
        {
            if (IsNotOperable || isWaitSPSkillState)
            {
                return;
            }
            AIUpdate(currentFrame);
            for (int i = 0; i < SkillExecutions.Count; i++)
            {
                SkillExecutions[i].OnUpdate(currentFrame);
            }
            for (int i = 0; i < RemoveSkillExecutions.Count; i++)
            {
                for (int j = 0; j < SkillExecutions.Count; j++)
                {
                    if (SkillExecutions[j] == null
                        || SkillExecutions[j].OwnerEntity == null
                        || SkillExecutions[j].SkillAbility.SkillBaseConfig.SkillID == RemoveSkillExecutions[i].SkillAbility.SkillBaseConfig.SkillID)
                    {
                        SkillExecutions.RemoveAt(j);
                        j--;
                    }
                }
            }
            if (RemoveSkillExecutions.Count > 0)
            {
                RemoveSkillExecutions.Clear();
            }
            if (PassiveSkillExecution != null)
            {
                PassiveSkillExecution.OnUpdate(currentFrame);
            }
            if (CurrentSkillComboExecution != null)
            {
                CurrentSkillComboExecution.OnUpdate(currentFrame);
            }
            var skills = SkillSlots.GetEnumerator();
            while (skills.MoveNext())
            {
                if (skills.Current.Value.SkillBaseConfig.skillType == (int)SKILL_TYPE.SSPMove)
                {
                    skills.Current.Value.CooldownTimer.UpdateAsFinish(BattleLogicDefine.LogicSecTime, delegate() { });
                }
                else
                {
                    skills.Current.Value.CooldownTimer.UpdateAsFinish(BattleLogicDefine.LogicSecTime);
                }
            }
            for (int i = 0; i < PassiveSkillLst.Count; i++)
            {
                PassiveSkillLst[i].CooldownTimer.UpdateAsFinish(BattleLogicDefine.LogicSecTime);
            }
            if (!BattleControlUtil.IsForbidMove(this, Action_inputOperateType == INPUT_OPERATE_TYPE.Move))
                KinematControl.Update(BattleLogicDefine.LogicSecTime);
            //塞入产生的BattleAction
            //ATKFocusCD?.UpdateAsFinish(BattleLogicDefine.LogicSecTime, () => { ATKFocusCD = null; });
            DEFFocusCD?.UpdateAsFinish(BattleLogicDefine.LogicSecTime, () =>
                            {
                                DEFFocusCD = null;
                                defendTargetUID = "";
                            }
            );
            var buffAbilityKeyLst = Keys2List(TypeIdBuffs.Keys);
            for (int i = 0; i < buffAbilityKeyLst.Count; i++)
            {
                if (TypeIdBuffs.ContainsKey(buffAbilityKeyLst[i])
                    && TypeIdBuffs[buffAbilityKeyLst[i]] != null)
                {
                    TypeIdBuffs[buffAbilityKeyLst[i]].LogicUpdate(BattleLogicDefine.LogicSecTime);
                }
                else
                {
                    OnBuffRemove(buffAbilityKeyLst[i]);
                }
            }
            BreakDefComponent?.LogicUpdate(BattleLogicDefine.LogicSecTime);
        }

        public static List<T> Keys2List<T, T1>(Dictionary<T, T1>.KeyCollection keys)
        {
            List<T> resKeys = new List<T>();
            foreach (T key in keys)
            {
                resKeys.Add(key);
            }
            return resKeys;
        }

        void AIUpdate(int currentFrame)
        {
            if (!isOpenAITree)
            {
                return;
            }
            if (agentActor != null)
            {
                agentActor.Simulate(currentFrame, BattleLogicDefine.LogicSecTime);
            }
            if (skillAgent != null
                && mAction_actorActionState == ACTOR_ACTION_STATE.ATK
                && mAction_actorActionState != ACTOR_ACTION_STATE.Dead)
            {
                skillAgent.Simulate(currentFrame, BattleLogicDefine.LogicSecTime);
            }
        }

        public void AddHp(int hp)
        {
            CurrentHealth.AddHp(hp);
            if (CurrentHealth.Value <= 0)
            {
                GameEventCenter.Broadcast(GameEvent.ActorDie, this);
            }
        }

        public void AddMp(int mp)
        {
            CurrentMp.Add(mp);
        }

        public void SetAutoTargetInfo(CombatActorEntity targetEntity, string partkey = "")
        {
            if (targetEntity == null
                || targetEntity.IsCantSelect)
            {
                return;
            }
            if (!string.IsNullOrEmpty(ATKFoucusUID))
            {
                return;
            }
            SetTargetInfo(targetEntity, partkey);
            //ATKFocusCD = null;
        }

        public void SetInputTargetInfo(CombatActorEntity targetEntity, string partkey = "")
        {
            if (targetEntity == null)
            {
                return;
            }
            lastInputMoveTime = 0;
            DEFFocusCD = null;
            defendTargetUID = "";
            // if (ATKFocusCD != null)`
            // {
            //     ATKFocusCD.Reset();
            // }
            // else
            // {
            //     int waitTime = (int)BattleUtil.GetGlobalK(GlobalK.AtkFocusOnFireTime_39);
            //     ATKFocusCD = new GameTimer(waitTime);
            // }
            ATKFoucusUID = targetEntity.UID;
            SetTargetInfo(targetEntity, partkey);
        }

        public void SetInputDefendTarget(CombatActorEntity defendEntity)
        {
            if (defendEntity == null)
            {
                return;
            }
            lastInputMoveTime = 0;
            //ATKFocusCD = null;
            ATKFoucusUID = "";
            if (DEFFocusCD != null)
            {
                DEFFocusCD.Reset();
            }
            else
            {
                int waitTime = (int)BattleUtil.GetGlobalK(GlobalK.DefFocusOnFiretime_40);
                DEFFocusCD = new GameTimer(waitTime);
            }
            defendTargetUID = defendEntity.UID;
        }

        private void SetTargetInfo(CombatActorEntity targetEntity, string partkey = "")
        {
            if (string.IsNullOrEmpty(targetEntity.UID))
            {
                BattleLog.LogError("Actor Entity UID " + targetEntity.ConfigID);
                return;
            }
            bool isSwitchTarget = !_targetKey.Equals(targetEntity.UID);
            _targetTeamKey = targetEntity.TeamKey;
            _targetKey = targetEntity.UID;
            var data = BattleLogicManager.Instance.BattleData.allActorDic.GetEnumerator();
            if (!string.IsNullOrEmpty(partkey))
            {
                _targetPartkey = partkey;
            }
            if (isSwitchTarget)
            {
                this.Publish(new SwitchTargetEvent() { targetUID = targetEntity.UID });
            }
            GameEventCenter.Broadcast(GameEvent.TargetChanged, this);
        }

        public void SetSkillTargetInfos(List<CombatActorEntity> targetEntitys)
        {
            if (targetEntitys == null)
            {
                _skilltTrgetKeys.Clear();
            }
            else
            {
                _skilltTrgetKeys.Clear();
                for (int i = 0; i < targetEntitys.Count; i++)
                {
                    _skilltTrgetKeys.Add(targetEntitys[i].UID);
                }
            }
        }

        public void ClearTargetInfo()
        {
            _targetTeamKey = -1;
            _targetKey = "";
            _targetPartkey = "";
            _skilltTrgetKeys.Clear();
            if (!string.IsNullOrEmpty(ATKFoucusUID))
            {
                CombatActorEntity atkFocusActor = BattleLogicManager.Instance.BattleData.GetActorEntity(ATKFoucusUID);
                if (atkFocusActor == null
                    || !atkFocusActor.IsDead)
                {
                    BattleLog.LogError("The Foucus Enemy is alive!!!!!!!!!!!!!!");
                }
                else
                {
                    ATKFoucusUID = "";
                }
            }
            // if (IsATKFocus)
            // {
            //     ATKFocusCD = null;
            // }
        }

        public void ClearOverTauntedList()
        {
            OTList.Clear();
            OTNumLst.Clear();
        }

        public void ClearTargetParts()
        {
            _targetPartkey = "";
        }

        public float GetCurrentAtkDistance()
        {
            SkillAbility normalAtk = GetNormalAttack();
            float atkDis = normalAtk != null ? normalAtk.SkillBaseConfig.Range * 0.01f : 1.0f;
            if (CurrentSkillExecution != null)
            {
                atkDis = CurrentSkillExecution.SkillAbility.SkillBaseConfig.Range * 0.01f;
            }
            else if (ReadySkill != null)
            {
                atkDis = ReadySkill.SkillBaseConfig.Range * 0.01f;
            }
            else if (RandomATK != null)
            {
                atkDis = RandomATK.SkillBaseConfig.Range * 0.01f;
            }
            return atkDis;
        }

        public float GetNormalAtkDistance()
        {
            if (RandomATK == null)
            {
                return 1;
            }
            float atkDis = RandomATK.SkillBaseConfig.Range * 0.01f;
            return atkDis;
        }
#endregion

#region Event
        /// <summary>
        /// 创建行动
        /// </summary>
        public T CreateAction<T>() where T : ActionExecution
        {
            if (CombatContext == null)
            {
                return null;
            }
            var action = CombatContext.GetComponent<CombatActionManageComponent>().CreateAction<T>(this);
            return action;
        }

#region 行动点事件
        private ActionPointManageComponent _actionPointManageComponent;

        public ActionPointManageComponent ActionPointManageComponent
        {
            get
            {
                if (_actionPointManageComponent == null)
                {
                    _actionPointManageComponent = GetComponent<ActionPointManageComponent>();
                }
                return _actionPointManageComponent;
            }
        }

        public void ListenActionPoint(ACTION_POINT_TYPE actionPointType, Action<ActionExecution> action)
        {
            ActionPointManageComponent.AddListener(actionPointType, action);
        }

        public void UnListenActionPoint(ACTION_POINT_TYPE actionPointType, Action<ActionExecution> action)
        {
            ActionPointManageComponent.RemoveListener(actionPointType, action);
        }

        public void TriggerActionPoint(ACTION_POINT_TYPE actionPointType, ActionExecution action = null)
        {
            ActionPointManageComponent.TriggerActionPoint(actionPointType, action);
        }
#endregion

        public void ReceiveDamage(ActionExecution combatAction, bool isDestory)
        {
            if (CurrentHealth.Value <= 0)
            {
                BattleLog.LogWarning("The Hero is dead ,but damage " + ConfigID);
                return;
            }
            if (!(combatAction is DamageAction))
            {
                return;
            }
            var damageAction = combatAction as DamageAction;
            if (CurrentHealth.Value < damageAction.DamageValue)
            {
                if (CurrentSkillExecution != null
                    && CurrentSkillExecution.SkillAbility.SkillBaseConfig.skillType == (int)SKILL_TYPE.SPSKL)
                {
                    damageAction.DamageValue = CurrentHealth.Value - 1;
                }
            }
            if (ConfigID == BattleConst.ShuiJinID)
            {
                if (damageAction.DamageValue > 0)
                {
                    BattleLog.LogWarning("Look Look");
                }
            }
            AddHp(-damageAction.DamageValue);
            BreakDefComponent.DeductVim(Mathf.FloorToInt(damageAction.VimValue * BreakDefComponent.DeductParam));
            if (ExectureReBornBuff())
            {
                BattleLog.LogWarning("Actor be reborn~~~~");
            }
            if (CurrentLifeState != ACTOR_LIFE_STATE.God
                && CurrentHealth.Value <= 0)
            {
                //检测是否有重生Buff,并触发
                SetLifeState(ACTOR_LIFE_STATE.Dead);
                battleItemInfo.battlePlayerRecord.KillMeUID = damageAction.Creator.UID;
                damageAction.Creator.battleItemInfo.battlePlayerRecord.killList.Add(UID);
                damageAction.Creator.TriggerActionPoint(ACTION_POINT_TYPE.PostKillHeroStatus, damageAction);
                OnClearBuff();
                EventManager.Instance.SendEvent<CombatActorEntity>("OnTriggerDeadPoint", this);
                TriggerActionPoint(ACTION_POINT_TYPE.PostDeadStatus, damageAction);
                CloseTriggerPassiveSkill();
            }
            BDamageAction st = new BDamageAction();
            st.damage = -damageAction.DamageValue;
            st.currentValue = CurrentHealth.Value;
            st.targetID = battleItemInfo._id;
            st.casterID = damageAction.Creator.UID;
            st.baoji = damageAction.IsCritical;
            st.hitType = damageAction.behitData.hitType;
            st.attackDamage = damageAction.attackDamage;
            st.atk = isAtker;
            st.DamageSource = damageAction.DamageSource;

            //更新伤害事件
            SendDamageAction(st);
            UpdateHeroInfoView();
            if (damageAction.OTNum > 0)
            {
                AddOTNum(damageAction.Creator.UID, (int)damageAction.OTNum);
            }

            //刷新Creater的怒气
            if (CurrentLifeState == ACTOR_LIFE_STATE.Dead)
            {
                combatAction.Creator.CurrentMp.Add((int)BattleUtil.GetGlobalK(GlobalK.ENERGYKILL_34));
#if !SERVER
                if (BattleLogicManager.Instance.IsOpenBattleViewLayer)
                {
                    BattleManager.Instance.KillEvent(damageAction.Creator);
                }
#endif
            }

            //刷新自己的怒气
            if (CurrentLifeState != ACTOR_LIFE_STATE.Dead)
            {
                CurrentMp.Add((int)BattleUtil.GetGlobalK(GlobalK.ENERGYATTACKED_33));
            }
            if (isDestory)
            {
                damageAction.EndExecute();
            }
        }

        public void ReceiveCure(ActionExecution combatAction)
        {
            if (CurrentHealth.Value <= 0)
            {
                BattleLog.LogWarning("The Hero is dead ,but cure " + ConfigID);
                return;
            }
            var cureAction = combatAction as CureAction;
            if (ConfigID == BattleConst.ShuiJinID)
            {
                if (cureAction.CureValue > 0)
                {
                    BattleLog.LogWarning("Look Look");
                }
            }
            AddHp(cureAction.CureValue);
            if (cureAction.CureValue < 0)
                BattleLog.LogWarning("治疗值小于0！！！！！Cure : " + cureAction.CureValue);
            BDamageAction st = new BDamageAction();
            st.damage = (int)cureAction.CureValue;
            st.targetID = battleItemInfo._id;
            st.casterID = cureAction.Creator.UID;
            st.currentValue = CurrentHealth.Value;
            st.atk = isAtker;
            st.hitType = cureAction.behitData.hitType;
            //更新伤害事件
            SendDamageAction(st);
            UpdateHeroInfoView();
        }

        public void SendDamageAction(BDamageAction damagerAction)
        {
            if (BattleLogicManager.HasInstance()
                && BattleLogicManager.Instance.delegateUpdateBattleDamage != null)
            {
                BattleLogicManager.Instance.delegateUpdateBattleDamage(damagerAction);
            }
        }

        public void UpdateHeroInfoView()
        {
            //更新人物信息
            if (BattleLogicManager.HasInstance()
                && BattleLogicManager.Instance.delegateUpdateHero != null)
            {
                BattleLogicManager.Instance.delegateUpdateHero(battleItemInfo._id);
            }
        }

        public void WaitSPSkillEvent()
        {
            isWaitSPSkillState = true;
        }

        public void ResumeWaitSPSkillEvent()
        {
            isWaitSPSkillState = false;
        }
#endregion

        //关闭AI
        public void StopAI()
        {
            SetLifeState(ACTOR_LIFE_STATE.StopLogic);
        }

        public void OpenAI()
        {
            if (IsDead)
            {
                return;
            }
            SetLifeState(ACTOR_LIFE_STATE.Alive);
        }
    }

    public class SwitchTargetEvent
    {
        public string targetUID { get; set; }
    }

    public class FocusPreTargetEvent
    {
        public string targetUID { get; set; }
    }

    public class BreakDefEvent
    {
        public string uid;
    }

    public class BreakDefQteEvent { }

    public class BreakStateEnd { }
}