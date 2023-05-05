using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using System;
using Sirenix.OdinInspector;

public partial class ItemView : MonoBehaviour
{
    [ShowInInspector, ReadOnly]
    public ItemViewData data = new ItemViewData();
    public bool isClickable;
    public bool isPressable;

    void Awake()
    {
        this.IsGotMarkVisible = false;
    }

    public bool IsGotMarkVisible
    {
        set
        {
            this.Group_gotMark.gameObject.SetActive(value);
        }
    }
    public void Set(CommonItem commonItem)
    {
        this.data.SetAsCommonItem(commonItem);
        this.Refresh();
    }
    public void Set(ItemInfo itemInfo)
    {
        this.data.SetAsItemInfo(itemInfo);
        this.Refresh();
    }

    public void Set(VirtualItem virtualItem)
    {
        this.data.SetAsVirtualItem(virtualItem);
        this.Refresh();
    }

    public void Set(int rowId, int count)
    {
        var vi = new VirtualItem();
        vi.id = rowId;
        vi.val = count;
        this.Set(vi);
    }

    public void Set(int rowId)
    {
        this.data.SetAsRowId(rowId);
        this.Refresh();
    }

    public void Refresh()
    {
        this.RefreshSubView();
        this.RefreshDebugId();
    }
    
    void RefreshSubView()
    {
        var itype = this.data.IType;
        if (itype == IType.Avatar)
        {
            Root.SetSelected(ItemView_Skin);
            ItemView_Skin.Bind(this.data);
        }
        else if (itype == IType.Puzzle)
        {
            Root.SetSelected(ItemView_Circuit);
            if (null == data.InstanceId)
            {
                ItemView_Circuit.SetCircuitId(data.RowId);
            }
            else
            {
                ItemView_Circuit.SetInfo(HeroCircuitManager.GetCircuitInfo(data.InstanceId));
            }
        }
        else
        {
            Root.SetSelected(ItemView_Normal);
            this.ItemView_Normal.Bind(this.data);
        }
    }


    void RefreshDebugId()
    {
        var isDev = DeveloperLocalSettings.IsDevelopmentMode;
        var isZ = DeveloperLocalSettings.IsStatusOpenInDevMoe;
        if (isDev && isZ)
        {
            this.Text_debugId.text = "id: " + this.data.RowId.ToString();
            this.Group_debug.gameObject.SetActive(true);
        }
        else
        {
            this.Group_debug.gameObject.SetActive(false);
        }

    }

    public async void ShowDetailPage()
    {
        if (data.IType == IType.Puzzle)
        {
            // 显示tips
            if (null != data.InstanceId)
            {
                UIEngine.Stuff.ShowFloating<CircuitInfoFloating>(data.InstanceId);
            }
            else
            {
                UIEngine.Stuff.ShowFloating<CircuitInfoFloating>(data.RowId);
            }
        }
        else
        {
            var rowId = this.data.RowId;
            var count = this.data.Count;
            //var f = UIEngine.Stuff.ShowFloatingImediatly<ItemTipFloating>();
            //f.Bind(dataId);
            //var pos = this.transform.position;
            //f.SetPosition(pos);
            var f = await UIEngine.Stuff.ShowFloatingAsync<ItemInfoFloating>();
            f.Set(rowId, count);
        }
    }

    public Func<ItemView, bool> clickHandlerHook;

    public void OnButton(string msg)
    {
        if (msg == "click")
        {
            if(isClickable)
            {
                bool isProcessed = false;
                if(clickHandlerHook != null)
                {
                    isProcessed = clickHandlerHook.Invoke(this);
                }

                if(!isProcessed)
                {
                    this.ShowDetailPage();
                }
            }
        }
    }

    public void OnLongPress()
    {
        if (this.isPressable)
        {
            this.ShowDetailPage();
        }
    }
}
