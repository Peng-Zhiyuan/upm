using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;

public class SoliderTest : MonoBehaviour
{
    // Start is called before the first frame update
    IEnumerator Start()
    {
        _Go.transform.position = mBpos;
        mSkeletonAnimation = _Go.GetComponent<SkeletonAnimation>();
        mAnimationState = mSkeletonAnimation.AnimationState;
        mSkeletonAnimation.AnimationName = "RDIdle";
        mMoveTick = 0;
        mMoveTime = Vector3.Distance(mEpos, mBpos) / mMoveSpeed;

        // StartCoroutine("EyeAni");
        yield return new WaitForSeconds(1);
        IEnumerator _eye = EyeAni();
        StartCoroutine(_eye);
        yield return new WaitForSeconds(1);
        mSkeletonAnimation.AnimationName = "RDRun";
        IEnumerator _move = MoveAni();
        StartCoroutine(_move);
        yield return new WaitForSeconds(mMoveTime);
        mSkeletonAnimation.AnimationName = "RDIdle";
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator EyeAni()
    {
        float offset = 0.0f;
        while (offset < 3.0f)
        {
            offset += Time.deltaTime* EmitSpeed;
            MaterialPropertyBlock _mpb = new MaterialPropertyBlock();
            _mpb.SetFloat("_EmitOffset", offset);
            _Render.SetPropertyBlock(_mpb);
            //
           // _Render.material.SetFloat("_EmitOffset", offset);
            yield return null;
        }

    }

    IEnumerator MoveAni()
    {
        while (mMoveTick < mMoveTime)
        {
            mMoveTick += Time.deltaTime;
            _Go.transform.position = Vector3.Lerp(mBpos, mEpos, mMoveTick/ mMoveTime);
            yield return null;
        }
    }

    public GameObject _Go;
    public MeshRenderer _Render;
    Vector3 mBpos = new Vector3(8.0f, 0.25f, 15.0f);
    Vector3 mEpos = new Vector3(3.8f, 0.25f, 6.4f);
    public float mMoveSpeed = 3.0f;
    float mMoveTick = 0;
    float mMoveTime = 0;
    public float EmitSpeed = 0.5f;
    SkeletonAnimation mSkeletonAnimation;
    Spine.AnimationState mAnimationState;

}
