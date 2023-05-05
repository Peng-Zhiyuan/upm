
using System;
using System.Collections.Generic;
using UnityEngine;

public partial class DrawEliminationResultFloating : Floating
{
    public override void OnShow(object param)
    {
        if (param is DrawEliminationResultParam p)
        {
            Txt_judgement.text = p.judge;
            Txt_passTime.text = $"{p.costTime / 1000}s";

            Node_prize.Selected = p.rewards != null;
            if (p.rewards != null)
            {
                List_items.DataList = p.rewards;
            }
        }
    }

    private void Awake()
    {
        List_items.ViewSetter = _OnRenderItem;
    }

    private void _OnRenderItem(object data, Transform tf)
    {
        var itemInfo = data as ItemInfo;
        var view = tf.GetComponent<ItemView>();
        view.Set(itemInfo);
    }
    
    public void OnConfirm()
    {
        Remove();
        UIEngine.Stuff.Back();
    }
}

public class DrawEliminationResultParam
{
    public string judge;
    public int costTime;
    public List<ItemInfo> rewards;
}