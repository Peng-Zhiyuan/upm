using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;
using UnityEngine.UI;

public partial class CommandView : MonoBehaviour
{
    MethodInfo methodInfo;
    public void Bind(MethodInfo methodInfo)
    {
        this.methodInfo = methodInfo;
        this.Refresh();
    }

    int ParamCount
    {
        get
        {
            var count = this.methodInfo.GetParameters().Length;
            return count;
        }
    }

    ParameterInfo ParamInfo1
    {
        get
        {
            var paramList = this.methodInfo.GetParameters();
            if(paramList.Length < 1)
            {
                return null;
            }
            var ret = paramList[0];
            var type = ret.ParameterType;
            return ret;
        }
    }

    ParameterInfo ParamInfo2
    {
        get
        {
            var paramList = this.methodInfo.GetParameters();
            if (paramList.Length < 2)
            {
                return null;
            }
            var ret = paramList[1];
            var type = ret.ParameterType;
            return ret;
        }
    }

    void Refresh()
    {
        this.RefreshButton();
        this.RefreshArg1();
        this.RefreshArg2();
    }

    void RefreshArg1()
    {
        var info = ParamInfo1;
        if(info != null)
        {
            var paramName = info.Name;
            var defaultText = this.Input_arg1.placeholder.GetComponent<Text>();
            defaultText.text = paramName;
            this.Input_arg1.gameObject.SetActive(true);
        }
        else
        {
            this.Input_arg1.gameObject.SetActive(false);
        }
    }
    void RefreshArg2()
    {
        var info = ParamInfo2;
        if (info != null)
        {
            var paramName = info.Name;
            var defaultText = this.Input_arg2.placeholder.GetComponent<Text>();
            defaultText.text = paramName;
            this.Input_arg2.gameObject.SetActive(true);
        }
        else
        {
            this.Input_arg2.gameObject.SetActive(false);
        }
    }

    static object Parse(string str, Type toType)
    {
        if(toType == typeof(string))
        {
            return str;
        }
        else if(toType == typeof(int))
        {
            return int.Parse(str);
        }
        else if (toType == typeof(long))
        {
            return long.Parse(str);
        }
        else if (toType == typeof(bool))
        {
            return bool.Parse(str);
        }
        throw new Exception("unexcept type: " + toType.Name);
    }

    void RefreshButton()
    {
        var methodName = methodInfo.Name;
        var text = this.Button.GetComponentInChildren<Text>();
        text.text = methodName;
    }

    public void OnButton(string msg)
    {
        if(msg == "click")
        {
            var paramList = new List<object>();
            var p1 = this.ParamInfo1;
            if(p1 != null)
            {
                var input = Input_arg1.text;
                input = input.Trim();
                var paramType = ParamInfo1.ParameterType;
                var obj = Parse(input, paramType);
                paramList.Add(obj);
            }

            var p2 = this.ParamInfo2;
            if (p2 != null)
            {
                var input = Input_arg2.text;
                input = input.Trim();
                //paramList.Add(input);
                var paramType = ParamInfo2.ParameterType;
                var obj = Parse(input, paramType);
                paramList.Add(obj);
            }

            var paramArray = paramList.ToArray();
            this.methodInfo.Invoke(null, paramArray);
        }
    }
}
