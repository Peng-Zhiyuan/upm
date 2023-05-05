using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITaskRow
{
    public int Id { get; set; }

    public string Icon { get; set; }

    public string Name { get; set; }


    public string Desc { get; set; }


    public int Target { get; set; }


    public int Value { get; set; }

    public List<RewardInfo> Items { get; }

    public int Show { get; set; }


    public JumpInfo Jump { get; set; }

    public int Score { get; set; }

    public int doubleTask { get; set; }
}

public interface IRewardRow
{
    public int Id { get; set; }

    public int Score { get; set; }

    public List<RewardInfo> Rewards { get; }
}

 public partial class TaskRow : ITaskRow
{

}

public partial class DailyTaskRow : ITaskRow
{

}

public partial class WeekTaskRow : ITaskRow
{

}

public partial class DailyRewardRow : IRewardRow
{

}

public partial class WeekRewardRow : IRewardRow
{

}
