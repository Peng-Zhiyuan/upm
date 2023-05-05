using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[HelpURL("https://www.element3ds.com/thread-216743-1-1.html")]
public class E3D_Move_A_To_B_Base : MonoBehaviour
{
    private E3D_Move_A_To_B[] sameScriptQueue;
    private int index = 0;
    private int maxIndex;

    public void PM_SetOn()
    {
        if (sameScriptQueue.Length > 1)
        {
            for (int i = 0; i < sameScriptQueue.Length; i++)
            {
                sameScriptQueue[i].enabled = false;
            }
        }

        if (sameScriptQueue[0].enabled == false)
        {
            sameScriptQueue[0].enabled = true;
        }
        else
        {
            sameScriptQueue[0].enabled = false;
            sameScriptQueue[0].enabled = true;
        }
    }


    public virtual void OnUpdate() { }

    public void Awake()
    {
        sameScriptQueue = GetComponents<E3D_Move_A_To_B>();
        maxIndex = sameScriptQueue.Length;
        for (int i = 0; i < sameScriptQueue.Length; i++)
        {
            if (i != 0)
                sameScriptQueue[i].enabled = false;
        }
    }

    public void Update()
    {
        if (index < maxIndex || index == 0)
        {
            if (sameScriptQueue[index].overMove)
            {
                if (++index < maxIndex && sameScriptQueue[index])
                {
                    sameScriptQueue[index].enabled = true;
                }
            }
            else
            {
                if (sameScriptQueue[index] && sameScriptQueue[index].startMove)
                    sameScriptQueue[index].OnUpdate();
            }
        }
        else
        {
            index = 0;
        }
    }

}


[HelpURL("https://www.element3ds.com/thread-216743-1-1.html")]
public class E3D_Move_A_To_B : E3D_Move_A_To_B_Base
{
    public float times = 2;
    public bool loop = true;
    public float delayTime;
    public bool showLine = false;

    public bool lookAtDir = true;
    public AnimationCurve curve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));
    public GameObject startObject;
    public GameObject endObject;

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

    //================================================//
    public void PM_SetStartObj(GameObject obj)
    {
        //Debug.Log("PM:GmaeObjct:" + obj.name);
        startObject = obj;
    }

    public void PM_SetEndObj(GameObject obj)
    {
        //Debug.Log("PM:GmaeObjct:" + obj.name);
        endObject = obj;
    }
    public void PM_SetTime(float t)
    {
        //Debug.Log("PM:Time:" + t);
        times = t;
    }

    //================================================//
    public bool overMove;
    public bool startMove;
    //================================================//
    void OnEnable()
    {
        if (Application.isPlaying)
        {
            startMove = false;
            overMove = false;
            DelayTimeRun();
            SetUseCurve(true);
        }
    }

    void OnDisable()
    {
        if (Application.isPlaying)
        {
            overMove = false;
            startMove = false;
            SetUseCurve(false);
            //SetOldTransform();
        }
    }

    public void Start()
    {
        overMove = false;
        startMove = false;
        SaveOldTransform();
        DelayTimeRun();
    }

    public void DelayTimeRun()
    {
        StartCoroutine(StartCurveInit());
    }
    IEnumerator StartCurveInit()
    {
        yield return new WaitForSeconds(delayTime);
        MoveInit();
        SetUseCurve(true);
        startMove = true;
    }

    //初始旋转
    public void MoveInit()
    {
#if DEBUG_LOG
        Debug.Log(this.gameObject.name);
#endif
        _startWorldPos = this.transform.position;
        _startLocalRotation = this.transform.localRotation;
        if (startObject)
            this.transform.position = startObject.transform.position;
        if (lookAtDir && endObject)
            transform.LookAt(endObject.transform);
    }

    private float fixedDeltaTime = 1.0f / 60;
    private float m_LastUpdateShowTime = 0f;  //上一次更新帧率的时间;  
    private float m_UpdateShowDeltaTime = 0.01f;//更新帧率的时间间隔;  

    public override void OnUpdate()
    {
        //===============================20200430===================================//

        if (!useCurveMove) return;
        if (Time.realtimeSinceStartup - m_LastUpdateShowTime >= m_UpdateShowDeltaTime)
        {
            if (startMove)
                Move_A2B(fixedDeltaTime);
            m_LastUpdateShowTime = Time.realtimeSinceStartup;
        }

        //===============================20200430===================================//
    }

    float _tempTime;

    public void Move_A2B(float deltaTime)
    {
        if (_tempTime < times)
        {
            MoveOneShot(deltaTime);
            _tempTime += deltaTime;
        }
        else
        {
            if (loop)
            {
                this.transform.position = startObject.transform.position;
                this.transform.LookAt(endObject.transform);
            }
            else
            {
                useCurveMove = false;
                overMove = true;
                startMove = false;
            }
            _tempTime = 0;
        }
    }

    private Vector3 _tempVec;
    private float _tempLength;
    private Vector3 _tempTwoPointDir;

    private Vector3 _tempForwardDir;
    private Vector3 _tempLastPos;
    private void MoveOneShot(float deltaTime)
    {
        if (endObject == null)
        {
            Debug.Log("<color=red>" + "指定结束位置不存在！" + "</color>");
            return;
        }
        if (startObject == null)
        {
            Debug.Log("<color=red>" + "指定初始位置不存在！" + "</color>");
            return;
        }

        _tempLength = Vector3.Distance(startObject.transform.position, endObject.transform.position);
        _tempTwoPointDir = (endObject.transform.position - startObject.transform.position).normalized;
        _tempVec = startObject.transform.position + _tempTwoPointDir * curve.Evaluate(_tempTime / times) * _tempLength;
        transform.position = _tempVec;

        if (transform.forward != _tempTwoPointDir)
        {
            if (lookAtDir)
            {
                _tempForwardDir = (transform.position - _tempLastPos).normalized;
                //注视旋转--（Z朝向，自身（本地）Y朝向）;
                Quaternion temp = Quaternion.LookRotation(_tempForwardDir, transform.InverseTransformDirection(transform.up));
                transform.localRotation = Quaternion.Slerp(transform.localRotation, temp, deltaTime * 5.0f);
                
                _tempLastPos = this.transform.position;
            }
        }
    }

    public void SetUseCurve(bool state)
    {
        useCurveMove = state;
        _tempTime = 0;
    }
}




