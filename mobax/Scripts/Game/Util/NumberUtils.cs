using UnityEngine;

public static class NumberUtils
{
    private static readonly string[] Magnitudes = {"", "K", "M", "B"};
    
    public static string Simplify(int num)
    {
        return _Simplify_en(num);
    }

    private static string _Simplify_en(int num)
    {
        var val = (float) num;
        var index = 0;
        float newVal;
        while ((newVal = val / 1000) >= 1 && index < Magnitudes.Length - 1)
        {
            val = newVal;
            ++index;
        }

        if (val >= 100)
        {
            val = Mathf.Floor(val);
        }
        else if (val >= 10)
        {
            val = Mathf.Floor(val * 10) / 10;
        }
        else
        {
            val = Mathf.Floor(val * 100) / 100;
        }

        return $"{val}{Magnitudes[index]}";
    }
}