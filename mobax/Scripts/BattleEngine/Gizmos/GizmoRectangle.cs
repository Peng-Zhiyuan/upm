namespace CodeHero
{
    using UnityEngine;

    public class GizmoRectangle : MonoBehaviour
    {
        public Bounds volume;
        public Color color = Color.red;

        private void OnDrawGizmos()
        {
            Gizmos.matrix = this.transform.localToWorldMatrix;
            Gizmos.color = color * 0.25F;
            Gizmos.DrawCube(volume.center, volume.size);
            Gizmos.color = color;
            Gizmos.DrawWireCube(volume.center, volume.size);
        }
    }
}