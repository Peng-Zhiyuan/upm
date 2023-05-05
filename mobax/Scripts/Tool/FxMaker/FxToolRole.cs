using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;

public class FxToolRole : MonoBehaviour
{





    // Start is called before the first frame update
    void Start()
    {
        SetupSpine();
        mDir = FX_TOOL_DIR.FORWARD;
        mOffset = mCameraTrans.position - mSpine.transform.position;
    }
    public void ChangeSpine(GameObject _go,float _offsetY)
    {
        if (mSpine != null)
        {
            mSpine.SetActive(false);
        }
        mSpine = _go;
        mSpine.transform.parent = transform;
        mSpine.transform.localPosition = new Vector3(0, _offsetY, 0);
        SetupSpine();
        mSpine.SetActive(true);
    }

    void SetupSpine()
    {
        mSkeletonAnimation = mSpine.GetComponent<SkeletonAnimation>();
        mSkeleton = mSkeletonAnimation.Skeleton;
        mAnimationState = mSkeletonAnimation.AnimationState;
        mSpine.transform.localScale = new Vector3(-mScale, mScale, mScale);
        SetDir(mDir);
    }

    // Update is called once per frame
    void Update()
    {
        if (mAniLock > 0.0f)
        {
            mAniLock -= Time.deltaTime;
            if (mAniLock <= 0.0f)
            {
                Idle();
            }
        }
        if (AutoLookCamera)
        {
            if (mCameraTrans != null)
            {
                mSpine.transform.LookAt(mCameraTrans);
                mSpine.transform.Rotate(Vector3.up, 180, Space.Self);
            }
        }

    }


    public void SetColor(Color _c)
    {
        mMainColor = _c;
        //MaterialPropertyBlock _mpb = new MaterialPropertyBlock();
        //_mpb.SetColor("_Color", _c);
        //_Render.SetPropertyBlock(_mpb);
    }


    public void SetDir(FX_TOOL_DIR _dir)
    {
        mDir = _dir;
        switch (mDir)
        {
            case FX_TOOL_DIR.FORWARD:
                {
                    mSpine.transform.localScale = new Vector3(mScale, mScale, mScale);
                }
                break;
            case FX_TOOL_DIR.BACK:
                {
                    mSpine.transform.localScale = new Vector3(mScale, mScale, mScale);
                }
                break;
            case FX_TOOL_DIR.LEFT:
                {
                    mSpine.transform.localScale = new Vector3(-mScale, mScale, mScale);
                }
                break;
            case FX_TOOL_DIR.RIGHT:
                {
                    mSpine.transform.localScale = new Vector3(-mScale, mScale, mScale);
                }
                break;
        }
        Idle();
    }

    void Run()
    {
        switch (mDir)
        {
            case FX_TOOL_DIR.FORWARD:
            case FX_TOOL_DIR.LEFT:
            default:
                mSkeletonAnimation.AnimationName = "RDWalk";
                break;
            case FX_TOOL_DIR.BACK:
            case FX_TOOL_DIR.RIGHT:
                mSkeletonAnimation.AnimationName = "LUWalk01";
                break;
        }
        mSkeletonAnimation.loop = true;
        mAniLock = 0.3f;
    }
    public void PlayAni(string _ani, bool _loop = false)
    {
        mSkeletonAnimation.AnimationName = "";
        mSkeletonAnimation.loop = _loop;
        mSkeletonAnimation.AnimationName = _ani;
        mAniLock = 3.0f;
    }

    void Idle()
    {
        switch (mDir)
        {
            case FX_TOOL_DIR.FORWARD:
            case FX_TOOL_DIR.LEFT:
            default:
                mSkeletonAnimation.AnimationName = "RDIdle";
                break;
            case FX_TOOL_DIR.BACK:
            case FX_TOOL_DIR.RIGHT:
                mSkeletonAnimation.AnimationName = "LUIdle";
                break;
        }
        mSkeletonAnimation.loop = true;
    }
    public bool AutoLookCamera = false;
    public GameObject mSpine;
    public Vector3 forwardDir = Vector3.forward;
    public Vector3 rightDir = Vector3.right;

    public Transform mCameraTrans;
    // public DRMOnSendPoint sendPointHandler;


    FX_TOOL_DIR mDir = FX_TOOL_DIR.FORWARD;//前后左右

    public float mScale = 0.2f;

    Vector3 mOffset = Vector3.zero;

    SkeletonAnimation mSkeletonAnimation;
    Spine.Skeleton mSkeleton;
    Spine.AnimationState mAnimationState;
    public MeshRenderer _Render;
    Color mMainColor = Color.white;
    float mAniLock = 0.0f;
}
