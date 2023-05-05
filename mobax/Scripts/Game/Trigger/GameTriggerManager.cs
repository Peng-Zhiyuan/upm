using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameTriggerManager : Single<GameTriggerManager> {
    private Dictionary<ulong, TriggerController> _allContorller = new Dictionary<ulong, TriggerController>();
    private List<TriggerController> _removeContorller = new List<TriggerController>();
    private List<TriggerController> _tempContorllers = new List<TriggerController>();

    public void OnCoreCreated() {
       // var triggerFactor = Battle.Instance.battleCore.TriggerFactory;
        //triggerFactor.RegisterAllTriggers();
    }


    public ulong PlayTrigger(TriggerController contorller, TriggerShareData data, System.Action callback = null) {
        _tempContorllers.Add(contorller);
        contorller.SetEndCallback(callback);
        DoTrigger(contorller, data);
        return contorller.GetUID;
    }

    private void DoTrigger(TriggerController contorller, TriggerShareData data) {
        contorller.ShareData = data;
        //if (GameStateManager.Instance.GetCurrentState() == eGameState.PlayingState)

        if (BattleStateManager.Instance.GetCurrentState() == eBattleState.Play) {
            contorller.Play();
        }
        else {
            contorller.Stop();
        }
    }

    public void StopTrigger(TriggerController triggerController) {
        triggerController.Stop();
    }

    public void CleanAllTrigger() {
        foreach (var data in _allContorller) {
            TriggerController ctrl = data.Value;
            ctrl.EndCallback();
        }

        _allContorller.Clear();
    }

    public void FixedUpdate() {
        float param_deltaTime = Time.fixedDeltaTime;
        for (int i = _tempContorllers.Count - 1; i >= 0; i--) {
            TriggerController tempContorller = _tempContorllers[i];
            _allContorller.Add(tempContorller.GetUID, tempContorller);
            _tempContorllers.RemoveAt(i);
        }

        foreach (var data in _allContorller) {
            TriggerController ctrl = data.Value;
            if (ctrl.IsEnd) {
                ctrl.EndCallback();
                _removeContorller.Add(ctrl);
            }
            else
                ctrl.FixedUpdate(param_deltaTime);
        }

        foreach (TriggerController ctrl in _removeContorller) {
            _allContorller.Remove(ctrl.GetUID);
            ctrl.Destroy();
        }

        _removeContorller.Clear();
    }
    
}