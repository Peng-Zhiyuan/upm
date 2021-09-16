using UnityEngine;
using System.Collections;
using UnityEditor;

namespace ElementSystem
{

    [CustomEditor(typeof(CodeLinkerObject))]
    public class CodeLinkerObjectEditor : Editor 
    {

        CodeLinkerObject codeLinkerObject
        {
            get
            {
                return target as CodeLinkerObject;
            }
        }

        public override void OnInspectorGUI()
        {
            // 现实关联链接代码
            var className = codeLinkerObject.GetClassName();
            var filePath = CodeLinkerEditorUtils.ResolveCodeFilePath(className);
            //var monoScript = AssetDatabase.LoadAssetAtPath(filePath, typeof(MonoScript));
            EditorGUI.BeginDisabledGroup(true);
            //EditorGUILayout.ObjectField("Linker File", monoScript, typeof(MonoScript), false);
            EditorGUILayout.TextField("Liner File", filePath);
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
                        CodeLinkerEditorUtils.GenerateCodeForTree(codeLinkerObject);
                    }
                }

            }
        }
            
    }
}