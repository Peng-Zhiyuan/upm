using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
public class CustomSetting 
{
    [MenuItem("Tools/ResetSettigns")]
    public static void ResetAll()
    {
        RenderSettings.skybox = null;
       /* AssetDatabase.MoveAsset("Assets/Arts/Models/$Pose", "Assets/Arts/Models/Pose");
        AssetDatabase.Refresh();*/
    }

    [MenuItem("Tools/OpenEditMode")]
    public static void OpenEditMode()
    {
        RenderExtend.editMode = true;

    }

    [MenuItem("Tools/CloseEditMode")]
    public static void CloseEditMode()
    {
        RenderExtend.editMode = false;
    }

}
