using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine.UI;

public partial class HeroBookEntrancePage : Page
{
    public ClickBehaviour tabPrefab;
    
    private List<ClickBehaviour> _tabs;
    private MutexComponent _currentTab;
    
    public override void OnNavigatedTo(PageNavigateInfo navigateInfo)
    {
    }
    
    public override void OnButton(string msg)
    {
        switch (msg)
        {
            case "back":
                UIEngine.Stuff.Back();
                break;
        }
    }

    private void Awake()
    {
        var groups = HeroBookManager.Instance.Groups;
        
        // Tabs准备好
        _tabs = new List<ClickBehaviour>(groups.Count);
        for (var index = 0; index < groups.Count; index++)
        {
            var clicker = Instantiate(tabPrefab, Node_tabs.transform);
            clicker.onClick = _ClickTab;
            _tabs.Add(clicker);
            // 初始化toggle
            var toggle = clicker.GetComponent<MutexComponent>();
            toggle.Selected = false;
            // 初始化文字
            var onTxt = clicker.transform.Find("Txt_on").GetComponent<Text>();
            var offTxt = clicker.transform.Find("Txt_off").GetComponent<Text>();
            onTxt.text = offTxt.text = LocalizationManager.Stuff.GetText("M4_herobook_word_tab", index + 1);
        }

        // scroll view的初始化
        ScrollView.OnSelected = _OnTabSelected;
        ScrollView.UpdateData(groups);
        ScrollView.SelectCell(0);
    }

    private void _ClickTab(ClickBehaviour behaviour)
    {
        var index = _tabs.IndexOf(behaviour);
        ScrollView.SelectCell(index);
    }

    private void _OnTabSelected(int index)
    {
        var tabClicker = _tabs[index];
        var tab = tabClicker.GetComponent<MutexComponent>();
        tab.Selected = true;
        if (null != _currentTab)
        {
            _currentTab.Selected = false;
            Slider_tab.transform.DOMoveX(tab.transform.position.x, .2f);
        }
        // 设置owner文本
        var groups = HeroBookManager.Instance.Groups;
        var list = groups[index].HeroList;
        var ownedCount = list.Count(item => HeroManager.Instance.GetHeroInfo(item.Roleid).Unlocked);
        Txt_owned.text = $"{ownedCount}/{list.Count}";
        
        _currentTab = tab;
    }
}