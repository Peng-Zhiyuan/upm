using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

public class PathRouteConfig : GizmosMonoBehaviour
{
    public Dictionary<int, PathLinePoint> Points = new Dictionary<int, PathLinePoint> ();
    [HideInInspector]
    public List<PathLine> Lines = new List<PathLine> ();
    [ListDrawerSettings (CustomAddFunction = "AddNewPoint", CustomRemoveElementFunction = "RemovePoint")]
    public List<PathLinePoint> PointList = new List<PathLinePoint>();

    [OnInspectorInit]
    void Awake()
    {
        if (PointList.Count == 0) {
            AddNewPoint();
        } 
        // reconstruct Points
        else {
            foreach (var p in PointList) {
                if (!Points.ContainsKey (p.Index)) {
                    Points.Add(p.Index, p);
                }
            }
        }
    }


    void AddNewPoint ()
    {
        var lastPoint = PointList.Count > 0 ? PointList[PointList.Count - 1] : null;
        var lastIndex = lastPoint != null ? lastPoint.Index : -1;
        var point = new PathLinePoint() { Index = lastIndex + 1};
        Points.Add(point.Index, point);
        PointList.Add(point);
        if (lastPoint != null) {
            point.Point = lastPoint.Point + new UnityEngine.Vector3(1,0,1);
            lastPoint.LinkedLineIndies.Add(point.Index);
        }
    }
    void RemovePoint (PathLinePoint p)
    {
        PointList.Remove(p);
        Points.Remove(p.Index);
    }
    PathLine NewEmptyPathLine()
    {
        var line = new PathLine();
        // 2 points
        line.Positions.Add(new PathLinePoint() { Index = 0});
        line.Positions.Add(new PathLinePoint() { Index = 1});
        return line;
    }

    
    public void OnGizmos()
    {
        
    }

    
}
