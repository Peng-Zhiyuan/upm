using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DistanceLine : MonoBehaviour
{
    // Start is called before the first frame update
    public float Distance = 10f;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDrawGizmos()
    {
        //throw new NotImplementedException();
        //Debug.LogError(Vector3.Distance(this.transform.position, this.transform.position + transform.forward * Distance));
        Gizmos.color = Color.red;
        
        
        Gizmos.DrawLine(this.transform.position, this.transform.position + transform.forward * Distance);
    }
}
