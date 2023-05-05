using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public partial class LauncherSimpleItem: MonoBehaviour
{
    public void Set(int itemId, int itemCount, int rarity)
    {
        Text_count.text = "" + itemCount;
        
        _SetImage(Image_icon, $"{itemId}.png");
        _SetImage(Image_bg, $"ItemBg_{rarity}.png");
    }
    
    private void _SetAlpha(Graphic graphic, float alpha)
    {
        var c = graphic.color;
        c.a = alpha;
        graphic.color = c;
    }

    private async void _SetImage(Image image, string url)
    {
        _SetAlpha(image, 0);
        var itemImage = await Remote.Stuff.LoadAsync<Sprite>(RemoteLocation.CustomRes, $"{url}");
        image.sprite = itemImage;
        image.DOFade(1, 0.2f);
    }
}