using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public partial class LauncherUiManager : SceneObject<LauncherUiManager>
{

    Canvas _canvas;
    public Canvas Canvas
    {
        get
        {
            if(_canvas == null)
            {
                _canvas = this.GetComponent<Canvas>();
            }
            return _canvas;
        }
    }

    public void Show(string name)
    {
        var prefab = Resources.Load<GameObject>(name);
        if (prefab == null)
        {
            throw new Exception("[LauncherUiManager] Can't find prefab: " + name);
        }
        var pageGo = GameObject.Instantiate(prefab, this.NormalLayer.transform);
        pageGo.name = name;
    }

    public T Show<T>(string fromResourceDir = null)
    {
        if (null != fromResourceDir)
        {
            if (!fromResourceDir.EndsWith("/"))
            {
                fromResourceDir = $"{fromResourceDir}/";
            }
        }
        
        var prefabName = typeof(T).Name;
        var fullPath = $"{fromResourceDir ?? ""}{prefabName}";
        var prefab = Resources.Load<GameObject>(fullPath);
        if (prefab == null)
        {
            throw new Exception("[LauncherUiManager] Can't find prefab: " + fullPath);
        }
        var pageGo = Instantiate(prefab, NormalLayer.transform);
        pageGo.name = prefabName;
        var page = pageGo.GetComponent<T>();
        return page;
    }

    public void Remove<T>()
    {
        var prefabName = typeof(T).Name;
        var page = NormalLayer.transform.Find(prefabName);
        if (page != null)
        {
            Destroy(page.gameObject);
        }
    }
}