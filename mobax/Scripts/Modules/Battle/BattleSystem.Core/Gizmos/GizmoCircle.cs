using UnityEngine;

namespace BattleSystem.Core
{
    public class GizmoCircle : IGizmo
    {
        private Vector3 center = Vector3.zero;
        private float r = 0;
        private Color c;

        public GizmoCircle(Vector3 pos, float radius, Color color)
        {
            center = pos;
            r = radius;
            c = color;
        }
        public void OnDrawGizmos(Vector3 pos)
        {
            GizmosTool.DrawCircle(center, r, Vector3.zero, c);
        }
    }

}
