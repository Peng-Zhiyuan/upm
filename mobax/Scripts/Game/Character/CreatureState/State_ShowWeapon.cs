public class State_ShowWeapon : CreatureState
{
    public override int GetStateID()
    {
        return (int)State.STATE_SHOWWEAPON;
    }

    public override bool Condition(BaseState curState)
    {
        return active;
    }

    public override void OnEnter(object param_UserData)
    {
        base.OnEnter(param_UserData);
        active = true;
        if (!_owner.IsHaveAnimationClip(CharacterActionConst.ShowWeapon))
            return;
        float t = _owner.GetAnimClipTime(CharacterActionConst.ShowWeapon);
        BattleTimer.Instance.DelayCall(t, delegate(object[] objects) { active = false; });
        _owner.ShowWeapon();
        _owner.WeaponState = true;
        _owner._cac.ChangeAction(CharacterActionConst.ShowWeapon);
    }
}