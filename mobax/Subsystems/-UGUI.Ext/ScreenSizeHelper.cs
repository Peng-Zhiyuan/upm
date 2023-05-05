using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
public class ScreenSizeHelper : Single<ScreenSizeHelper>
{

   private const int design_height = 2340;
   private const int design_width = 1080;
    public static Action ScreenSizeChange;
    private RectTransform rectTransform = null;

   public int DesignWidth => design_width;
   public int DesignHeight => design_height;
   
   public Vector2 CanvasSize
   {
       get
       {
           if(rectTransform == null) rectTransform = UIEngine.Stuff.Canvas.GetComponent<RectTransform>();
           return rectTransform.sizeDelta;
       }
   }
  
    public float FixOutScale
    {
        get
        {
            return Mathf.Max(FixHeightScale, FixWidthScale);
        }
    }

    public float FixInScale
    {
        get
        {
            return Mathf.Min(FixHeightScale, FixWidthScale);
        }
    }


  

    public float FixWidthScale
    {
        get
        {
            return IsBaseOnHeight ? FitWidthScale / FitHeightScale :1;
        }
    }

    public float FixHeightScale
    {
        get
        {
            return IsBaseOnHeight?1: FitHeightScale / FitWidthScale;
        }
    }

    public int UIWidth
   {
       get
       {
           if(IsBaseOnHeight)
           {
               return (int)(Screen.width / FitHeightScale);
           }
           else 
           {
               return  design_width;
           }
       }
   }

   
   public int UIHeight
   {
       get
       {
           if(IsBaseOnHeight)
           {
               return  design_height; 
           }
           else 
           {
               return (int)(Screen.height / FitWidthScale);
           }
       }
   }

    public Vector3 ScreenToUIPos(Vector3 screen_pos)
    {
        Vector3 pos = new Vector3(screen_pos.x - Screen.width/2,screen_pos.y - Screen.height/2, 0);
        return  pos * ScreenSizeHelper.Instance.FitInScale;
    }

     public Vector3 UIToScreenPos(Vector3 ui_pos)
    {
        Vector3 pos = ui_pos / ScreenSizeHelper.Instance.FitInScale;
        return  new Vector3(pos.x + Screen.width/2, pos.y + Screen.height/2, 0);
    }

   public float FitHeightScale
   {
       get
       {
           return Screen.height /  (float)design_height; 
       }
   }
    public float FitInScale
    {
        get
        {
            if (IsBaseOnHeight)//屏幕宽，基于高度缩放
            {
                return FitHeightScale;
            }
            else
            {
                return FitWidthScale;
            }
        }
    }

    public bool IsBaseOnHeight
    {
        get
        {
            return Screen.width * design_height > Screen.height * design_width;
        }
    }

    public float FitOutScale
    {
        get
        {
            if (IsBaseOnHeight)//屏幕宽，基于宽度缩放
            {
                return FitWidthScale;
            }
            else
            {
                return FitHeightScale;
            }
        }
    }

    public float FitWidthScale
    {
        get
        {
            return Screen.width / (float)design_width;
        }
    }
    
}
