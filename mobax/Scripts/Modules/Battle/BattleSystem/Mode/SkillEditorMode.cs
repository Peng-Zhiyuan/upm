// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using System.Threading.Tasks;
// using BattleSystem.ProjectCore;
//
// [BattleModeClass(BattleModeType.SkillEditor)]
// public class SkillEditorMode : Mode
// {
//     public override string GetSceneName()
//     {
//         return "SkillMaker";
//     }
//
//     public override string GetPageName()
//     {
//         return "SkillEditorPage";
//     }
//
//     public override Task OnSceneLoaded()
//     {
//         BattleStateManager.Instance.GameResult = eBattleResult.None;
//         CameraManager.Instance.TryChangeState(CameraState.Ready);
//         var tcs = new TaskCompletionSource<bool>();
//         tcs.SetResult(true);
//         return tcs.Task;
//     }
// }