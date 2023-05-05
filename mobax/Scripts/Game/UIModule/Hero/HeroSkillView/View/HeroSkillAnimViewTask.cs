/* Created:Loki Date:2022-10-17*/

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using BattleEngine.Logic;
using BattleEngine.View;

public sealed class HeroSkillAnimViewTask : HeroSkillViewTask
{
    public PlayAnimationTaskData TaskData;
    private Animator _animator;
    private List<Animator> _subAnimtors = new List<Animator>();

    public override void InitTask(AbilityTaskData data)
    {
        base.InitTask(data);
        TaskData = AbilityTaskData as PlayAnimationTaskData;
        _animator = null;
        _subAnimtors = null;
    }

    public override void BeginExecute(int frameIdx)
    {
        if (ActorObject == null)
        {
            return;
        }
        _animator = ActorObject.GetComponent<Animator>();
        Animator[] subAnimators = ActorObject.GetComponentsInChildren<Animator>();
        _subAnimtors = subAnimators != null ? subAnimators.ToList() : new List<Animator>();
        UpdateSpeed(0);
        PlayAnim(TaskData.aniClipName);
    }

    public override void DoExecute(int frameIdx)
    {
        UpdateSpeed(frameIdx);
    }

    public override void EndExecute() { }

    private void PlayAnim(string anim)
    {
        if (_animator != null)
        {
            _animator.Play(anim, 0, 0);
        }
        if (_subAnimtors != null)
        {
            for (int i = 0; i < _subAnimtors.Count; i++)
            {
                _subAnimtors[i].Play(anim, 0, 0);
            }
        }
    }

    public void UpdateSpeed(int frameIdx)
    {
        if (_animator == null
            || TaskData == null)
        {
            return;
        }
        SetAnimSpeed(GetCurSpeed(TaskData.speedModify, frameIdx));
    }

    public void SetAnimSpeed(float speed = 1.0f)
    {
        if (_animator != null)
        {
            _animator.speed = speed;
        }
        if (_subAnimtors != null)
        {
            for (int i = 0; i < _subAnimtors.Count; i++)
            {
                if (_subAnimtors[i] != null)
                {
                    _subAnimtors[i].speed = speed;
                }
            }
        }
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
        if (_animator != null)
        {
            SetAnimSpeed(1.0f);
        }
    }

    public void StopSkill() { }
}