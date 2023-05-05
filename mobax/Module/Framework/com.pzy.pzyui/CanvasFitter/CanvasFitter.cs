using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Sirenix.OdinInspector;

public class CanvasFitter : MonoBehaviour
{
    public enum FitType
    {
        /// <summary>
        /// 外切
        /// </summary>
        ScaleToOuter,

        /// <summary>
        /// 内切
        /// </summary>
        ScaleToInner,

        /// <summary>
        /// 外切，但是 scale 不会小于 1
        /// </summary>
        ScaleToOuterOnlyExpand,

        /// <summary>
        /// 内切，但是 scale 不会小于 1
        /// </summary>
        ScaleToInnerOnlyExpand,

        /// <summary>
        /// 修改尺寸到和画布重合
        /// </summary>
        ResizeToCover,
        
    }

    public FitType fitType;

    private void Start()
    {
        this.Reporcess();
        UIEngine.Stuff.CanvasSizeChnaged += OnCanvasSizeChnaged;
    }

    private void OnDestroy()
    {
        UIEngine.Stuff.CanvasSizeChnaged -= OnCanvasSizeChnaged;
    }

    public void ReprocessIfDirty()
    {
        if(dirty)
        {
            this.Reporcess();
            this.dirty = false;
        }
    }

    bool dirty;
    void TryReprocess()
    {
        
        dirty = true;
        if (this.isActiveAndEnabled)
        {
            this.ReprocessIfDirty();
        }
    }

    private void OnRectTransformDimensionsChange()
    {
        this.TryReprocess();
    }

    public void OnParentScaleChnaged()
    {
        //this.dirty = true;
        //this.ReprocessIfDirty();
    }

    void OnCanvasSizeChnaged()
    {
        this.TryReprocess();
    }

    private void OnEnable()
    {
        this.dirty = true;
        this.ReprocessIfDirty();
    }


    DrivenRectTransformTracker tracker;

    [ShowInInspector]
    public void Reporcess()
    {
        tracker.Clear();
        var rectTransform = this.GetComponent<RectTransform>();
        rectTransform.localScale = Vector3.one;
        var localRect = rectTransform.rect;
        var worldRect = SpaceUtil.TransformRect(rectTransform, localRect);

        var canvasTansform = UIEngine.Stuff.CanvasTransform;
        var canvasRect = canvasTansform.rect;
        var canvasWorldRect = SpaceUtil.TransformRect(canvasTansform, canvasRect);

        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);

        if (this.fitType != FitType.ResizeToCover)
        {
            var xRatio = canvasWorldRect.width / worldRect.width;
            var yRatto = canvasWorldRect.height / worldRect.height;

            var worldScale = 0f;
            if (this.fitType == FitType.ScaleToInner)
            {
                worldScale = Math.Min(xRatio, yRatto);
            }
            else if (this.fitType == FitType.ScaleToOuter)
            {
                worldScale = Math.Max(xRatio, yRatto);
            }
            else if (this.fitType == FitType.ScaleToInnerOnlyExpand)
            {
                worldScale = Math.Min(xRatio, yRatto);
                if (worldScale < 1f)
                {
                    worldScale = 1f;
                }
            }
            else if (this.fitType == FitType.ScaleToOuterOnlyExpand)
            {
                worldScale = Math.Max(xRatio, yRatto);
                if (worldScale < 1f)
                {
                    worldScale = 1f;
                }
            }
            transform.localScale = new Vector3(worldScale, worldScale, worldScale);

            // 申明控制 scale
            tracker.Add(this, rectTransform, DrivenTransformProperties.Scale);

            // 位置设置到画布中心
            var centerWorldPos = canvasWorldRect.center;
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.position = centerWorldPos;
            tracker.Add(this, rectTransform, DrivenTransformProperties.Pivot | DrivenTransformProperties.AnchoredPosition);
        }
        else
        {
            var canvasRectInContentSpace = SpaceUtil.InverseTransformRect(rectTransform, canvasWorldRect);
            // 位置设置到画布中心
            var centerWorldPos = canvasWorldRect.center;
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.position = centerWorldPos;

            var width = canvasRectInContentSpace.width;
            var height = canvasRectInContentSpace.height;
            rectTransform.SetSizeDelta(new Vector2(width, height));

            tracker.Add(this, rectTransform, DrivenTransformProperties.Pivot | DrivenTransformProperties.AnchoredPosition | DrivenTransformProperties.SizeDelta);
        }


    }

}
