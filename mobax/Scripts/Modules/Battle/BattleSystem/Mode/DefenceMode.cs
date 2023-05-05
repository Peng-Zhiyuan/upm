using System.Collections.Generic;
using System.Threading.Tasks;
using BattleEngine.Logic;
using BattleSystem.ProjectCore;
using DG.Tweening;
using nobnak.Gist.ObjectExt;
using Unity.Mathematics;
using UnityEngine;

[BattleModeClass(BattleModeType.Defence)]
public class DefenceMode : PveMode
{
    //public DreamEscapeBattleParam _battleParam;

    public override BattleModeType ModeType
    {
        get { return BattleModeType.Defence; }
    }

    public override async Task OnPreCreateBattleGameObjectAsync()
    {
        await base.OnPreCreateBattleGameObjectAsync();
    }

    private string PlayerHeadIcon = "1501009";

    public override void OnUpdate()
    {
        base.OnUpdate();
        if (BattleManager.Instance.ActorMgr == null)
            return;
        var list = BattleManager.Instance.ActorMgr.GetCamp(1);
        foreach (var VARIABLE in list)
        {
            if (!VARIABLE.mData.IsDead)
            {
                if (!TargetPos.ContainsKey(VARIABLE.ID))
                    continue;
                if (!TargetPos[VARIABLE.ID].Warninged
                    && Vector3.Distance(VARIABLE.mData.GetPosition(), TargetPos[VARIABLE.ID].Target) < TargetPos[VARIABLE.ID].WarningDis)
                {
                    TargetPos[VARIABLE.ID].Warninged = true;
                    GameEventCenter.Broadcast(GameEvent.EnemyComing, VARIABLE, 1.5f, 0f);
                    //GameEventCenter.Broadcast(GameEvent.ShowTalk, "有敌人要突破防线了，拦住他！！", PlayerHeadIcon);
                    GameEventCenter.Broadcast(GameEvent.FocusEnemy, VARIABLE);
                }
                if (TargetPos[VARIABLE.ID].Pass == false
                    && Vector3.Distance(VARIABLE.mData.GetPosition(), TargetPos[VARIABLE.ID].Target) < 0.01f)
                {
                    TargetPos[VARIABLE.ID].Pass = true;
                    //VARIABLE.mData.AddHp(-VARIABLE.mData.CurrentHealth.Value);
                    Pass(VARIABLE);
                    TotalPassEnemy++;
                    //Debug.LogError("通过敌人数量" + TotalPassEnemy);
                    GameEventCenter.Broadcast(GameEvent.MonsterPass, TotalPassEnemy);
                    if (TotalPassEnemy >= 3)
                        EventManager.Instance.SendEvent("SetBattleResult", new BattleOutResultData() { bWin = false });
                }
                if (TargetPos[VARIABLE.ID].IsOnlyRunMonster
                    && !VARIABLE.mData.HasBuffControlType((int)ACTION_CONTROL_TYPE.control_11)
                    && TargetPos[VARIABLE.ID].dizzniessDistance < Vector3.Distance(TargetPos[VARIABLE.ID].dizzniessInitPos, VARIABLE.mData.GetPosition()))
                {
                    if (VARIABLE.mData.AttachBuff((int)BUFF_COMMON_CONFIG_ID.DIZZINESS) != null)
                    {
                        BuffRow buffRow = BuffUtil.GetBuffRow((int)BUFF_COMMON_CONFIG_ID.DIZZINESS, 1);
                        if (buffRow != null)
                        {
                            TargetPos[VARIABLE.ID].dizzniessInitPos = VARIABLE.mData.GetPosition();
                            TimerMgr.Instance.BattleSchedulerTimer(buffRow.Time * 0.001f + 1.0f, () => { BattleManager.Instance.SendActorMoveToPos(VARIABLE.ID, TargetPos[VARIABLE.ID].Target); });
                            GuideManagerV2.Stuff.Notify("DefenceTired");
                        }
                    }
                }
            }
        }
    }

