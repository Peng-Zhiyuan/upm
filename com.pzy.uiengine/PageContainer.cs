using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PageContainer : RecycledGameObject
{
    public RectTransform pageRoot;
    public RectTransform floatingRoot;
    
    private Page _page;
    public Page Page
    {
        set
        {
            this._page = value;
            value.PageContainer = this;
            value.rectTransform.SetParent(this.pageRoot, false);
        }
    }

    public void AddFloating(Floating floating)
    {
        floating.transform.SetParent(this.floatingRoot, false);
    }

    public void RecycleAllFloatings()
    {
        var childCount = this.floatingRoot.childCount;
        for(var i = childCount - 1; i >=0; i--)
        {
            var transform = this.floatingRoot.GetChild(i);
            var floating = transform.GetComponent<Floating>();
            if (floating != null)
            {
                floating.Recycle();
            }
            else
            {
                throw new System.Exception($"{transform.name} Do not have Floating.");
            }
        }
        
    }
}
