using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BattleEngine.Logic;
using BattleSystem.ProjectCore;
using DG.Tweening;
using Modules.Battle.BattleSystem.Page;
using ProtoBuf;
using Spine.Unity;
using SpineRegulate;
using UnityEngine;
using UnityEngine.UI;

public partial class HeroItemView : MonoBehaviour
{
    public GameObject Cell;
    //public Image IconMask;
   
    //public GameObject GuideLittle;
    //public GameObject Guide;
    //public Image Icon;
    //public BufferUIShow BufferShow;
    public int index;
    public Creature Role;
    public BattlePage Owner;
    //public GameObject Effect;
    //public Material _Material;
    public BufferBar BufferBar;
    //public GameObject VimFullEffect;
    //public GameObject VimFullEffect;
    //public RecycledGameObject VimEffect;
    //public GameObject Protect;
    public float SpineRootBeginY;
    public float SpineRootBeginSizeY;
    //public GameObject Defence;
    
    public bool IsSelected;
    public bool IsFocusing;

    //public UISpineUnit UISpineUnit;

    //public bool IsMiddle { get; set; }

    private bool drag = false;
    private Vector2 beginDragPos;
    
    public static HeroItemView LastDefenceTarget = null;
    public static HeroItemView LastSelectItem = null;

    public Transform GetHeadClick()
    {
        return GuildButton.transform;
    }
    
    public Transform GetDefenceButton()
    {
        return DefenceClick.transform;
    }
    public void DefenceTarget()
    {
        //DefenceTag.SetActive(true);
        UiUtil.SetActive(this.DefenceTag.gameObject, true);
        HideDefence();
        LastDefenceTarget = this;

        if(DefFocusEnemyView.LastFocusItem != null)
            DefFocusEnemyView.LastFocusItem.NoFocus();
        
        if(FocusEnemyView.LastFocusItem != null)
            FocusEnemyView.LastFocusItem.NoFocus();

        StartDefenceCD();
    }

    public void StartDefenceCD()
    {
        string name = "DefenceTimer" + index;
        TimerMgr.Instance.Remove(name);
        TimerMgr.Instance.BattleSchedulerTimerDelay((float)BattleUtil.GetGlobalK(GlobalK.DefFocusOnFiretime_40), delegate
        {
            HideDefence();
        }, false, name);
    }

    public void HideDefenceTag()
    {
        UiUtil.SetActive(this.DefenceTag.gameObject, false);
        string name = "DefenceTimer" + index;
        TimerMgr.Instance.Remove(name);

        if (LastSelectItem == this)
        {
            ShowDefenceTag();
        }
    }

    public static void HideDefence()
    {
        if (LastDefenceTarget != null)
        {
            LastDefenceTarget.HideDefenceTag();
            LastDefenceTarget = null;
            CheckLastSelectDef();
        }
    }

    public static void CheckLastSelectDef()
    {
        if (LastSelectItem != null)
        {
            LastSelectItem.ShowDefenceTag();
        }
    }

    public void ShowDefenceTag()
    {
        //this.DefenceClick.SetActive(!this.DefenceTag.GetComponent<GameObjectExt>().IsVis);
        
        UiUtil.SetActive(this.DefenceClick.gameObject, !this.DefenceTag.GetComponent<GameObjectExt>().IsVis);
    }

    public void SetDefenceVis(bool vis)
    {
        //this.DefenceClick.SetActive(vis);
        UiUtil.SetActive(this.DefenceClick.gameObject, vis);
        if (!vis)
        {
            this.DefenceClick.transform.localScale = Vector3.zero;
        }
        else
        {
            this.DefenceClick.transform.localScale = Vector3.one;
        }
    }
    public void SetData(Creature param_Role)
    {
        this.Role = param_Role;
    }

    private bool IsFriend = false;

