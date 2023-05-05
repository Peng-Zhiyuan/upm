using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using BattleEngine.Logic;
using BattleEngine.View;
using DG.Tweening;
using Neatly.Timer;
using PathfindingCore;
using Sequence = DG.Tweening.Sequence;
using Task = System.Threading.Tasks.Task;

public class Creature : UnitEntityBase<CombatActorEntity>
{
    protected CharCtrl_Hit _cc_hit = new CharCtrl_Hit();
    protected CharCtrl_HateLine _cc_hate = new CharCtrl_HateLine();
    protected CharCtrl_Probe _cc_probe = new CharCtrl_Probe();
    protected CharCtrl_Bubble _cc_bubble = new CharCtrl_Bubble();
    public CharCtrl_Turn _cc_Turn = new CharCtrl_Turn();
    public CharCtrl_FriendToBattle _cc_Friend = new CharCtrl_FriendToBattle();
    public CharCtrl_LinkerToBattle _cc_Linker = new CharCtrl_LinkerToBattle();

    public CharCtrl_BreakDef _cc_BreakDef = new CharCtrl_BreakDef();
    //角色动作
    public CharacterActionController _cac = new CharacterActionController();

    // cached components
    private RoleAgent roleAgent = null;
    public SceneObjectEvent objectEvent = new SceneObjectEvent();

    public GameTriggerConfig gameTriggerConfig;

    public override void Init(CombatActorEntity _data)
    {
        base.Init(_data);
        this.InitializeGameObject();
        NeatlyTimer.AddFrame(this.gameObject, FrameUpdate, 1);
        mData.ListenActionPoint(ACTION_POINT_TYPE.PostReceiveStatus, OnReceiveBuff);
        mData.Subscribe<RemoveBuffEvent>(OnRemoveBuff).AsCoroutine();
        mData.Subscribe<TriggerBuffEvent>(ReceiveBuffTriggerEffect).AsCoroutine();
        isNeedPlayDeadAnim = true;
    }

    protected void InitializeGameObject()
    {
        _cc_hate.Init(this);
    }

    public virtual SceneObjectType sceneObjectType
    {
        get { return SceneObjectType.None; }
    }

#region //Spine
    public Transform BoneRoot { get; set; }

    public float Radius { get; set; }

    //设置角色的层级
    public void SetLayer(string layer)
    {
        gameObject.layer = LayerMask.NameToLayer(layer);
    }

    private GameObject ModelObject;

    public GameObject GetModelObject
    {
        get { return ModelObject; }
    }

    public async Task CreateActor()
    {
        await LoadBone();
    }

    private async Task LoadBone()
    {
        SelfTrans.SetParent(SceneObjectManager.Instance.GetRootTransform);
        SelfTrans.SetPosition(mData.GetPosition());
        SelfTrans.SetLocalEulerAngleY(mData.GetEulerAngles().y);
        int configid = mData.ConfigID;
        if (IsDefenceTarget())
            configid = 1501008;
        var heroRow = StaticData.HeroTable.TryGet(configid);
        string modelPath = heroRow.Model;
        (int, int, int, long) avatarInfo = (0, 0, 0, 0);
        if (mData.isAtker
            && mData.PosIndex != BattleConst.PlayerPosIndex)
        {
            HeroInfo heroInfo = HeroManager.Instance.GetHeroInfo(configid);
            if (heroInfo.Unlocked)
            {
                modelPath = RoleHelper.GetAvatarName(heroInfo);
                avatarInfo = heroInfo.AvatarInfo;
            }
        }
        this.gameObject.name = heroRow.Id.ToString();
        var address = $"{modelPath}_low.prefab";
        _CameraLine = new GameObject();
        _CameraLine.transform.SetParent(transform);
        _CameraLine.name = "CameraLineRoot";
        _CameraLine.transform.localEulerAngles = new Vector3(0, 0, 0);
        var obj = await BattleResManager.Instance.LoadAvatarModel(address); //await GameObjectPoolUtil.ReuseAddressableObjectAsync<RecycledGameObject>(address);
        if (!Battle.Instance.IsFight)
            return;
        ModelObject = obj;
        GetModelObject.transform.SetParent(transform);
        GetModelObject.transform.localPosition = Vector3.zero;
        GetModelObject.transform.localScale = Vector3.one * mData.GetSize();
        GetModelObject.transform.localRotation = Quaternion.identity;
        BoneRoot = GetModelObject.transform;
        SubAnimtors = GetModelObject.GetComponentsInChildren<Animator>();
        RoleRender = GetModelObject.gameObject.GetOrAddComponent<RoleRender>();
        RoleRender.Init();
        await RoleRender.SwitchHeroSkin(avatarInfo);
        Animator.enabled = true;
        LoadOthers();
#if UNITY_EDITOR
        this.gameObject.AddComponent<DebugRoleInfo>().SetCreature(this);
        this.gameObject.AddComponent<DebugData>().SetCreature(this);
#endif
        gameObject.layer = LayerMask.NameToLayer("Role");
        if (mData.IsSubstitut()
            || mData.IsFriendAssist())
            gameObject.SetActive(false);
        if (!mData.isAtker)
        {
            await ShowAppearance();
            GameEventCenter.Broadcast(GameEvent.CreateHud_Success, this);
        }
        else
        {
            GameEventCenter.Broadcast(GameEvent.CreateHud_Success, this);
        }
        if (mData.PosIndex == BattleConst.PlayerPosIndex)
        {
            GetModelObject.SetActive(false);
        }
        if (IsDefenceTarget())
            GetModelObject.transform.localScale = new Vector3(0, 1, 0);
        CheckGlobalBuffMonster();

        //发送一下音效事件
        WwiseEventManager.SendEvent(TransformTable.Custom, "HeroMode_P3", this.gameObject);
    }

