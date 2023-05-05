using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Security;
using UnityEngine;
using UnityEngine.Rendering.LWRP;
using BattleSystem.ProjectCore;

// Physics Debug
// using RotaryHeart.Lib.PhysicsExtension;
// using Physics = RotaryHeart.Lib.PhysicsExtension.Physics;

[System.Serializable]
public class CameraProxy : IDisposable
{
    private CameraManager _owner = null;
    public Camera MainCamera
    {
        get
        {
            if (_owner == null)
                return null;
            return _owner.MainCamera;
        }
    }
    public Transform CameraTransform
    {
        get
        {
            if (_owner == null)
                return null;
            return _owner.CameraTransform;
        }
    }
    public Transform Target
    {
        get
        {
            if (_owner == null)
                return null;
            return _owner.Target;
        }
    }

    public void SetTarget(Transform tmp, Creature role, bool immediate = false)
    {
        if (_owner == null)
            return;
        _owner.SetTarget(tmp, role, immediate);
    }

    public void RecoveHAngle()
    {
        HOffset = 0f;
    }

    public float m_PlayerDistance { get; set; }

    public void Init(CameraManager owner)
    {
        _owner = owner;
    }

    private Vector3 _targetOffset = Vector3.zero;
    private float m_CurrentOffset = 0f;
    private float _CurrentOffsetValue = 0f;
    private float _CurrentOffsetSmooth = 0f;

    public void SetTempTargetPos(Vector3 pos)
    {
        if (pos == Vector3.zero)
        {
            _CurrentOffsetValue = 0f;
        }
        else
        {
            m_CurrentOffset = 0f;
            _CurrentOffsetValue = 1f;
            _targetOffset = pos - Target.position;
        }
    }

    public float m_CameraRadius = 0.2f;
    private bool m_IsEnterFalse = false;

    private float m_HangleValue = 26f;
    public float m_HAngle
    {
        set
        {
            m_HangleValue = value; 
            //CameraSetting.Ins.SetOffsetX(value);
        }
        get { return m_HangleValue; }
    }

    private float m_VAngleValue = 26f;
    public float m_VAngle
    {
        set { m_VAngleValue = value; }
        get { return m_VAngleValue; }
    }

    private float m_DistanceValue = 10f;
    public float Distance
    {
        set
        {
            m_DistanceValue = value; 
            //CameraSetting.Ins.SetDistance(-value);
        }
        get { return m_DistanceValue; }
    }

    /*private float m_FOVValue = 20f;
    public float m_FOV
    {
        set
        {
            m_FOVValue = value;
        }
        get
        {
            return m_FOVValue;
        }
    }*/

    public bool VFollowTarget { get; set; } = true;

    private float m_SideOffsetY = 0f;
    public float SideOffsetY
    {
        get { return m_SideOffsetY; }
        set
        {
            m_SideOffsetY = value;
            CameraSetting.Ins.SetOffsetY(value);
        }
    }
    
    public float m_SideOffsetZ = 0f;

    public float m_LookOffsetYDamp = 0.15f;
    public float m_HAngleSpeed = 2f;
    public float m_VAngleSpeed = 2f;
    public float m_DistanceSpeed = 0.5f;
    public float m_DistanceOffset = 0f;

    public float m_SideOffset = 0f;
    private float m_FieldOfView = 65f;

    public float FieldOfView
    {
        get { return m_FieldOfView; }
        set
        {
            m_FieldOfView = value;
            CameraSetting.Ins.SetFOV(value);
        }
    }
    
   
    public float m_HAngleDamp = 10.0f;
    public float m_VAngleDamp = 0.3f;
    public float m_DistanceDamp = 0.3f;
    public float m_SideOffsetDamp = 0.3f;
    public float m_SideOffsetYDamp = 0.3f;
    public float m_SideOffsetZDamp = 0.3f;
    public float m_FieldOfViewDamp = 0.3f;

    public float m_LookOffsetY = 0.85f;

    //镜头平滑时间调整参数
    public float m_SmoothScale = 1f;
    public bool TreeDissolveValue = true;
    public bool CheckRaycast = true;

