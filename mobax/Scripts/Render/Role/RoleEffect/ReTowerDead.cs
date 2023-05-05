using System;
using System.Collections;
using System.Collections.Generic;
using Spine.Unity;
using UnityEngine;

public class ReTowerDead : IRoleEffect
{
    private void Awake()
    {
        //ResetLogic();
    }

    protected override void Init()
    {
        base.Init();
        //effetType = ROLE_EFFECT_TYPE.DEAD;
        _mpbRock = new MaterialPropertyBlock();
    }

    protected override float EffectLogic(Vector3? arg1)
    {
       // var _max = RoleRender.deadTintTime;
        StartCoroutine(TowerDead());
        return deadTime;
    }



    IEnumerator TowerDead()
    {
        for (int i = 0; i < _roleAnis.Length; i++)
        {
            if (i == 0)
            {
                _roleAnis[i].loop = false;
                _roleAnis[i].AnimationState.TimeScale = 1.0f;
                _roleAnis[i].AnimationName = "idle05";
            }
            Debug.LogWarning("TowerDead");
        }
        //stop all fx
        for (int i = 0; i < _fxGos.Length; i++)
        {
            _fxGos[i].SetActive(false);
        }
        //stop the rock ani
        for (int i = 0; i < _animators.Length; i++)
        {
            _animators[i].enabled = false;
        }
        for (int i = 0; i < _stoneRenders.Length; i++)
        {
            _stoneRenders[i].gameObject.layer = LayerMask.NameToLayer("Unlit");
        }


        yield return new WaitForSeconds(_grayDelayTime);
        var tick = 0f;
        while (tick<grayTime)
        {
            tick += Time.deltaTime;
            var offset = tick / grayTime;
            //gray tower spine;
            _RoleRenders[0].SetMpFloat("_GrayOffset",offset);
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
        _RoleRenders[0].SetMpFloat("_GrayOffset",1.0f);
        // for (int i = 0; i < _spineRenders.Length; i++)
        // {
        //     _spineRenders[i].SetPropertyBlock(_mpbSpine);
        // }
        yield return new WaitForSeconds(_depthDelayTime);
        Debug.LogWarning("BobZuo:ChangeDepthNow");
        _RoleRenders[0].SetMpFloat("_GuideH",0.9f);
        // for (int i = 0; i < _spineRenders.Length; i++)
        // {
        //     _spineRenders[i].SetPropertyBlock(_mpbSpine);
        // }
    }

    protected override void ResetLogic()
    {
 
        for (int i = 0; i < _fxGos.Length; i++)
        {
            _fxGos[i].SetActive(true);
        }
        for (int i = 0; i < _roleAnis.Length; i++)
        {
            if (i == 0)
            {
                _roleAnis[i].loop = true;
                _roleAnis[i].AnimationState.TimeScale = 1f;
                _roleAnis[i].AnimationName = "idle01";
                Debug.LogWarning("TowerDeadReset");
            }

        }
        for (int i = 0; i < _animators.Length; i++)
        {
            _animators[i].enabled = true;
        }
   

        for (int i = 0; i < _RoleRenders.Length; i++)
        {
            _RoleRenders[i].SetMpFloat("_GrayOffset",0f);
            _RoleRenders[i].SetMpFloat("_GuideH",3.0f);
            _RoleRenders[i].ApplyMPBlock();
        }
        //gray all stones;
        _mpbRock.SetColor("_MainColor",Color.white);
        for (int i = 0; i < _stoneRenders.Length; i++)
        {
            _stoneRenders[i].SetPropertyBlock(_mpbRock);
            _stoneRenders[i].gameObject.layer = LayerMask.NameToLayer("Default");
        }
        
    }


    [Range(0.0f,5.0f)]
    public float deadTime;
    public GameObject[] _fxGos;
    public Renderer[] _stoneRenders;
    //public Renderer[] _spineRenders;
    public RoleRender[] _RoleRenders;
    public SkeletonAnimation [] _roleAnis;
    private MaterialPropertyBlock _mpbRock;
    public Animator[] _animators;
    [Range(0f,3f)]
    public float grayTime;
    public float _depthDelayTime;
    public float _grayDelayTime;
}