
using UnityEngine;

public class GizmosMonoBehaviour : MonoBehaviour
{
    public bool IsShown = true;
#if UNITY_EDITOR
    public virtual void OnGizmos() 
    {
        GizmosTool.drawString(gameObject.name, transform.position);
    }
    void OnDrawGizmos()
    {
        if (IsShown) {
            OnGizmos();
        }
    }
#endif
}
