/* Created:Loki Date:2022-12-22*/

using System.Collections.Generic;

namespace BattleEngine.Logic
{
    public class BattleServerData
    {
        public CreateBattleResponse createBattleResponse;
        public BattlePlayer battlePlayer;

        public List<BattleHero> atkHeroLst = new List<BattleHero>();
        public Dictionary<int, List<ActorIFF>> atkHeroIFFDic = new Dictionary<int, List<ActorIFF>>();
        public Dictionary<int, List<BattleHero>> atkSubHeroDic = new Dictionary<int, List<BattleHero>>();
        public Dictionary<int, List<BattleHero>> defHeroDic = new Dictionary<int, List<BattleHero>>();
        public Dictionary<int, List<ActorIFF>> defHeroIFFDic = new Dictionary<int, List<ActorIFF>>();
        public int BattleSeed;
        public string BattleKey;
        public int FinishFrame = 0;
        public BattlePathData pathData;
        public List<BattleResultCheckData> checkData = new List<BattleResultCheckData>();
    }

    public class BattlePathData
    {
        public List<byte[]> graphAssetList = new List<byte[]>();
        public List<FixedVector3> offsetList = new List<FixedVector3>();
        public List<FixedVector3> rotationList = new List<FixedVector3>();
    }
}