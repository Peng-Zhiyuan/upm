using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BattleEngine.Logic;
using BattleSystem.ProjectCore;
using DG.Tweening;
using UnityEngine;

public class GuardModeParam
{
    public List<FormationHeroInfo> heros = new List<FormationHeroInfo>();
}

[BattleModeClass(BattleModeType.Guard)]
public class GuardMode : PveMode
{
    //public DreamEscapeBattleParam _battleParam;
    private GuardModeParam guardParam = null;

    public override void OnCreate(PveModeParam pveParam, object modeParam, Battle battle, CreateBattleResponse response)
    {
        base.OnCreate(pveParam, modeParam, battle, response);
        guardParam = modeParam as GuardModeParam;
        //_battleParam = modeParam as DreamEscapeBattleParam;
    }

    public override BattleModeType ModeType
    {
        get { return BattleModeType.Guard; }
    }

    public override async Task OnPreCreateBattleGameObjectAsync() { }

    //VARIABLE.mData.AddHp(1 - VARIABLE.mData.CurrentHealth.Value);

    public override void OnUpdate()
    {
        base.OnUpdate();
        if (Role_SiLaLi != null
            && Role_SiLaLi.mCurrentState == ACTOR_ACTION_STATE.Dead)
        {
            EffectManager.Instance.CreateBodyEffect("fx_lightball_blue_disappear", Role_SiLaLi.GetBone("foot"), 6f, Vector3.zero, Vector3.one, Vector3.zero);
            Role_SiLaLi = null;
            EventManager.Instance.SendEvent("SetBattleResult", new BattleOutResultData() { bWin = false });
        }
    }

    private Dictionary<string, Vector3> TargetPos = new Dictionary<string, Vector3>();
    private int TotalPassEnemy = 0;

    public override async Task OnWaveExectue()
    {
        var role = SceneObjectManager.Instance.FindCreatureByConfigID(BattleConst.SiLaLiID);
        if (role != null)
        {
            List<CombatActorEntity> enemyList = BattleLogicManager.Instance.BattleData.defActorLst;
            int index = 1;
            for (int i = 0; i < enemyList.Count; i++)
            {
                if (enemyList[i].CurrentHealth.Value <= 0)
                {
                    continue;
                }
                enemyList[i].ClearAllOTNum();
                enemyList[i].AddOTNum(role.mData.UID, index * 50);
                index++;
            }
        }
    }

    public override async Task CreateHeros()
    {
        await this.SpawnHeros();
    }

    /*private int GetHeroIndex(int index)
    {
        if (index < guardParam.heros.Count)
            return guardParam.heros[index];
        else
        {
            return 0;
        }
    }*/

    public async Task SpawnHeros()
    {
        BattleManager.Instance.Init();
        MapUtil.RefreshMapTrigger(true);
        int index = 0;
        List<int> RoleID = new List<int>();
        foreach (var VARIABLE in Battle.Instance.param.memebers)
        {
            if (VARIABLE.MainHeroId == BattleConst.SiLaLiID)
                RoleID.Insert(0, VARIABLE.MainHeroId);
            else
            {
                RoleID.Add(VARIABLE.MainHeroId);
            }
        }
        List<float> temp_angles = new List<float>() { -90, 90, 90 };
        foreach (var VARIABLE in BattleDataManager.Instance.GetList(MapUnitType.MainHero))
        {
            var dir = Quaternion.AngleAxis(temp_angles[index], Vector3.up) * Vector3.forward;
            var pos = StartPos[index];
            //pos.y = 0f;
            VARIABLE.Id = RoleID[index];
            var hero = HeroManager.Instance.GetHeroInfo(VARIABLE.Id);
            if (!hero.Unlocked)
                continue;
            UnitFatory.CreateUnit(VARIABLE.Id, hero.Level, pos, dir, index, true, BattleConst.ATKCampID, BattleConst.ATKTeamID, true, 1f, null, null);
            index++;
        }
        if (this.AssistBattleInfo != null
            && this.AssistBattleInfo.heroes != null
            && this.AssistBattleInfo.heroes.Count > 0)
        {
            var hero = this.AssistBattleInfo.heroes[0];
            UnitFatory.CreateUnit(hero.id, hero.lv, Vector3.zero, Vector3.zero, BattleConst.FriendPosIndex, true, BattleConst.ATKCampID, BattleConst.ATKTeamID, true, 1f, hero);
        }
        await Battle.Instance.SpawnLeader(false);
    }

