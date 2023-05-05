using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
[ExecuteAlways]
public class CameraSetting : MonoInstance<CameraSetting>
{
    // Start is called before the first frame update
    private CinemachineVirtualCamera _CVCamer;
    private CinemachineImpulseSource _Impulse ;
    private CinemachineTransposer _Transposer;
    private CinemachineOrbitalTransposer _OrbitalTransposer;
    private CinemachineBasicMultiChannelPerlin _NoisePerlin;

    private NoiseSettings _NoiseProfile;
    
    public float m_HAngle = 0f;
    public float m_HAngleDamp = 5f;
    public float m_HAngleSpeed = 2f;
    private float m_CurrentHAngle = 0f;
    private float m_HAngleSmooth = 0f;
    public bool IsUpdate = true;
    void Awake()
    {
        _ins = this;
        _CVCamer = this.gameObject.GetComponent<CinemachineVirtualCamera>();
        _Transposer = _CVCamer.GetCinemachineComponent<CinemachineTransposer>();
        _Impulse = _CVCamer.GetComponent<CinemachineImpulseSource>();
        _OrbitalTransposer = _CVCamer.GetCinemachineComponent<CinemachineOrbitalTransposer>();
        _NoisePerlin = _CVCamer.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        _NoiseProfile = _NoisePerlin.m_NoiseProfile;
        CloseNoise();
    }

    private float atmp = 0.5f;
    // Update is called once per frame
    void Update()
    {
        /*if (Input.GetKeyDown(KeyCode.O))
        {
            atmp += 0.5f;
            Shake(2, 10, atmp);
        }*/
        //_OrbitalTransposer.m_XAxis.Value = Mathf.SmoothDampAngle( _OrbitalTransposer.m_XAxis.Value, m_HAngle, ref m_HAngleSmooth, m_HAngleDamp);
    }

    public void SyncPosImmediate()
    {
        _CVCamer.PreviousStateIsValid = false;
    }
    
    public void SetBindingMode(CinemachineTransposer.BindingMode mode)
    {
        if(_OrbitalTransposer == null)
            return;
        
        if(_OrbitalTransposer.m_BindingMode == mode)
            return;
        
        _OrbitalTransposer.m_BindingMode = mode;
    }

    /// <summary>
    /// 设置FOV
    /// </summary>
    /// <param name="fov"></param>
    public void SetFOV(float fov)
    {
        _CVCamer.m_Lens.FieldOfView = fov;
    }

    public float GetFov()
    {
        return _CVCamer.m_Lens.FieldOfView;
    }

    public void OpenNoise()
    {
        //var com = _CVCamer.AddCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        //com.m_NoiseProfile.OrientationNoise.SetValue();
        _NoisePerlin.m_NoiseProfile = _NoiseProfile;
    }
    
    public void CloseNoise()
    {
        //_CVCamer.DestroyCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        _NoisePerlin.m_NoiseProfile = null;
    }
    
    /// <summary>
    /// 设置FLookAt
    /// </summary>
    /// <param name="LookAt"></param>
    public void LookAt(Transform target)
    {
        _CVCamer.LookAt = target;
    }

    /// <summary>
    /// 设置Follow
    /// </summary>
    /// <param name="follow"></param>
    public void SetFollow(Transform follow)
    {
        _CVCamer.Follow = follow;
    }

   /// <summary>
   /// 震屏
   /// </summary>
   /// <param name="amplitude">震动幅度</param>
   /// <param name="frequency">震动频率</param>
   /// <param name="durTime">持续时间</param>
    public void Shake(float amplitude = 2f, float frequency = 10f, float durTime = 1f)
   {
       //CinemachineBasicMultiChannelPerlin[] perlins = null;
       CinemachineBrain brain = CameraManager.Instance.MainCamera.GetComponent<CinemachineBrain>();
       if (brain != null && brain.ActiveVirtualCamera != null)
       {
           /*var cinebase = brain.ActiveVirtualCamera.VirtualCameraGameObject
               .GetComponentInChildren<CinemachineVirtualCameraBase>();
           perlins = cinebase.GetComponentsInChildren<CinemachineBasicMultiChannelPerlin>();*/
           var cv = brain.ActiveVirtualCamera as CinemachineVirtualCamera;
           _Impulse = cv.GetComponent<CinemachineImpulseSource>();
           if (_Impulse == null)
           {
               _Impulse = cv.gameObject.AddComponent<CinemachineImpulseSource>();
           }
           if(_Impulse == null)
               return;
           
           _Impulse.m_ImpulseDefinition.m_AmplitudeGain = amplitude;
           _Impulse.m_ImpulseDefinition.m_FrequencyGain = frequency;
           _Impulse.m_ImpulseDefinition.m_TimeEnvelope.m_SustainTime = durTime - 0.8f;
           _Impulse.GenerateImpulse(new Vector3(-1, -1, 0));
       }

       /*if (perlins != null && perlins.Length > 0)
       {
           foreach (var VARIABLE in perlins)
           {
               VARIABLE.m_AmplitudeGain = amplitude;
           }
       }*/
        
    }

