using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public enum FillType
{
    FillByWidth,
    FillByHeight,
    FillInSize,
    FillOutSize
}
public class LauncherScaleFit : MonoBehaviour
{
    public FillType filType = FillType.FillOutSize;
    private int design_height = 2340;
    private int design_width = 1080;
    private RectTransform rectTransform = null;
    private Vector3 cacheScale;

    public void Awake()
    {
        this.cacheScale = this.transform.localScale;
        this.FitSize();
        
    }
    /*
    private void Update()
    {
        FitSize();
    }
    */
    public void FitSize()
    {
        var rectTransForm = this.GetComponent<RectTransform>();
        if (rectTransForm != null)
        {
            design_width = (int)rectTransForm.sizeDelta.x;
            design_height = (int)rectTransForm.sizeDelta.y;
        }

        switch (filType)
        {
            case FillType.FillByWidth: this.transform.localScale = this.cacheScale * this.FixWidthScale; break;
            case FillType.FillByHeight: this.transform.localScale = this.cacheScale * this.FixHeightScale; break;
            case FillType.FillInSize: this.transform.localScale = this.cacheScale * this.FixInScale; break;
            case FillType.FillOutSize: this.transform.localScale = this.cacheScale * this.FixOutScale; break;
            default: break;
        }
    }

    public Vector2 CanvasSize
    {
        get
        {
            if (rectTransform == null) rectTransform = LauncherUiManager.Stuff.GetComponent<RectTransform>();
            return rectTransform.sizeDelta;
        }
    }

    public float CanvasScale
    {
        get
        {
            if (rectTransform == null) rectTransform = LauncherUiManager.Stuff.GetComponent<RectTransform>();
            return rectTransform.localScale.x;
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
            return  Screen.width / (design_width * CanvasScale);
        }
    }

    public float FixHeightScale
    {
        get
        {
            return Screen.height / (design_height * CanvasScale);
        }
    }


    /*
    public float FitInScale
    {
        get
        {
            if (IsBaseOnHeight)//��Ļ������ڸ߶�����
            {
                return FitHeightScale;
            }
            else
            {
                return FitWidthScale;
            }
        }
    }
     public float FitOutScale
    {
        get
        {
            if (IsBaseOnHeight)//��Ļ������ڿ������
            {
                return FitWidthScale;
            }
            else
            {
                return FitHeightScale;
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

    public float FitWidthScale
    {
        get
        {
            return Screen.width / (float)design_width;
        }
    }

    public float FitHeightScale
    {
        get
        {
            return Screen.height / (float)design_height;
        }
    }
    */




}
