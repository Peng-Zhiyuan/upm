using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;
using UnityEngine.UI;

public static class CodeAnimation 
{

    public static void PlayElementAnimation(ListView listView, ref CancellationTokenSource cancellationTokenSource)
    {
        var list = listView.ViewList;
        var transformList = from view in list select view.GetComponent<Transform>();
        cancellationTokenSource?.Cancel();
        cancellationTokenSource = new CancellationTokenSource();
        CodeAnimation.PlayApearSequencely(transformList.ToList(), cancellationTokenSource.Token);
    }

    public static void PlayElementAnimation(DataListCreator dataListCreator, ref CancellationTokenSource cancellationTokenSource)
    {
        var list = dataListCreator.viewList;
        var transformList = from view in list select view.GetComponent<Transform>();
        cancellationTokenSource?.Cancel();
        cancellationTokenSource = new CancellationTokenSource();
        CodeAnimation.PlayApearSequencely(transformList.ToList(), cancellationTokenSource.Token);
    }

    public static void PlayElementAnimation(ScrollView badScrollView, ref CancellationTokenSource cancellationTokenSource)
    {
        var list = BadScrollViewUtil.GetViewList<Transform>(badScrollView);
        cancellationTokenSource?.Cancel();
        cancellationTokenSource = new CancellationTokenSource();
        CodeAnimation.PlayApearSequencely(list, cancellationTokenSource.Token);
    }

    public static async void PlayApearSequencely(List<Transform> list, CancellationToken cancellationToken)
    {
        Debug.Log($"[AnimatorUtil] play apear on {list.Count} element(s)");
        foreach (var view in list)
        {
            //view.gameObject.SetActive(false);
            var c = view.Get2AddComponent<CanvasGroup>();
            c.alpha = 0f;
        }
        foreach (var view in list)
        {
            //view?.gameObject.SetActive(true);
            if(view == null)
            {
                continue;
            }
            var c = view.gameObject.GetOrAddComponent<CanvasGroup>();
            c.alpha = 1f;
            AnimatorUtil.SendEvent(view?.gameObject, AnimatorUtil.EventType.Apear);
            await Task.Delay(1);
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }
        }
    }

}
