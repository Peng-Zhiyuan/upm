using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class CameraCtrl_Shake
{
    private CameraManager _owner = null;
    private float _cd = 0f;
    private Transform ShakeTransform
    {
        get
        {
            //return _owner.MainCamera.GetComponent<CinemachineBrain>().ActiveVirtualCamera.VirtualCameraGameObject.transform;
            return _owner.CVCamera.transform;
        }
    }
    public bool Enable
    {
        get;set;
    }

    public void UpdatePos(Vector3 pos, float effectId)
    {
        if (!Enable)
            return;
        if (ShakeTransform.gameObject.activeSelf)
        {
            ShakeTransform.localPosition = pos;
        }
    }

    float mIntensity = 0.7f;
    float mCurTime = 0f;
    float mDuration = 0f;
    private Vector2 mShakeOffset;
    public Vector2 ShakeOffset
    {
        get { return mShakeOffset; }
    }
    public void StartShake(float intensity, float durTime)
    {
        if (Enable)
            return;

        if (_cd > 0)
            return;

        mIntensity = intensity;
        mDuration = durTime;
        Enable = true;
        _cd = 0f;
    }

    public void Update(float deltaTime)
    {
        if(_cd > 0)
        {
            _cd -= deltaTime;
        }

        if (!Enable) return;

        float factor = mIntensity;

        float t = mCurTime / mDuration;
        factor = Mathf.Lerp(mIntensity, 0f, t);

        mShakeOffset = Random.insideUnitSphere * factor;

        if (deltaTime == 0f)
            mShakeOffset = Vector2.zero;

        if (mDuration > 0f)
        {
            mCurTime += deltaTime;
            if (mCurTime >= mDuration)
            {
                StopShake();
            }
        }

        if (ShakeTransform.gameObject.activeSelf)
        {
            ShakeTransform.localPosition += new Vector3(ShakeOffset.x, ShakeOffset.y, 0f);
        }
    }

    public void StopShake()
    {
        Reset();
    }

    public void Reset()
    {
        mCurTime = 0f;
        Enable = false;
        mShakeOffset = Vector3.zero;
        //_cd = 3f;
    }

    public void Init(CameraManager owner)
    {
        _owner = owner;

        Reset();
    }

    public void Dispose()
    {
        _owner = null;
    }
}
