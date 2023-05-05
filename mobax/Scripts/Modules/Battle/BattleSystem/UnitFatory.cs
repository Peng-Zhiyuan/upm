using System.Collections.Generic;
using BattleEngine.Logic;
using BattleSystem.ProjectCore;
using UnityEngine;

public class UnitFatory
{
    public static int UID = 0;

    public static CombatActorEntity CreateUnit(int rowId, int level, Vector3 pos, Vector3 dir, int slot, bool isHero, int camp, int team, bool isReady, float difficult = 1f, BattleHero heroinfo = null, Dictionary<AttrType, int> OtherAttrs = null)
    {
        var owner = CreateUnitReal(rowId, level, pos, dir, slot, isHero, camp, team, isReady, difficult, heroinfo, OtherAttrs);
#region LinkActorEntity 废弃
        // if (Battle.Instance.IsMainTask
        //     || Battle.Instance.GameMode.ModeType == BattleModeType.Arena)
        // {
        //     var list = FormationUtil.GetSubHeroList(Battle.Instance.param.pveParam.FormationIndex, rowId);
        //     int index = 0;
        //     foreach (var VARIABLE in list)
        //     {
        //         if (VARIABLE == 0)
        //             continue;
        //         Vector3 beginPos = pos - dir * 3;
        //         Vector3 offsetValue = index == 0 ? Vector3.left : Vector3.right;
        //         Vector3 initPos = beginPos + Quaternion.LookRotation(dir.normalized, Vector3.up) * offsetValue;
        //         var entity = CreateUnitReal(VARIABLE, level, initPos, dir, BattleConst.SSPAssistPosIndexStart, isHero, camp, team, isMonster, isReady, difficult, null, null);
        //         owner.LinkerUIDLst.Add(entity.UID);
        //         index++;
        //     }
        // }
#endregion
        return owner;
    }

    public static CombatActorEntity CreateUnitReal(int rowId, int level, Vector3 pos, Vector3 dir, int slot, bool isHero, int camp, int team, bool isReady, float difficult = 1f, BattleHero heroinfo = null, Dictionary<AttrType, int> OtherAttrs = null)
    {
        ///强制Y轴贴地
        if (!BattleUtil.IsInMap(pos)
            && slot != BattleConst.SSPAssistPosIndexStart
            && slot != BattleConst.FriendPosIndex)
        {
            pos = BattleUtil.GetWalkablePos(pos);
        }
        BattleEngine.Logic.ItemInfo itemInfo = CreateItemInfo(rowId, level, pos, dir, slot, isHero, camp, team, isReady, difficult, heroinfo, OtherAttrs);
        CombatActorEntity entity = Entity.Create<CombatActorEntity>();
        float scale = 1.0f;
        entity.SetTeamInfo(camp, team);
        entity.InitBattleInfo(itemInfo, null);
        if (!isHero)
        {
            MonsterRow monster = StaticData.MonsterTable.TryGet(rowId);
            entity.Weak = monster.Weak;
            entity.Sort = monster.Sort;
            entity.InitBattleInfo(itemInfo, null);
            entity.battleItemInfo.isBoss = entity.Sort == 2;
            entity.battleItemInfo.monsterid = rowId;
            scale = monster.Scale;
            entity.battleItemInfo.scale = monster.Scale;
        }
        entity.AttrData.SetDifficult(difficult);
        if (OtherAttrs != null)
        {
            foreach (var VARIABLE in OtherAttrs)
            {
                var val = entity.AttrData.GetValue((AttrType)VARIABLE.Key);
                entity.AttrData.AddBuffAttr(VARIABLE.Key, VARIABLE.Value);
            }
        }
        entity.BornCharacters(pos, dir, scale, StaticData.HeroTable[entity.ConfigID]);
        entity.SetPosition(pos);
        entity.SetForward(dir);
        entity.PosIndex = slot;
        if (slot == BattleConst.FriendPosIndex)
        {
            entity.SetLifeState(ACTOR_LIFE_STATE.Assist);
            entity.CurrentMp.SetFull();
        }
        else if (slot == BattleConst.PlayerPosIndex)
        {
            entity.SetLifeState(ACTOR_LIFE_STATE.LookAt);
        }
        else if (slot == BattleConst.SSPAssistPosIndexStart)
        {
            entity.SetLifeState(ACTOR_LIFE_STATE.Assist);
        }
        List<SkillRow> passiveSkillRowLst = new List<SkillRow>();
        foreach (var VARIABLE in itemInfo.skillsDic)
        {
            SkillRow sr = SkillUtil.GetSkillItem(VARIABLE.Key, VARIABLE.Value);
            if (sr != null)
            {
                if (sr.skillType == (int)SKILL_TYPE.Passive)
                {
                    passiveSkillRowLst.Add(sr);
                }
                else
                {
                    entity.AttachSkill(sr);
                }
            }
        }
        if (isHero
            && slot < 20
            && camp == BattleConst.ATKCampID)
        {
            var hero = HeroManager.Instance.GetHeroInfo(rowId);
            if (hero != null && hero.Unlocked)
            {
                foreach (var kv in hero.StarSkillMap)
                {
                    SkillRow row = SkillUtil.GetSkillItem(kv.Key, kv.Value);
                    if (row == null)
                    {
                        continue;
                    }
                    passiveSkillRowLst.Add(row);
                }
            }
            
            List<int> passiveSkillList = HeroCircuitManager.GetSkills(rowId);
            for (int i = 0; i < passiveSkillList.Count; i++)
            {
                SkillRow row = SkillUtil.GetSkillItem(passiveSkillList[i], 1);
                if (row == null)
                {
                    continue;
                }
                passiveSkillRowLst.Add(row);
            }

            //根据战力对比，添加额外buffer(主线或者竞技场)
            if (BattleDataManager.Instance.ExternalBufferID != -1)
            {
                entity.AttachBuff(BattleDataManager.Instance.ExternalBufferID);
            }

            //公会Buffer
            if (Battle.Instance.IsMainTask
                || Battle.Instance.IsResourceTask
                || Battle.Instance.mode.ModeType == BattleModeType.Dreamscape
                || Battle.Instance.mode.ModeType == BattleModeType.TowerFixed
                || Battle.Instance.mode.ModeType == BattleModeType.TowerNormal)
            {
                var buff_info = GuildManager.Stuff.GetDispatchBuffId(entity.ConfigID);
                if (buff_info.buffId != 0)
                {
                    entity.AttachBuff(buff_info.buffId, buff_info.buffLv);
                }
            }
            foreach (var VARIABLE in Battle.Instance.ExtBuffIDs)
            {
                entity.AttachBuff(VARIABLE);
            }
        }
        entity.InitPassiveSkill();
        entity.AddPassiveSkillLst(passiveSkillRowLst);
        entity.AttachSkillGroup();
        BattleLogicManager.Instance.BattleData.AddActorData(entity);
#if !SERVER
        if (BattleLogicManager.Instance.IsOpenBattleViewLayer)
        {
            EventManager.Instance.SendEvent<CombatActorEntity>("CreateEntity", entity);
        }
#endif

        //一些继承血量的模式，设置一下英雄血量
        if (isHero
            && BattleUtil.HeroHps != null
            && BattleUtil.HeroHps.TryGetValue(itemInfo.ConfigID, out var hp))
        {
            entity.CurrentHealth.SetValue(hp);
        }
        return entity;
    }

