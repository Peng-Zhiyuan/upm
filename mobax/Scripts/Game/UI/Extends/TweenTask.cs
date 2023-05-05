using System.Threading.Tasks;
using DG.Tweening;

public static class TweenTask
{
    public static Task Do(Tween tween)
    {
        var task = new TaskCompletionSource<bool>();
        tween.OnComplete(() =>
        {
            task.SetResult(true);
        });

        return task.Task;
    }
}