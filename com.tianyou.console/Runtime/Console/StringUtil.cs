using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.Text.RegularExpressions;

public class StringUtil
{
    /// <summary>
    /// 取最小的一位数
    /// </summary>
    /// <param name="first"></param>
    /// <param name="second"></param>
    /// <param name="third"></param>
    /// <returns></returns>
    static int LowerOfThree(int first, int second, int third)
    {
        int min = first;
        if (second < min)
            min = second;
        if (third < min)
            min = third;
        return min;
    }

    static int Levenshtein_Distance(string str1, string str2)
    {
        int[,] Matrix;
        int n=str1.Length;
        int m=str2.Length;

        int temp = 0;
        char ch1;
        char ch2;
        int i = 0;
        int j = 0;
        if (n ==0)
        {
            return m;
        }
        if (m == 0)
        {

            return n;
        }
        Matrix=new int[n+1,m+1];

        for (i = 0; i <= n; i++)
        {
            //初始化第一列
            Matrix[i,0] = i;
        }

        for (j = 0; j <= m; j++)
        {
            //初始化第一行
            Matrix[0, j] = j;
        }

        for (i = 1; i <= n; i++)
        {
            ch1 = str1[i-1];
            for (j = 1; j <= m; j++)
            {
                ch2 = str2[j-1];
                if (ch1.Equals(ch2))
                {
                    temp = 0;
                }
                else
                {
                    temp = 1;
                }
                Matrix[i,j] = LowerOfThree(Matrix[i - 1,j] + 1, Matrix[i,j - 1] + 1, Matrix[i - 1,j - 1] + temp);


            }
        }

        // for (i = 0; i <= n; i++)
        // {
        //     for (j = 0; j <= m; j++)
        //     {
        //         Console.Write(" {0} ", Matrix[i, j]);
        //     }
        //     Console.WriteLine("");
        // }
        return Matrix[n, m];

    }

    
    static float LevenshteinConsoleDistancePercentInner(string str1,string str2){
        int maxLenth = str1.Length > str2.Length ? str1.Length : str2.Length;
        int minLenth = str1.Length <= str2.Length ? str1.Length : str2.Length;
        var lowerStr1 = str1.ToLower();
        var lowerStr2 = str2.ToLower();
        int val = Levenshtein_Distance(lowerStr1, lowerStr2);
        var result = 1 - (float)val / maxLenth;

        // 开头匹配优先
        // 递归尾
        if(lowerStr1.Length != lowerStr2.Length){
            result += LevenshteinConsoleDistancePercentInner(lowerStr1.Substring(0, minLenth), lowerStr2.Substring(0, minLenth));
        }
        
        return result;
    }

    /// <summary>
    /// 计算字符串相似度
    /// </summary>
    /// <param name="str1"></param>
    /// <param name="str2"></param>
    /// <returns></returns>
    public static float LevenshteinConsoleDistancePercent(string str1,string str2)
    {
        var value = LevenshteinConsoleDistancePercentInner(str1, str2);

        var lowerStr1 = str1.ToLower();
        var lowerStr2 = str2.ToLower();
        // 包含关系优先
        if(lowerStr1.Contains(lowerStr2) || lowerStr2.Contains(lowerStr1)){
            value += 1f;
        }
        
        // 等长补充
        if(lowerStr1.Length == lowerStr2.Length){
            value += LevenshteinConsoleDistancePercentInner(str1, str2);
        }

        return value;
    }

    public static string List2String(List<string> list, string join = "-QAQ-"){
        StringBuilder sb = new StringBuilder();
        foreach(var s in list){
            sb.Append(s);
            sb.Append(join);
        }
        return sb.ToString();
    }

    public static List<string> String2List(string str, string join = "-QAQ-"){
        string[] strs = Regex.Split(str, join);
        List<string> result = new List<string>();
        foreach(var s in strs){
            var tempS = s.Trim();
            if(tempS != ""){
                result.Add(s);
            }
        }
        return result;
    }

}
