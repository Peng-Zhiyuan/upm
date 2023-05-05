using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[HelpURL("https://www.element3ds.com/thread-216743-1-1.html")]
public class E3D_BB : MonoBehaviour
{
    public E3D_BillBoard billboard = E3D_BillBoard.YLock;
    [HideInInspector]
    public int billboardIndex;
    public Camera mainCamera;
    public bool showLine = false;
    void Awake()
    {
        if (mainCamera == null)
            mainCamera = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();
    }

    Vector3 _tempLocalEulerAngles = Vector3.zero;
    void Update()
    {
        transform.LookAt(mainCamera.transform.position);
        switch (billboard)
        {
            case E3D_BillBoard.YLock:
                transform.localEulerAngles = new Vector3(0, transform.localEulerAngles.y, 0);
                break;
            case E3D_BillBoard.Free:
                break;
            default:
                break;
        }
    }

    void OnDrawGizmos()
    {
        if (!showLine) return;

        Gizmos.color = Color.red;
        if (mainCamera)
            Gizmos.DrawLine(transform.position, mainCamera.transform.position);

        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, transform.forward * 10);
    }
}

