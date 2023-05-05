using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public class PageContainer : RecycledGameObject
{
    public RectTransform pageRoot;
    public RectTransform floatingRoot;

    public bool isOverlay;
    public string pageName;
    public string pageTag;

    private Page _page;
    public Page Page
    {
        set
        {
            this._page = value;
            if(value != null)
            {
                value.container = this;
                value.rectTransform.SetParent(this.pageRoot, false);
                this.pageName = value.name;
                this.isOverlay = value.Overlay;
                this.pageTag = value.pageTag;
                this.name = "PageContainer - " + pageName;
            }
        }
        get
        {
            return _page;
        }
    }

    public bool IsLoaded
    {
        get
        {
            var p = this.Page;
            var ret = p != null;
            return ret;
        }
    }

    public bool Active
    {
        get
        {
            return this.gameObject.activeSelf;
        }
        set
        {
            this.gameObject.SetActive(value);
        }
    }
    public void AddFloating(Floating floating)
    {
        floating.transform.SetParent(this.floatingRoot, false);
    }

    public void ReleaseAllFloatings()
    {
        var childCount = this.floatingRoot.childCount;
        for (var i = childCount - 1; i >= 0; i--)
        {
            var transform = this.floatingRoot.GetChild(i);
            var floating = transform.GetComponent<Floating>();
            if (floating != null)
            {
                UIEngine.Stuff.MoveFloatingLayer(floating, null);
            }
            else
            {
                throw new System.Exception($"{transform.name} do not have Floating.");
            }
        }
    }

    public void DestoryPage()
    {
        if(this.Page != null)
        {
            GameObject.DestroyImmediate(this.Page.gameObject);
            BucketManager.Stuff.Main.Release($"{this.pageName}.prefab");
            MonoBehaviour.Destroy(this.Page);
            this.Page = null;
        }
    }

    async Task RecreatePageAsync()
    {
        if(this.Page == null)
        {
            var page = await UIEngine.ReusePageAsync(this.pageName);
            this.Page = page;
        }
    }

    public void DestoryPageIfNeed()
    {
        if(this.Page?.optimization == PageOptimization.DestroyOnDisabled)
        {
            if(!this.Active)
            {
                this.DestoryPage();
            }
        }
    }

    public async Task RecreatePageIfNeedAsync()
    {
        if (this.Page == null)
        {
            if (this.Active)
            {
                await this.RecreatePageAsync();
            }
        }
    }
}