    public float m_CurrentHAngle = 0f;
    private float m_CurrentVAngle = 0f;
    private float m_CurrentDistance = 10f;
    public float m_CurrentSideOffset = 0f;
    public float m_CurrentSideOffsetY = 0f;
    public float m_CurrentSideOffsetZ = 0f;
    private float m_CurrentFieldOfView = 45f;
    private float m_CurrentLookOffsetY = 1.7f;
    private float m_CurrentDistanceOffset = 0f;

    private float m_DistanceOffsetSmooth = 0f;
    private float m_DistanceSmooth = 0f;
    private float m_HAngleSmooth = 0f;
    private float m_HAngleOffsetSmooth = 0f;
    private float m_VAngleSmooth = 0f;
    private float m_SideOffsetSmooth = 0f;
    private float m_SideOffsetYSmooth = 0f;
    private float m_FieldOfViewSmooth = 0f;
    private float m_LookOffsetYSmooth = 0f;
    private float m_SideOffsetZSmooth = 0f;

    private bool _delaySync = false;
    private Vector3 _delayDir;
    private float _delayStartTime;
    private float _delayDistance = 0f;
    private float _delaySpeed = 0f;

    public float DistanceOffset
    {
        get { return m_DistanceOffset; }
        set { m_DistanceOffset = Mathf.Clamp(value, -2f, 2f); }
    }

    private float m_HAngleOffset = 0;
    private float m_CurrentHAngleOffset = 0f;
    public float HAngleOffset
    {
        get { return m_HAngleOffset; }
        set { m_HAngleOffset = value; }
    }
    public float LocalHOffset { get; set; }

    public float HAngleSign { get; set; } = 1;

    public float HOffset { get; set; }

    public float TransitionTime { get; set; }

    public void ResetTransitionTime()
    {
        TransitionTime = 0.3f;
    }

    public void SetCurrentDistance(float dis)
    {
        //Debug.LogError("SetCurrentDistance:"+ dis);
        m_CurrentDistance = dis;
    }

    public void ResetHAngle()
    {
        m_HAngle = 0;
        m_CurrentHAngle = 0;
        VFollowTarget = true;
    }

    public float DelayTime { get; set; }

    public bool AutoMove { get; set; }

    public bool SetSpeed { get; set; }

    public float OffsetSpeed { get; set; } = 0;

    public Vector3 ReginPos { get; set; }

    public float ZOffset { get; set; } = 0.5f;

    public float HOffsetAngle { get; set; } = 0f;

    public bool UseBone { get; set; }

    public bool ChangeImmedate
    {
        get;
        set;
    } = true;

    /// <summary>
    /// Determine the signed angle between two vectors, with normal 'n'
    /// as the rotation axis.
    /// </summary>
    public float AngleSigned(Vector3 v1, Vector3 v2, Vector3 n)
    {
        return Mathf.Atan2(Vector3.Dot(n, Vector3.Cross(v1, v2)), Vector3.Dot(v1, v2)) * Mathf.Rad2Deg;
    }

