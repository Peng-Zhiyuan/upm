using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DashboardItemCheckboxGroup : MonoBehaviour
{
    public Text headLabel;
    public Toggle toggle;

    public bool IsDefault
    {
        set
        {
            if (value)
            {
                this.toggle.GetComponentInChildren<Image>().color = Color.white;
            }
            else
            {
                var b = ColorUtility.TryParseHtmlString("#6B8BFF", out var color);
                this.toggle.GetComponentInChildren<Image>().color = color;
            }
        }
    }
}
