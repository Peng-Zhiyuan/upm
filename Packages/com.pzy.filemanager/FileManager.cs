using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Reflection;
using System;
using System.Text;

public static class FileManager
{
    public static string PersistentPath
    {
        get
        {
            if (Application.platform == RuntimePlatform.WindowsEditor)
            {
                // 在 windows 编辑器上 unity 没有提供该路径，自己指定到项目文件夹内
                return "./FileManagerPersistent";
            }
            else
            {
                return Application.persistentDataPath;
            }    
        }
    }

    public static string Write(string path, IFilelizable obj)
    {
        var filePath = $"{PersistentPath}/{path}";
        var dir = Path.GetDirectoryName(filePath);
        if(!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }
        if(obj == null)
        {
            File.Delete(filePath);
            return filePath;
        }
        var bytes = obj.Seriliaze();
        File.WriteAllBytes(filePath, bytes);
        return filePath;
    }

    public static string WriteBytes(string path, byte[] bytes)
    {
        var filePath = $"{PersistentPath}/{path}";
        var dir = Path.GetDirectoryName(filePath);
        if(!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }
        if(bytes == null)
        {
            File.Delete(filePath);
            return filePath;
        }
        File.WriteAllBytes(filePath, bytes);
        return filePath;
    }

    public static bool HasFile(string path)
    {
        var filePath = $"{PersistentPath}/{path}";
        var b = File.Exists(filePath);
        return b;
    }

    public static string[] GetFileList(string dirPath)
    {
        var root = $"{PersistentPath}/{dirPath}";
        if (Directory.Exists(root))
        {
            var list = Directory.GetFiles(root);
            return list;
        }
        return new string[0];
    }

    public static string[] GetDirectoryList(string dirPath)
    {
        var root = $"{PersistentPath}/{dirPath}";
        if (Directory.Exists(root))
        {
            var list = Directory.GetDirectories(root);
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

    public static T Read<T>(string path) where T : IFilelizable
    {
        var filePath = $"{PersistentPath}/{path}";
        if(!File.Exists(filePath))
        {
            return default(T);
        }
        var bytes = File.ReadAllBytes(filePath);
        var instance = Activator.CreateInstance(typeof(T)) as IFilelizable;
        instance.Deserilize(bytes);
        return (T)instance;
    }

    public static byte[] ReadBytes(string path)
    {
        var filePath = $"{PersistentPath}/{path}";
        if(!File.Exists(filePath))
        {
            return null;
        }
        var bytes = File.ReadAllBytes(filePath);
        return bytes;
    }
}