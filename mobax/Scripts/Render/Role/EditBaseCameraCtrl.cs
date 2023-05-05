using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditBaseCameraCtrl : MonoBehaviour
{
   
    private Transform m_Target;

    public static float rotateSpeed = 10f; 

    public static float moveSpeed = 0.15f;

    public static float zoomSpeed = 4f;   

    public RoleEditBase editBase;

    public Transform Target
    {
        get 
        {
            return editBase.CameraTarget;
        }
    }

   // private int isforward;//标记摄像机的移动方向
    //记录两个手指的旧位置
   // private Vector2 oposition1 = Vector2.zero;
   // private Vector2 oposition2 = Vector2.zero;

   // Vector2 m_screenPos = Vector2.zero; //记录手指触碰的位置
    private Touch oldTouch1;  //上次触摸点1(手指1)
    private Touch oldTouch2;  //上次触摸点2(手指2)

    //用于判断是否放大
/*    bool isEnlarge(Vector2 oP1, Vector2 oP2, Vector2 nP1, Vector2 nP2)
    {
        //函数传入上一次触摸两点的位置与本次触摸两点的位置计算出用户的手势
        float leng1 = Mathf.Sqrt((oP1.x - oP2.x) * (oP1.x - oP2.x) + (oP1.y - oP2.y) * (oP1.y - oP2.y));
        float leng2 = Mathf.Sqrt((nP1.x - nP2.x) * (nP1.x - nP2.x) + (nP1.y - nP2.y) * (nP1.y - nP2.y));
        if (leng1 < leng2)
        {
            //放大手势
            return true;
        }
        else
        {
            //缩小手势
            return false;
        }
    }*/

    void Start()
    {
        Input.multiTouchEnabled = true;//开启多点触碰
    }

    void Update()
    {
        m_Target = Target;
        if (m_Target == null) return;
#if UNITY_EDITOR

        float dx = Input.GetAxis("Mouse X");
        float dy = Input.GetAxis("Mouse Y");
        if (Mathf.Abs(dx) > 2 || Mathf.Abs(dy) > 2) return;//过滤鼠标点击其他windows异常
        if (Input.GetMouseButton(2))
        {

            dx *= moveSpeed;
            dy *= moveSpeed;
            editBase.CameraBiasDelta(-dx, -dy, 0);
        }

        if (Input.GetMouseButton(1))
        {

            dx *= rotateSpeed;
            dy *= rotateSpeed;
            if (Mathf.Abs(dx) > 0 || Mathf.Abs(dy) > 0)
            {
                editBase.RoleRotateDelta(-dx);
                //editBase.CameraVDelta(-dy);
            }
        }
        if (Input.GetAxis("Mouse ScrollWheel") != 0)
        {
            var dz = Input.GetAxis("Mouse ScrollWheel") * zoomSpeed;
            editBase.CameraBiasDelta(0,0, -dz);
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
                editBase.RoleRotateDelta(-touch.deltaPosition.x * Time.deltaTime * 10);
              
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
            editBase.CameraBiasDelta(0, 0, scaleFactor);
            //记住最新的触摸点，下次使用
            oldTouch1 = newTouch1;
            oldTouch2 = newTouch2;
        }
#endif 

    }
}