    private int DebufferID = -1;

    private void CheckGlobalBuffMonster()
    {
        if (mData.isAtker)
            return;
        if (mData.GetStagePassiveBuff() == null)
            return;
        DebufferID = EffectManager.Instance.CreateBodyEffect("fx_buff_quanju", GetBone("root"), 1000000, Vector3.zero, Vector3.one, Vector3.zero);
    }

    //是否是防守目标
    public bool IsDefenceTarget()
    {
        return mData.PosIndex == BattleConst.DefenceTargetIndex;
    }

    private bool m_bLoadBoneEnd = false;

    public virtual void LoadOthers()
    {
        m_bLoadBoneEnd = true;
        _cac.Init(this);
        _cc_hit.Init(this);
        _cc_probe.Init(this);
        _cc_bubble.Init(this);
        _cc_Turn.Init(this);
        _cc_Friend.Init(this);
        _cc_BreakDef.Init(this);
        _cc_Linker.Init(this);
    }
#endregion

    public Vector3 GetDirection()
    {
        if (SelfTrans == null)
        {
            return Vector3.zero;
        }
        return SelfTrans.forward.normalized;
    }

    public void SetDirection(Vector3 param_Direction)
    {
        if (param_Direction != Vector3.zero)
        {
            SelfTrans.forward = param_Direction;
        }
    }

#region //处理选中时的外发光
    public RoleRender RoleRender { get; set; }

    public bool IsShowSelected { get; set; }

    public void ShowSelectEffect(bool vis)
    {
        IsShowSelected = vis;
        if (vis)
        {
            if (RoleRender == null)
            {
                TimerMgr.Instance.BattleSchedulerTimer(0.2f, delegate
                                {
                                    if (RoleRender != null)
                                    {
                                        RoleRender.SwitchFocusArea(true, GetBone("Bip001 Spine"));
                                        RoleRender.ShowHeadTag(true, GetBone("Bip001 Spine"));
                                    }
                                }
                );
            }
            else
            {
                RoleRender.SwitchFocusArea(true, GetBone("Bip001 Spine"));
                RoleRender.ShowHeadTag(true, GetBone("Bip001 Spine"));
            }
        }
        else
        {
            if (RoleRender != null)
            {
                RoleRender.SwitchFocusArea(false, GetBone("Bip001 Spine"));
                RoleRender.ShowHeadTag(false, GetBone("Bip001 Spine"));
            }
        }
    }

    public void HideSelectEffect()
    {
        if (RoleRender != null)
            RoleRender.HideHeadTag();
    }

    public Tween hit1, hit2;

    public async void BeHit()
    {
        if (RoleRender == null)
        {
            return;
        }
        Color color = Color.red;
        RoleRender.SetBodyFresnel(true, color);
        if (hit2 != null)
            hit2.Kill();
        await Task.Delay(Mathf.CeilToInt(200));
        float t2 = 0.3f;
        hit2 = DOTween.To(() => color.a, x => color.a = x, 0, t2);
        hit2.OnUpdate(delegate { RoleRender.SetBodyFresnel(true, color); });
        hit2.OnComplete(() => { RoleRender.SetBodyFresnel(false, Color.yellow); });
    }

    private static Creature LastFocus = null;
    private static Creature LastPreFocus = null;

    public void ShowSwitchTarget(bool vis)
    {
        if (vis)
        {
            if (LastFocus != null)
            {
                LastFocus.ShowSwitchTarget(false);
            }
            if (RoleRender != null)
            {
                RoleRender.SwitchTargetSub(true, GetBone("Bip001 Spine"));
                LastFocus = this;
                TimerMgr.Instance.BattleSchedulerTimerDelay(0.001f, delegate { CameraSetting.Ins.SyncPosImmediate(); });
            }
        }
        else
        {
            if (RoleRender != null)
            {
                RoleRender.SwitchTargetSub(false, GetBone("Bip001 Spine"));
            }
        }
    }

    public void ShowPreFocusTarget(bool vis)
    {
        if (vis)
        {
            if (LastPreFocus != null)
            {
                LastPreFocus.ShowSwitchTarget(false);
            }
            if (RoleRender != null)
            {
                RoleRender.SwitchTargetSub(true, GetBone("Bip001 Spine"), 3f);
                LastPreFocus = this;
            }
        }
        else
        {
            if (RoleRender != null)
            {
                RoleRender.SwitchTargetSub(false, GetBone("Bip001 Spine"));
            }
        }
    }
#endregion

    public void FrameUpdate(float param_deltaTime)
    {
        _cc_hit.Update(param_deltaTime);
        _cac.Update(param_deltaTime);
        _cc_hate.Update(param_deltaTime);
        _cc_probe.Update(param_deltaTime);
        if (m_CurrentState == ACTOR_ACTION_STATE.Born
            || m_CurrentState == ACTOR_ACTION_STATE.Weak
            || mData.IsWaitSPSkillState
            || mData.IsNotOperable)
        {
            return;
        }
        Forward(param_deltaTime);
    }

