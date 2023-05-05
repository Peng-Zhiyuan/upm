using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Security.Cryptography;
using System.Text;
using System.IO;

public static class SecurityUtil
{
    public static string Md5(string origin)
    {
        MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
        byte[] bytes = Encoding.ASCII.GetBytes(origin);
        byte[] encoded = md5.ComputeHash(bytes);
        var base16 = DataToBase16(encoded);
        return base16;
    }

    public static string GetFileMd5(string path)
    {
        var fileStream = File.OpenRead(path);
        MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
        var data = md5.ComputeHash(fileStream);
        var base16 = DataToBase16(data);
        return base16;
    }

    public static string DataToBase16(byte[] data)
    {
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < data.Length; i++)
        {
            sb.Append(data[i].ToString("x2"));
        }
        return sb.ToString();
    }
}