    public void LateUpdate(float param_deltaTime)
    {
        if (CameraManager.Instance.Move)
        {
            CalculateCurrentPara();
            SyncCamera();
            return;
        }
        return;
        if (_owner.GetCurrentStateID() == (int)CameraState.Fixed)
            return;
        if (DelayTime > 0)
        {
            DelayTime -= param_deltaTime;
            if (DelayTime <= 0)
            {
                TurnImmediate();
            }
            else
            {
                return;
            }
        }

        //if (VFollowTarget)
        {
            if (_owner.TargetRole == null
                || _owner.TargetRole.Target == null)
            {
                if (VFollowTarget)
                    m_HAngle = -Target.transform.localEulerAngles.y - HOffset;
            }
            if (_owner.TargetRole != null
                && _owner.TargetRole.Target != null)
            {
                Creature role = _owner.TargetRole.Target as Creature;
                if (role.mData.IsDead)
                {
                    if (VFollowTarget)
                        m_HAngle = -Target.transform.localEulerAngles.y - HOffset;
                }
            }
            /*if (_owner.TargetRole != null && !_owner.TargetRole.ClientDead)
            {
                if (_owner.TargetRole.Target != null)
                {
                    Creature _targetRole = _owner.TargetRole.Target as Creature;
                    if (!_targetRole.ClientDead)
                    {
                        float angle = 0f;
                        /*Transform _targetBone = _targetRole.GetBone("Bip001 Spine");
                        Transform _fromBone = _owner.TargetRole.GetBone("Bip001 Spine");#1#
                        Transform _targetBone = _targetRole.GetBone("root");
                        Transform _fromBone = _owner.TargetRole.GetBone("root");
                        if (_targetBone != null && _fromBone != null)
                        {
                            Vector3 dir = _targetBone.position - _fromBone.position;
                            //Vector3 targetPos = _owner.Target.position + dir.normalized * ZOffset;
                            //float angle = Vector3.Angle(dir.normalized, Vector3.right);
                            angle = AngleSigned(dir.normalized, Vector3.right, Vector3.up) - 90;
                        }
                        
                        m_HAngle = angle + HOffsetAngle * HAngleSign;
                    }
                    
                    //Debug.LogError("hangle = " + m_HAngle);
                }
            }*/
            Transform temp = _owner.GetTarget();
            if (_owner.TargetRole != null
                && !_owner.TargetRole.mData.IsDead
                && temp != null)
            {
                // if (_owner.TargetRole.Target != null)
                {
                    //Creature _targetRole = _owner.TargetRole.Target as Creature;
                    //if (!_targetRole.ClientDead)
                    {
                        float angle = 0f;
                        /*Transform _targetBone = _targetRole.GetBone("Bip001 Spine");
                        Transform _fromBone = _owner.TargetRole.GetBone("Bip001 Spine");*/
                        Transform _targetBone = temp;
                        Transform _fromBone = _owner.TargetRole.GetBone("root");
                        if (_targetBone != null
                            && _fromBone != null)
                        {
                            Vector3 dir = _targetBone.position - _fromBone.position;
                            //Vector3 targetPos = _owner.Target.position + dir.normalized * ZOffset;
                            //float angle = Vector3.Angle(dir.normalized, Vector3.right);
                            angle = AngleSigned(dir.normalized, Vector3.right, Vector3.up) - 90;
                        }
                        m_HAngle = angle + HOffsetAngle * HAngleSign;
                    }

                    //Debug.LogError("hangle = " + m_HAngle);
                }
            }
        }

        //m_HAngle = -HOffset;
        //m_HAngle += m_CurrentHAngleOffset;
        //SetLookOffsetY();
        var mapGenerate = MapGenerateCore.Instance;
        var battle = Battle.Instance;
        var mode = battle.param.mode;
        // 肉鸽地图的时候相机正后方跑图
        if (mode == BattleSystem.ProjectCore.BattleModeType.Roguelike)
        {
            if (mapGenerate.JudgeEmptyTarget())
            {
                m_HAngle = -Target.transform.localEulerAngles.y;
            }
        }
        if (AutoMove && CameraManager.Instance.GetCurrentStateID() == (int)CameraState.Free2)
        {
            /*if (SetSpeed == false)
            {
                float y = Vector3.Cross(ReginPos - _owner.Target.position, CameraManager.Instance.MainCamera.transform.position - _owner.Target.position).y;
                if (y < 0)
                {
                    OffsetSpeed = OffsetSpeed;
                }
                else
                {
                    OffsetSpeed = -OffsetSpeed;
                }

                SetSpeed = true;
            }*/
            float y = Vector3.Cross(ReginPos - _owner.Target.position, CameraManager.Instance.MainCamera.transform.position - _owner.Target.position).y;
            if (y < 0)
            {
                //m_HAngleOffset += param_deltaTime * OffsetSpeed;
            }
            else
            {
                //m_HAngleOffset -= param_deltaTime * OffsetSpeed;
            }
        }
        ReginPos = CameraManager.Instance.MainCamera.transform.position;
        RaycastHitCheck();
        CalculateCurrentPara();
        GetCameraPos();
        if (_delaySync)
        {
            DelaySync(param_deltaTime);
            return;
        }
        SyncCamera();
        LookTarget();
        //TreeDissolveCheck(TreeDissolveValue);
        //RaycastHitCheck();
        CalculateOffset();
        //SyncCamera();
    }

    //延迟同步
    private void DelaySync(float param_deltaTime)
    {
        _delayStartTime -= param_deltaTime;
        if (_delayStartTime < 0f)
        {
            tmp_pos = tmp_newPos + _delayDir * _delayDistance;
            _delayDistance -= param_deltaTime * _delaySpeed;
            if (_delayDistance < 0f)
            {
                _delaySync = false;
            }
            SyncCamera();
        }
    }