    public void CreatureLookAt(Vector3 target)
    {
        if (target == SelfTrans.position)
            return;
        target.y = SelfTrans.position.y;
        SelfTrans.LookAt(target);
    }

    public string Name { get; set; }

    public bool IsUpdateFace { get; set; }

    public bool FirstAttack = false;
    //攻击目标
    private Creature m_target = null;
    public Creature Target
    {
        get { return m_target; }
        set
        {
            if (m_target == null
                && value != null)
            {
                FirstAttack = true;
            }
            if (m_target != null
                && m_target.sceneObjectType == SceneObjectType.Target)
            {
                m_target.Uninitialize();
            }
            m_target = value;
        }
    }

    public virtual void Uninitialize()
    {
        DestroyObject();
    }

#region Dead //死亡相关
    public bool _removeTag = false;
    public bool RemoveTag
    {
        get { return _removeTag; }
        set { _removeTag = value; }
    }

    private void PlayDeadState()
    {
        OnUnSelected();
        _removeTag = false;
        System.Action delegateDeadFinish = () =>
        {
            if (this.gameObject.activeSelf)
            {
                StartCoroutine(PlayDeadAnim());
            }
        };
        if (string.IsNullOrEmpty(mData.battleItemInfo.battlePlayerRecord.KillMeUID))
        {
            delegateDeadFinish();
            return;
        }
        CombatActorEntity killMeActorEntity = BattleLogicManager.Instance.BattleData.GetActorEntity(mData.battleItemInfo.battlePlayerRecord.KillMeUID);
        if (killMeActorEntity.CurrentSkillExecution != null
            && killMeActorEntity.CurrentSkillExecution.SkillAbility.SkillBaseConfig.skillType == (int)SKILL_TYPE.SPSKL)
        {
            killMeActorEntity.CurrentSkillExecution.onOver += delegateDeadFinish;
        }
        else
        {
            delegateDeadFinish();
        }
        EffectManager.Instance.RemoveEffect(DebufferID);
    }

    public bool isNeedPlayDeadAnim = true;

    IEnumerator PlayDeadAnim()
    {
        WwiseEventManager.SendEvent(TransformTable.HeroVoice_Dead, mData.ConfigID.ToString());
        if (isNeedPlayDeadAnim)
        {
            PlayAnim("dead");
            float waitTime = GetAnimClipTime("dead");
            yield return new WaitForSeconds(waitTime - 0.1f);
        }
        StartCoroutine(PlayDeadVFX());
    }

    IEnumerator PlayDeadVFX()
    {
        if (RoleRender != null)
            RoleRender.Dead();
        int DeadEffectID = EffectManager.Instance.CreateBodyEffect("fx_dead_boss", GetBone("Bip001 Pelvis"), 10000, Vector3.zero, Vector3.one, Vector3.zero);
        SetAnimSpeed(0.0f);
        yield return new WaitForSeconds(1.5f);
        this.SetActive(false);
        ShowSelectEffect(false);
        EffectManager.Instance.RemoveEffect(DeadEffectID);
        _removeTag = true;
    }
#endregion

    public string ID
    {
        get { return mData.UID; }
    }

    protected virtual void DestroyObject()
    {
        if (_cc_hit != null)
        {
            _cc_hit.Destroy();
            _cc_hit = null;
        }
        if (_cc_hate != null)
        {
            _cc_hate.Destroy();
            _cc_hate = null;
        }
        if (_cc_probe != null)
        {
            _cc_probe.Destroy();
            _cc_probe = null;
        }
        if (_cc_BreakDef != null)
        {
            _cc_BreakDef.Destroy();
            _cc_BreakDef = null;
        }
        Destroy(SelfTrans.gameObject);
        mData.UnListenActionPoint(ACTION_POINT_TYPE.PostReceiveStatus, OnReceiveBuff);
        mData.UnSubscribe<RemoveBuffEvent>(OnRemoveBuff);
        mData.UnSubscribe<TriggerBuffEvent>(ReceiveBuffTriggerEffect);
        DestroyFreshTween();
        NeatlyTimer.Remove(this.gameObject);
        EffectManager.Instance.RemoveEffect(DebufferID);
    }

    public void HideWeapon()
    {
        RoleRender.HideWeapon();
    }

    public bool WeaponState
    {
        get { return RoleRender.WeaponState; }
        set { RoleRender.WeaponState = value; }
    }

    public void ShowWeapon()
    {
        RoleRender?.ShowWeapon();
    }

    private Transform _headBone;
    public Transform HeadBone
    {
        get
        {
            if (_headBone == null)
                _headBone = GetBone("body_head");
            return _headBone;
        }
    }

    public Transform GetBone(string bone)
    {
        if (bone == "foot")
            return transform;
        if (string.IsNullOrEmpty(bone))
            bone = "root";
        Transform trans = null;
        if (RoleRender != null)
        {
            if (!RoleRender.m_Bones.TryGetValue(bone.ToLower(), out trans))
            {
                //Debug.LogError("缺少骨骼：" + bone);
            }
            if (trans == null)
            {
                RoleRender.m_BoneSlots.TryGetValue(bone.ToLower(), out trans);
            }
        }
        if (trans == null)
        {
            //Debug.LogError(ConfigID + "没找到结点: " + bone);
            return transform;
        }
        return trans;
    }

