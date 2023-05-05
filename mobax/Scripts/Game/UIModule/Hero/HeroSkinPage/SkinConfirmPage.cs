using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System;
public partial class SkinConfirmPage : Page
{
    private TaskCompletionSource<bool> tcs;
    public override async Task OnNavigatedToPreperAsync(PageNavigateInfo navigateInfo) 
    {
        var oldAvatarInfo = HeroDisplayer.Hero.AvatarInfo;
        var newAvatarInfo = HeroDressData.Instance.CurrentAvatarInfo;
        var dataList = HeroDressData.Instance.HairDic[HeroDisplayer.Hero.HeroId];
        int selIndex1 = dataList.FindIndex((row) =>
        {
            return row.Id == oldAvatarInfo.Item3;
        });
        selIndex1 = Mathf.Max(0, selIndex1);
        int selIndex2 = dataList.FindIndex((row) =>
        {
            return row.Id == newAvatarInfo.Item3;
        });
        selIndex2 = Mathf.Max(0, selIndex2);
        CommonItem item1 = new CommonItem();
        item1.Id = dataList[selIndex1].Id;
        item1.Icon = dataList[selIndex1].Icon;
        item1.Num = null;
        this.ItemView1.Set(item1);

        CommonItem item2 = new CommonItem();
        item2.Id = dataList[selIndex2].Id;
        item2.Icon = dataList[selIndex2].Icon;
        item2.Num = null;
        this.ItemView2.Set(item2);
        //this.HairIcon01.sprite = await this.PageBucket.GetOrAquireSpriteAsync($"{ dataList[selIndex1].Icon}.png");
        //this.HairIcon02.sprite = await this.PageBucket.GetOrAquireSpriteAsync($"{ dataList[selIndex2].Icon}.png");

    }


    public Task<bool> AskConfirmAsync()
    {
        tcs = new TaskCompletionSource<bool>();
        return tcs.Task;
    }
    public void OnConfirm()
    {
        tcs.SetResult(true);
        UIEngine.Stuff.Back();
      
       
    }

    public void OnCancel()
    {
        tcs.SetResult(false);
        UIEngine.Stuff.Back();
       

    }
}
