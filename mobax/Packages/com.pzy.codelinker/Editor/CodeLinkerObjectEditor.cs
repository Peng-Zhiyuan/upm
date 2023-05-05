using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;

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

        //private void OnEnable()
        //{
        //    this.RebuildIsTsHasTag();
        //}


        //bool isTsHasTag;
        //void RebuildIsTsHasTag()
        //{
        //    this.isTsHasTag = CodeLinkerEditorTsUtils.IsAlreadyCodeLinkerCode(this.codeLinkerObject);
        //}


        public override void OnInspectorGUI()
        {

            // 现实关联链接代码
            //var className = codeLinkerObject.GetClassName();
            var mainComponent = CodeLinkerEditorCsUtils.GetMainComponent(this.codeLinkerObject.gameObject);
            if(mainComponent == null)
            {
                EditorGUILayout.LabelField("Main Component Not Found");
                return;
            }
            var className = mainComponent.GetType().Name;

            var filePath = CodeLinkerEditorCsUtils.ResolveCodeFilePath(className);
            //var monoScript = AssetDatabase.LoadAssetAtPath(filePath, typeof(MonoScript));
            EditorGUI.BeginDisabledGroup(true);
            //EditorGUILayout.ObjectField("Linker File", monoScript, typeof(MonoScript), false);
            EditorGUILayout.TextField("Class Name", className);
            EditorGUILayout.TextField("Linker File", filePath);
            EditorGUI.EndDisabledGroup();
            
        
            //base.OnInspectorGUI();
            if (!Application.isPlaying)
            {
               
                
                if (GUILayout.Button("GenerateCode"))
                {
                    CodeLinkerEditorCsUtils.GenerateCodeForTree(codeLinkerObject);
                }
                

            }
        }
            
    }
}