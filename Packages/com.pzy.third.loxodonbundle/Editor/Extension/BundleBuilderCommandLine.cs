using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;
using Loxodon.Framework.Bundles.Editors;
using System.Text;
using System.IO;

public class BundleBuilderCommandLine 
{
    /// <summary>
    /// 命令行统一编译方法(的CLI接口)
    /// </summary>
    /// 
    public static void Build()
    {
        // 命令行参数检查
        var args = System.Environment.GetCommandLineArgs ();

        // 默认参数
        var dataVersion = "";
        var copyToStreamingAssets = "";
        var useHashFilename = "";
        var buildTarget = "";
        var compression = "";
            
        // 处理命令行选项
        ReadOptions((option, arg) =>
            {
                switch(option)
                {
                case "--dataVersion":
                    dataVersion = arg;
                    break;
                case "--copyToStreamingAssets":
                    copyToStreamingAssets = arg;
                    break;
                case "--useHashFilename":
                    useHashFilename = arg;
                    break;
                case "--buildTarget":
                    buildTarget = arg;
                    break;
                case "--compression":
                    compression = arg; 
                    break;
                default:
                    Debug.Log("Unsupport arg: " + arg);
                    break;
                }

            });

        // // swtich platform
        // if(platform != "")
        // {
        //     SetPlatform(platform);
        // }

        var buildVM = new BuildVM();
        //buildVM.OnEnable();

        // change dataVersion
        if(dataVersion != "")
        {
            buildVM.DataVersion = dataVersion;
        }
        else
        {
            throw new Exception("need arg --dataVersion");
        }

        // change copyToStreamingAssets
        if(copyToStreamingAssets != "")
        {
            buildVM.CopyToStreaming = bool.Parse(copyToStreamingAssets);
        }
        else
        {
            throw new Exception("need arg --copyToStreamingAssets");
        }

        // change useHashFilename
        if(useHashFilename != "")
        {
            buildVM.UseHashFilename = bool.Parse(useHashFilename);
        }
        else
        {
            throw new Exception("need arg --useHashFilename");
        }

        // set compression
        if(compression != "")
        {
            if(compression == "LZMA")
            {
                buildVM.Compression = CompressOptions.StandardCompression;
            }
            else if(compression == "LZ4")
            {
                buildVM.Compression = CompressOptions.ChunkBasedCompression;
            }
            else if(compression == "NONE")
            {
                buildVM.Compression = CompressOptions.Uncompressed;
            }
            else
            {
                throw new Exception("unsupport --compression param : " + compression);
            }
        }
        else
        {
            throw new Exception("not provide --compression param");
        }
    
        // change platform
        if(buildTarget != "")
        {
            SetPlatform(buildTarget);
        }

        if(buildTarget != "")
        {
            if(buildTarget == "android")
            {
                buildVM.BuildTarget = BuildTarget.Android;
            }
            else if(buildTarget == "ios")
            {
                buildVM.BuildTarget = BuildTarget.iOS;
            }
            else
            {
                throw new Exception("unsupport --buildTarget " + buildTarget);
            }
        }
        else
        {
            throw new Exception("must provide arg --buildTarget");
        }
       

        buildVM.Build(false);
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
    }

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

    private static void ReadOptions(Action<string, string> onOption)
    {
        var args = System.Environment.GetCommandLineArgs ();
        for (int i = 0; i < args.Length; i++) {
            if (args [i].StartsWith("-", StringComparison.Ordinal)) {
                string option = args[i];
                string arg = "";
                if (i + 1 < args.Length)
                {
                    if (!args[i + 1].StartsWith("-", StringComparison.Ordinal))
                    {
                        arg = args[i + 1];
                    }
                }
                onOption(option, arg);
            }
        }
    }

}
