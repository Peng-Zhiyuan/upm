using UnityEngine;

namespace BattleSystem.Core
{
    public class GizmoRect : IGizmo
    {
        private Vector3[] points;
        private Color c;
        public GizmoRect(Color color, params Vector3[] pos)
        {
            this.points = pos;
            this.c = color;
        }

        public void OnDrawGizmos(Vector3 pos)
        {
            GizmosTool.DrawRect(points, Color.red);
        }
    }
}
