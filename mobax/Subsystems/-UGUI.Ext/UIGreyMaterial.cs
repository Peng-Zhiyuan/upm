using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;
public enum MAT_TYPE
{
    NORMAL = 0,
    GREY = 1,
}

public class UIGreyMaterial : MonoBehaviour
{
   private MAT_TYPE curMaterialType = MAT_TYPE.NORMAL;
   private MaskableGraphic[] renderList;
   private Material greyMaterial = null;
   private List<Material> cacheMaterials = new List<Material>();


    public async Task SwitchGrey(bool isGrey)
   {
       //console.log("SwitchGrey:"+isGrey);
        var newMaterial = isGrey ? MAT_TYPE.GREY : MAT_TYPE.NORMAL;
        await this.SwitchMaterial(newMaterial);
   }

    //public async Task SwitchDarken(bool isDarken)
    //{
    //    var newMaterial = isDarken ? MAT_TYPE.DARKEN : MAT_TYPE.NORMAL;
    //    await this.SwitchMaterial(newMaterial);
    //}

    //public  bool Grey
    //{
    //    get {
    //        return this.curMaterialType == MAT_TYPE.GREY;
    //    }
    //    set{
    //        var newMaterial = value ? MAT_TYPE.GREY : MAT_TYPE.NORMAL;
    //        await this.SwitchMaterial(newMaterial);
    //    }
    //}

    //public bool Darken
    //{
    //    get{
    //        return this.curMaterialType == MAT_TYPE.DARKEN;
    //    }
    //    set{
    //        var newMaterial = value ? MAT_TYPE.DARKEN : MAT_TYPE.NORMAL;
    //        this.SwitchMaterial(newMaterial);
    //    }
      
    //}

    public async Task ResetMat()
    {
        await this.SwitchMaterial(MAT_TYPE.NORMAL);
    }
 
    private async Task Init()
    {
        this.renderList = this.GetComponentsInChildren<MaskableGraphic>(true);
        foreach (var render in this.renderList)
        {
            cacheMaterials.Add(render.material);
        }
        var address = "Mat-Custom-Grey.mat";
        //this.greyMaterial = await AddressableRes.AquireAsync<Material>(address);
        var bucket = BucketManager.Stuff.GetBucket(UIEngine.LatestNavigatePageName);
        this.greyMaterial = await bucket.GetOrAquireAsync<Material>(address);

        //this.materials.Add(greyMaterial);
    }

    public async Task SwitchMaterial(MAT_TYPE materialType)
    {
        this.curMaterialType = materialType;
        if (this.renderList == null)
        {
            await this.Init();
        }
        for(int i = 0; i < this.renderList.Length; i++)
        {
            var render = this.renderList[i];
            if (materialType == MAT_TYPE.GREY)
            {
                render.material = this.greyMaterial;
            }
            else
            {
                render.material = this.cacheMaterials[i];
            }
        }
    }
}
