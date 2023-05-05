using System;
using System.Collections.Generic;
using System.Linq;

/** 排序方式 */
public enum HeroSortMode
{
    HeroId = 1,
    Level,  // 等级
    Star,   // 星级
    Rarity, // 稀有度
    Power, // 战力
}

public class HeroFilter
{
    public delegate List<ItemInfo> ListProvide();
    
    /** 职业过滤 */
    private List<int> _jobFilters;
    /** 属性过滤 */
    private List<int> _elemFilters;
    /** 拼图对应的颜色过滤 */
    private List<int> _circuitFilters;
    /** 排除列表 */
    private List<HeroInfo> _exceptions;
    /** 排序方式 */
    private HeroSortMode _sortMode;
    /** 升降序 */
    private SortOrderEnum _sortOrder;
    /** 当前展示英雄 */
    private HeroInfo _displayHero;
    /** 展示列表 */
    private List<HeroInfo> _displayList;
    /** 列表改变时候的操作 */
    private Action _onListChanged;
    /** 排序对应map */
    private Dictionary<HeroSortMode, string> _sortKeyMap;
    /** 默认排序 */
    private HeroSortMode[] _defaultCompares;
    /** 当前排序缓存 */
    private HeroSortMode[] _currentCompares;
    /** 列表 */
    private ListProvide _listProvider;

    public SortOrderEnum SortOrder => _sortOrder;
    public HeroFilter()
    {
        _sortKeyMap = new Dictionary<HeroSortMode, string>
        {
            [HeroSortMode.Level] = "Level",
            [HeroSortMode.Star] = "StarNumber",
            [HeroSortMode.Rarity] = "Rarity",
            [HeroSortMode.HeroId] = "HeroId",
            [HeroSortMode.Power] = "Power",
        };
        _defaultCompares = new[]
        {
            HeroSortMode.Power,
            HeroSortMode.Rarity,
            HeroSortMode.Star,
            HeroSortMode.Level,
            HeroSortMode.HeroId
        };
        _currentCompares = new HeroSortMode[_defaultCompares.Length];
        _sortOrder = SortOrderEnum.Descend;
    }
    
    public List<ItemInfo> SourceList => _listProvider?.Invoke() ?? HeroManager.Instance.AvailList;

    /** 当前展示的英雄 */
    public HeroInfo DisplayHero => _displayHero;
    /** 当前列表（过滤或者排序后） */
    public List<HeroInfo> DisplayList
    {
        get
        {
            if (null == _displayList)
            {
                ResetList();
            }
            else if (_displayList.Count != SourceList.Count)
            {
                RefreshList();
            }

            return _displayList;
        }
    }

    public HeroInfo DefaultHero => DisplayList.First();

    public void SetListProvider(ListProvide provider)
    {
        _listProvider = provider;
    }
    
    public void ResetFilter()
    {
        SetFilter(null, null);
    }
    
    public void SetFilter(List<int> jobFilters, List<int> elemFilters)
    {
        _jobFilters = jobFilters;
        _elemFilters = elemFilters;
    }

    public void ResetCircuitFilter()
    {
        SetCircuitFilter(null);
    }
    
    public void SetCircuitFilter(List<int> circuitFilters)
    {
        _circuitFilters = circuitFilters;
    }

    public void SetException(HeroInfo heroInfo)
    {
        _exceptions ??= new List<HeroInfo>();
        _exceptions.Add(heroInfo);
    }

    /// <summary>
    /// 判断是否符合过滤条件
    /// </summary>
    /// <returns></returns>
    public bool MatchFilter(HeroInfo heroInfo)
    {
        // if (!heroInfo.Show) return false;
        if (null != _exceptions && _exceptions.Contains(heroInfo)) return false;

        if (_jobFilters?.Count > 0)
        {
            if (_jobFilters.IndexOf(heroInfo.Job) < 0) return false;
        }

        if (_elemFilters?.Count > 0)
        {
            if (_elemFilters.IndexOf(heroInfo.Conf.Element) < 0) return false;
        }

        if (_circuitFilters?.Count > 0)
        {
            if (!_circuitFilters.Exists(colorType => heroInfo.Conf.Puzzles.Exists( 
                circuitInfo => circuitInfo.Id != 0 && HeroCircuitHelper.GetSkillColor(circuitInfo.Id) == colorType)))
                return false;
        }

        return true;
    }

    /** 设置sort信息 */
    public void SetSort(HeroSortMode sortMode)
    {
        _sortMode = sortMode;
    }
    
