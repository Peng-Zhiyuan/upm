using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using System.Threading.Tasks;
public class SkinUnit : MonoBehaviour
{
    public Image SkinIcon;
    //public Text SkinName;
    public async void Set(int id)
    {
        var bucket = BucketManager.Stuff.GetBucket(UIEngine.LatestNavigatePageName);
        if (StaticData.AvatarTable.ContainsKey(id))
        {
            AvatarRow ar = StaticData.AvatarTable[id] as AvatarRow;
            this.SkinIcon.sprite = await bucket.GetOrAquireSpriteAsync($"{ar.Icon}.png");
        }
        else if (StaticData.ClothColorTable.ContainsKey(id))
        {
            ClothColorRow ccr = StaticData.ClothColorTable[id] as ClothColorRow;
            this.SkinIcon.sprite = await bucket.GetOrAquireSpriteAsync($"{ccr.Icon}.png");
        }
    }

}
