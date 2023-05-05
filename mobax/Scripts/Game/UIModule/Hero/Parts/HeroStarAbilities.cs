using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

public class HeroStarAbilities : MonoBehaviour
{
    public ScrollRect scrollRect;
    public LayoutElement layoutElement;
    public HeroStarPassiveCell passiveCellPrefab; 
    public HeroStarCircuitCell circuitCellPrefab; 
    public HeroStarAttrCell attrCellPrefab;
    
    private RectTransform _view;
    private int _maxHeight;
    private HeroInfo _heroInfo;
    private List<HeroStarPassiveCell> _passiveCells;
    private List<HeroStarCircuitCell> _puzzleCells;
    private List<HeroStarAttrCell> _attrCells;
    private Dictionary<int, HeroStarPassiveCell> _passiveMap;
    private Dictionary<int, HeroStarAttrCell> _attrMap;
    private int _passiveNum;
    private int _puzzleNum;
    private int _attrNum;
    
    // SetSiblingIndex
    public void SetInfo(HeroInfo heroInfo)
    {
        _heroInfo = heroInfo;
        _passiveCells ??= new List<HeroStarPassiveCell>();
        _puzzleCells ??= new List<HeroStarCircuitCell>();
        _attrCells ??= new List<HeroStarAttrCell>();
        _passiveNum = 0;
        _puzzleNum = 0;
        _attrNum = 0;
        if (null == _passiveMap)
        {
            _passiveMap = new Dictionary<int, HeroStarPassiveCell>();
        }
        else
        {
            _passiveMap.Clear();
        }

        if (null == _attrMap)
        {
            _attrMap = new Dictionary<int, HeroStarAttrCell>();
        }
        else
        {
            _attrMap.Clear();
        }

        var cfg = heroInfo.FirstStarConfig;
        // 加个报错
        if (cfg.Group != heroInfo.StarConfig.Group)
        {
            throw new Exception($"英雄({heroInfo.Name.Localize()})升级到了错误的id:{heroInfo.StarId}");
        }

        while (true)
        {
            _CheckAdd(cfg);
            // 如果已经到了当前的星级了， 就退出
            if (cfg == heroInfo.StarConfig) break;
            
            cfg = StaticData.HeroStarTable.TryGet(cfg.Next);
        }
        // 刷新属性
        _RefreshAttributes(heroInfo.StarConfig);
        
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

        _RefreshPreferredHeight();
    }

    public void Unlock(HeroStarRow cfg)
    {
        if (_CheckAdd(cfg) || _RefreshAttributes(cfg))
        {
            _RefreshPreferredHeight();
        }
    }
    
    private void Awake()
    {
        _view = GetComponent<RectTransform>();
    }

    private void _AddPassiveCell(int skillId)
    {
        var cell = _GetPassiveCell();
        cell.SetInfo(_heroInfo, skillId);
        _passiveMap[skillId] = cell;
        cell.transform.SetSiblingIndex(_passiveNum - 1);
    }

    private void _AddPuzzleCell(int puzzleId)
    {
        var cell = _GetPuzzleCell();
        cell.SetInfo(puzzleId);
        cell.transform.SetSiblingIndex(_passiveNum + _puzzleNum - 1);
    }

    private void _AddAttrCell(int attr, int val)
    {
        var cell = _GetAttrCell();
        cell.SetInfo(attr, val);
        _attrMap[attr] = cell;
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
            cell = Instantiate(passiveCellPrefab, scrollRect.content);
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
            cell = Instantiate(circuitCellPrefab, scrollRect.content);
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
            cell = Instantiate(attrCellPrefab, scrollRect.content);
            _attrCells.Add(cell);
        }

        ++_attrNum;
        return cell;
    }

    private async void _RefreshPreferredHeight()
    {
        await Task.Delay(1);
        if (0 == _maxHeight)
        {
            _maxHeight = (int) LayoutUtility.GetPreferredSize(_view, 1);
        }
        layoutElement.preferredHeight = Math.Min(_maxHeight, scrollRect.content.sizeDelta.y);
        scrollRect.ScrollToBottom();
    }

    private bool _CheckAdd(HeroStarRow cfg)
    {
        var changed = false;
        var skillId = cfg.skillId;
        if (skillId != 0)
        {
            if (!_passiveMap.TryGetValue(skillId, out var cell))
            {
                _AddPassiveCell(skillId);
                changed = true;
            }
            else
            {
                cell.RefreshLevel(cfg.skillLevel);
            }
        }

        var puzzle = cfg.Puzzle;
        if (puzzle != 0)
        {
            _AddPuzzleCell(puzzle);
            changed = true;
        }

        return changed;
    }

    private bool _RefreshAttributes(HeroStarRow cfg)
    {
        var changed = false;
        for (var i = 0; i < cfg.levelAttrs.Length; i++)
        {
            var val = cfg.levelAttrs[i];
            if (val <= 0) continue;
            
            var attr = (int) (HeroAttr.VIM + i);
            if (!_attrMap.TryGetValue(attr, out var cell))
            {
                _AddAttrCell(attr, val);
                changed = true;
            }
            else
            {
                cell.RefreshValue(val);
            }
        }

        return changed;
    }
}