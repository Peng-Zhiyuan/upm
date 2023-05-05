using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public partial class MaskFloating : Floating
{
    public void OnButtonClick()
    {
        if(clickToBack)
        {
            UIEngine.Stuff.Back();
        }
    }

    public bool IsMaskTransparent
    {
        set
        {
            this.Image_black.gameObject.SetActive(!value);
        }
    }

    public bool clickToBack;
}
