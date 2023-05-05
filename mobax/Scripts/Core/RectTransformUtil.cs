using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RectTransformUtil 
{
    public static void RepositionAsPopTip(RectTransform contentRectTransform, Vector2 worldPosition, RectTransform viewTransform)
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentRectTransform);

        var viewRect = viewTransform.rect;
        var worldViewRect = SpaceUtil.TransformRect(viewTransform, viewRect);

        var contentRect = contentRectTransform.rect;
        var worldContentRect = SpaceUtil.TransformRect(contentRectTransform, contentRect);

        var pivodLeft = false;
        var pivodRight = false;
        var pivodTop = false;
        var pivodBottom = false;
        if (worldViewRect.Contains(worldPosition + new Vector2(worldContentRect.width, 0)))
        {
            pivodLeft = true;
        }
        else
        {
            pivodRight = true;
        }
        if (worldViewRect.Contains(worldPosition - new Vector2(0, worldContentRect.height)))
        {
            pivodTop = true;
        }
        else
        {
            pivodBottom = true;
        }

        if (pivodLeft && pivodTop)
        {
            contentRectTransform.pivot = new Vector2(0, 1);
        }
        else if (pivodLeft && pivodBottom)
        {
            contentRectTransform.pivot = new Vector2(0, 0);
        }
        else if (pivodRight && pivodTop)
        {
            contentRectTransform.pivot = new Vector2(1, 1);
        }
        else if (pivodRight && pivodBottom)
        {
            contentRectTransform.pivot = new Vector2(1, 0);
        }

        //contentRectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        //contentRectTransform.anchorMax = new Vector2(0.5f, 0.5f);

        //var localPosition = this.rectTransform.worldToLocalMatrix.MultiplyPoint(worldPosition);

        contentRectTransform.position = worldPosition;
    }
}
