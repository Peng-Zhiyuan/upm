using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomCameraController : MonoBehaviour
{
    [SerializeField]
    public Transform m_Target = null;

    public float m_HAngle = 0f;
    public float m_VAngle = 0f;
    public float m_Distance = 5f;
    public float m_SideXOffset = 0f;
    public float m_SideYOffset = 3f;
    public float m_SideZOffset = 3f;
    public float m_FieldOfView = 45f;

    private float m_HAngleDamp = 0.15f;
    private float m_VAngleDamp = 0.15f;
    private float m_DistanceDamp = 0.15f;
    private float m_SideOffsetDamp = 0.15f;
    private float m_FieldOfViewDamp = 0.15f;

    public float m_LookOffsetY = 1.7f;

    private float m_CurrentHAngle = 0f;
    private float m_CurrentVAngle = 0f;
    private float m_CurrentDistance = 5f;
    private float m_CurrentSideXOffset = 0f;
    private float m_CurrentSideYOffset = 0f;
    private float m_CurrentSideZOffset = 0f;
    private float m_CurrentFieldOfView = 45f;

    private float m_DistanceSmooth = 0f;
    private float m_HAngleSmooth = 0f;
    private float m_VAngleSmooth = 0f;
    private float m_SideOffsetYSmooth = 0f;
    private float m_SideOffsetXSmooth = 0f;
    private float m_SideOffsetZSmooth = 0f;
    private float m_FieldOfViewSmooth = 0f;


    public bool LookHead = false;

    public bool OpenSmoothDamp = true;

    private void Start()
    {

        // 初始化当前相机数值
        Initialize();
       
    }

    public void CameraVDelta(float val)
    {
        this.m_VAngle += val;
    }

    public void CameraBiasDelta(float x, float y, float z)
    {
        this.m_SideXOffset += x;
        this.m_SideYOffset += y;
        this.m_SideZOffset += z;
    }

    public void CameraHDelta(float val)
    {
        this.m_HAngle += val;
    }

    private void Initialize()
    {
        m_CurrentHAngle = m_HAngle;
        m_CurrentVAngle = m_VAngle;
        m_CurrentDistance = m_Distance;
        m_CurrentSideXOffset = m_SideXOffset;
        m_CurrentSideYOffset = m_SideYOffset;
        m_CurrentSideZOffset = m_SideZOffset;
        m_CurrentFieldOfView = m_FieldOfView;
    }

    private void LateUpdate()
    {
        if (!m_Target)
        {
            if (!SceneObjectManager.IsAccessable)
                return;
            if (SceneObjectManager.Instance.LocalPlayerCamera == null) return;
            m_Target = SceneObjectManager.Instance.LocalPlayerCamera.transform;
        }
        
        if (!m_Target)
        {
            Debug.LogError("Main Camera : Target Not Found !");
            return;
        }


        if (OpenSmoothDamp)
        {

            m_CurrentDistance = Mathf.SmoothDamp(m_CurrentDistance, m_Distance, ref m_DistanceSmooth, m_DistanceDamp);
            m_CurrentHAngle = Mathf.SmoothDampAngle(m_CurrentHAngle, m_HAngle, ref m_HAngleSmooth, m_HAngleDamp);
            m_CurrentVAngle = Mathf.SmoothDampAngle(m_CurrentVAngle, m_VAngle, ref m_VAngleSmooth, m_VAngleDamp);
            m_CurrentSideXOffset = Mathf.SmoothDamp(m_CurrentSideXOffset, m_SideXOffset, ref m_SideOffsetXSmooth, m_SideOffsetDamp);
            m_CurrentSideYOffset = Mathf.SmoothDamp(m_CurrentSideYOffset, m_SideYOffset, ref m_SideOffsetYSmooth, m_SideOffsetDamp);
            m_CurrentSideZOffset = Mathf.SmoothDamp(m_CurrentSideZOffset, m_SideZOffset, ref m_SideOffsetZSmooth, m_SideOffsetDamp);
            m_CurrentFieldOfView = Mathf.SmoothDamp(m_CurrentFieldOfView, m_FieldOfView, ref m_FieldOfViewSmooth, m_FieldOfViewDamp); 
        }
        else
        {
            m_CurrentDistance = m_Distance;
            m_CurrentHAngle = m_HAngle;
            m_CurrentVAngle = m_VAngle;
            m_CurrentSideXOffset = m_SideXOffset;
            m_CurrentSideYOffset = m_SideYOffset;
            m_CurrentSideZOffset = m_SideZOffset;
            m_CurrentFieldOfView = m_FieldOfView;  
        }
    
        UpdateCameraLogic();
    }

    public void ForceSet()
    {
        if (!m_Target)
        {
            if (!SceneObjectManager.IsAccessable)
                return;
            if (SceneObjectManager.Instance.LocalPlayerCamera == null) return;
            m_Target = SceneObjectManager.Instance.LocalPlayerCamera.transform;
        }
        
        if (!m_Target)
        {
            Debug.LogError("Main Camera : Target Not Found !");
            return;
        }
        m_CurrentDistance = m_Distance;
        m_CurrentHAngle = m_HAngle;
        m_CurrentVAngle = m_VAngle;
        m_CurrentSideXOffset = m_SideXOffset;
        m_CurrentSideYOffset = m_SideYOffset;
        m_CurrentSideZOffset = m_SideZOffset;
        m_CurrentFieldOfView = m_FieldOfView;
        UpdateCameraLogic();
    }

    void UpdateCameraLogic()
    {
        Vector3 tmp_offset = Vector3.zero;
        tmp_offset.x = Mathf.Cos(m_CurrentVAngle * Mathf.Deg2Rad) * m_CurrentDistance * Mathf.Sin(m_CurrentHAngle * Mathf.Deg2Rad);
        tmp_offset.z = -Mathf.Cos(m_CurrentVAngle * Mathf.Deg2Rad) * m_CurrentDistance * Mathf.Cos(m_CurrentHAngle * Mathf.Deg2Rad);
        tmp_offset.y = Mathf.Sin(m_CurrentVAngle * Mathf.Deg2Rad) * m_CurrentDistance;

        Vector3 tmp_target = m_Target.position + Vector3.up * m_LookOffsetY;
        Vector3 tmp_pos = tmp_target + tmp_offset;

        transform.position = tmp_pos;
        transform.LookAt(tmp_target, Vector3.up);

        tmp_pos += transform.right * m_CurrentSideXOffset;
        tmp_pos += transform.up * m_CurrentSideYOffset;
        tmp_pos -= transform.forward * m_CurrentSideZOffset;
        // m_CurrentSideXOffset = 0;
        // m_CurrentSideYOffset = 0;
        //RaycastHit tmp_hit;
        //if (Physics.Raycast(tmp_target, tmp_offset, out tmp_hit, Vector3.Distance(tmp_target, tmp_pos), LayerMask.GetMask("Default")))
        //{
        //    tmp_pos = tmp_hit.point - tmp_offset.normalized * 0.1f;
        //    m_CurrentDistance = tmp_hit.distance;
        //}
        transform.position = tmp_pos;
        GetComponent<Camera>().fieldOfView = m_CurrentFieldOfView;
    }

    public void SetTarget(Transform target)
    {
        m_Target = target;
        /*if (LookHead)
        {
            Creature role = target as 
            Bip001 Head
        }
            m_Target = target.Find*/
    }

    public void Front()
    {
        
    }
}
