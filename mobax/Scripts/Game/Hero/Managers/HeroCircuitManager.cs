using System.Collections.Generic;
using System.Linq;
using UnityEngine;

enum CircuitLockType
{
    Na = 1,
    Locked,
    Available
}

public static class HeroCircuitManager
{
    // { [index: number]: HeroCircuitInfo }
    private static Dictionary<string, HeroCircuitInfo> _heroCircuitMap;
    // { [stage: number]: List<string> }
    private static Dictionary<int, List<string>> _blockMap;
    // { [index: number]: List<HeroCircuitInfo> }
    private static Dictionary<int, List<HeroCircuitInfo>> _heroCircuits;
    // { [index: number]: { [attr: number]: number } }
    private static Dictionary<int, Dictionary<HeroAttr, int>> _circuitAttrMap;
    // { [index: number]: List<string> }
    private static Dictionary<int, List<string>> _heroCircuitOccupied;

    static HeroCircuitManager()
    {
        _heroCircuitMap = new Dictionary<string, HeroCircuitInfo>();
        _blockMap = new Dictionary<int, List<string>>();
        _heroCircuits = new Dictionary<int, List<HeroCircuitInfo>>();
        _circuitAttrMap = new Dictionary<int, Dictionary<HeroAttr, int>>();
        _heroCircuitOccupied = new Dictionary<int, List<string>>();
        
        // 在这里做一个监听， 如果某块拼图天生就已经是装在拼盘上了，那么要给它逻辑上放上去
        Database.Stuff.itemDatabase.AddItemTypeChange(IType.Puzzle, _OnCircuitChanged);
    }

    public static List<ItemInfo> All
    {
        get
        {
            var list = Database.Stuff.itemDatabase.GetItemInfoListByIType(IType.Puzzle);
            return list;
        }
    }

    /** 判断是否还有空位 */
    public static bool CheckMapFull(HeroInfo heroInfo)
    {
        _InitCircuits(heroInfo.HeroId);
        var occupiedList = GetOccupied(heroInfo.HeroId);
        var blocksCount = 0;
        
        var arr = StaticData.PuzzleBgTable.TryGet(heroInfo.Job).Colls;
        foreach (var row in arr)
        {
            switch ((CircuitUnlockType) row.Type)
            {
                case CircuitUnlockType.T0_Basic:
                // 如果星级小于当前值， 就是开放了
                case CircuitUnlockType.T1_Star when row.Stage <= heroInfo.CircuitStarStage:
                // 如果等级小于当前值， 就是开放了
                case CircuitUnlockType.T2_Level when row.Stage <= heroInfo.CircuitLevelStage:
                    break;
                default:
                    continue;
            }
            
            blocksCount += row.Nodes.Count;
        }

        return occupiedList.Count >= blocksCount;
    }

    /** 该英雄可以用的列表 */
    public static List<ItemInfo> GetList(HeroInfo heroInfo)
    {
        return All.FindAll(item =>
        {
            // 绑定的导师（英雄）
            var bind = item.attach.TryGet("bind", "");
            if (string.IsNullOrEmpty(bind)) return true;
            
            return bind == heroInfo.InstanceId;
        });
    }

    public static List<ItemInfo> GetBindList(int heroId)
    {
        var heroInfo = HeroManager.Instance.GetHeroInfo(heroId);
        return GetBindList(heroInfo);
    }
    
    public static List<ItemInfo> GetBindList(HeroInfo heroInfo)
    {
        return All.FindAll(item =>
        {
            // 绑定的导师（英雄）
            var bind = item.attach.TryGet("bind", "");
            return bind == heroInfo.InstanceId;
        });
    }

    /** 获取拼图信息 */
    public static HeroCircuitInfo GetCircuitInfo(ItemInfo itemInfo) 
    {
        if (!_heroCircuitMap.TryGetValue(itemInfo._id, out var circuitInfo))
        {
            circuitInfo = _heroCircuitMap[itemInfo._id] = new HeroCircuitInfo(itemInfo);
        }

        return circuitInfo;
    }
    
