using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System;

public class UIEngineUtil
{
    public static Vector2 WorldPositionToCanvasPositionAnchordLeftBottom(Vector2 worldPosition)
    {
        var screenLocation = Camera.main.WorldToScreenPoint(worldPosition);
        var canvasTransform = UIEngine.Stuff.CanvasTransform;
        var scrrenToCanvas = canvasTransform.sizeDelta.y / Screen.height;
        var canvasPosX = screenLocation.x * scrrenToCanvas;
        var canvasPosY = screenLocation.y * scrrenToCanvas;
        var anchordLeftBottonPos = new Vector2(canvasPosX, canvasPosY);
        return anchordLeftBottonPos;
    }

    public static float CanvasLengthToScreenLengthRate 
    {
        get
        {
            var canvasTransform = UIEngine.Stuff.CanvasTransform;
            var rate = Screen.height / canvasTransform.sizeDelta.y;
            return rate;
        }
    }

    public static float ScrrenLengthToCanvasLengthRate 
    {
        get
        {
            var canvasTransform = UIEngine.Stuff.CanvasTransform;
            var rate = canvasTransform.sizeDelta.y / Screen.height;
            return rate;
        }
    }

    /// <summary>
    /// 页面动画器是不是要管理退出？
    /// 如果动画器上有 will_exit 触发器，就认为管理，否则认为不管理
    /// </summary>
    public static bool IsAnimatorHasDisapear(Page page)
    {
        if(page == null)
        {
            return false;
        }
        var animator = page.GetComponent<Animator>();
        if(animator == null)
        {
            return false;
        }
        var has = animator.HasState(0, Animator.StringToHash("Disable"));
        return has;
    }

    public static async Task WaitAnimationDisableCompleteAsync(Page page)
    {
        var animator = page.GetComponent<Animator>();
        AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);
        while(!info.IsName("Disapear") || info.normalizedTime < 1.0f)
        {
            await Task.Delay(10);
            info = animator.GetCurrentAnimatorStateInfo(0);
        }
    }

    public static async Task PlayAndWaitAnimatorDisapearAsync(Page page)
    {
        var animatorWillHook = IsAnimatorHasDisapear(page);
        if (!animatorWillHook)
        {
            Debug.Log($"[UIEngine] not wait animator exit (page: {page?.name})");
            return;
        }
        var animator = page.GetComponent<Animator>();
        animator.Play("Disable");
        Debug.Log("[UIEngine] wait animator disable: " + page.name);
        await UIEngineUtil.WaitAnimationDisableCompleteAsync(page);
        Debug.Log("[UIEngine] animator disable: " + page.name);
    }

    public static (List<PageContainer> popedList, List<PageContainer> pushedList) RemoveCommonName(List<PageContainer> popedList, List<PageContainer> pushedList)
    {
        if(popedList.Count == 0 || pushedList.Count == 0)
        {
            return (popedList, pushedList);
        }
        // 两个输入的列表中，如果他们存在 pageName 相同的对象，移除这些对象
        var popedDict = new Dictionary<string, PageContainer>();
        var pushedDict = new Dictionary<string, PageContainer>();
        foreach (var poped in popedList)
        {
            popedDict[poped.pageName] = poped;
        }
        foreach (var pushed in pushedList)
        {
            pushedDict[pushed.pageName] = pushed;
        }
        var popedCommonNameList = new List<PageContainer>();
        var pushedCommonNameList = new List<PageContainer>();
        foreach (var poped in popedList)
        {
            if (pushedDict.ContainsKey(poped.pageName))
            {
                popedCommonNameList.Add(poped);
            }
        }
        foreach (var pushed in pushedList)
        {
            if (popedDict.ContainsKey(pushed.pageName))
            {
                pushedCommonNameList.Add(pushed);
            }
        }
        var copyPopedList = new List<PageContainer>(popedList);
        var copyPushedList = new List<PageContainer>(pushedList);

        foreach (var poped in popedCommonNameList)
        {
            copyPopedList.Remove(poped);
        }
        foreach (var pushed in pushedCommonNameList)
        {
            copyPushedList.Remove(pushed);
        }
        return (copyPopedList, copyPushedList);
    }




}