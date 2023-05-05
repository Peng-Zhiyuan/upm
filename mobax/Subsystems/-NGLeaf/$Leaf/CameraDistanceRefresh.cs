using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraDistanceRefresh : MonoBehaviour
{

    public Material mat;
    // Update is called once per frame
    void Update()
    {
        if (this.mat == null) return;
      
        if (Camera.main != null)
        {
            float distance = Vector3.Distance(Camera.main.GetPosition(), this.transform.position);
            this.mat.SetFloat("_CameraDistance", distance);
           // Debug.LogError("distance:"+ distance);
        }
        else
        {
            this.mat.SetFloat("_CameraDistance", 10);
        }
    }
}
