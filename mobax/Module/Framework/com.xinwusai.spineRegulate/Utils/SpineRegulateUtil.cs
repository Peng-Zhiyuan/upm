using UnityEngine;

namespace SpineRegulate
{
    public class SpineRegulateUtil
    {
        public static void ClearAllChildren(GameObject obj)
        {
            if (obj == null)
            {
                return;
            }

            if (obj.transform.childCount <= 0) return;
            // 正序删除可能影响节点删不干净
            for (int i = obj.transform.childCount - 1; i >= 0; i--)
            {
                Object.DestroyImmediate(obj.transform.GetChild(i).gameObject);
            }
        }

        public static GameObject InstantiatePrefab(GameObject root, GameObject prefab)
        {
            GameObject obj = Object.Instantiate(prefab, root.transform, true);
            obj.name = prefab.name;
            if (root == null)
            {
                Debug.LogError("Cant find the root");
            }

            obj.transform.localPosition = Vector3.zero;
            obj.transform.localEulerAngles = Vector3.zero;
            obj.transform.localScale = Vector3.one;
            return obj;
        }
    }
}