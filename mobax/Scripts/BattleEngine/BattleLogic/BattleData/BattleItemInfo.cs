//------------------------------
//Author:Rocky
//CreateTime:2020/12/23
//------------------------------

using UnityEngine.UI;

namespace BattleEngine.Logic
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    public class ItemInfo : ICloneable
    {
        /**
     * 数据行id
     */
        public int id;
        public string _id; // 实例id
        public string uid; // 所属角色uid
        public int val; // 堆叠数量
        public int bag; // 背包分类
        public long ? ums;

        public int lv;
        public int slv; //星耀
        public int blv; //突破
        public Dictionary<int, int> skillsDic = new Dictionary<int, int>(); //英雄的技能
        public int[] att = new int[(int)HeroAttr.MaxNum + 1];
        
        //怪物的配置id（monster table）
        public int monsterid;

        ///   <summary>
        ///   浅拷贝
        ///   </summary>
        public System.Object Clone()
        {
            return this.MemberwiseClone();
        }

        public int ConfigID
        {
            get { return id; }
        }
    }

    public class BattleItemInfo : ItemInfo
    {
        public BattleItemInfo(string _id, int id)
        {
            this._id = _id;
            this.id = id;
            battlePlayerRecord = new BattlePlayerRecord(_id, id);
        }

        public BattleItemInfo(HeroInfo heroInfo)
        {
            _id = heroInfo.ItemInfo._id;
            id = heroInfo.HeroId;
            lv = heroInfo.Level;
            uid = heroInfo.ItemInfo.uid;
            slv = heroInfo.Star;
            blv = heroInfo.BreakLevel;
            att = new int[(int)HeroAttr.MaxNum];
            for (int i = 1; i < (int)HeroAttr.MaxNum; i++)
            {
                att[i] = heroInfo.GetAttribute((HeroAttr)i);
            }
            battlePlayerRecord = new BattlePlayerRecord(_id, id);
        }

        public BattleItemInfo(ItemInfo info, int[] param_attr)
        {
            this._id = info._id;
            this.id = info.id;
            this.lv = info.lv;
            this.slv = info.slv;
            this.blv = info.blv;
            this.att = info.att;
            battlePlayerRecord = new BattlePlayerRecord(info._id, info.id);
        }

        //战斗数值记录
        public BattlePlayerRecord battlePlayerRecord;
        public bool isBoss = false;
        public float scale = 1;
        public int warnRange = -1;
    }

    /// <summary>
    /// 英雄战场信息
    /// </summary>
    public class ActorIFF
    {
        public int PosIndex;
        public int CampID;
        public int TeamID;
        public Vector3 pos;
        public Vector3 dir;
    }
}