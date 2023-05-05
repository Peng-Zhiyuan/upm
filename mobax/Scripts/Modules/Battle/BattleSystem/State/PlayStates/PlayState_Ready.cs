using System.Threading.Tasks;
using BattleEngine.Logic;
using BattleEngine.View;
using BattleSystem.Core;
using BattleSystem.ProjectCore;
using Neatly.Timer;
using UnityEngine;

public class PlayState_Ready : BattleStateBase
{
    public override int GetStateID()
    {
        return (int)ePlayState.Ready;
    }

    public override void OnDestroy() { }

    public bool Condition(BaseState curState)
    {
        return active;
    }

    public override void OnStateChangeRequest(int newState, object param_UserData) { }

    public override void OnEnter(object param_UserData)
    {
        Debug.LogWarning("进入战斗装备状态");
        BattleFlow();
    }

    public async void BattleFlow()
    {
        /*
        if (Owner.battle.Wave == 1)
        {
            CameraManager.Instance.ShowTransitionBlack();
        }
            
        
        await Task.Delay(1000);*/
        //Battle.Instance.CloseLoading();
        await Owner.battle.GameMode.SetReadyCamera();
        if (!Battle.Instance.IsFight)
            return;
        //1.播放开始动画
        if (Owner.battle.Wave == 1)
        {
            if (Battle.Instance.param.mode == BattleModeType.Dreamscape)
            {
                if (!Battle.Instance.IsFight)
                    return;
            }

            //GameEventCenter.Broadcast(GameEvent.ShowStartAnimtion);
            /*if (PlotPipelineManager.Stuff.CheckPlot(Battle.Instance.CurStage, EPlotEventType.StartBattle))
            {
                await Task.Delay(1000);
                BattleEngine.Logic.BattleManager.Instance.BtnPause(true);
                PlotPipelineManager.Stuff.StartPipelineAsync(Battle.Instance.CurStage, EPlotEventType.StartBattle); 
                await PlotPipelineManager.Stuff.WaitCompleteAsync();
                BattleEngine.Logic.BattleManager.Instance.BtnPause(false);
            }*/
            if (!Battle.Instance.IsArenaMode)
                TrackManager.BattleStart(Battle.Instance.CopyId.ToString());
            else
            {
                ArenaMode mode = Battle.Instance.GameMode as ArenaMode;
                var opponentId = mode.param.targetUid;
                var row = ArenaUtil.GetArenaRowByScore(ArenaManager.Stuff.info.score);
                TrackManager.PvpStart(mode.param.targetUid, row.Id.ToString());
            }

            //await Task.Delay(1500);
        }
        /*if (Owner.battle.Wave == 2 && PlotPipelineManager.Stuff.CheckPlot(Battle.Instance.CurStage, EPlotEventType.StartPoint2))
        {
            BattleEngine.Logic.BattleManager.Instance.BtnPause(true);
            PlotPipelineManager.Stuff.StartPipelineAsync(Battle.Instance.CurStage, EPlotEventType.StartPoint2); 
            await PlotPipelineManager.Stuff.WaitCompleteAsync();
            BattleEngine.Logic.BattleManager.Instance.BtnPause(false);
        }*/
        if (StageBattleUtil.IsBossWave())
        {
            //if (Battle.Instance.Wave == 1)
            //await Task.Delay(2000);
            // AudioManager.PlayBgmInBackground("Bgm_Fight_Boss.wav");
            BattleTimeManager.Instance.PauseBattleTime();
            TimerMgr.Instance.PauseType(TimerType.Battle);
            LogicFrameTimerMgr.Instance.PauseAll();
            BattleResManager.Instance.PauseALLUsingEffect();
            NeatlyTimer.instance.timeStop = true;
            var stageinfo = StaticData.StageTable.TryGet(Battle.Instance.CopyId);
            if (!string.IsNullOrEmpty(stageinfo.Performance))
            {
                //GameEventCenter.Broadcast(GameEvent.ShowUI, false);
                CameraManager.Instance.HideCamera();
                CameraManager.Instance.CVCamera.SetActive(false);
                BattleSpecialEventManager.Instance.AddEvent(SpecailEventType.HideUI);
                var go = await GameObjectPoolUtil.ReuseAddressableObjectAsync<TimelineUnit>(BucketManager.Stuff.Battle, stageinfo.Performance + ".prefab");
                if (go != null)
                {
                    foreach (var VARIABLE in BattleEngine.Logic.BattleManager.Instance.ActorMgr.GetAllActors())
                    {
                        VARIABLE.SetActive(false);
                    }
                    var list = BattleEngine.Logic.BattleManager.Instance.ActorMgr.GetCamp(1);
                    foreach (var VARIABLE in list)
                    {
                        if (!VARIABLE.mData.IsDead)
                        {
                            go.transform.parent = null;
                            go.transform.position = VARIABLE.transform.position;
                            go.transform.rotation = VARIABLE.transform.rotation;
                            go.Director.Play();
                            break;
                        }
                    }
                    await Task.Delay((int)(go.Director.duration * 1000));
                    if (go != null)
                    {
                        GameObject.Destroy(go.gameObject);
                        //GameEventCenter.Broadcast(GameEvent.ShowUI, true);
                        BattleSpecialEventManager.Instance.RemoveEvent(SpecailEventType.HideUI);
                        foreach (var VARIABLE in BattleEngine.Logic.BattleManager.Instance.ActorMgr.GetAllActors())
                        {
                            if (!VARIABLE.mData.IsDead)
                                VARIABLE.SetActive(true);
                        }
                    }
                    CameraManager.Instance.ShowCamera();
                    CameraManager.Instance.CVCamera.SetActive(true);
                }
            }
            //BattleTimeManager.Instance.BeginBattleTime();
            TimerMgr.Instance.ResumeType(TimerType.Battle);
            LogicFrameTimerMgr.Instance.ResumeAll();
            BattleResManager.Instance.ResumeALLUsingEffect();
            NeatlyTimer.instance.timeStop = false;
        }
        active = false;
        m_StateMachine.ChangeState((int)ePlayState.Play, null);
        /*BattleEngine.Logic.BattleLogicManager.Instance.ResetActorLogic(BattleEngine.Logic.BattleManager.Instance.mBattleData);
        BattleEngine.Logic.BattleManager.Instance.BattleExecute();
        BattleEngine.Logic.BattleManager.Instance.ActorMgr.OpenAI();
        BattleStateManager.Instance.ChangeState(eBattleState.Play);*/
    }

    public override void OnLeave() { }

    public override void Update(float param_deltaTime) { }
}