using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector.Editor;
using static CustomHandles;
using Sirenix.OdinInspector;
using System.Linq;
using System.Threading.Tasks;

[CustomEditor(typeof(PathRouteConfig))]
public class PathRouteConfigEditor : OdinEditor
{
    public bool ShowControlId = false;
    public Texture SelectTex;
    public Texture UnselectTex;
    PathSelectPoint PathSelectPointPrefab;

    PathRouteConfig RouteConfig;
    public PathLinePoint SelectedPoint = null;
    SerializedObject SerializedObject;
    SerializedProperty SelectTexProperty;
    SerializedProperty SelectPointProperty;
    [OnInspectorInit]
    protected override void OnEnable()
    {
        base.OnEnable();
        RouteConfig = (PathRouteConfig)target;

        SerializedObject = new SerializedObject(this);
        SelectTexProperty = SerializedObject.FindProperty("SelectTex");

        SelectTex = AssetDatabase.LoadAssetAtPath("Assets/Art/Textures/Effect/Effect_Rope006.png", typeof(Texture)) as Texture;
        UnselectTex = AssetDatabase.LoadAssetAtPath("Assets/Art/Textures/Effect/Effect_Rope007.png", typeof(Texture)) as Texture;
    }
    

    public override void OnInspectorGUI()
    {
        if (SelectedPoint != null ) {
            //SelectPointProperty = SerializedObject.FindProperty("SelectedPoint");
            //EditorGUILayout.PropertyField(SelectPointProperty, new GUIContent("Selected Point"), true);
        }

        //EditorGUILayout.PropertyField(SelectTexProperty);
        //EditorGUILayout.PropertyField("SelectTex", SelectTex,typeof (Texture), false);
        //EditorGUILayout.ObjectField("UnselectTex", SelectTex,typeof (Texture), false);
        
        DrawDefaultInspector();
        IsPaintMode = GUILayout.Toggle(IsPaintMode, "Paint Mode", "Button");
        // Operations
        PaintMode(IsPaintMode);
        if (GUILayout.Button ("Build")) {
            RebuildPathRoute();
        }
    }
    public bool IsPaintMode = false;

    int SelectedIndex = -1;
    bool IsDragging = false;
    Vector3 endPos = Vector3.zero;
    Color LinkColor = Color.red;
    Color PathColor = new Color (0.298f, 1, 1);

