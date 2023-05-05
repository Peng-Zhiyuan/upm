using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class HeroManager : Single<HeroManager>
{
    private Dictionary<int, HeroInfo> _heroMap = new Dictionary<int, HeroInfo>();

    public List<ItemInfo> List => Database.Stuff.itemDatabase.GetItemInfoListByIType(IType.Hero);

    // 标记特殊的不要展示出来
    public List<ItemInfo> AvailList => List.FindAll(item =>
        {
            var cfg = StaticData.HeroTable.TryGet(item.id);
            return cfg.Show == 1 && cfg.Special == 0;
        }
    );

    public HeroInfo GetHeroInfo(int heroId)
    {
        if (!_heroMap.TryGetValue(heroId, out var heroInfo))
        {
            heroInfo = _heroMap[heroId] = new HeroInfo(heroId);
        }
        return heroInfo;
    }

    public bool Have(int heroId)
    {
        var heroInfo = GetHeroInfo(heroId);
        return heroInfo.Unlocked;
    }

    public void TryUploadAllMyHeroPower()
    {
        var heroList = AvailList;
        foreach (var one in heroList)
        {
            UploadHeroPowerIfNeed(one.id);
        }
    }

    public void UploadHeroPowerIfNeed(int heroId)
    {
        if (!_heroMap.TryGetValue(heroId, out var heroInfo))
        {
            heroInfo = GetHeroInfo(heroId);
            if (heroInfo.Conf.Special == 0 && heroInfo.Unlocked && heroInfo.ServerPower <= 0)
            {
                // 如果服务端还没有记录战力，上报一下
                HeroApi.ReportPowerAsync(heroId, heroInfo.Power);
                Debug.Log("[HeroManager] UploadHeroPower id: " + heroId + ", power: " + heroInfo.Power);
            }
        }
    }
}

public class HeroPuzzleListener
{
    private int _heroId;

    public HeroPuzzleListener(int heroId)
    {
        _heroId = heroId;
    }

    public void Refresh()
    {
        // var heroInfo = HeroManager.Instance.GetHeroInfo(_heroId);
        // var puzzleNode = Reminder.GetNode($"{HeroReminderConst.Hero_heroPuzzlePrefix}{_heroId}");
        // if (HeroPuzzleManager.Instance.CheckMapFull(heroInfo))
        // {
        //     puzzleNode.SetValue(false);
        //     Database.Stuff.itemDatabase.RemoveItemTypeChange(IType.Puzzle, _OnPuzzleChanged);
        // }
        // else
        // {
        //     var puzzlesOnMap = HeroPuzzleManager.Instance.GetPuzzles(heroInfo.HeroId);
        //     var puzzles = HeroPuzzleManager.Instance.GetList(heroInfo);
        //     puzzleNode.SetValue(puzzlesOnMap.Count < puzzles.Count);
        //     Database.Stuff.itemDatabase.AddItemTypeChange(IType.Puzzle, _OnPuzzleChanged);
        // }
    }

    private void _OnPuzzleChanged(int itemId)
    {
        Refresh();
    }
}