    public Transform GetBone(string bone, int index = 0)
    {
        return RoleRender.GetBoneTrans(bone);
    }

#region 大招
    private bool _hasPerfectSkill = false;
    public bool HasPerfectSkill
    {
        get { return _hasPerfectSkill; }
        set { _hasPerfectSkill = value; }
    }
#endregion

    public string idleAnim = CharacterActionConst.Idle;
    public string IdleAnim
    {
        get { return idleAnim; }
        set { idleAnim = value; }
    }
    private bool m_bInBattle = false;
    public bool InBattle
    {
        get { return m_bInBattle; }
        set
        {
            m_bInBattle = value;
            objectEvent.Broadcast(GameEvent.BattleStateChange);
        }
    }

    public void SetHateLine(Creature target)
    {
        if (_cc_hate == null)
            return;
        _cc_hate.SetHateLine(target);
    }

    public BattleItemInfo RoleItemData
    {
        get { return mData.battleItemInfo; }
    }

    public bool IsSelf()
    {
        return mData.isAtker;
    }

    private float _magic = 0f;
    public float Magic
    {
        get { return mData.CurrentMp.Value; }
    }

    public float LimitedTime { get; set; }

    public int NormalSkillIndex { get; set; }

    public int Slot { get; set; }

#region Animator 状态机控制  新战斗底层
    private Animator[] SubAnimtors;
    private Animator _animator;
    private Animator Animator
    {
        get
        {
            if (_animator == null
                && GetModelObject != null)
            {
                _animator = GetModelObject.GetComponentInChildren<Animator>();
            }
            return _animator;
        }
    }

    protected ACTOR_ACTION_STATE m_CurrentState = ACTOR_ACTION_STATE.None;
    public ACTOR_ACTION_STATE mCurrentState
    {
        get { return m_CurrentState; }
    }

    public void PlayAnim(string anim, bool isImmediatePlay = false)
    {
        if (string.IsNullOrEmpty(anim)
            || Animator == null)
        {
            return;
        }
        ResetAllTriggers();
        if (isImmediatePlay)
        {
            Animator.Play(anim, 0, 0);
        }
        else if (anim.Equals("idle")
                 || anim.Equals("stand"))
        {
            Animator.SetBool("moving", false);
            Animator.SetBool("walk", false);
            Animator.SetBool("run", false);
            if (anim.Equals("stand"))
            {
                Animator.Play(anim, 0, 0);
            }
        }
        else if (anim.Equals("run1"))
        {
            Animator.SetBool("moving", true);
        }
        else if (anim.Equals("run"))
        {
            Animator.SetBool("run", true);
        }
        else if (anim.Equals("walk"))
        {
            Animator.SetBool("walk", true);
        }
        else if (!isImmediatePlay)
        {
            Animator.SetTrigger(anim);
        }
        PlaySubAnim(anim);
    }

    private AnimatorControllerParameter[] animatorAPS = null;

    public void ResetAllTriggers()
    {
        if (animatorAPS == null)
        {
            animatorAPS = Animator.parameters;
        }
        if (animatorAPS != null)
        {
            for (int i = 0; i < animatorAPS.Length; i++)
            {
                if (animatorAPS[i].type == AnimatorControllerParameterType.Trigger)
                {
                    if (Animator.GetBool(animatorAPS[i].name))
                    {
                        Animator.ResetTrigger(animatorAPS[i].name);
                    }
                }
            }
        }
    }

    private void PlaySubAnim(string anim)
    {
        if (string.IsNullOrEmpty(anim)
            || SubAnimtors == null)
        {
            return;
        }
        for (int i = 0; i < SubAnimtors.Length; i++)
        {
            SubAnimtors[i].Play(anim);
        }
    }

    public void SetAnimSpeed(float speed = 1.0f)
    {
        if (Animator == null)
        {
            return;
        }
        Animator.speed = speed;
        for (int i = 0; i < SubAnimtors.Length; i++)
        {
            SubAnimtors[i].speed = speed;
        }
    }

    public float GetAnimClipTime(string clipName)
    {
        return Animator.GetClipLength(clipName);
    }

    public bool IsHaveAnimationClip(string clipName)
    {
        return Animator.HasState(0, Animator.StringToHash(clipName));
    }

    public void ToMoveAnim(float speed = 1.0f, string anim = "run1")
    {
        if (mData.isSpellingSkill()
            && mData.CurrentSkillExecution.SkillAbility.SkillConfigObject.CanbeDragged)
        {
            return;
        }
        PlayAnim(anim);
        if (!mData.IsWaitSPSkillState)
            SetAnimSpeed(speed);
    }

    public void ToIdleAnim(float speed = 1.0f)
    {
        if (IgnoreIdle)
            return;
        if (Battle.Instance.BattleStarted)
        {
            mData.SetActionState(ACTOR_ACTION_STATE.Idle);
            PlayAnim("idle");
        }
        else
        {
            PlayAnim("stand");
        }
        if (!mData.IsWaitSPSkillState)
            SetAnimSpeed(speed);
        string buffAnim = GetRevertAnim();
        if (!buffAnim.Equals("idle"))
        {
            PlayAnim(buffAnim, true);
        }
    }

