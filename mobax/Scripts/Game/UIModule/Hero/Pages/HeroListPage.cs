using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;
public partial class HeroListPage : Page
{
    public MutexComponent TabPrefab;

    private HeroSortInfo[] _sortList;
    private List<int> _jobFilters;
    private List<HeroInfo> _list;
    private Dictionary<int, HeroListItem> _cardMap;

    public override void OnNavigatedTo(PageNavigateInfo navigateInfo)
    {
        _RefreshListView();
    }

    public override void OnButton(string msg)
    {
        switch (msg)
        {
            case "handbook":
                // UIEngine.Stuff.Forward<HeroBookEntrancePage>();
                ToastManager.ShowLocalize("common_lockTip");
                break;
            case "communicate":
                ToastManager.ShowLocalize("common_lockTip");
                break;
        }
    }

    /// <summary>
    /// 根据heroId取得cell
    /// </summary>
    /// <param name="heroId"></param>
    /// <returns></returns>
    public HeroListItem GetCell(int heroId)
    {
        if (_cardMap.TryGetValue(heroId, out var cell))
        {
            if (cell.HeroInfo.HeroId == heroId)
            {
                return cell;
            }
        }

        return null;
    }

    public void OnSortOptionChanged()
    {
        var index = Dropdown_sort.value;
        HeroDisplayer.Filter.SetSort(_sortList[index].Mode);
        HeroDisplayer.Filter.ResortList();
        _RefreshListView();
    }

    public void OnOrderChanged()
    {
        HeroDisplayer.Filter.ReverseList();
        _RefreshListView();
        _RefreshOrderView();
    }
    
    private void Awake()
    {
        // 初始化下拉框
        _sortList = new[]
        {
            new HeroSortInfo { Mode = HeroSortMode.Power, Content = "m3_formationSortCombat" },
            new HeroSortInfo { Mode = HeroSortMode.Level, Content = "m3_formationSortLv" },
            new HeroSortInfo { Mode = HeroSortMode.Rarity, Content = "m3_formationSortQuality" },
            new HeroSortInfo { Mode = HeroSortMode.Star, Content = "m3_formationSortStar" },
        };
        var options = new List<Dropdown.OptionData>();
        foreach (var info in _sortList)
        {
            options.Add(new Dropdown.OptionData {text = info.Content.Localize()});
        }
        Dropdown_sort.options = options;
        // list初始化
        //List_heroes.onItemRenderAction = _OnHeroRender;
        this.ListView_hero.ViewCreated += ListView_OnViewCreated;
        // 数据初始化
        _list = HeroDisplayer.List;
        // 默认排序设置一个
        HeroDisplayer.Filter.SetSort(_sortList.First().Mode);
        // 然后构建好tab
        var tabs = Tabs_jobs.tabs = new Dictionary<MutexComponent, int>();
        tabs[TabPrefab] = 0; // 0表示全部
        foreach (HeroJob job in Enum.GetValues(typeof(HeroJob)))
        {
            var tab = Instantiate(TabPrefab, TabPrefab.transform.parent);
            tab.name = $"Tab_{job}";
            tabs[tab] = (int) job;
            
            // 设置图标
            var icon = tab.transform.Find("icon").GetComponent<Image>();
            icon.rectTransform.sizeDelta = new Vector2(80, 80);
            UiUtil.SetSpriteInBackground(icon, () => HeroHelper.GetJobAddress(job));
        }
        Tabs_jobs.Reset();
        Tabs_jobs.OnSelect = _OnTabChanged;
        // 默认选中全部
        Tabs_jobs.SetSelect(0);
        // 刷新view
        _RefreshOrderView();

        //this.List_heroes.ListRebuilt += List_heroes_ListRebuilt;
    }

    //private void List_heroes_ListRebuilt()
    //{
    //    this.PlayElementAnimation();
    //}

    private void _OnTabChanged(int job)
    {
        if (job != 0)
        {
            if (null == _jobFilters)
            {
                _jobFilters = new List<int>();
            }
            else
            {
                _jobFilters.Clear();
            }
            
            _jobFilters.Add(job);
        }

        var jobFilters = job == 0 ? default : _jobFilters;
        HeroDisplayer.Filter.SetFilter(jobFilters, null);
        HeroDisplayer.Filter.RefreshList();
        _RefreshListView();
    }

    private void _RefreshListView()
    {
        _ResetCellMap();
        //List_heroes.numItems = (uint) _list.Count;

        this.ListView_hero.Reset(this._list);
        //this.PlayElementAnimation();
    }

    //CancellationTokenSource cts;
    //void PlayElementAnimation()
    //{
    //    //var list = BadScrollViewUtil.GetViewList(this.List_heroes);

    //    //cts?.Cancel();
    //    //this.cts = new CancellationTokenSource();
    //    //CodeAnimation.PlayApearSequencely(list, cts.Token);
    //    CodeAnimation.PlayElementAnimation(this.List_heroes, ref this.cts);
    //}

    private void _RefreshOrderView()
    {
        Mutex_order.Selected = HeroDisplayer.Filter.SortOrder == SortOrderEnum.Descend;
    }

    private void _ResetCellMap()
    {
        if (null == _cardMap)
        {
            _cardMap = new Dictionary<int, HeroListItem>();
        }
        else
        {
            _cardMap.Clear();
        }
    }

    private void ListView_OnViewCreated(RectTransform tf, object data)
    {
        var heroInfo = data as HeroInfo;
        var cell = tf.GetComponent<HeroListItem>();
        cell.SetInfo(heroInfo, _OnHeroClick);
        _cardMap[heroInfo.HeroId] = cell;
    }

    private void _OnHeroClick(HeroInfo heroInfo)
    {
        UIEngine.Stuff.ForwardOrBackTo<HeroPage>(heroInfo);
    }

    protected override async Task LogicBackAsync()
    {
        await UiUtil.BackToMainGroupThenReplaceAsync<MainPage>();
    }

}

internal struct HeroSortInfo
{
    public HeroSortMode Mode;
    public string Content;
}