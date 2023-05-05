using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Threading.Tasks;
using UnityEngine.UI;
using System;

public abstract class TestCase 
{
    public abstract Task RunAsync();


    public bool IsGameObjectExists(string gameObjectName)
    {
        this.Log($"IsGameObjectExists: {gameObjectName}");
        var go = FindGameObject(gameObjectName);
        if (go != null)
        {
            this.Log($"true");
            return true;
        }
        else
        {
            this.Log($"false");
            return false;
        }
    }

    public void Log(string msg)
    {
        Debug.Log(msg);
        TestManager.TestGUI.AddMsg(msg);
        TestManager.WriteToCurrentLogFile(msg);
    }

    public async Task WaitAsync(float seconds)
    {
        this.Log($"WaitAsync: {seconds}");
        var ms = seconds * 1000;
        await Task.Delay((int)ms);
    }

    RaycastTestInfo RaycastTest(GameObject rectGameObject)
    {
        var point = GetCenterScrrenPoint(rectGameObject);

        var data = new PointerEventData(EventSystem.current);
        data.position = point;
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(data, results);

        var info = new RaycastTestInfo();
        info.eventData = data;


        int? maxDepth = null;
        GameObject maxDepthGo = null;
        RaycastResult? maxDepthRaycastResult = null;
        foreach(var r in results)
        {
            var go = r.gameObject;
            var depth = r.depth;
            if(maxDepth == null || depth > maxDepth.Value)
            {
                maxDepth = depth;
                maxDepthGo = go;
                maxDepthRaycastResult = r;
            }
        }

        info.eventTarget = maxDepthGo;
        if(maxDepthRaycastResult != null)
        {
            data.pointerCurrentRaycast = maxDepthRaycastResult.Value;
        }

        return info;
    }

    /// <summary>
    /// 在 gameObjectName 的中心位置点击屏幕
    /// </summary>
    /// <param name="gameObjectName"></param>
    public void Click(string gameObjectName)
    {
        this.Log($"Click: {gameObjectName}");
        var targetGo = FindGameObject(gameObjectName);
        if (targetGo == null)
        {
            throw new TestException($"Click: 没有找到游戏对象: {gameObjectName}");
        }

        var testInfo = RaycastTest(targetGo);

        this.SendEvent<IPointerClickHandler>(testInfo, ExecuteEvents.pointerClickHandler);


        // pzy:
        // 显示点击位置
        //var touchFloting = UIEngine.Stuff.FindFloating<TouchEffectFloating>();
        //touchFloting.CreateEffect(testInfo.eventData.position);
    }

    GameObject SendEvent<T>(RaycastTestInfo testInfo, ExecuteEvents.EventFunction<T> functo) where T : IEventSystemHandler
    {
        var eventTarget = testInfo.eventTarget;
        var data = testInfo.eventData;

        while (eventTarget != null)
        {
            var processed = ExecuteEvents.Execute<T>(eventTarget, data, functo);
            if (processed)
            {
                break;
            }
            else
            {
                eventTarget = eventTarget.transform.parent?.gameObject;
            }
        }
        Debug.Log("     eventHandler: " + eventTarget);
        return eventTarget;
    }

    /// <summary>
    /// 获取一个拥有 RectTransform 的游戏对象，其中心点在屏幕上的坐标
    /// </summary>
    /// <param name="gameObject"></param>
    Vector2 GetCenterScrrenPoint(GameObject gameObject)
    {
        var rectTransform = gameObject.transform as RectTransform;
        var rect = rectTransform.rect;
        var centerPointAtLocalSpace = rect.center;
        var worldPosition = rectTransform.TransformPoint(centerPointAtLocalSpace);
        var screenCanvasCamera = UIEngine.Stuff.Canvas.worldCamera;
        var screenPoint = RectTransformUtility.WorldToScreenPoint(screenCanvasCamera, worldPosition);
        return screenPoint;
    }

    public void SetInputFieldText(string gameObjectName, string content)
    {
        this.Log($"SetInputFieldText: (gameObjectName: {gameObjectName}, content: {content})");
        var targetGo = FindGameObject(gameObjectName);
        if (targetGo == null)
        {
            throw new TestException($"SetInputFieldText: 没有找到游戏对象: {gameObjectName}");
        }
        var input = targetGo.GetComponentInChildren<InputField>();
        if (input == null)
        {
            throw new TestException($"SetInputFieldText: 没有找到 InputField 组件: {gameObjectName}");
        }
        input.text = content;
    }

    public GameObject FindGameObject(string searchPath)
    {
        var parts = searchPath.Split('/');
        GameObject hotGo = null;
        foreach(var p in parts)
        {
            if(hotGo == null)
            {
                hotGo = GameObject.Find(p);
            }
            else
            {
                hotGo = hotGo.transform.Find(p).gameObject;
            }
            if(hotGo == null)
            {
                return null;
            }
        }
        return hotGo;
    }


    /// <summary>
    /// 寻找游戏对象，并向其中的 button 组件传递点击事件
    /// </summary>
    /// <param name="gameObjectName">游戏对象名称</param>
    public async Task DragAsync(string gameObjectName, Vector2 dragShift, float dragDuration)
    {
        this.Log($"Drag: {gameObjectName}");


        var targetGo = FindGameObject(gameObjectName);
        if (targetGo == null)
        {
            throw new TestException($"Drag: 没有找到游戏对象: {gameObjectName}");
        }

        var testInfo = RaycastTest(targetGo);

        var eventTarget = SendEvent<IBeginDragHandler>(testInfo, ExecuteEvents.beginDragHandler);
        if(eventTarget == null)
        {
            eventTarget = SendEvent<IPointerDownHandler>(testInfo, ExecuteEvents.pointerDownHandler);
        }

        // pzy:
        // 显示点击效果
        //var touchFloting = UIEngine.Stuff.FindFloating<TouchEffectFloating>();
        //touchFloting.CreateEffect(testInfo.eventData.position);

        if (eventTarget == null)
        {
            return;
        }

        await Task.Delay(1);

        var data = testInfo.eventData;
        data.position += dragShift;
        ExecuteEvents.Execute<IDragHandler>(eventTarget, data, ExecuteEvents.dragHandler);

        var ts = TimeSpan.FromSeconds(dragDuration);
        await Task.Delay(ts);

        ExecuteEvents.Execute<IEndDragHandler>(eventTarget, data, ExecuteEvents.endDragHandler);
        ExecuteEvents.Execute<IPointerUpHandler>(eventTarget, data, ExecuteEvents.pointerUpHandler);

    }

    public async Task WaiteGameObjectAsync(string serachPath, float maxWaitSeconds)
    {
        this.Log($"WaiteGameObject: (serachPath: {serachPath}, maxWaitSeconds: {maxWaitSeconds})");
        var lostSeconds = 0f;
        while(true)
        {
            var go = FindGameObject(serachPath);
            if(go == null)
            {
                await Task.Delay(100);
                lostSeconds += 0.1f;
                if(lostSeconds >= maxWaitSeconds)
                {
                   throw new TestException($"WaiteGameObject: 等待 {serachPath} 时超时 ({maxWaitSeconds}s)");
                }
            }
            else
            {
                break;
            }
        }
    }
}
