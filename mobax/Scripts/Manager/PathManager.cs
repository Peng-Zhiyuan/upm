using UnityEngine;
using System.Collections;
using UnityEngine.AI;

public class PathManager : BattleComponent<PathManager>
{
    private NavMeshPath _path = new NavMeshPath();

    public Vector3[] GetPath(Vector3 from, Vector3 to)
    {
        if (NavMesh.CalculatePath(from, to, NavMesh.AllAreas, _path))
        {
            return _path.corners;
        }
        return null;
    }

    public Vector3 GetAnglePoint(Vector3 from, Vector3 target, float dis, float angle)
    {
        Vector3 newVec = Quaternion.Euler(0, angle, 0) * (from - target);
        return newVec.normalized * dis + target;
    }

    public bool IsEmpty(Vector3 pos, float offset, GameObject ignore = null)
    {
        bool res = true;
        Vector3 c = pos + new Vector3(offset / 2, 0, offset / 2);
        Vector3 b = pos + new Vector3(-offset / 2, 0, offset / 2);
        Vector3 d = pos + new Vector3(offset / 2, 0, -offset / 2);
        Vector3 a = pos + new Vector3(-offset / 2, 0, -offset / 2);
        return !IsHit(a, ignore) || !IsHit(b, ignore) || !IsHit(c, ignore) || !IsHit(d, ignore);
    }

    public bool IsHit(Vector3 pos, GameObject ignore = null)
    {
        Ray r = new Ray();
        RaycastHit hit;
        r.origin = pos;
        r.direction = Vector3.up;
        if (Physics.Raycast(r, out hit, 100f, LayerMask.GetMask("Char")))
        {
            if (hit.collider.gameObject != ignore)
                return true;
        }
        return false;
    }

    public Vector3 GetEmptyPoint(Vector3 target, Vector3 from, float dis, GameObject ignore = null)
    {
        if (IsEmpty(target, 0.2f))
            return target;
        float angle;
        for (int i = 0; i < 36; i++)
        {
            if (i / 2 == 0)
            {
                angle = Mathf.Ceil(i / 2) * 10f;
                Vector3 pos = GetAnglePoint(from, target, dis, angle);
                if (IsEmpty(pos, 0.2f, ignore))
                    return pos;
            }
            else
            {
                angle = -Mathf.Ceil(i / 2) * 10f;
                Vector3 pos = GetAnglePoint(from, target, dis, angle);
                if (IsEmpty(pos, 0.2f, ignore))
                    return pos;
            }
        }
        return target;
    }
}