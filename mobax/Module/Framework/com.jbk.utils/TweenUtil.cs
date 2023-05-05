using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;

public static class TweenUtil
{
    public static TweenerCore<int, int, NoOptions> TweenTo(int from, int to, float duration, Action<int> onUpdate)
    {
        var currentValue = from;
        return DOTween.To(() => currentValue, x =>
        {
            currentValue = x;
            onUpdate?.Invoke(x);
        }, to, duration);
    }
}