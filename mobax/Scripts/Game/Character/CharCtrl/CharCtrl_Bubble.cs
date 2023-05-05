using UnityEngine;
using Object = UnityEngine.Object;
using BattleSystem.ProjectCore;
using EtuUnity;

public enum EmojiEvent
{
    None,
    EnterLevel = 1,     //进入关卡
    CastSkill = 2,      //释放技能
    FriendDie = 3,      //队友死亡
    Kill = 4,           //击杀敌人
    JoinTeam = 5,       //加入队伍
    LeaveTeam = 6,      //替补下场
    CatEnter = 7,      //出现猫
    UseItem = 8,      //使用道具
}
public class CharCtrl_Bubble
{
    public Creature _owner = null;
    
    public void Init(Creature owner)
    {
        _owner = owner;
        
        owner.objectEvent.AddListener(GameEvent.ShowHeadBubble, this, ShowHeadBubble);
    }

    public void Update(float param_deltaTime)
    {
    }

    private int EffectID = 0;
    private void ShowHeadBubble(object[] param)
    {
        /*Creature role = param[0] as Creature;
        bool isSelf = (bool)param[1];
        if (isSelf)
        {
            if(role.ID != _owner.ID)
                return;
        }
        else
        {
            if(role.ID == _owner.ID)
                return;
        }*/
        
        if (EffectID != 0)
        {
            EffectManager.Instance.RemoveEffect(EffectID);
            EffectID = 0;
        }
        float durT = 3f;
        Vector3 scale = Vector3.one;
        Vector3 pos = new Vector3(0.05f, 0.2f, 0);
        if (IsLeft())
        {
            scale.x = -1f;
            pos.x = -pos.x;
        }
        EffectID = EffectManager.Instance.CreateBodyEffect("fx_wave_1", _owner.HeadBone, durT, pos, scale,
            Vector3.zero);
        
        TimerMgr.Instance.BattleSchedulerTimer(durT, () =>
        {
            EffectManager.Instance.RemoveEffect(EffectID);
            EffectID = 0;
        });
    }

    public void Destroy()
    {
        _owner.objectEvent.RemoveListener(GameEvent.ShowHeadBubble, this, ShowHeadBubble);
    }

    public bool IsLeft()
    {
        var dir = _owner.transform.position - CameraManager.Instance.MainCamera.transform.position;
        return Vector3.Cross(CameraManager.Instance.MainCamera.transform.forward, dir).y < 0 ;
    }
    
    public void Trigger(EmojiEvent evt)
    {
        //Debug.LogError("--configid = " + (_owner.ConfigID * 100 + (int) evt));
        if(_owner == null)
            return;
        
        if(_owner.mData == null || _owner.mData.battleItemInfo == null)
            return;
        
        var data = StaticData.RolechatTable.TryGet(_owner.ConfigID * 100 + (int) evt);
        if(data == null)
            return;
        
        PlayEmoji(data);

        PlayTalkShow(data);
        
        PlayMood(data);
    }

    private void PlayTalkShow(RolechatRow info)
    {
        if(info.textIds.Count == 0)
            return;
        
        var message =
            LanguageData.MsgList10Table.TryGet(info.textIds[UnityEngine.Random.Range(0, info.textIds.Count)]);
        if(message == null)
            return;

        //Debug.LogError(info.Id + "------" + message);
        GameEventCenter.Broadcast(GameEvent.TalkShow, message, info.roleId);
    }
    
    private void PlayMood(RolechatRow info)
    {
        if(string.IsNullOrEmpty(info.Mood))
            return;

        //Debug.LogError(info.Id + "------" + message);
        GameEventCenter.Broadcast(GameEvent.PlayMood, info.Mood, info.roleId);
    }

    private void PlayEmoji(RolechatRow data)
    {
        if(_owner.mData.IsDead)
            return;
        
        if(UnityEngine.Random.Range(1, 10000) > data.Rate)
            return;
        
        if (EffectID != 0)
        {
            EffectManager.Instance.RemoveEffect(EffectID);
            EffectID = 0;
        }
        float durT = 3f;
        Vector3 scale = Vector3.one;
        Vector3 pos = new Vector3(-0.05f, -0.5f, 0);

        bool isleft = IsLeft();
        if (!isleft)
        {
            if (data.Emoji.Equals("fx_emoji_9_1"))
            {
                data.Emoji = "fx_emoji_9_1_r";
                isleft = true;
            }
            /*else
            {
                scale.x = -1f;
                pos.x = -pos.x;
            }*/
        }
        
        HudManager.Instance.ShowEmoji(_owner.ID, data.Emoji, isleft);
        /*EffectID = EffectManager.Instance.CreateBodyEffect(data.Emoji, _owner.HeadBone, durT, pos, scale,
            Vector3.zero);
        
        TimerMgr.Instance.BattleSchedulerTimer(durT, () =>
        {
            EffectManager.Instance.RemoveEffect(EffectID);
            EffectID = 0;
        });*/
        
        
    }
}