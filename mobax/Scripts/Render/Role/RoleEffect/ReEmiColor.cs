using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// spine角色自发光变化效果
/// </summary>
public class ReEmiColor : IRoleEffect
{
    protected override void Init()
    {
        base.Init();
        if (_curve.length > 1)
        {
            _totalTime= _curve.keys[_curve.length-1].time;
        }
        ResetLogic();
    }

    void Update()
    {
        if (ToolStatic._tooType != ToolStatic.TOOL_TYPE.FX_MAKER)
        {
            return;
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (!_init)
            {
                Init();
            }
            EffectLogic(Vector3.zero);
        }
    }

    protected override float EffectLogic(Vector3? arg1)
    {
        StartCoroutine(Effect());
        return _totalTime;
    }

    IEnumerator Effect()
    {
        yield return new WaitForSeconds(_delayTime);
        var tick = 0f;
        while (tick<_totalTime)
        {
            tick += Time.deltaTime;
            var offset=_curve.Evaluate(tick);
            var emmitColor = Color.Lerp(_bColor, _eColor, offset);
            _roleRender.SetMpColor("_EmmisionColor",emmitColor);
            _roleRender.ApplyMPBlock();
            yield return null;
        }
    }

    protected override void ResetLogic()
    {
        StopAllCoroutines();
        _roleRender.SetMpColor("_EmmisionColor",_bColor);
        _roleRender.ApplyMPBlock();
    }
    public AnimationCurve _curve;
    [ColorUsageAttribute(true,true,0f,8f,0.125f,3f)]
    public Color _bColor;
    [ColorUsageAttribute(true,true,0f,8f,0.125f,3f)]
    public Color _eColor;
    private float _totalTime = 0f;
    
   // public IRoleRender _roleRender;
}
