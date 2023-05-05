using System;
using System.Collections.Generic;

public partial class PuzzleShapeRow
{
    private List<Pos> _dots;
    
    public List<Pos> Dots
    {
        get { return _dots ??= Nodes.ConvertAll(posStr => new Pos(int.Parse(posStr.X), int.Parse(posStr.Y))); }
    }
}