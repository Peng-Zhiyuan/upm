using System;
using System.Collections.Generic;
using UnityEngine;

public partial class HeroAttrsPopLegacy : Page
{
    private HeroInfo _heroInfo;
    private List<HeroAttrRow> _showList;
    
    public override void OnNavigatedTo(PageNavigateInfo navigateInfo)
    {
        if (navigateInfo.param is HeroInfo heroInfo)
        {
            _heroInfo = heroInfo;
            List_atts.numItems = (uint) _showList.Count;
        }
    }

    public void Quit()
    {
        UIEngine.Stuff.Back();
    }

    private void Awake()
    {
        List_atts.onItemRenderAction = _OnAttrsRender;
        _showList = StaticData.HeroAttrTable.ElementList.FindAll(item => item.Show != 0 && item.Ralated == 0);
    }

    private void _OnAttrsRender(int index, Transform tf)
    {
        var item = tf.GetComponent<HeroAttrDetailItem>();
        item.SetInfo(_heroInfo, _showList[index], index);
    }
}