    //同步配置参数
    private void CalculateCurrentPara()
    {
        if (_delaySync)
            return;
        /*if (TransitionTime > 0)
        {
            TransitionTime -= Time.deltaTime;
            m_SmoothScale = 1 - _owner.TransitionCurve.Evaluate(TransitionTime / 0.3f) / 3 + 0.5f;
        }
        else
        {
            m_SmoothScale = 1f;
        }*/
        //Debug.LogError("m_CurrentDistance:" + m_CurrentDistance+ "  m_Distance:"+ m_Distance);
        //    ChangeSmoothTime();
        //    m_VAngle = Mathf.Clamp(m_VAngle, -30f,60f);
        //m_CurrentDistanceOffset = Mathf.SmoothDamp(m_CurrentDistanceOffset, m_DistanceOffset, ref m_DistanceOffsetSmooth, m_DistanceDamp * m_SmoothScale);
        var tmp_max_distance = Mathf.Min(maxDistance, m_DistanceValue + m_DistanceOffset);
        m_CurrentDistance = Mathf.SmoothDamp(m_CurrentDistance, tmp_max_distance, ref m_DistanceSmooth, m_DistanceDamp * m_SmoothScale);
        m_CurrentHAngle = Mathf.SmoothDampAngle(m_CurrentHAngle, m_HAngle + m_HAngleOffset + LocalHOffset, ref m_HAngleSmooth, m_HAngleDamp * m_SmoothScale);
        m_CurrentVAngle = Mathf.SmoothDampAngle(m_CurrentVAngle, m_VAngle, ref m_VAngleSmooth, m_VAngleDamp * m_SmoothScale);
        m_CurrentSideOffset = Mathf.SmoothDamp(m_CurrentSideOffset, m_SideOffset, ref m_SideOffsetSmooth, m_SideOffsetDamp * m_SmoothScale);
        m_CurrentSideOffsetY = Mathf.SmoothDamp(m_CurrentSideOffsetY, m_SideOffsetY, ref m_SideOffsetYSmooth, m_SideOffsetYDamp * m_SmoothScale);
        m_CurrentFieldOfView = Mathf.SmoothDamp(m_CurrentFieldOfView, m_FieldOfView, ref m_FieldOfViewSmooth, m_FieldOfViewDamp * m_SmoothScale);
        m_CurrentLookOffsetY = Mathf.SmoothDamp(m_CurrentLookOffsetY, m_LookOffsetY, ref m_LookOffsetYSmooth, m_LookOffsetYDamp * m_SmoothScale);
        m_CurrentOffset = Mathf.SmoothDamp(m_CurrentOffset, _CurrentOffsetValue, ref _CurrentOffsetSmooth, m_DistanceDamp * m_SmoothScale);
        m_CurrentSideOffsetZ = Mathf.SmoothDamp(m_CurrentSideOffsetZ, m_SideOffsetZ, ref m_SideOffsetZSmooth, m_SideOffsetZDamp * m_SmoothScale);
        //m_CurrentHAngleOffset = Mathf.SmoothDampAngle(m_CurrentHAngleOffset, m_HAngleOffset, ref m_HAngleOffsetSmooth, m_HAngleDamp * m_SmoothScale);
    }

    private void ChangeSmoothTime()
    {
        m_SmoothScale = 1f;
    }

    private Vector3 tmp_offset;
    private Vector3 tmp_target;
    private Vector3 tmp_newPos;
    private Vector3 tmp_pos;

    //计算临时参数
    private void CalculateTempPara()
    {
        GetCameraPos();
        tmp_pos = tmp_newPos;
    }

    //同步偏移量
    private void CalculateOffset()
    {
        //SideOffset
        tmp_pos += CameraTransform.right * m_CurrentSideOffset;
        tmp_pos += CameraTransform.up * m_CurrentSideOffsetY;
        tmp_pos += CameraTransform.forward * m_CurrentSideOffsetZ;
    }

    public void CleanOffset()
    {
        m_SideOffset = 0f;
        m_SideOffsetY = 0f;
        m_CurrentSideOffset = 0f;
        m_CurrentSideOffsetY = 0f;
    }