    public void Init( int index, BattlePage owner)
    {
        this.index = index;
        Cell = this.gameObject;
        var go = this.gameObject;
        this.Owner = owner;
        var trans = go.transform;
        go.name = index.ToString();
        //this.DefenceTag.SetActive(false);
        UiUtil.SetActive(this.DefenceTag.gameObject, false);
        
        //info.Name = trans.Find("Normal/Name").GetComponent<Text>();
        //Protect = trans.Find("Root/Protect").gameObject;
        //Effect = go.transform.Find("Root/HeadMask/JoinEffect").gameObject;
        //Defence = go.transform.Find("Root/Defence").gameObject;
        //var buffer_root = trans.Find("BufferItems");

        //UISpineUnit = go.transform.Find("UISpineUnit").GetComponent<UISpineUnit>();
        BufferBar = new BufferBar(BufferItems.transform, "RoleBuffer.prefab");
        SpineRootBeginSizeY = SpineGraphic.transform.parent.GetComponent<RectTransform>().sizeDelta.y;
        SpineRootBeginY = SpineGraphic.transform.parent.GetComponent<RectTransform>().anchoredPosition3D.y;
        HeadClick.name = index.ToString();
        var longController = HeadClick.GetComponent<DragLongPressController>();
        /*longController.AddDoubleClick(go =>
        {
            //Debug.LogError("双击：" + go.name);

            var index = int.Parse(go.transform.parent.name);
            if (index >= this.Owner.MainHeros.Count)
                return;

            var role = this.Owner.MainHeros[index];
            this.Owner.CastSuperSkill(role, false);
        });*/
        longController.AddClickUp((go, go2, pos) =>
                        {
                            if (Role == null
                                || Role.mData.IsDead)
                                return;

                            if (drag)
                            {
                                if (pos.y - beginDragPos.y < BattlePage.SuperSkillMoveOffset)
                                    return;
                                if (Role.mData.CurrentMp.Value < Role.mData.CurrentMp.MaxValue)
                                    return;
                                if (IsFriend)
                                {
                                    this.Owner.SendFriendToBattle();
                                    //DisableMask.SetActive(true);
                                    UiUtil.SetActive(this.DisableMask.gameObject, true);
                                    //VimFullEffect.SetActive(false);
                                    //this.VimFullBackGround.SetActive(false);
                                    //this.VimFullRoot.SetActive(false);

                                    UiUtil.SetActive(this.VimFullBackGround.gameObject, false);
                                    UiUtil.SetActive(this.VimFullRoot.gameObject, false);
                                }
                                else
                                {
                                    if (!Role.mData.IsWaitSPSkillState)
                                    {
                                        this.Owner.CastSuperSkill(Role, false);
                                        //SuperRelease2.SetActive(true);
                                        UiUtil.SetActive(this.SuperRelease2.gameObject, true);
                                        TimerMgr.Instance.BattleSchedulerTimer(1f, delegate
                                        {
                                            //SuperRelease2.SetActive(false);
                                            UiUtil.SetActive(this.SuperRelease2.gameObject, false);
                                        });
                                    }
                                }
                            }
                            else
                            {
                                this.Owner.CameraClick(HeadClick.gameObject);
                            }
                        }
        );
        longController.AddClickDown((go, pos) =>
                        {
                            drag = false;

                            //Debug.LogError("单击：" + go.name);
                            if (Role == null
                                || Role.mData.IsDead)
                                return;

                            /*if (Role.Selected)
                            {
                                this.Owner.CastSuperSkill(Role, false);
                            }
                            else
                            {
                                this.Owner.CameraClick(go.transform.parent.gameObject);
                            }*/
                            if (IsFocusing)
                            {
                                BattleEngine.Logic.BattleManager.Instance.DefendFocusOnFiring(Role.mData.UID);
                                owner.ShowFocusVis(false);
                                WwiseEventManager.SendEvent(TransformTable.Custom, "FocusDefenceLine");
                            }
                            else
                            {
                                //this.Owner.CastSuperSkill(Role, false);
                                //this.Owner.CameraClick(go.transform.parent.gameObject);
                            }
                        }
        );
        longController.AddBeginDragEvent((go, pos) =>
                        {
                            if (Role == null
                                || Role.mData.IsDead)
                                return;
                            if (Battle.Instance.IsArenaMode)
                                return;
                            drag = true;
                            beginDragPos = pos;
                        }
        );
        /*longController.AddDragEvent((go, pos, raycastTarget) =>
                        {
                            if (drag == false)
                                return;
                            if (Role == null
                                || Role.mData.IsDead)
                                return;
                            if (pos.y - beginDragPos.y < BattlePage.SuperSkillMoveOffset)
                                return;
                            if (Role.mData.CurrentMp.Value < Role.mData.CurrentMp.MaxValue)
                                return;
                            if (IsFriend)
                            {
                                this.Owner.SendFriendToBattle();
                                DisableMask.SetActive(true);
                                //VimFullEffect.SetActive(false);
                                this.VimFullBackGround.SetActive(false);
                                this.VimFullRoot.SetActive(false);
                            }
                            else
                            {
                                if (!Role.mData.IsWaitSPSkillState)
                                {
                                    this.Owner.CastSuperSkill(Role, false);
                                    /*SuperRelease2.SetActive(true);
                                    TimerMgr.Instance.BattleSchedulerTimer(1f, delegate { SuperRelease2.SetActive(false); });#1#
                                }
                                //this.PlaySuperSkillAnim();
                            }
                        }
        );*/
        ButtonUtil.SetClick(HeadClick, () =>
                        {
                            //this.CameraClick(go);
                        }
        );
        ButtonUtil.SetClick(DefenceClick, () =>
            {
                //this.DefenceClick.SetActive(false);
                UiUtil.SetActive(this.DefenceClick.gameObject, false);
                DefenceTarget();
                            BattleManager.Instance.DefendFocusOnFiring(Role.mData.UID);
                            //owner.ShowFocusVis(false);
                            WwiseEventManager.SendEvent(TransformTable.Custom, "FocusDefenceLine");
                            //owner.ResetFocusCD();
                            BattleDataManager.Instance.UseItemByType(3);
                        }
        );
        Cell.SetActive(false);
        //UiUtil.SetActive(this.Cell.gameObject, false);
    }

