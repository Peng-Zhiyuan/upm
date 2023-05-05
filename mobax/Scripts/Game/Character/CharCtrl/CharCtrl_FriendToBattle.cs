/* Created:Loki Date:2022-09-28*/

using System.Threading.Tasks;
using UnityEngine;
using BattleEngine.Logic;
using BattleEngine.View;

public class CharCtrl_FriendToBattle
{
    private Creature _owner = null;
    private bool isBattle = false;
    public bool IsBattle
    {
        get { return isBattle; }
        set { isBattle = value; }
    }

    public void Init(Creature owner)
    {
        _owner = owner;
        isBattle = false;
    }

    public async void FriendToBattle(Vector3 targetPos)
    {
        if (isBattle || _owner.mData.PosIndex != BattleConst.FriendPosIndex)
        {
            return;
        }
        isBattle = true;
        _owner.SelfTrans.position = targetPos;
        _owner.SetDirection(_owner.mData.BornRot);
        _owner.RoleRender.SetFresnel(true, ColorUtil.HexToColor("FFD18A"));
        CameraManager.Instance.TryChangeState(CameraState.Exchange, _owner);
        CameraManager.Instance.TurnImmediate();
        await Task.Delay(300);
        _owner.SetActive(true);
        GameObject showFx = await BattleResManager.Instance.CreatorFx("fx_refresh_yellow.prefab");
        if (showFx != null)
        {
            TransformUtil.InitTransformInfo(showFx, null);
            showFx.transform.position = _owner.mData.BornPos;
            ParticleSystemPlayCtr showFxCtr = showFx.GetComponent<ParticleSystemPlayCtr>();
            showFxCtr.Play();    
        }
        TimerMgr.Instance.BattleSchedulerTimer(1.0f, delegate
                        {
                            _owner.RoleRender.SetFresnel(false, Color.yellow);
                            TimerMgr.Instance.BattleSchedulerTimer(0.01f, delegate { _owner.ShowSelectEffect(_owner.IsShowSelected); });
                        }
        );
        GameEventCenter.Broadcast(GameEvent.FriendToBattle);
    }

    public async void FriendQuitBattle()
    {
        if (!isBattle)
        {
            return;
        }
        BattlePage battlePage = UIEngine.Stuff.FindPage("BattlePage") as BattlePage;
        if (battlePage != null)
        {
            battlePage.CloseFriendTime();
        }
        GameObject showFx = await BattleResManager.Instance.CreatorFx("fx_refresh_yellow.prefab");
        if (showFx != null)
        {
            TransformUtil.InitTransformInfo(showFx, null);
            showFx.transform.position = _owner.SelfPositionXZ;
            ParticleSystemPlayCtr showFxCtr = showFx.GetComponent<ParticleSystemPlayCtr>();
            showFxCtr.Play();    
        }
        await Task.Delay(200);
        _owner.SetActive(false);
        isBattle = false;
    }
}