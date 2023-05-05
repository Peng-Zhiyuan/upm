namespace CodeHero
{
    using UnityEngine;

    /// <summary>
    /// 辅助矩形，x 轴为正前方
    /// </summary>
    public class GizmosRectangle : MonoBehaviour
    {
#if UNITY_EDITOR
        public Vector3 Pivot = new Vector3(0, 0.01F, 0);
        public Vector3 Size = new Vector3(1, 1, 4);
        public Color onNormalColor = new Color(0, 1, 0, 0.5F);
        public Mesh mesh;

        private void Awake()
        {
            if (mesh == null)
            {
                Vector3[] vertices = { new Vector3(-1, 0, -1), new Vector3(-1, 0, 1), new Vector3(1, 0, 1), new Vector3(1, 0, -1), };
                int[] triangles = { 0, 1, 2, 0, 2, 3, };
                Vector2[] uvs = { new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, 0), };
                mesh = new Mesh()
                {
                                name = "default", vertices = vertices, uv = uvs, triangles = triangles,
                };
                mesh.RecalculateNormals();
            }
        }

        private void OnDrawGizmos()
        {
            this.Draw(onNormalColor);
        }

        void Draw(Color color)
        {
            Gizmos.color = color;
            Quaternion rotation = this.transform.rotation;
            Gizmos.matrix = Matrix4x4.TRS(this.transform.position, rotation, Vector3.one);
            Gizmos.DrawMesh(mesh, 0, Pivot, Quaternion.identity, Size);
            Gizmos.color = Color.red;
            Gizmos.DrawLine(Pivot, Pivot + Vector3.right);
            Gizmos.color = Color.green;
            Gizmos.DrawLine(Pivot, Pivot + Vector3.up);
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(Pivot, Pivot + Vector3.forward);
        }
#endif
    }
}