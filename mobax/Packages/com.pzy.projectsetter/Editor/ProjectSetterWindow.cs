using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System;

public class ProjectSetterWindow : EditorWindow
{
    [MenuItem("pzy.com.*/ProjectSetter/Window")]
    public static void OnShow()
    {
        var w = EditorWindow.GetWindow<ProjectSetterWindow>("Project Setter");
        w.Show();
    }

    static List<string> _settingsList; 
    static List<string> SettingsList
    {
        get
        {
            if (_settingsList == null)
            {
                var nameList = new List<string>();
                var root = ProjectSetter.settingsRoot;
                if (Directory.Exists(root))
                {
                    var dirList = Directory.GetDirectories(root);
                    foreach (var path in dirList)
                    {
                        var name = Path.GetFileName(path);
                        nameList.Add(name);
                    }
                }
                _settingsList = nameList;
            }
            return _settingsList;
        }
    }

    void ClearSettingsListCache()
    {
        _settingsList = null;
    }


    void OnGUI()
    {
        if(GUILayout.Button("Refresh"))
        {
            this.ClearSettingsListCache();
        }

        if(SettingsList.Count == 0)
        {
            GUILayout.Label("No Settings Found In " + ProjectSetter.settingsRoot);
        }
        else
        {
            EditorGUILayout.Separator();
            GUILayout.Label("Settings:");
            var nameList = SettingsList;
            foreach (var name in nameList)
            {
                var b = GUILayout.Button(name);
                if (b)
                {
                    ProjectSetter.Set(name);
                }
            }
        }


    }

    //[MenuItem("Tools/ProjectSetter/Alpha")]
    //public static void SetAlpha()
    //{
    //    ProjectSetter.Set("Alpha");
    //}

    //[MenuItem("Tools/ProjectSetter/Taptap")]
    //public static void SetTaptap()
    //{
    //    ProjectSetter.Set("Taptap");
    //}

    //[MenuItem("Tools/ProjectSetter/Bilibili")]
    //public static void SetBilibili()
    //{
    //    ProjectSetter.Set("Bilibili");
    //}

    //[MenuItem("Tools/ProjectSetter/Beta")]
    //public static void SetPts()
    //{
    //    ProjectSetter.Set("Beta");
    //}

    //[MenuItem("Tools/ProjectSetter/M4399")]
    //public static void SetM4399()
    //{
    //    ProjectSetter.Set("M4399");
    //}

    //[MenuItem("Tools/ProjectSetter/Gume")]
    //public static void SetGume()
    //{
    //    ProjectSetter.Set("Gume");
    //}

    //[MenuItem("Tools/ProjectSetter/test")]
    //public static void Test()
    //{
    //    ProjectSetter.Set("Assembly-CSharp");
    //}
}