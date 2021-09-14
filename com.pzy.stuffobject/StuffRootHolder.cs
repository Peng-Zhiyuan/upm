using UnityEngine;
using System.Collections;

public static class StuffRootHolder
{
    private static GameObject _stuffRoot;
    public static GameObject StuffRoot
    {
        get
        {
            if (_stuffRoot == null)
            {
                _stuffRoot = new GameObject();
                _stuffRoot.name = "StuffRoot";
                GameObject.DontDestroyOnLoad(_stuffRoot);
            }
            return _stuffRoot;
        }
    }
}
