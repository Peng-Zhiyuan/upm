using System.Collections;
using UnityEngine;
using System.Threading.Tasks;
using System;


// 导航的最小单元
public class Page : View
{
    // TODO: 临时处理，我想 PageSack 需要重构了，以直接包含 PageContainer
    public bool ContainerActive 
    {
        get 
        {
            return this.pageContainer.gameObject.activeSelf;
        }
        set 
        {
            this.pageContainer.gameObject.SetActive(value);
        }
    }

    [HideInInspector]
    public Action OnPoped;

    public Action Poped
    {
        get
        {
            return this.OnPoped;
        }
        set
        {
            this.OnPoped = value;
        }
    }


    [HideInInspector]
    public PageContainer pageContainer;


    public PageContainer PageContainer
    {
        get
        {
            return pageContainer;
        }
        set
        {
            pageContainer = value;
        }
    }


    [Tooltip("叠加窗口，使其后面的窗口也保持活动")]
    public bool Overlay;

    public bool IsOverlay
    {
        get
        {
            return Overlay;
        }
        set
        {
            this.Overlay = value;
        }
    }

    [Tooltip("一次性页面，当出栈时不回收，而是销毁")]
    public bool isOneTime;

    public bool IsOneTime
    {
        get
        {
            return this.isOneTime;
        }
    }

    // 当被导航到这个窗口时发生
    public virtual void OnNavigatedTo (PageNavigateInfo navigateInfo) {}
    // 当从这个窗口导航到别的窗口时发生
    public virtual void OnNavigatedFrom () { }

    public virtual void OnPush () { }

    public virtual void OnPop () { }

    public virtual bool OnGlobleBack () {
        return false;
    }

    public virtual async Task OnPreper(PageNavigateInfo navigateInfo) {}

    public virtual async Task OnPopAsync() { }

    public void RecyclePageAndContainer()
    {
        if(!this.isOneTime)
        {
            this.Recycle();
        }
        else
        {
            var pageName = this.gameObject.name;
            Debug.LogError($"{pageName} is oneTime, not recyle");
            GameObject.Destroy(this.gameObject);
        }

        this.pageContainer.Recycle();
        this.pageContainer.RecycleAllFloatings();
    }

    public virtual void OnForwardTo(PageNavigateInfo navigateInfo) { }

    public virtual void OnBackTo(PageNavigateInfo navigateInfo) { }
}