using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

public partial class HeroStarSpotAbilities : MonoBehaviour
{
    public HeroStarPassiveCell passiveCellPrefab; 
    public HeroStarCircuitCell circuitCellPrefab;
    public HeroStarAttrCell attrCellPrefab;
    public Color passedColor;
    public Color lockedColor;
    
    private int _maxHeight;
    private HeroInfo _heroInfo;
    private List<HeroStarPassiveCell> _passiveCells;
    private List<HeroStarCircuitCell> _puzzleCells;
    private List<HeroStarAttrCell> _attrCells;
    private int _passiveNum;
    private int _puzzleNum;
    private int _attrNum;
    private bool _passed;
    
    // SetSiblingIndex
    public void SetInfo(HeroInfo heroInfo, HeroStarRow cfg)
    {
        _heroInfo = heroInfo;
        _passiveCells ??= new List<HeroStarPassiveCell>();
        _puzzleCells ??= new List<HeroStarCircuitCell>();
        _attrCells ??= new List<HeroStarAttrCell>();
        _passiveNum = 0;
        _puzzleNum = 0;
        _attrNum = 0;
        // 简单用cfg的id大小来判断是否已经过了这个星级
        _passed = heroInfo.StarConfig.Id >= cfg.Id;
        Txt_starInfo.color = _passed ? passedColor : lockedColor;
        Txt_starInfo.text = $"{cfg.Star}-{cfg.Starlevel}";
        _CheckAdd(cfg, !_passed);
        
        // 没用到的都要关掉
        for (var i = _passiveNum; i < _passiveCells.Count; ++i)
        {
            _passiveCells[i].gameObject.SetActive(false);
        }
        for (var i = _puzzleNum; i < _puzzleCells.Count; ++i)
        {
            _puzzleCells[i].gameObject.SetActive(false);
        }
        for (var i = _attrNum; i < _attrCells.Count; ++i)
        {
            _attrCells[i].gameObject.SetActive(false);
        }
    }

    private void _AddPassiveCell(int skillId, int level, bool locked)
    {
        var cell = _GetPassiveCell();
        cell.SetInfo(_heroInfo, skillId, level, locked);
        cell.transform.SetSiblingIndex(_passiveNum - 1);
    }

    private void _AddPuzzleCell(int puzzleId, bool locked)
    {
        var cell = _GetPuzzleCell();
        cell.SetInfo(puzzleId, locked);
        cell.transform.SetSiblingIndex(_passiveNum + _puzzleNum - 1);
    }

    private void _AddAttrCell(int attr, int val, bool locked)
    {
        var cell = _GetAttrCell();
        cell.SetInfo(attr, val, locked);
        cell.transform.SetSiblingIndex(_passiveNum + _puzzleNum + _attrNum - 1);
    }

    private HeroStarPassiveCell _GetPassiveCell()
    {
        HeroStarPassiveCell cell;
        if (_passiveNum < _passiveCells.Count)
        {
            cell = _passiveCells[_passiveNum];
            cell.gameObject.SetActive(true);
        }
        else
        {
            cell = Instantiate(passiveCellPrefab, Content.transform);
            _passiveCells.Add(cell);
        }

        ++_passiveNum;
        return cell;
    }
    
    private HeroStarCircuitCell _GetPuzzleCell()
    {
        HeroStarCircuitCell cell;
        if (_puzzleNum < _puzzleCells.Count)
        {
            cell = _puzzleCells[_puzzleNum];
            cell.gameObject.SetActive(true);
        }
        else
        {
            cell = Instantiate(circuitCellPrefab, Content.transform);
            _puzzleCells.Add(cell);
        }

        ++_puzzleNum;
        return cell;
    }
    
    private HeroStarAttrCell _GetAttrCell()
    {
        HeroStarAttrCell cell;
        if (_attrNum < _attrCells.Count)
        {
            cell = _attrCells[_attrNum];
            cell.gameObject.SetActive(true);
        }
        else
        {
            cell = Instantiate(attrCellPrefab, Content.transform);
            _attrCells.Add(cell);
        }

        ++_attrNum;
        return cell;
    }

    private void _CheckAdd(HeroStarRow cfg, bool locked)
    {
        var skillId = cfg.skillId;
        if (skillId != 0)
        {
            _AddPassiveCell(skillId, cfg.skillLevel, locked);
        }

        var puzzle = cfg.Puzzle;
        if (puzzle != 0)
        {
            _AddPuzzleCell(puzzle, locked);
        }

        var prevCfg = HeroInfoEx.PrevStarCfg(_heroInfo, cfg.Id);
        for (var i = 0; i < cfg.levelAttrs.Length; i++)
        {
            var val = cfg.levelAttrs[i];
            if (val <= 0) continue;

            if (null != prevCfg)
            {
                val -= prevCfg.levelAttrs[i];
            }
            if (val <= 0) continue;
            
            var attr = HeroAttr.VIM + i;
            _AddAttrCell((int) attr, val, locked);
        }
    }
}