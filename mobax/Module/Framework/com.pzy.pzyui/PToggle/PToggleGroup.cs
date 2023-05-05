using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PToggleGroup : MonoBehaviour
{
    void Awake()
    {
        this.BuildToggleList();
    }


    Property<string> _dataSource;
    public Property<string> DataSource
    {
        get
        {
            return _dataSource;
        }
        set
        {
            if(value == _dataSource)
            {
                return;
            }
            var before = _dataSource;
            _dataSource = value;
            var post = _dataSource;
            this.OnDataSourceChanged(before, post);
        }
    }

    void OnDataSourceChanged(Property<string> before, Property<string> post)
    {
        this.Refresh();
    }

    public void Refresh()
    {
        if(this.DataSource == null)
        {
            this.SelectedIndex = -1;
        }
        else
        {
            var value = this.DataSource.Value;
            this.SelectedTag = value;
        }
    }

    List<PToggle> _toggleList;
    public List<PToggle> ToggleList
    {
        get
        {
            if(_toggleList == null)
            {
                this.BuildToggleList();
            }
            return _toggleList;
        }
    }

    public PToggle GetToggle(string tag)
    {
        foreach(var one in this.ToggleList)
        {
            if(one.toggleTag == tag)
            {
                return one;
            }
        }
        return null;
    }

    void BuildToggleList()
    {
        _toggleList = new List<PToggle>();
        var list = this.GetComponentsInChildren<PToggle>();
        foreach (var toggle in list)
        {
            toggle.group = this;
            this._toggleList.Add(toggle);
        }
    }

    public event Action SelectionChanged;
    int _selectedIndex = -1;
    public int SelectedIndex
    {
        get
        {
            return _selectedIndex;
        }
        set
        {
            if(value == _selectedIndex)
            {
                return;
            }
            _selectedIndex = value;
            for (int i = 0; i < ToggleList.Count; i++)
            {
                var toggle = this.ToggleList[i];
                if(i == value)
                {
                    toggle.IsOn = true;
                }
                else
                {
                    toggle.IsOn = false;
                }
            }
            SelectionChanged?.Invoke();
        }
    }

    public string SelectedTag
    {
        get
        {
            if(this.Selected == null)
            {
                return null;
            }
            return this.Selected.toggleTag;
        }
        set
        {
            for(int i = 0; i < this.ToggleList.Count; i++)
            {
                var toggle = this.ToggleList[i];
                if(value == toggle.toggleTag)
                {
                    this.SelectedIndex = i;
                    return;
                }
            }
            this.SelectedIndex = -1;
        }
    }

    public PToggle Selected
    {
        get
        {
            if(this.SelectedIndex == -1)
            {
                return null;
            }
            var toggle = ToggleList[this.SelectedIndex];
            return toggle;
        }
        set
        {
            var index = this.ToggleList.IndexOf(value);
            this.SelectedIndex = index;
        }
    }

    public Action<PToggle> UserSelected;

    /// <summary>
    /// 用户重新点击已选中的页签
    /// </summary>
    public Action UserReclickedSelection;
    public void UserSelect(PToggle toggle)
    {
        if(toggle == this.Selected)
        {
            UserReclickedSelection?.Invoke();
            return;
        }
        this.Selected = toggle;
        UserSelected?.Invoke(toggle);
        if(this.DataSource != null)
        {
            this.DataSource.Value = this.SelectedTag;
        }
    }
}