    public void RefreshBuffer()
    {
        if (this.Role == null)
            return;
        BufferBar.SetData(this.Role.mData);
    }

    public async Task PlaySuperSkillAnim()
    {
        Transform root = Owner.GetSuperSkillEffect();
        
        root.SetParent(this.transform);
        root.localPosition = Vector3.zero;
        root.transform.GetComponent<Animator>().Play("fx_ui_spine_offset", 0, 0);
        var spine = Owner.GetSuperSkillGraph();
        
        HeroRow hero = StaticData.HeroTable.TryGet(Role.ConfigID);
        if (hero == null)
            return;
        string headspine = "-30.2_-849.2_0.3";
        if (!string.IsNullOrEmpty(hero.Headspine))
        {
            headspine = hero.Headspine;
        }
        string[] strs = headspine.Split('_');
        var pos = new Vector3(float.Parse(strs[0]), float.Parse(strs[1]), 0);
        spine.GetComponent<RectTransform>().anchoredPosition3D = pos;
        spine.transform.localScale = Vector3.one * float.Parse(strs[2]);
        
        var bucket = BucketManager.Stuff.GetBucket(UIEngine.LatestNavigatePageName);
        var data = await bucket.GetOrAquireAsync<SkeletonDataAsset>("Live_" + Role.mData.ConfigID + "_SkeletonData.asset", true);
        if (data == null)
        {
            return;
        }
        //root.SetActive(true);
        UiUtil.SetActive(root.gameObject, true);
        UiUtil.SetSkeleton(spine, data, "idle", true, 0);
        //UiUtil.SetSkeletonInBackground(spine, () => "Live_" + Role.mData.ConfigID + "_SkeletonData.asset");
        TimerMgr.Instance.BattleSchedulerTimer(1.6f, delegate
        {
            //root.SetActive(false);
            UiUtil.SetActive(root.gameObject, false);
        });
    }

