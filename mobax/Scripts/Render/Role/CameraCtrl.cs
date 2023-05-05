
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraCtrl : MonoBehaviour
{

    public Transform target;

    public static float rotateSpeed = 10f;  
    public static float rotateLerp = 8;

    public static float moveSpeed = 0.5f;
    public static float moveLerp = 10f;

    public static float zoomSpeed = 10f;   
    public static float zoomLerp = 4f;     

    private Vector3 position, targetPosition;
    
    private Quaternion rotation, targetRotation;
  
    private float distance, targetDistance;
   
    private const float default_distance = 5f;


    void Start()
    {
        targetRotation = Quaternion.identity;
        targetPosition = target.position;
        targetDistance = default_distance;
    }


    void Update()
    {
        float dx = Input.GetAxis("Mouse X");
        float dy = Input.GetAxis("Mouse Y");
        if (Input.GetMouseButton(2))
        {
            dx *= moveSpeed;
            dy *= moveSpeed;
            targetPosition -= transform.up * dy + transform.right * dx;
        }


        if (Input.GetMouseButton(1))
        {
            dx *= rotateSpeed;
            dy *= rotateSpeed;
            if (Mathf.Abs(dx) > 0 || Mathf.Abs(dy) > 0)
            {
                
                Vector3 angles = transform.rotation.eulerAngles;
                angles.x = Mathf.Repeat(angles.x + 180f, 360f) - 180f;
                angles.y += dx;
                angles.x -= dy;
                targetRotation.eulerAngles = new Vector3(angles.x, angles.y, 0);
            }
        }

  
        targetDistance -= Input.GetAxis("Mouse ScrollWheel") * zoomSpeed;
    }

    private void FixedUpdate()
    {
        rotation = Quaternion.Slerp(rotation, targetRotation, Time.deltaTime * rotateLerp);
        position = Vector3.Lerp(position, targetPosition, Time.deltaTime * moveLerp);
        distance = Mathf.Lerp(distance, targetDistance, Time.deltaTime * zoomLerp);
        transform.rotation = rotation;
        transform.position = position - rotation * new Vector3(0, 0, distance);
    }
}