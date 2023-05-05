
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class PathRoute : MonoInstance<PathRoute>
{
    public PathRouteConfig Config;
    public ForwardPath Entrance;
    public GameObject AimPoint;
    public float AimSpeed = 5;
    public bool IsShown = false;
    private void Start()
    {
        _ins = this;
        Init();
       // Debug.LogError("Start");
    }

    List<ForwardPath> InitedPath = new List<ForwardPath>();
    List<ForwardPath> Paths = new List<ForwardPath>();
    public void Init()
    {

        AimPointDistance = 0;
        PathLength = 0;
        if (Entrance == null) return;
        Entrance.SetSelect (true);
        InitPath(Entrance, true);

        //IsShown = false;
        foreach (var path in InitedPath) {
            PathLength += path.GetPathLength();
            
        }
        foreach (var path in Paths) {
            path.Init(IsShown);
        }
        //if (AimPoint != null)
        //    AimPoint.SetActive(IsShown);

        GameEventCenter.AddListener(GameEvent.PlayingStage,this, OnShowPath);
    }
    public void OnReset()
    {
        AimPointDistance = 0;
        PathLength = 0;
        InitedPath.Clear();
        Entrance.SetSelect(true);
        InitPath(Entrance, false);

        CalcuPathLength();
        foreach (var path in Paths) {
            path.Init(IsShown);
        }
        if (AimPoint != null)
            AimPoint.SetActive(IsShown);
    }
    void OnShowPath(object[] eventData)
    {

        var param = (int) eventData[0];
        if (param != 0)
            return;

        foreach (var path in Paths) {
            path.gameObject.SetActive(true);
        }
        IsShown = true;
    }

    float PathLength = 0;
    public float GetPathLength ()
    {
        return PathLength;
    }
    void InitPath (ForwardPath p, bool isSelectFirst)
    {
        p.ResetNodes();
        var lastPos = p.GetLastNodePoint();
        for (var i = 0; i< p.NextPaths.Count; i++) {
            var next = p.NextPaths[i];
            // correct first pos by parent's last path node 
            next.Nodes[0].transform.position = lastPos;
            
            
            if (isSelectFirst && !next.GetSelected())
                next.SetSelect(i == 0 && p.GetSelected());
            InitPath(next, isSelectFirst);
            if (next.GetSelected() && p.PathSelectPoint != null)
                SetArrow(p.PathSelectPoint, next);
            if (next.GetSelected ()) {
                if (!InitedPath.Contains(next)) {
                    InitedPath.Add(next);
                }
            }
        }
        if (p.GetSelected () && !InitedPath.Contains(p)) {
            InitedPath.Add(p);
        }
        if (!Paths.Contains(p))
            Paths.Add(p);
    }
    void SetArrow (PathSelectPoint selectPoint, ForwardPath nextPath)
    {
        //selectPoint.SelectObject.SetActive(false);

        var nextPathPoint1 = nextPath.GetNodePoint(0);
        var nextPathPoint2 = nextPath.GetNodePoint(1);
        var direction = Vector3.Normalize(nextPathPoint2 - nextPathPoint1);
        var axis = Vector3.Cross(Vector3.right, direction);
        var radien = Mathf.Acos (Vector3.Dot(direction, Vector3.right));
        selectPoint.ArrowTrans.transform.rotation = Quaternion.AngleAxis(Mathf.Rad2Deg * radien, axis);
    }

    public void CalcuPathLength ()
    {
        PathLength = 0;
        foreach (var path in InitedPath) {
            PathLength += path.GetPathLength();
        }
    }

    public Vector3? FirstPoint
    {
        get 
        {
            List<Transform> nodes = this.Nodes;
            if (nodes.Count > 0)
            {
                return nodes[0].position;
            }
            return null;
        }
       
          
    }

    private List<Transform> nodes = new List<Transform>();

    public List<Transform> Nodes
    {
        get 
        {
            nodes.Clear();
            for (int i = InitedPath.Count - 1; i >= 0; i--)
            {
                var path = InitedPath[i];
                for (int j = 0; j < path.Nodes.Count; j++)
                {
                    if (nodes.Count == 0 || j > 0)
                    {
                        nodes.Add(path.Nodes[j]);
                    }
                }
            }
            return nodes;
        }
    }


    public Vector3? LastPoint
    {
        get
        {
            List<Transform> nodes = this.Nodes;
            if (nodes.Count > 0)
            {
                return nodes[nodes.Count - 1].position;
            }
            return null;
        }
       
    }

    public Vector3 GetNextPos(Vector3 pos, out int index)
    {
        
        List<Transform> nodes = this.Nodes;
        index = nodes.Count - 1;
        var result = nodes[index].position;
        var closed = float.MaxValue;
        Vector3 closedPos = nodes[index].position;
        for (int i = 0; i < nodes.Count-1 ; i++)
        {
            var curNodePos = nodes[i].position;
            var nextNodePos = nodes[i + 1].position;

            var closestPoint = ClosestPoint(curNodePos, nextNodePos, pos);
            var distance = Vector3.Magnitude(pos - closestPoint);
            //当两个线段相近时，优先使用后面的优先系数0.2f
            if (distance <= closed + 0.2f) {
                closed = distance;
                result = nextNodePos;
                closedPos = closestPoint;
                index = i;
            }
        }
        return  closed < 1 ? result: closedPos;
    }
    public float GetProgress(Vector3 pos)
    {

        List<Transform> nodes = new List<Transform>();
        for (int i = InitedPath.Count - 1; i >= 0; i--) {
            var path = InitedPath[i];
            for (int j = 0; j < path.Nodes.Count; j++) {
                if (nodes.Count == 0 || j > 0) {
                    nodes.Add(path.Nodes[j]);
                }
            }
        }
        var length = 0f;
        var lastPoint = nodes[0].position;
        var closed = float.MaxValue;
        for (int i = 0; i < nodes.Count - 1; i++) {
            var curNodePos = nodes[i].position;
            var nextNodePos = nodes[i + 1].position;

            var closestPoint = ClosestPoint(curNodePos, nextNodePos, pos);
            var distance = Vector3.Magnitude(pos - closestPoint);
            if (distance < closed) {
                closed = distance;
                length = Vector3.Magnitude(closestPoint - curNodePos);
                lastPoint = curNodePos;
            }
        }
        for (int i = 0; i < nodes.Count - 1; i++) {
            var node = nodes[i];
            if (node.position == lastPoint)
                break;

            var nextNode = nodes[i + 1];
            length += Vector3.Magnitude(nextNode.position - node.position);
        }

        return length;
    }

    Vector3 ClosestPoint(Vector3 a, Vector3 b, Vector3 p)
    {
        var m = b - a;
        var t = Vector3.Dot(p - a, m) / Vector3.Dot(m, m);
        var c = a + t * m;
        var exceed = Vector3.Dot(c - a, b - c) < 0;
        if (exceed) {
            var d1 = Vector3.SqrMagnitude(p - a);
            var d2 = Vector3.SqrMagnitude(p - b);
            return d1 < d2 ? a : b;
        }
        return c;

    }

    void CacheSelectedPath (ForwardPath p)
    {
        for (var i = 0; i < p.NextPaths.Count; i++) {
            var next = p.NextPaths[i];
            CacheSelectedPath(next);
            if (next.GetSelected()) {
                if (!InitedPath.Contains(next)) {
                    InitedPath.Add(next);
                }
            }
        }
        if (p.GetSelected() && !InitedPath.Contains(p)) {
            InitedPath.Add(p);
        }
    }

    /*   public void CheckEvent(float distance)
       {
           //var dir = GetCurrentPoint(distance);
           var path = hitInfo.collider.gameObject.GetComponent<ForwardPath>();
           if (path != null)
           {
               for (var i = 0; i < Entrance.NextPaths.Count; i++)
               {
                   if (LockPath)
                   {
                       Debug.LogError("路线冻结！");
                       return;
                   }
                   var p = Entrance.NextPaths[i];

                   p.SetSelect(p == path);
                   if (p == path)
                       GameEventCenter.Broadcast(GameEvent.MiniSelection, i);
               }
           }
       }*/

    float currentDistance = 0;
    public void SyncDistance(float distance)
    {
        currentDistance = distance;
    }
    public Vector3 GetCurrentPoint(float distance, bool triggerEvent = false,float delta = 0)
    {
        if(triggerEvent) currentDistance = distance;
        return GetPoint(distance, triggerEvent, delta);
    }

    Vector3 GetPoint (float distance, bool triggerEvent = false, float delta = 0)
    {
        var result = Vector3.zero;
        var length = distance;
        var currentPath = Entrance;
        var lastPath = Entrance;
        while (currentPath != null) {
            var l = currentPath.GetPathLength();
            length -= l;
            if (length < 0) {

                var curDis = length + l;
                result = currentPath.GetCurrentPoint(curDis, triggerEvent, delta);
                break;
            }
            lastPath = currentPath;
            currentPath = currentPath.GetNextPath();
        }

        if (currentPath == null && length >= 0)
        {
            result = lastPath.GetCurrentPoint(lastPath.GetPathLength(), triggerEvent, delta);
        }  
        return result;
    }

    float AimPointDistance = 0;
    private void Update()
    {
        if(!Battle.Instance.IsFight)
        {
            return;
        }

        if (false && Input.GetMouseButtonDown(0))    
        {
            DoRaycast();
        }

        if (AimPoint == null || !IsShown)
            return;
        if (AimPointDistance > PathLength)
            AimPointDistance = 0;

        AimPointDistance += Time.deltaTime * AimSpeed;
        AimPoint.transform.position = GetPoint(AimPointDistance);
    }

    public bool LockPath
    {
        get 
        {
            return currentDistance > Entrance.GetPathLength();
        }
    }
    void DoRaycast()
    {
        RaycastHit hitInfo;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hitInfo, 1000, LayerMask.GetMask ("Map"))) {
            var path = hitInfo.collider.gameObject.GetComponent<ForwardPath> ();
            if (path != null) {
                StartCoroutine (SelectPath(path));
            }
        }

        OnReset();
    }

    IEnumerator SelectPath (ForwardPath path)
    {
        ForwardPath selected = null;
        for(var i = 0; i < Entrance.NextPaths.Count; i++) {
            if (LockPath)
            {
                Debug.LogError("路线冻结！");
                yield break;
            }
            var p = Entrance.NextPaths[i];
            // Hard code switch paths
            p.SetSelect(!p.GetSelected());

            if (p.GetSelected() && path.PathSelectPoint != null) {
                SetArrow(path.PathSelectPoint, p);
                selected = p;
            }
            // if (p == path)
            GameEventCenter.Broadcast(GameEvent.MiniSelection, i);
        }
        StartCoroutine(ArrowClickEffect(path.PathSelectPoint, 0.2f));
        yield return HighlightPathLine(selected, 0.6f);
    }

    IEnumerator ArrowClickEffect(PathSelectPoint selectPoint, float duration)
    {
        selectPoint.ArrowTrans.GetChild(0).localScale = Vector3.one * 1.2f;
        selectPoint.SelectObject.transform.GetChild(0).localScale = Vector3.one * 1.2f;
        yield return new WaitForSeconds(duration);
        selectPoint.ArrowTrans.GetChild(0).localScale = Vector3.one;
        selectPoint.SelectObject.transform.GetChild(0).localScale = Vector3.one;

    }
    IEnumerator HighlightPathLine (ForwardPath path, float duration)
    {
        Entrance.Line.material.SetColor("_MainColor", new Color(3f, 3f, 3f));
        path.Line.material.SetColor("_MainColor", new Color(3f, 3f, 3f));
        yield return new WaitForSeconds(duration);
        //var elapsed = 0f;
        //var fadeDur = 0.3f;
        //var fadeProgress = elapsed / fadeDur;
        //do {
        Entrance.Line.material.SetColor("_MainColor", Color.white);
        path.Line.material.SetColor("_MainColor", Color.white);
        //} while (fadeProgress < 1);

    }
}
