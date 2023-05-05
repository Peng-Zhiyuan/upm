using UnityEngine;
using System.Collections.Generic;

public class NGGlobalSettings
{
    public enum RoleState
    {
        Normal = 0,
        Depth = 1,
        //BaseOnly = 2
    }
    public enum SceneState
    {
        Normal = 0,
        //VR = 1,
    }

    //Bloom
    public static bool UseBloom = true;

    public static bool EnableCustomBloom = false;

    public static int BloomCustomRange;

    public static float BloomCustomThreshold;

    public static float BloomCustomIntensity;

    //Radia Blur
    public static bool UseRadiaBlur = false;

    public static float RadiaBlurLevel;

    public static float RadiaBlurCenterX;

    public static float RadiaBlurCenterY;

    public static float RadiaBlurBufferRadius;

    //GreyWhiteNew
    public static bool UseGreyWhiteNew = false;

    public static Material GreyWhiteNewMaterial;

    public static float GreyWhiteNewAmount;

    public static int GreyWhiteNewSwitch;

    public static int GreyWhiteNewChange;

    //role_shadowcaster标记的更换成深度shader
    public static bool RenderDepthShader = false;

    //public static Dictionary<int, int> GrabObjects = new Dictionary<int, int>();
    public static int GrabObjectsCount = 0;
    //public static LayerMask FissureHoleLayer;

    public static SceneState GlobalSceneState = SceneState.Normal;

    //Role State
    public static RoleState GlobalRoleState = RoleState.Normal;

    public static bool UseRimColor = false;

}
