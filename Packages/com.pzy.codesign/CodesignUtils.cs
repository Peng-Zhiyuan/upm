using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using ICSharpCode.SharpZipLib.Zip;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Reflection;
using System.Threading;
using System.Diagnostics;
using CustomLitJson;

public static class CodesignUtils 
{
    public static string ByteToHexStr(Byte[] source)
    {
        Byte highByte, lowByte;
        Byte[] dest = new Byte[2 * source.Length];
        string signval = "";
        for (int i = 0; i < source.Length; i++)
        {
            highByte = (Byte)(source[i] >> 4);
            lowByte = (Byte)(source[i] & 0x0f);
            highByte = (Byte)(highByte + 0x30);
            if (highByte > 0x39)
                signval += (char)(highByte + 0x07);
            else
                signval += (char)highByte;

            lowByte = (Byte)(lowByte + 0x30);

            if (lowByte > 0x39)
                signval += (char)(lowByte + 0x07);
            else
                signval += (char)(lowByte);
        }
        return signval;
    }

    public static string GetSuperAndroidPK()
    {
#if UNITY_EDITOR || UNITY_IOS
        return "404816A94866FCAFD3FE163FF0C77EC1";
#else
        return "error";
#endif
    }

    public static string GetSuperCodeMD5()
    {
#if UNITY_EDITOR || UNITY_IOS
        return "404816A94866FCAFD3FE163FF0C77EC1";
#else
        return "error";
#endif
    }

    public static String GetAndroidPkMD5()
    {
        var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        var activity = player.GetStatic<AndroidJavaObject>("currentActivity");
        var PackageManager = new AndroidJavaClass("android.content.pm.PackageManager");

        var packageName = activity.Call<string>("getPackageName");

        var GET_SIGNATURES = PackageManager.GetStatic<int>("GET_SIGNATURES");
        var packageManager = activity.Call<AndroidJavaObject>("getPackageManager");
        var packageInfo = packageManager.Call<AndroidJavaObject>("getPackageInfo", packageName, GET_SIGNATURES);
        var signatures = packageInfo.Get<AndroidJavaObject[]>("signatures");
        if (signatures != null && signatures.Length > 0)
        {
            byte[] pk = signatures[0].Call<byte[]>("toByteArray");
            var md5Culculator = MD5.Create();
            var md5Bytes = md5Culculator.ComputeHash(pk);
            md5Culculator.Dispose();
            var hex = ByteToHexStr(md5Bytes);
            return hex;
        }

        return null;
    }

    public static void GetAndroidCodeMD5Asyn(Action<ReadZipReport> result)
    {
        var t = new Thread(() =>
        {
            try
            {
                var resport = ReadZip();
                result?.Invoke(resport);
            }
            catch(Exception e)
            {
                UnityEngine.Debug.LogException(e);
                var resport = new ReadZipReport()
                {
                    codeMD5 = "crash",
                    fileList = "crash",
                    fileListMD5 = "crash",
                };
                result?.Invoke(resport);
            }
        });
        t.Start();
    }

    /*
    public static ReadZipReport ReadZip()
    {
        var sw = new Stopwatch();
        sw.Start();
        var ret = new ReadZipReport();
        ZipConstants.DefaultCodePage = Encoding.UTF8.CodePage;
        var apkPath = Application.dataPath;
        var file = File.Open(apkPath, FileMode.Open, FileAccess.Read);
        var stream = new ZipInputStream(file);
        var LENGTH = 2048;
        var tempBytes = new byte[LENGTH];
        MD5 codeMD5Calculator = null;
        if(CCodesign.calculateCodeMD5)
        {
            codeMD5Calculator = MD5.Create();
        }
        var sb = new StringBuilder();
        var first = true;
        var entry = stream.GetNextEntry();
        while (entry != null)
        {
            if (entry.IsFile)
            {
                var name = entry.Name;
                var isManaged = name.Contains("Managed");
                var isSo = name.EndsWith(".so", StringComparison.Ordinal);
                var isDex = name.EndsWith(".dex", StringComparison.Ordinal);

                if(isManaged || isSo || isDex)
                {
                    var size = stream.Length;
                    if(CCodesign.calculateCodeMD5)
                    {
                        var len = stream.Read(tempBytes, 0, LENGTH);
                        while (len > 0)
                        {
                            codeMD5Calculator.TransformBlock(tempBytes, 0, len, tempBytes, 0);
                            len = stream.Read(tempBytes, 0, LENGTH);
                        }
                    }
                    // file list
                    if(first)
                    {
                        first = false;
                    }
                    else
                    {
                        sb.Append(",");
                    }
                    var shortName = Path.GetFileName(name);
                    sb.Append(shortName);
                    sb.Append(",");
                    sb.Append(size);

                    //Debug.Log(shortName + " - " + size + " bytes");
                }
            
            }
        
            try
            {
                entry = stream.GetNextEntry();
            }
            catch
            {
                break;
            }
        }
        stream.Close();
        if(CCodesign.calculateCodeMD5)
        {
            codeMD5Calculator.TransformFinalBlock(tempBytes, 0, 0);
            var hash = codeMD5Calculator.Hash;
            var hex = ByteToHexStr(hash);
            ret.codeMD5 = hex;
            codeMD5Calculator.Dispose();
        }
        {
            var fileList = sb.ToString();
            ret.fileList = fileList;
            var fileListBytes = Encoding.UTF8.GetBytes(fileList);
            var md5Culculator = MD5.Create();
            var hash = md5Culculator.ComputeHash(fileListBytes);
            md5Culculator.Dispose();
            var hex = ByteToHexStr(hash);
            ret.fileListMD5 = hex;
        }
        sw.Stop();
#if !RELEASE
        UnityEngine.Debug.Log("ReadZip usetime " + sw.ElapsedMilliseconds);
#endif
        return ret;
    }*/


