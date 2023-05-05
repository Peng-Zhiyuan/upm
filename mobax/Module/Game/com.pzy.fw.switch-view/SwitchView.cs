using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;

public partial class SwitchView : MonoBehaviour
{
    PToggleGroup _group;

    public PToggleGroup Group
    {
        get
        {
            if (_group == null)
            {
                var g = this.gameObject.GetComponent<PToggleGroup>();
                _group = g;
            }

            return _group;
        }
    }

    private void Awake()
    {
        this.Image_selectionBg.gameObject.SetActive(false);
        var toggleList = this.Group.ToggleList;
        foreach (var toggle in toggleList)
        {
            toggle.StatusChnaged += OnToggleStatusChnaged;
            toggle.SelectedObject.gameObject.SetActive(false);
            toggle.SelectedObject = null;
        }

        this.Group.SelectionChanged += OnSelectionChnaged;
        this.Group.UserSelected += OnUserSelected;
    }

    void OnUserSelected(PToggle p)
    {
        if (p.wwiseAction == ToggleAction.None)
        {
            WwiseEventManager.SendEvent(TransformTable.UiControls, "ui_tabSwitching");
        }
        else
        {
            switch (p.wwiseAction)
            {
                case ToggleAction.BtnHall:
                    WwiseEventManager.SendEvent(TransformTable.UiControls, "ui_hall");
                    break;
                case ToggleAction.BtnStore:
                    WwiseEventManager.SendEvent(TransformTable.UiControls, "ui_store");
                    break;
                case ToggleAction.BtnCharacter:
                    WwiseEventManager.SendEvent(TransformTable.UiControls, "ui_character");
                    break;
                case ToggleAction.BtnWorkshop:
                    WwiseEventManager.SendEvent(TransformTable.UiControls, "ui_workshop");
                    break;
                case ToggleAction.BtnCall:
                    WwiseEventManager.SendEvent(TransformTable.UiControls, "ui_call");
                    break;
            }
        }
    }

    private void OnRectTransformDimensionsChange()
    {
        RefreshSelectionBackground(false);
    }

    void OnSelectionChnaged()
    {
        RefreshSelectionBackground();
    }

    void RefreshSelectionBackground(bool useAnimation = true)
    {
        var ptoggle = this.Group.Selected;
        if (ptoggle == null)
        {
            this.Image_selectionBg.gameObject.SetActive(false);
            return;
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(this.rectTransform());
        var toggleView = ptoggle.GetComponent<SwitchItemView>();
        var selectedTarget = toggleView.SelectedImage;
        var localRect = selectedTarget.rectTransform().rect;
        var worldRect = SpaceUtil.TransformRect(selectedTarget.transform, localRect);
        var finalLocalRect = SpaceUtil.InverseTransformRect(this.Image_selectionBg.transform.parent, worldRect);
        var localSize = finalLocalRect.size;
        var rectTransform = this.Image_selectionBg.rectTransform();
        var visible = this.Image_selectionBg.gameObject.activeSelf;
        this.Image_selectionBg.gameObject.SetActive(true);

        if (visible && useAnimation)
        {
            this.Image_selectionBg.rectTransform().DOLocalMove(finalLocalRect.center, 0.2f);
            this.Image_selectionBg.rectTransform().DOSizeDelta(localSize, 0.2f);
        }
        else
        {
            rectTransform.localPosition = finalLocalRect.center;
            rectTransform.sizeDelta = finalLocalRect.size;
        }
    }


    Color TextLightColor = Color.white;
    Color TextDarkColor = ColorUtil.HexToColor("B7B7B7");

    void OnToggleStatusChnaged(PToggle toggle)
    {
        var text = toggle.GetComponentInChildren<Text>();
        if (text == null)
        {
            return;
        }

        if (toggle.IsOn)
        {
            text.color = TextLightColor;
        }
        else
        {
            text.color = TextDarkColor;
        }
    }

    public string SelectedTag
    {
        get { return this.Group.SelectedTag; }
        set { this.Group.SelectedTag = value; }
    }

    public event Action SelectionChnaged
    {
        add { this.Group.SelectionChanged += value; }
        remove { this.Group.SelectionChanged -= value; }
    }
}