namespace CodeHero
{
    using UnityEngine;

    public class GizmoIcon : MonoBehaviour
    {
#if UNITY_EDITOR
        public Vector3 offset = Vector3.up;
        public string iconName = "";
        private void OnDrawGizmos()
        {
            Gizmos.DrawIcon(this.transform.position + offset, iconName, true);
        }
#endif
    }
}