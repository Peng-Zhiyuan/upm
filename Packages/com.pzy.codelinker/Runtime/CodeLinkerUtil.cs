using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class CodeLinkerUtil
{
    private static void _GetComponentsInChildrenNoRescure<T>(Transform t, List<T> list)
    {
        if (!t.gameObject.activeSelf)
        {
            return;
        }
        var com = t.GetComponent<T>();
        if (com != null)
        {
            list.Add(com);
            return;
        }
        else
        {
            for (int i = 0; i < t.childCount; i++)
            {
                var child = t.GetChild(i);
                _GetComponentsInChildrenNoRescure<T>(child, list);
            }
        }
    }

    public static T[] GetComponentsInChildrenNoRescure<T>(Transform t)
    {
        var list = new List<T>();
        _GetComponentsInChildrenNoRescure(t, list);
        return list.ToArray();
    }

    // 寻找子transform, 路径不包括跟节点, 如果找不到报错
    public static Transform FindByPath(Transform tran, string path, bool canBeNull = false)
    {
        if (tran == null)
        {
            if (!canBeNull)
            {
                throw new System.Exception("params tran can't be null");
            }
            else
            {
                return null;
            }
        }
        if (string.IsNullOrEmpty(path))
        {
            return tran;
        }
        string[] nodes = path.Split('/');
        int index = 0;
        Transform tarTran = tran;
        while(index < nodes.Length)
        {
            string node = nodes[index];
            tarTran = tarTran.Find(node);
            if (tarTran == null)
            {
                break;
            }
            index++;
        }
        if (tarTran == null && !canBeNull)
        {
            throw new System.Exception("path: " + path + " not found in tranform: " + tran.name);
        }
        return tarTran;
    }
        

}