    private bool isInit = false;
    public async void ResetData(Creature role)
    {
        if(isInit && !role.mData.IsDead)
            return;
        
        //VimFullEffect.SetActive(false);
        //this.VimFullBackGround.SetActive(false);
        this.Role = role;
        //this.VimFullRoot.SetActive(false);
        
        UiUtil.SetActive(VimFullBackGround.gameObject, false);
        UiUtil.SetActive(VimFullRoot.gameObject, false);
        
        RefreshBuffer();
        if (role != null)
        {
            //Icon.material = role.mData.IsDead ? _Material : null;
            /*SpineGraphic.GetComponent<SkeletonGraphicUnit>().SetGrey(role.mData.IsDead);
            SpineGraphic.enabled = false;
            SpineGraphic.enabled = true;*/
            if (role.mData.IsDead)
            {
                SpineGraphic.color = ColorUtil.HexToColor("DB7979");
                Owner.StopSpineAnimation(SpineGraphic);
            }
            else
            {
                SpineGraphic.color = Color.white;
            }

            //-39.5_-911.2_0.33
            //SpineGraphic.GetComponent<Animation>()
            //UiUtil.SetSkeletonInBackground(SpineGraphic, () => "shubeier_SkeletonData.asset");
            //Dead.SetActive(role.mData.IsDead);
            UiUtil.SetActive(Dead.gameObject, role.mData.IsDead);
            //UiUtil.SetSpriteInBackground(this.Icon, () => "Icon_" + Role.mData.ConfigID + ".png");
            HeroRow hero = StaticData.HeroTable.TryGet(Role.ConfigID);
            if (hero == null)
                return;
            string headspine = "-30.2_-849.2_0.3";
            if (!string.IsNullOrEmpty(hero.Headspine))
            {
                headspine = hero.Headspine;
            }
            string[] strs = headspine.Split('_');
            var pos = new Vector3(float.Parse(strs[0]), float.Parse(strs[1]), 0);
            SpineUnitBeginY = pos.y;
            if (!Role.Selected)
            {
                //SpineGraphic.GetComponent<RectTransform>().anchoredPosition3D = pos;
                //ResetPosY(this.SpineGraphic.transform, SpineUnitBeginY + 50);
            }
            else
            {
                pos.y += 50f;
            }

            if (!isInit)
            {
                SpineGraphic.GetComponent<RectTransform>().anchoredPosition3D = pos;
                isInit = true;
            }
            else
            {
                ResetPosY(this.SpineGraphic.transform, pos.y, 0.03f);
            }
            //SpineGraphic.GetComponent<RectTransform>().anchoredPosition3D.x = pos.x;
            //SpineGraphic.GetComponent<RectTransform>().anchoredPosition3D = pos;
            //ResetPosY(this.SpineGraphic.transform, pos.y, 0.03f);
            //SpineGraphic.GetComponent<RectTransform>().anchoredPosition3D = pos;
            SpineGraphic.transform.localScale = Vector3.one * float.Parse(strs[2]);

            //SpineRootBeginSizeY = SpineGraphic.transform.parent.GetComponent<RectTransform>().sizeDelta.y;
            //SpineRootBeginY = SpineGraphic.transform.parent.GetComponent<RectTransform>().anchoredPosition3D.y;
            UiUtil.SetSkeletonInBackground(SpineGraphic, () => "Live_" + Role.mData.ConfigID + "_SkeletonData.asset");
            if (hero.Job == 0)
                return;
            UiUtil.SetSpriteInBackground(this.Job, () => "Icon_occ" + hero.Job + ".png");
            if (hero.Element != 0)
            {
                UiUtil.SetSpriteInBackground(this.Att, () => "element_" + hero.Element + ".png");
            }
            Level.text = string.Format("{0:00}", role.mData.battleItemInfo.lv);

            //UISpineUnit.Bind(role.ConfigID, ESpineTemplateType.HalfModel);
        }
        else
        {
            this.transform.parent.SetActive(false);
            //UiUtil.SetActive(transform.parent.gameObject, false);
        }
        /*//CastSuperSkill(player);
        var obj = await BuketManager.Stuff.Main.GetOrAquireAsync<SkeletonDataAsset>("shubeier_SkeletonData.asset", true);
        if (obj != null)
        {
            SpineGraphic.skeletonDataAsset = obj;
            SpineGraphic.Initialize(true);
            //SpineGraphic.AnimationState.SetAnimation(0, "fennu", true);
        }
        */
        UpdateData();
    }

