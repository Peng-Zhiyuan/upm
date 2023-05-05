using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[ExecuteInEditMode]
public class EditorExexuteBehaviour : MonoBehaviour
{
    protected virtual void RefreshNow()
    {

    }
#if (UNITY_EDITOR && !UNITY_EDITOR_DISABLE)
    public bool m_refreshNow = false;
    void Update()
    {
        if (Application.isPlaying) return;

        if (m_refreshNow)
        {
            m_refreshNow = false;
            DoRefresh(true);
        }
    }
    void DoRefresh(bool recursive)
    {
        if (m_refreshNow) m_refreshNow = false;
        this.RefreshNow();
    }
#endif
}
