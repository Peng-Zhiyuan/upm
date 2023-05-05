using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FragmentManager
{
    public Transform container;

    public UIEngineElement view;


    public FragmentManager(Transform container)
    {
        this.container = container;
        this.view = container.GetComponent<UIEngineElement>();
    }

    public List<LagacyFragment> _fragmentList;

    List<LagacyFragment> FragmentList
    {
        get
        {
            if (this._fragmentList == null)
            {
                var componentList = this.container.GetComponentsInChildren<LagacyFragment>(true);
                _fragmentList = new List<LagacyFragment>();
                _fragmentList.AddRange(componentList);
                foreach(var fragment in _fragmentList)
                {
                    fragment.fragmentManager = this;
                }
            }
            return _fragmentList;
        }
    }

    LagacyFragment currentFragment;

    public LagacyFragment CurrentFragment
    {
        get
        {
            return currentFragment;
        }
    }

    public void SwitchToNull()
    {
        if (this.currentFragment != null)
        {
            this.currentFragment.OnSwitchedFrom();
            this.currentFragment.gameObject.SetActive(false);
        }
        this.currentFragment = null;
    }
    
    /// <summary>
    /// 切换到指定碎片，如果指定碎片已经是当前碎片则什么也不会发生
    /// 碎片会在容器内自动搜索
    /// </summary>
    /// <typeparam name="T">碎片类型</typeparam>
    public void SwitchFragment<T>() where T : LagacyFragment
    {
        LagacyFragment newFragment = null;
        foreach (var one in this.FragmentList)
        {
            var type = one.GetType();
            if (type == typeof(T))
            {
                one.gameObject.SetActive(true);
                newFragment = one;
                if (one != currentFragment)
                {
                    one.OnSwitchedTo();
                }
            }
            else
            {
                one.gameObject.SetActive(false);
                if (one == currentFragment)
                {
                    one.OnSwitchedFrom();
                }
            }
        }
        currentFragment = newFragment;
    }

    public void OnPageNavigatedTo(PageNavigateInfo info)
    {
        this.CurrentFragment?.OnPageNavigatedTo(info);
    }
}
