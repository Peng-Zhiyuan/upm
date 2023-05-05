/*
========================================
	脚本名字_： E3D_Scale
	创建时间_： 2019/6/2/15/48/21
	创建者_   微元素
========================================
*/

using System;
using System.Collections;

using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[HelpURL("https://www.element3ds.com/thread-216743-1-1.html")]
public class E3D_Scale : MonoBehaviour
{
    public float times = 2;
    public bool loop = true;
    public float delayTime;
    public bool showLine = false;

    public float intensity = 1;
    public bool isScaleX = true;
    public bool isScaleY = true;
    public bool isScaleZ = true;
    public AnimationCurve curve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));

    public bool useCurveMove = false;
    private Vector3 _localScale;
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
    public bool startMove;
    void Start()
    {
        startMove = false;
        SaveOldTransform();
        //StartCoroutine(DelayOne());
        DelayTimeRun();
    }
    /// <summary>
    /// Play模式下默认延迟一帧应用曲线
    /// </summary>
    /// <returns></returns>
    IEnumerator DelayOne()
    {
        yield return null;
#if DEBUG_LOG
        Debug.Log("<color=red>" + "延迟一帧结束---" + "</color>");
#endif
    }

    private float fixedDeltaTime = 1.0f / 60;
    private float deltaTime;
    void Update()
    {
        //=========================================//
        if (startMove)
        {
            if (!useCurveMove) return;
            float delta = Time.deltaTime;
            while (delta > fixedDeltaTime)
            {
                deltaTime = fixedDeltaTime;
                ScaleControll(deltaTime);
                delta -= fixedDeltaTime;
            }
            if (delta > 0f)
            {
                deltaTime = delta;
                ScaleControll(deltaTime);
            }
        }
        //=========================================//
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
    //延迟执行
    public void DelayTimeRun()
    {
        StartCoroutine(StartCurveInit());
    }
    //初始化数据
    IEnumerator StartCurveInit()
    {
        yield return new WaitForSeconds(delayTime);
        SetUseCurve(true);
        CacheInitInfo();
        startMove = true;
    }
    public void CacheInitInfo()
    {
        _localScale = this.transform.localScale;
    }

    //脚本状态切换
    public void SetUseCurve(bool state)
    {
        useCurveMove = state;
        _tempTime = 0;
    }
    private float _tempTime;
    private Vector3 _tempScale = Vector3.one;

    public void ScaleControll(double deltaTime)
    {
        if (_tempTime < times)
        {
            PlayOnShotScale();
            _tempTime += (float)deltaTime;
        }
        else
        {
            if (loop) PlayLoopScale();
            else
                useCurveMove = false;
            _tempTime = 0;
        }
    }
    float _percentage;
    private void PlayOnShotScale()
    {
        _percentage = (_tempTime / times);
        if (isScaleX && isScaleY && isScaleZ)
        {
            _tempScale = new Vector3(_localScale.x + curve.Evaluate(_percentage) * intensity,
                                    _localScale.y + curve.Evaluate(_percentage) * intensity,
                                    _localScale.z + curve.Evaluate(_percentage) * intensity);
        }
        else if (isScaleX)
        {
            _tempScale = new Vector3(_localScale.x + curve.Evaluate(_percentage) * intensity, _localScale.y, _localScale.z);
        }
        else if (isScaleY)
        {
            _tempScale = new Vector3(_localScale.x, _localScale.y + curve.Evaluate(_percentage) * intensity, _localScale.z);
        }
        else if (isScaleZ)
        {
            _tempScale = new Vector3(_localScale.x, _localScale.y, _localScale.z + curve.Evaluate(_percentage) * intensity);
        }
        transform.localScale = _tempScale;
    }
    private void PlayLoopScale()
    {
        //transform.localScale = startLocalScale;
    }
}

