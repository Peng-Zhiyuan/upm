using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using CustomLitJson;
using UnityEngine;
using System.Web;
public class NetworkUtil {
    private static DateTime UnixTimestamp = new DateTime (1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public static long GenerateTimestamp () {
        return GenerateTimestamp (DateTime.Now);
    }

    public static long GenerateTimestamp (DateTime time) {
        return (long) (time.ToUniversalTime () - UnixTimestamp).TotalSeconds;
    }

    public static DateTime ConvertFromTimestamp (long timestamp) {
        return TimeZone.CurrentTimeZone.ToLocalTime (UnixTimestamp.AddSeconds (timestamp));
    }

    // public static string Join<T> (string separator, IEnumerable<T> values) {
    //     StringBuilder buffer = new StringBuilder ();
    //     foreach (T t in values) {
    //         if (buffer.Length != 0) buffer.Append (separator);
    //         buffer.Append (t == null ? "" : t.ToString ());
    //     }
    //     return buffer.ToString ();
    // }

    // public static string UrlEncode (string text) {
    //     if (string.IsNullOrEmpty (text)) return string.Empty;
    //     return HttpUtility.UrlEncode(text);
    //     // if (string.IsNullOrEmpty (text)) return string.Empty;
    //     // StringBuilder buffer = new StringBuilder (text.Length);
    //     // byte[] data = Encoding.UTF8.GetBytes (text);
    //     // foreach (byte b in data) {
    //     //     char c = (char) b;
    //     //     if (!(('0' <= c && c <= '9') || ('a' <= c && c <= 'z') || ('A' <= c && c <= 'Z')) &&
    //     //         "-_.~".IndexOf (c) == -1) {
    //     //         buffer.Append ('%' + Convert.ToString (c, 16).ToUpper ().PadLeft (2, '0'));
    //     //     } else {
    //     //         buffer.Append (c);
    //     //     }
    //     // }
    //     // return buffer.ToString ();
    // }

    
    // public static string UrlDecode (string text) {
    //     if (string.IsNullOrEmpty (text)) return string.Empty;
    //     return HttpUtility.UrlDecode(text);
    // }

    public static string MD5 (string val) {
        return MD5 (val, Encoding.UTF8);
    }

    public static string MD5 (string val, Encoding encoding) {
        if (string.IsNullOrEmpty (val)) return "";

        MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider ();

        byte[] output = md5.ComputeHash (encoding.GetBytes (val));

        md5.Clear ();

        StringBuilder code = new StringBuilder ();
        for (int i = 0; i < output.Length; i++) {
            code.Append (output[i].ToString ("x2"));
        }
        return code.ToString ();
    }

    static CustomLitJson.JsonMapper _main_json_mapper;
    public static CustomLitJson.JsonMapper MainJasonMapper () {
        if (_main_json_mapper == null) {
            _main_json_mapper = new CustomLitJson.JsonMapper ();
        }
        return _main_json_mapper;
    }

    static CustomLitJson.JsonMapper _sub_json_mapper;
    public static CustomLitJson.JsonMapper SubJasonMapper () {
        if (_sub_json_mapper == null) {
            _sub_json_mapper = new CustomLitJson.JsonMapper ();
        }
        return _sub_json_mapper;
    }
    //public static string UnzipString (byte[] compbytes) {
    //    return ZlibStream.UncompressString (compbytes);
    //}

    public static object ToObject (System.Type type, string json) {
        return NetworkUtil.MainJasonMapper ().ReadValue (type, new CustomLitJson.JsonReader (json));
    }

    public static WWWForm DictionaryToWWWForm (Dictionary<string, string> arg) {
      
        WWWForm form = new WWWForm ();
        foreach (var kv in arg) {
            if (kv.Value != null) {
                form.AddField (kv.Key, kv.Value);
            }
        }
        //参数签名
        string _sign = NetworkSign.MD5CryptoServiceProvider(NetworkUtil.DictionaryToParamsString(arg));
        form.AddField ("_sign", _sign);
        return form;
    }
    
    
    public static WWWForm DictionaryToEncryptedWWWForm(Dictionary<string, string> arg)
	{
		string jsonStr = CustomLitJson.JsonMapper.Instance.ToJson(arg);
		string encryptedStr = EncryptionMgr.EncryptString(jsonStr);
		WWWForm form = new WWWForm();
		form.AddField("_data", encryptedStr);
         //参数签名
        string _sign = NetworkSign.MD5CryptoServiceProvider(NetworkUtil.DictionaryToParamsString(arg));
        form.AddField ("_sign", _sign);
		return form;
	}

    public static string DictionaryToParamsString (Dictionary<string, string> dic) {

        string ret = "";
        foreach (var keyVal in dic) {
            if(ret != "")ret+="&";
            ret += keyVal.Key + "=" + keyVal.Value;
        }
        return ret;
    }

    public static string DictionaryToString (Dictionary<string, string> dic) {
        string ret = "";
        foreach (var keyVal in dic) {
            ret += keyVal.Key + " : " + keyVal.Value + "    ";
        }
        return ret;
    }
}