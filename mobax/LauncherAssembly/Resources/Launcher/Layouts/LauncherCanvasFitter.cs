using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Sirenix.OdinInspector;

public class LauncherCanvasFitter : MonoBehaviour
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
        //UIEngine.Stuff.CanvasSizeChnaged += OnCanvasSizeChnaged;
    }

    private void OnDestroy()
    {
        //UIEngine.Stuff.CanvasSizeChnaged -= OnCanvasSizeChnaged;
    }

    void ReprocessIfDirty()
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
        var worldRect = SpaceTool.TransformRect(rectTransform, localRect);

        var canvasTansform = LauncherUiManager.Stuff.Canvas.GetComponent<RectTransform>();
        var canvasRect = canvasTansform.rect;
        var canvasWorldRect = SpaceTool.TransformRect(canvasTansform, canvasRect);

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
            var canvasRectInContentSpace = SpaceTool.InverseTransformRect(rectTransform, canvasWorldRect);
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


    static class SpaceTool
    {
        // 世界矩形到本地矩形
        public static Rect InverseTransformRect(Transform localObj, Rect worldRect)
        {
            var worldMin = worldRect.min;
            var worldMax = worldRect.max;

            var localMin = localObj.InverseTransformPoint(worldMin);
            var localMax = localObj.InverseTransformPoint(worldMax);

            var localWidth = localMax.x - localMin.x;
            var localHeight = localMax.y - localMin.y;

            var localRect = new Rect(localMin.x, localMin.y, localWidth, localHeight);
            return localRect;
        }

        /// <summary>
        /// 本地矩形到世界
        /// </summary>
        public static Rect TransformRect(Transform localObj, Rect localRect)
        {
            var localMin = localRect.min;
            var localMax = localRect.max;

            var worldMin = localObj.TransformPoint(localMin);
            var worldMax = localObj.TransformPoint(localMax);

            var worldWidth = worldMax.x - worldMin.x;
            var worldHeight = worldMax.y - worldMin.y;

            var worldRect = new Rect(worldMin.x, worldMin.y, worldWidth, worldHeight);
            return worldRect;
        }

        /// <summary>
        /// 本地高度到世界高度
        /// </summary>
        public static float TransformHeight(Transform localObj, float localHeight)
        {
            var localV = new Vector3(0, localHeight, 0);
            var worldV = localObj.TransformVector(localV);
            var worldHeight = worldV.y;
            return worldHeight;
        }


        /// <summary>
        /// 世界高度到本地高度
        /// </summary>
        public static float InverseTransformHeight(Transform localObj, float worldHight)
        {
            var worldV = new Vector3(0, worldHight, 0);
            var localV = localObj.InverseTransformVector(worldV);
            var localHeight = localV.y;
            return localHeight;
        }
    }
}

