/* Created:Loki Date:2023-02-02*/

using DG.Tweening;
using UnityEngine;

public static class RenderUtil
{
    public static void SwitchKeyword(Renderer render, string key, bool isSwitch)
    {
        var mats = render.GetMaterials();
        for (int j = 0; j < mats.Length; j++)
        {
            if (mats[j] == null)
            {
                continue;
            }
            if (isSwitch)
            {
                mats[j].EnableKeyword(key);
            }
            else
            {
                mats[j].DisableKeyword(key);
            }
        }
    }

    public static void SetMpFloat(Renderer renderer, string _name, float _value, MaterialPropertyBlock _matPropBlock = null)
    {
        if (_matPropBlock == null)
        {
            _matPropBlock = new MaterialPropertyBlock();
        }
        _matPropBlock.SetFloat(_name, _value);
        renderer.SetPropertyBlock(_matPropBlock);
    }

    public static void SetMpColor(Renderer renderer, string _name, Color _value, MaterialPropertyBlock _matPropBlock = null)
    {
        if (_matPropBlock == null)
        {
            _matPropBlock = new MaterialPropertyBlock();
        }
        _matPropBlock.SetColor(_name, _value);
        renderer.SetPropertyBlock(_matPropBlock);
    }

    public static void DoTweenFresnelAction(Renderer render, float EndVal, float time, MaterialPropertyBlock _matPropBlock = null)
    {
        Tween t = DOTween.To(() => EndVal, x => EndVal = x, 1f, time);
        // 给执行 t 变化时，每帧回调一次 UpdateTween 方法
        t.OnUpdate(delegate { RenderUtil.SetMpFloat(render, "_FresnelEffect", EndVal, _matPropBlock); });
    }
}