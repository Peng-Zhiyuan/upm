using System.Collections.Generic;
using BattleSystem.ProjectCore;

public class PveModeParam
{
    public int CopyId; // 关卡id
    public EFormationIndex FormationIndex; // 编队index
    public EPveMapType MapType; // pve地图类型
    public int SceneId; // pve-制作关卡 、 pveMap-大地图场景
    public List<MapWaveDataObject> WaveDataList; // 波次数据
    public List<MapPartConfig> MapParts; // 近景地块数据
    public List<EnvironmentPartConfig> EnvironmentParts; // 中景地块数据
    public List<MapEffectBase> MapEffects; // 地块特效数据
    public List<int> Items; // 战术道具
    public string AssistUID; // 助战uid
}