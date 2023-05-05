using System.Collections.Generic;
using System.Linq;
using BattleEngine.Logic;
using BattleSystem.ProjectCore;
using UnityEngine;
using Debug = behaviac.Debug;

public class BattleDataManager : Singleton<BattleDataManager>
{
    public List<MapWaveDataObject> WaveDataList { get; set; }

    public int CopyId { get; set; }
    //public List<int> Items { get; set; }

    //主线和竞技场战斗，根据双方战力对比额外添加buffer
    public int ExternalBufferID = -1;

    //大招是否播放timeline
    public bool IsPlayCutScene { get; set; }

    float _BattleSpeed = 1f;
    public float BattleSpeed
    {
        get { return _BattleSpeed; }
        set
        {
            _BattleSpeed = value;
            if (!Battle.Instance.IsFight || Battle.Instance.mode.ModeType == BattleModeType.SkillView)
            {
                Time.timeScale = 1f;
            }
            else
            {
                Time.timeScale = _BattleSpeed * _TimeScale;
            }
        }
    }

    private float _TimeScale = 1f;
    public float TimeScale
    {
        get { return _TimeScale; }
        set
        {
            _TimeScale = value;
            
            if (!Battle.Instance.IsFight || Battle.Instance.mode.ModeType == BattleModeType.SkillView)
            {
                Time.timeScale = 1f;
            }
            else
            {
                Time.timeScale = _BattleSpeed * _TimeScale;
            }
        }
    }

    public void SetData(Mode mode)
    {
        this.WaveDataList = mode.GetWaveDataList();
        //this.Items = mode.GetBattleItems();
        this.CopyId = mode.GetCopyId();
        ItemRecord.Clear();
        ExternalBufferID = CaculateExternalBuffer(mode);
        if (ExternalBufferID != -1)
        {
            BattleLog.LogWarning("添加战力Buffer = " + ExternalBufferID);
        }
        InitStageBuff();
        IsPlayCutScene = BattleSettingUtil.GetInt("SSEnable", 1) == 1;
        BattleSpeed = BattleSettingUtil.GetFloat("BattleSpeed", 1f);
        
        SendSpeedChanged(BattleSettingUtil.GetInt("SpeedSet", 1));
    }

    public List<MapWaveModelData> GetList(MapUnitType type, int Wave = -1)
    {
        if (Wave == -1)
            Wave = Battle.Instance.Wave;
        if (WaveDataList == null
            || Wave > WaveDataList.Count)
        {
            return new List<MapWaveModelData>();
        }
        var curData = WaveDataList[Wave - 1];
        if (type == MapUnitType.Monster)
        {
            return curData.MonsterList;
        }
        else if (type == MapUnitType.MainHero)
        {
            return curData.MainHeroList;
        }
        else if (type == MapUnitType.SubHero)
        {
            return curData.SubHeroList;
        }
        else if (type == MapUnitType.Player)
        {
            return new List<MapWaveModelData>() { curData.LeadData };
        }
        return new List<MapWaveModelData>();
    }

    //使用道具
    public Dictionary<int, int> ItemRecord = new Dictionary<int, int>();

    public bool UseItem(int id)
    {
        if (id == 0)
            return false;
        int use_num = 0;
        if (ItemRecord.ContainsKey(id))
            use_num = ItemRecord[id];
        else
        {
            ItemRecord.Add(id, 0);
        }
        var cur_num = ItemUtil.GetHoldCount(id);
        if (cur_num < use_num + 1)
            return false;
        ItemRecord[id] += 1;
        GameEventCenter.Broadcast(GameEvent.RefreshItemNum, lasttype);
        return true;
    }

    public bool ItemIsCanUse(int id)
    {
        int use_num = 0;
        if (ItemRecord.ContainsKey(id))
            use_num = ItemRecord[id];
        var cur_num = ItemUtil.GetHoldCount(id);
        if (cur_num < use_num + 1)
            return false;
        return true;
    }

    public int GetItemNum(int id)
    {
        var cur_num = ItemUtil.GetHoldCount(id);
        int use_num = 0;
        if (ItemRecord.ContainsKey(id))
            use_num = ItemRecord[id];
        return cur_num - use_num;
    }