    private void SyncCamera()
    {
        //CameraTransform.position = tmp_pos;
        //CameraManager.Instance.CVCamera.transform.position = tmp_pos;
        //CameraTransform.position = Vector3.MoveTowards(CameraTransform.position, tmp_pos, 8f * Time.deltaTime);

        //MainCamera.fieldOfView = m_CurrentFieldOfView;
        // CameraManager.Instance.CVCamera.m_Lens.FieldOfView = m_CurrentFieldOfView;
        /*CameraSetting.Ins.SetFOV(m_CurrentFieldOfView);
        CameraSetting.Ins.SetDistance(-m_CurrentDistance);
        CameraSetting.Ins.SetOffsetX(m_CurrentHAngle);
        CameraSetting.Ins.SetOffsetY(m_CurrentSideOffsetY);*/
        
        //CameraSetting.Ins.SetFOV(m_FieldOfView);
        CameraSetting.Ins.SetDistance(-m_CurrentDistance);
        CameraSetting.Ins.SetOffsetX(m_HAngle);
        CameraSetting.Ins.SetOffsetY(m_CurrentSideOffsetY);
    }

    private void LookTarget()
    {
        //CameraTransform.LookAt(tmp_target, Vector3.up);
        CameraManager.Instance.CVCamera.transform.LookAt(tmp_target, Vector3.up);
    }

    public void LookTarget(Vector3 target)
    {
        CameraTransform.LookAt(target, Vector3.up);
    }

    //树木遮挡检测
    private void TreeDissolveCheck(bool value)
    {
        if (value)
        {
            Shader.SetGlobalFloat("_DissolveDistance", m_CurrentDistance);
        }
        else
        {
            Shader.SetGlobalFloat("_DissolveDistance", -1);
        }
    }

    //射线检测
    private void RaycastHitCheck()
    {
        if (!CheckRaycast)
            return;
        if (_delaySync)
            return;
        /*
        int ignoreLayer = Physics.AllLayers ^ LayerMask.GetMask("Obstacle");
        //int ignoreLayer = Physics.AllLayers ^ LayerMask.GetMask("Obstacle");
        ignoreLayer = ignoreLayer ^ LayerMask.GetMask("Role");
        ignoreLayer = ignoreLayer ^ LayerMask.GetMask("CameraCheck");
        ignoreLayer = ignoreLayer ^ LayerMask.GetMask("Default");
        //ignoreLayer = ignoreLayer ^ LayerMask.GetMask("Destruction");
        //ignoreLayer = ignoreLayer ^ LayerMask.GetMask("CullObject");
        //ignoreLayer = ignoreLayer ^ LayerMask.GetMask("zhandou");
        // 调试使用：红色射线，仅Scene场景可见     
        */
        RaycastHit tmp_hit;
        if (Physics.SphereCast(tmp_target, m_CameraRadius, tmp_offset, out tmp_hit, Vector3.Distance(tmp_target, tmp_pos), LayerMask.GetMask("CameraOcc")))
        {
            tmp_pos = tmp_hit.point + tmp_hit.normal * m_CameraRadius;
#if UNITY_EDITOR
            Debug.DrawLine(tmp_target, tmp_pos, Color.red);
#endif
            //tmp_pos = tmp_hit.point;
            Vector3 pos = Target.position; // + _targetOffset * m_CurrentOffset;
            //CurrentDistance = Vector3.Distance(pos, tmp_pos);
            maxDistance = Vector3.Distance(pos, tmp_pos);
            //Debug.LogError("maxDistance:" + maxDistance);
            //m_Distance = Mathf.Clamp(Vector3.Distance(cameraPos, tmp_target), 1f, maxDistance);
            //HideMyself(m_CurrentDistance < GameSettingsManager.Instance.CameraDistanceMin);
        }
        else
        {
            maxDistance = 5;
            if (m_IsEnterFalse)
            {
                HideMyself(false);
            }
        }
    }

    private void HideMyself(bool param_Hide)
    {
        if (m_IsEnterFalse != param_Hide)
        {
            m_IsEnterFalse = param_Hide;
        }
    }

