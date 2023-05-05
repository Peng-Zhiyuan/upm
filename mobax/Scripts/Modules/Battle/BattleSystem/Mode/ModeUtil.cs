using System;
using BattleSystem.ProjectCore;
using System.Collections.Generic;
using System.Reflection;
using nobnak.Gist.ObjectExt;

public static class ModeUtil
{
    static Dictionary<BattleModeType, Type> battleModeToModeTypeDic = new Dictionary<BattleModeType, Type>();

    static void TryRebuildTypeDic()
    {
        if (battleModeToModeTypeDic.Count == 0)
        {
            lock (battleModeToModeTypeDic)
            {
                if (battleModeToModeTypeDic.Count != 0)
                {
                    return;
                }
                var assembly = typeof(Mode).Assembly;
                var coreModeTypeList = ReflectionUtil.GetSubClasses<Mode>(assembly);
                foreach (var one in coreModeTypeList)
                {
                    var attribute = one.GetCustomAttribute<BattleModeClassAttribute>(false);
                    if (attribute == null)
                    {
                        continue;
                    }
                    var thisMode = attribute.mode;
                    battleModeToModeTypeDic[thisMode] = one;
                }
            }
        }
    }

    static Type GetModeType(BattleModeType mode)
    {
        TryRebuildTypeDic();
        battleModeToModeTypeDic.TryGetValue(mode, out var type);
        if (type == null)
        {
            throw new Exception("[Battle] Not found battle core mode: " + mode);
        }
        return type;
    }

    public static Mode Create(BattleModeType mode, PveModeParam pveParam, object modeParam, Battle battle, CreateBattleResponse response)
    {
        var type = GetModeType(mode);
        var ret = Activator.CreateInstance(type) as Mode;
        ret.OnCreate(pveParam, modeParam, battle, response);
        return ret;
    }

    public static List<MapWaveDataObject> ReplaceMonsterInfo(List<MapWaveDataObject> mapWaveDataObjects, List<int> mosterGroups)
    {
        List<List<int>> monsterGroupID = new List<List<int>>();
        for (int i = 0; i < mosterGroups.Count; i++)
        {
            if (mosterGroups[i] == 0)
            {
                continue;
            }
            var monsterGroupRow = StaticData.PortMonsterTable.TryGet(mosterGroups[i]);
            List<int> groupList = new List<int>();
            if (monsterGroupRow.Boss != 0)
            {
                groupList.Add(monsterGroupRow.Boss);
            }
            if (monsterGroupRow.Monsters != null)
            {
                for (int j = 0; j < monsterGroupRow.Monsters.Length; j++)
                {
                    if (monsterGroupRow.Monsters[j] == 0)
                    {
                        continue;
                    }
                    groupList.Add(monsterGroupRow.Monsters[j]);
                }
            }
            if (groupList.Count == 0)
            {
                break;
            }
            monsterGroupID.Add(groupList);
        }
        List<MapWaveDataObject> newWaveDataObjects = new List<MapWaveDataObject>();
        for (int i = 0; i < monsterGroupID.Count; i++)
        {
            if (i >= mapWaveDataObjects.Count)
            {
                break;
            }
            MapWaveDataObject newData = new MapWaveDataObject();
            newData = mapWaveDataObjects[i].DeepCopy();
            newData.MonsterList.Clear();
            for (int j = 0; j < monsterGroupID[i].Count; j++)
            {
                if (j >= mapWaveDataObjects[i].MonsterList.Count)
                {
                    break;
                }
                MapWaveModelData newModeData = mapWaveDataObjects[i].MonsterList[j].DeepCopy();
                newModeData.Id = monsterGroupID[i][j];
                newData.MonsterList.Add(newModeData);
            }
            newWaveDataObjects.Add(newData);
        }
        return newWaveDataObjects;
    }

    public static List<MapWaveDataObject> ResetRandomMonster(PveModeParam pveModeParam)
    {
        var r = new List<MapWaveDataObject>();
        PveModeParam modeParam = pveModeParam;
        var stageRow = StaticData.StageTable.TryGet(modeParam.CopyId);
        var sceneId = stageRow.mapId;
        var mapCreateRow = StaticData.MapcreateTable.TryGet(sceneId);
        foreach (var rr in mapCreateRow.Monsters)
        {
            var waveData = modeParam.WaveDataList.Find(val => val.WaveId == rr.Waveid);
            if (waveData == null) continue;
            r.Add(ResetRandomMonster(waveData, rr));
        }
        return r;
    }

    public static MapWaveDataObject ResetRandomMonster(MapWaveDataObject waveData, Rougelike monsterGroup)
    {
        if (monsterGroup == null) return waveData;
        var monsterGroups = StaticData.RougelikeMonsterTable.TryGet(monsterGroup.Monsterid).Colls;
        if (monsterGroups == null) return waveData;

        //TODO:随机要改成随机种子
        var monsterGroupId = monsterGroups[UnityEngine.Random.Range(0, monsterGroups.Count)].Monsterid;
        var monsterGroupRow = StaticData.PortMonsterTable.TryGet(monsterGroupId);
        var monsterIdList = new List<int>();
        monsterIdList.AddRange(monsterGroupRow.Monsters);
        if (monsterGroupRow.Boss > 0)
        {
            monsterIdList.Add(monsterGroupRow.Boss);
        }
        for (var i = 0; i < waveData.MonsterList.Count; i++)
        {
            var monster = waveData.MonsterList[i];
            if (i > monsterIdList.Count - 1) continue;
            monster.Id = monsterIdList[i];
        }
        return waveData;
    }
}