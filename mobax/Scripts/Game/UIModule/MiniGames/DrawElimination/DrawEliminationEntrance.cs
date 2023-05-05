
using System;
using System.Collections.Generic;
using UnityEngine;

public partial class DrawEliminationEntrance : Page
{
    public override void OnNavigatedTo(PageNavigateInfo navigateInfo)
    {
        var judgeScore = DrawEliminationProcesser.BestJudge;
        var fastPassTime = DrawEliminationProcesser.FastPassTime;
        var passTimes = DrawEliminationProcesser.PassTimes;
        
        var judgementStr = judgeScore <= 0 ? "-" : DrawEliminationProcesser.GetJudgement(judgeScore).Name;
        Txt_bestJudgement.text = judgementStr;
        var fastPassTimeStr = fastPassTime <= 0 ? "-" : $"{fastPassTime / 1000}s";
        Txt_fastTime.text = fastPassTimeStr;
        var passTimesStr = passTimes <= 0 ? "-" : passTimes.ToString();
        Txt_passTimes.text = passTimesStr;
        Node_firstVictory.SetActive(!DrawEliminationProcesser.TodayPrized());
    }

    private void OnEnable()
    {
        DrawEliminationTalk.Bind(Txt_talk);
        DrawEliminationTalk.Goes(DrawEliminationTalkType.T1_Welcome);
    }
    
    public void OnGameStart()
    {
        UIEngine.Stuff.ForwardOrBackTo<DrawEliminationGame>();
    }
}