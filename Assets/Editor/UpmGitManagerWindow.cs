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
                var isTagExists = info.isReleaseTagExists;

                EditorGUILayout.BeginHorizontal();

                // 显示标签
                EditorGUILayout.LabelField($"{name} {version}");

                // package 文件引用
                var packageAsset = info.PackageAsset;
                EditorGUILayout.ObjectField(packageAsset, typeof(TextAsset), false);

                // 发布按钮
                if(isTagExists == null)
                {
                    GUILayout.Label("...");
                }
                else if(isTagExists == true)
                {
                    GUILayout.Label(" ");
                }
                else if(isTagExists == false)
                {
                    if (GUILayout.Button("Release"))
                    {
                        info.TryCreateReleaseTagInBackground();
                    }
                }

                // 是否已发布到 openupm
                var onOpenUpm = info.isReleasedOnOpenUpm;
                if (onOpenUpm == null)
                {
                    GUILayout.Label("...");
                }
                else if (onOpenUpm == true)
                {
                    GUILayout.Label(" ");
                }
                else if (onOpenUpm == false)
                {
                    GUILayout.Label("Not Found On OpenUpm");
                }


                EditorGUILayout.EndHorizontal();

               
            }
        }
    }
}
