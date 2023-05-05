namespace BattleEngine.Logic
{
    public class BuffUtil
    {
        public static BuffRow GetBuffRow(int buffID, int level = 1)
        {
            var array = StaticData.BuffTable.TryGet(buffID);
            if (array == null
                || array.Colls.Count < level)
                return null;
            var buffRow = StaticData.BuffTable.TryGet(buffID).Colls[level - 1];
            buffRow.BuffID = buffID;
            return buffRow;
        }

        public static bool isTriggerChance(BuffEffect buffEffect)
        {
            if (buffEffect == null)
            {
                return false;
            }
            if (buffEffect.Per == 0
                || buffEffect.Per >= 1000)
            {
                return true;
            }
            int per = BattleLogicManager.Instance.Rand.RandomVaule(0, 1000);
            if (per <= buffEffect.Per)
            {
                return true;
            }
            return false;
        }
    }
}