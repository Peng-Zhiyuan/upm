using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleEffect : BattleComponent<BattleEffect>
{
    public void StartFocus(string uid)
    {
        var mainCamera = CameraManager.Instance.MainCamera;
        if (mainCamera != null)
        {
            var customData = mainCamera.GetComponent<CameraCustomData>();
            customData.colorAdjustments.fadeScene = 0.85f;
           /* TweenUtil.TweenTo(10000, 5000, 0.2f, delegate (int val)
            {
                if (customData == null) return;
                customData.colorAdjustments.fadeScene = val / 10000f;
            });*/

        }
        if (Battle.Instance.IsFight)
        {
            var actors = BattleEngine.Logic.BattleManager.Instance.ActorMgr.GetAllActors();
            foreach (var actor in actors)
            {
                if (actor.mData.UID == uid)
                {
                    actor.RoleRender.SetCustomLightColor(RoleRender.defaultLightColor);
                }
                else 
                {
                    actor.RoleRender.SetCustomLightColor(new Color(0.15f, 0.15f, 0.15f, 1));
                   
                }
            }
        }
    }
    public void EndFocus()
    {
        var mainCamera = CameraManager.Instance.MainCamera;// CameraManager.Instance.MainCamera;
        if (mainCamera != null)
        {
           
            var customData = mainCamera.GetComponent<CameraCustomData>();
            customData.colorAdjustments.fadeScene = 0f;
          /*  TweenUtil.TweenTo(50000, 10000, 0.2f, delegate (int val)
            {
                if (customData == null) return;
                customData.colorAdjustments.fadeScene = val/10000f;
            });*/
          
        }
        if (Battle.Instance.IsFight)
        {
            var actors = BattleEngine.Logic.BattleManager.Instance.ActorMgr.GetAllActors();
            foreach (var actor in actors)
            {
                if (actor == null || actor.RoleRender == null)
                {
                    continue;
                }
                actor.RoleRender.SetCustomLightColor(RoleRender.defaultLightColor);
            }
        }
    }

}
