using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System;

public abstract class TestConfig
{
    public async virtual Task OnPreOneCaseStart(TestCase testCase)
    {
        
    }

    public async virtual Task OnAfterOneCaseCompelte(TestCase testCase, bool isSuccess, Exception e)
    {

    }

}
