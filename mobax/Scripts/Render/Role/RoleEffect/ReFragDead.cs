using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReFragDead : IRoleEffect
{
   
    private string dieMat = "Dead";
    protected override void Init()
    {
        base.Init();
    }

    protected override float EffectLogic(Vector3? arg1)
    {
        StartCoroutine(Dead());
        return _deadTime+_deadTintTime;
    }
    
    public IEnumerator Dead()
    {
        if (_roleRender != null)
        {
            _roleRender.SwitchMaterials(dieMat);
        }
        else
        {
            yield break;
        }
      
        if (_deadTintTime > 0)
        {
            float _blackTime = _deadTintTime;
            while (_blackTime > 0.0f)
            {
                _blackTime -= Time.deltaTime;
                float _o = 1.0f - _blackTime / _deadTintTime;
                _roleRender.SetMpFloat("_BlackFadeIn", _o);
                //_roleRender.SetMpColor("BlackFadeIn", Color.Lerp(Color.white, _deadTintColor, _o));
                _roleRender.ApplyMPBlock();
                yield return null;
            }
        }
       
        if (_deadTime > 0.0f)
        {
            var _speed = 1 / _deadTime;
            var offset = 0.0f;
            while (offset < 1.0f)
            {
                _roleRender.SetMpFloat("_AlphaCutoff", offset);
                offset += Time.deltaTime * _speed;
                _roleRender.ApplyMPBlock();
                yield return null;
            }
            _roleRender.SetMpFloat("_AlphaCutoff", 1.0f);
            _roleRender.ApplyMPBlock();

        }
           /* if (_deadTime > 0.0f)
             {
                 var _speed = 1 / _deadTime;
                 var offset = 1.0f;
                 while (offset > 0.0f)
                 {
                     _roleRender.SetMpFloat("_Diss", offset);
                      offset -= Time.deltaTime * _speed;
                     yield return null;
                 }
                 _roleRender.SetMpFloat("_Diss", 0.0f);

             }*/
        

    }

    private void OnDisable()
    {
        ResetLogic();
    }


    protected override void ResetLogic()
    {
        _roleRender.ResetMaterials();
        //_roleRender.SetMpFloat("BlackFadeIn", 0);
        //_roleRender.SetMpFloat("_TintMode", 0);
        //_roleRender.SetMpColor("_TintColor", Color.white);
        // _roleRender.ApplyMPBlock();
    }
    public float _deadTime = 0.5f;
    public float _deadTintTime = 0f;
    public Color _deadTintColor = Color.black;

}
