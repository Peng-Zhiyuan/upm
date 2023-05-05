using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaycastUtil 
{

    public static bool useRaycast = true;
    public static Vector3 GetGroundPosition(Vector3 pos)
    {
        if (!useRaycast) return pos;
#if UNITY_EDITOR
        Debug.DrawLine(pos + Vector3.up * 2f, pos + Vector3.down * 2f, Color.red);
#endif
        RaycastHit[] hit = Physics.RaycastAll(pos + Vector3.up * 2f, Vector3.down, 4f, LayerMask.GetMask("Road"));//起始位置、方向、距离
        if (hit != null && hit.Length > 0)
        {
//            Debug.LogError(pos+"=>"+ hit[0].point+ "   hit.Length:"+ hit.Length);
            return hit[0].point;
        }
        return pos;
    }
}
