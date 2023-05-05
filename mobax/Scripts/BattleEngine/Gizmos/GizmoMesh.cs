namespace CodeHero
{
    using UnityEngine;

    public class GizmoMesh : MonoBehaviour
    {
#if UNITY_EDITOR
        public Mesh meshProxy;
        public Color color = new Color(1, 0, 1, 0.5F);
        public Vector3 offset = Vector3.zero;
        public Vector3 size = Vector3.one;
        private void OnDrawGizmos()
        {
            if (meshProxy == null)
            {
                meshProxy = this.GetComponent<MeshFilter>().sharedMesh;
            }
            Gizmos.matrix = Matrix4x4.TRS(this.transform.position, this.transform.rotation, size);
            Gizmos.color = color;
            Gizmos.DrawMesh(meshProxy, offset);
        }
#endif
    }
}