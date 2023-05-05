namespace BattleEngine.Logic
{
    using PathfindingCore;
    using PathfindingCore.Util;
    using System.Collections.Generic;
    using UnityEngine;

    public static class PathUtil
    {
        public static Path CalculatePath(AstarPathCore astar, Vector3 startPoint, Vector3 targetPoint, OnPathDelegate callback = null)
        {
            if (astar == null) return null;
            if (callback != null)
            {
                var p = ABPath.Construct(astar, startPoint, targetPoint, callback);
                astar.StartPath(p);
                return p;
            }
            else
            {
                var p = ABPath.Construct(astar, startPoint, targetPoint, RunFunnelModifiers);
                astar.StartPath(p);
                return p;
            }
        }

        public static Vector3 PickRandomPoint(Vector3 detination, float radius)
        {
            var position = Random.insideUnitSphere * radius;
            position.y = 0;
            position += detination;
            return position;
        }

        public static GraphNode GetNearestWalkableNode(AstarPathCore astar, Vector3 position)
        {
            GraphNode node = astar.GetNearest(position, NNConstraint.Default).node;
            return node;
        }

        public static Vector3 GetNearestWalkablePos(AstarPathCore astar, Vector3 targetPos)
        {
            var targetNode = astar.GetNearest(targetPos);
            return targetNode.position;
        }

        public static Vector3 GetNearestEdgeDir(AstarPathCore astar, Vector3 targetPos)
        {
            var targetNode = astar.GetNearest(targetPos);
            var nearestPos = targetPos;
            Int3 v1;
            Int3 v2;
            Int3 v3;
            (targetNode.node as PathfindingCore.TriangleMeshNode).GetVertices(out v1, out v2, out v3);
            Vector3 vec1 = ((Vector3)v1);
            Vector3 vec2 = ((Vector3)v2);
            Vector3 vec3 = ((Vector3)v3);
            float d1 = LineUtil.Instance.ClosestSqrDistance(vec1, vec2, nearestPos);
            float d2 = LineUtil.Instance.ClosestSqrDistance(vec2, vec3, nearestPos);
            float d3 = LineUtil.Instance.ClosestSqrDistance(vec3, vec1, nearestPos);
            if (d1 <= d2
                && d1 <= d3)
            {
                return vec2 - vec1;
            }
            else if (d2 <= d1
                     && d2 <= d3)
            {
                return vec2 - vec3;
            }
            else
            {
                return vec3 - vec1;
            }
            /*		if (LineUtil.Instance.InSameLine(vec1, vec2, nearestPos)) return vec2 - vec1;
                    else if (LineUtil.Instance.InSameLine(vec2, vec3, nearestPos)) return vec2 - vec3;
                    else if (LineUtil.Instance.InSameLine(vec3, vec1, nearestPos)) return vec3 - vec1;
                    else
                    {
                        Debug.LogError("������Ч");
                        return Vector3.zero;
                    }*/
        }

        public static List<Int3> GetNearestTriangle(AstarPathCore astar, Vector3 targetPos)
        {
            var targetNode = astar.GetNearest(targetPos);
            Int3 v1;
            Int3 v2;
            Int3 v3;
            (targetNode.node as PathfindingCore.TriangleMeshNode).GetVertices(out v1, out v2, out v3);
            return new List<Int3>() { v1, v2, v3 };
        }

        public static Vector3 ? GetNearestReachablePos(AstarPathCore astar, Vector3 startPos, Vector3 endPos)
        {
            var targetNode = astar.GetNearest(endPos);
            var startNode = astar.GetNearest(startPos, NNConstraint.Default);
            if (!IsReachable(startNode.node, targetNode.node)) return null;
            return targetNode.position;
            /*		Vector3 dir = (Vector3)targetNode.node.position - targetNode.position;

                    return targetNode.position + dir.normalized;*/
        }

        //public static GraphNode GetNearestReachableNode(Vector3 startPos, Vector3 endPos, int nodeDistance)
        //{

        //    var endNode = AstarPathCore.active.GetNearest(endPos).node;
        //    return endNode;
        //}

        public static List<Vector3> GetPointsAroundPoint(AstarPathCore astar, Vector3 center, float r, int count)
        {
            List<Vector3> posList = new List<Vector3>();
            for (int i = 0; i < count; i++)
            {
                float range = Mathf.Min(r * 0.5f, count);
                posList.Add(new Vector3(Random.Range(-range, range), 0, Random.Range(-range, range)));
            }
            PathUtilities.GetPointsAroundPoint(center, astar.graphs[0] as IRaycastableGraph, posList, r, 1f);
            return posList;
        }

        public static bool IsWalkable(AstarPathCore astar, Vector3 targetPos)
        {
            var constraint = NNConstraint.Default;
            var nnInfo = astar.GetNearest(targetPos, constraint);
            var targetNode = nnInfo.node;
            return targetNode != null && targetNode.Walkable;
        }

        public static bool IsReachable(AstarPathCore astar, Vector3 startPos, Vector3 endPos)
        {
            var endNode = astar.GetNearest(endPos, NNConstraint.Default);
            if (endNode.node == null) return false;
            var startNode = astar.GetNearest(startPos, NNConstraint.Default);
            return IsReachable(startNode.node, endNode.node);
        }

        //public static bool HasGround(Vector3 pos)
        //{
        //    RaycastHit hit;
        //    int layer = 1 << LayerMask.NameToLayer("Default") | 1 << LayerMask.NameToLayer("Road");
        //    if (Physics.Raycast(pos + Vector3.up, Vector3.down, out hit, 100, layer, QueryTriggerInteraction.Ignore))
        //    {
        //        return hit.point.y >= CoreConf.minGroundOffest;
        //    }
        //    return false;
        //}
        public static bool IsReachable(GraphNode startNode, GraphNode endNode)
        {
            if (startNode == null
                || endNode == null) return false;
            return PathUtilities.IsPathPossible(startNode, endNode);
        }

        public static void RunFunnelModifiers(Path p)
        {
            if (p.path == null
                || p.path.Count == 0
                || p.vectorPath == null
                || p.vectorPath.Count == 0)
            {
                return;
            }
            List<Vector3> funnelPath = ListPool<Vector3>.Claim();

            // Split the path into different parts (separated by custom links)
            // and run the funnel algorithm on each of them in turn
            var parts = Funnel.SplitIntoParts(p);
            if (parts.Count == 0)
            {
                // As a really special case, it might happen that the path contained only a single node
                // and that node was part of a custom link (e.g added by the NodeLink2 component).
                // In that case the SplitIntoParts method will not know what to do with it because it is
                // neither a link (as only 1 of the 2 nodes of the link was part of the path) nor a normal
                // path part. So it will skip it. This will cause it to return an empty list.
                // In that case we want to simply keep the original path, which is just a single point.
                return;
            }
            bool unwrap = true;
            bool splitAtEveryPortal = false;
            for (int i = 0; i < parts.Count; i++)
            {
                var part = parts[i];
                if (!part.isLink)
                {
                    var portals = Funnel.ConstructFunnelPortals(p.path, part);
                    var result = Funnel.Calculate(portals, unwrap, splitAtEveryPortal);
                    funnelPath.AddRange(result);
                    ListPool<Vector3>.Release(ref portals.left);
                    ListPool<Vector3>.Release(ref portals.right);
                    ListPool<Vector3>.Release(ref result);
                }
                else
                {
                    // non-link parts will add the start/end points for the adjacent parts.
                    // So if there is no non-link part before this one, then we need to add the start point of the link
                    // and if there is no non-link part after this one, then we need to add the end point.
                    if (i == 0
                        || parts[i - 1].isLink)
                    {
                        funnelPath.Add(part.startPoint);
                    }
                    if (i == parts.Count - 1
                        || parts[i + 1].isLink)
                    {
                        funnelPath.Add(part.endPoint);
                    }
                }
            }
            UnityEngine.Assertions.Assert.IsTrue(funnelPath.Count >= 1);
            ListPool<Funnel.PathPart>.Release(ref parts);
            // Pool the previous vectorPath
            ListPool<Vector3>.Release(ref p.vectorPath);
            p.vectorPath = funnelPath;
        }
    }
}