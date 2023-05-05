using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class BarrageMsgUnit : RecycledGameObject
{
    public Text barrageInfo;
    public Outline outLine;
    public float move_speed;
    //private float start_y = 600;
    private float end_x = -10000;
    public void SetInfo(string msg, int height, float speed, Color c, Color outLineColor)
    {
        barrageInfo.text = msg;
        float width = UIEngine.Stuff.Canvas.GetComponent<CanvasScaler>().referenceResolution.x;
        Debug.LogError("width:"+ width);
        var start_x = (width + barrageInfo.preferredWidth) / 2;
        Debug.LogError("start_x:" + start_x);
        end_x = -(width + barrageInfo.preferredWidth) / 2;
        var start_y = 550 + height * 50;
        this.transform.localPosition = new Vector3(start_x, start_y, 0);
        move_speed = speed * 100;
        barrageInfo.color = c;
        outLine.effectColor = outLineColor;
    }

    public void UpdatePos()
    {
        this.transform.localPosition -= Vector3.right * move_speed * Time.deltaTime;
    }

    public bool IsFinished
    {
        get 
        {
            return this.transform.localPosition.x < end_x;
        }
    }
}
