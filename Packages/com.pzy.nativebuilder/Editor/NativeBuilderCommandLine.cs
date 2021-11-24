using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;
using NativeBuilder;
using System.Text;
using System.IO;

public class NativeBuilderCommandLine 
{
    /// <summary>
    /// 命令行统一编译方法(的CLI接口)
    /// </summary>
    /// 
    public static void Build()
    {
        // 默认参数
        var id = "";
        var scene = "";
        var productName = "";
        var platform = "";
        var il2cpp = "";
        var exportAndroidProject = "";
        var version = "";
        var androidVersionCode = "";
        var developmentMode = "";
        var obb = "";

        // 自动设置导出安卓工程的密钥的参数
        //PlayerSettings.Android.keystorePass = "ginicats";
        //PlayerSettings.Android.keyaliasPass = "ginicats";
        
        // 处理命令行选项
        CommandLineUtil.ReadOptions((option, arg) =>
            {
                switch(option)
                {
                case "--id":
                    id = arg;
                    break;
                case "--scene":
                    scene = arg;
                    break;
                case "--productName":
                    productName = arg;
                    break;
                case "--platform":
                    platform = arg;
                    break;
                case "--il2cpp":
                    il2cpp = arg;
                    break;
                case "--exportAndroidProject":
                    exportAndroidProject = arg;
                    break;
                case "--version":
                    version = arg;
                    break;
                case "--androidVersionCode":
                    androidVersionCode = arg;
                    break;
                case "--developmentMode":
                    developmentMode = arg;
                    break;
                case "--obb":
                    obb = arg;
                    break;
                }

            });

        // MonoAdapterBuilder.BuildMonoAdapter();

        //DeleteOdinAotDll();
        
        // swtich platform
        if(platform != "")
        {
            SetPlatform(platform);
        }

        // change id
        if(id != "")
        {
            SetId(id);
        }

        // change scene
        if(scene != "")
        {
            SetPackageScene(scene);
        }

        // change productName
        if(productName != "")
        {
            SetProductName(productName);
        }

        if(il2cpp != "")
        {
            if(il2cpp == "true")
            {
                SetIl2Cpp(true);
            }
            else
            {
                SetIl2Cpp(false);
            }
        }

        if(exportAndroidProject != "")
        {
            if(exportAndroidProject == "true")
            {
                SetExportAndroidProject(true);
            }
            else
            {
                SetExportAndroidProject(false);
            }
        }

        if(version != "")
        {
            SetVersion(version);
        }

        if(androidVersionCode != "")
        {
            var intCode = int.Parse(androidVersionCode);
            SetAndroidVersionCode(intCode);
        }

        if(obb != "")
        {
            var b = bool.Parse(obb);
            SetObbOption(b);
        }

        // generate lua call csharp wraper and hotfix class wraper/
        //CSObjectWrapEditor.Generator.ClearAll();
        //CSObjectWrapEditor.Generator.GenAll();

        // 设置日志的追踪信息
        SetLogTraceRules();

        // 调用过气 NativeBuilder 编译
        switch (EditorUserBuildSettings.activeBuildTarget)
        {
            case BuildTarget.Android:
            {
                AndroidBuild.Build(developmentMode == "true");
                break;
            }
            case BuildTarget.iOS:
            {
                IOSBuild.Build(developmentMode == "true");
                break;
            }
            case BuildTarget.WebGL:
            {
                WebGLBuild.Build(developmentMode == "true");
                break;
            }
                // 在这里添加更多的编译平台...

        }
    }

    // 设置 log 的追踪信息
    // 只有异常才包含对战信息，其他类型则只打印内容
    // 此举在于优化某些库中输出的大量 log
    static void SetLogTraceRules()
    {
        PlayerSettings.SetStackTraceLogType(LogType.Assert, StackTraceLogType.None);
        PlayerSettings.SetStackTraceLogType(LogType.Error, StackTraceLogType.None);
        PlayerSettings.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
        PlayerSettings.SetStackTraceLogType(LogType.Warning, StackTraceLogType.None);
        PlayerSettings.SetStackTraceLogType(LogType.Exception, StackTraceLogType.ScriptOnly);
    }

    static void SetObbOption(bool b)
    {
        PlayerSettings.Android.useAPKExpansionFiles = b;
    }

    static void SetAndroidVersionCode(int code)
    {
        PlayerSettings.Android.bundleVersionCode = code;
    }

    static void SetVersion(string v)
    {
        PlayerSettings.bundleVersion = v;
    }

