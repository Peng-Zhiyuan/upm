using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class FillSprite : MonoBehaviour
{
    //public Image image;
    private float cacheWidth = 0;
    private float cacheHeight = 0;
    private RectTransform rectTransform;
    private void Awake()
    {
        rectTransform = this.GetComponent<RectTransform>();
        cacheWidth = rectTransform.rect.width;
        cacheHeight = rectTransform.rect.height;
    }
    private Image mImage;
    public Image image
    {
        get
        {
            if(mImage == null)
            {
               mImage =  this.GetComponent<Image>();
            }
            return mImage;
        }
    }

    private float Width
    {
        get 
        {
            return rectTransform.rect.width;
        }
        set
        {
            rectTransform.sizeDelta = new Vector2(value, this.cacheHeight);
           // rectTransform.sizeDelta.Set(value, this.cacheHeight);// = new Vector2(value, this.cacheHeight);
        }
       
    }

    public float FillRange
    {
        get
        {
            return Width / cacheWidth;
        }
        set
        {
            var ratio = Mathf.Clamp(value, 0, 1);
            this.Width = this.cacheWidth * ratio;
            // Debug.LogError("ratio:"+ratio+"   his.Width:"+this.Width );
        }
    }

}
