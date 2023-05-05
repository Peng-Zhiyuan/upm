using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Binding<T> where T : unmanaged
{
    Property<T> dist;
    ObservableValue<T> source;
    bool isBinded;

    public Binding(Property<T> dist, ObservableValue<T> source, bool autoBind = true)
    {
        this.dist = dist;
        this.source = source;
        if(autoBind)
        {
            this.Bind();
        }
    }

    public void Bind()
    {
        if(this.isBinded)
        {
            return;
        }
        dist.Value = source.Value;
        source.Changed += OnValueChanged;
        this.isBinded = true;
    }

    void OnValueChanged()
    {
        var value = this.source.Value;
        dist.Value = value;
    }

    public void Unbind()
    {
        if(!this.isBinded)
        {
            return;
        }
        this.source.Changed -= OnValueChanged;
        isBinded = false;
    }

}
