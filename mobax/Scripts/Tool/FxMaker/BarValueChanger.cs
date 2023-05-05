using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BarValueChanger : MonoBehaviour
{
    public InputField mInput;
    public Slider mBar;
    float mValue;

    public void Init(float _min,float _max)
    {
        mBar.minValue = _min;
        mBar.maxValue = _max;
        mBar.value = 0;
        mValue = 0;
    }

    public void SetValue(float _v)
    {
        mValue = _v;
        mInput.text = mValue.ToString();
        mBar.value = mValue;
    }

    public float GetValue()
    {
        return mValue;
    }


    public void OnBarChange()
    {
        mValue = mBar.value;
        mInput.text = mValue.ToString("0.0");
    }

    public void OnInputChange()
    {
        float _v = mValue;
        if(float.TryParse(mInput.text,out _v))
        {
            mValue = _v;
            mBar.value = mValue;
        }
    }

    public string ToStr()
    {
        return mValue.ToString("0.0");
    }
    public void InitWithStr(string _str)
    {
        mValue = float.Parse(_str);
        mInput.text = mValue.ToString("0.0");
        mBar.value = mValue;
    }
}
