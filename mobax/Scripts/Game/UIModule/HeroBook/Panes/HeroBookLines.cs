using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public partial class HeroBookLines : HeroBookBasePane
{
    public UISpineUnit HeroSpine;
    
    private List<LibDialogRow> _list;
    private Dictionary<LibDialogRow, HeroBookLineItem> _itemMap;
    private HeroBookLineItem _currentItem;

    private void Awake()
    {
        List_lines.onItemRenderAction = _RenderLineItem;
        List_lines.GetComponent<ItemClickBehaviour>().OnItemClick = _OnItemClick;
        _itemMap = new Dictionary<LibDialogRow, HeroBookLineItem>();
        
        TypeWriter_line.OnTextAniComplete(() =>
        {
            _currentItem.StopTalking();
        });
    }

    protected override void _InternalRefreshHero()
    {
        if (!_heroChanged) return;
        
        _list = StaticData.LibDialogTable.TryGet(_heroInfo.HeroId).Colls;
        _itemMap.Clear();
        List_lines.numItems = (uint) _list.Count;
        Img_picked.SetActive(false);
        Node_line.SetActive(false);
    }

    private void _RenderLineItem(int index, Transform tf)
    {
        var lineItem = tf.GetComponent<HeroBookLineItem>();
        var cfg = index < _list.Count ? _list[index] : null;
        lineItem.SetInfo(_heroInfo, cfg);
        
        // map关联
        if (null != cfg)
        {
            _itemMap[cfg] = lineItem;
        }
    }

    private void _OnItemClick(ClickableItem item)
    {
        if (item is HeroBookLineItem lineItem)
        {
            _Pick(lineItem, true);
        }
    }

    private void _Pick(HeroBookLineItem item, bool ease = false)
    {
        if (!HeroBookHelper.CheckBreakUnlock(_heroInfo, item.Cfg.Unlock, out var lockMessage))
        {
            ToastManager.Show(lockMessage);
            return;
        }
        
        if (_currentItem == item)
        {
            _Talk(item.Cfg.Dialog);
            return;
        }
        
        if (null != _currentItem)
        {
            _currentItem.SetTalking(false);
        }
        
        if (!Img_picked.gameObject.activeSelf)
        {
            ease = false;
            Img_picked.SetActive(true);
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

        _currentItem = item;
        item.SetTalking(true);
        _Talk(item.Cfg.Dialog);
    }

    private void _Talk(string content)
    {
        Node_line.SetActive(true);
        TypeWriter_line.StopAni();
        TypeWriter_line.GetComponent<Text>().text = "";
        TypeWriter_line.PlayAni(content.Localize(), .02f, 1);
        var cfg = _currentItem.Cfg;
        // 展示表情
        if (!string.IsNullOrEmpty(cfg.Face))
        {
            HeroSpine.ShowAnimation(cfg.Face);
        }
        // 展示声音
        if (!string.IsNullOrEmpty(cfg.Cv))
        {
            WwiseEventManager.SendEvent(TransformTable.Voice, cfg.Cv);
        }
        // 结束时候恢复状态
        TypeWriter_line.OnTextAniComplete(() =>
        {
            _currentItem.StopTalking();
            HeroSpine.ShowIdle();
        });

    }
}