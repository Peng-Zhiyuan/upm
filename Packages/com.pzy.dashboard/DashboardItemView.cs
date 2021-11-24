using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class DashboardItemView : MonoBehaviour
{
    public DashboardItemDropDownGroup dropdownGroup;
    public DashboardItemLabelGroup labelGroup;
    public DashboardItemCheckboxGroup checkboxGroup;

    DashboardItemType _viewType;
    public DashboardItemType ViewType
    {
        set
        {
            _viewType = value;
            this.SetAllGroupDeactive();
            var group = GetViewGroup(value);
            group.gameObject.SetActive(true);
        }
        get
        {
            return _viewType;
        }
    }

    public GameObject ActivedGroup
    {
        get
        {
            var value = this.ViewType;
            var group = GetViewGroup(value);
            return group;
        }
    }

    void SetAllGroupDeactive()
    {
        this.dropdownGroup.gameObject.SetActive(false);
        this.labelGroup.gameObject.SetActive(false);
        this.checkboxGroup.gameObject.SetActive(false);
    }

    GameObject GetViewGroup(DashboardItemType type)
    {
        if(type == DashboardItemType.DropDown)
        {
            return this.dropdownGroup.gameObject;
        }
        else if(type == DashboardItemType.Label)
        {
            return this.labelGroup.gameObject;
        }
        else if(type == DashboardItemType.Checkbox)
        {
            return this.checkboxGroup.gameObject;
        }
        else
        {
            throw new Exception($"[DashbaordItemView] unsupport view type: {type}");
        }
    }
}


