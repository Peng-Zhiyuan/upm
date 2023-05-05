

public interface ICostItem
{
    int Id { get; set; }
    int Num { get; set; }
}

public partial class CostItem: ICostItem
{
}

public partial class RewardInfo : ICostItem
{
}