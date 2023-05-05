using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using BattleSystem.ProjectCore;

public class BattleModeClassAttribute : Attribute
{
    public BattleModeType mode;
    public BattleModeClassAttribute(BattleModeType mode)
    {
        this.mode = mode;
    }
}
