namespace BattleEngine.View
{
    using System.Collections.Generic;
    using Logic;

    public sealed class PlayAnimationTaskData : AbilityTaskData
    {
        public string aniClipName;
        public float aniSpeed;
        public Dictionary<int, float> speedModify;
        public float crossFade;
        public bool isExitToIdel = false;

        public override SKILL_ACTION_ELEMENT_TYPE GetSkillActionElementType()
        {
            return SKILL_ACTION_ELEMENT_TYPE.Animation;
        }

        public override SKILL_ACTION_ELEMENT_PLATFORM GetSkillActionElementPlatform()
        {
            return SKILL_ACTION_ELEMENT_PLATFORM.VIEW;
        }

        public override void Init(SkillActionElementItem element)
        {
            base.Init(element);
            PlayAnimActionElement playAnimActionElement = element as PlayAnimActionElement;
            if (playAnimActionElement == null)
            {
                return;
            }
            aniClipName = playAnimActionElement.anim;
            speedModify = new Dictionary<int, float>();
            if (playAnimActionElement.speedModify != null
                && playAnimActionElement.speedModify.Count > 0)
            {
                for (int i = 0; i < playAnimActionElement.speedModify.Count; i++)
                {
                    speedModify.Add(playAnimActionElement.speedModify[i].Frame, playAnimActionElement.speedModify[i].value);
                }
            }
            this.isExitToIdel = playAnimActionElement.isExitToIdel;
        }
    }

    public sealed class SkillPlayAnimationActionViewTask : AbilityViewTask
    {
        private PlayAnimationTaskData _taskData;
        public PlayAnimationTaskData TaskData
        {
            get
            {
                if (_taskData == null)
                {
                    _taskData = taskInitData as PlayAnimationTaskData;
                }
                return _taskData;
            }
        }

        public override void BeginExecute(int frameIdx)
        {
            base.BeginExecute(frameIdx);
            PlayAnim(SkillAbilityExecution.OwnerEntity);
        }

        public override void DoExecute(int frameIdx)
        {
            base.DoExecute(frameIdx);
            UpdateSpeed(frameIdx);
        }

        public override void EndExecute()
        {
            ResetSpeed();
            base.EndExecute();
        }

        private Creature actor;

        public void PlayAnim(CombatActorEntity actorEntity)
        {
            if (actorEntity == null
                || actorEntity.CurrentHealth.Value <= 0)
            {
                return;
            }
            actor = BattleManager.Instance.ActorMgr.GetActor(actorEntity.UID);
            UpdateSpeed(0);
            actor.PlayAnim(TaskData.aniClipName);
        }

        public void UpdateSpeed(int frameIdx)
        {
            if (actor == null
                || TaskData == null)
            {
                return;
            }
            actor.SetAnimSpeed(GetCurSpeed(TaskData.speedModify, frameIdx));
        }

        float GetCurSpeed(Dictionary<int, float> speedModify, int frame)
        {
            float speed = 1;
            if (speedModify == null)
            {
                return speed;
            }
            List<int> modifyFrames = new List<int>(speedModify.Keys);
            modifyFrames.Sort();
            for (int i = 0; i < modifyFrames.Count; i++)
            {
                if (modifyFrames[i] <= frame)
                {
                    speed = speedModify[modifyFrames[i]];
                }
            }
            return speed;
        }

        public void ResetSpeed()
        {
            if (actor != null)
            {
                actor.SetAnimSpeed(1.0f);
            }
        }
    }
}