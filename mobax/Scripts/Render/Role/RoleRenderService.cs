using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoleRenderService : Service
{
    public override void OnCreate()
    {
       // Debug.LogError("Set DessolveMask");
        InitRoleRender();
    }

    private async void InitRoleRender()
    {
        Texture2D dessolveMask = await BucketManager.Stuff.Main.GetOrAquireAsync<Texture2D>("mask2.png");
        Shader.SetGlobalTexture("_DessolveMask", dessolveMask);
       // Debug.LogError("Set DessolveMask");
    }

}