    Dictionary<PathLinePoint, int> PointControlIDMap = new Dictionary<PathLinePoint, int>();
    void OnSceneGUI()
    {
        if (IsPaintMode)
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
        
        foreach (var kv in RouteConfig.Points) {
            var PointIndex = kv.Key;
            DragHandleResult dragResult = DragHandleResult.none;
            var p = RouteConfig.Points[PointIndex];
            // point
            var pos = DragHandle(p.Point, 0.3f, Handles.SphereHandleCap, LinkColor, out dragResult);
            if (!PointControlIDMap.ContainsKey(p))
                PointControlIDMap.Add(p, 0);
            PointControlIDMap[p] = lastDragHandleID;
            // point label
            var nextPoints = "";
            for (int i = 0; i < p.LinkedLineIndies.Count; i++) {
                var linked = p.LinkedLineIndies[i];
                nextPoints += linked;
                if (i != p.LinkedLineIndies.Count - 1)
                    nextPoints += ",";
            }
            Handles.Label(p.Point+Vector3.back * 0.1f, string.Format ("{0}{1}[{2}]", p.Index, ShowControlId ? string.Format ("{0}",PointControlIDMap[p]) : "", nextPoints));

            switch (dragResult) {
                case DragHandleResult.LMBDrag:
                    IsDragging = true;
                    endPos = pos;
                    break;
                case DragHandleResult.LMBRelease:
                    IsDragging = false;
                    // start & end controlId 
                    var startPoint = PointControlIDMap.FirstOrDefault(item => item.Value == lastDragHandleID).Key;
                    var endPoint = PointControlIDMap.FirstOrDefault(item => item.Value == HandleUtility.nearestControl).Key;
                    OperateLink(startPoint, endPoint);
                    break;
                case DragHandleResult.LMBClick:
                    SelectedIndex = PointIndex;
                    SelectedPoint = RouteConfig.Points.ContainsKey(SelectedIndex) ? RouteConfig.Points[SelectedIndex] : null;
                    //SelectedPointSerializedObjec = new SerializedObject(SelectedPoint);
                    break;
            }
        }

        foreach (var kv in RouteConfig.Points) {
            var PointIndex = kv.Key;
            var p = kv.Value;
            if (p.LinkedLineIndies.Count > 0) {
                var beginPoint = p.Point;
                var invalidIndies = new List<int>();
                for (int j = 0; j < p.LinkedLineIndies.Count; j++) {
                    var arrowId = GUIUtility.GetControlID (FocusType.Passive);
                    var linkedIndex = p.LinkedLineIndies[j];
                    var linkedPoint = RouteConfig.Points.ContainsKey(linkedIndex) ? RouteConfig.Points[linkedIndex] : null; 
                    // draw arrow
                    if (linkedPoint != null) {
                        var dir = (linkedPoint.Point - beginPoint);
                        GizmosHelper.ArrowForHandle(beginPoint, dir, PathColor, 0.4f, 25);
                    } 
                    // check invalid link
                    else
                        invalidIndies.Add(linkedIndex);
                }
                foreach (var r in invalidIndies) {
                    p.LinkedLineIndies.Remove(r);
                }
            }
        }
        var removePoints = new List<PathLinePoint>();
        // remove invalid point in map
        foreach (var p in PointControlIDMap.Keys) {
            if (!RouteConfig.Points.ContainsKey(p.Index))
                removePoints.Add(p);
        }
        foreach (var p in removePoints) {
            PointControlIDMap.Remove(p);
        }

        if (IsDragging) {
            Handles.color = LinkColor;
            Handles.DrawLine(WorldStartPos, endPos);
            Handles.Label(endPos, endPos.ToString());
        }

        foreach (var kv in RouteConfig.Points) {
            if (SelectedIndex == kv.Key)
                kv.Value.Point = Handles.PositionHandle(kv.Value.Point, Quaternion.identity);
        }

        if (IsPaintMode) {
            Tools.current = Tool.None;
            Tools.viewTool = ViewTool.None;

            //if (Event.current.type == EventType.MouseDown) Event.current.Use();
            //if (Event.current.type == EventType.MouseMove) Event.current.Use();
        }

        BuildPathLines();
    }

    void BuildPathLines ()
    {
        var pointPathsMap = new Dictionary<PathLinePoint, List<PathLine>>();
        var headLinkedCount = new Dictionary<PathLinePoint, int>();
        foreach (var p in RouteConfig.PointList) {
            foreach (var nextIndex in p.LinkedLineIndies) {
                if (RouteConfig.Points.Count <= nextIndex)
                    return;
                var next = RouteConfig.Points[nextIndex];
                if (!headLinkedCount.ContainsKey(next))
                    headLinkedCount.Add(next, 0);
                headLinkedCount[next]++;
            }
        }
        // entrance point
        var beginPoints = new List<PathLinePoint>() { RouteConfig.PointList[0] };
        var nextPoints = new List<PathLinePoint>();
        var pointSequnece = new List<PathLinePoint>();
        var pathLines = new List<PathLine>();
        // build forward paths
        do {
            nextPoints.Clear();
            foreach (var p in beginPoints) {
                pointSequnece.Clear();
                pointSequnece.Add(p); // first point
                foreach (var childIndex in p.LinkedLineIndies) {
                    if (RouteConfig.Points.Count <= childIndex)
                        return;
                    var childPoint = RouteConfig.Points[childIndex];
                    pointSequnece.Add(childPoint); // second point
                    var linkedCount = childPoint.LinkedLineIndies.Count;
                    // till split (1toN ot Nto1) or end
                    while (linkedCount == 1 && headLinkedCount[childPoint] == 1) {
                        var nIndex = childPoint.LinkedLineIndies[0];
                        var nPoint = RouteConfig.Points[nIndex];
                        pointSequnece.Add(nPoint);
                        childPoint = nPoint;
                        linkedCount = childPoint.LinkedLineIndies.Count;
                    }
                    // splt point queue to nextPoints
                    if ((linkedCount > 1 || headLinkedCount[childPoint] > 1) && !nextPoints.Contains(childPoint)) {
                        nextPoints.Add(childPoint);
                    }

                    if (pointSequnece.Count < 2) {
                        pointSequnece.Clear();
                        pointSequnece.Add(p); // first point
                        continue;
                    }
                    // build path from pointSequence
                    var pathline = new PathLine();
                    for (int i = 0; i < pointSequnece.Count; i++) {
                        var linePoint = pointSequnece[i];
                        pathline.Positions.Add(linePoint);

                        if (!pointPathsMap.ContainsKey(linePoint))
                            pointPathsMap.Add(linePoint, new List<PathLine>());
                        pointPathsMap[linePoint].Add(pathline);
                    }

                    pathLines.Add(pathline);

                    pointSequnece.Clear();
                    pointSequnece.Add(p); // first point
                }
            }
            beginPoints.Clear();
            beginPoints.AddRange(nextPoints);
        } while (beginPoints.Count > 0);
        RouteConfig.Lines = pathLines;
    }