    private async Task Pass(Creature role)
    {
        role.ShowAppearance();
        await Task.Delay(500);
        if (!Battle.Instance.IsFight)
            return;
        role.gameObject.SetActive(false);
        role.mData.AddHp(-role.mData.CurrentHealth.Value);
    }

    private class MonsterParam
    {
        public bool IsOnlyRunMonster = false;
        public Vector3 dizzniessInitPos = Vector3.zero;
        public float dizzniessDistance = 8;
        public Vector3 Target;
        public float WarningDis;
        public bool IsRuner;
        public bool Warninged;
        public bool Pass = false;
    }

    //private List<MonsterParam> Targets = new List<MonsterParam>();
    private Dictionary<string, MonsterParam> TargetPos = new Dictionary<string, MonsterParam>();
    private float WarningDis = 4f;
    private int TotalPassEnemy = 0;

    public override async Task OnWaveExectue()
    {
        var list = BattleManager.Instance.ActorMgr.GetDefLst();
        TargetPos.Clear();
        Creature VARIABLE = null;
        AgentActor actorAI;
        for (int i = 0; i < list.Count; i++)
        {
            VARIABLE = list[i];
            actorAI = VARIABLE.mData.GetAI();
            Vector3 pos = VARIABLE.GetPosition() + VARIABLE.GetDirection().normalized * 20;
            MonsterParam param = new MonsterParam() { Target = pos, IsRuner = true, WarningDis = UnityEngine.Random.Range(4f, 6f) };
            if (actorAI != null
                && (actorAI.CurrentTreeTask.GetName() == "TankBehavior_RunEnd" || actorAI.CurrentTreeTask.GetName() == "TankBehavior_RunEnd_2"))
            {
                param.IsOnlyRunMonster = true;
                param.dizzniessInitPos = VARIABLE.GetPosition();
                BattleManager.Instance.SendActorMoveToPos(VARIABLE.ID, pos);
            }
            TargetPos.Add(VARIABLE.ID, param);
        }
    }

    public override async Task OnWaveEnd()
    {
        //镜头复位
        //CameraManager.Instance.CameraProxy.CleanOffset();
        //CameraManager.Instance.CameraProxy.TurnImmediate();
        //CameraManager.Instance.TryChangeState(CameraState.Front, );
    }

    public void ResetPos()
    {
        int index = 0;
        foreach (var VARIABLE in BattleDataManager.Instance.GetList(MapUnitType.MainHero))
        {
            var dir = Quaternion.AngleAxis(StageBattleUtil.GetMapRotationY(VARIABLE), Vector3.up) * Vector3.forward;
            var pos = VARIABLE.Pos;
            var creature = BattleEngine.Logic.BattleManager.Instance.ActorMgr.GetActorByIndex(index);
            if (creature != null)
            {
                creature.mData.StopAI();
                //ToDO:稍后移到底层
                creature.mData.ClearTargetInfo();
                creature.mData.SetPosition(pos);
                creature.mData.SetForward(dir);
                creature.SetPosition(pos);
                creature.SetDirection(dir);
                creature.ToIdleAnim();
                //Debug.LogError(VARIABLE.Index + "位置 " + pos);
            }
            index++;
            /*
            if (VARIABLE.Index == 1)
            {
                SceneObjectManager.Instance.LocalPlayerCamera.SetPosition(VARIABLE.Pos);
                SceneObjectManager.Instance.LocalPlayerCamera.SetDirection(dir);
                
                //CameraManager.Instance.TryChangeState(CameraState.Front, creature);
            }*/
        }
    }

    public override async Task BattleReady()
    {
        //await this.battle.SpawnHeros();
    }

