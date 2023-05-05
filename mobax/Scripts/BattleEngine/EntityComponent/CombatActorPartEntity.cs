namespace BattleEngine.Logic
{
    public sealed class CombatActorPartEntity : CombatUnitEntity
    {
        public string parentID;
        public string partNodeKey; //节点名 手部
        public string partKey; //部件名 左右手
        public string partUINode; //UI显示的攻击节点
        public string partLTranKey;
        //public HeroPartsRowSub partConfig;
    }
}