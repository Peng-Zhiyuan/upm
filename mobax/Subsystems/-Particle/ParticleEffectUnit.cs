using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ParticleEffectUnit : MonoBehaviour //RecycledGameObject
{
//     public List<ParticleEffectOffset> effectOffsetList;
//
//     public void Init(float radius)
//     {
//         for (int i = 0; i < effectOffsetList.Count; i++)
//         {
//             effectOffsetList[i].offset = radius;
//         }
//     }
//
// #if UNITY_EDITOR
//     public bool m_refreshNow = false;
//
//     void Update()
//     {
//         if (Application.isPlaying) return;
//         if (m_refreshNow)
//         {
//             m_refreshNow = false;
//             DoRefresh();
//         }
//     }
//
//     void DoRefresh()
//     {
//         if (m_refreshNow) m_refreshNow = false;
//         var list = GetComponentsInChildren<ParticleEffectOffset>(true);
//         for (int i = 0; i < list.Length; i++)
//         {
//             effectOffsetList.Add(list[i]);
//         }
//     }
// #endif
}