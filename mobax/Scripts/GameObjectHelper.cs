using UnityEngine;

public class GameObjectHelper
{
    public static Transform FindChild(Transform trans, string goName)
    {
        if (goName.Equals("root")
            || goName.Equals("foot"))
        {
            return trans;
        }
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
}