    private void GetCameraOffset()
    {
        //float hAngle = m_CurrentHAngle;
        //if(VFollowTarget)
        //m_HAngle = -Target.transform.localEulerAngles.y - HOffset;
        float tempDistance = m_CurrentDistance;
        tmp_offset = Vector3.zero;
        tmp_offset.x = Mathf.Cos(m_CurrentVAngle * Mathf.Deg2Rad) * tempDistance * Mathf.Sin(m_CurrentHAngle * Mathf.Deg2Rad);
        tmp_offset.z = -Mathf.Cos(m_CurrentVAngle * Mathf.Deg2Rad) * tempDistance * Mathf.Cos(m_CurrentHAngle * Mathf.Deg2Rad);
        tmp_offset.y = Mathf.Sin(m_CurrentVAngle * Mathf.Deg2Rad) * tempDistance;

        //Debug.LogError("offset = " + tmp_offset);
    }

    private void GetTargetPos()
    {
        if (Target == null)
        {
            tmp_target = Vector3.zero;
            return;
        }
        Vector3 targetPos = Target.position;
        if (_targetOffset != Vector3.zero)
        {
            targetPos += _targetOffset * m_CurrentOffset;
        }
        if (_regionPos != Vector3.zero
            && CameraManager.Instance.IsFree()
            && _transitionDis > 0f)
        {
            float percent = 1 - Vector3.Distance(_regionPos, Target.position) / _transitionDis;
            //_regionPos = Vector3.Lerp (_regionPos, Target.position, 10 * Time.deltaTime * _owner.TransitionCurve.Evaluate(percent) );
            //_regionPos = Vector3.Lerp (_regionPos, Target.position, 10 * Time.deltaTime );
            _regionPos = Vector3.MoveTowards(_regionPos, Target.position, 10 * Time.deltaTime * _owner.TransitionCurve.Evaluate(percent));

            //m_CurrentDistance = Mathf.SmoothVector3(m_CurrentDistance, m_Distance, ref m_DistanceSmooth, m_DistanceDamp * m_SmoothScale);
            //RegionPos = Vector3.MoveTowards(RegionPos, Target.position, 20 * Time.deltaTime);
            if (Vector3.Distance(_regionPos, Target.position) < 0.01f)
            {
                _regionPos = Vector3.zero;
            }
            else
            {
                targetPos = _regionPos;
            }
        }
        if (_owner.TargetRole != null
            && !_owner.TargetRole.mData.IsDead)
        {
            Transform temp = _owner.GetTarget();
            if (temp != null)
            {
                //Vector3 dir = _owner.TargetRole.Target.GetPosition() - _owner.TargetRole.GetPosition();
                //targetPos = targetPos + dir.normalized * 0.5f;

                //Creature _targetRole = _owner.TargetRole.Target as Creature;
                //if (!_targetRole.ClientDead && !_owner.TargetRole.ClientDead)
                {
                    Transform _targetBone = temp;
                    Transform _fromBone = _owner.TargetRole.GetBone("body_hit");
                    /*Transform _targetBone = _targetRole.GetBone("root");
                    Transform _fromBone = _owner.TargetRole.GetBone("root");*/
                    if (_fromBone != null)
                    {
                        Vector3 offset = targetPos - _owner.TargetRole.GetPosition();
                        Vector3 dir = _targetBone.position - _fromBone.position;
                        if (UseBone)
                        {
                            targetPos = _fromBone.position + dir.normalized * ZOffset + offset;
                            //return;
                        }
                        else
                        {
                            targetPos = _fromBone.position;
                        }
                    }
                }
                /*else
                {
                    targetPos = targetPos + Vector3.up * 0.8f;
                }*/
            }
            else
            {
                targetPos = targetPos + Vector3.up * 0.8f;
            }
        }
        else
        {
            targetPos = targetPos + Vector3.up * 0.8f;
        }
        tmp_target = targetPos;
        //tmp_target = targetPos + Vector3.up * m_CurrentLookOffsetY;
        //tmp_target = targetPos + Vector3.up * 0.8f;
    }

    private float _transitionTime = 0;
    private float _transitionTotalTime = 0.5f;
    private Vector3 _regionPos = Vector3.zero;
    private float _transitionDis = 10f;

    public Vector3 RegionPos
    {
        get { return _regionPos; }
        set
        {
            _regionPos = value;
            //_transitionTime = 0f;
        }
    }

    public void CaculateTransitionDis()
    {
        if (Target != null)
            _transitionDis = Vector3.Distance(_regionPos, Target.position);
    }

