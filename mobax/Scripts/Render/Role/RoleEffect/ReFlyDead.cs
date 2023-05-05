using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MathDL;

public class ReFlyDead : IRoleEffect
{
    protected override void Init()
    {
        base.Init();
        effetType = ROLE_EFFECT_TYPE.DEAD_FLY;
    }

    protected override float EffectLogic(Vector3? arg1)
    {
        Vector3 _force = arg1.Value;
        StopAllCoroutines();
        StartCoroutine(DeadFlyEffect(_force));
        return deadTime;
    }
    IEnumerator DeadFlyEffect(Vector3 _force)
    {
        var dcis = PhyMath.MakeDCIS(_force);
        float t = 0f;
        _beginPos= transform.position;
        while (t<deadTime)
        {
            t += Time.deltaTime;
            transform.position=_beginPos+PhyMath.Plerp(dcis, t);
            yield return null;
        }
    }
    protected override void ResetLogic()
    {
        StopAllCoroutines();
        transform.position = _beginPos;
    }
    [Range(0.0f,5.0f)]
    public float deadTime;

    private Vector3 _beginPos;
}
