using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
public class UIComponentDict : MonoBehaviour
{
    [System.Serializable]
    public class Node
    {
        public string m_name;
        public string m_hierarchy;
        public Transform m_trans;
    }

    public List<Node> m_nodeList = new List<Node>();
    public List<Canvas> m_canvasList = new List<Canvas>();
    public bool m_refreshNow = false;

    private Transform m_cachedTrans;
    private Transform CachedTrans
    {
        get
        {
            if (m_cachedTrans == null) m_cachedTrans = transform;
            return m_cachedTrans;
        }
    }

    private Dictionary<string, Transform> m_nameDict = null;
    private Dictionary<string, Transform> NameDict
    {
        get
        {
            if (m_nameDict == null)
            {
                m_nameDict = new Dictionary<string, Transform>();
                foreach (Node node in m_nodeList)
                {
                    m_nameDict[node.m_name] = node.m_trans;
                }
            }
            return m_nameDict;
        }
    }

    private Dictionary<string, Transform> m_hierarchyDict = null;
    private Dictionary<string, Transform> HierarchyDict
    {
        get
        {
            if (m_hierarchyDict == null)
            {
                m_hierarchyDict = new Dictionary<string, Transform>();
                foreach (Node node in m_nodeList)
                {
                    m_hierarchyDict[node.m_hierarchy] = node.m_trans;
                }
            }
            return m_hierarchyDict;
        }
    }

    public Transform GetTransByName(string name)
    {
        Transform trans;
        if (!NameDict.TryGetValue(name, out trans)) return null;
        return trans;
    }

    public Transform GetTransByHierarchy(string name)
    {
        Transform trans;
        if (!HierarchyDict.TryGetValue(name, out trans)) return null;
        return trans;
    }

#if UNITY_EDITOR
    void Update()
    {
        if (Application.isPlaying) return;
        if (m_refreshNow)
        {
            m_refreshNow = false;
            DoRefresh(true);
        }
    }
#endif

    void DoRefresh(bool recursive)
    {
        if (m_refreshNow) m_refreshNow = false;
        m_nodeList.Clear();
        m_canvasList.Clear();
        Transform[] transes = GetComponentsInChildren<Transform>(true);
        foreach (Transform trans in transes)
        {
            GameObject go = trans.gameObject;
            //if (go == this.gameObject) continue;
            UIComponentDict dict = (trans.parent != null) ? FindInParents<UIComponentDict>(trans.parent) : FindInParents<UIComponentDict>(trans);
            //if (go.GetComponent<UIComponentDict>() == null
            //    && dict != this)
            if (go != this.gameObject && dict != this)
                continue;
            string hierarchy = UIComponentDict.GetHierarchy(trans, CachedTrans);
            m_nodeList.Add(new Node() { m_name = go.name, m_hierarchy = hierarchy, m_trans = trans });
        }
        m_nodeList.Sort((x, y) => { return string.Compare(x.m_name, y.m_name); });
        if (recursive)
        {
            UIComponentDict[] dicts = GetComponentsInChildren<UIComponentDict>(true);
            foreach (UIComponentDict dict in dicts)
            {
                if (dict != this) dict.DoRefresh(false);
            }
            m_canvasList.AddRange(GetComponentsInChildren<Canvas>(true));
        }
    }

    static public T FindInParents<T>(Transform trans) where T : Component
    {
        if (trans == null) return null;
#if UNITY_FLASH
        object comp = trans.GetComponent<T>();
#else
        T comp = trans.GetComponent<T>();
#endif
        if (comp == null)
        {
            Transform t = trans.transform.parent;

            while (t != null && comp == null)
            {
                comp = t.gameObject.GetComponent<T>();
                t = t.parent;
            }
        }
#if UNITY_FLASH
        return (T)comp;
#else
        return comp;
#endif
    }

    static public string GetHierarchy(Transform target, Transform root = null)
    {
        if (target == null || target == root) return "";
        string path = target.name;

        while (target.parent != null && target.parent != root)
        {
            target = target.parent;
            path = target.name + "/" + path;
        }
        return path;
    }
}