    public static ReadZipReport ReadZip()
    {
#if !RELEASE
        var sw = new Stopwatch();
        sw.Start();
#endif
        var ret = new ReadZipReport();
        ZipConstants.DefaultCodePage = Encoding.UTF8.CodePage;
        var apkPath = Application.dataPath;
        var LENGTH = 2048;
        var tempBytes = new byte[LENGTH];
        MD5 codeMD5Calculator = null;
        if (CCodesign.calculateCodeMD5)
        {
            codeMD5Calculator = MD5.Create();
        }
        var sb = new StringBuilder();
        var first = true;

        var zipFileStream = File.OpenRead(apkPath);
        var zipFile = new ZipFile(zipFileStream);
        var e = zipFile.GetEnumerator();

        while (e.MoveNext())
        {
            var entry = e.Current as ZipEntry;

            if (entry.IsFile)
            {
                var name = entry.Name;
                var isManaged = name.Contains("Managed");
                var isSo = name.EndsWith(".so", StringComparison.Ordinal);
                var isDex = name.EndsWith(".dex", StringComparison.Ordinal);

                if (isManaged || isSo || isDex)
                {
                    var size = entry.Size;
                    if (CCodesign.calculateCodeMD5)
                    {
                        using(var stream = zipFile.GetInputStream(entry))
                        {
                            var len = stream.Read(tempBytes, 0, LENGTH);
                            while (len > 0)
                            {
                                codeMD5Calculator.TransformBlock(tempBytes, 0, len, tempBytes, 0);
                                len = stream.Read(tempBytes, 0, LENGTH);
                            }
                        }
                    }
                    // file list
                    if (first)
                    {
                        first = false;
                    }
                    else
                    {
                        sb.Append(",");
                    }
                    var shortName = Path.GetFileName(name);
                    sb.Append(shortName);
                    sb.Append(",");
                    sb.Append(size);

                    //Debug.Log(shortName + " - " + size + " bytes");
                }

            }

        }
        zipFile.Close();
        if (CCodesign.calculateCodeMD5)
        {
            codeMD5Calculator.TransformFinalBlock(tempBytes, 0, 0);
            var hash = codeMD5Calculator.Hash;
            var hex = ByteToHexStr(hash);
            ret.codeMD5 = hex;
            codeMD5Calculator.Dispose();
        }
        {
            var fileList = sb.ToString();
            ret.fileList = fileList;
            var fileListBytes = Encoding.UTF8.GetBytes(fileList);
            var md5Culculator = MD5.Create();
            var hash = md5Culculator.ComputeHash(fileListBytes);
            md5Culculator.Dispose();
            var hex = ByteToHexStr(hash);
            ret.fileListMD5 = hex;
        }
#if !RELEASE
        sw.Stop();
        UnityEngine.Debug.Log("ReadZip usetime " + sw.ElapsedMilliseconds);
#endif
        return ret;
    }

    public static string Encry(string time, string androidPk, string codeMD5, string ssk, string fileList, string fileListMD5)
    {
        var jd = new JsonData();
        jd["sj"] = time;
        jd["azgy"] = androidPk;
        jd["dmzw"] = codeMD5;
        jd["ssk"] = ssk;
        jd["wjzw"] = fileListMD5;
        jd["wj"] = fileList;
        var json = JsonMapper.Instance.ToJson(jd);

        int len = json.Length;
        var rbytes = new Byte[len];
        for(var i = 0; i < len; i++)
        {
            var ch = json[i];
            var asc = (byte)ch;
            rbytes[i] = asc;
        }

        string hex = ByteToHexStr(rbytes);
        return hex;
    }

    //    public static string Encry(string time, string androidPk, string codeMD5, string ssk, string fileList, string fileListMD5)
    //    {
    //        string mingwen = $"sj={time}&azgy={androidPk}&dmzw={codeMD5}&ssk={ssk}&wjzw={fileListMD5}&wj={fileList}";
    //        int len = mingwen.Length;
    //        Byte[] rbytes = new Byte[len + 1];
    //        byte k = (byte)time[time.Length - 1];

    //#if !RELEASE
    //        UnityEngine.Debug.Log("mingwen: " + mingwen);
    //#endif
    //        if (k % 2 == 1)
    //        {
    //            for (int i = 0; i < len; i++)
    //            {
    //                int p = i % 2 == 0 ? i * 3 : -i * 2;
    //                rbytes[i] = (byte)((~(mingwen[i] + p)) + k);
    //            }
    //            rbytes[len] = k;
    //        }
    //        else
    //        {
    //            //            for (int i = 0; i < len; i++)
    //            //            {
    //            //                int p = i % 2 == 1 ? (i + 1) * 2 : -i * 5;
    //            //                rbytes[i] = (byte)(ByteCircelLeftMove((byte)(mingwen[i] ^ p)) + k);
    //            //            }
    //            //            rbytes[len] = k;
    //            for (int i = 0; i < len; i++)
    //            {
    //                int p = i % 2 == 0 ? i * 3 : -i * 2;
    //                rbytes[i] = (byte)((~(mingwen[i] + p)) - k);
    //            }
    //            rbytes[len] = k;
    //        }

    ///*
    //        var sb = new StringBuilder();
    //        foreach (var b in rbytes)
    //        {
    //            sb.Append(b.ToString() + ", ");
    //        }
    //        Debug.Log(sb.ToString());
    //*/

    //        string sign_val = ByteToHexStr(rbytes);
    //        return sign_val;
    //    }

    private static byte ByteCircelLeftMove(byte b)
    {
        if (b >= 128)
        {
            return (byte)((b << 1) + 1);
        }
        return (byte)(b << 1);
    }


}
