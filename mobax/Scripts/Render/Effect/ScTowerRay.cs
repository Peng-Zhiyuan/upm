using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// float0 激光半径
/// float1 扫射速度 1s/x角度  ，360就代表1s转一圈
/// </summary>
public class ScTowerRay : ScEffectInit
{
    public override void SetFloatArgs(float arg, int index = 0)
    {
        switch (index)
        {
            default: 
            case 0:
                rangeArg = arg;
                break;
            case 1:
                speedArg = arg;
                break;
        }
    }

    [Button]
    public override void Play()
    {
        StartCoroutine(PlayEffect());
    }

    IEnumerator PlayEffect()
    {
        rotateTime = 360f / speedArg;
        line.SetPosition(1,new Vector3(rangeArg*(1.0f/targetOffset),0f,0f));
        root.SetActive(true);
        var tick = 0f;
        
        while (tick<rotateTime)
        {
            var offset = tick / rotateTime;
            transform.rotation=Quaternion.Euler(0f,offset*360f,0f);
            tick += Time.deltaTime;
            yield return null;
        }
        yield return new WaitForSeconds(delayShowTime);
        root.SetActive(false);
    }

    // public void OnGUI()
    // {
    //     if (GUI.Button(new Rect(0, Screen.width - 100, 100, 30), "Play"))
    //     {
    //         Play();
    //     }
    // }
    public float targetOffset = 1f;
    public float rangeArg;
    public float speedArg;
    float rotateTime;
    public float delayShowTime;
    public LineRenderer line;
}