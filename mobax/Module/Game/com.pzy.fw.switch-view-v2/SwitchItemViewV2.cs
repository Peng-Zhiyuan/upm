using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public partial class SwitchItemViewV2 : MonoBehaviour
{
    PToggle _toggle;
    PToggle Toggle
    {
        get
        {
            if(_toggle == null)
            {
                _toggle = this.GetComponent<PToggle>();
            }
            return _toggle;
        }
    }

    void Start()
    {
        this.Toggle.StatusChnaged += StatusChnaged;
        this.Refresh();
    }

    Color selectedTextColor = ColorUtil.HexToColor("5F606F");
    Color unselectedTextColor = ColorUtil.HexToColor("C8C8C8");

    void Refresh()
    {
        var isOn = this.Toggle.IsOn;
        if (isOn)
        {
            this.Image_selected.DOFade(1, 0.2f);
            this.Text_label.DOColor(selectedTextColor, 0.2f);
            //this.Text_label.color = selectedTextColor;
        }
        else
        {
            this.Image_selected.DOFade(0, 0.2f);
            this.Text_label.DOColor(unselectedTextColor, 0.2f);
            //this.Text_label.color = unselectedTextColor;
        }
    }

    void StatusChnaged(PToggle toggle)
    {
        this.Refresh();
    }
}
