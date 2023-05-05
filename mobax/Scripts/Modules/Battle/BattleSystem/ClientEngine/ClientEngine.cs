using BattleSystem.Core;
using BattleSystem.ProjectCore;
using Game.Skill;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BattleEngine.Logic;
using UnityEngine;

public class ClientEngine : BattleComponent<ClientEngine>
{
    public override void OnUpdate()
    {
#if UNITY_EDITOR
        if (Input.GetKeyUp(KeyCode.P))
        {
            BattleManager.Instance.KillAllEnemy();
        }
        if (Input.GetKeyUp(KeyCode.M))
        {
            BattleManager.Instance.KillOneHero();
        }
        if (Input.GetKeyUp(KeyCode.L))
        {
            foreach (var VARIABLE in BattleManager.Instance.ActorMgr.GetAllActors())
            {
                if (VARIABLE.mData.CurrentHealth.Value <= 0)
                {
                    continue;
                }
                VARIABLE.mData.CurrentHealth.SetMaxValue(99999);
                VARIABLE.mData.CurrentHealth.Reset();
            }
        }

        if (Input.GetKeyUp(KeyCode.Y))
        {
            Formula.ShowLog = !Formula.ShowLog;
        }
#endif
    }
}