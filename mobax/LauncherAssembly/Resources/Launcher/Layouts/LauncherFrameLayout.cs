using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class LauncherFrameLayout : MonoBehaviour, ILayoutGroup
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
    }

    public FitType fitType;

    public bool topExpandUnsafeArea;
    public bool bottomExpandUnsafeArea;

    private void Start()
    {
        // 计算不安全区扩展
        if (Application.isPlaying)
        {
            if (this.topExpandUnsafeArea)
            {
                var selfSize = this.GetComponent<RectTransform>().rect.size;
                var h = 100; //UIEngine.Stuff.safeArea.TopNonSafeHight;
                var worldH = 100;//LauncherSpaceUtil.InverseTransformHeight(UIEngine.Stuff.CanvasTransform, h);
                var localH = LauncherSpaceUtil.TransformHeight(this.transform, worldH);
                GetComponent<RectTransform>().SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, -localH, selfSize.y + localH);
            }
            else if (this.bottomExpandUnsafeArea)
            {
                var selfSize = this.GetComponent<RectTransform>().rect.size;
                var h = 100;//UIEngine.Stuff.safeArea.BottomNonSafeHight;
                var worldH = 100;//LauncherSpaceUtil.InverseTransformHeight(UIEngine.Stuff.CanvasTransform, h);
                var localH = LauncherSpaceUtil.TransformHeight(this.transform, worldH);
                GetComponent<RectTransform>().SetInsetAndSizeFromParentEdge(RectTransform.Edge.Bottom, localH, selfSize.y + localH);
            }
            
        }

        this.Reporcess();
    }

    void ReprocessIfDirty()
    {
        if (dirty)
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

    public void SetLayoutHorizontal()
    {
        this.TryReprocess();

    }

    public void SetLayoutVertical()
    {
        this.TryReprocess();
    }

    private void OnEnable()
    {
        this.ReprocessIfDirty();
    }

    RectTransform FirstChild
    {
        get
        {
            if(this.transform.childCount == 0)
            {
                return null;
            }
            var child = this.transform.GetChild(0);
            var rect = child.GetComponent<RectTransform>();
            return rect;
        }
    }

    DrivenRectTransformTracker tracker;

    [ShowInInspector]
    public void Reporcess()
    {
        tracker.Clear();
        var child = this.FirstChild;
        if(child == null)
        {
            return;
        }
        child.pivot = new Vector2(0.5f, 0.5f);
        child.anchorMin = new Vector2(0.5f, 0.5f);
        child.anchorMax = new Vector2(0.5f, 0.5f);
        child.anchoredPosition = Vector2.zero;
        var childSize = child.rect.size;
        var selfSize = this.GetComponent<RectTransform>().rect.size;

        var xRatio = selfSize.x / childSize.x;
        var yRatto = selfSize.y / childSize.y;

        var scale = 0f;
        if (this.fitType == FitType.ScaleToInner)
        {
            scale = Math.Min(xRatio, yRatto);
        }
        else if (this.fitType == FitType.ScaleToOuter)
        {
            scale = Math.Max(xRatio, yRatto);
        }

        child.localScale = new Vector3(scale, scale, scale);

        // 申明控制 scale
        tracker.Add(this, child, DrivenTransformProperties.Scale | DrivenTransformProperties.Pivot | DrivenTransformProperties.AnchoredPosition | DrivenTransformProperties.AnchorMin | DrivenTransformProperties.AnchorMax);


    }

}
