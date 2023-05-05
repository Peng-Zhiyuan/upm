using UnityEngine.Playables;

namespace BattleEngine.View
{
    using BattleSystem.ProjectCore;
    using UnityEngine;
    using Logic;

    public sealed class BattleTimeLineActionTaskData : AbilityTaskData
    {
        public string timeLinePath = "";

        public override SKILL_ACTION_ELEMENT_TYPE GetSkillActionElementType()
        {
            return SKILL_ACTION_ELEMENT_TYPE.TimeLine;
        }

        public override SKILL_ACTION_ELEMENT_PLATFORM GetSkillActionElementPlatform()
        {
            return SKILL_ACTION_ELEMENT_PLATFORM.VIEW;
        }

        public override void Init(SkillActionElementItem element)
        {
            base.Init(element);
            FeatureTimeLineElement actionElement = element as FeatureTimeLineElement;
            timeLinePath = actionElement.timeLinePath;
        }
    }

    public sealed class SkillBattleTimeLineActionTask : AbilityViewTask
    {
        private BattleTimeLineActionTaskData _taskData;
        public BattleTimeLineActionTaskData TaskData
        {
            get
            {
                if (_taskData == null)
                {
                    _taskData = taskInitData as BattleTimeLineActionTaskData;
                }
                return _taskData;
            }
        }
        public System.Action delegateTimelineEnd;

        private CombatActorEntity owner;
        private SkillTimelineUnit TimeLine = null;
        private bool isPlayEnable = true;

        public override void BeginExecute(int frameIdx)
        {
            base.BeginExecute(frameIdx);
            owner = SkillAbilityExecution.OwnerEntity;
            isPlayEnable = true;
            if (Battle.Instance.param.mode == BattleModeType.Arena
                || !owner.isAtker
                || (!BattleDataManager.Instance.IsPlayCutScene && Battle.Instance.param.mode != BattleModeType.SkillView))
            {
                isPlayEnable = false;
                return;
            }
            GameEventCenter.Broadcast(GameEvent.SuperSkillStart, SkillAbilityExecution.OwnerEntity.UID);
            BattleSpecialEventManager.Instance.AddEvent(SpecailEventType.HideUI);
            BattleSpecialEventManager.Instance.AddEvent(SpecailEventType.Dark);
            Creature actor = BattleManager.Instance.ActorMgr.GetActor(SkillAbilityExecution.OwnerEntity.UID);
            float bgmVolum = AudioManager.BgmVolume;
            AudioManager.BgmVolume = bgmVolum * 0.4f;
            string path = AddressablePathConst.SkillEditorPathParse(TaskData.timeLinePath);
            PlayAsync(path, actor);
        }

        public async void PlayAsync(string timelinePath, Creature obj)
        {
            TimeLine = await GameObjectPoolUtil.ReuseAddressableObjectAsync<SkillTimelineUnit>(BucketManager.Stuff.Battle, timelinePath);
            if (TimeLine == null)
            {
                isPlayEnable = false;
                return;
            }
            //位置
            TimeLine.GameObject.SetActive(false);
            TimeLine.Transform.parent = null;
            TimeLine.Transform.position = obj.SelfTrans.position;
            TimeLine.Transform.localRotation = obj.SelfTrans.localRotation;
            if (TimeLine != null
                && TimeLine.AvatarObj == null)
            {
                string modelPath = StrBuild.Instance.ToStringAppend(owner.battleItemInfo.GetHeroRow().Model, ".prefab");
                if (owner.isAtker)
                {
                    HeroInfo heroInfo = HeroManager.Instance.GetHeroInfo(owner.ConfigID);
                    if (heroInfo.Unlocked)
                    {
                        modelPath = StrBuild.Instance.ToStringAppend(RoleHelper.GetAvatarName(heroInfo), ".prefab");
                    }
                }
                await TimeLine.PlaySkillTimeLine(modelPath);
            }
            TimeLine.Director.timeUpdateMode = DirectorUpdateMode.GameTime;
            TimeLine.GameObject.SetActive(true);
            obj.isTimeline = true;
            obj.transform.position = new Vector3(20000, 0, 0);
            TimeLine.Director.Play();
            //隐藏镜头
            UIEngine.Stuff.IsPageLayerEnabled = false;
            CameraManager.Instance.MainCamera.gameObject.SetActive(false);
            BattleSpecialEventManager.Instance.ShowDark(TimeLine.GetComponentInChildren<Camera>());
            BattleManager.Instance.SPSSkillMgr.PlaySPSkillPreEffect(owner.UID);
            LogicFrameTimerMgr.Instance.ScheduleTimer((float)TimeLine.Director.duration, () => { TimeLineEnd(); });
        }

        public override void BreakExecute(int frameIdx)
        {
            if (isPlayEnable)
            {
                TimeLineEnd();
            }
            base.BreakExecute(frameIdx);
        }

        public override void EndExecute()
        {
            base.EndExecute();
        }

        private void TimeLineEnd()
        {
            GameEventCenter.Broadcast(GameEvent.SuperSkillEnd, owner.ConfigID);
            if (!isPlayEnable)
            {
                return;
            }
            isPlayEnable = false;
            UIEngine.Stuff.IsPageLayerEnabled = true;
            BattleSpecialEventManager.Instance.CloseDark(TimeLine.GetComponentInChildren<Camera>());
            BattleSpecialEventManager.Instance.RemoveEvent(SpecailEventType.HideUI);
            BattleSpecialEventManager.Instance.RemoveEvent(SpecailEventType.Dark);
            BucketManager.Stuff.Battle.Pool.Recycle(TimeLine);
            CameraManager.Instance.MainCamera.gameObject.SetActive(true);
            Creature ownCreature = BattleManager.Instance.ActorMgr.GetActor(owner.UID);
            ownCreature.isTimeline = false;
            ownCreature.mData.ResumeWaitSPSkillEvent();
            ownCreature.transform.position = ownCreature.mData.GetPosition();
            CameraFollowTarget.Instance.SyncCamera();
            CameraSetting.Ins.SyncPosImmediate();
            
            BattleManager.Instance.SPSSkillMgr.FinishSPSkillPreEffect(owner.UID);
            var aa = StrBuild.Instance.ToStringAppend("skill_", SkillAbilityExecution.SkillAbility.SkillBaseConfig.SkillID.ToString());
            WwiseEventManager.SendEvent(TransformTable.EndSkillTimeline, aa, ownCreature.gameObject);
            if (delegateTimelineEnd != null)
            {
                delegateTimelineEnd();
            }
        }
    }
}