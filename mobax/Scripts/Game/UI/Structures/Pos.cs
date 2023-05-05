public partial class Pos
{
    public Pos()
    {
    }
    
    public Pos(int x, int y)
    {
        X = x;
        Y = y;
    }
    
    public static Pos Default { get; } = new Pos(int.MaxValue, int.MaxValue);
    
    public static bool operator ==(Pos pos1, Pos pos2)
    {
        return _Equal(pos1, pos2);
    }
    
    public static bool operator !=(Pos pos1, Pos pos2)
    {
        return !_Equal(pos1, pos2);
    }

    private static bool _Equal(Pos pos1, Pos pos2)
    {
        return pos1.X == pos2.X && pos1.Y == pos2.Y;
    }
}
