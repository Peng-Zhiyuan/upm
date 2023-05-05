/* Created:Loki Date:2023-01-13*/

using System.Threading.Tasks;
using UnityEngine;
using BattleEngine.Logic;
using BattleEngine.View;
using BattleSystem.ProjectCore;

public class CharCtrl_LinkerToBattle
{
    private Creature _owner = null;
    private string CreatureUID = "";
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

    /// <summary>
    /// 发大招触发链接者发动出厂技能
    /// </summary>
    /// <param name="SSPSKillUID">发大招英雄UID</param>
    public async void LinkerToBattle(string SSPSKillUID)
    {
        if (isBattle || _owner.mData.PosIndex != BattleConst.SSPAssistPosIndexStart)
        {
            return;
        }
        isBattle = true;
        CreatureUID = SSPSKillUID;
        _owner.SetPosition(_owner.mData.GetPosition());
        _owner.SetLocalRotation(_owner.mData.GetEulerAngles().y);
        _owner.RoleRender.SetFresnel(true, ColorUtil.HexToColor("FFD18A"));
        GameEventCenter.Broadcast(GameEvent.ShowTargetCamera, _owner, 180);
        await Task.Delay(300);
        _owner.SetActive(true);
        GameObject showFx = await BattleResManager.Instance.CreatorFx("fx_refresh_yellow.prefab");
        if (showFx != null)
        {
            TransformUtil.InitTransformInfo(showFx, null);
            showFx.transform.position = _owner.SelfPositionXZ;
            ParticleSystemPlayCtr showFxCtr = showFx.GetComponent<ParticleSystemPlayCtr>();
            showFxCtr.Play();    
        }
        await Task.Delay(1000);
        _owner.RoleRender.SetFresnel(false, Color.yellow);
        TimerMgr.Instance.BattleSchedulerTimer(0.01f, delegate { _owner.ShowSelectEffect(_owner.IsShowSelected); });
        GameEventCenter.Broadcast(GameEvent.CloseTargetCamera, _owner);
    }

    public async void LinkerQuitBattle()
    {
        if (!isBattle)
        {
            return;
        }
        Debug.LogWarning("Linker Quit Battle " + _owner.mData.ConfigID);
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