using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CameraUtil
{
    // 获得摄像机正在渲染的区域的尺寸
    // 其比值与设备分辨率相同
    public static Vector2 GetRanderingSize()
    {
        float leftBorder;
        float rightBorder;
        float topBorder;
        float downBorder;
        //the up right corner
        Vector3 cornerPos = Camera.main.ViewportToWorldPoint(new Vector3(1f, 1f, Mathf.Abs(Camera.main.transform.position.z)));
        leftBorder = Camera.main.transform.position.x - (cornerPos.x - Camera.main.transform.position.x);
        rightBorder = cornerPos.x;
        topBorder = cornerPos.y;
        downBorder = Camera.main.transform.position.y - (cornerPos.y - Camera.main.transform.position.y);
        var width = rightBorder - leftBorder;
        var height = topBorder - downBorder;
        return new Vector2(width, height);
    }

    // 设置摄像机渲染尺寸
    // 根据一个设计渲染区域，并且宽度适配实际设备，高度自动变动
    // 返回高度实际渲染区域和设计渲染区域的比值
    public static float SetCameraSizeByDecisionRevelutionAndFixAtWidth(int width, int height)
    {
        var hvw = Screen.height / (float)Screen.width;
        var cameraHeight = 1080 * hvw;
        var halfCameraHeight = cameraHeight / 2;
        Camera.main.orthographicSize = halfCameraHeight;
        var scaled = cameraHeight / 1920f;
        return scaled;
    }

    /// <summary>
    /// 世界坐标转换为屏幕坐标
    /// </summary>
    /// <param name="worldPoint">屏幕坐标</param>
    /// <returns></returns>
    public static Vector2 WorldPointToScreenPoint(Vector3 worldPoint)
    {
        // Camera.main 世界摄像机
        Vector2 screenPoint = CameraManager.WorldToScreenPoint(CameraManager.Instance.MainCamera, worldPoint);
        return screenPoint;
    }

    /// <summary>
    /// 屏幕坐标转换为世界坐标
    /// </summary>
    /// <param name="screenPoint">屏幕坐标</param>
    /// <param name="planeZ">距离摄像机 Z 平面的距离</param>
    /// <returns></returns>
    public static Vector3 ScreenPointToWorldPoint(Vector2 screenPoint, float planeZ)
    {
        // Camera.main 世界摄像机
        Vector3 position = new Vector3(screenPoint.x, screenPoint.y, planeZ);
        Vector3 worldPoint = Camera.main.ScreenToWorldPoint(position);
        return worldPoint;
    }

    // RectTransformUtility.WorldToScreenPoint
    // RectTransformUtility.ScreenPointToWorldPointInRectangle
    // RectTransformUtility.ScreenPointToLocalPointInRectangle
    // 上面三个坐标转换的方法使用 Camera 的地方
    // 当 Canvas renderMode 为 RenderMode.ScreenSpaceCamera、RenderMode.WorldSpace 时 传递参数 canvas.worldCamera
    // 当 Canvas renderMode 为 RenderMode.ScreenSpaceOverlay 时 传递参数 null

    // UI 坐标转换为屏幕坐标
    public static Vector2 UIPointToScreenPoint(Vector3 worldPoint)
    {
        Camera uiCamera = UIEngine.Stuff.UICamera;
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(uiCamera, worldPoint);
        return screenPoint;
    }

    // 屏幕坐标转换为 UGUI 坐标
    public static Vector3 ScreenPointToUIPoint(RectTransform rt, Vector2 screenPoint)
    {
        Vector3 globalMousePos;
        //UI屏幕坐标转换为世界坐标
        Camera uiCamera = UIEngine.Stuff.UICamera;

        // 当 Canvas renderMode 为 RenderMode.ScreenSpaceCamera、RenderMode.WorldSpace 时 uiCamera 不能为空
        // 当 Canvas renderMode 为 RenderMode.ScreenSpaceOverlay 时 uiCamera 可以为空
        RectTransformUtility.ScreenPointToWorldPointInRectangle(rt, screenPoint, uiCamera, out globalMousePos);
        // 转换后的 globalMousePos 使用下面方法赋值
        // target 为需要使用的 UI RectTransform
        // rt 可以是 target.GetComponent<RectTransform>(), 也可以是 target.parent.GetComponent<RectTransform>()
        // target.transform.position = globalMousePos;
        return globalMousePos;
    }

    // 屏幕坐标转换为 UGUI RectTransform 的 anchoredPosition
    public static Vector2 ScreenPointToUILocalPoint(RectTransform parentRT, Vector2 screenPoint)
    {
        Vector2 localPos;
        Camera uiCamera = UIEngine.Stuff.UICamera;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRT, screenPoint, uiCamera, out localPos);
        // 转换后的 localPos 使用下面方法赋值
        // target 为需要使用的 UI RectTransform
        // parentRT 是 target.parent.GetComponent<RectTransform>()
        // 最后赋值 target.anchoredPosition = localPos;
        return localPos;
    }
}