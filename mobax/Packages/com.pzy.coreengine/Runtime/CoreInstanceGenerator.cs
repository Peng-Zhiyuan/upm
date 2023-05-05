using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CoreInstanceGenerator 
{
    static int nextGenId = 1;
    public static int GenerateId()
    {
        var ret = nextGenId;
        nextGenId++;
        return ret;
    }
}
