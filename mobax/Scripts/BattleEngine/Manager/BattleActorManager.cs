using BattleSystem.ProjectCore;

namespace BattleEngine.View
{
    using System.Collections.Generic;
    using UnityEngine;
    using Logic;

    /// <summary>
    /// 当前房间战斗角色备战中的橘色
    /// 查找一个角色
    /// 查找当前队伍
    /// </summary>
    public sealed class BattleActorManager
    {
        private List<Creature> TotalActors { get; set; } = new List<Creature>();
        private Dictionary<string, Creature> MappingOfUID { get; set; } = new Dictionary<string, Creature>();

        public void Init()
        {
            EventManager.Instance.AddListener<CombatActorEntity>("CreateEntity", CreateEntity);
            EventManager.Instance.AddListener<CombatActorEntity>("RemoveEntity", RemoveEntity);
        }

        private void CreateEntity(CombatActorEntity enity)
        {
            CreateCreature(enity);
        }

        private void RemoveEntity(CombatActorEntity enity)
        {
            foreach (var VARIABLE in TotalActors)
            {
                if (enity == VARIABLE.mData)
                {
                    VARIABLE.SetActive(false);
                    TotalActors.Remove(VARIABLE);
                    break;
                }
            }
            if (MappingOfUID.TryGetValue(enity.UID, out var temp))
            {
                MappingOfUID.Remove(enity.UID);
            }
        }

        public Creature CreateCreature(CombatActorEntity data)
        {
            Creature tmp_Creature = null;
            if (!MappingOfUID.TryGetValue(data.UID, out tmp_Creature))
            {
                var go = new GameObject();
                tmp_Creature = go.AddComponent<Creature>();
                tmp_Creature.Init(data);
                TotalActors.Add(tmp_Creature);
                MappingOfUID.Add(data.UID, tmp_Creature);
                tmp_Creature.CreateActor();
            }
            return tmp_Creature;
        }

        public void SetAllActorToIdle()
        {
            for (int i = 0; i < TotalActors.Count; i++)
            {
                TotalActors[i].ToResumeAnim();
                TotalActors[i].ToIdleAnim();
            }
        }

        public Creature GetActor(string uid)
        {
            for (int i = 0; i < TotalActors.Count; i++)
            {
                if (TotalActors[i].mData.UID == uid)
                {
                    return TotalActors[i];
                }
            }
            return null;
        }

        public Creature GetActorByIndex(int index)
        {
            foreach (var VARIABLE in TotalActors)
            {
                if (VARIABLE.mData.PosIndex == index)
                    return VARIABLE;
            }
            return null;
        }

        public Creature GetActorByConfigID(int configID)
        {
            foreach (var VARIABLE in TotalActors)
            {
                if (VARIABLE.mData.ConfigID == configID)
                    return VARIABLE;
            }
            return null;
        }

        public Creature GetPlayer()
        {
            foreach (var VARIABLE in TotalActors)
            {
                if (VARIABLE.mData.PosIndex == BattleConst.PlayerPosIndex)
                    return VARIABLE;
            }
            return null;
        }

        public void OpenAI()
        {
            foreach (var VARIABLE in TotalActors)
            {
                if (VARIABLE.IsEnemy
                    || VARIABLE.IsMain)
                    VARIABLE.mData.OpenAI();
            }
        }

        public Creature GetAliveSubstitute()
        {
            foreach (var VARIABLE in TotalActors)
            {
                if (VARIABLE.IsHero
                    && VARIABLE.mData.IsSubstitut()
                    && !VARIABLE.mData.IsDead)
                {
                    return VARIABLE;
                }
            }
            return null;
        }

        public void RemoveActor(string uid)
        {
            foreach (var VARIABLE in TotalActors)
            {
                if (VARIABLE.mData.UID == uid)
                {
                    TotalActors.Remove(VARIABLE);
                    break;
                }
            }
            Creature role = null;
            if (MappingOfUID.TryGetValue(uid, out role))
            {
                MappingOfUID.Remove(uid);
            }
        }

        public List<Creature> GetCamp(int _campID, bool self = true)
        {
            List<Creature> actors = new List<Creature>();
            for (int i = 0; i < TotalActors.Count; i++)
            {
                int campID = TotalActors[i].mData.CampID;
                bool available = TotalActors[i].mData.Alive();
                if (self)
                {
                    // 选择阵营ID
                    if (available && _campID == campID)
                    {
                        actors.Add(TotalActors[i]);
                    }
                }
                else
                {
                    // 反选阵营ID
                    if (available && _campID != campID)
                    {
                        actors.Add(TotalActors[i]);
                    }
                }
            }
            return actors;
        }