    public void ToBornAnim(float speed = 1.0f)
    {
        PlayAnim("stand", true);
    }

    public void ToPauseAnim()
    {
        SetAnimSpeed(0.0f);
    }

    public void ToResumeAnim()
    {
        SetAnimSpeed(1.0f);
    }

    void StopMove()
    {
        if (mData.ConfigID == 1701001
            || mData.ConfigID == 1701000) { }
        else
        {
            SelfTrans.position = mData.GetPosition();
        }
        ToIdleAnim();
    }

    public void SetState(ACTOR_ACTION_STATE state)
    {
        if (m_CurrentState == ACTOR_ACTION_STATE.Controlled
            && state == ACTOR_ACTION_STATE.Dead)
        {
            m_CurrentState = state;
            Play();
        }
        if (m_CurrentState == state
            || m_CurrentState == ACTOR_ACTION_STATE.Weak
            || m_CurrentState == ACTOR_ACTION_STATE.Dead
            || m_CurrentState == ACTOR_ACTION_STATE.Controlled)
            return;
        m_CurrentState = state;
        Play();
    }

    public void Play()
    {
        if (m_CurrentState == ACTOR_ACTION_STATE.Idle)
        {
            ToIdleAnim();
        }
        else if (m_CurrentState == ACTOR_ACTION_STATE.Move)
        {
            if (mData.CurrentSkillExecution != null)
            {
                mData.CurrentSkillExecution.BreakActionsImmediate();
            }
            ToMoveAnim();
        }
        else if (m_CurrentState == ACTOR_ACTION_STATE.ATK)
        {
            StopMove();
        }
        else if (m_CurrentState == ACTOR_ACTION_STATE.Hurt)
        {
            PlayAnim("hit");
        }
        else if (m_CurrentState == ACTOR_ACTION_STATE.Dead)
        {
            NeatlyTimer.Remove(this.gameObject);
            if (mData.CurrentSkillExecution != null)
                mData.CurrentSkillExecution.BreakActionsImmediate();
            PlayDeadState();
            GameEventCenter.Broadcast(GameEvent.HudHide, this);
            GameEventCenter.Broadcast(GameEvent.RoleDie, this);
        }
        else if (m_CurrentState == ACTOR_ACTION_STATE.Victory) { }
        else if (m_CurrentState == ACTOR_ACTION_STATE.Controlled) { }
        else if (m_CurrentState == ACTOR_ACTION_STATE.Born)
        {
            ToBornAnim();
        }
        else if (m_CurrentState == ACTOR_ACTION_STATE.Static)
        {
            ToPauseAnim();
        }
    }

    public bool isTimeline = false;
    public void Doing()
    {
        if (ModelObject == null || isTimeline)
            return;

        // 死亡判定
        if (mData.CurrentHealth.Value <= 0)
        {
            SetState(ACTOR_ACTION_STATE.Dead);
            return;
        }
        if (mData.IsWaitSPSkillState
            || mData.IsCantSelect
            || !mData.isOpenAITree)
        {
            return;
        }
        // 状态机更新
        SetState(mData.Action_actorActionState);
        if (m_CurrentState == ACTOR_ACTION_STATE.Move)
        {
            SetMoveTargetPosition(mData.targetPos);
            if (!Animator.GetBool("moving"))
            {
                Debug.LogWarning("Move but current not move");
                ToMoveAnim();
            }
        }
        if (m_CurrentState == ACTOR_ACTION_STATE.Controlled)
        {
            //SetMoveTargetPosition(mData.targetPos);
        }
        if (m_CurrentState == ACTOR_ACTION_STATE.MoveATK)
        {
            SetMoveTargetPosition(mData.targetPos);
        }
        if (mData.KinematControl.mHorizontalChargeVelocity != Vector3.zero)
        {
            SetMoveTargetPosition(mData.targetPos);
        }
    }

    protected Vector3 targetPos;
    private Vector3 selfPos = Vector3.zero;
    public Vector3 SelfPositionXZ
    {
        get
        {
            if (transform != null)
            {
                selfPos.x = SelfTrans.position.x;
                selfPos.z = SelfTrans.position.z;
            }
            return selfPos;
        }
    }
    private Transform selfTrans;
    public Transform SelfTrans
    {
        get
        {
            if (selfTrans == null)
            {
                selfTrans = this.transform;
            }
            return selfTrans;
        }
    }

    private void SetMoveTargetPosition(Vector3 pos)
    {
        targetPos = pos;
        SelfTrans.SetLocalEulerAngleY(mData.GetEulerAngles().y);
        SelfTrans.DOMove(mData.GetPosition(), BattleLogicDefine.LogicSecTime);
    }

    public void Forward(float _deltaTime)
    {
        if (Mathf.CeilToInt(SelfTrans.GetEulerAngle().y) != Mathf.CeilToInt(mData.GetEulerAngles().y))
        {
            SelfTrans.SetLocalEulerAngleY(mData.GetEulerAngles().y);
        }
    }
#endregion

#region BUFF
    private Dictionary<string, GameObject> vertigoParticle = new Dictionary<string, GameObject>();
    private string buffAnim;

