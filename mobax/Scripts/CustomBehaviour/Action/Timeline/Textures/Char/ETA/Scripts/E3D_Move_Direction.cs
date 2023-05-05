using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[HelpURL("https://www.element3ds.com/thread-216743-1-1.html")]
public class E3D_Move_Direction : MonoBehaviour
{
    public float times = 2;
    public bool loop = true;
    public float delayTime;
    public bool showLine = false;

    public bool lookAtDir = true;
    public float lookAtSpeed = 2.0f;
    public Vector3 direction = Vector3.zero;
    public float intensity = 1;
    public AnimationCurve curve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));
    public bool useCurveMove = false;
    public bool world = false;
    public E3D_Move_Enum moveEnum = E3D_Move_Enum.forward;
    public int enumIndex = 2;

    /// <summary>
    /// 是否为播放状态
    /// </summary>
    public bool _isPlay = false;

    public Vector3 _startLocalPos;

    private OldTransformInfo _oldTransformInfo;
    //保存初始设置
    public void SaveOldTransform()
    {
        _oldTransformInfo.localPosition = transform.localPosition;
        _oldTransformInfo.localEulerAngles = transform.localEulerAngles;
        _oldTransformInfo.localScale = transform.localScale;
    }

    //设置初始值
    public void SetOldTransform()
    {
        transform.localPosition = _oldTransformInfo.localPosition;
        transform.localEulerAngles = _oldTransformInfo.localEulerAngles;
        transform.localScale = _oldTransformInfo.localScale;
    }
    public bool startMove;
    void Start()
    {
        SaveOldTransform();
        startMove = false;
        DelayTimeRun();
    }

    //================================================//

    public void PM_SetTime(float t)
    {
        Debug.Log("PM:Time:" + t);
        times = t;
    }

    public void PM_SetOn()
    {
        if (this.enabled == false)
        {
            this.enabled = true;
        }
        else
        {
            this.enabled = false;
            this.enabled = true;
        }
    }
    //================================================//

    public void DelayTimeRun()
    {
        StartCoroutine(StartCurveInit());
    }
    IEnumerator StartCurveInit()
    {
        SaveOldTransform();
        MoveInit();
        yield return new WaitForSeconds(delayTime);
        SetUseCurve(true);
        startMove = true;
        inverseTimes = 1.0f / times;
        //m_LastUpdateShowTime = Time.realtimeSinceStartup;
    }
    private Vector3 tempWorldPos;
    //初始旋转
    public void MoveInit()
    {
        _startLocalPos = this.transform.localPosition;
        tempWorldPos = this.transform.position;
    }
    public void UseCurve(bool useCurveState)
    {
        useCurveMove = useCurveState;
    }

    private float fixedDeltaTime = 1.0f / 60;
    private float inverseTimes;
    private Vector3 endMovePos;
    private float m_LastUpdateShowTime = 0f;  //上一次更新帧率的时间;    
    private float m_UpdateShowDeltaTime = 0.01f;//更新帧率的时间间隔;  
    void Update()
    {
        if (useCurveMove && startMove)
        {
            if (Time.realtimeSinceStartup - m_LastUpdateShowTime >= m_UpdateShowDeltaTime)
            {
                MoveDirection(fixedDeltaTime);
                m_LastUpdateShowTime = Time.realtimeSinceStartup;
            }
        }
    }

    void OnEnable()
    {
        if (Application.isPlaying)
        {
            DelayTimeRun();
            SetUseCurve(true);
        }
    }

    void OnDisable()
    {
        if (Application.isPlaying)
        {
            startMove = false;
            SetUseCurve(false);
            SetOldTransform();
        }
    }

    private float _tempTime;
    public void MoveDirection(float deltaTime)
    {
        if (_tempTime < times)
        {
            MoveOneShot();
            _tempTime += deltaTime;
        }
        else
        {
            if (loop)
            {
                MoveLoop();
            }
            else
            {
                if (times < 0.1f)
                    transform.localPosition = endMovePos;
                useCurveMove = false;
            }

            _tempTime = 0;
        }
    }

    private Vector3 _tempVec;
    private void MoveOneShot()
    {
        if (world)
        {
            _tempVec = _startLocalPos + direction.normalized * curve.Evaluate(_tempTime * inverseTimes) * intensity;
            transform.localPosition = _tempVec;
            LookAlongRoute(transform.position);
        }
        else
        {
            float tT = curve.Evaluate(inverseTimes * _tempTime);

            if (transform.parent != null)
            {
                Quaternion que = QuaternionUtils.SubtractRotation(Quaternion.identity, transform.parent.transform.rotation);
                switch (moveEnum)
                {
                    case E3D_Move_Enum.forward:
                        _tempVec = _startLocalPos + que * transform.forward * tT * intensity;
                        endMovePos = _startLocalPos + que * transform.forward * intensity;
                        break;
                    case E3D_Move_Enum.back:
                        _tempVec = _startLocalPos + que * transform.forward * tT * intensity * (-1);
                        endMovePos = _startLocalPos + que * transform.forward * intensity * (-1);
                        break;
                    case E3D_Move_Enum.up:
                        _tempVec = _startLocalPos + que * transform.up * tT * intensity;
                        endMovePos = _startLocalPos + que * transform.up * intensity;
                        break;
                    case E3D_Move_Enum.down:
                        _tempVec = _startLocalPos + que * transform.up * tT * intensity * (-1);
                        endMovePos = _startLocalPos + que * transform.up * intensity * (-1);
                        break;
                    case E3D_Move_Enum.right:
                        _tempVec = _startLocalPos + que * transform.right * tT * intensity;
                        endMovePos = _startLocalPos + que * transform.right * intensity;
                        break;
                    case E3D_Move_Enum.left:
                        _tempVec = _startLocalPos + que * transform.right * tT * intensity * (-1);
                        endMovePos = _startLocalPos + que * transform.right * intensity * (-1);
                        break;
                }
            }
            else
            {
                switch (moveEnum)
                {
                    case E3D_Move_Enum.forward:
                        _tempVec = _startLocalPos + transform.forward * tT * intensity;
                        endMovePos = _startLocalPos + transform.forward * intensity;
                        break;
                    case E3D_Move_Enum.back:
                        _tempVec = _startLocalPos + transform.forward * tT * intensity * (-1);
                        endMovePos = _startLocalPos + transform.forward * intensity * (-1);
                        break;
                    case E3D_Move_Enum.up:
                        _tempVec = _startLocalPos + transform.up * tT * intensity;
                        endMovePos = _startLocalPos + transform.up * intensity;
                        break;
                    case E3D_Move_Enum.down:
                        _tempVec = _startLocalPos + transform.up * tT * intensity * (-1);
                        endMovePos = _startLocalPos + transform.up * intensity * (-1);
                        break;
                    case E3D_Move_Enum.right:
                        _tempVec = _startLocalPos + transform.right * tT * intensity;
                        endMovePos = _startLocalPos + transform.right * intensity;
                        break;
                    case E3D_Move_Enum.left:
                        _tempVec = _startLocalPos + transform.right * tT * intensity * (-1);
                        endMovePos = _startLocalPos + transform.right * intensity * (-1);
                        break;
                }
            }
            transform.localPosition = _tempVec;
        }
    }
    private Vector3 _tempForwardDir;
    private Vector3 _tempLastPos;

    /// <summary>
    /// 旋转注视曲线
    /// </summary>
    /// <param name="targetVec"></param>
    public void LookAlongRoute(Vector3 targetVec)
    {
        if (!lookAtDir) return;
        _tempForwardDir = (targetVec - _tempLastPos).normalized;

        //注视旋转--（Z朝向，自身（本地）Y朝向）;
        Quaternion temp = Quaternion.LookRotation(_tempForwardDir, transform.InverseTransformDirection(transform.up));
        transform.localRotation = Quaternion.Slerp(transform.localRotation, temp, Time.deltaTime * lookAtSpeed);
        //更新上一个位置
        _tempLastPos = this.transform.position;
    }

    private void MoveLoop()
    {
        this.transform.localPosition = _startLocalPos;
    }

    //更新状态
    public void SetUseCurve(bool state)
    {
        useCurveMove = state;
        _tempTime = 0;
    }

    void OnDrawGizmos()
    {
        if (!showLine) return;

        Gizmos.color = Color.blue;
        if (world)
            Gizmos.DrawRay(transform.position, direction * 10);
        else
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(transform.position, transform.forward * 10);
        }
    }
}

