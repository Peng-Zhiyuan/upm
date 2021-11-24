using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class View : RecycledGameObject
{

    private RectTransform _rectTransform;
    public RectTransform rectTransform 
    {
        get 
        {
            if (_rectTransform == null) 
            {
                this._rectTransform = this.gameObject.GetComponent<RectTransform> ();
            }
            return _rectTransform;
        }
    }

    public RectTransform RectTransform
    {
        get
        {
            return this.rectTransform;
        }
    }

}