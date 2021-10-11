using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class TestGUI : MonoBehaviour
{
    public string title = "";
    public Queue<string> msgQueue = new Queue<string>();


    GUIStyle font = null;

    private void OnGUI()
    {
        if(font == null)
        {
            font = new GUIStyle();
            font.normal.background = null;    //设置背景填充
            font.normal.textColor = new Color(0.8f, 1, 0.8f);   //设置字体颜色
            font.fontSize = 30;       //字体大小
        }

        GUILayout.Label(title, font);

        var e = msgQueue.GetEnumerator();
        while(e.MoveNext())
        {
            var msg = e.Current;
            GUILayout.Label(msg, font);
        }
        e.Dispose();
    }

    public void AddMsg(string msg)
    {
        msgQueue.Enqueue(msg);
        if(msgQueue.Count > 3)
        {
            msgQueue.Dequeue();
        }
    }
}
