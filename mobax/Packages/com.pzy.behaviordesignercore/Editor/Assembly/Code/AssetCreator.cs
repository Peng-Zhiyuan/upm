namespace BehaviorDesigner.Editor
{
    using System;
    using System.IO;
    using UnityEditor;
    using UnityEngine;

    public class AssetCreator : UnityEditor.EditorWindow
    {
        private AssetClassType m_ClassType;
        private string m_AssetName;

        private static string ActionTaskContents(string name)
        {
            return ("using UnityEngine;\nusing BehaviorDesigner.Runtime;\nusing BehaviorDesigner.Runtime.Tasks;\n\npublic class " + name + " : Action\n{\n\tpublic override void OnStart()\n\t{\n\t\t\n\t}\n\n\tpublic override TaskStatus OnUpdate()\n\t{\n\t\treturn TaskStatus.Success;\n\t}\n}");
        }

        private static string ConditionalTaskContents(string name)
        {
            return ("using UnityEngine;\nusing BehaviorDesigner.Runtime;\nusing BehaviorDesigner.Runtime.Tasks;\n\npublic class " + name + " : Conditional\n{\n\tpublic override TaskStatus OnUpdate()\n\t{\n\t\treturn TaskStatus.Success;\n\t}\n}");
        }

        public static void CreateAsset(System.Type type, string name)
        {
            ScriptableObject obj2 = ScriptableObject.CreateInstance(type);
            string assetPath = UnityEditor.AssetDatabase.GetAssetPath(UnityEditor.Selection.activeObject);
            if (assetPath == string.Empty)
            {
                assetPath = "Assets";
            }
            else if (Path.GetExtension(assetPath) != string.Empty)
            {
                assetPath = assetPath.Replace(Path.GetFileName(UnityEditor.AssetDatabase.GetAssetPath(UnityEditor.Selection.activeObject)), string.Empty);
            }
            UnityEditor.AssetDatabase.CreateAsset((UnityEngine.Object) obj2, UnityEditor.AssetDatabase.GenerateUniqueAssetPath(assetPath + "/" + name + ".asset"));
            UnityEditor.AssetDatabase.SaveAssets();
        }

        private static void CreateScript(string name, AssetClassType classType)
        {
            string assetPath = UnityEditor.AssetDatabase.GetAssetPath(UnityEditor.Selection.activeObject);
            if (assetPath == string.Empty)
            {
                assetPath = "Assets";
            }
            else if (Path.GetExtension(assetPath) != string.Empty)
            {
                assetPath = assetPath.Replace(Path.GetFileName(UnityEditor.AssetDatabase.GetAssetPath(UnityEditor.Selection.activeObject)), string.Empty);
            }
            string path = UnityEditor.AssetDatabase.GenerateUniqueAssetPath(assetPath + "/" + name + ".cs");
            StreamWriter writer = new StreamWriter(path, false);
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(path);
            string str4 = string.Empty;
            if (classType == AssetClassType.Action)
            {
                str4 = ActionTaskContents(fileNameWithoutExtension);
            }
            else if (classType == AssetClassType.Conditional)
            {
                str4 = ConditionalTaskContents(fileNameWithoutExtension);
            }
            else if (classType == AssetClassType.SharedVariable)
            {
                str4 = SharedVariableContents(fileNameWithoutExtension);
            }
            writer.Write(str4);
            writer.Close();
            UnityEditor.AssetDatabase.Refresh();
        }

        private void OnGUI()
        {
            this.m_AssetName = UnityEditor.EditorGUILayout.TextField("Name", this.m_AssetName, Array.Empty<GUILayoutOption>());
            UnityEditor.EditorGUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
            if (GUILayout.Button("OK", Array.Empty<GUILayoutOption>()))
            {
                CreateScript(this.m_AssetName, this.m_ClassType);
                base.Close();
            }
            if (GUILayout.Button("Cancel", Array.Empty<GUILayoutOption>()))
            {
                base.Close();
            }
            UnityEditor.EditorGUILayout.EndHorizontal();
        }

        private static string SharedVariableContents(string name)
        {
            string str = name.Remove(0, 6);
            string[] textArray1 = new string[13];
            textArray1[0] = "using UnityEngine;\nusing BehaviorDesigner.Runtime;\n\n[System.Serializable]\npublic class ";
            textArray1[1] = str;
            textArray1[2] = "\n{\n\n}\n\n[System.Serializable]\npublic class ";
            textArray1[3] = name;
            textArray1[4] = " : SharedVariable<";
            textArray1[5] = str;
            textArray1[6] = ">\n{\n\tpublic override string ToString() { return mValue == null ? \"null\" : mValue.ToString(); }\n\tpublic static implicit operator ";
            textArray1[7] = name;
            textArray1[8] = "(";
            textArray1[9] = str;
            textArray1[10] = " value) { return new ";
            textArray1[11] = name;
            textArray1[12] = " { mValue = value }; }\n}";
            return string.Concat(textArray1);
        }

        public static void ShowWindow(AssetClassType classType)
        {
            AssetCreator window = GetWindow<AssetCreator>(true, "Asset Name");
            Vector2 vector = new Vector2(300f, 55f);
            window.minSize = vector;
            window.minSize = vector;
            window.ClassType = classType;
        }

        private AssetClassType ClassType
        {
            set
            {
                this.m_ClassType = value;
                AssetClassType classType = this.m_ClassType;
                if (classType == AssetClassType.Action)
                {
                    this.m_AssetName = "NewAction";
                }
                else if (classType == AssetClassType.Conditional)
                {
                    this.m_AssetName = "NewConditional";
                }
                else if (classType == AssetClassType.SharedVariable)
                {
                    this.m_AssetName = "SharedNewVariable";
                }
            }
        }

        public enum AssetClassType
        {
            Action,
            Conditional,
            SharedVariable
        }
    }
}

