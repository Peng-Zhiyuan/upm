using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.UI;

public partial class ItemView_Skin : MonoBehaviour
{
    ItemViewData data;

    /**
     * number 可以是物品组 id，也可以是物品 id，指定 id 的模式用来方便处理没有物品实例的情况
     */
    public void Bind(ItemViewData data)
    {
        this.data = data;
        this.Refresh();
    }

    void Refresh()
    {
        this.RefreshCount();
        this.RefreshQualityColor();
        this.RefreshIcon();
    }

    void RefreshQualityColor()
    {
        var quality = this.data.Quality;
        UiUtil.SetAtlasSpriteInBackground(Image_bg, "PageGroup2Atlas.spriteatlas", () => $"ItemBg_{quality}");
    }

    void RefreshCount()
    {
        if (this.data.Count != null)
        {
            var count = data.Count.Value;
            var text = NumberUtil.ToThousandSeparatorFormat(count);
            this.Text_count.text = text;
            this.ItemGroup.gameObject.SetActive(true);
        }
        else
        {
            this.ItemGroup.gameObject.SetActive(false);
        }
    }

    string IconAddress
    {
        get
        {
            var iconName = this.data.IconName;
            return $"{iconName}.png";
        }
    }

    void RefreshIcon()
    {
        if (IconAddress.Equals(".png"))
        {
            Debug.LogError("Cant find the item " + this.data.RowId + " icon path");
            this.Image_defaultIcon.gameObject.SetActive(true);
            this.Image_icon.gameObject.SetActive(false);
            return;
        }
        UiUtil.SetSpriteInBackground(this.Image_icon, () => IconAddress, (isDefault) =>
                        {
                            if (isDefault)
                            {
                                this.Image_defaultIcon.gameObject.SetActive(true);
                                this.Image_icon.gameObject.SetActive(false);
                            }
                            else
                            {
                                this.Image_defaultIcon.gameObject.SetActive(false);
                                this.Image_icon.gameObject.SetActive(true);
                            }
                        }
        );
    }
}