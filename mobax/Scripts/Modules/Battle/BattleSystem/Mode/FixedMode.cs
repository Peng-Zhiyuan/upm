using System.Collections.Generic;
using System.Threading.Tasks;
using BattleEngine.Logic;
using BattleSystem.ProjectCore;
using DG.Tweening;
using nobnak.Gist.ObjectExt;
using Unity.Mathematics;
using UnityEngine;

[BattleModeClass(BattleModeType.Fixed)]
public class FixedMode : PveMode
{
    public override BattleModeType ModeType
    {
        get { return BattleModeType.Fixed; }
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
        if (!Battle.Instance.BattleStarted)
            return;
        var list = BattleManager.Instance.ActorMgr.GetCamp(1);
        if (list.Count <= 2)
        {
            if (Battle.Instance.Wave < Battle.Instance.MaxWave)
            {
                Battle.Instance.Wave++;
                SpawnMonster();
                GameEventCenter.Broadcast(GameEvent.NextDefenceWave);
            }
        }
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
                    //GameEventCenter.Broadcast(GameEvent.EnemyComing, VARIABLE, 1.5f, 0f);
                    //GameEventCenter.Broadcast(GameEvent.ShowTalk, "有敌人要突破防线了，拦住他！！", PlayerHeadIcon);
                    //GameEventCenter.Broadcast(GameEvent.FocusEnemy, VARIABLE);
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
    private Creature Role_SiLaLi = null;
    public async Task ReadyShow()
    {
        if (Battle.Instance.Wave != 1)
            return;
        var role = SceneObjectManager.Instance.FindCreatureByConfigID(BattleConst.ShuiJinID);
        if (role == null)
            return;
        Role_SiLaLi = role;

        /*SceneObjectManager.Instance.LocalPlayerCamera.SetDirection(new Vector3(-1, 0, 0));
        CameraManager.Instance.TryChangeState(CameraState.Ready, role.GetPosition() + new Vector3(0, 1f, 0));
        SceneObjectManager.Instance.LocalPlayerCamera.SetPosition(role.GetPosition() + new Vector3(0, 1f, 0));

        CameraManager.Instance.CameraProxy.SideOffsetY = 0.5f;
        CameraManager.Instance.CameraProxy.Distance = 2.5f;
        CameraManager.Instance.TurnImmediate();*/
        /*await Task.Delay(3000);
        if (!Battle.Instance.IsFight)
            return;*/
        //SceneObjectManager.Instance.LocalPlayerCamera.transform.DOMove(role.GetPosition() + new Vector3(0, 1f, 0), 1.5f);
        //
        /*await Task.Delay(1500);
        if (!Battle.Instance.IsFight)
            return;*/
    }


    public override async Task SetReadyCamera()
    {
        if (Role_SiLaLi == null)
        {
            SpawnDefenceTarget();
        }
        
        Creature target = SceneObjectManager.Instance.FindCreatureByConfigID(BattleConst.ShuiJinID);
        if (target == null)
        {
            Debug.LogError("------------------角色配置不对");
            return;
        }
        
        if (Battle.Instance.Wave != 1)
        {
            await SpawnMonster();
            if (!Battle.Instance.IsFight)
                return;
            //ResetPos();
            
            CameraManager.Instance.TryChangeState(CameraState.Defence, target);
            //CameraManager.Instance.CameraProxy.TurnImmediate();
            //CameraManager.Instance.CameraProxy.RecoveHAngleImmediate();
            CameraManager.Instance.CameraProxy.OffsetSpeed = 1;
        }
        else
        {
            
            await SpawnMonster();
            
            ResetPos();
            
            await ReadyShow();
            if (!Battle.Instance.IsFight)
                return;
            
            SceneObjectManager.Instance.SetSelectPlayer(GetCenterRole());
            var center = GetCenterPosition();
            CameraManager.Instance.ShowTransitionBlack();
            await Task.Delay(400);
            
            //SceneObjectManager.Instance.LocalPlayerCamera.SetDirection(dir);

            //target = GetCenterRole();
            CameraManager.Instance.CameraProxy.SetTarget(target.transform, target, true);
            
            CameraManager.Instance.TryChangeState(CameraState.Defence, target);
            CameraManager.Instance.CameraProxy.OffsetSpeed = 3;
            CameraManager.Instance.TurnImmediate();
            FocusMonster();
            await Task.Delay(100);
            if (!Battle.Instance.IsFight)
                return;
            //await OtherGo();
            //if(!Battle.Instance.IsFight)
            //return;
            await ShowWeapon();
            if (!Battle.Instance.IsFight)
                return;
            GameEventCenter.Broadcast(GameEvent.ShowRoleHp, Role_SiLaLi);
            
            GuideManagerV2.Stuff.Notify("Battle.DefenceStart");
        }
        BattleManager.Instance.StopAllAI();
        
        await FocusMonster();
        if (!Battle.Instance.IsFight)
            return;
        
        /*if (Battle.Instance.Wave == 1)
        {
            CameraManager.Instance.CameraProxy.SetTarget(target.transform, target, true);
            //SceneObjectManager.Instance.SetSelectPlayer(target);
            //SceneObjectManager.Instance.SetSelectPlayer(GetCenterRole());
            //BattleSubmitUtil.SubmitByNormalParam(CmdType.SelectRole, target.ID);
        }*/
       
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
                CombatActorEntity monsterActor = UnitFatory.CreateUnit(VARIABLE.Id, 1, pos, dir, 0, false, BattleConst.DEFCampID, BattleConst.DEFTeamID, true, difficult);
                monsterActor.SetLifeState(ACTOR_LIFE_STATE.Alive);
                var dis = Vector3.Distance(LinePos, pos);
                if (dis > MaxDistance)
                {
                    MaxDistance = dis;
                }
                Battle.Instance.AddDebuffSkill(monsterActor, VARIABLE.Index);
            }
        }
    }

    //生成防御目标
    private void SpawnDefenceTarget()
    {
        Vector3 dir = Vector3.forward;
        foreach (var VARIABLE in BattleDataManager.Instance.GetList(MapUnitType.MainHero))
        {
            dir = Quaternion.AngleAxis(StageBattleUtil.GetMapRotationY(VARIABLE) + 180f, Vector3.up) * Vector3.forward;
            break;
        }

        var root = GameObject.Find("shuijing");
        if(root == null)
            return;
        
        var unit = UnitFatory.CreateUnit(BattleConst.ShuiJinID, 0, root.transform.position, dir, BattleConst.DefenceTargetIndex, true, BattleConst.ATKCampID, BattleConst.ATKTeamID, true);
        unit.AttachBuff((int)BUFF_COMMON_CONFIG_ID.Force_HP);
        unit.AttachBuff((int)BUFF_COMMON_CONFIG_ID.Force_Debuff);
        unit.AttachBuff((int)BUFF_COMMON_CONFIG_ID.SUPER_ARMOR);
        unit.AttachBuff((int)BUFF_COMMON_CONFIG_ID.FORBID_CURE);
        unit.isOpenAITree = false;
        
        LinePos = root.transform.position;
    }

    public override void RegistCondition()
    {
        base.RegistCondition();
        var role = SceneObjectManager.Instance.FindCreatureByConfigID(BattleConst.ShuiJinID);
        if (role != null)
        {
            BattleResultManager.Instance.AddCheckData(new BattleResultCheckData() { resultType = BATTLE_RESULT_TYPE.DEFEND_TARGET, Param = role.ID });
        }
    }
}