    public void RefreshList()
    {
        if (_sortMode == default)
        {
            throw new Exception("Sort mode must be set before list refreshed");
        }
        
        if (null == _displayList)
        {
            _displayList = new List<HeroInfo>();
        }
        else
        {
            _displayList.Clear();
        }

        SourceList.ForEach(itemInfo =>
        {
            var heroInfo = HeroManager.Instance.GetHeroInfo(itemInfo.id);
            if (MatchFilter(heroInfo))
            {
                _displayList.Add(heroInfo);
            }
        });

        ResortList();
    }

    /// <summary>
    /// 对list重新排序
    /// </summary>
    public void ResortList()
    {
        // 重新排序
        if (_currentCompares.First() != _sortMode)
        {
            var find = false;
            for (var i = 0; i < _defaultCompares.Length; i++)
            {
                var compare = _defaultCompares[i];
                if (find)
                {
                    _currentCompares[i] = _defaultCompares[i];
                }
                else if (i == 0)
                {
                    _currentCompares[i] = _sortMode;
                }
                else
                {
                    _currentCompares[i] = _defaultCompares[i - 1];

                    if (compare == _sortMode)
                    {
                        find = true;
                    }
                }
            }
        }

        _InternalRefreshList(_displayList);
    }
    
    /// <summary>
    /// 列表逆序
    /// </summary>
    public void ReverseList()
    {
        switch (_sortOrder)
        {
            case SortOrderEnum.Ascend:
                _sortOrder = SortOrderEnum.Descend;
                break;
            case SortOrderEnum.Descend:
                _sortOrder = SortOrderEnum.Ascend;
                break;
        }

        _InternalRefreshList(DisplayList);
    }

    /** 重置 */
    public void ResetList()
    {
        _sortOrder = SortOrderEnum.Descend;
        _sortMode = HeroSortMode.Level;
        ResetFilter();
        RefreshList();
    }
    
    // 设置当前显示的英雄
    public void SetHero(HeroInfo heroInfo)
    {
        _displayHero = heroInfo;
    }

    /** 取得偏移offset位置的英雄 */
    public HeroInfo GetHeroByOffset(int offset)
    {
        var displayIndex = DisplayList.IndexOf(_displayHero);
        var newIndex = (displayIndex + offset + DisplayList.Count) % DisplayList.Count;
        return DisplayList[newIndex];
    }
    
    /** 翻页到偏移offset位置的英雄 */
    public void TurnTo(int offset)
    {
        _displayHero = GetHeroByOffset(offset);
    }

    public void SetHeroByOffsetWithSkinCheck(int offset)
    {
        var displayIndex = DisplayList.IndexOf(_displayHero);
        HeroInfo hero;
        do
        {
             displayIndex = (displayIndex + offset + DisplayList.Count) % DisplayList.Count;
             hero = DisplayList[displayIndex];
        }
        while (!HeroDressData.Instance.HasSkin(hero.HeroId));
        _displayHero = hero;
    }

    private int _Compare(HeroInfo heroInfo1, HeroInfo heroInfo2, HeroSortMode[] compares)
    {
        // 先看是否是收藏的，收藏的永远都是优先的
        if (heroInfo1.LikeValue != heroInfo2.LikeValue)
        {
            return heroInfo1.LikeValue < heroInfo2.LikeValue ? 1 : -1;
        }

        foreach (var currentCompare in compares)
        {
            var compareResult = _CompareWith(heroInfo1, heroInfo2, currentCompare);
            if (compareResult != 0)
            {
                return compareResult;
            }
        }

        return 0;
    }

    private int _CompareWith(HeroInfo heroInfo1, HeroInfo heroInfo2, HeroSortMode compareSeed)
    {
        var key = _sortKeyMap[compareSeed];

        var val1 = _GetAttribute(heroInfo1, key);
        var val2 = _GetAttribute(heroInfo2, key);
        var val = val1 == val2 ? 0 : val1 > val2 ? 1 : -1;
        if (_sortOrder == SortOrderEnum.Descend)
        {
            val *= -1;
        }

        return val;
    }
    
    /** 排序内在逻辑 */
    private void _InternalRefreshList(List<HeroInfo> list)
    {
        list.Sort((h1, h2) => _Compare(h1, h2, _currentCompares));
        
        _onListChanged?.Invoke();
    }

    private int _GetAttribute(HeroInfo heroInfo, string attr)
    {
        var propertyInfo = heroInfo.GetType().GetProperty(attr);
        if (null == propertyInfo) return 0;
        
        var val = (int) propertyInfo.GetValue(heroInfo);
        return val;
    }
}