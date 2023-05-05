using System.Collections.Generic;
using BattleEngine.Logic;

/// <summary>
/// 处理受击伤害
/// </summary>
public class CharCtrl_Hit
{
    public Creature _owner = null;
    private List<BehitData> _skillBehitData = new List<BehitData>();
    private int _skillBehitCount;

    public void Init(Creature owner)
    {
        _owner = owner;
    }

    public void Update(float param_deltaTime)
    {
        //UpdateSkillBehit();
    }

    public void AddSkillBehitData(BehitData data)
    {
        // _skillBehitCount++;
        // _skillBehitData.Insert(0, data);
    }

    private void UpdateSkillBehit()
    {
        // if (_owner == null)
        //     return;
        // for (int iter = _skillBehitData.Count - 1; iter >= 0; iter--)
        // {
        //     BehitData tmp_data = _skillBehitData[iter];
        //     if (tmp_data == null)
        //         return;
        //
        //     // 攻击时双方的距离
        //     float tmp_distance = Vector3.Distance(_skillBehitData[iter].atkPos, _owner.GetPosition());
        //
        //     //飞行时间
        //     //TODO 飞行速度暂时写死了，后面用技能的速度算
        //     float damageDelaySpeed = 8f;
        //     float clinetDelayTime = damageDelaySpeed == 0f ? 0f : tmp_distance / damageDelaySpeed;
        //     clinetDelayTime = 0f;
        //     BattleTimer.Instance.DelayCall(clinetDelayTime, HandleSkilDamage, _skillBehitData[iter]);
        //     _skillBehitData.RemoveAt(iter);
        // }
    }

    private void HandleSkilDamage(object[] param)
    {
        // if (_owner == null)
        //     return;
        // if (!HudManager.Instance.Visible)
        //     return;
        // _skillBehitCount--;
        // BehitData tmp_data = param[0] as BehitData;
        // var data = new TriggerShareDataSkill();
        // data.sceneObj = _owner;
        // data.hurtType = tmp_data.hurtType;
        // data.destinationPos = tmp_data.desPos;
        // data.hitData = tmp_data;
        // Group group = Group.Behit;
        // if (tmp_data.HitIndex == 2)
        // {
        //     group = Group.Behit2;
        // }
        // else if (tmp_data.HitIndex == 3)
        // {
        //     group = Group.Behit3;
        // }
        // TriggerController controller = this._owner.gameTriggerConfig.CreateController(data, tmp_data.skillRow.SkillID, group);
        // HandleBehitHPMP(tmp_data.defendId, tmp_data, controller != null);
        // if (tmp_data.bIsCrit)
        // {
        //     if (_owner.sceneObjectType == SceneObjectType.Monster)
        //     {
        //         AudioManager.PlaySeInBackground("js_aa1001_knockback");
        //     }
        //     else
        //     {
        //         AudioManager.PlaySeInBackground("gw_b1_zlh_knockback");
        //     }
        //     CameraManager.Instance.StartShake(0.1f, 0.3f);
        // }
        // if (tmp_data.isHurtAnim)
        // {
        //     _owner._cac.ChangeAction(CharacterActionConst.Hurt);
        // }
    }

    //处理血量
    private bool HandleBehitHPMP(string defendId, BehitData tmp_data, bool hasHurtTrigger)
    {
        // Creature tmp_creature = SceneObjectManager.Instance.Find(defendId, true);
        // if (tmp_creature != null)
        // {
        //     Creature tmp_caster_creature = SceneObjectManager.Instance.Find(tmp_data.casterId, true);
        //     bool isLeft = false;
        //     if (tmp_caster_creature != null)
        //     {
        //         isLeft = tmp_creature.GetPosition().x < tmp_caster_creature.GetPosition().x;
        //     }
        //     if (tmp_data.damage != 0)
        //         DamageManager.Instance.ShowDamagePanel(tmp_creature, tmp_data.damage, tmp_data.bIsCrit, isLeft, tmp_data);
        // }
        return false;
    }

    public int SkillBehitCount()
    {
        return _skillBehitCount;
    }

    public void Destroy()
    {
        _owner = null;
    }
}