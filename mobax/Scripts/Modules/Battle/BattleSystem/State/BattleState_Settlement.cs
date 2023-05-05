using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using BattleEngine.Logic;
using BattleSystem.Core;
using BattleSystem.ProjectCore;
using DG.Tweening;
using UnityEngine;

public class BattleState_Settlement : BattleStateBase
{
    public override int GetStateID()
    {
        return (int)eBattleState.Settlement;
    }
    
    public override void OnDestroy()
    {

    }

    public override bool Condition(BaseState curState)
    {
        return false;
    }

    public override void OnStateChangeRequest(int newState, object param_UserData)
    {

    }
    
    public async override void OnEnter(object param_UserData)
    {
#if !Server
        //await Focus();
        DOTween.KillAll();
        BattleDataManager.Instance.TimeScale = 1f;

        //await Task.Delay(1000);
        if (BattleLogicManager.Instance.IsOpenBattleViewLayer)
        {
            CameraManager.Instance.TryChangeState(CameraState.Settlement);   
        }
        
        GameEventCenter.Broadcast(GameEvent.ShowUI, true);
        UIEngine.Stuff.IsPageLayerEnabled = true;
        
        if(BattleManager.Instance.ActorMgr.GetDefLst(false).Count == 0)
            await PlotPipelineControlManager.Stuff.StartPipelineAsync(Battle.Instance.CopyId, EPlotEventType.KillMonsterAll, null);
#endif

        /*if (Owner.battle.LevelInfo.OnlyPlot)
        {
            Owner.GameResult = eBattleResult.Victory;
            BattleLogic.Instance.Victory();
            await Battle.Instance.GameMode.BattleResult(true, new List<BattleRecord>(), new List<BattleRecord>(), 0);
        }
        else
        {*/

        UIEngine.Stuff.BackTo<BattlePage>();
        
        await Battle.Instance.GameMode.BattleEnd(BattleResultManager.Instance.IsBattleWin);
        
            AudioManager.PauseBgm();
            if (BattleResultManager.Instance.IsBattleWin)
            {
                Owner.GameResult = eBattleResult.Victory;
                //AudioManager.PlaySeInBackground("Victory.wav");
            }
            else if (!BattleResultManager.Instance.IsBattleWin)
            {
                Owner.GameResult = eBattleResult.Fail;
                //AudioManager.PlaySeInBackground("Fail.wav");
            }
            else
            {
                Debug.LogError("战斗结果错误");
                return;
            }

            var page = UIEngine.Stuff.FindPage("BattlePage");
            (page as BattlePage).EndGame();
            
            GameEventCenter.Broadcast(GameEvent.ClientBattleEnd);

            var myTeamBattleRecord = BattleEngine.Logic.BattleLogicManager.Instance.BattleData.GetAtkRecordLst();
            var enemyTeamBattleRecord = BattleEngine.Logic.BattleLogicManager.Instance.BattleData.GetDefRecordLst();
            // TODO: 老铁取一下是否胜利  老铁已取好
            var isWin = BattleResultManager.Instance.IsBattleWin;
            float duration = BattleEngine.Logic.BattleTimeManager.Instance.CurrentBattleTime;
            
            //发送副本战斗数据
            if (Battle.Instance.IsZoneBattle)
                Battle.Instance.SendBattleResult();
            else
            {
                await Battle.Instance.GameMode.BattleResult(isWin, myTeamBattleRecord, enemyTeamBattleRecord, duration);
            }
            

            // todo: ???? 这个延迟2秒是啥意思 --xinwusai
            // await Task.Delay(2000);
            //AudioManager.PlayBgmInBackground("Bgm_System_Inside.wav");
            AudioManager.ResumeBgm();
        //}
    }
    
    private async Task Focus()
    {
        // 站位距离
        const int distance = 1;
        
        var index = 0;

        Creature selected_role = SceneObjectManager.Instance.GetSelectPlayer();
        if (selected_role == null)
        {
            return;
        }
        //Battle.Instance.battleCore.CoreService.storage.AllUnits.TryGetValue(selected_role.ID, out var refRole);

        float durT = 0;
        bool left = true;
        foreach (var VARIABLE in SceneObjectManager.Instance.GetAllPlayer())
        {
            
            if (VARIABLE.IsMain && VARIABLE.Selected == false)
            {
                //Battle.Instance.battleCore.CoreService.storage.AllUnits.TryGetValue(VARIABLE.ID, out var role);
                if(!VARIABLE.IsMain)
                    continue;
                
                int times = 1;
                if (left)
                {
                    left = false;
                    times = -1;
                }

                var refY = -1;
                Vector3 offsetPos = Vector3.Cross(selected_role.mData.GetForward(), new Vector3(0, refY, 0)).normalized *
                                    distance * times;

                var temp_target = selected_role.GetPosition() + offsetPos;

                float duration = VARIABLE.MoveTo(temp_target);

                durT = Mathf.Max(durT, duration);
            }
        }

        await Task.Delay((int) (1000 * durT));

        foreach (var VARIABLE in SceneObjectManager.Instance.GetAllPlayer())
        {

            if (VARIABLE.IsMain && VARIABLE.Selected == false)
            {
                VARIABLE.SetDirection(selected_role.GetDirection());
            }
            VARIABLE.mData.StopAI();
            //VARIABLE.HideWeapon();
            VARIABLE.PlayAnim("win");
        }
    }

    public override void OnLeave()
    {
    }

    public override void Update(float param_deltaTime)
    {

    }
}