    /** 获取拼图信息 */
    public static HeroCircuitInfo GetCircuitInfo(string instanceId)
    {
        var itemInfo = Database.Stuff.itemDatabase.GetItemInfoByInstanceId(instanceId);
        return GetCircuitInfo(itemInfo);
    }

    public static List<HeroCircuitInfo> GetCircuits(int heroId)
    {
        _InitCircuits(heroId);
        return _heroCircuits[heroId];
    }
    
    /** 取得对应英雄的拼图技能 */
    public static List<int> GetSkills(int heroId)
    {
        var circuitInfos = GetCircuits(heroId);
        var skills = new List<int>();
        foreach (var circuitInfo in circuitInfos)
        {
            if (circuitInfo.SkillUnlocked && circuitInfo.Skill > 0)
            {
                // 不再重复添加了
                if (!skills.Contains(circuitInfo.Skill))
                {
                    skills.Add(circuitInfo.Skill);
                }
            }
        }

        return skills;
    }

    /** 获取该英雄的所有拼图属性  */
    public static Dictionary<HeroAttr, int> GetCircuitAttrMap(int heroId)
    {
        _InitCircuits(heroId);
        return _circuitAttrMap.TryGetValue(heroId);
    }

    public static void PutOn(int heroId, HeroCircuitInfo circuit)
    {
        var circuits = GetCircuits(heroId);
        circuits.Add(circuit);
        // 占据的格子
        var occupied = GetOccupied(heroId);
        var coordinate = circuit.Coordinate;
        foreach (var node in circuit.Nodes)
        {
            occupied.Add($"{coordinate.X + node.X}_{coordinate.Y + node.Y}");
        }
        // Debug.Log("当前已占据的位置: ${occupied}");

        // 然后要把属性也加上
        if (!_circuitAttrMap.TryGetValue(heroId, out var attrMap))
        {
            attrMap = _circuitAttrMap[heroId] = new Dictionary<HeroAttr, int>();
        }
        HeroCircuitHelper.GetOrRefreshAttrs(circuit, attrMap);
    }

    /** 拿走一块积木 */
    public static void TakeAway(HeroCircuitInfo circuit)
    {
        // 没有使用的circuit不需要take away
        if (!circuit.ItemInfo.IsUsed) return;
        
        var heroId = circuit.HeroId;
        TakeAway(circuit, heroId);
    }
    
    /** 拿走一块积木 */
    public static void TakeAway(HeroCircuitInfo circuit, int heroId)
    {
        // 拼图数组中删除
        var circuits = GetCircuits(heroId);
        // 如果本来就不在拼图里，就可以不往下走了
        if (!circuits.Remove(circuit)) return;
        // 拼图格子标识中删除
        var occupied = GetOccupied(heroId);
        var coordinate = circuit.Coordinate;
        foreach (var node in circuit.Nodes)
        {
            occupied.Remove($"{coordinate.X + node.X}_{coordinate.Y + node.Y}");
        }
        // 属性也得更新
        if (_circuitAttrMap.TryGetValue(heroId, out var attrMap))
        {
            HeroCircuitHelper.UpdateAttrsByRemove(circuit, attrMap);
        }
    }

    /** 拿掉所有积木 */
    public static void TakeAwayAll(int heroId)
    {
        var circuits = GetCircuits(heroId);
        circuits.Clear();
        // 占的格子也都清掉
        var occupied = GetOccupied(heroId);
        occupied.Clear();
        // 属性也清掉
        _circuitAttrMap[heroId].Clear();
    }

    public static List<string> GetOccupied(int heroId)
    {
        if (!_heroCircuitOccupied.TryGetValue(heroId, out var occupiedMap))
        {
            _heroCircuitOccupied[heroId] = occupiedMap = new List<string>();
        }

        return occupiedMap;
    }

