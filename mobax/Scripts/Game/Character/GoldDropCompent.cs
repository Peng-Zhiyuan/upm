/* Created:Loki Date:2022-10-09*/

using UnityEngine;
using BattleEngine.Logic;
using Random = UnityEngine.Random;
using BattleEngine.View;

public class DropRuleData
{
    public float FxStepTime;
    public int FxStepCount;
    public int HpToGoldKey;
    public int HpToGoldValue;
}

public class GoldDropCompent : MonoBehaviour
{
    private Creature _ownCreature;
    public Creature OwnCreature
    {
        get { return _ownCreature; }
    }
    private DropRuleData _dropRuleData;
    private bool _isStealMonster = false;

    private float _currentReceiveDamage = 0.0f;
    private bool isCloseGoldFx = false;
    private int PlayFxNum = 0;

    private int getGoldNum = 0;
    public int GetGoldNum
    {
        get { return getGoldNum; }
    }

    public void AddGoldNum(int goldNum)
    {
        getGoldNum += goldNum;
    }

    public void InitData(Creature own, DropRuleData ruleData, bool isStealMonster = false)
    {
        _ownCreature = own;
        _dropRuleData = ruleData;
        if (!own.mData.isAtker)
        {
            _ownCreature.mData.UnListenActionPoint(ACTION_POINT_TYPE.PostReceiveDamage, ReceiveDamageAction);
            _ownCreature.mData.ListenActionPoint(ACTION_POINT_TYPE.PostReceiveDamage, ReceiveDamageAction);
        }
        _isStealMonster = isStealMonster;
        _currentReceiveDamage = 0;
        PlayFxNum = 0;
    }

    private void ReceiveDamageAction(ActionExecution combatAction)
    {
        var damageAction = combatAction as DamageAction;
        if (damageAction == null
            || damageAction.DamageValue <= 0
            || damageAction.Creator == null
            || damageAction.DamageSkillRow == null
            || damageAction.Creator.UID == _ownCreature.mData.UID)
        {
            return;
        }
        Creature targetCreature = BattleManager.Instance.ActorMgr.GetActor(damageAction.Creator.UID);
        if (targetCreature == null)
        {
            return;
        }
        if (_ownCreature.mData.CurrentHealth.Value <= 0 && _isStealMonster)
        {
            BattleGoldDropManager.Stuff.PlayDeadFx(_ownCreature, targetCreature);
            return;
        }
        _currentReceiveDamage += damageAction.DamageValue;
        if (_currentReceiveDamage < _dropRuleData.HpToGoldKey)
        {
            return;
        }
        int OffsetFx = Mathf.FloorToInt(_currentReceiveDamage * 1.0f / _dropRuleData.HpToGoldKey);
        for (int i = 0; i < OffsetFx; i++)
        {
            if (isCloseGoldFx)
            {
                BattleGoldDropManager.Stuff.ImmediateAddGold(targetCreature);
            }
            else
            {
                int random = Random.Range(0, 100);
                if (random < 50)
                {
                    BattleGoldDropManager.Stuff.PlayGoldFX(_ownCreature, targetCreature, BattleGoldDropManager.SingleGoldFXPath, true);
                }
                else
                {
                    BattleGoldDropManager.Stuff.PlayGoldFX(_ownCreature, targetCreature, BattleGoldDropManager.MoreGoldFXPath, false);
                }
                PlayFxNum += 1;
                if (PlayFxNum > _dropRuleData.FxStepCount)
                {
                    isCloseGoldFx = true;
                    TimerMgr.Instance.BattleSchedulerTimer(_dropRuleData.FxStepTime, () =>
                                    {
                                        PlayFxNum = 0;
                                        isCloseGoldFx = false;
                                    }
                    );
                }
            }
        }
        _currentReceiveDamage -= OffsetFx * _dropRuleData.HpToGoldKey;
        _currentReceiveDamage = Mathf.Max(0, _currentReceiveDamage);
    }
}