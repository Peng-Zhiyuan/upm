//******************************************************
//
//	File Name 	: 		CtrGizmosEditor.cs
//	
//	Author  	:		dust Taoist

//	CreatTime   :		8/16/2019 18:14
//******************************************************

using System;
using System.Collections;
using System.Reflection;
using UnityEditor;

/// <summary>
/// Global control of gizmos display or off
/// </summary>
public class CtrGizmosEditor
{
    private static bool _globalGizmosOn;

    /*
 *  # instead of   shift  

    & instead of  alt

    % instead of Ctrl

 */
 
 [MenuItem("Tools/E3D-EditorTools/Export", false, 102)]
    static void ExportPackage()
    {
        var path = EditorUtility.SaveFilePanel("Save unitypackage", "", "", "unitypackage");
        if (path == "")
            return;

        var assetPathNames = new string[Selection.objects.Length];
        for (var i = 0; i < assetPathNames.Length; i++)
        {
            assetPathNames[i] = AssetDatabase.GetAssetPath(Selection.objects[i]);
        }

        assetPathNames = AssetDatabase.GetDependencies(assetPathNames);

        AssetDatabase.ExportPackage(assetPathNames, path, ExportPackageOptions.Interactive | ExportPackageOptions.Recurse | ExportPackageOptions.IncludeDependencies);
    }
	
    [MenuItem("Tools/E3D-EditorTools/Scene View/Toggle Gizmos &G")]
    private static void ToggleAllSceneGizmos()
    {
        _globalGizmosOn = !_globalGizmosOn;
        ToggleGizmos(_globalGizmosOn);
    }

    private static void DisableAllSceneGizmos()
    {
        _globalGizmosOn = false;
        ToggleGizmos(_globalGizmosOn);
    }

    private static void EnableAllSceneGizmos()
    {
        _globalGizmosOn = true;
        ToggleGizmos(_globalGizmosOn);
    }

    private static void ToggleGizmos(bool gizmosOn)
    {
        int val = gizmosOn ? 1 : 0;
        Assembly asm = Assembly.GetAssembly(typeof(Editor));
        Type type = asm.GetType("UnityEditor.AnnotationUtility");
        if (type != null)
        {
            MethodInfo getAnnotations = type.GetMethod("GetAnnotations", BindingFlags.Static | BindingFlags.NonPublic);
            MethodInfo setGizmoEnabled = type.GetMethod("SetGizmoEnabled", BindingFlags.Static | BindingFlags.NonPublic);
            MethodInfo setIconEnabled = type.GetMethod("SetIconEnabled", BindingFlags.Static | BindingFlags.NonPublic);
            var annotations = getAnnotations.Invoke(null, null);
            foreach (object annotation in (IEnumerable)annotations)
            {
                Type annotationType = annotation.GetType();
                FieldInfo classIdField = annotationType.GetField("classID", BindingFlags.Public | BindingFlags.Instance);
                FieldInfo scriptClassField = annotationType.GetField("scriptClass", BindingFlags.Public | BindingFlags.Instance);
                if (classIdField != null && scriptClassField != null)
                {
                    int classId = (int)classIdField.GetValue(annotation);
                    string scriptClass = (string)scriptClassField.GetValue(annotation);
                    setGizmoEnabled.Invoke(null, new object[] { classId, scriptClass, val });
                    setIconEnabled.Invoke(null, new object[] { classId, scriptClass, val });
                }
            }
        }
    }
}
