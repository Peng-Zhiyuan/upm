using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using UnityEditor;
using System.Text;


public class ProtoCompiler : AssetPostprocessor
{
    private const string PATH2 = "PuertsExt/Auto Compile Proto To Js";


    //[MenuItem(PATH2)]
    //public static void ToggleAutoFormat()
    //{
    //    var b = AutoCompileToJs;
    //    var newValue = !b;
    //    AutoCompileToJs = newValue;
    //}

    //[MenuItem(PATH2, true)]
    //public static bool ToggleAutoFormatValidate()
    //{
    //    var isChecked = AutoCompileToJs;
    //    Menu.SetChecked(PATH2, isChecked);
    //    return true;
    //}

    //static bool AutoCompileToJs
    //{
    //    set
    //    {
    //        PlayerPrefs.SetInt(PATH2, value ? 1 : 0);
    //    }
    //    get
    //    {
    //        var value = PlayerPrefs.GetInt(PATH2, 0);
    //        return value == 1;
    //    }
    //}

    //public static void OnPostprocessAllAssets(string[] importedAsset, string[] deleteAsset, string[] movedAssets, string[] movedFromAssetPaths)
    //{
    //    if(AutoCompileToJs)
    //    {
    //        var hasProtoChanged = false;
    //        foreach (var path in importedAsset)
    //        {
    //            var ext = Path.GetExtension(path);
    //            if (ext == ".proto")
    //            {
    //                hasProtoChanged = true;
    //                break;
    //            }
    //        }
    //        if (!hasProtoChanged)
    //        {
    //            foreach (var path in deleteAsset)
    //            {
    //                var ext = Path.GetExtension(path);
    //                if (ext == ".proto")
    //                {
    //                    hasProtoChanged = true;
    //                    break;
    //                }
    //            }
    //        }

    //        if (hasProtoChanged)
    //        {
    //            CompileAllProtoFileToJs();
    //        }
    //    }


    //}

    public static int Bash(string command, string workingDir)
    {
        return ExecUtil.Run("bash", $"-c \"{command}\"", false, workingDir);
    }


    public static void CompileToCsharp(string protoFilePath)
    {
        if(Application.platform == RuntimePlatform.WindowsEditor)
        {
            var protoParentDir = Path.GetDirectoryName(protoFilePath);
            var fileName = Path.GetFileName(protoFilePath);
            var currentDir = System.Environment.CurrentDirectory;
            var protogenPath = $"{currentDir}/Packages/com.pzy.protobuf/protogen~/protogen.exe";
            var genCsharpDir = $"{currentDir}/Assets/ProtoGen/Csharp";
            ExecUtil.Run(protogenPath, $"--csharp_out={genCsharpDir} {fileName}", false, protoParentDir);

            var csFileName = Path.ChangeExtension(fileName, ".cs");
            Debug.Log("csFile: " + csFileName);
            //var genFilePath = $"{genCsharpDir}/{csFileName}";

            //CSharpCodeUtil.AddNamespaceToFile(genFilePath, "ProtoBuf");

            AssetDatabase.Refresh();
        }
        else if(Application.platform == RuntimePlatform.OSXEditor)
        {
            var protoParentDir = Path.GetDirectoryName(protoFilePath);
            var fileName = Path.GetFileName(protoFilePath);
            var currentDir = System.Environment.CurrentDirectory;
            var protogenPath = $"{currentDir}/Packages/com.pzy.protobuf/protogen~/protogen.exe";
            var genCsharpDir = $"{currentDir}/Assets/ProtoGen/Csharp";
            //ExecUtil.Run(protogenPath, $"--csharp_out={genCsharpDir} {fileName}", false, protoParentDir);
            Bash($"export PATH=\\$PATH:/Library/Frameworks/Mono.framework/Versions/Current/Commands; mono {protogenPath} --csharp_out={genCsharpDir} {fileName}", protoParentDir);


            var csFileName = Path.ChangeExtension(fileName, ".cs");
            Debug.Log("csFile: " + csFileName);
            //var genFilePath = $"{genCsharpDir}/{csFileName}";

            //CSharpCodeUtil.AddNamespaceToFile(genFilePath, "ProtoBuf");

            AssetDatabase.Refresh();
        }
    }

    public static void CompileAllProtoFileToJs()
    {
        var guidList = AssetDatabase.FindAssets("t:protoAsset");
        var pathList = new List<string>();
        foreach(var guid in guidList)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            pathList.Add(path);
        }

        var dirToTrueDic = new Dictionary<string, bool>();
        foreach(var path in pathList)
        {
            var dir = Path.GetDirectoryName(path);
            dirToTrueDic[dir] = true;
        }

        var parentDirList = new List<string>();
        foreach(var kv in dirToTrueDic)
        {
            var path = kv.Key;
            parentDirList.Add(path);
        }

        foreach(var parentDir in parentDirList)
        {
            var dirName = Path.GetFileName(parentDir);
            var nodeModuleName = dirName;
            var jsOutFile = Application.dataPath + $"/ProtoGen/$js/Modules/ProtoGen/{nodeModuleName}.js";
            CreateParentDirIfNeed(jsOutFile);

            // 创建 js 文件
            {
                //var execPath = "C:/Users/igg/AppData/Roaming/npm/pbjs.cmd";
                var execPath = LocalTools.RequireTool("pbjs", "this tools provide by npm protobufjs pakcgae");

                var code = ExecUtil.Run(execPath, $"-t static-module -w commonjs -o {jsOutFile} *.proto", false, parentDir);
                if(code != 0)
                {
                    throw new Exception("error in pbjs");
                }
            }

            // 创建 Typing 模块
            {
                var typingFilePath = Application.dataPath + $"/../TsProj/Modules/ProtoGen/{nodeModuleName}.d.ts";
                CreateParentDirIfNeed(typingFilePath);

                //var execPath = "C:/Users/igg/AppData/Roaming/npm/pbts.cmd";
                var execPath = LocalTools.RequireTool("pbts", "this tools provide by npm protobufjs pakcgae");
                var code = ExecUtil.Run(execPath, $"-o {typingFilePath} {jsOutFile}", false, parentDir);
                if(code != 0)
                {
                    throw new Exception("error in pbts");
                }
            }

            // 修改 js 为 js.txt
            var target = jsOutFile + ".txt";
            if(File.Exists(target))
            {
                File.Delete(target);
            }
            File.Move(jsOutFile, target);
        }

        AssetDatabase.Refresh();

    }

    static void CreateParentDirIfNeed(string path)
    {
        var dir = Path.GetDirectoryName(path);
        if(!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }
    }
}
