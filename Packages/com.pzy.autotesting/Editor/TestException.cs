using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class TestException : Exception
{
    public TestException(string msg) : base(msg)
    {

    }
}
