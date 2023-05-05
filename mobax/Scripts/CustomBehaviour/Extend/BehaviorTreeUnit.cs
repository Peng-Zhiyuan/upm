using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime;
using System.Threading.Tasks;
using BattleSystem.Core;
public class BehaviorTreeUnit : CoreComponent
{
    public Behavior behaviorTree;
    public void Init()
    {
		behaviorTree.StartWhenEnabled = true;
		behaviorTree.RestartWhenComplete = true;
		behaviorTree.PauseWhenDisabled = true;
		behaviorTree.ResetValuesOnRestart = false;
#if UNITY_EDITOR || DLL_DEBUG || DLL_RELEASE
	    behaviorTree.showBehaviorDesignerGizmo = false;
#endif
    }
}
