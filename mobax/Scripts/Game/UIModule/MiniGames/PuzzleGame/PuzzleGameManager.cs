using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public class PuzzleGameManager : Single<PuzzleGameManager>
{
    // public 

    private List<int> _idList;
    private int _idIndex = -1;
    private GamePuzzleRow _puzzleCfg;
    private byte[] _flags;

    public bool Completed => Array.TrueForAll(_flags, val => val == 1);
    
    public GamePuzzleRow FetchPuzzle()
    {
        _idList ??= StaticData.GamePuzzleTable.ElementList.ConvertAll(item => item.Id);

        if (_idIndex < 0 || _idIndex >= _idList.Count)
        {
            // shuffle
            _idList.Sort((id1, id2) => Random.Range(0, 10) >= 5 ? 1 : -1);
            _idIndex = 0;
        }

        _puzzleCfg = StaticData.GamePuzzleTable.TryGet(_idList[_idIndex]);
        _flags ??= new byte[_puzzleCfg.Field * _puzzleCfg.Field];
        ++_idIndex;
        return _puzzleCfg;
    }

    public void SetFlag(Pos node)
    {
        SetFlag(node.X, node.Y);
    }
    
    public void SetFlag(int x, int y)
    {
        _flags[y * _puzzleCfg.Field + x] = 1;
    }
    
    public void ClearFlag(Pos node)
    {
        ClearFlag(node.X, node.Y);
    }
    
    public void ClearFlag(int x, int y)
    {
        _flags[y * _puzzleCfg.Field + x] = 0;
    }

    public bool CheckOverlay(int x, int y)
    {
        return _flags[y * _puzzleCfg.Field + x] == 1;
    }
}