    private void GetCameraPos()
    {
        GetCameraOffset();
        GetTargetPos();
        tmp_newPos = tmp_target + tmp_offset;
        tmp_pos = tmp_newPos;
    }

    public void Dispose()
    {
        _owner = null;
    }

    public void Delay(float delayTime)
    {
        if (Target != null)
        {
            DelaySync(Target.position, 10f, delayTime);
        }
    }

    //延迟同步镜头
    public void DelaySync(Vector3 destPos, float speed, float delayTime, float distance = 0f)
    {
        GetCameraOffset();
        tmp_target = destPos + Vector3.up * m_CurrentLookOffsetY;
        tmp_newPos = tmp_target + tmp_offset;
        tmp_pos = tmp_newPos;
        _delayDistance = Vector3.Distance(CameraTransform.position, tmp_pos);
        _delayDir = (CameraTransform.position - tmp_pos).normalized;
        _delaySpeed = Mathf.Max(0f, speed);
        _delayStartTime = delayTime;
        if (distance > 0f)
        {
            _delayDistance = Mathf.Max(0f, distance);
        }
        _delaySync = true;
    }

    public void TurnImmediate()
    {
        m_CurrentDistance = Distance;
        //Debug.LogError("m_CurrentDistance:" + m_CurrentDistance);
        m_CurrentHAngle = m_HAngle;
        m_CurrentVAngle = m_VAngle;
        m_CurrentOffset = m_SideOffset;
        m_CurrentFieldOfView = m_FieldOfView;
        m_CurrentSideOffsetY = m_SideOffsetY;
        _regionPos = Vector3.zero;
        SyncCamera();
        CameraSetting._ins.SyncPosImmediate();
    }

    public float CurrentDistance
    {
        get { return m_CurrentDistance; }
        set { m_CurrentDistance = value; }
    }

    public void ResetCurrentDistance()
    {
        Distance = 10;
    }

    private float maxDistance = 20;

    public void SyncPosition(Vector3 cameraPos, float syncTime)
    {
        if (cameraPos != Vector3.zero)
        {
            GetTargetPos();
            Distance = Mathf.Clamp(Vector3.Distance(cameraPos, tmp_target), 1f, maxDistance);
            tmp_offset = cameraPos - tmp_target;
            Vector3 h_dir = tmp_offset;
            h_dir.y = 0f;
            Vector3 back = Vector3.forward * -1f;
            back.y = 0f;
            float tmp_H_Angle = Vector3.Angle(back, h_dir);
            if (Vector3.Cross(back, h_dir).y < 0)
            {
                m_HAngle = tmp_H_Angle;
            }
            else
            {
                m_HAngle = 360f - tmp_H_Angle;
            }
            float tmp_V_Angle = Vector3.Angle(h_dir, tmp_offset);
            if (tmp_offset.y < 0f)
                tmp_V_Angle = tmp_V_Angle * -1f;
            m_VAngle = Mathf.Clamp(tmp_V_Angle, -70, 70);
            //Debug.LogError("m_CurrentDistance:"+ m_CurrentDistance);
            //Debug.LogError("m_Distance:" + m_Distance);
            m_CurrentDistance = Mathf.SmoothDamp(m_CurrentDistance, m_DistanceValue, ref m_DistanceSmooth, Mathf.Min(syncTime, 0f, m_DistanceDamp * m_SmoothScale));
            m_CurrentHAngle = Mathf.SmoothDampAngle(m_CurrentHAngle, m_HAngle + m_HAngleOffset + LocalHOffset, ref m_HAngleSmooth, Mathf.Min(syncTime, 0f, m_HAngleDamp * m_SmoothScale));
            m_CurrentVAngle = Mathf.SmoothDampAngle(m_CurrentVAngle, m_VAngle, ref m_VAngleSmooth, Mathf.Min(syncTime, 0f, m_VAngleDamp * m_SmoothScale));
        }
    }

    //朝向正面
    public void Front()
    {
        this.m_HAngle = 180f;
        /*if (Target == null)
            return;
        this.m_HAngle = 180f - Target.localEulerAngles.y;*/
    }

    public void RecoveHAngleImmediate()
    {
        this.m_HAngle = 0;
        this.m_CurrentHAngle = 0;
        m_HAngleOffset = 0;
        LocalHOffset = 0;
    }

