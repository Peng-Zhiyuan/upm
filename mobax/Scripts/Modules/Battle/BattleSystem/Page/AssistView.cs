using System;
using System.Collections.Generic;
using BattleSystem.ProjectCore;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public partial class AssistView : MonoBehaviour
{
    public List<Image> Heads = new List<Image>();

    private void Awake()
    {
        GameEventCenter.AddListener(GameEvent.SuperSkillEnd, this, this.SuperSkillEnd);
    }

    private void OnDestroy()
    {
        GameEventCenter.RemoveListener(GameEvent.SuperSkillEnd, this);
    }

    private void SuperSkillEnd(object[] data)
    {
        int configID = (int) data[0];
        SetData(configID);
    }

    public void SetData(int ConfigID)
    {
        if (!Battle.Instance.IsMainTask
            && Battle.Instance.GameMode.ModeType != BattleModeType.Arena)
            return;
        
        var list = FormationUtil.GetSubHeroList(Battle.Instance.param.pveParam.FormationIndex, ConfigID);
        if(list.Count <= 0)
            return;

        int index = 0;
        foreach (var VARIABLE in list)
        {
            if(VARIABLE == 0)
                continue;
            
            var info = StaticData.HeroTable.TryGet(VARIABLE);
            if (info != null)
            {
                UiUtil.SetSpriteInBackground(Heads[index], () => info.Head + ".png");
                index++;
            }
        }
        
        if(index == 0)
            return;
        
        this.AssistItem2.SetActive(index >= 2);
        this.SkillPercent1.SetActive(index == 1);
        this.SkillPercent2.SetActive(index >= 2);

        var seq = DOTween.Sequence();
        seq.Append(this.transform.DOLocalMoveX(-this.transform.rectTransform().sizeDelta.x / 2f - 10, 0.6f).SetEase(Ease.OutQuint));
        seq.AppendInterval(1.2f);
        seq.Append(this.transform.DOLocalMoveX(this.transform.rectTransform().sizeDelta.x + 30f, 0.4f).SetEase(Ease.OutQuint));
    }
}