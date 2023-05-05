using System.Linq;
using BattleEngine.Logic;
using BattleSystem.Core;
using BattleSystem.ProjectCore;
using UnityEngine;

public class PlayState_Play : BattleStateBase
{
    public override int GetStateID()
    {
        return (int)ePlayState.Play;
    }

    public override void OnDestroy() { }

    public bool Condition(BaseState curState)
    {
        return active;
    }

    public override void OnStateChangeRequest(int newState, object param_UserData) { }

    public override void OnEnter(object param_UserData)
    {
        Follow();
    }

    private async void Follow()
    {
        await Battle.Instance.OnWaveStart();
        
        GameEventCenter.Broadcast(GameEvent.ClientBattleStart);
        GameEventCenter.Broadcast(GameEvent.ShowHeadIcon);
        
        Debug.LogWarning("进入战斗状态");
        if (Owner.battle.Wave == 1)
        {
            Battle.Instance.GameMode.RegistCondition();
            // ----------------------------- 触发引导事件 -----------------------------
            GuideManagerV2.Stuff.Notify("Battle.Ready");
            EventManager.Instance.SendEvent<float>("BattleBeginExecute", 0.0f);
        }
        BattleManager.Instance.BattleExecute();
        EventManager.Instance.SendEvent<int>("BattleWaveExecute", Owner.battle.Wave);
        //打开怪物AI
        foreach (var VARIABLE in BattleLogicManager.Instance.BattleData.defActorLst)
        {
            if (!VARIABLE.IsCantSelect)
            {
                VARIABLE.OpenAI();
            }
        }
        Battle.Instance.GameMode.OnWaveExectue();
        BattleLogicManager.Instance.IsBattleEnd = false;
        SyncPlayerLocation();
    }

    public void SyncPlayerLocation()
    {
        var VARIABLE = Battle.Instance.PlayerData;
        if (VARIABLE == null)
            return;
        var dir = Quaternion.AngleAxis(StageBattleUtil.GetMapRotationY(VARIABLE), Vector3.up) * Vector3.forward;
        var pos = VARIABLE.Pos;
        var player = BattleManager.Instance.ActorMgr.GetPlayer();
        if (player != null)
        {
            player.SetPosition(pos);
            player.mData.SetPosition(pos);
        }
    }

    public override void OnLeave()
    {
        BattleTimeManager.Instance.PauseBattleTime();
    }

    private bool _flag = false;

    public override void Update(float param_deltaTime) { }
}