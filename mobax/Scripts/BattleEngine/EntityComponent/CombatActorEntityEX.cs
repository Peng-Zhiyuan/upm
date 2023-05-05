namespace BattleEngine.Logic
{
    public static class CombatActorEntityHelper
    {
        public static CombatActorEntity GetMaxHpEnemy(CombatActorEntity owner)
        {
            CombatActorEntity entity = null;
            var data = BattleLogicManager.Instance.BattleData.allActorDic.GetEnumerator();
            int max = 0;
            while (data.MoveNext())
            {
                CombatActorEntity enemy = data.Current.Value;
                if (enemy.isAtker == owner.isAtker)
                {
                    continue;
                }
                if (enemy.CurrentHealth.Value > max)
                {
                    entity = enemy;
                    max = enemy.CurrentHealth.Value;
                }
            }
            return entity;
        }

        public static CombatActorEntity GetMinHpEnemy(CombatActorEntity owner)
        {
            CombatActorEntity entity = null;
            var data = BattleLogicManager.Instance.BattleData.allActorDic.GetEnumerator();
            int min = -1;
            while (data.MoveNext())
            {
                CombatActorEntity enemy = data.Current.Value;
                if (enemy.isAtker == owner.isAtker)
                {
                    continue;
                }
                if (enemy.CurrentHealth.Value < min
                    || min == -1)
                {
                    entity = enemy;
                    min = enemy.CurrentHealth.Value;
                }
            }
            return entity;
        }

        public static CombatActorEntity GetMaxAttEnemy(CombatActorEntity owner, int attType)
        {
            CombatActorEntity entity = null;
            var data = BattleLogicManager.Instance.BattleData.allActorDic.GetEnumerator();
            int max = 0;
            while (data.MoveNext())
            {
                CombatActorEntity enemy = data.Current.Value;
                if (enemy.isAtker == owner.isAtker)
                {
                    continue;
                }
                if (enemy.AttrData.GetValue((AttrType)attType) > max)
                {
                    entity = enemy;
                    max = enemy.CurrentHealth.Value;
                }
            }
            return entity;
        }

        public static CombatActorEntity GetMinAttEnemy(CombatActorEntity owner, int attType)
        {
            CombatActorEntity entity = null;
            var data = BattleLogicManager.Instance.BattleData.allActorDic.GetEnumerator();
            int min = -1;
            while (data.MoveNext())
            {
                CombatActorEntity enemy = data.Current.Value;
                if (enemy.isAtker == owner.isAtker)
                {
                    continue;
                }
                if (enemy.AttrData.GetValue((AttrType)attType) < min
                    || min == -1)
                {
                    entity = enemy;
                    min = enemy.CurrentHealth.Value;
                }
            }
            return entity;
        }

        public static CombatActorEntity GetMaxDisEnemy(CombatActorEntity owner)
        {
            CombatActorEntity entity = null;
            var data = BattleLogicManager.Instance.BattleData.allActorDic.GetEnumerator();
            float max = 0;
            while (data.MoveNext())
            {
                CombatActorEntity enemy = data.Current.Value;
                if (enemy.isAtker == owner.isAtker)
                {
                    continue;
                }
                float dis = MathUtil.DoubleDistance(owner.GetPositionXZ(), enemy.GetPositionXZ());
                if (dis > max)
                {
                    entity = enemy;
                    max = dis;
                }
            }
            return entity;
        }

        public static CombatActorEntity GetMinDisEnemy(CombatActorEntity owner)
        {
            CombatActorEntity entity = null;
            var data = BattleLogicManager.Instance.BattleData.allActorDic.GetEnumerator();
            float min = -1;
            while (data.MoveNext())
            {
                CombatActorEntity enemy = data.Current.Value;
                if (enemy.isAtker == owner.isAtker)
                {
                    continue;
                }
                float dis = MathUtil.DoubleDistance(owner.GetPositionXZ(), enemy.GetPositionXZ());
                if (dis < min
                    || min == -1)
                {
                    entity = enemy;
                    min = dis;
                }
            }
            return entity;
        }

        public static CombatActorEntity GetMaxHpTeammate(CombatActorEntity owner)
        {
            CombatActorEntity entity = null;
            var data = BattleLogicManager.Instance.BattleData.allActorDic.GetEnumerator();
            int max = 0;
            while (data.MoveNext())
            {
                CombatActorEntity enemy = data.Current.Value;
                if (enemy.isAtker != owner.isAtker)
                {
                    continue;
                }
                if (enemy.CurrentHealth.Value > max)
                {
                    entity = enemy;
                    max = enemy.CurrentHealth.Value;
                }
            }
            return entity;
        }

        public static CombatActorEntity GetMinHpTeammate(CombatActorEntity owner)
        {
            CombatActorEntity entity = null;
            var data = BattleLogicManager.Instance.BattleData.allActorDic.GetEnumerator();
            int min = 0;
            while (data.MoveNext())
            {
                CombatActorEntity enemy = data.Current.Value;
                if (enemy.isAtker != owner.isAtker)
                {
                    continue;
                }
                if (enemy.CurrentHealth.Value < min)
                {
                    entity = enemy;
                    min = enemy.CurrentHealth.Value;
                }
            }
            return entity;
        }

        public static CombatActorEntity GetMaxAttTeammate(CombatActorEntity owner, int attType)
        {
            CombatActorEntity entity = null;
            var data = BattleLogicManager.Instance.BattleData.allActorDic.GetEnumerator();
            int max = 0;
            while (data.MoveNext())
            {
                CombatActorEntity enemy = data.Current.Value;
                if (enemy.isAtker != owner.isAtker)
                {
                    continue;
                }
                if (enemy.AttrData.GetValue((AttrType)attType) > max)
                {
                    entity = enemy;
                    max = enemy.CurrentHealth.Value;
                }
            }
            return entity;
        }

        public static CombatActorEntity GetMinAttTeammate(CombatActorEntity owner, int attType)
        {
            CombatActorEntity entity = null;
            var data = BattleLogicManager.Instance.BattleData.allActorDic.GetEnumerator();
            int min = 0;
            while (data.MoveNext())
            {
                CombatActorEntity enemy = data.Current.Value;
                if (enemy.isAtker != owner.isAtker)
                {
                    continue;
                }
                if (enemy.AttrData.GetValue((AttrType)attType) < min)
                {
                    entity = enemy;
                    min = enemy.CurrentHealth.Value;
                }
            }
            return entity;
        }

        public static CombatActorEntity GetMaxDisTeammate(CombatActorEntity owner)
        {
            CombatActorEntity entity = null;
            var data = BattleLogicManager.Instance.BattleData.allActorDic.GetEnumerator();
            float max = 0;
            while (data.MoveNext())
            {
                CombatActorEntity enemy = data.Current.Value;
                if (enemy.isAtker != owner.isAtker)
                {
                    continue;
                }
                float dis = MathHelper.ActorDistance(owner, enemy);
                if (dis > max)
                {
                    entity = enemy;
                    max = dis;
                }
            }
            return entity;
        }

        public static CombatActorEntity GetMinDisTeammate(CombatActorEntity owner)
        {
            CombatActorEntity entity = null;
            var data = BattleLogicManager.Instance.BattleData.allActorDic.GetEnumerator();
            float min = 0;
            while (data.MoveNext())
            {
                CombatActorEntity enemy = data.Current.Value;
                if (enemy.isAtker != owner.isAtker)
                {
                    continue;
                }
                float dis = MathHelper.ActorDistance(owner, enemy);
                if (dis < min)
                {
                    entity = enemy;
                    min = dis;
                }
            }
            return entity;
        }
    }
}