    public void SetCameraFieldOfView(float fValue)
    {
        if (MainCamera != null)
        {
            //MainCamera.fieldOfView = fValue;
            //CameraManager.Instance.CVCamera.m_Lens.FieldOfView = fValue;
            CameraSetting.Ins.SetFOV(fValue);
        }
    }

    public float GetCameraFieldOfView()
    {
        if (MainCamera != null)
        {
            //return MainCamera.fieldOfView;
            return CameraManager.Instance.CVCamera.m_Lens.FieldOfView;
        }
        return 45f;
    }
    /*
    public void SetPosition(float hAngle, float vAngle, float distance = 10f, float sideOffset = 0f, float sideOffsetY = 0f)
    {
        m_Distance = distance;
        m_HAngle = hAngle;
        m_VAngle = vAngle;
        m_SideOffset = sideOffset;
        m_SideOffsetY = sideOffsetY;

        m_CurrentDistance = m_Distance;
       
        m_CurrentHAngle = m_HAngle;
        m_CurrentVAngle = m_VAngle;
        m_CurrentSideOffset = m_SideOffset;
    }
    */

    public void SetDefaultCameraFieldOfView()
    {
        //MainCamera.fieldOfView = 45f;
        CameraManager.Instance.CVCamera.m_Lens.FieldOfView = 45f;
    }

    public static readonly float[] DefaultLookOffsetYList = { 0f, 1.85f, 1.7f, 1.7f, 1.6f }; //1-战士,2-刺客,3-法师,4-辅助
    public static readonly float[] SwimLookOffsetYList = { 0f, 2.0f, 1.7f, 1.7f, 1.6f }; //1-战士,2-刺客,3-法师,4-辅助

    public static readonly float[] BagLookOffsetYList = { 0f, 1.1f, 0.9f, 0.9f, 0.9f };
    public static readonly float[] BagSwimLookOffsetYList = { 0f, 1.3f, 1.1f, 1.1f, 0.9f };

    public void SetLookOffsetY(float y)
    {
        //if (_owner.GetCurrentStateID() == (int)CameraState.Dialog || _owner.GetCurrentStateID() == (int)CameraState.SkillPreview || _owner.GetCurrentStateID() == (int)CameraState.RolePreview)
        //{
        //    return;
        //}

        //Creature param_Creature = SceneObjectManager.Instance.Myself;
        //if (param_Creature == null || _owner == null)
        //    return;
        m_LookOffsetY = y;
    }

    public void FollowSelectHero()
    {
        if (_SpellOwner != null)
        {
            CameraSetting.Ins.SetFollow(_SpellOwner.CameraPoint);
            CameraSetting.Ins.LookAt(_SpellOwner.CameraPoint);
            return;
        }
        
        Creature follow = null;
        Creature look = null;
        if (CameraManager.Instance.FocusTarget != null)
        {
            if (CameraManager.Instance.FocusTarget.mData.IsDead)
            {
                CameraManager.Instance.FocusTarget = null;
                look = SceneObjectManager.Instance.GetSelectPlayer();
            }
            else
                look = CameraManager.Instance.FocusTarget;
        }
        else
        {
            look = SceneObjectManager.Instance.GetSelectPlayer();
        }
        if (look != null)
        {
            CameraSetting.Ins.LookAt(look.CameraPoint);
            //CameraSetting.Ins.LookAt(look.GetBone("body_hit"));
        }
        follow = SceneObjectManager.Instance.GetSelectPlayer();
        if (follow != null)
        {
            //CameraSetting.Ins.SetFollow(follow.CameraPoint);
            CameraSetting.Ins.SetFollow(CameraFollowTarget.Instance.transform);
        }
        else
        {
            /*if (SceneObjectManager.Instance.LocalPlayerCamera != null)
            {
                CameraSetting.Ins.SetFollow(SceneObjectManager.Instance.LocalPlayerCamera.transform);
                CameraSetting.Ins.LookAt(SceneObjectManager.Instance.LocalPlayerCamera.transform);
            }*/
        }
    }
    
    
    #region 大招释放时镜头处理

    public Creature _SpellOwner = null;

    public void PlaySpellSkill(Creature owner)
    {
        _SpellOwner = owner;
    }

    public void StopSpellSkill(Creature owner)
    {
        if (_SpellOwner == owner)
        {
            _SpellOwner = null;
        }
    }

    #endregion
}