    public static BattleEngine.Logic.ItemInfo CreateItemInfo(int rowId, int level, Vector3 pos, Vector3 dir, int slot, bool isHero, int camp, int team, bool isReady, float difficult = 1f, BattleHero heroinfo = null, Dictionary<AttrType, int> OtherAttrs = null)
    {
        BattleEngine.Logic.ItemInfo itemInfo = new BattleEngine.Logic.ItemInfo();
        itemInfo._id = "hero_" + rowId + UID++;
        if (isHero)
        {
            itemInfo.id = rowId;
            itemInfo.lv = level;
            itemInfo.blv = 1;
            itemInfo.slv = 1;
            BattleHero data = Battle.Instance.GetHeroData(rowId);
            if (data != null)
            {
                itemInfo.att = data.Attr;
            }
            else
            {
                var heroRow = StaticData.HeroTable.TryGet(rowId);
                for (int i = 0; i < heroRow.Attrs.Length; i++)
                {
                    itemInfo.att[i + 1] = heroRow.Attrs[i];
                }
            }
        }
        else
        {
            MonsterRow monster = StaticData.MonsterTable.TryGet(rowId);
            itemInfo.id = monster.heroID;
            itemInfo.lv = monster.monsterLv;
            itemInfo.blv = 1;
            itemInfo.slv = 1;
            var attrGroupArryRow = StaticData.AttrGroupTable.TryGet(monster.attrGroup);
            if (attrGroupArryRow != null)
            {
                AttrGroupRow row = attrGroupArryRow.Colls.Find(it => it.Rank == monster.monsterLv);
                if (row != null)
                {
                    for (int i = 0; i < row.Attrs.Length; i++)
                    {
                        itemInfo.att[i + 1] = row.Attrs[i];
                    }
                    if (BattleConst.Difficult == EStageDifficult.Hard)
                    {
                        var scale1 = row.Ratio * 0.001f;
                        for (int i = 0; i < itemInfo.att.Length; i++)
                        {
                            var old = itemInfo.att[i];
                            var v = old * scale1;
                            itemInfo.att[i] = (int)v;
                        }
                    }
                }
            }
            else
            {
                Debug.LogError($"AttrGroup表没有找到怪物组: {monster.attrGroup}, 请检查");
            }
        }
        AttachSkill(itemInfo, isHero, heroinfo);
        return itemInfo;
    }

    public static void AttachSkill(BattleEngine.Logic.ItemInfo info, bool isHero, BattleHero heroinfo = null)
    {
        List<int> skillList = ItemInfoUtil.GetHeroSkill(info);
        for (int j = 0; j < skillList.Count; j++)
        {
            int skillID = skillList[j];
            if (skillID == 0)
            {
                continue;
            }
            int lv = 1;
            if (isHero && heroinfo == null)
            {
                var hero = HeroManager.Instance.GetHeroInfo(info.id);
                if (hero != null
                    && hero.Unlocked)
                {
                    lv = hero.GetSkillLevel(skillID);
                }
            }
            else
            {
                if (Battle.Instance.IsArenaMode
                    && heroinfo != null
                    && heroinfo.Skill != null
                    && heroinfo.Skill.ContainsKey(skillID))
                {
                    lv = heroinfo.Skill[skillID];
                }
            }
            SkillRow sr = SkillUtil.GetSkillItem(skillID, lv);
            if (sr != null)
            {
                info.skillsDic.Add(skillID, lv);
            }
        }
    }

    public static void CreateEntity(ItemInfo data) { }
}