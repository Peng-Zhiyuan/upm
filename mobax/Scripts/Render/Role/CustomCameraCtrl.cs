using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomCameraCtrl : MonoBehaviour
{
    private Touch oldTouch1;  //上次触摸点1(手指1)
    private Touch oldTouch2;  //上次触摸点2(手指2)
    // 模型
    private Transform m_Target;
    // 旋转速度
    public static float rotateSpeed = 10f; 
    // 移动速度
    public static float moveSpeed = 0.15f;
    // 镜头拉伸速度
    public static float zoomSpeed = 4f;   //速度比例因子

    public CustomCameraController cameraCtrl;
    //public static bool lockVDelta = true;
    //public static bool lockHDelta= true;
    public static bool lockCamera = true;

    public Transform Target
    {
        get 
        {
            return cameraCtrl.m_Target;
        }
    }
  
    void Update()
    {
        m_Target = Target;
        if (m_Target == null) return;
#if UNITY_EDITOR
        float dx = Input.GetAxis("Mouse X");
        float dy = Input.GetAxis("Mouse Y");

        // 鼠标左键移动
        if (Input.GetMouseButton(2))
        {
            dx *= moveSpeed;
            dy *= moveSpeed;
            cameraCtrl.CameraBiasDelta(-dx, -dy, 0);
        }

        // 鼠标右键旋转
        if (Input.GetMouseButton(1))
        {
            dx *= rotateSpeed;
            dy *= rotateSpeed;
            if (Mathf.Abs(dx) > 0 || Mathf.Abs(dy) > 0)
            {
                if (lockCamera)
                {
                    this.Target.transform.rotation = this.Target.transform.rotation * Quaternion.Euler(0, -dx, 0);
                }
                else 
                {
                    cameraCtrl.CameraHDelta(-dx);
                    cameraCtrl.CameraVDelta(-dy);
                }
            }
        }
        if (Input.GetAxis("Mouse ScrollWheel") != 0)
        {
            // 鼠标滚轮拉伸
            var dz = Input.GetAxis("Mouse ScrollWheel") * zoomSpeed;
            cameraCtrl.CameraBiasDelta(0,0, -dz);
        }
#else
         if (Input.touchCount <= 0)
         {
            //Debug.LogError("没有手指触屏");
            return;
         }
           
        if (Input.touchCount == 1) //单点触碰移动摄像机
        {
            Touch touch = Input.GetTouch(0);
            if (Input.touches[0].phase == TouchPhase.Moved) //手指在屏幕上移动，移动摄像机
            {
                float dx = touch.deltaPosition.x * Time.deltaTime * 10;
                float dy = touch.deltaPosition.y * Time.deltaTime * 10;
                if (lockCamera)
                {
                    this.Target.transform.rotation = this.Target.transform.rotation * Quaternion.Euler(0, -dx, 0);
                }
                else
                {
                    cameraCtrl.CameraHDelta(-dx);
                    cameraCtrl.CameraVDelta(-dy);
                }
              
            }
        }

        else if (Input.touchCount > 1)//多点触碰
        {
            //多点触摸, 放大缩小
            Touch newTouch1 = Input.GetTouch(0);
            Touch newTouch2 = Input.GetTouch(1);
            //第2点刚开始接触屏幕, 只记录，不做处理
            if (newTouch2.phase == TouchPhase.Began)
            {
                oldTouch2 = newTouch2;
                oldTouch1 = newTouch1;
                return;
            }
            //计算老的两点距离和新的两点间距离，变大要放大模型，变小要缩放模型
            float oldDistance = Vector2.Distance(oldTouch1.position, oldTouch2.position);
            float newDistance = Vector2.Distance(newTouch1.position, newTouch2.position);
            //两个距离之差，为正表示放大手势， 为负表示缩小手势
            float offset = newDistance - oldDistance;
            //放大因子， 一个像素按 0.01倍来算(100可调整)
            float scaleFactor = -offset * 2 / 100f;
            cameraCtrl.CameraBiasDelta(0, 0, scaleFactor);
            //记住最新的触摸点，下次使用
            oldTouch1 = newTouch1;
            oldTouch2 = newTouch2;
        }
#endif

    }
}