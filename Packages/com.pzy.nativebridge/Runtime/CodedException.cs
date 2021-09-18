using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CodedException : Exception
{
    public string code;

    public CodedException(string code, string msg):  base($"{code}:{msg}")
    {
        this.code = code;
    }

}
