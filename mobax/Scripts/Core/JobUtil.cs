using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class JobUtil
{
    public static string GetIconAddress(int jobId)
    {
        var address = $"Icon_occ{jobId}.png";
        return address;
    }
}
