using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using DG.Tweening;
using UnityEngine.UI;

public class ListViewElementWrapper : MonoBehaviour
{
    public Transform tempRoot;

    [ShowInInspector, ReadOnly]
    Transform recordedChild;
    public void RecordView()
    {
        var childCount = this.gameObject.transform.childCount;
        if(childCount == 1)
        {
            recordedChild = this.gameObject.transform.GetChild(0);
        }
        else
        {
            recordedChild = null;
        }
    }

    public void MoveToTemp()
    {
        if(recordedChild != null)
        {
            recordedChild.SetParent(this.tempRoot, true);
        }
    }

    public void MoveBack()
    {
        if (recordedChild != null)
        {
            recordedChild.SetParent(this.transform, true);
        }
    }

    public async void TweenChildToSelf()
    {
        if (recordedChild != null)
        {
            var layout = this.GetComponent<LayoutGroup>();
            layout.enabled = false;
            var tween = recordedChild.DOLocalMove(Vector3.zero, 0.2f);
            await tween.AsyncWaitForCompletion();
            layout.enabled = true;
        }
    }
}