    private void StartFullAnim()
    {
        /*DOTween.Kill($"VimFloat{Cell.name}");

        float t = 0.2f;
        var seq = DOTween.Sequence();
        seq.Append(this.Root.DOLocalMoveY(15f, t));
        seq.Append(this.Root.DOLocalMoveY(-15f, 2*t));
        seq.Append(this.Root.DOLocalMoveY(0f, t));
        seq.SetLoops(-1);
        seq.SetId($"VimFloat{Cell.name}");*/
    }

    private void ResetSize(Transform obj, float offset, float durT = 0.5f)
    {
        var rect = obj.GetComponent<RectTransform>();
        var size = rect.sizeDelta;
        DOTween.To(() => rect.sizeDelta.y, x =>
                        {
                            size.y = x;
                            rect.sizeDelta = size;
                        }, offset, durT
        );
    }

    private void ResetPosY(Transform obj, float offset, float durT = 0.5f)
    {
        var rect = obj.GetComponent<RectTransform>();
        var pos = rect.anchoredPosition3D;
        DOTween.To(() => pos.y, x =>
                        {
                            pos.y = x;
                            rect.anchoredPosition3D = pos;
                        }, offset, durT
        );
    }
    
    private void ResetScale(Transform obj, float offset, float durT = 0.5f)
    {
        var scale = obj.localScale;
        DOTween.To(() => scale.y, x =>
            {
                scale.y = x;
                obj.localScale = scale;
            }, offset, durT
        );
    }

    private void ResetRect(Transform obj, float offset)
    {
        var size = obj.GetComponent<RectTransform>().sizeDelta;
        obj.GetComponent<RectTransform>().sizeDelta = new Vector2(size.x, size.y + offset);
        var pos = obj.GetComponent<RectTransform>().anchoredPosition3D;
        pos.y = pos.y + offset / 2;
        obj.GetComponent<RectTransform>().anchoredPosition3D = pos;
    }
    

    public void Disable()
    {
        //Protect.gameObject.SetActive(true);
        UiUtil.SetActive(Protect.gameObject, true);
    }

    /*public void Die()
    {
        Icon.material = Role.mData.IsDead ? _Material : null;
    }*/

    private string[] VimEffectNames = new string[] { "fx_ui_roleitem_headmask_1.prefab", "fx_ui_roleitem_headmask_2.prefab", "fx_ui_roleitem_headmask_3.prefab", "fx_ui_roleitem_headmask_4.prefab", "fx_ui_roleitem_headmask_5.prefab", "fx_ui_roleitem_headmask_6.prefab", };

