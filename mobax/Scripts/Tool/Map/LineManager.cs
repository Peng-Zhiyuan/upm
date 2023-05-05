using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class LineManager : MonoBehaviour
{
    public Transform MapRoot;
    public Transform Mover;
    public Transform Train;
    public float Speed = 100f;

    public List<Vector3> points = new List<Vector3>();

    private bool bLoaded = false;
    // Start is called before the first frame update
    void Start()
    {
        this.Train.SetActive(false);
        if (MapRoot == null)
            MapRoot = transform.parent;
        LoadData();
        Train.eulerAngles = Mover.eulerAngles;
    }

    private async Task LoadData()
    {
        await Task.Delay(2000);
        bLoaded = true;
        bool bFirst = true;
        Vector3 lastpoint = Vector3.zero;
        var componts = MapRoot.GetComponentsInChildren<PointController>();
        if (componts.Length == 0)
        {
            this.SetActive(false);
            return;
        }
        foreach (var VARIABLE in componts)
        {
            var list = VARIABLE.GetPath();
            if (bFirst)
            {
                bFirst = false;
                lastpoint = list.First();
            }

            {
                list.Sort((l, r) =>
                {
                    if (Vector3.Distance(lastpoint, l) > Vector3.Distance(lastpoint, r))
                        return 1;
                    else
                    {
                        return -1;
                    }
                }); 
               
                lastpoint = list.Last();
            }
            
            points.AddRange(list);
        }
        
        Train.position = points.First();
        Mover.position = points.First();
        var dir = (points[1] - points[0]).normalized;
        Mover.forward = dir;
        Train.forward = dir;

        float len = 0;
        for(int i = 0; i < points.Count - 1; i++)
        {
            len += Vector3.Distance(points[i], points[i + 1]);
        }

        this.Train.SetActive(true);
        float durT = len / Speed;
        Mover.DOPath(points.ToArray(), durT, PathType.Linear).SetLookAt(0).SetEase(Ease.Linear).SetLoops(-1);
        Mover.SetActive(false);
    }

    public void MoveOver()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(!bLoaded)
            return;
        //Train.LookAt(Mover.transform);
        //Train.eulerAngles = Vector3.Slerp(Train.eulerAngles, Mover.eulerAngles, Time.deltaTime * 1f);
        Train.position = Vector3.Slerp(Train.position, Mover.position, Time.deltaTime);
        //Train.position = Mover.position;
        
        Quaternion tmp_sourceRotation = Train.transform.localRotation;
        Quaternion tmp_targetRotation = Quaternion.LookRotation(Mover.forward, Vector3.up);
        Quaternion tmp_rotated = Quaternion.Lerp(tmp_sourceRotation, tmp_targetRotation, Time.deltaTime * 1f);
        Train.transform.localRotation = tmp_rotated;
    }

    private void OnDrawGizmos()
    {
        //UnityEditor.Handles.DrawWireArc(Vector3.zero, Vector3.up, new Vector3(1, 0, 0), 30, 1);
    }
}
