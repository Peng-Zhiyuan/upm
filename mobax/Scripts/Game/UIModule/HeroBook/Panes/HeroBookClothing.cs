using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public partial class HeroBookClothing : HeroBookBasePane
{
    public Image Img;
    
    private List<LibClothRow> _list;
    private Dictionary<LibClothRow, HeroBookClothingItem> _itemMap;

    private void Awake()
    {
        List_clothings.ForceRebuildList();
        List_clothings.onItemRenderAction = _RenderClothingItem;
        List_clothings.GetComponent<ItemClickBehaviour>().OnItemClick = _OnItemClick;
        _itemMap = new Dictionary<LibClothRow, HeroBookClothingItem>();
    }

    protected override void _InternalRefreshHero()
    {
        if (!_heroChanged) return;
        
        _list = StaticData.LibClothTable.TryGet(_heroInfo.HeroId).Colls;
        _itemMap.Clear();
        List_clothings.numItems = (uint) _list.Count;
        // 设置一个默认选中
        _Pick(_list[0]);
    }

    private void _RenderClothingItem(int index, Transform tf)
    {
        var clothingItem = tf.GetComponent<HeroBookClothingItem>();
        var cfg = index < _list.Count ? _list[index] : null;
        clothingItem.SetInfo(cfg);
        
        // map关联
        if (null != cfg)
        {
            _itemMap[cfg] = clothingItem;
        }
    }

    private void _OnItemClick(ClickableItem item)
    {
        if (item is HeroBookClothingItem clothingItem)
        {
            _Pick(clothingItem, true);
        }
    }

    private void _Pick(LibClothRow cfg, bool ease = false)
    {
        if (_itemMap.TryGetValue(cfg, out var item))
        {
            _Pick(item, ease);
        }
    }

    private void _Pick(HeroBookClothingItem item, bool ease = false)
    {
        if (item.Locked)
        {
            ToastManager.ShowLocalize("M4_chara_locked");
            return;
        }
        
        if (!ease)
        {
            Img_picked.transform.parent = item.transform;
            Img_picked.SetAnchoredPosition(new Vector2());
        }
        else
        {
            var picker = Img_picked.transform;
            if (picker.parent != item.transform)
            {
                picker.parent = transform;
                picker.localScale = item.transform.localScale;;
            }
            picker.DOMove(item.GetPosition(), .2f).SetEase(Ease.OutCirc).onComplete = () =>
            {
                if (picker.parent != item.transform)
                {
                    picker.parent = item.transform;
                    picker.localScale = picker.parent.localScale;
                }
            };
        }
        
        UiUtil.SetSpriteInBackground(Img, () => $"{item.Cfg.Cloth}.png");
    }
}