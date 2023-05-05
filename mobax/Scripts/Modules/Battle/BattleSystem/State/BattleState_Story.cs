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

public class BattleState_Story : BattleStateBase
{
    public override int GetStateID()
    {
        return (int)eBattleState.Story;
    }

    public override void OnDestroy() { }

    public bool Condition(BaseState curState)
    {
        return false;
    }

    public override void OnStateChangeRequest(int newState, object param_UserData) { }

    public override void OnEnter(object param_UserData)
    {
        /*if (Owner.battle.LevelInfo.OnlyPlot)
        {
            Owner.GameResult = eBattleResult.Victory;
            //纯剧情副本
            StageBattleUtil.TriggerStory(Owner.battle.LevelInfo.StageId, EPlotEventType.NoFight, EndStory);
        }
        else
        {*/
            //PreCreateModels();
            if (GuideManagerV2.Stuff.IsExecutingForceGuide)
            {
                IntoBattle();
            }
            else
            {
                /*StageBattleUtil.TriggerStory(Owner.battle.CopyId, EPlotEventType.StartBattle, IntoBattle);
                if (BattleStateManager.Instance.StoryPlay)
                {
                    Battle.Instance.CloseLoading();
                }*/

                IntoBattle();
            }
        //}
        SetCamera();
    }

    private void SetCamera()
    {
        this.SetTimeLineCameraNotActive();
        //Debug.LogError("这里先注释了");
        //CameraManager.Instance.SetCameraData(Owner.battle.CameraPos, Owner.battle.CameraRotation);
        //CameraManager.Instance.TryChangeState(CameraState.Fixed);
    }

    private void SetTimeLineCameraNotActive()
    {
        var curScene = SceneManager.GetActiveScene();
        var objs = curScene.GetRootGameObjects().ToList();
        var timeLine = objs.Find(val => val.GetComponent<PlayableDirector>() != null);
        if (timeLine == null) return;
        var trans = timeLine.GetComponentsInChildren<Transform>();
        var cameraList = Array.FindAll(trans, val => val.GetComponent<CinemachineVirtualCamera>() != null);
        foreach (var camera in cameraList)
        {
            camera.gameObject.SetActive(false);
        }
    }
    private async void IntoBattle()
    {
        Debug.LogWarning("剧情回调");
        await Owner.battle.GameMode.BattleReady();
        BattleStateManager.Instance.ChangeState(eBattleState.Play);
    }
    public override void OnLeave() { }

    public override void Update(float param_deltaTime) { }
}