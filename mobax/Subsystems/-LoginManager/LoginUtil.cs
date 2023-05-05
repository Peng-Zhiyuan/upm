using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public static class LoginUtil 
{
    public static bool IsMatchedSidFilter(int sid, string filter)
    {
        if (filter == null || filter == "")
        {
            return true;
        }

        {
            var b = int.TryParse(filter, out var outNumber);
            if (b)
            {
                if (sid == outNumber)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }



        if (!filter.Contains("-"))
        {
            throw new Exception("[LoginManager] sid filter not validate");
        }
        var parts = filter.Split('-');
        var min = parts[0];
        var max = parts[1];
        var minValue = TryParse(min, 0);
        var maxValue = TryParse(max, int.MaxValue);
        if (sid >= minValue && sid <= maxValue)
        {
            return true;
        }
        else
        {
            return false;
        }

    }

    static int TryParse(string str, int retWhenFailed)
    {
        var b = int.TryParse(str, out var ret);
        if (!b)
        {
            return retWhenFailed;
        }
        return ret;
    }
}
