using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;
public class MarkupAdapter
{
    protected static StringBuilder StringBuilder = new StringBuilder();

    public string LeftMark { get; }
    public string RightMark { get; }

    public MarkupAdapter(string leftMark, string rightMark)
    {
        LeftMark = leftMark;
        RightMark = rightMark;
    }

    public string ToString(string str)
    {
        StringBuilder.Remove(0, StringBuilder.Length);
        StringBuilder.Append(LeftMark);
        StringBuilder.Append(str);
        StringBuilder.Append(RightMark);
        return StringBuilder.ToString();
    }
}