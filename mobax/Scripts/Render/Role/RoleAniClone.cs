using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Spine.Unity;
public class RoleAniClone : MonoBehaviour
{
    public SkeletonAnimation masterAni;
    public SkeletonAnimation[] followAni;
    private string lastAni = "";
    public string[] ignoreList;
    void Update()
    {
        if (masterAni == null)
        {
            return;
        }

        if (masterAni.AnimationName != lastAni)
        {
            lastAni = masterAni.AnimationName;
            if (ignoreList.Contains(lastAni))
            {
                return;
            }
            for (int i = 0; i < followAni.Length; ++i)
            {
                followAni[i].AnimationName = lastAni;
            }
        }
    }
}