        public List<Creature> GetAllCamp(int _campID, bool self = true)
        {
            List<Creature> actors = new List<Creature>();
            for (int i = 0; i < TotalActors.Count; i++)
            {
                int campID = TotalActors[i].mData.TeamKey;
                if (self)
                {
                    // 选择阵营ID
                    if (_campID == campID)
                    {
                        actors.Add(TotalActors[i]);
                    }
                }
                else
                {
                    // 反选阵营ID
                    if (_campID != campID)
                    {
                        actors.Add(TotalActors[i]);
                    }
                }
            }
            return actors;
        }

        public List<Creature> GetTeam(int _teamKey, bool self = true)
        {
            List<Creature> actors = new List<Creature>();
            for (int i = 0; i < TotalActors.Count; i++)
            {
                int teamKey = TotalActors[i].mData.TeamKey;
                if (self)
                {
                    // 选择队伍ID
                    if (_teamKey == teamKey)
                    {
                        actors.Add(TotalActors[i]);
                    }
                }
                else
                {
                    // 反选队伍ID
                    if (_teamKey != teamKey)
                    {
                        actors.Add(TotalActors[i]);
                    }
                }
            }
            return actors;
        }

        public List<Creature> GetAtkLst(bool isAll = true)
        {
            List<Creature> actors = new List<Creature>();
            for (int i = 0; i < TotalActors.Count; i++)
            {
                if (TotalActors[i].mData.isAtker)
                {
                    if (isAll)
                    {
                        actors.Add(TotalActors[i]);
                    }
                    else if (!isAll
                             && !TotalActors[i].mData.IsCantSelect)
                    {
                        actors.Add(TotalActors[i]);
                    }
                }
            }
            return actors;
        }

        public List<Creature> GetDefLst(bool isAll = true)
        {
            List<Creature> actors = new List<Creature>();
            for (int i = 0; i < TotalActors.Count; i++)
            {
                if (!TotalActors[i].mData.isAtker)
                {
                    if (isAll)
                    {
                        actors.Add(TotalActors[i]);
                    }
                    else if (!isAll
                             && !TotalActors[i].mData.IsCantSelect)
                    {
                        actors.Add(TotalActors[i]);
                    }
                }
            }
            return actors;
        }

        public List<Creature> GetAllActors()
        {
            return TotalActors;
        }

        public void Clear()
        {
            TotalActors.Clear();
            MappingOfUID.Clear();
        }

        public void UpdateActorManager()
        {
            for (int i = 0; i < TotalActors.Count; i++)
            {
                TotalActors[i].Doing();
            }
        }

        public void VisualOfAllActors(bool visiable, bool disable = false)
        {
            for (int i = 0; i < TotalActors.Count; i++)
            {
                if (visiable)
                {
                    this.SetLayer(TotalActors[i].gameObject, "Actor");
                }
                else
                {
                    this.SetLayer(TotalActors[i].gameObject, "DoNotRender");
                }
            }
        }

        private void SetLayer(GameObject target, string layerName)
        {
            Transform[] children = target.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < children.Length; i++)
            {
                children[i].gameObject.layer = LayerMask.NameToLayer(layerName);
            }
        }

        public void PlayHitActionState(BDamageAction damageAction)
        {
            if (damageAction.DamageSource == DamageSource.None
                || damageAction.DamageSource == DamageSource.Buff
                ||damageAction.damage >= 0)
            {
                return;
            }
            Creature creature = GetActor(damageAction.targetID);
            if (creature == null
                || creature.mData.IsCantSelect
                || creature.mData.HasBuffControlType())
            {
                return;
            }
            if (creature.mCurrentState == ACTOR_ACTION_STATE.Idle
                || (creature.mCurrentState == ACTOR_ACTION_STATE.ATK && creature.mData.CurrentSkillExecution == null))
            {
                creature.SetState(ACTOR_ACTION_STATE.Hurt);
            }
            if (Battle.Instance.mode.ModeType == BattleModeType.SkillView)
            {
                creature.PlayAnim("hit");
            }
        }
    }
}