    /// <summary>
    /// 设置相机位置
    /// </summary>
    /// <param name="pos"></param>
    public void SetPosition(Vector3 pos)
    {
        this.transform.position = pos;
    }

    /// <summary>
    /// 设置水平偏移值
    /// </summary>
    /// <param name="offset"></param>
    public void SetOffsetX(float offset)
    {
        //_Transposer.m_FollowOffset.x = offset;
        //var com = this.GetComponentInChildren<CinemachineComposer>().TrackedPoint;
        //com.x = offset;
        
        //this.GetComponentInChildren<CinemachineComposer>().TrackedPoint = com;
        //this._CVCamer.m_XAXis

        //m_HAngle = offset;
        //_OrbitalTransposer.m_XAxis.Value = offset;
        _OrbitalTransposer.m_Heading.m_Bias = offset;

    }

    public void SetDeadZone(float w, float h, float sw = 0.412f, float sh = 0.336f)
    {
        var com = _CVCamer.GetCinemachineComponent<CinemachineComposer>();
        if (com != null)
        {
            com.m_DeadZoneHeight = h;
            com.m_DeadZoneWidth = w;

            com.m_SoftZoneHeight = sh;
            com.m_SoftZoneWidth = sw;
        }
    }

    public void SetMaxMinRot(int max = 90, int min = -90)
    {
        _OrbitalTransposer.m_XAxis.m_MaxValue = max;
        _OrbitalTransposer.m_XAxis.m_MinValue = min;
    }
    
    /// <summary>
    /// 设置竖直方向偏移值
    /// </summary>
    /// <param name="offset"></param>
    public void SetOffsetY(float offset)
    {
        _Transposer.m_FollowOffset.y = offset;
    }

    public void SetDamping(float val)
    {
        _Transposer.m_XDamping = val;
        _Transposer.m_XDamping = val;
        _Transposer.m_XDamping = val;
    }
    
    /// <summary>
    /// 设置Z偏移值
    /// </summary>
    /// <param name="offset"></param>
    public void SetOffsetZ(float offset)
    {
        _Transposer.m_FollowOffset.z = offset;
    }

    public void SetDistance(float offset)
    {
        _OrbitalTransposer.m_FollowOffset.z = offset;
    }

    public float GetDistance()
    {
        return _OrbitalTransposer.m_FollowOffset.z;
    }
    
    /// <summary>
    /// 设置偏移值
    /// </summary>
    /// <param name="offset"></param>
    public void SetOffsetX(Vector3 offset)
    {
        _Transposer.m_FollowOffset = offset;
    }

    // 关闭层layer
    public void IgnoreLayer(Camera camera, string layer)
    {
        var index = LayerMask.NameToLayer(layer);
        camera.cullingMask &= ~(1 << index);
    }
    
    // 打开层layer
    public void OpenLayer(Camera camera, string layer)
    {
        var index = LayerMask.NameToLayer(layer);
        camera.cullingMask |= (1 << index);
    }
    
    /*camera.cullingMask = ~(1 << x);  // 渲染除去层x的所有层 
   
    camera.cullingMask &= ~(1 << x); // 关闭层x 
   
    camera.cullingMask |= (1 << x);  // 打开层x 
 
    camera.cullingMask = 1 << x + 1 << y + 1 << z; // 摄像机只显示第x层,y层,z层.
    */
    
    
}
