using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ObservableValue<T> 
{
    T _value;

    public T Value
    {
        get
        {
            return _value;
        }
        set
        {
            if(_value.Equals(value))
            {
                return;
            }
            _value = value;
            this.Changed?.Invoke();
        }
    }

    public event Action Changed;
}
