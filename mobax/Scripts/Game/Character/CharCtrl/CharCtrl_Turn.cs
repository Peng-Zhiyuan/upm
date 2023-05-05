using UnityEngine;
using System.Threading.Tasks;
using BattleEngine.Logic;

/// <summary>
/// 战斗角色上下场
/// </summary>
public class CharCtrl_Turn
{
    private Creature _owner = null;
    private bool isUpdate = false;
    private float DurTime = 2f;

    public void Init(Creature owner)
    {
        _owner = owner;
        DurTime = 0.0f;
        isTurning = false;
    }

    private bool isTurning = false;
    public bool IsTurning
    {
        get { return isTurning; }
        set { isTurning = value; }
    }

    /// <summary>
    /// 替补上场
    /// </summary>
    public async void TurnOn(Vector3 initPos, Vector3 dir, Creature leaveCreature)
    {
        // _owner.Trigger(EmojiEvent.JoinTeam);
        // CameraManager.Instance.TryChangeState(CameraState.Exchange, _owner);
        // _owner.gameObject.SetActive(true);
        // _owner.RoleRender.SetDissolve(0f);
        // _owner.SelfTrans.localPosition = initPos;
        // _owner.SelfTrans.localRotation = Quaternion.Euler(dir);
        // _owner.ShowWeapon();
        // Vector3 targetPos = leaveCreature.mData.GetPosition();
        // await Task.Delay(800);
        // BattleLog.LogWarning("Execute SendHeroTurnOn");
        // BattleManager.Instance.SendHeroTurnOn(leaveCreature.mData.UID, _owner.mData.UID, initPos);
        // BattleManager.Instance.SendSpendTurnOnSkill(_owner.mData.UID, targetPos);
        // isTurning = false;
    }

    /// <summary>
    /// 替补下场
    /// </summary>
    public async void TurnOff(Vector3 targetPos)
    {
        // BattleManager.Instance.SendHeroTurnOff(_owner.mData.UID);
        // _owner.mData.SetLifeState(ACTOR_LIFE_STATE.Substitut);
        // _owner.Trigger(EmojiEvent.LeaveTeam);
        // float t = _owner.MoveTo(targetPos);
        // await Task.Delay((int)(t * 1000));
        // if (_owner != null)
        // {
        //     _owner.gameObject.SetActive(false);
        // }
    }
}