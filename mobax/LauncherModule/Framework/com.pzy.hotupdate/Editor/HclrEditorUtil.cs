using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using HybridCLR.Editor;
using System.IO;
using CustomLitJson;
using UnityEditor.Build.Player;
using HybridCLR.Editor.Commands;
using Ionic.Zlib;
public static class HclrEditorUtil
{
    public static bool IsHclrEnabled
    {
        get
        {
            return SettingsUtil.Enable;
        }
        set
        {
            var settings = SettingsUtil.GlobalSettings;
            if (settings.enable == value)
            {
                return;
            }
            settings.enable = value;
            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssets();
        }
    }

    static string[] GetAllAotDellNmeList(BuildTarget target)
    {
        var ret = new List<string>();
        var fromDir = SettingsUtil.GetAssembliesPostIl2CppStripDir(target);
        var pathList = Directory.GetFiles(fromDir);
        foreach(var one in pathList)
        {
            var name = Path.GetFileNameWithoutExtension(one);
            ret.Add(name);
        }
        return ret.ToArray();
    }

    /// <summary>
    /// 将上一次构建 Hclr 保存的已裁剪后的程序集，其中需要补充元数据的那些，输出到目标目录
    /// </summary>
    /// <param name="toDir"></param>
    public static void OutputAotMetadataNeededStrippedDllToNativeProject(BuildTarget target, string nativeAssetDir, bool allAotDll = false)
    {
        var fromDir = SettingsUtil.GetAssembliesPostIl2CppStripDir(target);
        //var aotMetadatDll = SettingsUtil.AOTMetaAssemblies;
        var setting = SettingsUtil.GetSingletonAssets<HotUpdateAssemblyManifest>();
        var aotMetadatDll = setting.AOTMetadataDlls;
        if(allAotDll)
        {
            aotMetadatDll = GetAllAotDellNmeList(target);
        }

        var toDir = $"{nativeAssetDir}/AotAssembly";

        if(Directory.Exists(toDir))
        {
            Directory.Delete(toDir, true);
        }
        Directory.CreateDirectory(toDir);

        List<string> copiedDllList = new List<string>();
        foreach(var one in aotMetadatDll)
        {
            var from = $"{fromDir}/{one}.dll";
            var to = $"{toDir}/{one}.dll";
            if(File.Exists(from))
            {
                Debug.Log($"copy strip dll {one} ==> {to}");
                File.Copy(from, to, true);
                copiedDllList.Add(one + ".dll");
            }
        }

        // 生成目录
        var jd = new JsonData();
        foreach (var one in copiedDllList)
        {
            jd.Add(one);
        }
        var json = JsonMapper.Instance.ToJson(jd);
        File.WriteAllText(toDir + "/index.json", json);

    }


    public static List<string> CopyHotfixDll(BuildTarget target, string toDir, bool zip = true, string ext = ".data", bool includePdb = true)
    {
        //Directory.Delete(toDir, true);
        //Directory.CreateDirectory(toDir);
        var ret = new List<string>();
        if(!Directory.Exists(toDir))
        {
            Directory.CreateDirectory(toDir);
        }

        var fromDir = SettingsUtil.GetHotFixDllsOutputDirByTarget(target);
        var hotDllList = SettingsUtil.HotUpdateAssemblyNames;
        //foreach(var one in hotDllList)
        for(int i = 0; i < hotDllList.Count; i++)
        {
            {
                var one = hotDllList[i];
                var from = $"{fromDir}/{one}.dll";
                var fileName = GetFileName(i);
                var to = $"{toDir}/{fileName}{ext}";
                //File.Copy(from, to, true);
                if (zip)
                {
                    GzipTo(from, to);
                }
                else
                {
                    File.Copy(from, to, true);
                }

                ret.Add(to);
            }


            if(includePdb)
            {
                var one = hotDllList[i];
                var from = $"{fromDir}/{one}.pdb";
                var fileName = GetFileName(i);
                var to = $"{toDir}/{fileName}.pdb{ext}";
                //File.Copy(from, to, true);
                if (zip)
                {
                    GzipTo(from, to);
                }
                else
                {
                    File.Copy(from, to, true);
                }
            }
        }

        AssetDatabase.Refresh();
        return ret;
    }

    static char GetFileName(int index)
    {
        var ch = 'a';
        var ret = ch + index;
        return (char)ret;
    }

    static void GzipTo(string from, string to)
    {
        var data = File.ReadAllBytes(from);
        var zipedData = GZipStream.CompressBuffer(data);
        File.WriteAllBytes(to, zipedData);
    }


}
