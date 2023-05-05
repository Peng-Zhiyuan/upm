using UnityEngine;

namespace BattleSystem.Core
{
    public class GizmoFlabellate : IGizmo
    {
        public int angle;
        private Vector3 forwardDir;
        private Vector3 center = Vector3.zero;
        private float r = 0;
        private Color c;
        private Vector3 centerPos;
        public GizmoFlabellate(Vector3 center, float radius, int angle, Vector3 forwardDir, Color color)
        {
            r = radius;
            this.angle = angle;
            this.centerPos = center;
            this.forwardDir = forwardDir;
            c = color;

        }
        public void OnDrawGizmos(Vector3 pos)
        {
            GizmosTool.DrawFlabellate(centerPos, r, Vector3.up * 0f, c, this.angle, this.forwardDir);
        }
    }
}