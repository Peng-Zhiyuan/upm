using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class MessageHandlerAttribute : Attribute
{
    public string path = null;

    public MessageHandlerAttribute(string path = null)
    {
        this.path = path;
    }
}