    private async void OnReceiveBuff(ActionExecution combatAction)
    {
        var action = combatAction as AssignEffectAction;
        if (action.Buff == null
            || action.Buff.buffRow == null)
        {
            return;
        }
        var buffConfig = action.Buff.buffRow;
        BuffResRow buffResRow = StaticData.BuffResTable.TryGet(buffConfig.BuffRes);
        if (buffResRow == null)
        {
            return;
        }
        if (!string.IsNullOrEmpty(buffResRow.ParticleEffect))
        {
            string path = string.Format("{0}.prefab", buffResRow.ParticleEffect);
            if (!string.IsNullOrEmpty(path))
            {
                if (vertigoParticle.ContainsKey(path))
                {
                    vertigoParticle[path].GetComponent<ParticleSystemPlayCtr>().duration = 5;
                    BattleResManager.Instance.RecycleEffect(path, vertigoParticle[path]);
                    vertigoParticle.Remove(path);
                }
                Transform parent = null;
                if (string.IsNullOrEmpty(buffResRow.attachPoint))
                {
                    parent = transform;
                }
                else
                {
                    parent = GameObjectHelper.FindChild(transform, buffResRow.attachPoint);
                }
                Vector3 offsetVec = Vector3Util.StringToVec3(buffResRow.posOffset);
                GameObject particle = await BattleResManager.Instance.CreatorFx(path, parent, offsetVec);
                if (particle == null)
                {
                    return;
                }
                ParticleSystemPlayCtr fxCtr = particle.GetComponent<ParticleSystemPlayCtr>();
                if (vertigoParticle.ContainsKey(path))
                {
                    fxCtr.duration = 5;
                    fxCtr.Stop();
                    BattleResManager.Instance.RecycleEffect(path, particle);
                }
                else
                {
                    vertigoParticle.Add(path, particle);
                }
                fxCtr.duration = 0;
                fxCtr.Play();
            }
        }
        if (BattleControlUtil.IsForbidAttack(mData)
            && mCurrentState == ACTOR_ACTION_STATE.ATK
            && mData.CurrentHealth.Value > 0)
        {
            mData.BreakCurentSkillImmediate();
            ToIdleAnim();
        }
        if (BattleControlUtil.IsForbidMove(mData)
            && mCurrentState == ACTOR_ACTION_STATE.Move)
        {
            StopMove();
            mData.SetActionState(ACTOR_ACTION_STATE.Controlled);
        }
        buffAnim = buffResRow.Anim;
        if (!string.IsNullOrEmpty(buffAnim)
            && mData.CurrentHealth.Value > 0
            && !mData.HasBuffControlType((int)ACTION_CONTROL_TYPE.control_9))
        {
            PlayAnim(buffResRow.Anim);
        }
    }

    private void OnRemoveBuff(RemoveBuffEvent eventData)
    {
        var buffConfig = eventData.Buff.buffRow;
        BuffResRow buffResRow = StaticData.BuffResTable.TryGet(buffConfig.BuffRes);
        if (buffResRow == null)
        {
            return;
        }
        if (!string.IsNullOrEmpty(buffResRow.ParticleEffect))
        {
            string path = string.Format("{0}.prefab", buffResRow.ParticleEffect);
            if (!string.IsNullOrEmpty(path)
                && vertigoParticle.ContainsKey(path))
            {
                vertigoParticle[path].GetComponent<ParticleSystemPlayCtr>().duration = 5;
                BattleResManager.Instance.RecycleEffect(path, vertigoParticle[path]);
                vertigoParticle.Remove(path);
            }
        }
        if (!string.IsNullOrEmpty(buffResRow.ParticleHideEffect))
        {
            string path = string.Format("{0}.prefab", buffResRow.ParticleHideEffect);
            Transform parent = null;
            if (string.IsNullOrEmpty(buffResRow.attachPoint))
            {
                parent = SelfTrans;
            }
            else
            {
                parent = GameObjectHelper.FindChild(GetModelObject.transform, buffResRow.attachPoint);
            }
            Vector3 posOffset = Vector3Util.StringToVec3(buffResRow.posOffset);
            BattleResManager.Instance.CreatorFx(path, parent, posOffset);
        }
        if (!string.IsNullOrEmpty(buffResRow.Anim)
            && !mData.IsCantSelect)
        {
            string revertAnim = GetRevertAnim();
            if (revertAnim != "idle")
            {
                PlayAnim(revertAnim, true);
            }
        }
    }

    private void RemoveAllBuffEffect()
    {
        var data = vertigoParticle.GetEnumerator();
        while (data.MoveNext())
        {
            if (data.Current.Value == null)
            {
                continue;
            }
            BattleResManager.Instance.RecycleEffect(data.Current.Key, data.Current.Value);
        }
        vertigoParticle.Clear();
    }

