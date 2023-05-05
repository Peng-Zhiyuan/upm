namespace BattleEngine.Logic
{
    using UnityEngine;

    public class BehitData
    {
        public string casterId;
        public string defendId;
        public int damage;

        public Vector3 atkPos;
        public Vector3 desPos;
        public bool bIsDie = false;
        public SkillRow skillRow;
        public bool bIsCrit = false;
        public int HitIndex = 0;
        public bool isHurtAnim;
        public HitType hitType;
        public float block; //格挡值

        ////////////////客户端数据/////////////////
        public float time;

        public void SetBehitData(string param_casterId, string param_defendId, int param_damage, Vector3 param_atkPos, Vector3 param_desPos, SkillRow param_skillRow, bool param_bIsCrit)
        {
            time = BattleLogicManager.Instance.CurrentFrame * BattleLogicDefine.LogicSecTime;
            casterId = param_casterId;
            defendId = param_defendId;
            damage = param_damage;
            atkPos = param_atkPos;
            desPos = param_desPos;
            skillRow = param_skillRow;
            bIsCrit = param_bIsCrit;
        }

        public bool HasState(HitType state)
        {
            return FlagsHelper.IsSet(hitType, state);
        }

        public void SetState(HitType state)
        {
            FlagsHelper.Set(ref hitType, state);
        }

        public void RemoveState(HitType state)
        {
            FlagsHelper.Unset(ref hitType, state);
        }
    }
}