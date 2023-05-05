using System.Collections;
using System.Collections.Generic;
using BattleEngine.Logic;
using UnityEngine;

public class GuideService : Service
{
    public override void OnCreate()
    {
        GuideManagerV2.forceGuideOver = OnForceGuideCompelte;

        this.AddListener();
    }

    async void OnForceGuideCompelte()
    {
        GPC.Helper.Jingwei.Script.AgreementSigning.NoviceGuidPrefs.Accomplish();
        await FullResManager.Stuff.OnHardCheckPointAsync();

    }

    void AddListener()
    {
        // 大招釋放結束
        EventManager.Instance.RemoveListener<SkillAbilityExecution>("OnEndSkillPointPoint", this.OnEndSkillPointPoint);
        EventManager.Instance.AddListener<SkillAbilityExecution>("OnEndSkillPointPoint", this.OnEndSkillPointPoint);
    }

    void OnEndSkillPointPoint(SkillAbilityExecution combatAction)
    {
        if (combatAction.SkillAbility.SkillBaseConfig.skillType == (int) SKILL_TYPE.SPSKL)
        {
            this.Notify("OnEndSkillPointPoint");
        }
    }

    void Notify(string msg)
    {
        GuideManagerV2.Stuff.Notify(msg);
    }
}