    private async void ReceiveBuffTriggerEffect(TriggerBuffEvent row)
    {
        if (row == null
            || row.Buff == null
            || row.Buff.buffRow == null)
        {
            return;
        }
        BuffResRow buffResRow = StaticData.BuffResTable.TryGet(row.Buff.buffRow.BuffRes);
        if (buffResRow == null)
        {
            return;
        }
        if (!string.IsNullOrEmpty(buffResRow.ParticleTriggerEffect))
        {
            string path = string.Format("{0}.prefab", buffResRow.ParticleTriggerEffect);
            Transform parent = null;
            if (string.IsNullOrEmpty(buffResRow.attachPoint))
            {
                parent = SelfTrans;
            }
            else
            {
                parent = GameObjectHelper.FindChild(GetModelObject.transform, buffResRow.attachPoint);
            }
            Vector3 posOffset = Vector3Util.StringToVec3(buffResRow.posOffset);
            BattleResManager.Instance.CreatorFx(path, parent, posOffset);
        }
    }
#endregion

#region 选中
    public bool Selected { get; set; }

    public void OnSelected()
    {
        Selected = true;
        ShowSelectEffect(true);
        objectEvent.Broadcast(GameEvent.SceneObjectSelected, true);
        WwiseEventManager.SendEvent(TransformTable.Custom, "HeroMode_P1", this.gameObject);
    }

    public void OnUnSelected()
    {
        Selected = false;
        ShowSelectEffect(false);
        objectEvent.Broadcast(GameEvent.SceneObjectSelected, false);
        WwiseEventManager.SendEvent(TransformTable.Custom, "HeroMode_P3", this.gameObject);
    }
#endregion

    public int ConfigID
    {
        get { return mData.ConfigID; }
    }

    public bool IsEnemy
    {
        get { return mData.CampID == 1; }
    }

    public bool IsHero
    {
        get { return mData.CampID == 0; }
    }

    public bool IsAssitant
    {
        get { return mData.PosIndex == BattleConst.FriendPosIndex || mData.PosIndex == BattleConst.SSPAssistPosIndexStart; }
    }

    public bool IsPlayer
    {
        get { return mData.PosIndex == BattleConst.PlayerPosIndex; }
    }
    public bool IsMain
    {
        get { return IsHero && mData.PosIndex < BattleConst.MainPosEnd; }
    }
    public bool IsSub
    {
        get { return mData.PosIndex >= BattleConst.SubPosStart && mData.PosIndex <= BattleConst.SubPosEnd; }
    }

    public Transform GetTarget
    {
        get
        {
            if (!string.IsNullOrEmpty(mData.targetKey))
            {
                Creature target = BattleManager.Instance.ActorMgr.GetActor(mData.targetKey);
                if (target == null)
                    return null;
                else
                {
                    return target.GetBone("Bip001 Spine");
                }
            }
            return null;
        }
    }

    public Vector3 ClientMoveTarget { get; set; }

    public void StopClientMove()
    {
        if (!IsMove)
            return;
        IsMove = false;
        DOTween.Kill("domove" + ConfigID);
        if (SelfTrans == null)
            return;
        SelfTrans.position = ClientMoveTarget;
        mData.SetEulerAngles(SelfTrans.GetEulerAngle());
        mData.SetPosition(ClientMoveTarget);
        mData.BornCharacters(mData.GetPosition(), mData.GetEulerAngles(), mData.GetSize(), mData.battleItemInfo.GetHeroRow());
        mData.SetActionState(ACTOR_ACTION_STATE.Idle);
        PlayAnim("idle", true);
        ToIdleAnim();
    }

    public bool IsMove { get; set; }

    public float MoveTo(Vector3 pos, float speed = 0, string anim = "run1")
    {
        float temp = speed == 0 ? mData.AttrData.Att_Move : speed;
        mData.ClearTargetInfo();
        mData.StopAI();
        mData.SetActionState(ACTOR_ACTION_STATE.Move);
        PlayAnim(anim);
        float distance = Vector3.Distance(pos, SelfTrans.position);
        float duration = distance / temp;
        SelfTrans.LookAt(pos);
        ClientMoveTarget = pos;
        CalMovePath(pos, duration);
        IsMove = true;
        return duration;
    }

    private void CalMovePath(Vector3 moveEnd, float duration)
    {
        AstarPathCore pathCore = PathFindingManager.Instance.AstarPathCore;
        PathUtil.CalculatePath(pathCore, SelfTrans.position, moveEnd, (Path path) =>
                        {
                            PathUtil.RunFunnelModifiers(path);
                            Debug.LogWarning("Path " + path.vectorPath);
                            if (path.vectorPath.Count <= 2)
                            {
                                Sequence sq = DOTween.Sequence();
                                sq.SetId("domove" + ConfigID);
                                sq.Append(SelfTrans.DOMove(moveEnd, duration).SetEase(Ease.Linear));
                                sq.onComplete = delegate
                                {
                                    mData.SetEulerAngles(SelfTrans.GetEulerAngle());
                                    mData.SetPosition(moveEnd);
                                    mData.BornCharacters(mData.GetPosition(), mData.GetEulerAngles(), mData.GetSize(), mData.battleItemInfo.GetHeroRow());
                                    ToIdleAnim();
                                };
                            }
                            else
                            {
                                path.vectorPath[path.vectorPath.Count - 1] = moveEnd;
                                Sequence sq = DOTween.Sequence();
                                sq.SetId("domove" + ConfigID);
                                sq.Append(SelfTrans.DOPath(path.vectorPath.ToArray(), duration).SetEase(Ease.Linear).SetLookAt(0));
                                sq.onComplete = delegate
                                {
                                    mData.SetEulerAngles(SelfTrans.GetEulerAngle());
                                    mData.SetPosition(moveEnd);
                                    mData.BornCharacters(mData.GetPosition(), mData.GetEulerAngles(), mData.GetSize(), mData.battleItemInfo.GetHeroRow());
                                    ToIdleAnim();
                                    //isDrawing = false;
                                };
                                // isDrawing = true;
                                // pathDrawLst = path.vectorPath;
                            }
                        }
        );
    }

