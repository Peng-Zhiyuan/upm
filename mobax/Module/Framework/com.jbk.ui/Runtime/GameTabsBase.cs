using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Toggle = UnityEngine.UIElements.Toggle;

public class GameTabsBase<T> : SerializedMonoBehaviour
{
    public Dictionary<MutexComponent, T> tabs;
    [HideInInspector]
    public Action<T> OnSelect;
    [HideInInspector]
    public Predicate<T> OnBeforeSelect;
    [HideInInspector]
    public Action<MutexComponent> OnSelectComp;
    /** 允许关闭 */
    public bool allowOff;

    private MutexComponent _prevSelected;
    private MutexComponent[] _keys;
    private T[] _values;

    public void SetSelect(MutexComponent comp)
    {
        if (_prevSelected == comp)
        {
            if (comp == null) return;
            if (!allowOff) return;
            
            comp = null;
        }

        if (null != OnBeforeSelect && null != comp)
        {
            if (!OnBeforeSelect(tabs[comp]))
            {
                return;
            }
        }

        if (_prevSelected != null)
        {
            _prevSelected.Selected = false;
        }

        if (null != comp)
        {
            comp.Selected = true;
        }

        if (null != OnSelect)
        {
            OnSelect(null == comp ? default : tabs[comp]);
        }

        if (null != OnSelectComp)
        {
            OnSelectComp(comp);
        }

        _prevSelected = comp;
    }

    public void SetSelect(T label)
    {
        var index = Array.IndexOf(_values, label);
        _SetSelectIndex(index);
    }

    public void ClearSelect()
    {
        SetSelect(null);
    }

    public void Add(MutexComponent comp, T val)
    {
        tabs ??= new Dictionary<MutexComponent, T>();
        tabs[comp] = val;
    }

    public void Remove(MutexComponent comp)
    {
        tabs.Remove(comp);
    }

    [ShowInInspector]
    public void Reset()
    {
        if (null == tabs) return;
        
        _keys = tabs.Keys.ToArray();
        _values = tabs.Values.ToArray();
        foreach (var mutexComponent in _keys)
        {
            mutexComponent.Selected = false;

            var clickComp = mutexComponent.GetComponent<ClickBehaviour>();
            clickComp.onClick = _OnTabClicked;
        }
    }

    private void Awake()
    {
        if (null == _keys)
        {
            Reset();
        }
    }

    private void _OnTabClicked(ClickBehaviour behaviour)
    {
        var current = behaviour.GetComponent<MutexComponent>();
        SetSelect(current);
        
        // 播放音效
        WwiseEventManager.SendEvent(TransformTable.UiControls, "ui_tabSwitching");
    }

    private void _SetSelectIndex(int index)
    {
        var comp = index >= 0 && index < _keys.Length ? _keys[index] : null;
        SetSelect(comp);
    }
    
    
#if UNITY_EDITOR
    /** 选中一个 */
    public int editorSelectIndex;
    
    private void OnValidate()
    {
        if (null == tabs) return;
        
        if (null == _keys || _keys.Length != tabs.Keys.Count)
        {
            Reset();
        }
        
        _SetSelectIndex(editorSelectIndex);
    }
#endif
}