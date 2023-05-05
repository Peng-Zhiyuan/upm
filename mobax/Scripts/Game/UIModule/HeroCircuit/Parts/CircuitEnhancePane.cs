using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public partial class CircuitEnhancePane : CircuitBasePane
{
    private HeroCircuitInfo _pickedCircuit;
    private Dictionary<HeroCircuitInfo, CircuitPickableListCell> _cellMap;
    private CircuitFilter _filter;
    // 技能颜色
    private Color _defaultSkillDescColor;

    public override void SetInfo(HeroInfo _)
    {
        _RefreshView();
    }

    public override void OnButton(string msg)
    {
        switch (msg)
        {
            case "enhance":
                _DoEnhance();
                break;
            case "cancel":
                UIEngine.Stuff.Back();
                break;
        }
    }

    private void Awake()
    {
        List_circuit.onItemRenderAction = _OnCircuitRender;
        List_circuitAttrs.onItemRenderAction = _OnAttrsRender;
        // 过滤逻辑
        _filter = new CircuitFilter();
        Node_filter.BindFilter(_filter, false);
        _filter.OnListChanged = _OnListChanged;
        // 技能颜色
        _defaultSkillDescColor = Txt_skillDesc.Color;
    }

    private void _RefreshView()
    {
        _ResetPick();
        _ResetList();
        _RefreshEnhanceView();
    }

    private void _ResetList()
    {
        var list = HeroCircuitManager.All.ConvertAll(HeroCircuitManager.GetCircuitInfo);
        _filter.ResetList(list);
        _filter.Refresh();
        _RefreshListRelated();
    }

    private void _RefreshListRelated()
    {
        List_circuit.numItems = (uint) _filter.List.Count;
    }

    private void _ResetPick()
    {
        if (null == _cellMap)
        {
            _cellMap = new Dictionary<HeroCircuitInfo, CircuitPickableListCell>();
        }
        else
        {
            _cellMap.Clear();
        }
    }
    
    private void _OnCircuitRender(int index, Transform tf)
    {
        var cell = tf.GetComponent<CircuitPickableListCell>();
        var circuitInfo = _filter.List[index];
        cell.SetInfo(circuitInfo);
        cell.Selected = _pickedCircuit == circuitInfo;
        cell.onSelect = _OnCellSelect;
        _cellMap[circuitInfo] = cell;
    }

    private void _OnCellSelect(CircuitPickableListCell cell)
    {
        if (cell.Selected) return;

        // 先将之前的选中的取消掉
        if (null != _pickedCircuit)
        {
            if (_cellMap.TryGetValue(_pickedCircuit, out var prevCell))
            {
                if (prevCell.CircuitInfo == _pickedCircuit)
                {
                    prevCell.Selected = false;
                }
            }
        }

        cell.Selected = true;
        _pickedCircuit = cell.CircuitInfo;
        _RefreshEnhanceView();
    }
    
    private void _OnAttrsRender(int index, Transform tf)
    {
        var cell = tf.GetComponent<HeroAttrEnhanceCell>();
        cell.SetInfo(_pickedCircuit, index);
    }

    private void _RefreshEnhanceView()
    {
        var available = _pickedCircuit != null;
        Button_confirm.Selected = available;
        Node_enhanceInfo.Selected = available;

        if (available)
        {
            _RefreshInfoView();
        }
    }
    
    private void _RefreshInfoView()
    {
        var info = _pickedCircuit;
        var colorCfg = StaticData.PuzzleColorTable.TryGet(info.Color);
        // var costList = info.LevelConfig.Subs;
        var costCfg = info.LevelConfig;
        Cost_enhance.SetActive(!info.LevelMax);
        Cell_circuit.SetInfo(info);
        Txt_circuitName.SetLocalizer("M4_circuit_words_item_format", colorCfg.Desc.Localize(), info.Conf.Name.Localize());
        // 按钮状态
        Button_confirm.Selected = !info.LevelMax && ItemUtil.IsEnough(costCfg.Sub1, costCfg.Sub2, costCfg.Need);
        Label_enhance.SetLocalizer(info.LevelMax ? "M4_level_max" : "M4_circuit_word_enhance");
        // 显示单块属性  
        List_circuitAttrs.numItems = (uint) info.Attrs.Count;
        // 显示技能相关
        var skillArray = StaticData.SkillTable.TryGet(info.Skill);
        if (null != skillArray)
        {
            var skillCfg = skillArray.Colls.First();
            Txt_skillDesc.Text = skillCfg.Desc.Localize();
            Txt_skillDesc.Color = info.SkillUnlocked ? _defaultSkillDescColor : Color.gray;
        }
        else
        {
            Txt_skillDesc.Text = "";
        }
    
        if (!info.LevelMax)
        {
            Cost_enhance.SetRequire(costCfg.Sub1, costCfg.Sub2, costCfg.Need);
        }
    }

    private void _OnListChanged()
    {
        _RefreshListRelated();
    }

    private async void _DoEnhance()
    {
        if (_pickedCircuit == null)
        {
            ToastManager.ShowLocalize("m4_desc_puzzle_no_picked");
            return;
        }
        
        var info = _pickedCircuit;
        if (info.LevelMax)
        {
            ToastManager.ShowLocalize("M4_level_max");
            return;
        }
        
        var conf = info.LevelConfig;
        var costList = conf.Subs;
        if (!ItemUtil.IsEnough(costList))
        {
            ToastManager.ShowLocalize("M4_not_enough");
            return;
        }

        if (conf.Advance == 1)
        {
            // 突破后， 后端会拿掉， 所以前端就先下手为强
            HeroCircuitManager.TakeAway(info);
            await HeroCircuitApi.Break(info.InstanceId);
        }
        else
        {
            await HeroCircuitApi.LevelUp(info.InstanceId, info.LevelConfig.Next);
        }
        
        // 更新list上的对应显示
        if (null != _pickedCircuit)
        {
            if (_cellMap.TryGetValue(_pickedCircuit, out var prevCell))
            {
                if (prevCell.CircuitInfo == _pickedCircuit)
                {
                    prevCell.SetInfo(_pickedCircuit);
                }
            }
        }
        // 刷新强化
        _RefreshEnhanceView();
    }
}