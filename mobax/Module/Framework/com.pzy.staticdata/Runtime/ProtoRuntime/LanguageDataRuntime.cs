using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System;
using System.Reflection;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.Zip;
using System.IO;
using System.Reflection;


public static class LanguageDataRuntime 
{

    static Dictionary<string, byte[]> tableNameToBufferDic = new Dictionary<string, byte[]>();

    static bool isInitalizedInEditMode;
    static void OnEditModePopTableData(string tableName)
    {
        if(!isInitalizedInEditMode)
        {
            ReloadInEditMode();
            isInitalizedInEditMode = true;
        }
    }
    public static Func<byte[]> editModeRequestDataHandler;
    public static void ReloadInEditMode()
    {
        if (editModeRequestDataHandler == null)
        {
            throw new Exception("[LanguageDataRuntime] editModeRequestDataHandler not set");
        }

        var fileData = editModeRequestDataHandler.Invoke();
        var tableNameToBufferDic = UnzipFileDataToTableDataDic(fileData);
        Reset(tableNameToBufferDic);
        Debug.Log("[LanguageDataRuntime] data loaded");
    }

    public static bool dataFilled;

    public static byte[] PopTableData(string tableName)
    {
        if(!Application.isPlaying)
        {
            OnEditModePopTableData(tableName);
        }

        if (tableNameToBufferDic.ContainsKey(tableName))
        {
            var buffer = tableNameToBufferDic[tableName];
            tableNameToBufferDic.Remove(tableName);
            return buffer;
        }
        else
        {
            throw new Exception($"[LanguageDataRuntime] table '{tableName}' buffer not exsists. maybe already poped");
        }
    }


    public static Func<Task<byte[]>> requestLanguageDataHandler;
    public static async Task ReloadLanguageDataAsync()
    {
        if (requestLanguageDataHandler == null)
        {
            throw new Exception("[LanguageDataRuntime] requestLanguageDataHandler not set");
        }

        var task = requestLanguageDataHandler.Invoke();
        var fileData = await task;
        var tableNameToBufferDic = UnzipFileDataToTableDataDic(fileData);
        Reset(tableNameToBufferDic);
        Debug.Log("[LanguageDataRuntime] data loaded");
    }


    public static Func<Task<byte[]>> requestDataHandler;
    public static async Task ReloadAsync()
    {
        if (requestDataHandler == null)
        {
            throw new Exception("[LanguageDataRuntime] requestDataHandler not set");
        }

        var task = requestDataHandler.Invoke();
        var fileData = await task;
        var tableNameToBufferDic = UnzipFileDataToTableDataDic(fileData);
        Reset(tableNameToBufferDic);
        Debug.Log("[LanguageDataRuntime] data loaded");
    }

    static  Dictionary<string, byte[]> UnzipFileDataToTableDataDic(byte[] fileData)
    {
        var steam = new MemoryStream(fileData);
        var zipFile = new ZipFile(steam);
        var e = zipFile.GetEnumerator();
        var nameToBuffer = new Dictionary<string, byte[]>();
        while (e.MoveNext())
        {
            var entry = e.Current as ZipEntry;
            if (entry.IsFile)
            {
                var size = entry.Size;
                var name = entry.Name;
                using (var stream = zipFile.GetInputStream(entry))
                {
                    var reader = new BinaryReader(stream);
                    var buffer = reader.ReadBytes((int)size);
                    var nameWithoutExtension = Path.GetFileNameWithoutExtension(name);
                    nameToBuffer[nameWithoutExtension] = buffer;
                }
            }
        }
        steam.Dispose();
        return nameToBuffer;

    }

    static void Reset(Dictionary<string, byte[]> theTableNameToBufferDic)
    {
        Clean();
        foreach (var kv in theTableNameToBufferDic)
        {
            var tableName = kv.Key;
            var buffer = kv.Value;
            tableNameToBufferDic[tableName] = buffer;
        }
        dataFilled = true;
        Debug.Log("[LanguageDataRuntime] reseted");
    }


    public static void Clean()
    {
        // 移除 buffer 暂存
        tableNameToBufferDic.Clear();

        // 移除已生成的表对象
        var assembly = FindCsharpAssembly();
        if(assembly != null)
        {
            var genType = assembly.GetType("LanguageData");
            var files = genType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            foreach(var f in files)
            {
                var name = f.Name;
                if (name.StartsWith("_") && name.EndsWith("Table"))
                {
                    f.SetValue(null, null);
                    //Debug.Log($"[LanguageDataRuntime] clean field '{name}'");
                }
            }
        }
    }

    static Assembly FindCsharpAssembly()
    {
        var list = AppDomain.CurrentDomain.GetAssemblies();
        foreach(var one in list)
        {
            var name = one.GetName().Name;
            if(name == "Assembly-CSharp")
            {
                return one;
            }
        }
        return null;
    }
}
