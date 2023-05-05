using System.Collections;
using System.Collections.Generic;
using Spine;
using Spine.Unity;
using UnityEngine;
public class SpinePlayerUnit : RecycledGameObject {
    public SkeletonGraphic skeletonGraphic;
    private SkeletonData skeletonData;
    // public UIGreyMaterial greyMaterial;
    public bool loopAnim =  true;
    public int sdId = 0;
 
#if UNITY_EDITOR

    [Range (0, 100)]
    public int frameIndex = 0;
    public bool framePlay = false;


    void OnValidate () {
        if (!Application.isPlaying || framePlay) {
           
            if (skeletonGraphic == null || skeletonGraphic.skeletonDataAsset == null || skeletonGraphic.Skeleton == null) return;
            skeletonGraphic.AnimationState.SetEmptyAnimation(0,0);
            skeletonData = skeletonGraphic.skeletonDataAsset.GetSkeletonData (true);
            string animName = skeletonGraphic.startingAnimation;
            Spine.Animation anim = this.GetAnimaton (animName);
            if (anim != null) {
                Debug.Log ("set frame:" + frameIndex + "/" + Mathf.FloorToInt (anim.Duration * 30));
                //anim.PoseSkeleton (skeletonGraphic.Skeleton, frameIndex / 30.0f, true);
            }
        
            // skeletonGraphic.freeze = false;
            // skeletonGraphic.Update (frameIndex / 30.0f);
            // skeletonGraphic.freeze = true;
        }
    }

#endif
    public void SetSkeletonDataAsset (SkeletonDataAsset asset, int sdId) {
        if (skeletonGraphic != null) {
            this.sdId = sdId;
            skeletonGraphic.skeletonDataAsset = asset;
            skeletonData = asset.GetSkeletonData (true);
        }
    }

    // public void SwitchGrey(bool grey)
    // {
    //     this.greyMaterial.SwitchGrey(grey);    
    // }

    public void Frozen () {
        skeletonGraphic.Update (0.1f);
        skeletonGraphic.freeze = true;
    }

    public void ResetState () {
        skeletonGraphic.freeze = false;
        skeletonGraphic.color = Color.white;
        //this.greyMaterial.SwitchGrey(false);   
        this.ShowEffects ();
    }

    public void SetSDOffset () {

    }

    public override void OnRecycle () {
        if (skeletonGraphic != null) {
            skeletonGraphic.skeletonDataAsset = null;
        }
    }

    public void HideBoneSlot (string slotName) {
        SeBoneSlotColor (slotName, new Color (0, 0, 0, 0));
    }
    public void ShowBoneSlot (string slotName) {
        SeBoneSlotColor (slotName, new Color (1, 1, 1, 1));
    }

    private List<Slot> hideSlots = new List<Slot> ();

    // public void HideEffects () {
    //     var slots = this.skeletonGraphic.Skeleton.Slots.ToArray ();
    //     for (int i = slots.Length - 1; i >= 0; i--) {
    //         if (slots[i].Data.name.ToUpper ().StartsWith ("EFF")) {
    //             hideSlots.Add (slots[i]);
    //         }
    //     }
    // }

    void LateUpdate () {
        if (hideSlots.Count > 0) {
            //Debug.Log ("hideSlots.Count:" + hideSlots.Count);
            for (int i = 0; i < hideSlots.Count; i++) {
                hideSlots[i].A = 0;
            }
        }
    }
    public void ShowEffects () {
        for (int i = 0; i < hideSlots.Count; i++) {
            hideSlots[i].A = 1;
            //hideSlots[i].SetColor(new Color(0,0,0,0));
        }
        hideSlots.Clear ();
        // var slots = this.skeletonGraphic.Skeleton.Slots.ToArray();
        // for(int i = slots.Length -1 ; i >= 0 ; i--)
        // {
        //     if(slots[i].Data.name.ToUpper().StartsWith("EFF"))
        //     {
        //         slots[i].A = 1;
        //         hideSlots.Remove(slots[i]);
        //     }
        // }
    }
    public void SeBoneSlotColor (string slotName, Color color) {
        //var bone = this.skeletonGraphic.Skeleton.FindBone("Head");
        Spine.Slot slot = this.skeletonGraphic.Skeleton.FindSlot (slotName);
        slot.SetColor (color);

    }

    private void SetByHeroConf(int heroId)
    {
        var heroRow = StaticData.HeroTable[heroId];
        var s = SpineUtil.Instance.GetSDScale(heroRow.Id);
        this.transform.SetLocalScale(s);
        this.transform.localPosition = (Vector3)SpineUtil.Instance.GetSDOffset(heroRow.Id);
    }

    public void Hide () {
        this.gameObject.SetActive (false);
    }

    public void Show (int heroId = 0) {
        if(heroId > 0)
        {
            this.SetByHeroConf(heroId);
        }
        this.gameObject.SetActive (true);
    }

    public List<string> GetAllAnimatonNames () {
        List<string> animNames = new List<string> ();
        var animList = skeletonData.Animations;
        foreach (var anim in animList) {
            animNames.Add (anim.Name);
        }
        return animNames;
    }

    public Spine.Animation GetAnimaton (string name) {
        return skeletonData.FindAnimation (name);
    }
    // bool test = true;
    public float PlayAnim (string anim, bool loop) {
        if (skeletonGraphic != null && skeletonGraphic.AnimationState != null) {
            // test = !test;
            // if(test)
            // {
            //     this.HideEffectSlots();
            // }
            // else{
            //     this.ShowEffectSlots();
            // }
            TrackEntry entry = skeletonGraphic.AnimationState.SetAnimation (0, anim, loop);
            this.loopAnim = loop;
            if(loop)
            {
                skeletonGraphic.AnimationState.TimeScale = 1/Time.timeScale;
            }
            //TODO fix
            //return entry.animation.Duration;
        }
        return 0.0f;
    }

    public float PlayAnimRedirect (string anim, bool loop) {
        // if (sdId > 0) {
        //     switch (anim) {
        //         case "Wait":
        //             anim = SdAnim.Instance.GetWaitAnim (sdId);
        //             break;
        //         case "Walk":
        //             anim = SdAnim.Instance.GetWalkAnim (sdId);
        //             break;
        //         case "Atk":
        //             anim = SdAnim.Instance.GetAtkAnim (sdId, Vector2Int.right);
        //             break;
        //         case "Die":
        //             anim = SdAnim.Instance.GetDieAnim (sdId);
        //             break;
        //         case "Dmg":
        //             anim = SdAnim.Instance.GetDmgAnim (sdId);
        //             break;
        //         case "Lose":
        //             anim = SdAnim.Instance.GetLoseAnim (sdId);
        //             break;
        //         case "Win":
        //             anim = SdAnim.Instance.GetWinAnim (sdId);
        //             break;
        //         default:
        //             break;
        //     }
        // }
        return this.PlayAnim (anim, loop);
    }
}