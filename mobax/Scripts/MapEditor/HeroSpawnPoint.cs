using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroSpawnPoint : MonoBehaviour
{
   

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        Gizmos.DrawIcon(transform.position + Vector3.up * 0.3f, "heroTag.png", true);
    }
#endif
}
