using System.Collections.Generic;

public class HeroBookManager : Single<HeroBookManager>
{
    public List<HeroBookGroupData> Groups { get; }

    private int _groupIndex;
    private int _heroIndex;

    public HeroBookManager()
    {
        var list = StaticData.LiblistTable.ElementList;

        Groups = list.ConvertAll(rowsInfo => new HeroBookGroupData
        {
            Index = list.IndexOf(rowsInfo),
            HeroList = rowsInfo.Colls,
        });

    }

    public void Turn(int offset)
    {
        var index = _heroIndex + offset;
        _heroIndex = (index + Groups.Count) % Groups.Count;
    }
}