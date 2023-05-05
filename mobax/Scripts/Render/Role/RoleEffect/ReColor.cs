using System.Collections;
using UnityEngine;


/// <summary>
/// 通用的变色效果
/// </summary>

public class ReColor : IRoleEffect
{
    protected override float EffectLogic(Vector3? arg1)
    {
        if (_curve.length < 2)
        {
            return 0f;
        }
        StartCoroutine(Effect());
        return _curve.keys[_curve.length - 1].time;
    }

    protected override void ResetLogic()
    {
        StopAllCoroutines();
        _roleRender.SetMpColor(_key,Color.white);
    }

    IEnumerator Effect()
    {
        var time = _curve.keys[_curve.length - 1].time;
        _tick = 0f;
        while (_tick < time)
        {
            _tick += Time.deltaTime;
            if (_tick > time)
            {
                _tick = time;
            }
            Color _c = Color.Lerp(Color.white, _color,_curve.Evaluate(_tick));
            _roleRender.SetMpColor(_key, _c);
            yield return null;
        }
    }

    public string _key = "_Color";
    [ColorUsageAttribute(true,true,0f,8f,0.125f,3f)]
    public Color _color=Color.red;
    public AnimationCurve _curve;
    private float _tick = 0f;
}
