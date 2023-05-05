using System.Collections;
using UnityEngine;


/// <summary>
/// Mesh动画
/// </summary>

public class ReMeshAni : IRoleEffect
{
    protected override float EffectLogic(Vector3? arg1)
    {
        StartCoroutine(Effect());
        return _delayTime+_time;
    }

    protected override void ResetLogic()
    {
        StopAllCoroutines();
    }

    IEnumerator Effect()
    {
        yield return new WaitForSeconds(_delayTime);
        _animator.Play(_name);
    }
    public Animator _animator;
    public string _name;
    [Range(0f,3f)]
    public float _time;
}
