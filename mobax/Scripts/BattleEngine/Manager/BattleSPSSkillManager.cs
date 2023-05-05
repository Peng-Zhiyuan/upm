namespace BattleEngine.View
{
    using System.Collections.Generic;
    using UnityEngine;
    using Logic;

    public sealed class BattleSPSSkillManager
    {
        public Queue<string> SPSKillQueue = new Queue<string>(); //大招表现队列
        public string currentSPSkillUID = "";
        public bool IsPlaySPSkill
        {
            get
            {
                if (string.IsNullOrEmpty(currentSPSkillUID)
                    || SPSKillQueue.Count > 0)
                {
                    return false;
                }
                return true;
            }
        }
        public GameObject SPSkillVFX;

        public void PlaySPSkillPreEffect(string uid)
        {
            if (SPSKillQueue.Count > 0)
            {
                SPSKillQueue.Enqueue(uid);
            }
            else
            {
                ExecuteSPSkillEffect(uid);
            }
        }

        public void ExecuteSPSkillEffect(string uid)
        {
            if (!BattleManager.HasInstance()
                || BattleManager.Instance.ActorMgr == null)
            {
                return;
            }
            Creature actor = BattleManager.Instance.ActorMgr.GetActor(uid);
            if (actor == null)
            {
                return;
            }
            if (SPSkillVFX != null)
            {
                SPSkillVFX.SetActive(true);
                TransformUtil.InitTransformInfo(SPSkillVFX, actor.transform);
            }
            currentSPSkillUID = uid;
            List<Creature> lst = BattleManager.Instance.ActorMgr.GetAllActors();
            for (int i = 0; i < lst.Count; i++)
            {
                if (lst[i].mData.IsCantSelect
                    || lst[i].mData.UID == uid)
                    continue;
                lst[i].mData.WaitSPSkillEvent();
                lst[i].ToPauseAnim();
                if (lst[i].GetModelObject != null)
                    lst[i].GetModelObject.transform.localPosition = new Vector3(20000, 0, 0);
            }
            BattleTimeManager.Instance.PauseBattleTime();
            TimerMgr.Instance.PauseType(TimerType.Battle);
            BattleResManager.Instance.PauseALLUsingEffect();
            EventManager.Instance.SendEvent<string>("SuperSkillEnd", uid);
        }

        public void FinishSPSkillPreEffect(string uid, bool isBreak = false)
        {
            if (SPSKillQueue.Count > 0
                && !isBreak)
            {
                ExecuteSPSkillEffect(SPSKillQueue.Dequeue());
            }
            else
            {
                List<Creature> lst = BattleManager.Instance.ActorMgr.GetAllActors();
                for (int i = 0; i < lst.Count; i++)
                {
                    if (lst[i].mData.IsCantSelect
                        || lst[i].mData.UID == uid)
                        continue;
                    lst[i].mData.ResumeWaitSPSkillEvent();
                    lst[i].ToResumeAnim();
                    if (lst[i].GetModelObject != null)
                        lst[i].GetModelObject.transform.localPosition = Vector3.zero;
                }
                BattleTimeManager.Instance.BeginBattleTime();
                TimerMgr.Instance.ResumeType(TimerType.Battle);
                BattleResManager.Instance.ResumeALLUsingEffect();
                currentSPSkillUID = "";
                if (SPSkillVFX != null)
                {
                    SPSkillVFX.SetActive(false);
                }
            }
        }
    }
}