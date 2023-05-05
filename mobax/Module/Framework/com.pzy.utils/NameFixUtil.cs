using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
public static class NameFixUtil 
{
    /// <summary>
    /// 修复扩展名
    /// </summary>
    /// <param name="origin"></param>
    /// <param name="fix">带点，举例：".prefab"</param>
    /// <returns></returns>
    public static string ChangeExtension(string origin, string fix)
    {
        var ret = Path.ChangeExtension(origin, fix);
        return ret;
    }
}
