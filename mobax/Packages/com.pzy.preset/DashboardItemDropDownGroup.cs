using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DashboardItemDropDownGroup : MonoBehaviour
{
    public Text headlabel;
    public Dropdown dropdown;

    public bool IsDefault
    {
        set
        {
            if(value)
            {
                this.dropdown.GetComponent<Image>().color = Color.white;
            }
            else
            {
                var b = ColorUtility.TryParseHtmlString("#6B8BFF", out var color);
                this.dropdown.GetComponent<Image>().color = color;
            }
        }
    }
        
}