    static void SetExportAndroidProject(bool b)
    {
        EditorUserBuildSettings.exportAsGoogleAndroidProject = b;
    }

    static AndroidBuildSystem ParseAndroidBuildSystem(string str)
    {
        if(str == "internal")
        {
            return AndroidBuildSystem.Gradle;
        }
        else if(str == "gradle")
        {
            return AndroidBuildSystem.Gradle;
        }
        else if(str == "visualStudio")
        {
            return AndroidBuildSystem.VisualStudio;
        }
        throw new Exception("unsupport AndroidBuildSystem: " + str);
    }

    static void SetPlatform(string name)
    {
        if(name == "android")
        {
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
        }
        else if(name == "ios")
        {
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.iOS, BuildTarget.iOS);
        }
        else if(name == "webGL")
        {
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.WebGL, BuildTarget.WebGL);
        }
        else
        {
            throw new Exception("unsupport platform: " + name);
        }
       
    }

    static void SetProductName(string name)
    {
        PlayerSettings.productName = name;
    }

    static void SetId(string id)
    {
        var activeTargetGroup = ConvertBuildTargetToGroup(EditorUserBuildSettings.activeBuildTarget);
        PlayerSettings.SetApplicationIdentifier(activeTargetGroup, id);
    }

    static void SetPackageScene(string name)
    {
        var list = EditorBuildSettings.scenes;
        foreach (var s in list)
        {
            var n = Path.GetFileNameWithoutExtension(s.path);
            if(n == name)
            {
                s.enabled = true;
            }
            else
            {
                s.enabled = false;
            }
        }
        EditorBuildSettings.scenes = list;
    }

    static void SetIl2Cpp(bool b)
    {
        if(b)
        {
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.iOS, ScriptingImplementation.IL2CPP);
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
        } 
        else 
        {
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.iOS, ScriptingImplementation.Mono2x);
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.Mono2x);
        }

        // 奥丁 il2cpp 相关
        //var path = "Assets\\Plugins\\Sirenix\\Odin Inspector\\Config\\Editor\\AOTGenerationConfig.asset";
        //var assets = AssetDatabase.LoadAssetAtPath<Sirenix.Serialization.AOTGenerationConfig>(path);
        
        //if(b){
        //    assets.AutomateBeforeBuilds = true;
        //}
        //else{
        //    assets.AutomateBeforeBuilds = false;
        //}
    }

    //static void DeleteOdinAotDll(){
    //    string fileName = @"Assets\Plugins\Sirenix\Assemblies\AOT\Sirenix.Serialization.AOTGenerated.dll";

    //    Debug.Log(File.Exists(fileName));
    //    //文件是否存在
    //    if (File.Exists(fileName))
    //    {
    //        Debug.Log("Delete Odin aot dll file");
    //        //删除文件
    //        File.Delete(fileName);
    //    }
    //}

    static BuildTargetGroup ConvertBuildTargetToGroup(BuildTarget buildTarget)
    {
        switch (buildTarget)
        {
            case BuildTarget.StandaloneOSX:
            case BuildTarget.iOS:
                return BuildTargetGroup.iOS;
            case BuildTarget.StandaloneWindows:
            // case BuildTarget.StandaloneLinux:
            case BuildTarget.StandaloneWindows64:
            case BuildTarget.StandaloneLinux64:
            // case BuildTarget.StandaloneLinuxUniversal:
                return BuildTargetGroup.Standalone;
            case BuildTarget.Android:
                return BuildTargetGroup.Android;
            case BuildTarget.WebGL:
                return BuildTargetGroup.WebGL;
            case BuildTarget.WSAPlayer:
                return BuildTargetGroup.WSA;
            // case BuildTarget.Tizen:
            //     return BuildTargetGroup.Tizen;
            // case BuildTarget.PSP2:
            //     return BuildTargetGroup.PSP2;
            case BuildTarget.PS4:
                return BuildTargetGroup.PS4;
            // case BuildTarget.PSM:
            //     return BuildTargetGroup.PSM;
            case BuildTarget.XboxOne:
                return BuildTargetGroup.XboxOne;
            // case BuildTarget.N3DS:
            //     return BuildTargetGroup.N3DS;
            // case BuildTarget.WiiU:
            //     return BuildTargetGroup.WiiU;
            case BuildTarget.tvOS:
                return BuildTargetGroup.tvOS;
            case BuildTarget.Switch:
                return BuildTargetGroup.Switch;
            case BuildTarget.NoTarget:
            default:
                return BuildTargetGroup.Standalone;
        }
    }

}
