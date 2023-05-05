using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class PointController : MonoBehaviour
{
    [LabelText("类型 0: 直线 1：圆形")]
    public int type = 0;
    
    [LabelText("直线起点")]
    public Transform LineStartPoint;
    [LabelText("直线终点")]
    public Transform LineEndPoint;
    
    //[LabelText("直线型起始点")]
    //public List<GameObject> points = new List<GameObject>();

    [LabelText("转弯型起点")]
    public Transform StartPoint;
    [LabelText("转弯型终点")]
    public Transform EndPoint;
    [LabelText("转弯型圆心")]
    public Transform CenterPoint;
    //[LabelText("转弯型角度")]
    //public float Angel = 90f;
    [LabelText("转弯型分割段数，段数越大越圆滑")]
    public int MaxNum = 50;
    // Start is called before the first frame update

    public float parentScaleX = 1f;
    void Start()
    {
        //parentScaleX = transform.parent.parent.localScale.x;
    }

    public List<Vector3> GetPath()
    {
        float dir = transform.parent.localScale.x * transform.parent.localScale.z;
        List<Vector3> list = new List<Vector3>();
        if (LineStartPoint != null)
        {
            if (dir < 0)
            {
                list.Add(LineEndPoint.position);
            }
            else
            {
                list.Add(LineStartPoint.position);
            }
        }
        if (type == 1)
        {
            if(StartPoint == null || CenterPoint == null || EndPoint == null)
                return list;
            
            var v = Vector3.Dot((StartPoint.position - CenterPoint.position).normalized,
                (EndPoint.position - CenterPoint.position).normalized);
            float angle = Mathf.Acos(v);
            angle *= Mathf.Rad2Deg;
            
            if (transform.parent.localScale.x < 0)
                angle = -angle;
            if (transform.parent.localScale.z < 0)
                angle = -angle;
            
            for(int i = 0; i < MaxNum; i++)
            {
                var point = RotateRound(StartPoint.position, CenterPoint.position, Vector3.up, -angle * (i + 1) / MaxNum);
                list.Add(point);
            }
        }
        if (LineEndPoint != null)
        {
            if (dir < 0)
            {
                list.Add(LineStartPoint.position);
            }
            else
            {
                list.Add(LineEndPoint.position);
            }
        }
        
        this.SetActive(false);

        return list;
    }
    
    public Vector3 RotateRound(Vector3 position, Vector3 center, Vector3 axis, float angle)
    {
        return Quaternion.AngleAxis(angle, axis) * (position - center) + center;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if(StartPoint == null || CenterPoint == null || EndPoint == null)
            return;

        var v = Vector3.Dot((StartPoint.position - CenterPoint.position).normalized,
            (EndPoint.position - CenterPoint.position).normalized);
        float angle = Mathf.Acos(v);
        angle *= Mathf.Rad2Deg;
        var rad = Vector3.Distance(StartPoint.position, CenterPoint.position);

        //Debug.LogError(transform.parent.name);
        if (transform.parent.localScale.x < 0)
            angle = -angle;
        if (transform.parent.localScale.z < 0)
            angle = -angle;
        
        UnityEditor.Handles.color = Color.red;
        UnityEditor.Handles.DrawWireArc(CenterPoint.position, Vector3.up, (StartPoint.position - CenterPoint.position).normalized, -angle, rad);

        if (LineStartPoint != null && StartPoint!= null)
        {
            UnityEditor.Handles.DrawLine(LineStartPoint.position, StartPoint.position);
        }
        
        if (LineEndPoint != null && EndPoint!= null)
        {
            UnityEditor.Handles.DrawLine(LineEndPoint.position, EndPoint.position);
        }

        if (StartPoint == null && EndPoint == null && LineStartPoint != null && LineEndPoint != null)
        {
            UnityEditor.Handles.DrawLine(LineStartPoint.position, LineEndPoint.position);
        }
        
    }
#endif 
}