    public async void UpdateData()
    {
        //Cell.name = index.ToString();
        //head.Name.text = info.ID;
        if (!SceneObjectManager.IsAccessable)
        {
            return;
        }
        //Empty.SetActive(Role == null);
        UiUtil.SetActive(Empty.gameObject, Role == null);
        if (Role != null)
        {
            if (Role.mData.CurrentHealth.MaxValue == 0)
            {
                Cell.SetActive(false);
                //UiUtil.SetActive(Cell.gameObject, false);
                return;
            }
            Cell.SetActive(true);
            //UiUtil.SetActive(Cell.gameObject, true);
            Hp.fillAmount = Role.mData.CurrentHealth.Value * 1.0f / Role.mData.CurrentHealth.MaxValue;
            Vim.fillAmount = Role.mData.CurrentVim.Value * 1.0f / Role.mData.CurrentVim.MaxValue;
            VimBar.fillAmount = Role.mData.CurrentMp.Value * 1.0f / Role.mData.CurrentMp.MaxValue;
            //Full.SetActive(Role.mData.CurrentMp.Value >= Role.mData.CurrentMp.MaxValue && !Role.mData.IsDead);
            if (!Role.Selected)
            {
                //Cell.transform.localScale = Vector3.one * 0.7f;
                UnSelect();
            }
            else
            {
                //Cell.transform.localScale = Vector3.one;
                Select();
            }
            bool isFull = Role.mData.CurrentMp.Value >= Role.mData.CurrentMp.MaxValue && !Role.mData.IsDead;
            this.DisbaleSkill.SetActive(isFull && !BattleUtil.IsCanExecuteSPSkill(Role.mData));
            
            if (isFull)
            {
                if (this.VimFullRoot.GetComponent<GameObjectExt>().IsVis)
                    return;
                if (!Role.mData.IsDead)
                {
                    if (Battle.Instance.GameMode.ModeType == BattleModeType.Guard
                        && Role.ConfigID == BattleConst.SiLaLiID)
                        return;
                    
                    if(Battle.Instance.IsArenaMode)
                        return;
                    /*var info = StaticData.HeroTable.TryGet(Role.ConfigID);
                    VimEffect = await GameObjectPoolUtil.ReuseAddressableObjectAsync<RecycledGameObject>(BucketManager.Stuff.Main, VimEffectNames[4]);
                    VimEffect.transform.SetParent(VimEffectRoot);
                    VimEffect.transform.localPosition = Vector3.zero;
                    VimEffect.transform.localScale = new Vector3(0.9f, 1.6f, 1.5f);*/
                    //VimFullEffect.transform.localPosition = Vector3.zero;
                    //VimFullEffect.SetActive(true);
                    //this.VimFullRoot.SetActive(true);
                    //this.VimFullBackGround.SetActive(true);
                    UiUtil.SetActive(VimFullRoot.gameObject, true);
                    UiUtil.SetActive(VimFullBackGround.gameObject, true);
                    //Select();
                }
            }
            else
            {
                //VimFullEffect.SetActive(false);
                //this.Full.SetActive(false);
                //this.VimFullRoot.SetActive(false);
                //this.VimFullBackGround.SetActive(false);
                UiUtil.SetActive(VimFullRoot.gameObject, false);
                UiUtil.SetActive(VimFullBackGround.gameObject, false);
            }
        }
        else
        {
            Cell.SetActive(false);
            //UiUtil.SetActive(Cell.gameObject, false);
        }
    }

    private float SpineUnitBeginY = 0f;
    public void Select()
    {
        if (IsSelected || IsFocusing)
            return;

        LastSelectItem = this;
        //this.VimFullRoot.localScale = VimFullScale;
        ResetScale(this.VimFullRoot.transform, 1.1f);
        
        IsSelected = true;

        float offset = 150f;
        float offset_root = 110f;
        ResetSize(SpineGraphic.transform.parent, SpineRootBeginSizeY + offset_root);
        ResetPosY(SpineGraphic.transform.parent, SpineRootBeginY + offset_root / 2);
        
        ResetPosY(this.SpineGraphic.transform, SpineUnitBeginY + 50);

        ResetPosY(this.VimFullBackGround.transform, 260f);

        offset_root = 112f;
        ResetSize(this.BattleBigMoveBar02.transform, this.BattleBigMoveBar02_Region.transform.GetComponent<RectTransform>().sizeDelta.y + offset_root);
        ResetPosY(this.BattleBigMoveBar02.transform, this.BattleBigMoveBar02_Region.transform.GetComponent<RectTransform>().anchoredPosition3D.y + offset_root/ 2f);
        

        ResetSize(this.BattleRolePanel02.transform, this.BattleRolePanel02_Region.transform.GetComponent<RectTransform>().sizeDelta.y + offset_root);
        ResetPosY(this.BattleRolePanel02.transform, this.BattleRolePanel02_Region.transform.GetComponent<RectTransform>().anchoredPosition3D.y + offset_root/ 2f);

        ResetSize(this.VimFullEffect.transform, this.VimFullEffect_Region.transform.GetComponent<RectTransform>().sizeDelta.y + offset_root);
        ResetPosY(this.VimFullEffect.transform, this.VimFullEffect_Region.transform.GetComponent<RectTransform>().anchoredPosition3D.y + offset_root/ 2f);

        ResetSize(this.VimBar.transform, this.VimBar_Region.transform.GetComponent<RectTransform>().sizeDelta.y + offset_root);
        ResetPosY(this.VimBar.transform, this.VimBar_Region.transform.GetComponent<RectTransform>().anchoredPosition3D.y + offset_root/ 2f);

        VimBar.sprite = VimFullEffect_Region.sprite;
        
        //ResetPosY(Full.transform, 100);
        //this.DefenceClick.SetActive(!this.DefenceTag.GetComponent<GameObjectExt>().IsVis && !Battle.Instance.IsArenaMode);
        UiUtil.SetActive(DefenceClick.gameObject, !this.DefenceTag.GetComponent<GameObjectExt>().IsVis && !Battle.Instance.IsArenaMode);
        //SuperRelease.SetActive(true);
        UiUtil.SetActive(SuperRelease.gameObject, true);
        TimerMgr.Instance.BattleSchedulerTimer(0.7f, delegate
        {
            //SuperRelease.SetActive(false);
            UiUtil.SetActive(SuperRelease.gameObject, false);
        });
    }

