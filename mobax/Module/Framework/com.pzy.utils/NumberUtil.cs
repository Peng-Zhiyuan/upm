using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class NumberUtil
{
    public static string ToThousandSeparatorFormat(int count)
    {
        if (count < 1000000)
        {
            return count.ToString("#,0");
        }
        else
        {
            return count.ToString("#,#,k");
        }
    }
}
