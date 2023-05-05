using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading.Tasks;

public class PageNavigateInfo
{
    public bool useTransaction;

    // 操作发起时的参数
    public string firstBackTo;
    public string secondBackToTag;
    public bool thirdBack;
    public string fourthForward;
    public string callerFilePath;

    public object param;
    public Func<Task> preChangeVisibility;

    // 管线进行中计算出的参数
    public PageContainer from;
    public PageContainer to;
    public PageContainer forwardNewPageContainer;
    public NavigateOperation terminalOperation;

    public override string ToString()
    {
        return $"[backTo: {firstBackTo}, backToTag: {secondBackToTag}, back: {thirdBack}, forward: {fourthForward} | from: {from?.pageName}, to: {to?.pageName}]";
    }
}
