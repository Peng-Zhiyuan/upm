/* Created:Loki Date:2022-10-17*/

using System.Collections.Generic;
using BattleEngine.Logic;
using UnityEngine;
using BattleEngine.View;

public sealed class HeroSkillViewData
{
    public SkillRow _skillRow;
    public SkillConfigObject _SkillConfigObject;
    public int TotalFrame = 0;
    private float _skillFrameRate = 0.0f;
    public float SkillFrameRate
    {
        get
        {
            if (_SkillConfigObject == null)
            {
                return 0.033f;
            }
            if (_skillFrameRate == 0.0f)
            {
                _skillFrameRate = Mathf.FloorToInt(1000f / _SkillConfigObject.fps) * 0.001f;
            }
            return _skillFrameRate;
        }
    }
    public int SkillFrameFPS
    {
        get
        {
            if (_SkillConfigObject == null)
            {
                return 30;
            }
            return _SkillConfigObject.fps;
        }
    }
    public List<HeroSkillViewTask> ActionTaskDataLst = new List<HeroSkillViewTask>();

    public void InitData(GameObject actorObject, SkillRow row, SkillConfigObject configOject)
    {
        _skillRow = row;
        _SkillConfigObject = configOject;
        ActionTaskDataLst.Clear();
        List<SkillActionElementItem> saeis = _SkillConfigObject.actionElements;
        for (int i = 0; i < saeis.Count; i++)
        {
            if (saeis[i].type == SKILL_ACTION_ELEMENT_TYPE.Animation)
            {
                var taskData = new PlayAnimationTaskData();
                taskData.Init(saeis[i]);
                HeroSkillAnimViewTask task = new HeroSkillAnimViewTask();
                task.InitTask(taskData);
                task.ActorObject = actorObject;
                ActionTaskDataLst.Add(task);
                TotalFrame = Mathf.Max(TotalFrame, saeis[i].endFrame);
            }
            else if (saeis[i].type == SKILL_ACTION_ELEMENT_TYPE.Effect)
            {
                CreateEffectActionElement element = (saeis[i] as CreateEffectActionElement);
                if (element == null
                    || string.IsNullOrEmpty(element.res))
                {
                    continue;
                }
                var taskData = new SkillCreateEffectTaskData();
                taskData.Init(saeis[i]);
                HeroSkillCreateEffectViewTask task = new HeroSkillCreateEffectViewTask();
                task.InitTask(taskData);
                task.ActorObject = actorObject;
                ActionTaskDataLst.Add(task);
                TotalFrame = Mathf.Max(TotalFrame, saeis[i].endFrame);
            }
            else if (saeis[i].type == SKILL_ACTION_ELEMENT_TYPE.BattleEvent)
            {
                var taskData = new BattleEventActionTaskData();
                taskData.Init(saeis[i]);
                HeroSkillBattleEventViewTask task = new HeroSkillBattleEventViewTask();
                task.InitTask(taskData);
                task.ActorObject = actorObject;
                ActionTaskDataLst.Add(task);
                TotalFrame = Mathf.Max(TotalFrame, saeis[i].endFrame);
            }
            else if (saeis[i].type == SKILL_ACTION_ELEMENT_TYPE.SkinMeshFresnelEffect)
            {
                var taskData = new SkillFresnelActionTaskkData();
                taskData.Init(saeis[i]);
                HeroSkillFresnelViewTask task = new HeroSkillFresnelViewTask();
                task.InitTask(taskData);
                task.ActorObject = actorObject;
                ActionTaskDataLst.Add(task);
                TotalFrame = Mathf.Max(TotalFrame, saeis[i].endFrame);
            }
        }
    }
}