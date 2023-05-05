using System;
using UnityEngine;
using UnityEngine.UI;


// [RequireComponent(typeof(RectTransform))]
public class CountdownBehaviour : MonoBehaviour
{
    public Text countdownTxt;
    public bool fromOne;
        
    private DateTime _endTime;
    private bool _running;
    private string _updateFormat;
    private string _endString;
    private string _zeroString;

    public void Stop()
    {
        _running = false;
    }

    public void SetEnd(int sec, string updateFormat = @"{0:mm\:ss}", string endString = null, string zeroString = null)
    {
        SetEnd(Clock.Now.AddSeconds(sec), updateFormat, endString, zeroString);
    }
    
    public void SetEnd(DateTime dt, string updateFormat = @"{0:mm\:ss}", string endString = null, string zeroString = null)
    {
        // 设置一个默认值
        countdownTxt ??= GetComponent<Text>();

        _running = true;
        _endTime = dt;
        _updateFormat = updateFormat;
        _endString = endString;
        _zeroString = zeroString;
    }

    public void SetText(string text)
    {
        _running = false;
        countdownTxt.text = text;
    }
        
    private void Update()
    {
        if (!_running) return;

        var timeSpan = _endTime - Clock.Now;
        var leftSec = timeSpan.TotalSeconds;
        string countdownStr = null;
        if (leftSec < 0)
        {
            timeSpan = TimeSpan.Zero;
            _running = false;
            countdownStr = _endString;
        }
        else if (leftSec < 1)
        {
            countdownStr = _zeroString;
        }

        if (fromOne && timeSpan > TimeSpan.Zero)
        {
            timeSpan += new TimeSpan(0, 0, 1);
        }
        countdownTxt.text = countdownStr ?? string.Format(_updateFormat, timeSpan);
    }
}