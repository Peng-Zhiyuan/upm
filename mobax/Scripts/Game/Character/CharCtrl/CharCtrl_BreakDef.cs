using UnityEngine;
using System.Threading.Tasks;
using BattleEngine.Logic;

/// <summary>
/// 破防表现
/// </summary>
public class CharCtrl_BreakDef
{
    private Creature _owner = null;
    private bool isUpdate = false;
    private float DurTime = 2f;

    public void Init(Creature owner)
    {
        _owner = owner;
        DurTime = 0.0f;
        isTurning = false;
        
        _owner.mData.Subscribe<BreakDefEvent>(BreakDefEventHandler);
        _owner.mData.Subscribe<BreakStateEnd>(BreakDefEndHandler);
        _owner.mData.Subscribe<BreakDefQteEvent>(BreakDefQteEvent);
    }

    private bool isTurning = false;
    public bool IsTurning
    {
        get { return isTurning; }
        set { isTurning = value; }
    }
    
    private async void BreakDefEventHandler(BreakDefEvent data)
    {
        //if(GuideManager.Stuff.IsGuideProcessing())
        //    TriggerEngine.Stuff.Notify("GUIDE_STRENGTH_BROKEN");
        GuideManagerV2.Stuff.Notify("Battle.StrengthBroken");
        _owner.ShowVimEffect();
    }

    private void BreakDefQteEvent(BreakDefQteEvent data)
    {
        _owner.ShowVimEffect();

        
    }

    private void BreakDefEndHandler(BreakStateEnd data)
    {
        _owner.SetAnimSpeed(1);
    }

    public void Destroy()
    {
        _owner.mData.UnSubscribe<BreakDefEvent>(BreakDefEventHandler);
        _owner.mData.UnSubscribe<BreakStateEnd>(BreakDefEndHandler);
        _owner.mData.UnSubscribe<BreakDefQteEvent>(BreakDefQteEvent);
    }
}