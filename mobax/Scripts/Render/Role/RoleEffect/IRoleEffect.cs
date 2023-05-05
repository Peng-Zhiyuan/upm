using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditorInternal;
#endif
using UnityEngine;

public enum ROLE_EFFECT_TYPE
{
    NULL,
    AWAKE,
    HURT,
    DEAD,
    WEAPON_DISSOLVED,
    DEAD_FLY,
    // SPEC1,
    // MAX,

}

public class IRoleEffect : MonoBehaviour
{
    protected bool _init = false;
    public ROLE_EFFECT_TYPE effetType = ROLE_EFFECT_TYPE.NULL;
    //public RoleSpine roleSpine;
    public IRoleRender _roleRender;
    [Range(0.0f,5.0f)]
    public float _delayTime;
/*    public float TakeEffect(ROLE_EFFECT_TYPE _ret,Vector3 arg1)
    {
        if (!_init)
        {
            Init();
        }
        if (_ret == effetType)
        {
            return EffectLogic(arg1);
        }

        return 0f;
    }*/

    public float PlayEffect(Vector3? pos)
    {
        if (!_init)
        {
            Init();
        }
        return EffectLogic(pos);
    }
    public void Reset(ROLE_EFFECT_TYPE _ret)
    {
        if (_ret == effetType)
        {
            ResetLogic();
        }
    }

    protected virtual void Init()
    {
        _init = true;
        if (_roleRender == null)
        {
            _roleRender = transform.GetComponent<IRoleRender>();
        }
    }
    protected virtual float EffectLogic(Vector3? arg1)
    {
        return 0f;
    }
    protected virtual void ResetLogic()
    {

    }


}
