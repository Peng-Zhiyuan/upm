using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Floating : View
{
    public virtual void OnShow(object param){}

    public virtual void OnHide(){}

    public void Hide()
    {
        UIEngine.Stuff.RemoveFloating(this.name);
    }
    
}