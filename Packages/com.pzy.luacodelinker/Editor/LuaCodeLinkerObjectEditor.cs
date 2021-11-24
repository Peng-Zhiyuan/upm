using UnityEngine;
using System.Collections;
using UnityEditor;

namespace ElementSystem
{

    [CustomEditor(typeof(LuaCodeLinkerObject))]
    public class LuaCodeLinkerObjectEditor : Editor 
    {

        LuaCodeLinkerObject codeLinkerObject
        {
            get
            {
                return target as LuaCodeLinkerObject;
            }
        }

        public override void OnInspectorGUI()
        {
            // 现实关联链接代码
            var className = codeLinkerObject.GetClassName();
            var filePath = LuaCodeLinkerEditorUtils.ResolveCodeFilePath(className);
            //var monoScript = AssetDatabase.LoadAssetAtPath(filePath, typeof(MonoScript));
            EditorGUI.BeginDisabledGroup(true);
            //EditorGUILayout.ObjectField("Linker File", monoScript, typeof(MonoScript), false);
            EditorGUILayout.TextField("Linker File", filePath);
            EditorGUI.EndDisabledGroup();

            //base.OnInspectorGUI();
            if (!Application.isPlaying)
            {
                // 配合 Unity 2018 的嵌套 Prefab 功能，因此不允许嵌套设计器
                //if(codeLinkerObject.ParentDesigner == null)
                {
                    //EditorGUILayout.Separator();
                    if (GUILayout.Button("GenerateCode"))
                    {
                        LuaCodeLinkerEditorUtils.GenerateCodeForTree(codeLinkerObject);
                    }
                }

            }
        }
            
    }
}