    public async Task FocusMonster()
    {
        /*if(Battle.Instance.Wave == 1)
            return;
        */
        Creature target = null;
        List<Creature> roles = new List<Creature>();
        foreach (var VARIABLE in BattleEngine.Logic.BattleManager.Instance.ActorMgr.GetCamp(1))
        {
            if (!VARIABLE.mData.IsDead
                && VARIABLE.mData.Sort == 6)
            {
                Monster = VARIABLE;
                break;
            }
        }
        if (Monster == null)
            return;
        /*GameEventCenter.Broadcast(GameEvent.DefenceWaveTips);
        CameraManager.Instance.TryChangeState(CameraState.Front, Monster);*/
        if (Battle.Instance.Wave == 1)
        {
            var des = LocalizationManager.Stuff.GetText("M10_defense_chat_001");
            GameEventCenter.Broadcast(GameEvent.ShowTalk, des, PlayerHeadIcon);

            //GameEventCenter.Broadcast(GameEvent.ShowTalk, "小心这只特殊的怪物", PlayerHeadIcon);
        }
        GameEventCenter.Broadcast(GameEvent.EnemyComing, Monster, 1.5f, 180f);
        await Task.Delay(1000);
        if (!Battle.Instance.IsFight)
            return;
        if (Battle.Instance.Wave == 1)
        {
            var des = LocalizationManager.Stuff.GetText("M10_defense_chat_002");
            GameEventCenter.Broadcast(GameEvent.ShowTalk, des, PlayerHeadIcon);

            //GameEventCenter.Broadcast(GameEvent.ShowTalk, "它会直接冲向警戒线", PlayerHeadIcon);
        }
        await Task.Delay(500);
    }

    //临时记录
    private Creature Monster = null;

    public override async Task SetReadyCamera()
    {
        if (Battle.Instance.Wave != 1)
        {
            /*CameraManager.Instance.CameraProxy.m_Distance = 1f;
            await Task.Delay(1200);*/
            await SpawnMonster();
            if (!Battle.Instance.IsFight)
                return;
            //await Task.Delay(300);
            ResetPos();
        }
        else
        {
            ResetPos();
        }
        BattleManager.Instance.StopAllAI();

        //await FocusMonster();
        Creature target = null;
        target = GetCenterRole();
        var dir = target.GetDirection();
        var center = GetCenterPosition();
        SceneObjectManager.Instance.LocalPlayerCamera.SetDirection(dir);
        if (Battle.Instance.Wave != 1)
        {
            center = center + SceneObjectManager.Instance.LocalPlayerCamera.GetDirection().normalized * 10f;
            CameraManager.Instance.ShowTransitionBlack();
        }
        else { }
        CameraManager.Instance.TryChangeState(CameraState.Ready, center);
        Vector3 region = SceneObjectManager.Instance.LocalPlayerCamera.transform.position;
        if (Battle.Instance.Wave == 1)
        {
            LinePos = center - target.GetDirection() * 10f;
            EffectManager.Instance.CreateSenceEffect("fx_guard_line", center - target.GetDirection() * 10f, 30000f, Vector3.zero, Vector3.one, target.transform.localEulerAngles);
            CameraSetting.Ins.SetOffsetY(3.35f);
            await Task.Delay(1000);
            if (!Battle.Instance.IsFight)
                return;
            SceneObjectManager.Instance.LocalPlayerCamera.transform.DOMove(center - SceneObjectManager.Instance.LocalPlayerCamera.GetDirection().normalized * 4f, 2f);
            await Task.Delay(2600);
            if (!Battle.Instance.IsFight)
                return;
            await SpawnMonster();
            await Task.Delay(1700);
            if (!Battle.Instance.IsFight)
                return;
            GuideManagerV2.Stuff.Notify("Battle.DefenceStart");
            //await PlotPipelineControlManager.Stuff.StartPipelineAsync(Battle.Instance.CopyId, EPlotEventType.StartPoint1, null);

            //SceneObjectManager.Instance.LocalPlayerCamera.transform.DOMove(region, 1.2f);
            //SceneObjectManager.Instance.LocalPlayerCamera.SetPosition(region);
            //await Task.Delay(1000);
            /*TimerMgr.Instance.BattleSchedulerTimerDelay(10, delegate
            {
                var list = BattleManager.Instance.ActorMgr.GetCamp(1);
                foreach (var VARIABLE in list)
                {
                    if (!VARIABLE.mData.IsDead)
                    {
                        GameEventCenter.Broadcast(GameEvent.FocusEnemy, VARIABLE);
                        break;
                    }
                }
            });*/
        }
        else
        {
            CameraSetting.Ins.SetOffsetY(3.35f);
            await Task.Delay(1200);
            if (!Battle.Instance.IsFight)
                return;
            center = GetCenterPosition() + SceneObjectManager.Instance.LocalPlayerCamera.GetDirection().normalized * 2.5f;
            //center = GetCenterPosition();
            //SceneObjectManager.Instance.LocalPlayerCamera.SetPosition(center);
            SceneObjectManager.Instance.LocalPlayerCamera.transform.DOMove(center, 1.2f);
            await Task.Delay(1200);
            if (!Battle.Instance.IsFight)
                return;
        }
        await FocusMonster();
        if (!Battle.Instance.IsFight)
            return;
        if (Battle.Instance.Wave == 1)
        {
            await ShowWeapon();
            if (!Battle.Instance.IsFight)
                return;
            
            SceneObjectManager.Instance.SetSelectPlayer(target);
            SceneObjectManager.Instance.LocalPlayerCamera.SetPosition(region);
            //CameraSetting.Ins.SetDistance(-5.5f);
            await Task.Delay(1000);
            if (!Battle.Instance.IsFight)
                return;
        }
        else
        {
            GameEventCenter.Broadcast(GameEvent.DefenceWaveTips);
        }
        if (Battle.Instance.Wave == 1)
        {
            CameraManager.Instance.CameraProxy.SetTarget(target.transform, target, true);
            //SceneObjectManager.Instance.SetSelectPlayer(target);
            //BattleSubmitUtil.SubmitByNormalParam(CmdType.SelectRole, target.ID);
        }
        CameraManager.Instance.CameraProxy.RecoveHAngleImmediate();
        CameraManager.Instance.TryChangeState(CameraState.Free2);
        //CameraManager.Instance.CameraProxy.TurnImmediate();
        CameraManager.Instance.CameraProxy.OffsetSpeed = 3;
        if (Battle.Instance.Wave > 1)
        {
            await Task.Delay(2500);
            if (!Battle.Instance.IsFight)
                return;
        }
        BattleManager.Instance.StartAllAI();
        //BattleEngine.Logic.BattleLogicManager.Instance.ResetActorLogic(BattleEngine.Logic.BattleManager.Instance.mBattleData);
        BattleEngine.Logic.BattleManager.Instance.ActorMgr.OpenAI();
        BattleStateManager.Instance.ChangeState(eBattleState.Play);
    }

