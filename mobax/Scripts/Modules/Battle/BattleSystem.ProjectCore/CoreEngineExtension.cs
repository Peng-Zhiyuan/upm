using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BattleSystem.Core;

public static class CoreEngineExtension 
{
    public static BattleCore GetBattleCore(this CoreEngine coreEngine)
    {
        return coreEngine.owner as BattleCore;
    }
}
