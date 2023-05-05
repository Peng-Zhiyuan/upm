using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TopFloatingService : Service
{
    public override void OnCreate()
    {
        UIEngine.OnBeforeNavigatTo += OnNavigatedHandler; 
    }

    public bool isAttaching;
    public void OnNavigatedHandler(PageNavigateInfo info)
    {
        var toPageName = info.to?.pageName;
        if(toPageName == nameof(MainPage))
        {
            isAttaching = true;
        }
        else if(toPageName == nameof(LoginPage))
        {
            isAttaching = false;
        }

        if (!isAttaching)
        {
            UIEngine.Stuff.RemoveFloating<TopFloatingV2>();
            return;
        }

     
        var targetPage = this.FindHigestConfigureablePage();
        if(targetPage == null)
        {
            UIEngine.Stuff.RemoveFloating<TopFloatingV2>();
            return;
        }
        var f = UIEngine.Stuff.ShowFloatingImediatly<TopFloatingV2>();
        UIEngine.Stuff.MoveFloatingLayer(f, targetPage);
        f.OnAttachingPageChnaged(targetPage);
    }


    bool IsAttachable(PageContainer pageContainer)
    {
        var page = pageContainer.Page;
        if(page != null)
        {
            var b = page.TryGetComponent<PageExtraParam>(out var param);
            if(b)
            {
                return param.attachTopFloating;
            }
        }
        if(pageContainer.isOverlay)
        {
            return false;
        }
        return true;
       
    }

    public Page FindHigestConfigureablePage()
    {
        var list = UIEngine.Stuff.pageStack.ContainerList;
        for (var i = list.Count - 1; i >= 0; i--)
        {
            var pageContainer = list[i];
            var b = IsAttachable(pageContainer);
            if(b)
            {
                return pageContainer.Page;
            }
            if(!pageContainer.isOverlay)
            {
                return null;
            }
        }
        return null;
    }


}
