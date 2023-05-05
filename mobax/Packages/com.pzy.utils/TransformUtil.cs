using UnityEngine;

public static class TransformUtil
{
    public static Transform FindChild(Transform trans, string goName)
    {
        Transform child = trans.Find(goName);
        if (child != null)
            return child;
        Transform go = null;
        for (int i = 0; i < trans.childCount; i++)
        {
            child = trans.GetChild(i);
            go = FindChild(child, goName);
            if (go != null)
                return go;
        }
        return null;
    }

    public static void RemoveAllChildren(Transform a)
    {
        if (a == null)
        {
            return;
        }
        for (int i = 0; i < a.childCount; i++)
        {
            var go = a.GetChild(i).gameObject;
            if (Application.isPlaying)
            {
                GameObject.Destroy(go);
            }
            else
            {
                GameObject.DestroyImmediate(go);
            }
        }
        a.DetachChildren();
    }

    public static void HideAllChildren(Transform t)
    {
        if (t == null)
        {
            return;
        }
        for (int i = 0; i < t.childCount; i++)
        {
            Transform child = t.GetChild(i);
            child.gameObject.SetActive(false);
        }
    }

    public static void SetLocalScale2(this Transform tf, float scale)
    {
        tf.localScale = Vector3.one * scale;
    }

    public static void CustomSetParent2(this Transform tf, Transform parent, Vector3 position = default, Vector3? localScale = null)
    {
        tf.SetParent(parent);
        tf.localScale = localScale != null ? (Vector3)localScale : Vector3.one;
        tf.localPosition = position;
        tf.rotation = Quaternion.Euler(Vector3.zero);
    }

    public static bool InitTransformInfo(GameObject obj, Transform parent)
    {
        if (obj == null)
        {
            Debug.LogError("The obj is null");
            return false;
        }
        obj.transform.SetParent(parent);
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localRotation = Quaternion.identity;
        obj.transform.localScale = Vector3.one;
        return true;
    }

    public static bool InitTransformInfo(Transform trans)
    {
        if (trans == null)
        {
            Debug.LogError("The obj is null");
            return false;
        }
        trans.transform.localPosition = Vector3.zero;
        trans.transform.localRotation = Quaternion.identity;
        trans.transform.localScale = Vector3.one;
        return true;
    }

    public static Vector3 GetLookAtEuler(Vector3 selfPos, Vector3 targetPos)
    {
        Vector3 forwardDir = targetPos - selfPos;
        Quaternion lookAtRot = Quaternion.LookRotation(forwardDir);
        Vector3 resultEuler = lookAtRot.eulerAngles;
        return resultEuler;
    }
}