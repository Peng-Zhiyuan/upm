using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BattleSystem.ProjectCore;
using Cinemachine;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

public class BattleState_Explore : BattleStateBase
{
    public override int GetStateID()
    {
        return (int) eBattleState.Explore;
    }

    public override void OnDestroy()
    {
    }

    public bool Condition(BaseState curState)
    {
        return false;
    }

    public override void OnStateChangeRequest(int newState, object param_UserData)
    {
    }

    public override void OnEnter(object param_UserData)
    {
        PreCreateModels();
    }


    /// <summary>
    /// 预创建模型
    /// </summary>
    public void PreCreateModels()
    {
        //Debug.LogError("这里先注释，chenfei");
        /*var stageinfo = Battle.Instance.LevelInfo;

        int wave = 2;
        StageMonsterInfo info = StageBattleUtil.GetMonsterInfo(stageinfo.MonsterInfos, wave);
        if (info == null)
            info = StageBattleUtil.GetMonsterInfo(stageinfo.MonsterInfos, 1);
        var battleCore = Battle.Instance.GetBattleComponent<ClientEngine>().battleCore;

        if (info.heros != null)
        {
            var heroId = stageinfo.MainHeroId.First();
            if (heroId == 0) return;

            SceneObjectManager.Instance.CreatePreModel(heroId);
        }*/
    }


    public override void OnLeave()
    {
    }

    public override void Update(float param_deltaTime)
    {
        // if (true)
        // {
        //     BattleManager.Instance.ChangeState(eBattleState.Settlement);
        // }
    }
}