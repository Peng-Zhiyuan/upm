using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


public static class ClientConfigEditorCommand
{
    [MenuItem("pzy.com.*/ClientConfig/Generate")]
    static void Generate()
    {
        EtuUnity.Program.Main(new string[] { });
    }


}
