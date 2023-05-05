using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public static class CCodesign
{
    private static string SSK_EDITOR = "998";
    private static string SSK_ANDROID = "1";
    private static string SSK_IOS = "27";

    public static bool calculateCodeMD5 = false;

    public static Task<string> SignAsync(string time)
    {
        var tcs = new TaskCompletionSource<string>();
        Sign(time, result=>{
            tcs.SetResult(result);
        });
        return tcs.Task;
    }

    public static void Sign(string time, Action<string> onResult)
    {
        var androidPk = GetPK();
        GetZipReport(report =>
        {
            string ssk = GetSSK();
            string codeMD5 = report.codeMD5;
            string fileList = report.fileList;
            string fileListMD5 = report.fileListMD5;
            var sign = CodesignUtils.Encry(time, androidPk, codeMD5, ssk, fileList, fileListMD5);
            onResult?.Invoke(sign);
        });
    }

    private static string GetSSK()
    {
        switch (Application.platform)
        {
            case RuntimePlatform.Android:
                return SSK_ANDROID;
            case RuntimePlatform.IPhonePlayer:
                return SSK_IOS;
            case RuntimePlatform.WindowsEditor:
            case RuntimePlatform.OSXEditor:
                return SSK_EDITOR;
            default:
                throw new Exception("not found ssk for platform: " + Application.platform);
        }
    }

    private static string GetPK()
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            return CodesignUtils.GetAndroidPkMD5();
        }
        else
        {
            return CodesignUtils.GetSuperAndroidPK();
        }
       
    }

    private static void GetZipReport(Action<ReadZipReport> onResult)
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            CodesignUtils.GetAndroidCodeMD5Asyn(onResult);
        }
        else
        {
            var md5 = CodesignUtils.GetSuperCodeMD5();
            var report = new ReadZipReport
            {
                codeMD5 = md5,
                fileList = "",
                fileListMD5 = "",
            };
            onResult?.Invoke(report);
        }
    }

}