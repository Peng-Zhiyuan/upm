using System;
using FancyScrollView;
using FancyScrollView.Example02;
using UnityEngine;
using UnityEngine.UI;
using System;

public class SkinListCellViewContext
{
    public int SelectedIndex = -1;
    public Action<int> OnCellClicked;
}

public class SkinListCellViewData
{
    public AvatarRow _avatarRow { get; }

    public SkinListCellViewData(AvatarRow avatarRow, Action refreshInfoAction)
    {
        _avatarRow = avatarRow;
    }
}

public partial class SkinListCellView : FancyCell<SkinListCellViewData, SkinListCellViewContext>
{
    [SerializeField] Animator animator = default;
    private AvatarRow _avatarRow;
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

   

    public override async void UpdateContent(SkinListCellViewData itemData)
    {
        
        this._avatarRow = itemData._avatarRow;
        //{this._avatarRow.Icon.ToString().PadLeft(2,'0')}
        // this.DressIcon.transform.localPosition = Vector3.up * (-50);
       
       /* if (!string.IsNullOrEmpty(itemData._avatarRow.Icon))
        {
            this.DressIcon.sprite = await this.Bucket.GetOrAquireSpriteAsync($"{this._avatarRow.Icon}.png");
            //this.DressIcon.sprite = await this.Bucket.GetOrAquireSpriteAsync($"HalfCard_{this._avatarRow.heroId}_01.png");
        }
        else
        {
            this.DressIcon.sprite = await this.Bucket.GetOrAquireSpriteAsync($"HalfCard_{this._avatarRow.heroId}.png");
        }
  */
       
        //this.DressIcon.SetNativeSize();
        if (this._avatarRow.Id == 0)
        {
            SkinName.SetActive(false);
            CharacterSkinLockMask.SetActive(false);
        }
        else 
        {
            ItemInfo info = Database.Stuff.itemDatabase.GetFirstItemInfoOfRowId(this._avatarRow.Id);
            if (info != null)
            {
                SkinName.SetActive(true);
                SkinName.text = LocalizationManager.Stuff.GetText(this._avatarRow.Name);
                CharacterSkinLockMask.SetActive(false);
            }
            else
            {
                SkinName.SetActive(false);
                CharacterSkinLockMask.SetActive(true);
            }
        }
        this.DressIcon.sprite = await this.Bucket.GetOrAquireSpriteAsync($"{this._avatarRow.Icon}.png");
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