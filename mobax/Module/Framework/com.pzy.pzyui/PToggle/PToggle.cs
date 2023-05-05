using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.EventSystems;
using System;

public class PToggle : MonoBehaviour, IPointerClickHandler
{
    [ReadOnly]
    public PToggleGroup group;

    bool _isOn;

    public ToggleAction wwiseAction = ToggleAction.None;

    [ShowInInspector]
    public virtual bool IsOn
    {
        get
        {
            return _isOn;
        }
        set
        {
            _isOn = value;
            this.Refresh();
            this.StatusChnaged?.Invoke(this);
        }
    }

    void Refresh()
    {
        var isOn = this.IsOn;
        if (SelectedObject != null)
        {
            this.SelectedObject?.SetActive(isOn);
        }

        if (UnselectedObject != null)
        {
            this.UnselectedObject?.SetActive(!isOn);
        }
    }

    public event Action<PToggle> StatusChnaged;

    public GameObject SelectedObject;
    public GameObject UnselectedObject;

    /// <summary>
    /// React to clicks.
    /// </summary>
    public virtual void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        if(group != null)
        {
            this.group.UserSelect(this);
        }
    }

    public string toggleTag;
}