    //主线和竞技场战斗，根据双方战力对比额外添加buffer
    public int CaculateExternalBuffer(Mode mode)
    {
        if (!Battle.Instance.IsMainTask)
        {
            if (mode.ModeType != BattleModeType.Arena && mode.ModeType != BattleModeType.TowerFixed && mode.ModeType != BattleModeType.TowerNormal)
            {
                return -1;
            }
        }

        int atkPower = 0;
        var list = GetList(MapUnitType.MainHero);
        foreach (var VARIABLE in list)
        {
            var hero = HeroManager.Instance.GetHeroInfo(VARIABLE.Id);
            if (hero != null)
            {
                atkPower += hero.ServerPower;
            }
        }
        int defPower = 0;
        if (Battle.Instance.IsMainTask)
        {
            var stage = StaticData.StageTable.TryGet(CopyId);
            if (stage != null)
            {
                defPower = stage.suggestCombat;
            }
        }
        
        if (mode.ModeType == BattleModeType.TowerFixed || mode.ModeType == BattleModeType.TowerNormal)
        {
            TowerModeParam modeParam = (TowerModeParam)Battle.Instance.param.modeParam;
            var stage = StaticData.TowerTable.TryGet(modeParam.TowerID);
            if (stage != null)
            {
                defPower = stage.Power;
            }
        }
        
        if (mode.ModeType == BattleModeType.Arena)
        {
            var arenamode = mode as ArenaMode;
            defPower = arenamode.param.enemyPower;
        }
        //Debug.LogError($"关卡ID = {CopyId}， 双方战力 ：{atkPower}---{defPower}");
        float percent = atkPower * 1f / defPower;
        if (percent > 1.2f)
        {
            return (int)BattleUtil.GetGlobalK(GlobalK.ExternalBuffer_50);
        }
        else if (percent > 1f)
        {
            return (int)BattleUtil.GetGlobalK(GlobalK.ExternalBuffer_51);
        }
        else if (percent < 0.5f)
        {
            return (int)BattleUtil.GetGlobalK(GlobalK.ExternalBuffer_53);
        }
        else if (percent < 0.8f)
        {
            return (int)BattleUtil.GetGlobalK(GlobalK.ExternalBuffer_52);
        }
        return -1;
    }

    //获取道具（1破防，2助战， 3集火）
    public List<int> GetItems(int type)
    {
        List<int> list = new List<int>();
        foreach (var VARIABLE in StaticData.TacticTable.ElementList)
        {
            if (VARIABLE.Release == type)
            {
                var num = GetItemNum(VARIABLE.Id);
                if (num > 0)
                {
                    list.Add(VARIABLE.Id);
                }
            }
        }
        return list;
    }

    public void UseItem(Creature role, int ItemID)
    {
        if (role.mData.IsCantSelect)
        {
            return;
        }
        var tacticRow = StaticData.TacticTable.TryGet(ItemID);
        if (tacticRow == null)
            return;
        BattleManager.Instance.SendSpendItemSkill(role.mData.UID, tacticRow.skillId);
        BattleManager.Instance.BattleInfoRecord.SetItemUse(tacticRow.Id);
    }

    public List<int> GetItemSkillLst()
    {
        List<int> tempLst = new List<int>();
        var lst = StaticData.TacticTable.ElementList;
        for (int i = 0; i < lst.Count; i++)
        {
            if (GetItemNum(lst[i].Id) > 0
                && !tempLst.Contains(lst[i].skillId))
            {
                tempLst.Add(lst[i].skillId);
            }
        }
        return tempLst;
    }

#region 全场Debuff
    private List<int> stageBuffWaveIndexLst = new List<int>();
    private List<MapWaveModelData> useStageBuffModelDataLst = new List<MapWaveModelData>();
    private List<WavePassiveSkill> stagePassiveSkillLst = new List<WavePassiveSkill>();

    public void InitStageBuff()
    {
        stageBuffWaveIndexLst.Clear();
        useStageBuffModelDataLst.Clear();
        stagePassiveSkillLst.Clear();
        if (WaveDataList == null)
        {
            return;
        }
        int mosterIndex = 0;
        for (int i = 0; i < WaveDataList.Count; i++)
        {
            if (WaveDataList[i].WavePassiveSkillData == null
                || WaveDataList[i].WavePassiveSkillData.PassiveSkillID.Count == 0
                || WaveDataList[i].MonsterList == null
                || WaveDataList[i].MonsterList.Count == 0)
            {
                continue;
            }
            stageBuffWaveIndexLst.Add(i);
            mosterIndex = UnityEngine.Random.Range(0, WaveDataList[i].MonsterList.Count);
            useStageBuffModelDataLst.Add(WaveDataList[i].MonsterList[mosterIndex]);
            stagePassiveSkillLst.Add(WaveDataList[i].WavePassiveSkillData);
        }
        if (stageBuffWaveIndexLst.Count > 0)
        {
            return;
        }
        int stageBuffID = 0;
        if (Battle.Instance.mode.ModeType == BattleModeType.TowerNormal
            || Battle.Instance.mode.ModeType == BattleModeType.TowerFixed)
        {
            TowerModeParam modeParam = (TowerModeParam)Battle.Instance.param.modeParam;
            TowerRow towerRow = StaticData.TowerTable[modeParam.TowerID];
            if (towerRow.stageBuffRand != 0)
            {
                int randUse = UnityEngine.Random.Range(0, 1000);
                if (randUse > towerRow.stageBuffRand)
                {
                    return;
                }
            }
            stageBuffID = towerRow.stageBuffID;
        }
        else
        {
            StageRow stageRow = StaticData.StageTable.TryGet(CopyId);
            if (stageRow == null
                || stageRow.stageBuffID == 0)
            {
                return;
            }
            if (stageRow.stageBuffRand != 0)
            {
                int randUse = UnityEngine.Random.Range(0, 1000);
                if (randUse > stageRow.stageBuffRand)
                {
                    return;
                }
            }
            stageBuffID = stageRow.stageBuffID;
        }
        StageBuffRowArray rowArray = StaticData.StageBuffTable.TryGet(stageBuffID);
        if (rowArray == null
            || rowArray.Colls.Count == 0)
        {
            return;
        }
        int waveIndex = UnityEngine.Random.Range(0, WaveDataList.Count);
        mosterIndex = UnityEngine.Random.Range(0, WaveDataList[waveIndex].MonsterList.Count);
        int passiveIndex = UnityEngine.Random.Range(0, rowArray.Colls.Count);
        stageBuffWaveIndexLst.Add(waveIndex);
        useStageBuffModelDataLst.Add(WaveDataList[waveIndex].MonsterList[mosterIndex]);
        stagePassiveSkillLst.Add(new WavePassiveSkill() { PassiveSkillID = new List<int>() { rowArray.Colls[passiveIndex].Key } });
    }

