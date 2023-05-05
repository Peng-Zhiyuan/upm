using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;
using System;
public partial class HairCellView : MonoBehaviour
{

    public HairColorRow hairRow;
  
    private Bucket Bucket => BucketManager.Stuff.GetBucket(UIEngine.LatestNavigatePageName);
    public async void Bind(HairColorRow hr)
    {
        hairRow = hr;
        await this.Refresh();
    }

    public bool Selected
    {
        set
        {
            this.SelectFrame.enabled = value;
        }
    }

    public async Task Refresh()
    {
        if (this.hairRow.Id == 0)
        {
            CharacterSkinLockMask.SetActive(false);
        }
        else
        {
            bool canUsed = HeroDressData.Instance.IsHairCanUse(this.hairRow.Id);
            if(canUsed)
            {
                CharacterSkinLockMask.SetActive(false);
            }
            else
            {
                CharacterSkinLockMask.SetActive(true);
            }
        }
        ItemInfo info = Database.Stuff.itemDatabase.GetFirstItemInfoOfRowId(this.hairRow.Id);
        if (info != null && info.val > 0) this.HairCount.text = $"x{info.val}";
        else this.HairCount.text = "";
        this.HairIcon.sprite = await this.Bucket.GetOrAquireSpriteAsync($"{this.hairRow.Icon}.png");
    }

    public Action<HairCellView> onClick;
    public void OnButton(string msg)
    {
        if (msg == "click")
        {
            onClick?.Invoke(this);
        }
    }
}
