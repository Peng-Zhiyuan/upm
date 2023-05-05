namespace BattleEngine.Logic
{
    using System.Collections.Generic;

    public sealed partial class CombatActorEntity
    {
        private List<CombatActorEntity> OTList = new List<CombatActorEntity>();
        public Dictionary<string, FloatNumeric> OTNumLst = new Dictionary<string, FloatNumeric>();
        private Dictionary<string, FloatModifier> ChangeOTNumLst = new Dictionary<string, FloatModifier>();
        private FloatNumeric _maxOTNum;
        public FloatNumeric MaxOTNum
        {
            get
            {
                if (_maxOTNum == null) _maxOTNum = new FloatNumeric();
                return _maxOTNum;
            }
        }

        public int InitOTNum
        {
            get
            {
                HeroRow row = battleItemInfo.GetHeroRow();
                return row.overTaunted;
            }
        }

        public void AddOTNum(string uid, int num)
        {
            if (OTNumLst.ContainsKey(uid))
            {
                OTNumLst[uid].SetBase(OTNumLst[uid].baseValue + num);
            }
            else
            {
                FloatNumeric fn = new FloatNumeric();
                fn.SetBase(num);
                OTNumLst[uid] = fn;
            }
            RefreshOTList();
        }

        public void SetOTToTheTop(string uid)
        {
            RefreshOTList();
            if (OTList.Count == 0
                || !OTNumLst.ContainsKey(OTList[0].UID))
            {
                return;
            }
            float num = OTNumLst[OTList[0].UID].Value + 1000;
            if (OTNumLst.ContainsKey(uid))
            {
                num -= OTNumLst[uid].Value;
            }
            else
            {
                FloatNumeric fn = new FloatNumeric();
                fn.SetBase(0);
                OTNumLst[uid] = fn;
            }
            FloatModifier fm = new FloatModifier() { Value = num };
            OTNumLst[uid].AddFinalAddModifier(fm);
            ChangeOTNumLst[uid] = fm;
            RefreshOTList();
        }

        public void ClearModifierOTNum(string uid)
        {
            if (!OTNumLst.ContainsKey(uid))
            {
                return;
            }
            if (!ChangeOTNumLst.ContainsKey(uid))
            {
                return;
            }
            OTNumLst[uid].RemoveFinalAddModifier(ChangeOTNumLst[uid]);
        }

        public void ClearAllOTNum()
        {
            var data = OTNumLst.GetEnumerator();
            while (data.MoveNext())
            {
                data.Current.Value.SetBase(0);
            }
        }

        public void AddSkillOTNum(string uid, int skillid, float parms = 0)
        {
            int num = 0;
            if (OTNumLst.ContainsKey(uid))
            {
                OTNumLst[uid].SetBase(OTNumLst[uid].baseValue + num);
            }
            else
            {
                FloatNumeric fn = new FloatNumeric();
                fn.SetBase(num);
                OTNumLst[uid] = fn;
            }
            RefreshOTList();
        }

        public void InitPushAttackList(CombatActorEntity actorEntity)
        {
            if (actorEntity.CurrentHealth.Value <= 0
                || actorEntity.CampID == CampID
                || OTList.Contains(actorEntity))
                return;
            OTList.Add(actorEntity);
        }

        public void PushAttackList(CombatActorEntity actorEntity)
        {
            if (actorEntity.CurrentHealth.Value <= 0
                || actorEntity.CampID == CampID)
                return;
            if (!OTList.Contains(actorEntity))
            {
                OTList.Add(actorEntity);
            }
        }

        private void RefreshOTList()
        {
            RefreshAttackTargetEntityLiST();
            OTList.Sort((CombatActorEntity data1, CombatActorEntity data2) =>
                            {
                                if (!OTNumLst.ContainsKey(data1.UID)
                                    || !OTNumLst.ContainsKey(data2.UID))
                                    return 0;
                                if (OTNumLst[data1.UID].Value > OTNumLst[data2.UID].Value)
                                {
                                    return -1;
                                }
                                else if (OTNumLst[data1.UID] == OTNumLst[data2.UID])
                                {
                                    return 0;
                                }
                                else
                                {
                                    return 1;
                                }
                            }
            );
        }

        /// <summary>
        /// 不知有何意义,修改目标不频繁切换攻击目标，镜头不抖动
        /// </summary>
        private CombatActorEntity currentOTTargetEntity;

        public CombatActorEntity GetAttackTargetEntity()
        {
            RefreshAttackTargetEntityLiST();
            CombatActorEntity tempEntity = null;
            for (int i = 0; i < OTList.Count; i++)
            {
                if (OTList[i].IsCantSelect)
                    continue;
                tempEntity = OTList[i];
                break;
            }
            if (isAtker)
            {
                if (currentOTTargetEntity != null
                    && tempEntity != null
                    && !currentOTTargetEntity.UID.Equals(tempEntity.UID))
                {
                    if (OTNumLst[tempEntity.UID].Value > OTNumLst[currentOTTargetEntity.UID].Value + BattleUtil.GetGlobalK(GlobalK.SELF_OT_CHANGE_TARGET_MAX_VALUE_43))
                    {
                        currentOTTargetEntity = tempEntity;
                    }
                }
                else if (currentOTTargetEntity == null)
                {
                    currentOTTargetEntity = tempEntity;
                }
            }
            else
            {
                return tempEntity;
            }
            return currentOTTargetEntity;
        }

        public void RefreshAttackTargetEntityLiST()
        {
            if (currentOTTargetEntity != null
                && currentOTTargetEntity.IsCantSelect)
            {
                currentOTTargetEntity = null;
            }
            for (int i = 0; i < OTList.Count; i++)
            {
                if (OTList[i].CurrentHealth.Value <= 0)
                {
                    if (OTNumLst.ContainsKey(OTList[i].UID))
                    {
                        OTNumLst.Remove(OTList[i].UID);
                    }
                    OTList.RemoveAt(i);
                    i--;
                }
            }
        }

        public float GetFirstOTNum()
        {
            if (OTList.Count <= 0)
            {
                return 0;
            }
            if (!OTNumLst.ContainsKey(OTList[0].UID))
            {
                return 0;
            }
            return OTNumLst[OTList[0].UID].Value;
        }
    }
}