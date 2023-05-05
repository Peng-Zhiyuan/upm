using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Reflection;
using System;
using System.Text;

public static class FileManager
{
#if UNITY_EDITOR
    [UnityEditor.MenuItem("pzy.com.*/FileManager/Open Persisten Dir")]
    public static void OpenDir()
    {
        var path = Application.persistentDataPath;
        Application.OpenURL(path);
    }
#endif

    public static string PersistentPath
    {
        get
        {
            return Application.persistentDataPath;
            //if (Application.platform == RuntimePlatform.WindowsEditor)
            //{
            //    return "./FileManagerPersistent";
            //}
            //else
            //{
            //    return Application.persistentDataPath;
            //}
        }
    }

    public static string WriteBytes(string path, byte[] bytes)
    {
        var filePath = $"{PersistentPath}/{path}";
        var dir = Path.GetDirectoryName(filePath);
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }
        if (bytes == null)
        {
            File.Delete(filePath);
            return filePath;
        }
        File.WriteAllBytes(filePath, bytes);
        return filePath;
    }

    public static string WriteText(string path, string text)
    {
        var filePath = $"{PersistentPath}/{path}";
        var dir = Path.GetDirectoryName(filePath);
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }
        if (text == null)
        {
            File.Delete(filePath);
            return filePath;
        }
        File.WriteAllText(filePath, text);
        return filePath;
    }

    public static bool HasFile(string path)
    {
        var filePath = $"{PersistentPath}/{path}";
        var b = File.Exists(filePath);
        return b;
    }

    public static string[] GetFileList(string dirPath, string pattern = "*", SearchOption option = SearchOption.TopDirectoryOnly)
    {
        var root = $"{PersistentPath}/{dirPath}";
        if (Directory.Exists(root))
        {
            var list = Directory.GetFiles(root, pattern, option);
            for (int i = 0; i < list.Length; i++)
            {
                var str = list[i];
                str = str.Replace($"{PersistentPath}/", "");
                str = str.Replace('\\', '/');
                list[i] = str;
            }
            return list;
        }
        return new string[0];
    }

    public static string[] GetDirectoryList(string dirPath, string pattern = "*", SearchOption option = SearchOption.TopDirectoryOnly)
    {
        var root = $"{PersistentPath}/{dirPath}";
        if (Directory.Exists(root))
        {
            var list = Directory.GetDirectories(root, pattern, option);
            for(int i = 0; i < list.Length; i++)
            {
                var str = list[i];
                str = str.Replace($"{PersistentPath}/", "");
                str = str.Replace('\\', '/');
                list[i] = str;
            }

            return list;
        }
        return new string[0];
    }

    public static void DeleteFile(string path)
    {
        var diskPath = $"{PersistentPath}/{path}";
        if (File.Exists(diskPath))
        {
            File.Delete(diskPath);
        }
    }

    public static void DeleteDirectory(string path)
    {
        var diskPath = $"{PersistentPath}/{path}";
        if (Directory.Exists(diskPath))
        {
            Directory.Delete(diskPath, true);
        }
    }

    public static byte[] ReadBytes(string path)
    {
        var filePath = $"{PersistentPath}/{path}";
        if (!File.Exists(filePath))
        {
            return null;
        }
        var bytes = File.ReadAllBytes(filePath);
        return bytes;
    }

    public static string ReadText(string path)
    {
        var filePath = $"{PersistentPath}/{path}";
        if (!File.Exists(filePath))
        {
            return null;
        }
        var text = File.ReadAllText(filePath);
        return text;
    }

    /// <summary>
    /// 删除一个目录下的所有一级子目录
    /// </summary>
    public static void DeleteAllSubDir(string rootDir, string exludePath)
    {
        var list = FileManager.GetDirectoryList(rootDir);
        foreach (var path in list)
        {
            if (!path.StartsWith(exludePath))
            {
                Debug.Log($"[RemoteRes] delete {path}");
                FileManager.DeleteDirectory(path);
            }
        }
    }
}