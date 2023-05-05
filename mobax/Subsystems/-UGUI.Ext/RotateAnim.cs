using UnityEngine;
using System.Collections;

public class RotateAnim : MonoBehaviour
{
    public float RPS = 1;
    public Vector3 Axis = Vector3.forward;
    float mTime;
    private void Update()
    {
        mTime += Time.deltaTime/Time.timeScale;;
        transform.RotateAround (transform.position, Axis,  RPS);
    }
}
