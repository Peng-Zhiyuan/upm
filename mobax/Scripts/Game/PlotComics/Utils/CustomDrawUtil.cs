using System;

namespace Plot.Runtime
{
#if UNITY_EDITOR
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// 这个是为了还原unity的显示设置 让配置更贴近
    /// </summary>
    public class CustomDrawUtil
    {
        private bool PickButton(bool value, GUIContent label,Action callback)
        {
            var button = GUILayout.Button(EditorGUIUtility.IconContent("Pick"));
            //     , new GUIStyle()
            // {
            //     fixedWidth = 20,
            //     fixedHeight = 20,
            // });
            if (button)
            {
                callback?.Invoke();
            }

            return button;
        }
    }
#endif
}