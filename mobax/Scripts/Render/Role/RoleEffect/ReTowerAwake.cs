using System;
using System.Collections;
using System.Collections.Generic;
using Spine.Unity;
using UnityEngine;

public class ReTowerAwake : IRoleEffect
{
    private void Awake()
    {
        //ResetLogic();
        Init();
        ResetLogic();
    } 
    

    protected override void Init()
    {
        base.Init();
        //effetType = ROLE_EFFECT_TYPE.DEAD
        _mpbRock = new MaterialPropertyBlock();
    }

    protected override float EffectLogic(Vector3? arg1)
    {
       // var _max = RoleRender.deadTintTime;
       if (_curCoroutine != null)
       {
           StopCoroutine(_curCoroutine);
       }
       _curCoroutine=StartCoroutine(TowerAwake());
        return deadTime;
    }



    IEnumerator TowerAwake()
    {
        Debug.LogWarning("BobZuo:TowerAwake DoEffect");
        for (int i = 0; i < _roleAnis.Length; i++)
        {
            _roleAnis[i].loop = true;
            _roleAnis[i].AnimationState.TimeScale = 1.0f;
            _roleAnis[i].AnimationName = "idle01";
        }
        //stop all fx
        for (int i = 0; i < _fxGos.Length; i++)
        {
            _fxGos[i].SetActive(true);
        }
        //stop the rock ani
        for (int i = 0; i < _animators.Length; i++)
        {
            _animators[i].enabled = true;
        }

        yield return new WaitForSeconds(_grayDelayTime);
        
        var tick = 0f;
        while (tick<grayTime)
        {
            tick += Time.deltaTime;
            var offset =  Mathf.Max(0,1f-tick / grayTime);
            //gray all spines;
    
     
            for (int i = 0; i < _RoleRenders.Length; i++)
            {
                _RoleRenders[i].SetMpFloat("_GrayOffset",offset);
                Debug.LogWarning("BobZuo:TowerAwake DoEffectOffset "+offset);
            }
            //gray all stones;
            var color = Color.white;
            offset = Mathf.Max(0,1f-tick / grayTime)*0.5f+0.5f;
            color *= offset;
            _mpbRock.SetColor("_MainColor",color);
            for (int i = 0; i < _stoneRenders.Length; i++)
            {
                _stoneRenders[i].SetPropertyBlock(_mpbRock);
            }
            yield return null;
        }
        yield return new WaitForSeconds(_depthDelayTime);
 
        for (int i = 0; i < _RoleRenders.Length; i++)
        {
            _RoleRenders[i].SetMpFloat("_GrayOffset",0f);
            _RoleRenders[i].SetMpFloat("_GuideH",i==0?3f:5f);
            _RoleRenders[i].ApplyMPBlock();
            Debug.LogWarning("BobZuo:TowerAwake DoEffectFinish");
        }
        
        for (int i = 0; i < _stoneRenders.Length; i++)
        {
            _stoneRenders[i].gameObject.layer = LayerMask.NameToLayer("Default");
        }
    }
    
 

    protected override void ResetLogic()
    {
        Debug.LogWarning("BobZuo:TowerAwake Reset");
        if (_curCoroutine != null)
        {
            StopCoroutine(_curCoroutine);
        }
        for (int i = 0; i < _fxGos.Length; i++)
        {
            _fxGos[i].SetActive(false);
        }
        for (int i = 0; i < _roleAnis.Length; i++)
        {
            _roleAnis[i].loop = false;
            if (_roleAnis[i].AnimationState!=null)
            {
                _roleAnis[i].AnimationState.TimeScale = 1f;
                _roleAnis[i].AnimationName = "idle05";
            }
          
        }
        for (int i = 0; i < _animators.Length; i++)
        {
            _animators[i].enabled = false;
        }
        for (int i = 0; i < _RoleRenders.Length; i++)
        {
            _RoleRenders[i].SetMpFloat("_GrayOffset",1.0f);
            _RoleRenders[i].SetMpFloat("_GuideH",i==0?3f:5f);
            _RoleRenders[i].ApplyMPBlock();
        }
        //gray all stones;
        _mpbRock.SetColor("_MainColor",Color.gray);
        for (int i = 0; i < _stoneRenders.Length; i++)
        {
            _stoneRenders[i].SetPropertyBlock(_mpbRock);
            _stoneRenders[i].gameObject.layer = LayerMask.NameToLayer("Unlit");
        }
    }


    [Range(0.0f,5.0f)]
    public float deadTime;
    public GameObject[] _fxGos;
    public Renderer[] _stoneRenders;
    //public Renderer[] _spineRenders;
    public RoleRender[] _RoleRenders;
    public SkeletonAnimation [] _roleAnis;
    private MaterialPropertyBlock[] _mpbSpines;
    private MaterialPropertyBlock _mpbRock;
    public Animator[] _animators;
    [Range(0f,3f)]
    public float grayTime;
    public float _depthDelayTime;
    public float _grayDelayTime;
    private Coroutine _curCoroutine;


}