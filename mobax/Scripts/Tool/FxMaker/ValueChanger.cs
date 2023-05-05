using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ValueChanger : MonoBehaviour
{
    public InputField mInput;


    public void SetValue(string _v)
    {

        mInput.text = _v;
    }

    public string GetValue()
    {
        return mInput.text;
    }

    public string ToStr()
    {
        return mInput.text;
    }
    public void InitWithStr(string _str)
    {
        mInput.text = _str;
    }

}
