using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.Utilities;
using UnityEngine.UI;

public class CircuitFilter
{
    #region 静态内容部分
    public static List<GeneralSortItem> GeneralSorts { get;}
    public static List<GeneralFilterItem> GeneralFilters { get;}
    
    static CircuitFilter()
    {
        GeneralSorts = new List<GeneralSortItem>
        {
            new GeneralSortItem {SortDelegate = _GetPower, Description = "common_words_power"}, // 战斗力
            new GeneralSortItem {SortDelegate = _GetLevel, Description = "M4_circuit_sort_level"}, // 回路等级
            new GeneralSortItem {SortDelegate = _GetHpScore, Description = "M4_circuit_sort_hp"}, // 血量加成
            new GeneralSortItem {SortDelegate = _GetAttackScore, Description = "M4_circuit_sort_attack"}, // 攻击加成
            new GeneralSortItem {SortDelegate = _GetDefenceScore, Description = "M4_circuit_sort_defence"}, // 防御加成
        };
        
        GeneralFilters = new List<GeneralFilterItem>
        {
            new GeneralFilterItem {FilterDelegate = _FilterAll, Description = "M4_circuit_filter_all"}, // ALL
            new GeneralFilterItem {FilterDelegate = _FilterAttack, Description = "M4_circuit_filter_attack"}, // 攻击类
            new GeneralFilterItem {FilterDelegate = _FilterDefence, Description = "M4_circuit_filter_defence"}, // 防御类
            new GeneralFilterItem {FilterDelegate = _FilterAssist, Description = "M4_circuit_filter_assist"}, // 辅助类
            new GeneralFilterItem {FilterDelegate = _FilterCure, Description = "M4_circuit_filter_cure"}, // 治疗类
            // new GeneralFilterItem {Key = "order_control", Description = "控制类"}, // 战斗力
        };
    }

    private static int _GetPower(HeroCircuitInfo circuit)
    {
        return HeroCircuitHelper.GetPower(circuit);
    }

    private static int _GetLevel(HeroCircuitInfo circuit)
    {
        return circuit.Level;
    }

    private static int _GetHpScore(HeroCircuitInfo circuit)
    {
        return _GetAttrScore(circuit, HeroAttr.HP) + _GetAttrScore(circuit, HeroAttr.HPUP);
    }

    private static int _GetAttackScore(HeroCircuitInfo circuit)
    {
        return _GetAttrScore(circuit, HeroAttr.ATK) + _GetAttrScore(circuit, HeroAttr.ATKUP);
    }

    private static int _GetDefenceScore(HeroCircuitInfo circuit)
    {
        return _GetAttrScore(circuit, HeroAttr.DEF) + _GetAttrScore(circuit, HeroAttr.DEFUP);
    }

    private static int _GetAttrScore(HeroCircuitInfo circuit, HeroAttr attr)
    {
        var val = HeroCircuitHelper.GetAttr(circuit, attr);
        var cfg = StaticData.HeroAttrTable.TryGet((int) attr);
        return val * cfg.Power;
    }

    private static bool _FilterAll(HeroCircuitInfo circuit)
    {
        return true;
    }

    private static bool _FilterAttack(HeroCircuitInfo circuit)
    {
        return circuit.Color == (int) CircuitColorEnum.C1_Attack;
    }

    private static bool _FilterDefence(HeroCircuitInfo circuit)
    {
        return circuit.Color == (int) CircuitColorEnum.C2_Defence;
    }

    private static bool _FilterAssist(HeroCircuitInfo circuit)
    {
        return circuit.Color == (int) CircuitColorEnum.C3_Assist;
    }

    private static bool _FilterCure(HeroCircuitInfo circuit)
    {
        return circuit.Color == (int) CircuitColorEnum.C4_Cure;
    }
    #endregion
    
    /** 排序方式 */
    private string _sortMode;
    /** 列表的数据源 */
    private List<HeroCircuitInfo> _rawList;
    /** 过滤后的列表 */
    private List<HeroCircuitInfo> _filteredList;

    public GeneralSortItem CurrentSort { get; private set; }
    public GeneralFilterItem CurrentFilter { get; private set; }
    public SortOrderEnum CurrentOrder { get; private set; } = SortOrderEnum.Descend;
    public bool FilterUnequipped { get; set; }
    public List<HeroCircuitInfo> List => _filteredList;
    public Action OnListChanged;

    public CircuitFilter()
    {
        CurrentSort = GeneralSorts.First();
        CurrentFilter = GeneralFilters.First();
    }
    
    public void ResetList(List<HeroCircuitInfo> list)
    {
        _rawList = list;
    }

    public void Refresh()
    {
        SetFilter(CurrentFilter);
    }

    public void SwitchOrder()
    {
        var order = (SortOrderEnum) (((int) CurrentOrder + 1) % 2);
        SetOrder(order);
    }

    public void SetOrder(SortOrderEnum order)
    {
        CurrentOrder = order;
        _InternalSetSort(CurrentSort);
        OnListChanged?.Invoke();
    }

    public void SetSort(GeneralSortItem item)
    {
        CurrentSort = item;
        _InternalSetSort(item);
        OnListChanged?.Invoke();
    }

    public void SetFilter(GeneralFilterItem item)
    {
        CurrentFilter = item;
        _InternalSetFilter(item);
        _InternalSetSort(CurrentSort);
        OnListChanged?.Invoke();
    }

    public void SetFilterUnequipped(bool val)
    {
        FilterUnequipped = val;
        _InternalSetFilter(CurrentFilter);
        _InternalSetSort(CurrentSort);
        OnListChanged?.Invoke();
    }

    private void _InternalSetSort(GeneralSortItem item)
    {
        _filteredList.Sort((circuit1, circuit2) =>
        {
            var val1 = item.SortDelegate(circuit1);
            var val2 = item.SortDelegate(circuit2);
            int judge;
            if (val1 == val2)
            {
                judge = string.Compare(circuit1.InstanceId, circuit2.InstanceId, StringComparison.Ordinal);
            }
            else
            {
                judge = val1 > val2 ? 1 : -1;
            }
            if (CurrentOrder == SortOrderEnum.Descend)
            {
                judge *= -1;
            }

            return judge;
        });
    }

    private void _InternalSetFilter(GeneralFilterItem item)
    {
        _filteredList = new List<HeroCircuitInfo>();
        foreach (var circuit in _rawList)
        {
            if (FilterUnequipped && circuit.ItemInfo.IsUsed) continue;
            
            if (item.FilterDelegate(circuit))
            {
                _filteredList.Add(circuit);
            }
        }
    }
}

public delegate int CircuitSortDelegate(HeroCircuitInfo circuit);
public delegate bool CircuitFilterDelegate(HeroCircuitInfo circuit);

public class BaseFilterInfo
{
    public string Description;
}

public class ExtraFilterItem : BaseFilterInfo
{
    public Action<ClickBehaviour> FilterAction;
}

public class GeneralFilterItem : BaseFilterInfo
{
    public CircuitFilterDelegate FilterDelegate;
}

public class GeneralSortItem : BaseFilterInfo
{
    public CircuitSortDelegate SortDelegate;
}