

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_IPHONE
using UnityEditor.iOS.Xcode;
using UnityEditor;
#endif

public class XcodeUtil : MonoBehaviour
{
    public static void SetEmbedSwiftToNo(string xcodeprojParentDirPath)
    {
#if UNITY_IPHONE
        // 打开 Xcode 工程
        var pbxPath = PBXProject.GetPBXProjectPath(xcodeprojParentDirPath);
        var project = new PBXProject();
        project.ReadFromFile(pbxPath);

        // 获取主 target 的名称和 GUID
        var mainTargetGuid = project.GetUnityMainTargetGuid();
        var frameworkTargetGuid = project.GetUnityFrameworkTargetGuid();

        // 关闭 Embed Swift 选项
        project.SetBuildProperty(mainTargetGuid, "ALWAYS_EMBED_SWIFT_STANDARD_LIBRARIES", "NO");
        project.SetBuildProperty(frameworkTargetGuid, "ALWAYS_EMBED_SWIFT_STANDARD_LIBRARIES", "NO");

        // 保存修改
        project.WriteToFile(pbxPath);
#endif
    }

    /// <summary>
    /// 权限中添加后台运行中的远程通知项目
    /// 后台运行下的其他子项目会被清空
    /// </summary>
    /// <param name="xcodeprojParentDirPath"></param>
    public static void AddCapability_UIBackgroundModesRemoteNotification(string xcodeprojParentDirPath)
    {
#if UNITY_IPHONE
        var plistPath = $"{xcodeprojParentDirPath}/Info.plist";
        var doc = new PlistDocument();
        doc.ReadFromFile(plistPath);

        var rootDict = doc.root;
        PlistElementArray urlArray = null;
        //Add BackgroundModes
        if (!rootDict.values.ContainsKey("UIBackgroundModes"))
        {
            urlArray = rootDict.CreateArray("UIBackgroundModes");
        }
        else
        {
            urlArray = rootDict.values["UIBackgroundModes"].AsArray();
        }   
        urlArray.values.Clear();
        urlArray.AddString("remote-notification");

        doc.WriteToFile(plistPath);

#endif
    }
}
