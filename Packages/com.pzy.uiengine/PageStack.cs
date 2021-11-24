using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading.Tasks;


public class PageStack  
{
    Stack<Page> stack = new Stack<Page>();

    public void Push(Page page, object param)
    {
        if(stack.Count > 0)
        {
            var top = stack.Peek(); 
            top.OnNavigatedFrom();
        }

        page.Transform.localScale = Vector3.one;
        var from = Peek();
        stack.Push(page);

        page.OnPush();
        //Debug.Log("Navigate to: " + page.name);
        //GuideCtr.CheckGuide (page.name); 
        PageNavigateInfo naviInof = new PageNavigateInfo();
        naviInof.param = param;
        naviInof.isBack = false;
        if(from != null){
            naviInof.lastPageType = from.GetType();
        }
        else{
            naviInof.lastPageType = null;
        }
        page.ContainerActive = true;
        page.Active = true;
        page.OnForwardTo(naviInof);
        page.OnNavigatedTo(naviInof);
        
        
        RecaculateActive();

        UIEngine.TopPageChanged?.Invoke(page);

        // set sibling position
        if(from != null)
        {
            var index = from.Transform.GetSiblingIndex();
            page.Transform.SetSiblingIndex(index + 1);
        }
    }

    public async Task<Page> PopAsync()
    {
        if(stack.Count == 0) return null;
        var page = stack.Pop();
        page.OnNavigatedFrom();
        page.OnPop();
        await page.OnPopAsync();
        page.ContainerActive = false;
        page.Active = false;
        RecaculateActive();
        if (stack.Count > 0)
        {
            var top = stack.Peek();
            //GuideCtr.CheckGuide (top.name); 
            UIEngine.TopPageChanged?.Invoke(top);
            
            PageNavigateInfo naviInof = new PageNavigateInfo();
            naviInof.param = null;
            naviInof.isBack = true;
            naviInof.lastPageType = page.GetType();

            top.OnBackTo(naviInof);
            top.OnNavigatedTo(naviInof);
        }
        
        return page;
    }

    public async Task<List<Page>> PopUtilAsync(Page target, object param)
    {
        var ret = new List<Page>();
        if(stack.Count == 0) return ret;
        if(Peek() == target) return ret;
        var page = stack.Pop();
        page.ContainerActive = false;
        page.Active = false;
        page.OnNavigatedFrom();
        page.OnPop();
        await page.OnPopAsync();
        ret.Add(page);

        Type lastPageType = null;

        while(stack.Peek() != target)
        {
            var p = stack.Pop();
            p.ContainerActive = false;
            p.Active = false;
            p.OnPop();
            await p.OnPopAsync();
            ret.Add(p);
            lastPageType = p.GetType();
        }

        RecaculateActive();
        if (stack.Count > 0)
        {
            var top = stack.Peek();

            PageNavigateInfo naviInof = new PageNavigateInfo();
            naviInof.param = param;
            naviInof.isBack = true;
            naviInof.lastPageType = lastPageType;

            top.OnBackTo(naviInof);
            top.OnNavigatedTo(naviInof);
        }
        UIEngine.TopPageChanged?.Invoke(page);
        return ret;
    }

	
    public Page Peek()
    {
        if(stack.Count > 0)
        {
            return stack.Peek();
        }
        else
        {
            return null;
        }
    }

    public Page Find(string name)
    {
        foreach (var page in stack)
        {
            if (page.GameObject.name == name)
            {
                return page;
            }
        }
        return null;
    }

    public int Count
    {
        get
        {
            return stack.Count;
        }
    }

    private Stack<Page> tempStack = new Stack<Page>();
    public async Task<Page> RemoveAsync(string name)
    {
        Page ret = null;
        while (stack.Count > 0)
        {
            var page = stack.Pop();
            if (page.GameObject.name != name)
            {
                tempStack.Push(page);
            }
            else
            {
                ret = page;
                page.ContainerActive = false;
                page.Active = false;
                page.OnPop();
                await page.OnPopAsync();
                break;
            }
        }
        while (tempStack.Count > 0)
        {
            var page = tempStack.Pop();
            stack.Push(page);
        }
        if (ret != null)
        {
            RecaculateActive();
        }
        return ret;
    }

    public void Log(string tag)
    {
        var str = "";
        var list = this.stack.ToArray();
        Array.Reverse(list);
        for(var i = 0; i < list.Length; i++)
        {
            if(i > 0)
            {
                str += " -> ";
            }
            var page = list[i];
            str += page.GameObject.name;
            if(page.Active)
            {
                str += "*";
            }
        }
        Debug.Log("[UIEngine] [" + tag + "] " + str);
    }

    public async Task RelpaceTopAsync(Page page, object param)
    {
		if(stack.Count==0)return;
        var old = stack.Pop();
        old.ContainerActive = false;
        old.Active = false;
        old.OnNavigatedFrom();
        old.OnPop();
        await old.OnPopAsync();

        old.Poped?.Invoke();
        old.RecyclePageAndContainer();

	
        stack.Push(page);
        page.ContainerActive = true;
        page.Active = true;
        page.OnPush();

        PageNavigateInfo naviInof = new PageNavigateInfo();
        naviInof.param = param;
        naviInof.isBack = false;
        naviInof.lastPageType = old.GetType();

        page.OnNavigatedTo(naviInof);
        UIEngine.TopPageChanged?.Invoke(page);
        RecaculateActive();
    }

    static Page[] tempArray = new Page[50];
    private void RecaculateActive()
    {
        stack.CopyTo(tempArray, 0);
        var visible = true;
        for (var i = 0; i < stack.Count; i++)
        {
            Page page = tempArray[i];
            page.ContainerActive = visible;
            page.Active = visible;
            if (!page.IsOverlay)
            {
                visible = false;
            }
        }
    }

    public Stack<Page> InnerStack
    {
        get
        {
            return stack;
        }
    }
}
