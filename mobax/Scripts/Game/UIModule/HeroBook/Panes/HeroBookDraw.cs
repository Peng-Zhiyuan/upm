using System.Collections.Generic;
using DG.Tweening;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UI;

public partial class HeroBookDraw : HeroBookBasePane
{
    public PerfectCardUnit HeroSpine;
    public Image HeroImage;
    
    private List<LibPlateRow> _list;
    private Dictionary<LibPlateRow, HeroBookDrawItem> _itemMap;

    private void Awake()
    {
        List_draws.ForceRebuildList();
        List_draws.onItemRenderAction = _RenderDrawItem;
        List_draws.GetComponent<ItemClickBehaviour>().OnItemClick = _OnItemClick;
        _itemMap = new Dictionary<LibPlateRow, HeroBookDrawItem>();
    }

    protected override void _InternalRefreshHero()
    {
        // if (!_heroChanged) return;

        var cfgArr = StaticData.LibPlateTable.TryGet(_heroInfo.HeroId);
        _list = cfgArr?.Colls ?? null;
        _itemMap.Clear();
        var count = _list?.Count ?? 0;
        List_draws.numItems = (uint) count;
        // 设置一个默认选中
        Img_picked.SetActive(count > 0);
        if (count > 0)
        {
            _Pick(_list?[0]);
        }
    }

    private void _RenderDrawItem(int index, Transform tf)
    {
        var drawItem = tf.GetComponent<HeroBookDrawItem>();
        var cfg = index < _list.Count ? _list[index] : null;
        drawItem.SetInfo(_heroInfo, cfg);
        
        // map关联
        if (null != cfg)
        {
            _itemMap[cfg] = drawItem;
        }
    }

    private void _OnItemClick(ClickableItem item)
    {
        if (item is HeroBookDrawItem drawItem)
        {
            _Pick(drawItem, true);
        }
    }

    private void _Pick(LibPlateRow cfg, bool ease = false)
    {
        if (_itemMap.TryGetValue(cfg, out var item))
        {
            _Pick(item, ease);
        }
    }

    private async void _Pick(HeroBookDrawItem item, bool ease = false)
    {
        if (!HeroBookHelper.CheckUnlock(_heroInfo, item.Cfg, out var lockMessage))
        {
            ToastManager.Show(lockMessage);
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

        var displayType = (HeroBookDisplayType) item.Cfg.displayAs;
        HeroSpine.SetActive(displayType == HeroBookDisplayType.T1_Spine);
        HeroImage.SetActive(displayType == HeroBookDisplayType.T2_Image);
        switch (displayType)
        {
            case HeroBookDisplayType.T1_Spine:
                await HeroSpine.RefreshHeroCard(_heroInfo.HeroId);
                break;
            case HeroBookDisplayType.T2_Image:
                UiUtil.SetSpriteInBackground(HeroImage, () => $"{item.Cfg.Plate}.jpg");
                break;
        }
    }
}

internal enum HeroBookDisplayType
{
    T1_Spine = 1,
    T2_Image
}