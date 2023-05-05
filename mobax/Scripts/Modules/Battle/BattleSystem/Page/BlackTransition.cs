using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class BlackTransition : MonoBehaviour
{
       public Image Up;
       public Image Down;

       public float BeginY;
       public float OffsetY;
       public float DurTime;
       public float WaitTime = 0.5f;
       public float FadeInTime = 0.2f;

       public void Awake()
       {
              Up.gameObject.SetActive(false);
              Down.gameObject.SetActive(false);
       }

       public void ShowEffect(Action action)
       {
              Up.gameObject.SetActive(true);
              Down.gameObject.SetActive(true);
              Up.transform.localPosition = new Vector3(0, BeginY, 0);
              Down.transform.localPosition = new Vector3(0, -BeginY, 0);
              var color = Up.color;
              color.a = 0;
              Up.color = color;
              Down.color = color;
              
              var seq = DOTween.Sequence();
              seq.Append(Up.DOFade(1, FadeInTime));
              seq.Insert(0, Down.DOFade(1, FadeInTime));
              seq.AppendInterval(WaitTime);
              seq.Append(Up.transform.DOLocalMoveY(OffsetY, DurTime));
              seq.Insert(WaitTime + FadeInTime, Down.transform.DOLocalMoveY(-OffsetY, DurTime));
              seq.onComplete = delegate
              {
                     if (action != null)
                     {
                            action.Invoke();
                     }
              };
       }
}