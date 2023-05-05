using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class TransitionEffectController : MonoBehaviour
{
    public float DelayTime = 0f;

    public float DurTime = 0.8f;

    public float BeginY = -5f;

    public float EndY = 5f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ShowEffect()
    {
        Vector3 pos = this.transform.localPosition;
        pos.y = BeginY;
        this.transform.localPosition = pos;
        this.transform.DOLocalMoveY(EndY, DurTime);
    }

    /*private void OnGUI()
    {
        GUI.skin.button.fontSize = 30;
        if (GUI.Button(new Rect(40, 40, 200, 100), "开始移动！"))
        {
            ShowEffect();
        }
    }*/
}
