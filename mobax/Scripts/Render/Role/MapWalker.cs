using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;

public class MapWalker : MonoBehaviour
{
    enum DIR
    {
        FORWARD,
        BACK,
        LEFT,
        RIGHT,
    }

    public bool AutoLookCamera = false;
    public GameObject mGirl;
    public float moveSpeed = 10;
    public Vector3 forwardDir = Vector3.forward;
    public Vector3 rightDir = Vector3.right;

    public Transform mCameraTrans;
   // public DRMOnSendPoint sendPointHandler;


    DIR mDir = DIR.FORWARD;//前后左右

    public float mScale = 0.2f;

    Vector3 mOffset = Vector3.zero;





    SkeletonAnimation mSkeletonAnimation;
    Spine.Skeleton mSkeleton;
    Spine.AnimationState mAnimationState;
    public MeshRenderer _Render;

    public Transform huangNvTrans;


    enum AVATAR_SHOW_MODE
    {
        MAIN,
        SUB,
        ALL,
    }

    AVATAR_SHOW_MODE mShowMode = AVATAR_SHOW_MODE.MAIN;








    float mAniLock = 0.0f;
    // Start is called before the first frame update
    void Start()
    {
        mSkeletonAnimation = mGirl.GetComponent<SkeletonAnimation>();
        mSkeleton = mSkeletonAnimation.Skeleton;
        mAnimationState = mSkeletonAnimation.AnimationState;

        mDir = DIR.FORWARD;
        mGirl.transform.localScale = new Vector3(-mScale, mScale, mScale);

        mOffset = mCameraTrans.position - mGirl.transform.position;
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
        MoveUpdate();
        if (AutoLookCamera)
        {
            if (mCameraTrans!=null)
            {
                mGirl.transform.LookAt(mCameraTrans);
                mGirl.transform.Rotate(Vector3.up, 180, Space.Self);
                huangNvTrans.transform.rotation = mGirl.transform.rotation;

            }
        }

        if (Input.GetKeyDown(KeyCode.M))
        {

            switch (mShowMode)
            {
                case AVATAR_SHOW_MODE.MAIN:
                    mShowMode = AVATAR_SHOW_MODE.SUB;
                    mGirl.SetActive(false);
                    huangNvTrans.gameObject.SetActive(true);
                    break;
                case AVATAR_SHOW_MODE.SUB:
                    mShowMode = AVATAR_SHOW_MODE.ALL;
                    mGirl.SetActive(true);
                    huangNvTrans.gameObject.SetActive(true);
                    break;
                case AVATAR_SHOW_MODE.ALL:
                    mShowMode = AVATAR_SHOW_MODE.MAIN;
                    mGirl.SetActive(true);
                    huangNvTrans.gameObject.SetActive(false);
                    break;
            }
        }
    }
    bool switchGirlShow = true;
    Vector3 LastDir = Vector3.zero;

    public float clicksPerSecond = 10;
    float clicksTimer;

    void MoveUpdate()
    {
        clicksTimer += Time.deltaTime;

        float disX = 0.0f;
        float disZ = 0.0f;
        float _speed = moveSpeed;
        Vector3 CurDir = Vector3.zero;
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            disX -= Time.deltaTime * _speed;
            mDir = DIR.BACK;
            mGirl.transform.localScale = new Vector3(mScale, mScale, mScale);

            MaterialPropertyBlock _mpb = new MaterialPropertyBlock();
            _mpb.SetVector("_LightDirAdjust", new Vector4(0, 0, -1, 0));
            _Render.SetPropertyBlock(_mpb);
            CurDir = Vector3.left;
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            disX += Time.deltaTime * _speed;
            mDir = DIR.FORWARD;
            mGirl.transform.localScale = new Vector3(mScale, mScale, mScale);

            MaterialPropertyBlock _mpb = new MaterialPropertyBlock();
            _mpb.SetVector("_LightDirAdjust",new Vector4(0,0,-1,0));
            _Render.SetPropertyBlock(_mpb);
            CurDir = Vector3.right;
        }
        if (Input.GetKey(KeyCode.UpArrow))
        {
            disZ += Time.deltaTime * _speed;
            mDir = DIR.RIGHT;
            mGirl.transform.localScale = new Vector3(-mScale, mScale, mScale);

            MaterialPropertyBlock _mpb = new MaterialPropertyBlock();
            _mpb.SetVector("_LightDirAdjust", new Vector4(0, 0, 1, 0));
            _Render.SetPropertyBlock(_mpb);
            CurDir = Vector3.forward;
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            disZ -= Time.deltaTime * _speed;
            mDir = DIR.LEFT;
            mGirl.transform.localScale = new Vector3(-mScale, mScale, mScale);

            MaterialPropertyBlock _mpb = new MaterialPropertyBlock();
            _mpb.SetVector("_LightDirAdjust", new Vector4(0, 0, 1, 0));
            _Render.SetPropertyBlock(_mpb);
            CurDir = Vector3.back;
        }
        //if (CurDir != LastDir)
        //{
        //    sendPointHandler.CurPoint = mGirl.transform.position;
        //    sendPointHandler.IsTriggered = true;
            
        //}else if (CurDir != Vector3.zero && clicksTimer > 1.0f / clicksPerSecond) {
        //    clicksTimer = 0;
        //    sendPointHandler.CurPoint = mGirl.transform.position;
        //    sendPointHandler.IsTriggered = true;
        //}
        
        
        LastDir = CurDir;

        if (disX != 0.0f)
        {
            Vector3 _tpos= mGirl.transform.position +rightDir * disX;
            mGirl.transform.position = _tpos;
            mCameraTrans.position = mGirl.transform.position + mOffset;
            _tpos.y = huangNvTrans.transform.position.y;
            huangNvTrans.transform.position = _tpos+Vector3.forward;
            Run();
        }
        if (disZ != 0.0f)
        {
            Vector3 _tpos = mGirl.transform.position + forwardDir * disZ;
            mGirl.transform.position = _tpos;
            mCameraTrans.position = mGirl.transform.position + mOffset;
            _tpos.y = huangNvTrans.transform.position.y;
            huangNvTrans.transform.position = _tpos + Vector3.forward;
            Run();
        }
    }
    void PlayMove()
    {

    }

    void Run()
    {
        switch (mDir)
        {
            case DIR.FORWARD:
            case DIR.LEFT:
            default:
                mSkeletonAnimation.AnimationName = "RDWalk";
                break;
            case DIR.BACK:
            case DIR.RIGHT:
                mSkeletonAnimation.AnimationName = "LUWalk01";
                break;
        }
        mSkeletonAnimation.loop = true;
        mAniLock = 0.3f;
    }

    void Idle()
    {
        switch (mDir)
        {
            case DIR.FORWARD:
            case DIR.LEFT:
            default:
                mSkeletonAnimation.AnimationName = "RDIdle";
                break;
            case DIR.BACK:
            case DIR.RIGHT:
                mSkeletonAnimation.AnimationName = "LUIdle";
                break;
        }
        mSkeletonAnimation.loop = true;
    }

}
