using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using BattleEngine.Logic;
using BattleSystem.Core;
using BattleSystem.ProjectCore;
using UnityEngine;
using UnityEngine.Analytics;

public class BattleState_Move: BattleStateBase
{
    public override int GetStateID()
    {
        return (int)eBattleState.Move;
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
        //var coreService = Battle.Instance.battleCore.CoreService;

        //coreService.MoveToNext();
        LoadData();
        Debug.LogWarning("进入移动状态");
    }

    private async void LoadData()
    {
        if (Battle.Instance.Wave == 1)
        {
            if (!GuideManagerV2.Stuff.IsExecutingForceGuide)
            {
                CameraManager.Instance.Move = false;
                await PlotPipelineControlManager.Stuff.StartPipelineAsync(Battle.Instance.CopyId, EPlotEventType.EndPoint1, null);
                CameraManager.Instance.Move = true;
            }
        }
        if (Owner.battle.NextWaveAvailable())
        {
            GameEventCenter.Broadcast(GameEvent.CameraRoll, false);
            GameEventCenter.Broadcast(GameEvent.ClientBattleEnd);
            
            /*if (Battle.Instance.GameMode.ModeType == BattleModeType.Guard)
            {
                var mode = Battle.Instance.GameMode as GuardMode;
                await mode.NextWave();
                BattleStateManager.Instance.ChangeState(eBattleState.Play);
                return;
            }*/
            
            float durT = Owner.battle.MoveToNext();
            if(Battle.Instance.param.mode != BattleModeType.Fixed && Battle.Instance.param.mode != BattleModeType.Defence && Battle.Instance.param.mode != BattleModeType.Guard)
                Owner.battle.SpawnMonsters();
            Owner.battle.SpawnLeader(true);

            await Battle.Instance.GameMode.ShowSomething();

            BattleMono.Instance.StartCoroutine(MoveCoroutine(durT));
        }
    }

    private IEnumerator MoveCoroutine(float durT)
    {
        yield return new WaitForSeconds(durT);
            
        if (Battle.Instance.param.mode != BattleModeType.Fixed && Battle.Instance.param.mode != BattleModeType.Defence && Battle.Instance.param.mode != BattleModeType.Guard)
        {
            foreach (var VARIABLE in BattleManager.Instance.ActorMgr.GetAllActors())
            {
                if (VARIABLE.IsMain)
                {
                    if (Vector3.Distance(VARIABLE.ClientMoveTarget, VARIABLE.SelfTrans.position) > 0.1f)
                    {
                        VARIABLE.ShowAppearance("fx_refresh_blue");
                    }
                    VARIABLE.StopClientMove();
                    VARIABLE.mData.OpenAI();
                }
            }
            yield return new WaitForSeconds(1f);
            CameraManager.Instance.TryChangeState(CameraState.Free2);
        }

        BattleStateManager.Instance.ChangeState(eBattleState.Play);
    }
    


    public override void OnLeave()
    {
    }

    public override void Update(float param_deltaTime)
    {
        /*var coreService = Battle.Instance.battleCore.CoreService;
        foreach (var uid in Battle.Instance.battleCore.CoreService.storage.heroes)
        {
            // 忽略assistant
            // if (CoreAction.Instance.IsAssistant(uid)) continue;
            // 只要有一个没停下， 就不能结束
            if (!coreService.IsStopped(uid)) return;
        }

        if (!BattleManager.Instance.GameOver)
        {
            StageBattleUtil.TiggerStory(Owner.battle.param.stageInfo.StageId, EPlotEventType.StartPoint2, delegate
            {
                BattleManager.Instance.ChangeState(eBattleState.Play);
            });
        }
        else
        {
            // 如果没有下一波了， 那就要settle了
            // CoreAction.Instance.FaceSame();
            BattleManager.Instance.ChangeState(eBattleState.Settlement);
        }*/
    }
}
