using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Reflection;
using System;
using System.Text;

public static class FileManager
{
    public static string Write(string path, IFilelizable obj)
    {
        var filePath = $"{Application.persistentDataPath}/{path}";
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
        var filePath = $"{Application.persistentDataPath}/{path}";
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
        var filePath = $"{Application.persistentDataPath}/{path}";
        return File.Exists(filePath);
    }

    public static string[] GetFileList(string dirPath)
    {
        if(Directory.Exists(dirPath))
        {
            return Directory.GetFiles(dirPath);
        }
        return new string[0];
    }

    public static string[] GetDirectoryList(string dirPath)
    {
        var root = $"{Application.persistentDataPath}/{dirPath}";
        if (Directory.Exists(root))
        {
            return Directory.GetDirectories(root);
        }
        return new string[0];
    }

    public static void DeleteFile(string path)
    {
        if(File.Exists(path))
        {
            File.Delete(path);
        }
    }

    public static void DeleteDirectory(string path)
    {
        if (Directory.Exists(path))
        {
            Directory.Delete(path, true);
        }
    }

    public static T Read<T>(string path) where T : IFilelizable
    {
        var filePath = $"{Application.persistentDataPath}/{path}";
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
        var filePath = $"{Application.persistentDataPath}/{path}";
        if(!File.Exists(filePath))
        {
            return null;
        }
        var bytes = File.ReadAllBytes(filePath);
        return bytes;
    }
}