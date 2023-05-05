using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ForwardPath : MonoBehaviour
{
    public bool ConnetLast = false;
    public bool ConnetNext = false;
    public List<Transform> Nodes = new List<Transform> ();
    List<Vector3> NodePoints = new List<Vector3>();
    List<float> IntervalLength = new List<float>();
    public List<WayPoint> SelectPoints = new List<WayPoint>();
    public PathSelectPoint PathSelectPoint;
    bool IsSelected = false;

    public Texture SelectTex;
    public Texture UnselectTex;
    //public Color SelectColor = Color.white; 
    //public Color UnselectColor = new Color(0.2745f, 0.2745f, 0.2745f);

    public Vector3 GetNodePoint (int index)
    {
        if (NodePoints.Count > index)
            return NodePoints[index];
        return Vector3.zero;
    }
    public Vector3 GetLastNodePoint()
    {
        return GetNodePoint(NodePoints.Count - 1);
    }

    public bool GetSelected ()
    {
        return IsSelected;
    }
    public void SetSelect (bool selected)
    {
        foreach (var s in SelectPoints) {
            s.SetSelect(selected);
        }
        if (Application.isPlaying) 
            Line.material.SetTexture("_MainTex", selected ? SelectTex : UnselectTex);
        else if (Line.sharedMaterial != null)
            Line.sharedMaterial.SetTexture("_MainTex", selected ? SelectTex : UnselectTex);

        IsSelected = selected;
    }
    public LineRenderer Line;
    public float EdgeOffset = 1;
    public List<ForwardPath> NextPaths = new List<ForwardPath> ();
    private void Awake()
    {
        if (Line == null) {
            var lineObject = new GameObject("LineRenderer", typeof(LineRenderer));
            Line = lineObject.GetComponent<LineRenderer>();
            Line.transform.parent = transform;
            InitMaterial();
        }
        if (Application.isPlaying) {
            var origMaterials = new List<Material>();
            Line.GetMaterials(origMaterials);
            InitMaterial();
        }

        Line.startColor = Color.white;
        Line.endColor = Color.white;
    }
    // instance new material
    void InitMaterial()
    {
        var m = new Material(Shader.Find("E3DEffect/URP/C1/Add"));
        m.SetTexture("_MainTex", IsSelected ? SelectTex : UnselectTex);
        m.SetColor("_MainColor", Color.white);
        var ms = new Material[1];
        ms[0] = m;
        Line.materials = ms;
    }

    public void ResetNodes ()
    {
        if (NodePoints.Count != Nodes.Count)
            NodePoints.Clear();

        PathLength = 0;
        for (var i = 0; i < Nodes.Count; i++) {
            var node = Nodes[i];
            if (node == null)
                return;
            if (NodePoints.Count < Nodes.Count)
                NodePoints.Add(Vector3.zero);
            NodePoints[i] = node.position;
            
            // Calculate length
            if (i > 0) {
                var lastPoint = NodePoints[i - 1];
                var l = (NodePoints[i] - lastPoint).magnitude;
                if (IntervalLength.Count < i)
                    IntervalLength.Add(0);
                IntervalLength[i - 1] = l;
                PathLength += l;
            }
        }

        var nodes = new List<Vector3>();
        // first & end point offset
        for (var i = 0; i < NodePoints.Count; i++) {
            var node = NodePoints[i];
            if (NodePoints.Count > 1) {
                if (i == 0 && !ConnetLast)
                    node = NodePoints[i] + (NodePoints[i + 1] - NodePoints[i]).normalized * EdgeOffset;
                else if (i == NodePoints.Count - 1 && !ConnetNext)
                    node = NodePoints[i] + (NodePoints[i - 1] - NodePoints[i]).normalized * EdgeOffset;
            }
            nodes.Add(node);
        }
        Line.SetVertexCount(NodePoints.Count);
        Line.SetPositions(nodes.ToArray());

        if (PathSelectPoint != null) {
            PathSelectPoint.transform.position = NodePoints[NodePoints.Count - 1];
        }
    }

    public void Init(bool show)
    {
        this.index = -1;
        this.gameObject.SetActive(show);
    }
    private int index = -1;
    float minThrethold = 0.0001f;
    public Vector3 GetCurrentPoint (float distance, bool triggerEvent, float delta = 0)
    {
        var result = Vector3.zero;
        int i = 0;
        var length = distance;
        for (; i < IntervalLength.Count; i++) {
            var l = IntervalLength[i];
            length -= l;

            if (length <= minThrethold) {
                length += l;
                var beginPoint = NodePoints[i];
                var endPoint = NodePoints[i + 1];
                if (triggerEvent && index != i)
                {

                    //走进端事件
                    index = i;
                    EventManager.Instance.SendEvent<string>("BeginPathEvent", this.gameObject.name + "_" + this.Nodes[index].name);
                }
               
                if (triggerEvent && l - length < delta)
                {
                    //走出端事件
                    EventManager.Instance.SendEvent<string>("PathEvent", this.gameObject.name + "_" + this.Nodes[index + 1].name);
                }
                result = Vector3.Lerp(beginPoint, endPoint, length / l);
                break;
            }
        }

        return result;
    }
    float PathLength = -1;
    public float GetPathLength()
    {
        return PathLength;
    }
    public ForwardPath GetNextPath()
    {
        foreach (var next in NextPaths) {
            if (next.IsSelected)
                return next;
        }
        return null;
    }

    private void Update()
    {
        if (Application.isPlaying)
            return;
        if (NodePoints.Count != Nodes.Count)
            ResetNodes();

        for (int i = 0; i < Nodes.Count; i++) {
            var node = Nodes[i];
            if (node == null)
                break; ;
            if (node.position != NodePoints[i]) {
                ResetNodes();
                break;
            }
        }
    }
}