    public override void SendZoneResult()
    {
        var remTime = BattleTimeManager.Instance.BattleTime - BattleTimeManager.Instance.CurrentBattleTime;
        SeaBattleManager.SetBattleResult(new SeaBattleBattleResult() { win = BattleResultManager.Instance.IsBattleWin, leftTime = (int)remTime, passNum = TotalPassEnemy, lossHp = 0 });
    }

    private async Task SpawnMonster()
    {
        var list = BattleDataManager.Instance.GetList(MapUnitType.Monster);
        if (list != null
            && list.Count > 0)
        {
            MaxDistance = 0.1f;
            float difficult = 1f;
            foreach (var VARIABLE in list)
            {
                var dir = Quaternion.AngleAxis(StageBattleUtil.GetMapRotationY(VARIABLE), Vector3.up) * Vector3.forward;
                var pos = VARIABLE.Pos;
                var enemyEntity = UnitFatory.CreateUnit(VARIABLE.Id, 1, pos, dir, 0, false, BattleConst.DEFCampID, BattleConst.DEFTeamID, true, difficult);
                var dis = Vector3.Distance(LinePos, pos);
                if (dis > MaxDistance)
                {
                    MaxDistance = dis;
                }
                
                Battle.Instance.AddDebuffSkill(enemyEntity, VARIABLE.Index);
            }
        }
    }

    /*public float MaxDistance
    {
        get;
        set;
    }
    
    public Vector3 LinePos
    {
        get;
        set;
    }*/
}