using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
public class RolePreviewCtrl : MonoBehaviour
{
    // 模型
    private Transform m_Target;
    // 旋转速度
    public static float rotateSpeed = 10f; 
    // 移动速度
    public static float moveSpeed = 0.15f;
    // 镜头拉伸速度
    public static float zoomSpeed = 4f;   //速度比例因子



    public CustomCameraController cameraCtrl;   
    private Touch oldTouch1;  //上次触摸点1(手指1) 
    private Touch oldTouch2;  //上次触摸点2(手指2)
    public float farOffset = 0;
    public float nearOffset = 0;
    public SkyBoxSetting skyBoxSetting;
    private RoleViewConfData viewConfData;
    public bool lockCameraDistance = false;
    public static bool lockCamera = false;
   // public FancyScrollView.Scroller scroller;
    public RectTransform TouchValidMask;
    public RectTransform TouchInValidMask;
    public void SetConfData(RoleViewConfData confData,  CameraCtrlMode cameraMode, bool forceLookCamera = true)
    {

        viewConfData = confData;
        lockCamera = false;
        if (viewConfData != null)
        {
            switch (cameraMode)
            {
                default:
                case CameraCtrlMode.LOCK_FAR:
                    cameraCtrl.m_LookOffsetY = viewConfData.lookMinOffset;
                    cameraCtrl.m_SideZOffset = viewConfData.maxOffset;
                    lockCameraDistance = true;
                    break;
                case CameraCtrlMode.LOCK_NEAR:
                    cameraCtrl.m_LookOffsetY = viewConfData.lookMaxOffset;
                    cameraCtrl.m_SideZOffset = viewConfData.minOffset;
                    lockCameraDistance = true;
                    break;
                case CameraCtrlMode.UNLOCK_FAR:
                    cameraCtrl.m_LookOffsetY = viewConfData.lookMinOffset;
                    cameraCtrl.m_SideZOffset = viewConfData.maxOffset;
                    lockCameraDistance = false;
                    break;
                case CameraCtrlMode.UNLOCK_NEAR:
                    cameraCtrl.m_LookOffsetY = viewConfData.lookMaxOffset;
                    cameraCtrl.m_SideZOffset = viewConfData.minOffset;
                    lockCameraDistance = false;
                    break;
            }
            
            if(Target != null && forceLookCamera) Target.rotation = Quaternion.identity;
        }
    }

    public Transform Target
    {
        get 
        {
            return cameraCtrl.m_Target;
        }
    }
    private void RoleRotateDelta(float delta)
    { 
         Target.rotation = Target.rotation * Quaternion.Euler(0, delta, 0);
    }

    void Update()
    {
        m_Target = Target;
        if (m_Target == null) return;
     
       // if (scroller && scroller.dragging) return;

        /*     GameObject go = EventSystem.current.currentSelectedGameObject;
             if (go != null && go.CompareTag("IGNORE_TOUCH"))
             {
                 Debug.Log("忽略滑动：" + go.name);
                 return;

             }*/


        // pzy:
        // 以下代码有编译问题
        // 暂时移除
        // if (Input.touchCount <= 0)
        //  {
        //Debug.LogError("没有手指触屏");
        //      return;
        // }


        
        if (Input.touchCount > 1 && !lockCameraDistance)//多点触碰
            {
                cameraCtrl.OpenSmoothDamp = false;
                //if(cameraCtrl.m_SideZOffset > 1 || cameraCtrl.m_SideZOffset < 0.3) return;
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
                //cameraCtrl.CameraBiasDelta(0, 0, scaleFactor);
                float zOffset = cameraCtrl.m_SideZOffset + scaleFactor;
                if (viewConfData != null)
                {
                    cameraCtrl.m_SideZOffset = Mathf.Clamp(zOffset, viewConfData.minOffset, viewConfData.maxOffset);
                    float r = Mathf.Clamp(Mathf.Abs(cameraCtrl.m_SideZOffset - viewConfData.maxOffset) / Mathf.Abs(viewConfData.minOffset - viewConfData.maxOffset), 0, 1);
                    cameraCtrl.m_LookOffsetY = viewConfData.lookMinOffset + r * (viewConfData.lookMaxOffset - viewConfData.lookMinOffset);
                    if (cameraCtrl.m_SideXOffset > 0)
                    {

                        cameraCtrl.m_SideXOffset = Mathf.Lerp(0.15f, 0.1f, r);
                    }
                }
                //记住最新的触摸点，下次使用
                oldTouch1 = newTouch1;
                oldTouch2 = newTouch2;
            }
            else if (InputProxy.TouchMove()) //单点触碰移动摄像机
            {
                if (TouchValidMask!= null)
                {
                    Vector3 position = InputProxy.TouchPosition();
                    bool touchValid = RectTransformUtility.RectangleContainsScreenPoint(TouchValidMask, position);
                    if (!touchValid) return;
                }
                if (TouchInValidMask != null)
                {
                    Vector3 position = InputProxy.TouchPosition();
                    bool touchInvalid = RectTransformUtility.RectangleContainsScreenPoint(TouchInValidMask, position);
                    if (touchInvalid) return;
                }

            cameraCtrl.OpenSmoothDamp = false;
                float touchDelta = InputProxy.TouchDelta().x;
#if UNITY_EDITOR
                touchDelta *= 25f;
#endif
                this.RoleRotateDelta(-touchDelta);
                if (skyBoxSetting != null)
                {

                    skyBoxSetting.RotateSky(-touchDelta * Time.deltaTime);
                }
            }
#if UNITY_EDITOR
            if (Input.GetAxis("Mouse ScrollWheel") != 0 && !lockCameraDistance)
            {
                cameraCtrl.OpenSmoothDamp = false;
                var dz = Input.GetAxis("Mouse ScrollWheel") * zoomSpeed;
                float zOffset = cameraCtrl.m_SideZOffset - dz;
                if (viewConfData != null)
                {
                    cameraCtrl.m_SideZOffset = Mathf.Clamp(zOffset, viewConfData.minOffset, viewConfData.maxOffset);
                   
                    //Debug.LogError("zOffset："+ zOffset + " viewConfData.zMinOffset：" + viewConfData.maxOffset+ "  viewConfData.zMaxOffset:"+ viewConfData.minOffset);
                    float r = Mathf.Clamp(Mathf.Abs(cameraCtrl.m_SideZOffset - viewConfData.maxOffset) / Mathf.Abs(viewConfData.minOffset - viewConfData.maxOffset), 0, 1);
                    //Debug.LogError("r："+r);
                    cameraCtrl.m_LookOffsetY = viewConfData.lookMinOffset + r * (viewConfData.lookMaxOffset - viewConfData.lookMinOffset);
                    if (cameraCtrl.m_SideXOffset > 0)
                    {
                    
                        cameraCtrl.m_SideXOffset = Mathf.Lerp(0.15f, 0.1f, r);
                    }
                }
            }
#endif


        

    
    }
}