using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum RunAwayType
{
    Translate,
    Run,


}
public class RunawayPoint : MonoBehaviour
{
    public RunAwayType runType = RunAwayType.Run;
    public float delay = 0;
#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        Gizmos.DrawIcon(transform.position + Vector3.up * 0.3f, "goalTag.png", true);
    }

#endif

}

