using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReHurt : IRoleEffect
{

    IEnumerator HurtEffect()
    {
        //_roleRender.SetMpFloat("_GrayOffset",0f);
        _roleRender.SetMpColor("_TintColor", Color.white);
        _roleRender.SetMpFloat("_TintMode", 1);
        mHurtTick = mHurtTime;
        while (mHurtTick > 0.0f)
        {
            mHurtTick -= Time.deltaTime;
            float _o = 1.0f - mHurtTick / mHurtTime;
            Color _c = Color.Lerp(Color.white, mHurtColor, _o);
            _roleRender.SetMpColor("_TintColor", _c);
            yield return null;
        }
        yield return new WaitForSeconds(mHurtTime1);
   
        _roleRender.SetMpColor("_TintColor", mHurtColor2);

        _roleRender.SetMpFloat("_TintMode", 2);
        yield return new WaitForSeconds(mHurtTime1);
        _roleRender.SetMpFloat("_TintMode", 0);
        _roleRender.SetMpColor("_TintColor", Color.white);
    }
    protected override float EffectLogic(Vector3? arg1)
    {
        this.ResetLogic();
        StartCoroutine(HurtEffect());
        return mHurtTime+mHurtTime1;
    }
    protected override void ResetLogic()
    {
        StopAllCoroutines();
        _roleRender.SetMpFloat("_TintMode", 0);
        _roleRender.SetMpColor("_TintColor", Color.white);
    }
    public Color mHurtColor = Color.red; 
    public Color mHurtColor2 = Color.white;
    public float mHurtTime = 0.2f;
    public float mHurtTime1 = 0.2f;
    float mHurtTick;
    
}
