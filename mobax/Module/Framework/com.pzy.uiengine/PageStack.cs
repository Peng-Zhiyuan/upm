using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading.Tasks;
using System.Text;


public class PageStack  
{
    List<PageContainer> _stack = new List<PageContainer>();
    public void StackPush(PageContainer page)
    {
        _stack.Add(page);
    }
    public PageContainer StackPop()
    {
        var index = _stack.Count - 1;
        var page = _stack[index];
        _stack.RemoveAt(index);
        return page;
    }
    public PageContainer StackTryPeek()
    {
        var index = _stack.Count - 1;
        if(index < 0)
        {
            return null;
        }
        var page = _stack[index];
        return page;
    }
    public PageContainer StackTryPeekSecond()
    {
        var index = _stack.Count - 2;
        if(index < 0)
        {
            return null;
        }
        var page = _stack[index];
        return page;
    }
    public int StackCount()
    {
        var count = _stack.Count;
        return count;
    }
    public PageContainer TopIndex(int topIndex)
    {
        var count = _stack.Count;
        var index = count - 1 - topIndex;
        if(index < 0)
        {
            throw new Exception($"[PageStack] page count is {count}, can't top index {topIndex}");
        }
        var page = _stack[index];
        return page;
    }


    public void Push(PageContainer container)
    {

        container.Transform.localScale = Vector3.one;
        this.StackPush(container);

        container.Page.OnPush();

        //container.Active = true;
        //container.Page.Active = true;
        
        //RecaculateActive();


    }

    public PageContainer Pop()
    {
      
        if (_stack.Count == 0)
        {
            return null;
        }
        var container = this.StackPop();

        container.Page.OnPop();
       // container.Active = false;
       //// container.Page.PageBucket.ReleaseAll();
       // container.Page.Active = false;
        //RecaculateActive();

        return container;
    }

    public PageContainer FromTopFindName(string name)
    {
        var index = this.ContainerList.Count - 1;
        for(int i = index; i >=0; i--)
        {
            var container = this.ContainerList[i];
            if(container.pageName == name)
            {
                return container;
            }
        }
        throw new Exception($"[PageStack] FromTopFindName: not found name {name}");
    }

    public PageContainer FromTopFindTag(string tag)
    {
        var index = this.ContainerList.Count - 1;
        for (int i = index; i >= 0; i--)
        {
            var page = this.ContainerList[i];
            if (page.Page.pageTag == tag)
            {
                return page;
            }
        }
        throw new Exception($"[PageStack] FromTopFindTag: not found tag {tag}");
    }

    public List<PageContainer> PopUtil(PageContainer target)
    {
        var ret = new List<PageContainer>();
        if (_stack.Count == 0)
        {
            return ret;
        }
        if (this.StackTryPeek() == target)
        {
            return ret;
        }
        var from = this.StackPop();
        //from.Active = false;
        //from.Page.Active = false;
        from.Page.OnPop();
        //await page.OnPopAsync();
        ret.Add(from);


        while(this.StackTryPeek() != target)
        {
            var container = this.StackPop();
            //container.Active = false;
            //container.Page.Active = false;
            container.Page.OnPop();
            ret.Add(container);
        }

        //RecaculateActive();

        return ret;
    }


    public PageContainer Find(string name)
    {
        foreach (var container in _stack)
        {
            if (container.pageName == name)
            {
                return container;
            }
        }
        return null;
    }

    public PageContainer FindByTag(string tag)
    {
        foreach (var container in _stack)
        {
            if (container.pageTag == tag)
            {
                return container;
            }
        }
        return null;
    }

    public int Count
    {
        get
        {
            return _stack.Count;
        }
    }

    private Stack<PageContainer> tempStack = new Stack<PageContainer>();
    public PageContainer Remove(string name)
    {
        PageContainer ret = null;
        while (_stack.Count > 0)
        {
            var container = this.StackPop();
            if (container.pageName != name)
            {
                tempStack.Push(container);
            }
            else
            {
                ret = container;
                //container.Active = false;
                //container.Page.Active = false;
                container.Page.OnPop();
                //await page.OnPopAsync();
                break;
            }
        }
        while (tempStack.Count > 0)
        {
            var page = tempStack.Pop();
            this.StackPush(page);
        }
        //if (ret != null)
        //{
        //    RecaculateActive();
        //}
        return ret;
    }

    public string GetStackPrint()
    {
        var sb = new StringBuilder();
        var list = this._stack;
        for(var i = 0; i < list.Count; i++)
        {
            if(i > 0)
            {
                sb.Append(" -> ");
            }
            var container = list[i];

            var pageName = container.pageName;
            var pageTag = container.pageTag;
            var isLoaded = container.IsLoaded;
            var isActive = container.Active;

            sb.Append(pageName);

            if (!string.IsNullOrEmpty(pageTag))
            {
                sb.Append($" ({pageTag})");
            }

            if (isActive)
            {
                sb.Append("*");
            }

            if(!isLoaded)
            {
                sb.Append($" (Unloaded)");
            }
        }
        return sb.ToString();
    }




    public List<PageContainer> ContainerList
    {
        get
        {
            return _stack;
        }
    }
}
