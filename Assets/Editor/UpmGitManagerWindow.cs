using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class UpmGitManagerWindow : EditorWindow
{
    [MenuItem("UpmGitManager/Open Window")]
    static void OpenWindow()
    {
        var window = EditorWindow.GetWindow<UpmGitManagerWindow>("UpmGitManager", true);
        window.Show(true);
    }

    public void OnGUI()
    {
        var b = GUILayout.Button("Update Database");
        if(b)
        {
            UpmGitDatabase.UpdatePackageInfoInBackground();
        }


        //if (GUILayout.Button("Test Git Tag"))
        //{
        //    UpmGitUtil.IsGitTagExists("com.p");
        //}

        var status = UpmGitDatabase.status;
        EditorGUILayout.LabelField("staths: " + status);

     
        if(status == UpmGitDatabaseStatus.AllRight)
        {
            var packageInfoList = UpmGitDatabase.packageInfoList;
            foreach (var info in packageInfoList)
            {
                var name = info.nativePackageInfo.name;
                var displayName = info.nativePackageInfo.displayName;
                var version = info.nativePackageInfo.version;

                var releaseTagName = info.ReleaseTagName;
                var isTagExists = info.IsReleaseTagExists;

                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.LabelField($"{name} {version}");


                if (!isTagExists)
                {
                    if (GUILayout.Button("Release"))
                    {
                        var success = info.CreateReleaseTag();
                        if(success)
                        {
                            info.IsReleaseTagExists = true;
                        }
                    }
                }


                EditorGUILayout.EndHorizontal();

               
            }
        }
    }
}
