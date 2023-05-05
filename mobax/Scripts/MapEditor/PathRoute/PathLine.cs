using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PathLine 
{
    public List<PathLinePoint> Positions = new List<PathLinePoint>();
    public bool IsDoubleWay = false;
}
[InlineProperty]
[Serializable]
public class PathLinePoint
{
    [HideInInspector]
    public int Index;
    [LabelText ("@\"P\"+Index")]
    public Vector3 Point;
    [HideInInspector]
    public List<int> LinkedLineIndies = new List<int> ();
}
