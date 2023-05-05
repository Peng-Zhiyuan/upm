using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public partial class PProgressbar : MonoBehaviour
{
    [ShowInInspector]
    public void Set(float beforeRate, float afterRate, bool beforeInForeground = true)
    {
        if(beforeInForeground)
        {
            this.Image_before.GetComponent<RectTransform>().anchorMax = new Vector2(beforeRate, 1);
            this.Image_after.GetComponent<RectTransform>().anchorMax = new Vector2(afterRate, 1);
        }
        else
        {
            this.Image_after.GetComponent<RectTransform>().anchorMax = new Vector2(beforeRate, 1);
            this.Image_before.GetComponent<RectTransform>().anchorMax = new Vector2(afterRate, 1);
        }
    }
}