    public MapWaveModelData GetWaveMonsterData(int wave)
    {
        for (int i = 0; i < stageBuffWaveIndexLst.Count; i++)
        {
            if (stageBuffWaveIndexLst[i] == wave - 1)
            {
                return useStageBuffModelDataLst[i];
            }
        }
        return null;
    }

    public WavePassiveSkill GetWavePassiveSkill(int wave)
    {
        for (int i = 0; i < stageBuffWaveIndexLst.Count; i++)
        {
            if (stageBuffWaveIndexLst[i] == wave - 1)
            {
                return stagePassiveSkillLst[i];
            }
        }
        return null;
    }
#endregion

#region 道具使用
    public Dictionary<int, int> Items = new Dictionary<int, int>();
    public Dictionary<int, float> ItemsCD = new Dictionary<int, float>();
    public Dictionary<int, float> ItemsRemTime = new Dictionary<int, float>();

    public void InitItem(List<int> items)
    {
        ItemsCD.Clear();
        Items.Clear();
        foreach (var VARIABLE in items)
        {
            var itemRow = StaticData.TacticTable.TryGet(VARIABLE);
            if (itemRow == null)
                continue;
            ItemsCD.Add(itemRow.Release, itemRow.Cd / 1000f);
            Items.Add(itemRow.Release, VARIABLE);
        }
    }

    public void ResetItemEnable(int type)
    {
        bool enable = GetTypeEnableState(type);
        int val = enable ? 0 : 1;
        PlayerPrefs.SetInt($"{Database.Stuff.roleDatabase.Me._id}ItemEnable{type}", val);
    }

    public bool GetTypeEnableState(int type)
    {
        return PlayerPrefs.GetInt($"{Database.Stuff.roleDatabase.Me._id}ItemEnable{type}", 0) == 1;
    }

    //
    public bool IsItemCanUse(int type)
    {
        bool enable = GetTypeEnableState(type);
        ;
        if (!enable)
            return false;
        if (GetItemID(type) == 0)
            return false;
        if (GetItemCD(type) > 0)
            return false;
        return true;
    }

    public int GetItemID(int type)
    {
        int dt;
        if (Items.TryGetValue(type, out dt))
        {
            return dt;
        }
        return 0;
    }

    public void StartItemCD(int type)
    {
        float dt;
        if (!ItemsCD.TryGetValue(type, out dt))
        {
            return;
        }
        if (!ItemsRemTime.ContainsKey(type))
        {
            ItemsRemTime.Add(type, dt);
        }
        else
        {
            ItemsRemTime[type] = dt;
        }
    }

    public float GetItemCD(int type)
    {
        if (!ItemsRemTime.ContainsKey(type))
        {
            return 0;
        }
        return ItemsRemTime[type];
    }
#endregion

    public void UpdateFrame(float dt)
    {
        foreach (var VARIABLE in ItemsRemTime.Keys.ToArray())
        {
            ItemsRemTime[VARIABLE] -= dt;
        }
        foreach (var VARIABLE in ItemsRemTime.Keys.ToArray())
        {
            if (ItemsRemTime[VARIABLE] <= 0)
                ItemsRemTime.Remove(VARIABLE);
        }
    }

    private int lasttype = 0;

    public void UseItemByType(int type)
    {
        if (!IsItemCanUse(type))
            return;
        var id = GetItemID(type);
        lasttype = type;
        if (UseItem(id))
            GameEventCenter.Broadcast(GameEvent.FocusItemUseEvent);
        else
        {
            return;
        }
        BattleManager.Instance.BattleInfoRecord.SetItemUse(id);
        //owner.ShowPlayerUseItemAnim();
        Creature creature = SceneObjectManager.Instance.CurSelectHero;
        if (creature != null)
        {
            UseItem(creature, id);
        }
        StartItemCD(type);
    }
    
    private string[] speedsounds = new string[] {"OriginalSpeed", "DoubleSpeed", "TripleSpeed"};

    public void SendSpeedChanged(int index)
    {
        WwiseEventManager.SendEvent(TransformTable.Custom, speedsounds[index-1]);
    }
}