    // private bool isDrawing = false;
    // private List<Vector3> pathDrawLst = new List<Vector3>();
    //
    // private void Update()
    // {
    //     if (isDrawing)
    //     {
    //         if (mData.PosIndex == 0)
    //         {
    //             Draw.Polyline(pathDrawLst, Color.red);
    //         }
    //         else if (mData.PosIndex == 1)
    //         {
    //             Draw.Polyline(pathDrawLst, Color.yellow);
    //         }
    //         else if (mData.PosIndex == 2)
    //         {
    //             Draw.Polyline(pathDrawLst, Color.green);
    //         }
    //     }
    // }

    private GameObject _CameraLine;
    public Transform CameraPoint
    {
        get
        {
            Transform target = GetTarget;
            if (target == null)
            {
                _CameraLine.transform.position = GetBone("Bip001 Spine").position + SelfTrans.forward * 1f;
            }
            else
            {
                var selfPos = GetBone("Bip001 Spine").position;
                var dir = (target.position - selfPos);
                var len = dir.magnitude;
                var pos = GetBone("Bip001 Spine").position;
                len = Mathf.Clamp(len, 0.2f, 2.2f);
                var point = GetBone("Bip001 Spine").position + dir.normalized * len / 2f;
                //point.y = pos.y *  0.67f;
                _CameraLine.transform.position = point;
            }
            return _CameraLine.transform;
        }
    }

    public Vector3 CamereAngleRot
    {
        get { return _CameraLine.transform.localEulerAngles; }
        set { _CameraLine.transform.localEulerAngles = value; }
    }

    public void Trigger(EmojiEvent evt)
    {
        if (!IsMain)
            return;
        if (_cc_bubble != null)
            _cc_bubble.Trigger(evt);
    }

    public bool IsLeft()
    {
        var dir = transform.position - CameraManager.Instance.MainCamera.transform.position;
        return Vector3.Cross(CameraManager.Instance.MainCamera.transform.forward, dir).y < 0;
    }

    public void ShowVimEffect()
    {
        int EffectID = EffectManager.Instance.CreateBodyEffect("fx_vim_posui", GetBone("body_hit"), 5f, Vector3.zero, Vector3.one, Vector3.zero);
        TimerMgr.Instance.BattleSchedulerTimer(5f, () =>
                        {
                            if (EffectManager.Instance == null)
                            {
                                return;
                            }
                            EffectManager.Instance.RemoveEffect(EffectID);
                        }
        );
    }

    public async Task ShowAppearance(string effect = "fx_refresh_red")
    {
        RoleRender.HideSkinnedMeshRender();
        int EffectID = EffectManager.Instance.CreateBodyEffect(effect, GetBone("root"), 2f, Vector3.zero, Vector3.one, Vector3.zero);
        await Task.Delay(200);
        if (!Battle.Instance.IsFight)
            return;
        RoleRender.ShowSkinnedMeshRender();
    }

    public bool IgnoreIdle { get; set; }

    public string GetRevertAnim()
    {
        string anim = "idle";
        var data = mData.TypeIdBuffs.GetEnumerator();
        while (data.MoveNext())
        {
            if (data.Current.Value.buffRow == null
                || data.Current.Value.buffRow.BuffRes == 0)
            {
                continue;
            }
            BuffResRow row = StaticData.BuffResTable.TryGet(data.Current.Value.buffRow.BuffRes);
            if (row == null
                || string.IsNullOrEmpty(row.Anim))
            {
                continue;
            }
            anim = row.Anim;
        }
        return anim;
    }

    public async void ShowFreshEffect(int DurTime1, int DurTime2, int DurTime3, Color color)
    {
        color.a = 0;
        Tween t = DOTween.To(() => color.a, x => color.a = x, 1f, DurTime1 / 1000f);
        // 给执行 t 变化时，每帧回调一次 UpdateTween 方法
        t.OnUpdate(delegate { RoleRender.SetFresnel(true, color); });
        t.SetId("FreshEffect1" + ConfigID);
        await Task.Delay(DurTime1 + DurTime2);
        if (!Battle.Instance.IsFight)
            return;
        Tween t3 = DOTween.To(() => color.a, x => color.a = x, 0, DurTime3 / 1000f);
        // 给执行 t 变化时，每帧回调一次 UpdateTween 方法
        t3.OnUpdate(delegate { RoleRender.SetFresnel(true, color); });
        t3.SetId("FreshEffect3" + ConfigID);
        await Task.Delay(DurTime3);
        if (!Battle.Instance.IsFight)
            return;
        RoleRender.SetFresnel(false, Color.yellow);
    }

    private void DestroyFreshTween()
    {
        DOTween.Kill("FreshEffect1" + ConfigID);
        DOTween.Kill("FreshEffect3" + ConfigID);
    }
}