namespace CodeHero
{
    using BattleEngine.Logic;
    using UnityEngine;

    public class GizmosShape : MonoBehaviour
    {
        public GameObject follow;
        public SKILL_JUDGEX_SHAPE type = SKILL_JUDGEX_SHAPE.Cube;
        public Vector3 Size = Vector3.one;
        public Vector3 Center = Vector3.zero; // 中心偏移
        public float Radius = 1;
        public float OuterRadius = 1;
        public float Angle = 60;
        public float RotY = 0;

        private void OnDrawGizmos()
        {
            switch (type)
            {
                case SKILL_JUDGEX_SHAPE.Cube:
                    this.Cube();
                    break;
                case SKILL_JUDGEX_SHAPE.Sector:
                    this.Sector();
                    break;
                case SKILL_JUDGEX_SHAPE.Cylinder:
                    this.Cylinder();
                    break;
                case SKILL_JUDGEX_SHAPE.Annular:
                    this.Cylinder();
                    this.OuterCylinder();
                    break;
            }
        }

        void Cube()
        {
            Gizmos.color = Color.green;
            Vector3 cubeCenter = transform.position + Quaternion.Euler(0, transform.eulerAngles.y, 0) * new Vector3(Center.x, Center.y, Center.z + Size.z / 2);
            Gizmos.matrix = transform.localToWorldMatrix;
            Vector3 center = new Vector3(Center.x, Center.y, Center.z + Size.z / 2f);
            Gizmos.DrawWireCube(center, new Vector3(Size.x, Size.y, Size.z));
            Gizmos.DrawLine(center, new Vector3(center.x + Size.x / 2, center.y, center.z + Size.z / 2));
            Gizmos.color = Color.red;
            Gizmos.DrawLine(center, new Vector3(center.x, center.y, center.z + Size.z / 2));
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(center, new Vector3(center.x + Size.x / 2, center.y, center.z));
        }

        void Sector()
        {
            ////---- 扇形线框
            Gizmos.color = Color.green;
            Vector3 center = Center + transform.position;
            float curRot = 0;
            Vector3 up = new Vector3(0, Size.y, 0);
            Vector3 down = new Vector3(0, 0, 0);
            Vector3 dir = Quaternion.Euler(0, curRot + RotY, 0) * transform.forward;
            Vector3 fromDir = Quaternion.Euler(0, -Angle / 2, 0) * dir;
            Vector3 toDir = Quaternion.Euler(0, Angle / 2, 0) * dir;
            Vector3 pFrom = center + fromDir.normalized * Radius;
            Vector3 pTo = center + toDir.normalized * Radius;
            Gizmos.DrawLine(center + up, pFrom + up);
            Gizmos.DrawLine(center + up, pTo + up);
            GizmosTools.DrawWireSemicircle(center + up, dir, Radius, (int)Angle);
            Gizmos.DrawLine(center + down, pFrom + down);
            Gizmos.DrawLine(center + down, pTo + down);
            GizmosTools.DrawWireSemicircle(center + down, dir, Radius, (int)Angle);
            Gizmos.DrawLine(center + up, center + down);
            Gizmos.DrawLine(pFrom + up, pFrom + down);
            Gizmos.DrawLine(pTo + up, pTo + down);
            Gizmos.DrawLine(center + dir.normalized * Radius + up, center + dir.normalized * Radius + down);
        }

        void Cylinder()
        {
            Gizmos.color = Color.green;
            Vector3 center = Center + transform.position;
            Vector3 up = new Vector3(0, Size.y, 0);
            Vector3 down = new Vector3(0, 0, 0);
            GizmosTools.DrawWireSemicircle(center + up, transform.forward, Radius, 360, transform.up);
            GizmosTools.DrawWireSemicircle(center + down, transform.forward, Radius, 360, transform.up);
            Vector3 p1 = center + (new Vector3(1, 0, 0)).normalized * Radius;
            Vector3 p2 = center + (new Vector3(-1, 0, 0)).normalized * Radius;
            Vector3 p3 = center + (new Vector3(0, 0, -1)).normalized * Radius;
            Vector3 p4 = center + (new Vector3(0, 0, 1)).normalized * Radius;
            Vector3 p5 = center + (new Vector3(1, 0, 1)).normalized * Radius;
            Vector3 p6 = center + (new Vector3(-1, 0, -1)).normalized * Radius;
            Vector3 p7 = center + (new Vector3(1, 0, -1)).normalized * Radius;
            Vector3 p8 = center + (new Vector3(-1, 0, 1)).normalized * Radius;
            Gizmos.DrawLine(p1 + up, p1 + down);
            Gizmos.DrawLine(p2 + up, p2 + down);
            Gizmos.DrawLine(p3 + up, p3 + down);
            Gizmos.DrawLine(p4 + up, p4 + down);
            Gizmos.DrawLine(p5 + up, p5 + down);
            Gizmos.DrawLine(p6 + up, p6 + down);
            Gizmos.DrawLine(p7 + up, p7 + down);
            Gizmos.DrawLine(p8 + up, p8 + down);
        }

        void OuterCylinder()
        {
            Gizmos.color = Color.green;
            Vector3 center = Center + transform.position;
            Vector3 up = new Vector3(0, Size.y, 0);
            Vector3 down = new Vector3(0, 0, 0);
            GizmosTools.DrawWireSemicircle(center + up, transform.forward, OuterRadius, 360, transform.up);
            GizmosTools.DrawWireSemicircle(center + down, transform.forward, OuterRadius, 360, transform.up);
            Vector3 p1 = center + (new Vector3(1, 0, 0)).normalized * OuterRadius;
            Vector3 p2 = center + (new Vector3(-1, 0, 0)).normalized * OuterRadius;
            Vector3 p3 = center + (new Vector3(0, 0, -1)).normalized * OuterRadius;
            Vector3 p4 = center + (new Vector3(0, 0, 1)).normalized * OuterRadius;
            Vector3 p5 = center + (new Vector3(1, 0, 1)).normalized * OuterRadius;
            Vector3 p6 = center + (new Vector3(-1, 0, -1)).normalized * OuterRadius;
            Vector3 p7 = center + (new Vector3(1, 0, -1)).normalized * OuterRadius;
            Vector3 p8 = center + (new Vector3(-1, 0, 1)).normalized * OuterRadius;
            Gizmos.DrawLine(p1 + up, p1 + down);
            Gizmos.DrawLine(p2 + up, p2 + down);
            Gizmos.DrawLine(p3 + up, p3 + down);
            Gizmos.DrawLine(p4 + up, p4 + down);
            Gizmos.DrawLine(p5 + up, p5 + down);
            Gizmos.DrawLine(p6 + up, p6 + down);
            Gizmos.DrawLine(p7 + up, p7 + down);
            Gizmos.DrawLine(p8 + up, p8 + down);
        }
    }
}