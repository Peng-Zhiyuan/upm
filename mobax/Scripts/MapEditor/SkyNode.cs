using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkyNode : MonoBehaviour
{
    // follow main camera
    void Update()
    {
        if (Camera.main == null)
            return;


        transform.position = new Vector3(
            Camera.main.transform.position.x,
            transform.position.y,
            Camera.main.transform.position.z
        );
    }
}
