using System.Collections.Generic;
using BattleEngine.Logic;

namespace BattleEngine.Manager
{
    public class BattleItemUseManager : Singleton<BattleItemUseManager>
    {
        public List<int> Items { get; set; }
        
        public void InitData()
        {
            //BattleServerManager.Instance.
        }
    }
}