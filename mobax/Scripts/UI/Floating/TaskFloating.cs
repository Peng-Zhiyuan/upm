using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public partial class TaskFloating : Floating
{
    public Text prefab_line;

    Dictionary<string, string> keyToDesDic = new Dictionary<string, string>();
    public void Add(string key, string des)
    {
        keyToDesDic[key] = des;
        this.Refresh();
        Debug.Log("[TaskFloating] add key: " + key);
    }

    public void Remove(string key)
    {
        this.keyToDesDic.Remove(key);
        this.Refresh();
        Debug.Log("[TaskFloating] remove key: " + key);
    }

    void Refresh()
    {
        this.prefab_line.gameObject.SetActive(false);
        TransformUtil.RemoveAllChildren(this.Pannel.transform);
        foreach(var kv in this.keyToDesDic)
        {
            var des = kv.Value;
            var line = GameObject.Instantiate(prefab_line);
            line.transform.parent = this.Pannel.transform;
            line.text = des;
            line.gameObject.SetActive(true);
        }
        var fitterList = this.Pannel.transform.GetComponentsInChildren<ContentSizeFitter>();
        foreach(var one in fitterList)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(one.GetComponent<RectTransform>());
        }
    }
}
