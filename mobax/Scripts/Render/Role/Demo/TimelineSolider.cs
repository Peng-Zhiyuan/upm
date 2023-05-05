using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;

public class TimelineSolider : MonoBehaviour
{

    public void Awake()
    {
        Init();
    }
    public void Init ()
    {
        if (mAni == null)
            mAni = gameObject.GetComponent<SkeletonAnimation>();
        if (mAniGraphic == null)
            mAniGraphic = gameObject.GetComponent<SkeletonGraphic>();
    }
    public void PlayAni(string _ani, float elapsed = 0)
    {
        if (mAni != null) {
            if (!Application.isPlaying) {
                mAni.Initialize(true);
            }
            mAni.AnimationName = _ani;
            if (!Application.isPlaying) {
                mAni.Update (elapsed);
            }
        }
        if (mAniGraphic != null) {
            mAniGraphic.startingAnimation = null;
            mAniGraphic.startingAnimation = _ani;
            mAniGraphic.Initialize(true);
        }
    }
    public void PlaySkill()
    {
        if (mAni == null)
            return;
        mAni.AnimationName = "RDAttack";
        mFx.transform.position = transform.position;
        mFx.SetActive(false);
        mFx.SetActive(true);
        //var _FxUnit = mFx.GetComponent<FxToolUti>();
        //if (_FxUnit != null)
        //{
        //    _FxUnit.Reset();
        //}
    }
    SkeletonAnimation mAni;
    public SkeletonAnimation Anim
    {
        get {
            return mAni;
        }
    }
    SkeletonGraphic mAniGraphic;
    public SkeletonGraphic AnimGraphic
    {
        get {
            return mAniGraphic;
        }
    }
    public GameObject mFx;
}
