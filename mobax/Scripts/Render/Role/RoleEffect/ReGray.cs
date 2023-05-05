using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReGray : IRoleEffect
{
    protected override void Init()
    {
        base.Init();
        _mpb = new MaterialPropertyBlock();
        if (_render == null)
        {
            _render = transform.GetComponent<Renderer>();
        }
        ResetLogic();
    }

    protected override float EffectLogic(Vector3? arg1)
    {
        StartCoroutine(GrayEffect());
        return _time;
    }

    IEnumerator GrayEffect()
    {
        var tick = 0f;
        _mpb.Clear();
        _mpb.SetFloat("_GrayDir",_grayDir?0f:1.0f);
        while (tick<_time)
        {
            tick += Time.deltaTime;
            var offset = tick / _time;
            _mpb.SetFloat("_GrayOffset",offset);
            _render.SetPropertyBlock(_mpb);
            yield return null;
        }
        _mpb.SetFloat("_GrayDir",1.1f);
        _render.SetPropertyBlock(_mpb);
    }

    protected override void ResetLogic()
    {
        _mpb.Clear();
        _mpb.SetFloat("_GrayOffset",0f);
        _render.SetPropertyBlock(_mpb);
    }

    public bool _grayDir = true;
    [Range(0.0f, 5.0f)] 
    public float _time;
    private MaterialPropertyBlock _mpb;
    public Renderer _render;
    
}