    /** 这个格子是否开放了 */
    public static bool CheckStageAvailable(HeroInfo hero, HeroCircuitInfo circuit, int coordinateX, int coordinateY)
    {
        foreach (var node in circuit.Nodes)
        {
            // Debug.Log(`位置： ${node.x}, ${node.y}`);
            var x = coordinateX + node.X;
            var y = coordinateY + node.Y;

            if (!CoordinateAvailable(hero.Job, hero.CircuitStarStage, hero.CircuitLevelStage, x, y))
            {
                return false;
            }
        }

        return true;
    }

    /** 位置是否还有效 */
    public static bool CheckPlaceAvailable(int heroId, HeroCircuitInfo circuit, int coordinateX, int coordinateY)
    {
        var occupied = GetOccupied(heroId);
        
        foreach (var node in circuit.Nodes)
        {
            var x = coordinateX + node.X;
            var y = coordinateY + node.Y;
            var flag = $"{x}_{y}";

            if (occupied.Contains(flag))
            {
                Debug.LogError($"Block->{flag} has been taken, {string.Join(",", occupied)}");
                return false;
            }
        }

        return true;
    }

    /** 判断是否有部分已经有拖到外面的 */
    public static bool CheckOutside(int shape, int coordinateX, int coordinateY)
    {
        var halfH = CircuitCellExt.MapSizeH / 2;
        var halfV = CircuitCellExt.MapSizeV / 2;
        var nodes = StaticData.PuzzleShapeTable.TryGet(shape).Dots;
        foreach (var node in nodes)
        {
            var x = coordinateX + node.X;
            var y = coordinateY + node.Y;

            if (x > halfH || x < -halfH || y > halfV || y < -halfV)
                return true;
        }

        return false;
    }
    
    /** 检查坐标是否开放了 */
    public static bool CoordinateAvailable(int type, int starStage, int lvStage, int x, int y)
    {
        var arr = StaticData.PuzzleBgTable.TryGet(type).Colls;
        foreach (var row in arr)
        {
            switch ((CircuitUnlockType) row.Type)
            {
                case CircuitUnlockType.T0_Basic:
                // 如果星级小于当前值， 就是开放了
                case CircuitUnlockType.T1_Star when row.Stage <= starStage:
                // 如果等级小于当前值， 就是开放了
                case CircuitUnlockType.T2_Level when row.Stage <= lvStage:
                    break;
                default:
                    continue;
            }
            
            foreach (var rowNode in row.Dots)
            {
                if (rowNode.X == x && rowNode.Y == y) return true;
            }
        }
        
        return false;
    }

    /** 通过坐标找到circuitInfo */
    public static HeroCircuitInfo FindCircuit(int heroId, int x, int y)
    {
        if (_heroCircuits.TryGetValue(heroId, out var circuits))
        {
            foreach (var heroCircuitInfo in circuits)
            {
                var coordinate = heroCircuitInfo.Coordinate;

                foreach (var circuitNode in heroCircuitInfo.Nodes)
                {
                    var nodeX = coordinate.X + circuitNode.X;
                    var nodeY = coordinate.Y + circuitNode.Y;

                    if (nodeX == x && nodeY == y)
                    {
                        return heroCircuitInfo;
                    }
                }
            }
        }

        return null;
    }

    private static void _InitCircuits(int heroId)
    {
        if (!_heroCircuits.ContainsKey(heroId))
        {
            _heroCircuits[heroId] = new List<HeroCircuitInfo>();
            
            var list = GetList(HeroManager.Instance.GetHeroInfo(heroId));
            foreach (var info in list)
            {
                var circuit = GetCircuitInfo(info);
                if (circuit.HeroId == heroId)
                {
                    PutOn(heroId, circuit);
                }
            }
        }
    }

    private static void _OnCircuitChanged(ItemInfo itemInfo)
    {
        if (string.IsNullOrEmpty(itemInfo.UsedHero)) return;

        var heroId = HeroHelper.InstanceIdToRowId(itemInfo.UsedHero);
        // 如果该英雄拼图组还没初始化的话， 那么就不需要自动帮其装上
        if (!_heroCircuits.ContainsKey(heroId)) return;
        // 数据层放上去
        var circuit = GetCircuitInfo(itemInfo);
        PutOn(heroId, circuit);
    }
}