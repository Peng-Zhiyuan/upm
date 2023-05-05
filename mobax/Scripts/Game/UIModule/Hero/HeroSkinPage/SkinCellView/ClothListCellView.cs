﻿using System;
using FancyScrollView;
using FancyScrollView.Example02;
using UnityEngine;
using UnityEngine.UI;
using System;

public class ClothListCellViewContext
{
    public int SelectedIndex = -1;
    public Action<int> OnCellClicked;
}

public class ClothListCellViewData
{
    public ClothColorRow _clothRow { get; }

    public ClothListCellViewData(ClothColorRow colthColorRow, Action refreshInfoAction)
    {
        _clothRow = colthColorRow;
    }
}

public partial class ClothListCellView : FancyCell<ClothListCellViewData, ClothListCellViewContext>
{
    [SerializeField] Animator animator = default;
    private ClothColorRow _clothRow;
    private Bucket Bucket => BucketManager.Stuff.GetBucket(UIEngine.LatestNavigatePageName);
    static class AnimatorHash
    {
        public static readonly int CellAni = Animator.StringToHash("scroll");
    }

    public override void Initialize()
    {
        //Debug.LogError("Initialize");
        this.transform.localPosition = Vector3.zero;
        this.ClueUncheck.onClick.AddListener(() => Context.OnCellClicked?.Invoke(Index));

    }

   

    public override async void UpdateContent(ClothListCellViewData itemData)
    {
        
        
        this._clothRow = itemData._clothRow;
        //{this._avatarRow.Icon.ToString().PadLeft(2,'0')}
        // this.DressIcon.transform.localPosition = Vector3.up * (-50);
       

        if (this._clothRow.Id == 0)
        {
            SkinName.SetActive(false);
            CharacterSkinLockMask.SetActive(false);
        }
        else
        {
            ItemInfo info = Database.Stuff.itemDatabase.GetFirstItemInfoOfRowId(this._clothRow.Id);
            if (info != null)
            {
                SkinName.SetActive(true);
                SkinName.text = LocalizationManager.Stuff.GetText(this._clothRow.Name);
                CharacterSkinLockMask.SetActive(false);
            }
            else
            {
                SkinName.SetActive(false);
                CharacterSkinLockMask.SetActive(true);
            }
        }
        this.DressIcon.sprite = await this.Bucket.GetOrAquireSpriteAsync($"{this._clothRow.Icon}.png");
    }


    public override void UpdatePosition(float position)
    {
        currentPosition = position;

        if (animator.isActiveAndEnabled)
        {
            animator.Play(AnimatorHash.CellAni, -1, position);
        }

        animator.speed = 0;
    }

    // GameObject が非アクティブになると Animator がリセットされてしまうため
    // 現在位置を保持しておいて OnEnable のタイミングで現在位置を再設定します
    float currentPosition = 0;

    void OnEnable() => UpdatePosition(currentPosition);
}