    void OperateLink (PathLinePoint start, PathLinePoint end)
    {
        if (start == null || end == null || start == end)
            return;

        if (start.LinkedLineIndies.Contains(end.Index))
            start.LinkedLineIndies.RemoveAll(elem => elem == end.Index);
        else
            start.LinkedLineIndies.Add(end.Index);
    }

    bool PaintModeEnable;
    SceneView PaintModeView;
    private bool IsRestoreCamera2DMode;
    private Vector3 RestoreCamearPosition;
    void PaintMode (bool value)
    {
        if (PaintModeEnable != value) {
            if (value) {
                PaintModeView = SceneView.lastActiveSceneView;
                if (PaintModeView == null) return;
                IsRestoreCamera2DMode = PaintModeView.in2DMode;
                RestoreCamearPosition = PaintModeView.pivot;

                PaintModeView.LookAt(new Vector3(0, 10, 0), Quaternion.Euler (90, 0, 0));
                PaintModeView.Repaint();
            } else {
                PaintModeView.in2DMode = IsRestoreCamera2DMode;
                PaintModeView.LookAt(RestoreCamearPosition);
            }
            PaintModeEnable = value;
        }
    }

    public void RebuildPathRoute ()
    {
        if (RouteConfig.PointList.Count < 2)
            return;

        PathRoute root = null;
        var pointPathsMap = new Dictionary<PathLinePoint, List<ForwardPath>>();
        var transPointMap = new Dictionary<Transform, PathLinePoint>();
        var headLinkedCount = new Dictionary<PathLinePoint, int>();
        foreach (var p in RouteConfig.PointList) {
            foreach (var nextIndex in p.LinkedLineIndies) {
                var next = RouteConfig.Points[nextIndex];
                if (!headLinkedCount.ContainsKey(next))
                    headLinkedCount.Add (next, 0);
                headLinkedCount[next]++;
            }
        }
        // entrance point
        var beginPoints = new List<PathLinePoint>() { RouteConfig.PointList[0] };
        var nextPoints = new List<PathLinePoint>();
        var pointSequnece = new List<PathLinePoint>();
        var forwardPaths = new List<ForwardPath>();
        var pathLineIndex = 1;
        // build forward paths
        do {
            nextPoints.Clear();
            foreach (var p in beginPoints) {
                pointSequnece.Clear();
                pointSequnece.Add(p); // first point
                foreach (var childIndex in p.LinkedLineIndies) {
                    var childPoint = RouteConfig.Points[childIndex];
                    pointSequnece.Add(childPoint); // second point
                    var linkedCount = childPoint.LinkedLineIndies.Count;
                    // till split (1toN ot Nto1) or end
                    while (linkedCount == 1 && headLinkedCount[childPoint] == 1) {
                        var nIndex = childPoint.LinkedLineIndies[0];
                        var nPoint = RouteConfig.Points[nIndex];
                        pointSequnece.Add(nPoint);
                        childPoint = nPoint;
                        linkedCount = childPoint.LinkedLineIndies.Count;
                    }
                    // splt point queue to nextPoints
                    if ((linkedCount > 1 || headLinkedCount[childPoint] > 1) && !nextPoints.Contains (childPoint)) {
                        nextPoints.Add(childPoint);
                    }

                    if (pointSequnece.Count < 2) {
                        pointSequnece.Clear();
                        pointSequnece.Add(p); // first point
                        continue;
                    }
                    // build path from pointSequence
                    var pathLineGo = new GameObject("PathLine" + pathLineIndex++);
                    var forwardPath = pathLineGo.AddComponent<ForwardPath>();
                    forwardPath.SelectTex = SelectTex;
                    forwardPath.UnselectTex = UnselectTex;
                    forwardPath.EdgeOffset = 0.38f;
                    for  (int i = 0; i < pointSequnece.Count; i++) {
                        var linePoint = pointSequnece[i];
                        var node = new GameObject("Point" + ((p.Index+1) * 100 + i+1));
                        node.transform.parent = forwardPath.transform;
                        node.transform.position = linePoint.Point;
                        forwardPath.Nodes.Add (node.transform);

                        transPointMap.Add(node.transform, linePoint);
                        if (!pointPathsMap.ContainsKey(linePoint))
                            pointPathsMap.Add(linePoint, new List<ForwardPath>());
                        pointPathsMap[linePoint].Add (forwardPath);
                    }
                    forwardPath.ResetNodes();

                    if (root == null) {
                        var rootGo = new GameObject("PathRoute");
                        root = rootGo.AddComponent<PathRoute>();
                        root.IsShown = true;
                        root.Entrance = forwardPath;
                        
                    }
                    forwardPaths.Add(forwardPath);
                    pathLineGo.transform.parent = root.transform;

                    pointSequnece.Clear();
                    pointSequnece.Add(p); // first point
                }
            }
            beginPoints.Clear();
            beginPoints.AddRange (nextPoints);
        } while (beginPoints.Count > 0);

        // link next paths
        foreach (var forwardPath in forwardPaths) {
            var lastPointTrans = forwardPath.Nodes[forwardPath.Nodes.Count - 1];
            var lastPoint = transPointMap[lastPointTrans];

            var pointPaths = pointPathsMap[lastPoint];
            var connectPath = pointPaths.FindAll(pathOfPoint =>
            {
                if (pathOfPoint.Nodes.Count > 0) {
                    var firstPointTrans = pathOfPoint.Nodes[0];
                    var firstPoint = transPointMap[firstPointTrans];
                    return firstPoint == lastPoint;
                }
                return false;
            });

            forwardPath.NextPaths.AddRange(connectPath);
        }
        // select point
        SetPathSelectPoints(forwardPaths);

        root.Init();
        root.Config = RouteConfig;
    }

    async Task LoadPathPointAsync()
    {
        if (PathSelectPointPrefab == null) 
        {
            // pzy：建议更换为 AssetDatabase 的 API

            var address = "PathSelectPoint.prefab";
            //PathSelectPointPrefab = await AddressableRes.AquireAsync<PathSelectPoint>(address);
            var bucket = BucketManager.Stuff.Main;
            PathSelectPointPrefab = await bucket.GetOrAquireAsync<PathSelectPoint>(address);
        } 
    }
    async void SetPathSelectPoints (List<ForwardPath> forwardPaths)
    {
        await LoadPathPointAsync();
        foreach (var forwardPath in forwardPaths) {
            if (forwardPath.NextPaths.Count > 0) {
                forwardPath.PathSelectPoint = NewPathSelectPoint();
                forwardPath.PathSelectPoint.transform.parent = forwardPath.transform;
                forwardPath.PathSelectPoint.transform.position = forwardPath.GetLastNodePoint();
            }
        }
    }

    PathSelectPoint NewPathSelectPoint ()
    {
        if (PathSelectPointPrefab != null) {
            return Instantiate(PathSelectPointPrefab);
        }

        return null;
    }
}
