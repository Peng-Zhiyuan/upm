using UnityEngine;

namespace BattleSystem.Core
{
    public class GizmoLine : IGizmo
    {
        public Vector3 startPoint = Vector3.zero;
        public Vector3 endPoint = Vector3.zero;
        public Color c;

        public GizmoLine(Vector3 sPoint, Vector3 ePoint, Color color)
        {
            startPoint = sPoint;
            endPoint = ePoint;
            c = color;
        }
        public void OnDrawGizmos(Vector3 pos)
        {
            GizmosTool.DrawLine(startPoint, endPoint, c);
        }
    }
}