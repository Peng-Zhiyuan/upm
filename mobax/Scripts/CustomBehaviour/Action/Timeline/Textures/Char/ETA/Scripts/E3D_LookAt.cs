using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[HelpURL("https://www.element3ds.com/thread-216743-1-1.html")]
public class E3D_LookAt : MonoBehaviour
{
    public float times = 2;
    public float delayTime;
    public bool showLine = false;
    public GameObject lookAtObj;
    public bool always = true;
    public bool lockY = true;
    public float alwaysLookAtSpeed = 10;
    public AnimationCurve curve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));
    public bool lockMovceDir = false;
    public bool useCurveMove;

    private Quaternion _startLocalRotation;
    private bool _tempUseCurveState = false;
    /// <summary>
    /// 是否为播放状态
    /// </summary>
    public bool _isPlay = false;

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
        startMove = false;
        SaveOldTransform();
        StartCoroutine(DelayOne());
        DelayTimeRun();
    }

    //================================================//

    public void PM_SetTime(float t)
    {
        times = t;
    }
    public void PM_SetTarget(GameObject t)
    {
        Debug.Log("PM:Target:" + t);
        lookAtObj = t;
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
    /// <summary>
    /// Play模式下默认延迟一帧应用曲线
    /// </summary>
    /// <returns></returns>
    IEnumerator DelayOne()
    {
        yield return null;
#if DEBUG_LOG
        Debug.Log ("<color=red>" + "延迟一帧结束---" + "</color>");
#endif
    }

    public void DelayTimeRun()
    {
        StartCoroutine(StartCurveInit());
    }
    IEnumerator StartCurveInit()
    {
        yield return new WaitForSeconds(delayTime);
        SetUseCurve(true);
        SetRotateInit();
        startMove = true;
        tempLastWorldPos = transform.position;
    }

    //初始旋转
    public void SetRotateInit()
    {
        _startLocalRotation = this.transform.localRotation;
        if (lookAtObj)
        {
            transform.localRotation = _startLocalRotation;

            ease = new EasingFunction(GetCurve);
            _vector3s = new Vector3[3];

            _vector3s[0] = transform.localEulerAngles;
            transform.LookAt(lookAtObj.transform);
            _vector3s[1] = transform.localEulerAngles;
            transform.localEulerAngles = _vector3s[0];
        }
    }

    private Vector3 tempLastWorldPos;
    private Vector3 tempNextWorldPos;
    private bool tempLast = true;
    private Vector3 tempLastDir;
    private Vector3 tempCurrentDir;
    private Quaternion lastMiddle;
    private Quaternion nextMiddle;
    private bool overLookDir = false;
    private Vector3 cacheLastPosition;
    private Quaternion cacheFinalRotation;

    void Update()
    {
        if (startMove)
        {
            if (!useCurveMove) return;
            E3D_RotateLookAtObj(Time.deltaTime);
            if (lockMovceDir && !overLookDir)
            {
                //Debug.Log("LookAtDir = " + lockMovceDir);
                if (tempLast)
                {
                    tempNextWorldPos = transform.position;
                    tempLastDir = (tempNextWorldPos - tempLastWorldPos).normalized;
                    //===============================================================//
                    lastMiddle = Quaternion.LookRotation(tempLastDir);
                    if (lastMiddle != Quaternion.identity)
                        transform.rotation = Quaternion.Slerp(transform.rotation, lastMiddle, alwaysLookAtSpeed * Time.deltaTime);
                    //===============================================================//
                    tempLast = false;
                }
                if (Time.frameCount % 2 == 0 && tempLast == false)
                {
                    tempCurrentDir = (transform.position - tempNextWorldPos).normalized;
                    //===============================================================//
                    nextMiddle = Quaternion.LookRotation(tempCurrentDir);
                    if (nextMiddle != Quaternion.identity)
                        transform.rotation = Quaternion.Slerp(transform.rotation, nextMiddle, alwaysLookAtSpeed * Time.deltaTime);
                    //===============================================================//
                    tempLastWorldPos = tempNextWorldPos;
                    tempLast = true;
                }
            }
        }
    }

    void OnEnable()
    {
        if (Application.isPlaying)
        {
            StartCoroutine(DelayOne());
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

    public void SetUseCurve(bool state)
    {
        useCurveMove = state;
        _tempTime = 0;
    }

    private float _tempTime;
    Vector3[] _vector3s;
    Vector3 _tempSelf = Vector3.zero;
    Vector3 _tempLookAt = Vector3.one;

    private Quaternion desiredRotation;
    private Quaternion lastRotation;

    public void E3D_RotateLookAtObj(float deltaTime)
    {
        if (lookAtObj == null)
        {
#if DEBUG_LOG
            Debug.Log ("<color=red>" + "旋转注视对象不存在" + "</color>");
#endif
            return;
        }

        if (_tempTime < times)
        {
            if(lockMovceDir == true)
            {
                if (_tempTime < times - 0.01 && _tempTime > times - 0.07 && overLookDir == false)
                {
                    overLookDir = true;
                    cacheLastPosition = transform.position;
                }
            }
            else
            {
                if (always == true)
                {
                    transform.LookAt(lookAtObj.transform);
                    _vector3s[1] = transform.localEulerAngles;
                    transform.localEulerAngles = _vector3s[0];

                    LookRotationToObj(_vector3s[0], _vector3s[1], _vector3s[2], (_tempTime / times));
                }
                else
                {
                    LookRotationToObj(_vector3s[0], _vector3s[1], _vector3s[2], (_tempTime / times));
                }
            }

            _tempTime += deltaTime;
        }
        else
        {
            if (always == true)
            {
                if (lockMovceDir == false && overLookDir == false)
                {
                    Vector3 lookAtPos = lookAtObj.transform.position;
                    //Debug.Log("249");
                    if (lockY)
                    {
                        lookAtPos.y = transform.position.y;
                    }
                    var diff = lookAtPos - transform.position;
                    if (diff != Vector3.zero && diff.sqrMagnitude > 0)
                    {
                        desiredRotation = Quaternion.LookRotation(diff, Vector3.up);
                    }
                    lastRotation = Quaternion.Slerp(lastRotation, desiredRotation, alwaysLookAtSpeed * deltaTime);
                    transform.rotation = lastRotation;
                }
                else
                {
                    if (overLookDir)
                    {
                        //Debug.Log("260");
                        cacheFinalRotation = Quaternion.LookRotation((transform.position - cacheLastPosition).normalized);
                        transform.rotation = Quaternion.Slerp(transform.rotation, cacheFinalRotation, alwaysLookAtSpeed * deltaTime);
                    }
                }
            }
            else
            {
                useCurveMove = false;
                _tempTime = 0;
            }
        }
    }

    /// <summary>
    /// 获取曲线数值
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    private float GetCurve(float start, float end, float value)
    {
        float result = start + (end - start) * curve.Evaluate(value);
        return result;
    }

    private delegate float EasingFunction(float start, float end, float value);
    private EasingFunction ease;

    /// <summary>
    /// 实时设置最新的旋转 rotation
    /// </summary>
    /// <param name="zeroIndex">自身eulerAngles</param>
    /// <param name="oneIndex">目标eulerAngles</param>
    /// <param name="twoIndex">实时设置的eulerAngles</param>
    /// <param name="percentage">自身-->目标中间占比</param>
    private void LookRotationToObj(Vector3 zeroIndex, Vector3 oneIndex, Vector3 twoIndex, float percentage)
    {
        #region ----Itween----
        // 锁Y
        if (lockY == true)
        {
            oneIndex.x = zeroIndex.x;
            oneIndex.z = zeroIndex.z;
            oneIndex = new Vector3(0, clerp(zeroIndex.y, oneIndex.y, 1), 0);

            twoIndex.x = 0;
            twoIndex.y = ease(zeroIndex.y, oneIndex.y, (percentage));
            twoIndex.z = 0;
        }
        else
        {
            oneIndex = new Vector3(clerp(zeroIndex.x, oneIndex.x, 1), clerp(zeroIndex.y, oneIndex.y, 1), clerp(zeroIndex.z, oneIndex.z, 1));

            twoIndex.x = ease(zeroIndex.x, oneIndex.x, (percentage));
            twoIndex.y = ease(zeroIndex.y, oneIndex.y, (percentage));
            twoIndex.z = ease(zeroIndex.z, oneIndex.z, (percentage));
        }
        transform.localRotation = Quaternion.Euler(twoIndex);
        #endregion
    }

    /// <summary>
    /// 计算两个角度过渡最短数值
    /// </summary>
    /// <param name="start">自身角度</param>
    /// <param name="end">目标角度</param>
    /// <param name="value">自身到目标的百分比</param>
    /// <returns>返回一个最短路劲</returns>
    private float clerp(float start, float end, float value)
    {
        float min = 0.0f;
        float max = 360.0f;
        float half = Mathf.Abs((max - min) / 2.0f);
        float retval = 0.0f;
        float diff = 0.0f;

        if ((end - start) < -half)
        {
            diff = ((max - start) + end) * value;
            retval = start + diff;
        }
        else if ((end - start) > half)
        {
            diff = -((max - end) + start) * value;
            retval = start + diff;
        }
        else retval = start + (end - start) * value;
        return retval;
    }
    void OnDrawGizmos()
    {
        if (!showLine) return;

        Gizmos.color = Color.red;
        if (lookAtObj)
            Gizmos.DrawLine(transform.position, lookAtObj.transform.position);

        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, transform.forward * 10);
    }
}
