using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class TalkItemInfo
{
    public GameObject go;
    public Text des;
    public Image head;
    public int State;
    public string schedualname;
    public bool isPlot;
    public bool hold;
    public int ConfigID;

    public bool isUp = true;


    public void Init(GameObject go)
    {
        this.go = go;
        this.des = go.transform.Find("BubblePanel/Tip").GetComponent<Text>();
        this.go.transform.localScale = Vector3.one;
        this.head = go.transform.Find("BubblePanel/HeadMask/HeadPortrait").GetComponent<Image>();
    }

    public async void SetContent(string des, int configID, bool isPlot = false)
    {
        des = des.Replace("{username}", Database.Stuff.roleDatabase.Me.name);
        var generator = this.des.cachedTextGeneratorForLayout;
        var setting = this.des.GetGenerationSettings(Vector2.zero);
        float width = generator.GetPreferredHeight(des, setting) / this.des.pixelsPerUnit;
        int size = Mathf.CeilToInt(32 - (width - 1000) / 100);
        size = Mathf.Max(20, size);
        size = Mathf.Min(32, size);
        this.des.fontSize = size;
        this.des.text = des;
        ConfigID = configID;

        var info = StaticData.HeroTable.TryGet(configID);
        if(info == null)
            return;

        var address = "Icon_" + info.Model + ".png";
        UiUtil.SetSpriteInBackground(head,() => address, (useDefault) =>
        {
            // 处理 sprite 找不到的情况
            if(!useDefault)
            {
                this.head.enabled = true;
            }
            else
            {
                this.head.enabled = false;
            };
        });
    }
    
    public async void SetContentTalk(string des, string headimage, bool isPlot = false)
    {
        des = des.Replace("{username}", Database.Stuff.roleDatabase.Me.name);
        var generator = this.des.cachedTextGeneratorForLayout;
        var setting = this.des.GetGenerationSettings(Vector2.zero);
        float width = generator.GetPreferredHeight(des, setting) / this.des.pixelsPerUnit;
        int size = Mathf.CeilToInt(32 - (width - 1000) / 100);
        size = Mathf.Max(20, size);
        size = Mathf.Min(32, size);
        this.des.fontSize = size;
        this.des.text = des;

        var address = "Bubble_" + headimage + ".png";
        UiUtil.SetSpriteInBackground(head,() => address);
    }

    public void Show()
    {
        if (this.go == null)
        {
            return;
        }
        this.go.transform.DOKill(true);
        this.go.transform.localScale = Vector3.one;
        this.go.transform.localPosition = new Vector3(0, 0, 0);
        this.go.transform.DOLocalMoveX(414, 0.8f).SetEase(Ease.OutExpo);
    }

    public void Back()
    {
        if (this.go == null)
        {
            return;
        }
        this.go.transform.DOKill(true);
        this.go.transform.DOLocalMoveX(-300, 0.6f).SetEase(Ease.OutExpo);
    }

    public void Hide()
    {
        if (this.go == null)
        {
            return;
        }
        this.go.transform.localPosition = new Vector3(-300, 0, 0);
    }

    public void Down()
    {
        if (this.go == null)
        {
            return;
        }
        this.go.transform.DOKill(true);
        this.go.transform.DOScale(Vector3.one * 0.8f, 0.2f);
        this.go.transform.DOLocalMove(new Vector3(385, -180, 0), 0.2f);
    }

}