    public async Task SpawnMonsters(bool IsReady = false)
    {
        var list = BattleDataManager.Instance.GetList(MapUnitType.Monster);
        if (list != null)
        {
            float difficult = 1f;
            foreach (var VARIABLE in list)
            {
                var dir = Quaternion.AngleAxis(StageBattleUtil.GetMapRotationY(VARIABLE), Vector3.up) * Vector3.forward;
                var pos = VARIABLE.Pos;
                pos.y = 0f;
                var enemyEntity = UnitFatory.CreateUnit(VARIABLE.Id, 1, pos, dir, 0, false, BattleConst.DEFCampID, BattleConst.DEFTeamID, IsReady, difficult);
                Battle.Instance.AddDebuffSkill(enemyEntity, VARIABLE.Index);
            }
        }
    }

    public override async Task OnWaveEnd()
    {
        if (Battle.Instance.Wave == 2)
        {
            BattleManager.Instance.StopAllAI();
            foreach (var VARIABLE in BattleManager.Instance.ActorMgr.GetCamp(0))
            {
                VARIABLE.mData.StopAI();
            }

            //BattleManager.Instance.StopAllAI();
        }
    }

    private List<float> angles = new List<float>() { 90, -90, -90 };

    public async Task ResetPos()
    {
        int index = 0;
        foreach (var VARIABLE in BattleDataManager.Instance.GetList(MapUnitType.MainHero))
        {
            var dir = Quaternion.AngleAxis(angles[index], Vector3.up) * Vector3.forward;
            var pos = StartPos[index];
            pos.y = 0f;
            var creature = BattleEngine.Logic.BattleManager.Instance.ActorMgr.GetActorByIndex(index);
            index++;
            if (creature != null)
            {
                if (creature.ConfigID == BattleConst.SiLaLiID)
                {
                    continue;
                }
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
        }
    }

    public override void SendZoneResult()
    {
        var remTime = BattleTimeManager.Instance.BattleTime - BattleTimeManager.Instance.CurrentBattleTime;
        int losshp = 0;
        var role = SceneObjectManager.Instance.FindCreatureByConfigID(BattleConst.SiLaLiID);
        if (role != null)
        {
            losshp = role.mData.CurrentHealth.MaxValue - role.mData.CurrentHealth.Value;
        }
        int killNum = 0;
        foreach (var VARIABLE in BattleManager.Instance.ActorMgr.GetCamp(0))
        {
            killNum += VARIABLE.mData.battleItemInfo.battlePlayerRecord.killList.Count;
        }
        SeaBattleManager.SetBattleResult(new SeaBattleBattleResult()
                        {
                                        win = BattleResultManager.Instance.IsBattleWin,
                                        leftTime = (int)remTime,
                                        passNum = 0,
                                        lossHp = losshp,
                                        killNum = killNum
                        }
        );
    }

    public override void RegistCondition()
    {
        base.RegistCondition();
        var role = SceneObjectManager.Instance.FindCreatureByConfigID(BattleConst.SiLaLiID);
        if (role != null)
        {
            BattleResultManager.Instance.AddCheckData(new BattleResultCheckData() { resultType = BATTLE_RESULT_TYPE.DEFEND_TARGET, Param = role.ID });
        }
    }

    List<Vector3> StartPos = new List<Vector3>() { new Vector3(-35, 0, 0), new Vector3(-38, 0, -1.5f), new Vector3(-38, 0, 1.5f) };

    private int LightBallLoopEffectID = 0;
    private Creature Role_SiLaLi = null;

    public async Task ReadyShow()
    {
        if (Battle.Instance.Wave != 1)
            return;
        var role = SceneObjectManager.Instance.FindCreatureByConfigID(BattleConst.SiLaLiID);
        if (role == null)
            return;
        Role_SiLaLi = role;

        //CameraManager.Instance.CameraProxy.SetTarget(role.transform, role, true);
        //SceneObjectManager.Instance.SetSelectPlayer(role);
        SceneObjectManager.Instance.LocalPlayerCamera.SetDirection(new Vector3(1, 0, 0));
        CameraManager.Instance.TryChangeState(CameraState.Ready, role.GetPosition() + new Vector3(-4, 1.5f, 0));
        SceneObjectManager.Instance.LocalPlayerCamera.SetPosition(role.GetPosition() + new Vector3(-4, 1.5f, 0));
        //CameraSetting.Ins.SetOffsetY(4f);
        CameraManager.Instance.CameraProxy.SideOffsetY = 0.5f;
        CameraManager.Instance.CameraProxy.Distance = 2.5f;
        //CameraManager.Instance.CameraProxy.m_HAngle = 180;
        CameraManager.Instance.TurnImmediate();
        await Task.Delay(3000);
        if (!Battle.Instance.IsFight)
            return;
        var message = LanguageData.MsgList5Table.TryGet("M5_seabattle_sirare_plot1");
        if (message != null)
            GameEventCenter.Broadcast(GameEvent.TalkShow, message, Role_SiLaLi.ConfigID);
        SceneObjectManager.Instance.LocalPlayerCamera.transform.DOMove(role.GetPosition() + new Vector3(0, 1f, 0), 1.5f);
        role.HideWeapon();
        /*var list = BattleManager.Instance.ActorMgr.GetCamp(0);
        float maxT = 0;
        int index = 0;
        foreach (var VARIABLE in list)
        {
            /*if (!VARIABLE.IsMain
                || VARIABLE.ConfigID != BattleConst.SiLaLiID)
                continue;#1#
            Vector3 pos = VARIABLE.GetPosition() + VARIABLE.GetDirection().normalized * 20;
            string anim = "walk";
            if (VARIABLE.ConfigID == BattleConst.SiLaLiID)
                anim = "walk_guard";
            
            var durT = VARIABLE.MoveTo(StartPos[index], 1.5f, anim);
            if (durT > maxT)
                maxT = durT;
            index++;
        }

        await Task.Delay((int) (maxT * 1000));
        if(!Battle.Instance.IsFight)
            return;*/

        //role.PlayAnim("stand", true);
        await Task.Delay(1500);
        if (!Battle.Instance.IsFight)
            return;
        //转身
        role.PlayAnim("arround", true);
        Debug.LogError("播放转身动作");
        await Task.Delay(1830);
        if (!Battle.Instance.IsFight)
            return;
        role.SetDirection(new Vector3(1, 0, 0));
        role.PlayAnim("cast1", true);
        EffectManager.Instance.CreateBodyEffect("fx_lightball_ball_create", role.GetBone("foot"), 4.5f, Vector3.zero, Vector3.one, Vector3.zero);
        //CameraManager.Instance.CameraProxy.Distance = 2;
        //CameraManager.Instance.CameraProxy.SideOffsetY = 0.5f;
        GameEventCenter.Broadcast(GameEvent.EnemyComing, role, 1.5f, 180f);
        await Task.Delay(3900);
        if (!Battle.Instance.IsFight)
            return;
        LightBallLoopEffectID = EffectManager.Instance.CreateBodyEffect("fx_lightball_blue", role.GetBone("foot"), 30000f, Vector3.zero, Vector3.one, Vector3.zero);
        role.IgnoreIdle = true;
        await Task.Delay(1500);
        if (!Battle.Instance.IsFight)
            return;
    }

    private string PlayerHeadIcon = "1501009";

    public async void FocusMonster()
    {
        Creature Monster = null;
        List<Creature> roles = new List<Creature>();
        int index = 1;
        foreach (var VARIABLE in BattleEngine.Logic.BattleManager.Instance.ActorMgr.GetCamp(1))
        {
            if (!VARIABLE.mData.IsDead)
            {
                index++;
                if (index == 2)
                {
                    Monster = VARIABLE;
                }
            }
        }
        if (Monster == null)
            return;
        if (Battle.Instance.Wave == 1)
        {
            var message = LanguageData.MsgList5Table.TryGet("M5_seabattle_sirare_plot2");
            if (message != null)
                GameEventCenter.Broadcast(GameEvent.TalkShow, message, 1502014);
        }
        GameEventCenter.Broadcast(GameEvent.EnemyComing, Monster, 1.5f, 180f);
        //await Task.Delay(1500);
    }

    public async Task OtherGo()
    {
        var list = BattleManager.Instance.ActorMgr.GetCamp(0);
        float maxT = 0;
        int index = 1;
        foreach (var VARIABLE in list)
        {
            if (!VARIABLE.IsMain
                || VARIABLE.ConfigID == BattleConst.SiLaLiID)
                continue;
            Vector3 pos = VARIABLE.GetPosition() + VARIABLE.GetDirection().normalized * 20;
            var durT = VARIABLE.MoveTo(StartPos[index]);
            if (durT > maxT)
                maxT = durT;
            index++;
        }
        await Task.Delay((int)(maxT * 1000) + 1000);
    }

    public async Task NextWave()
    {
        await Task.CompletedTask;
        //SpawnMonsters();
    }

    public override async Task ShowSomething()
    {
        ResetPos();
        var center = GetCenterPosition();
        var target = GetCenterRole();
        var dir = target.GetDirection();
        //SceneObjectManager.Instance.LocalPlayerCamera.SetDirection(dir);
        //CameraManager.Instance.CameraProxy.ChangeImmedate = false;
        //CameraManager.Instance.TryChangeState(CameraState.Ready, center);
        //SceneObjectManager.Instance.LocalPlayerCamera.transform.position = new Vector3(-46, 0, 0);
        SceneObjectManager.Instance.LocalPlayerCamera.transform.DOMove(new Vector3(-46, 1.5f, 2), 2f);
    }

    public override async Task SetReadyCamera()
    {
        //await ResetPos();

        //await Task.Delay(1000);
        //if (Battle.Instance.Wave == 3)
        {
            //BattleManager.Instance.StopAllAI();

            //GameEventCenter.Broadcast(GameEvent.DefenceWaveTips);
            Creature target = SceneObjectManager.Instance.FindCreatureByConfigID(BattleConst.SiLaLiID);
            if (target == null)
            {
                Debug.LogError("------------------角色配置不对");
                return;
            }
            if (Battle.Instance.Wave == 1)
            {
                var dir = target.GetDirection();
                target.mData.SetLifeState(ACTOR_LIFE_STATE.Guard);
                await ReadyShow();
                if (!Battle.Instance.IsFight)
                    return;
                
                await SpawnMonsters();
                //dir = Quaternion.AngleAxis(180,Vector3.up) * dir;
                var center = GetCenterPosition();
                CameraManager.Instance.ShowTransitionBlack();
                SceneObjectManager.Instance.SetSelectPlayer(target);
                await Task.Delay(400);
                await ResetPos();
                //SceneObjectManager.Instance.LocalPlayerCamera.SetDirection(dir);
                CameraManager.Instance.CameraProxy.SetTarget(target.transform, target, true);
                
                CameraManager.Instance.TryChangeState(CameraState.Guard, target);
                CameraManager.Instance.CameraProxy.OffsetSpeed = 3;
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
            }
            else
            {
                SceneObjectManager.Instance.SetSelectPlayer(target);
                /*var center = GetCenterPosition();
                var dir = target.GetDirection();
                SceneObjectManager.Instance.LocalPlayerCamera.SetDirection(dir);
                CameraManager.Instance.TryChangeState(CameraState.Ready, center);*/
                await Task.Delay(2000);
                if (!Battle.Instance.IsFight)
                    return;
                await SpawnMonsters();
                await Task.Delay(1000);
                if (!Battle.Instance.IsFight)
                    return;
                //CameraManager.Instance.CameraProxy.SetTarget(target.transform, target, true);
                //SceneObjectManager.Instance.SetSelectPlayer(target);
                //BattleSubmitUtil.SubmitByNormalParam(CmdType.SelectRole, target.ID);
                CameraManager.Instance.TryChangeState(CameraState.Guard, target);
                //CameraManager.Instance.CameraProxy.TurnImmediate();
                //CameraManager.Instance.CameraProxy.RecoveHAngleImmediate();
                CameraManager.Instance.CameraProxy.OffsetSpeed = 1;
                //await Task.Delay(3000);
            }
            BattleManager.Instance.StartAllAI();
            //BattleLogicManager.Instance.ResetActorLogic(BattleEngine.Logic.BattleManager.Instance.mBattleData);
            BattleManager.Instance.ActorMgr.OpenAI();
            //BattleStateManager.Instance.ChangeState(eBattleState.Play);
            var role = SceneObjectManager.Instance.FindCreatureByConfigID(BattleConst.SiLaLiID);
            if (role != null)
            {
                role.mData.SetLifeState(ACTOR_LIFE_STATE.Guard);
                //BattleLogicManager.Instance.AtkerFocusOnFiring(1001, role.mData.UID);
                role.mData.AttachBuff((int)BUFF_COMMON_CONFIG_ID.SUPER_ARMOR);
                await Task.Delay(1);
                role.PlayAnim("cast2");
            }
        }
    }

    public override async Task BattleEnd(bool win)
    {
        var role = SceneObjectManager.Instance.FindCreatureByConfigID(BattleConst.SiLaLiID);
        if (role == null)
            await Task.CompletedTask;
        role.IgnoreIdle = false;

        //role.mData.lifeState = ACTOR_LIFE_STATE.StopLogic;
        CameraManager.Instance.TryChangeState(CameraState.Front, role);
        //CameraSetting.Ins.SetOffsetY(4f);
        CameraManager.Instance.CameraProxy.SideOffsetY = 1f;
        CameraManager.Instance.CameraProxy.Distance = 3;
        CameraManager.Instance.CameraProxy.m_HAngle = 0;
        EffectManager.Instance.RemoveEffect(LightBallLoopEffectID);
        if (win)
        {
            role.PlayAnim("cast3", true);
            EffectManager.Instance.CreateBodyEffect("fx_exposure", role.GetBone("foot"), 2f, Vector3.zero, Vector3.one, Vector3.zero);
            await Task.Delay(1700);
            if (!Battle.Instance.IsFight)
                return;
            EffectManager.Instance.CreateCameraEffect("fx_exposure_2", 2.5f, new Vector3(0, 0.23f, 0), Vector3.one);
            await Task.Delay(2000);
            if (!Battle.Instance.IsFight)
                return;
        }
        else
        {
            //role.mData.AddHp(- role.mData.CurrentHealth.Value);
            //role.PlayAnim("dead");
            //role.PlayAnim();
            //EffectManager.Instance.CreateBodyEffect("fx_lightball_blue_disappear", role.GetBone("foot"), 4.5f, Vector3.zero, Vector3.one, Vector3.zero);
        }
    }
}