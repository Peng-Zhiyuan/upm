
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[HelpURL("https://www.element3ds.com/thread-216743-1-1.html")]
public class E3D_Move_ToObj : MonoBehaviour
{
    public float times = 2;
    public bool loop = true;
    public float delayTime;
    public bool showLine = false;

    public bool lookAtDir = true;
    public AnimationCurve curve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));
    public GameObject targetObject;


    public bool useCurveMove = false;
    private Vector3 _startWorldPos;
    private Quaternion _startLocalRotation;
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
        //DelayTimeRun();
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

    public void PM_SetEndObj(GameObject o)
    {
        //Debug.Log("PM:EndObject:" + o.name);
        this.targetObject = o;
    }
    //================================================//


    /// <summary>
    /// Play模式下默认延迟一帧应用曲线
    /// </summary>
    /// <returns></returns>
    IEnumerator DelayOne()
    {
        yield return null;
        //Debug.Log("<color=red>" + "延迟一帧结束---" + "</color>");
    }

    public void DelayTimeRun()
    {
        StartCoroutine(StartCurveInit());
    }

    IEnumerator StartCurveInit()
    {
        yield return new WaitForSeconds(delayTime);
        SetUseCurve(true);
        MoveInit();
        startMove = true;
    }

    //初始移动
    public void MoveInit()
    {
        _startWorldPos = this.transform.position;
        _startLocalRotation = this.transform.localRotation;
    }


    void Update()
    {
        if (!useCurveMove) return;
        if (startMove)
        {
            Move_ToObj(Time.deltaTime);
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

    private float _tempTime;
    public void Move_ToObj(float deltaTime)
    {
        if (_tempTime < times)
        {
            MoveOneShot();
            _tempTime += deltaTime;
        }
        else
        {
            if (loop) MoveLoop();
            else
                useCurveMove = false;
            _tempTime = 0;
        }
    }
    private Vector3 _tempVec;
    private float _tempLength;
    private Vector3 _tempToTargetDir;
    private void MoveOneShot()
    {
        if (targetObject == null)
        {
            //Debug.Log("<color=red>" + "指定结束位置不存在！" + "</color>");
            return;
        }
        _tempLength = Vector3.Distance(_startWorldPos, targetObject.transform.position);
        _tempToTargetDir = (targetObject.transform.position - _startWorldPos).normalized;


        _tempVec = _startWorldPos + _tempToTargetDir * curve.Evaluate(_tempTime / times) * _tempLength;
        transform.position = _tempVec;

        LookAlongRoute(transform.position);
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
        transform.localRotation = Quaternion.Slerp(transform.localRotation, temp, Time.deltaTime * 5.0f);
        //更新上一个位置
        _tempLastPos = this.transform.position;
    }
    private void MoveLoop()
    {
        //To Do
        this.transform.localRotation = _startLocalRotation;
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

        Gizmos.color = Color.red;
        if (targetObject)
            Gizmos.DrawLine(transform.position, targetObject.transform.position);

        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, transform.forward * 10);
    }
}

