using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIEngineUtil
{
    public static Vector2 WorldPositionToCanvasPositionAnchordLeftBottom(Vector2 worldPosition)
    {
        var screenLocation = Camera.main.WorldToScreenPoint(worldPosition);
        var canvasTransform = UIEngine.Stuff.CanvasTransform;
        var scrrenToCanvas = canvasTransform.sizeDelta.y / Screen.height;
        var canvasPosX = screenLocation.x * scrrenToCanvas;
        var canvasPosY = screenLocation.y * scrrenToCanvas;
        var anchordLeftBottonPos = new Vector2(canvasPosX, canvasPosY);
        return anchordLeftBottonPos;
    }

    public static float CanvasLengthToScreenLengthRate 
    {
        get
        {
            var canvasTransform = UIEngine.Stuff.CanvasTransform;
            var rate = Screen.height / canvasTransform.sizeDelta.y;
            return rate;
        }
    }

    public static float ScrrenLengthToCanvasLengthRate 
    {
        get
        {
            var canvasTransform = UIEngine.Stuff.CanvasTransform;
            var rate = canvasTransform.sizeDelta.y / Screen.height;
            return rate;
        }
    }

    // public static Vector2 CanvasPositionAnchordLeftBottomToWorldPosition(Vector2 anchordLeftBottonPos)
    // {


    //     var screenLocation = Camera.main.WorldToScreenPoint(worldPosition);
    //     var canvasTransform = UIEngine.Stuff.CanvasTransform;
    //     var scrrenToCanvas = canvasTransform.sizeDelta.y / Screen.height;
    //     var canvasPosX = screenLocation.x * scrrenToCanvas;
    //     var canvasPosY = screenLocation.y * scrrenToCanvas;
    //     var anchordLeftBottonPos = new Vector2(canvasPosX, canvasPosY);
    //     return anchordLeftBottonPos;
    // }
}