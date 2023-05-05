using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public static class CSharpCodeUtil 
{
    public static string AddNamespace(string code, string @namespace)
    {
        var outCode = $"namespace {@namespace}\n{{{code}\n}}";
        return outCode;
    }

    public static void AddNamespaceToFile(string path, string @namespace)
    {
        var code = File.ReadAllText(path);
        var newCode = AddNamespace(code, @namespace);
        File.WriteAllText(path, newCode);
    }
}
