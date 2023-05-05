using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public partial class HeroBookEmotion : HeroBookBasePane
{
    public UISpineUnit HeroSpine;
    
    private List<LibEmoRow> _list;
    private Dictionary<LibEmoRow, HeroBookEmotionItem> _itemMap;

    private void Awake()
    {
        List_emotions.onItemRenderAction = _RenderEmotionItem;
        List_emotions.GetComponent<ItemClickBehaviour>().OnItemClick = _OnItemClick;
        _itemMap = new Dictionary<LibEmoRow, HeroBookEmotionItem>();
    }

    protected override void _InternalRefreshHero()
    {
        if (!_heroChanged) return;
        
        var heroId = _heroInfo.HeroId;
        // 设置list
        _list = StaticData.LibEmoTable.TryGet(heroId).Colls;
        _itemMap.Clear();
        List_emotions.numItems = (uint) _list.Count;
    }

    private void _RenderEmotionItem(int index, Transform tf)
    {
        var emotionItem = tf.GetComponent<HeroBookEmotionItem>();
        var cfg = index < _list.Count ? _list[index] : null;
        emotionItem.SetInfo(cfg);
        
        // map关联
        if (null != cfg)
        {
            _itemMap[cfg] = emotionItem;
        }
    }

    private void _OnItemClick(ClickableItem item)
    {
        if (item is HeroBookEmotionItem emotionItem)
        {
            // 展示表情
            HeroSpine.ShowAnimation(emotionItem.Cfg.Face);
        }
    }
}