    public void UnSelect()
    {
        if (!IsSelected || IsFocusing)
            return;

        ResetScale(this.VimFullRoot.transform, 1f);
        //this.VimFullRoot.localScale = Vector3.one;
        //Selected.SetActive(false);
        //Job.color = UnSelectedColor;
       // SuperRelease.SetActive(false);
        //this.DefenceClick.SetActive(false);
        UiUtil.SetActive(SuperRelease.gameObject, false);
        UiUtil.SetActive(DefenceClick.gameObject, false);
        IsSelected = false;
        //ResetPosY(Full.transform, 40);

        ResetPosY(this.VimFullBackGround.transform, 120f);
        //ResetSize(SpineGraphic.transform, SpineBeginSizeY);
        ResetSize(SpineGraphic.transform.parent, SpineRootBeginSizeY);
        ResetPosY(SpineGraphic.transform.parent, SpineRootBeginY);
        
        ResetPosY(this.SpineGraphic.transform, SpineUnitBeginY);
        
        ResetSize(this.BattleBigMoveBar02.transform, this.BattleBigMoveBar02_Region.transform.GetComponent<RectTransform>().sizeDelta.y);
        ResetPosY(this.BattleBigMoveBar02.transform, this.BattleBigMoveBar02_Region.transform.GetComponent<RectTransform>().anchoredPosition3D.y);
        
        ResetSize(this.BattleRolePanel02.transform, this.BattleRolePanel02_Region.transform.GetComponent<RectTransform>().sizeDelta.y);
        ResetPosY(this.BattleRolePanel02.transform, this.BattleRolePanel02_Region.transform.GetComponent<RectTransform>().anchoredPosition3D.y);

        ResetSize(this.VimFullEffect.transform, this.VimFullEffect_Region.transform.GetComponent<RectTransform>().sizeDelta.y);
        ResetPosY(this.VimFullEffect.transform, this.VimFullEffect_Region.transform.GetComponent<RectTransform>().anchoredPosition3D.y);

        ResetSize(this.VimBar.transform, this.VimBar_Region.transform.GetComponent<RectTransform>().sizeDelta.y);
        ResetPosY(this.VimBar.transform, this.VimBar_Region.transform.GetComponent<RectTransform>().anchoredPosition3D.y);

        VimBar.sprite = VimBar_Region.sprite;
    }
    
    public void PlayAnimtion(string anim)
    {
        if (Role != null
            && !Role.mData.IsDead)
            Owner.PlayAnimtion(SpineGraphic, anim, Role.ConfigID.ToString());
    }
}