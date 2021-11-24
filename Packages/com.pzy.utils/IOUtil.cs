using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public static class IOUtil 
{
    public static void DeleteDirectoryIfExits(string path)
    {
        var b = Directory.Exists(path);
        if(b)
        {
            Directory.Delete(path, true);
        }
    }
}
