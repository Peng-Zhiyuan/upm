using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Collections.Generic;
using CustomLitJson;

namespace EtuUnity
{
    public class CacheManager
    {
        public static void WriteCache(string filePath, List<TableResult> tableResultList)
        {
            var md5 = GetMD5HashFromFile(filePath);
            var root = "ClientConfigCache";
            var cacheFilePath = $"{root}/cache/{md5}";
            var cacheJson = TableResultListToCacheJson(tableResultList);
            SureParentDirCraeted(cacheFilePath);
            File.WriteAllText(cacheFilePath, cacheJson);
        }

        public static List<TableResult> ReadCache(string filePath)
        {
            var md5 = GetMD5HashFromFile(filePath);
            var root = "ClientConfigCache";
            var cacheFilePath = $"{root}/cache/{md5}";
            var cacheJson = File.ReadAllText(cacheFilePath);
            var tableResultList = CacheJsonToTableResultList(cacheJson);
            return tableResultList;
        }

        public static bool HasCache(string filePath)
        {
            var md5 = GetMD5HashFromFile(filePath);
            var root = "ClientConfigCache";
            var cacheFilePath = $"{root}/cache/{md5}";
            var exsists = File.Exists(cacheFilePath);
            return exsists;
        }

        public static void Clean()
        {
            var root = "ClientConfigCache";
            var dir = $"{root}/cache";
            var b = Directory.Exists(dir);
            if (b)
            {
                Directory.Delete(dir, true);
            }
        }

        private static void SureParentDirCraeted(string path)
        {
            var dirPath = Path.GetDirectoryName(path);
            var b = Directory.Exists(dirPath);
            if (!b)
            {
                Directory.CreateDirectory(dirPath);
            }
        }

        public static String TableResultListToCacheJson(List<TableResult> resultList)
        {
            //var w = new CacheWrapper();
            //w.list = resultList;
            var json = JsonMapper.Instance.ToJson(resultList);
            return json;
        }

        public static List<TableResult> CacheJsonToTableResultList(string jsonCache)
        {
            var w = JsonMapper.Instance.ToObject<List<TableResult>>(jsonCache);
            //var list = w.list;
            return w;
        }

        public static string GetMD5HashFromFile(string fileName)
        {
            var file = new FileStream(fileName, System.IO.FileMode.Open);
            var md5 = new MD5CryptoServiceProvider();
            var bytes = md5.ComputeHash(file);
            file.Close();
            var sb = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                sb.Append(bytes[i].ToString("x2"));
            }
            return sb.ToString();
        }
    }
}