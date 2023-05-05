using System.Collections.Generic;
using System.Text.RegularExpressions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

public static class StringUtil2
{
    public static string FormatInteger(int val, int count)
    {
        return string.Format("{0:D" + count + "}", val.ToString());
    }

    public static string get_utf8(string unicodeString)
    {
        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(unicodeString);
        string hexString = "";
        System.Text.StringBuilder strB = new System.Text.StringBuilder();
        for (int i = 0; i < bytes.Length; i++)
        {
            strB.Append(bytes[i].ToString("X2"));
        }

        hexString = strB.ToString();
        return hexString;
    }

    public static string ToUrlEncode(string strCode)
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        byte[] byStr = System.Text.Encoding.UTF8.GetBytes(strCode); //默认是System.Text.Encoding.Default.GetBytes(str)
        System.Text.RegularExpressions.Regex regKey = new System.Text.RegularExpressions.Regex("^[A-Za-z0-9]+$");
        StrBuild.Instance.ClearSB();
        for (int i = 0; i < byStr.Length; i++)
        {
            string strBy = System.Convert.ToChar(byStr[i]).ToString();
            if (regKey.IsMatch(strBy))
            {
                StrBuild.Instance.Append(strBy);
            }
            else
            {
                StrBuild.Instance.Append(@"%" + System.Convert.ToString(byStr[i], 16));
            }
        }

        return (StrBuild.Instance.GetString());
    }

    /// <summary>
    /// 首字母大写
    /// </summary>
    /// <param name="word"></param>
    /// <returns></returns>
    public static string CapitalizeFirstLetter(string word)
    {
        return word.Substring(0, 1).ToUpper() + word.Substring(1);
    }

    public static string RemoveHead(string origin, string head)
    {
        var b = origin.StartsWith(head);
        if (!b)
        {
            return origin;
        }

        return origin.Substring(head.Length);
    }

    public static string RemoveTail(string origin, string tail)
    {
        var b = origin.EndsWith(tail);
        if (!b)
        {
            return origin;
        }

        return origin.Substring(0, origin.Length - tail.Length);
    }

    /// <summary>
    /// 截取字符串中开始和结束字符串中间的字符串
    /// </summary>
    /// <param name="source">源字符串</param>
    /// <param name="startStr">开始字符串</param>
    /// <param name="endStr">结束字符串</param>
    /// <returns>中间字符串</returns>
    public static string SubstringSingle(string source, string startStr, string endStr)
    {
        Regex rg = new Regex("(?<=(" + startStr + "))[.\\s\\S]*?(?=(" + endStr + "))",
            RegexOptions.Multiline | RegexOptions.Singleline);
        return rg.Match(source).Value;
    }

    /// <summary>
    /// （批量）截取字符串中开始和结束字符串中间的字符串
    /// </summary>
    /// <param name="source">源字符串</param>
    /// <param name="startStr">开始字符串</param>
    /// <param name="endStr">结束字符串</param>
    /// <returns>中间字符串</returns>
    public static List<string> SubstringMultiple(string source, string startStr, string endStr)
    {
        Regex rg = new Regex("(?<=(" + startStr + "))[.\\s\\S]*?(?=(" + endStr + "))",
            RegexOptions.Multiline | RegexOptions.Singleline);
        MatchCollection matches = rg.Matches(source);
        List<string> resList = new List<string>();
        foreach (Match item in matches)
            resList.Add(item.Value);
        return resList;
    }

    public static IEnumerable<byte> ToBytes(this string str)
    {
        byte[] byteArray = Encoding.Default.GetBytes(str);
        return byteArray;
    }

    public static byte[] ToByteArray(this string str)
    {
        byte[] byteArray = Encoding.Default.GetBytes(str);
        return byteArray;
    }

    public static byte[] ToUtf8(this string str)
    {
        byte[] byteArray = Encoding.UTF8.GetBytes(str);
        return byteArray;
    }

    public static byte[] HexToBytes(this string hexString)
    {
        if (hexString.Length % 2 != 0)
        {
            throw new ArgumentException(String.Format(CultureInfo.InvariantCulture,
                "The binary key cannot have an odd numberßß of digits: {0}", hexString));
        }

        var hexAsBytes = new byte[hexString.Length / 2];
        for (int index = 0; index < hexAsBytes.Length; index++)
        {
            string byteValue = "";
            byteValue += hexString[index * 2];
            byteValue += hexString[index * 2 + 1];
            hexAsBytes[index] = byte.Parse(byteValue, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        }

        return hexAsBytes;
    }

    public static string Fmt(this string text, params object[] args)
    {
        return string.Format(text, args);
    }

    public static string IntToRoman(int num)
    {
        string res = String.Empty;
        List<int> val = new List<int> {1000, 900, 500, 400, 100, 90, 50, 40, 10, 9, 5, 4, 1};
        List<string> str = new List<string> {"M", "CM", "D", "CD", "C", "XC", "L", "XL", "X", "IX", "V", "IV", "I"};
        for (int i = 0; i < val.Count; ++i)
        {
            while (num >= val[i])
            {
                num -= val[i];
                res += str[i];
            }
        }

        return res;
    }
}