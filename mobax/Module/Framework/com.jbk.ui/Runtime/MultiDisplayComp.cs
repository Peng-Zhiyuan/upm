using System;
using System.Collections.Generic;
using UnityEngine;

public class MultiDisplayComp : MonoBehaviour
{
    [Tooltip("设置选中时显示的图片")]
    public List<GameObject> list;

    public void HideAll()
    {
        foreach (var go in list)
        {
            go.SetActive(false);
        }
    }
    
    public void Show(params Component[] showList)
    {
        foreach (var go in list)
        {
            var find = Array.FindIndex(showList, comp => go == comp.gameObject) >= 0;
            go.SetActive(find);
        }
    }
}