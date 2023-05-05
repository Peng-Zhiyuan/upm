using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReWeaponDissolved : IRoleEffect
{

    IEnumerator DissolveEffect()
    {
        _roleRender.SwitchKeyword("_DISSOLVE_EFFECT", true, RenderGroup.OnlyWeapon);
        _roleRender.SetMpFloat("_DissolveEffect", 1);
        _roleRender.SetMpColor("_DissolveColor", mDissolveColor);
        _roleRender.SetMpFloat("_DissolveDir", 1);
        _roleRender.SetMpFloat("_DissolveRange", 0.5f);
        _roleRender.ApplyMPBlock(RenderGroup.OnlyWeapon);
        mDissolveTick = mDissolveTime;
       
        while (mDissolveTick > 0.0f)
        {
            mDissolveTick -= Time.deltaTime;
            float _o = 1.0f - mDissolveTick / mDissolveTime;
            _roleRender.SetMpFloat("_DissolveRatio", _o);
            _roleRender.ApplyMPBlock(RenderGroup.OnlyWeapon);
            yield return null;
        }
        yield return new WaitForSeconds(mDissolveTime);
    }

    private void OnDisable()
    {
        ResetLogic();
    }
    protected override float EffectLogic(Vector3? arg1)
    {
        //this.ResetLogic();
        StartCoroutine(DissolveEffect());
        return mDissolveTime;
    }
    protected override void ResetLogic()
    {
        StopAllCoroutines();
        _roleRender.SwitchKeyword("_DISSOLVE_EFFECT", false, RenderGroup.OnlyWeapon);
        _roleRender.SetMpFloat("_DissolveEffect", 0);
        _roleRender.SetMpColor("_DissolveColor",  Color.yellow);
        _roleRender.SetMpFloat("_DissolveDir", 1);
        _roleRender.SetMpFloat("_DissolveRatio", 0);
        _roleRender.SetMpFloat("_DissolveRange", 0);
        _roleRender.ApplyMPBlock(RenderGroup.OnlyWeapon);
    }

    [SerializeField, ColorUsageAttribute(true, true)]
    public Color mDissolveColor = Color.yellow * 1.5f; 

    public float mDissolveTime = 1f;

    float mDissolveTick;
    
}
