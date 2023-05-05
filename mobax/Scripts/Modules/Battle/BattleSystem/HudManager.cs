using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using DG.Tweening;
using BattleSystem.Core;
using System.Threading.Tasks;
using BattleEngine.Logic;
using FFmpeg.AutoGen;
using Modules.Battle.BattleSystem.Page;
using Neatly.Timer;

public class HudManager : BattleComponent<HudManager>
{
    Transform hudRoot;
    GameObject _hudPrefab;

    public bool Visible { set; get; } = true;

    public override async Task OnLoadResourcesAsync()
    {
        if (hudRoot == null)
        {
            hudRoot = GameObject.Find("Huds").transform;
        }
        var bucket = BucketManager.Stuff.Battle;
        this._hudPrefab = await bucket.GetOrAquireAsync<GameObject>("Hud.prefab");
    }

    public async void ActivateBuffAbility(BuffAbility buff)
    {
        Hud temp_hud = null;
        foreach (var hud in this.hud_list)
        {
            if (hud.info.ID == buff.SelfActorEntity.UID)
            {
                temp_hud = hud;
                hud.BufferCach.Add(LocalizationManager.Stuff.GetText(buff.buffRow.Name));
                break;
            }
        }
        if (temp_hud == null)
            return;
        if (temp_hud.BufferCach.Count > 1)
            return;
        PlayBuffAnim(temp_hud, temp_hud.BufferCach[0]);
    }

    async void PlayBuffAnim(Hud hud, string name)
    {
        var prefab = "Buffer.prefab";
        var go = await GameObjectPoolUtil.ReuseAddressableObjectAsync<RecycledGameObject>(BucketManager.Stuff.Battle, prefab);
        go.transform.SetParent(hud.bufferRoot);
        go.transform.localPosition = Vector3.zero;
        go.transform.localScale = Vector3.one;
        go.GameObject.GetComponent<Text>().text = LocalizationManager.Stuff.GetText(name);
        var animation = go.GetComponent<Animation>();
        //go.GetComponent<Animation>().Play();
        animation.Play("HudBufferAnim");
        animation["HudBufferAnim"].time = 0;
        await Task.Delay(500);
        hud.BufferCach.RemoveAt(0);
        if (hud.BufferCach.Count > 0)
        {
            PlayBuffAnim(hud, hud.BufferCach[0]);
        }
        await Task.Delay(700);
        go.Recycle();
    }

    private static GameObject HudRoot;
    public void SetVis(bool vis)
    {
        if (HudRoot == null)
        {
            HudRoot = GameObject.Find("BattleCanvas");
        }
        HudRoot?.SetActive(vis);
    }

    void AddListner()
    {
        GameEventCenter.AddListener(GameEvent.CreateHud_Success, this, this.CreateHud);
        GameEventCenter.AddListener(GameEvent.HudHide, this, this.HudHide);

        //GameEventCenter.AddListener(GameEvent.UpdateHpMp, this, this.UpdateHpMp);
        GameEventCenter.AddListener(GameEvent.EmotionChanged, this, this.EmotionUpdate);
        GameEventCenter.AddListener(GameEvent.GameModeChanged, this, this.GameModeChanged);
        GameEventCenter.AddListener(GameEvent.ShowHudName, this, this.ShowHudName);
        //GameEventCenter.AddListener(GameEvent.ShowHudDamageEffect, this, this.ShowHudDamageEffect);
        EventManager.Instance.AddListener<CombatActorEntity>("BattleRefreshBuff", BufferRefresh);
        GameEventCenter.AddListener(GameEvent.SelectChanged, this, this.SelectChanged);
        GameEventCenter.AddListener(GameEvent.ShowBreakDamageChanged, this, this.ShowBreakDamageChanged);
        GameEventCenter.AddListener(GameEvent.ShowBreakDamage, this, this.ShowBreakDamage);
    }

    public void ShowBreakDamage(object[] data)
    {
        var actor = data[0] as CombatActorEntity;
        foreach (var VARIABLE in this.hud_list)
        {
            if (VARIABLE.info.ID == actor.UID)
            {
                VARIABLE.ShowBreakDamgeRoot();
                break;
            }
        }
    }

    public void ShowWarningBar(string uid, float time)
    {
        foreach (var VARIABLE in this.hud_list)
        {
            if (VARIABLE.info.ID == uid)
            {
                VARIABLE.ShowWarningBar(time);
                break;
            }
        }
    }

    public void ShowEmoji(string uid, string emoji, bool isleft)
    {
        foreach (var VARIABLE in this.hud_list)
        {
            if (VARIABLE.info.ID == uid)
            {
                VARIABLE.ShowEmoji(emoji, isleft);
                break;
            }
        }
    }
    
    public void ShowBreakDamageChanged(object[] data)
    {
        var actor = data[0] as CombatActorEntity;
        foreach (var VARIABLE in this.hud_list)
        {
            if (VARIABLE.info.ID == actor.UID)
            {
                VARIABLE.ShowBreakDamageAnim();
                break;
            }
        }
    }
    
    public void SelectChanged(object[] data)
    {
        var role = data[0] as Creature;
        foreach (var VARIABLE in hud_list)
        {
            VARIABLE.SelectChange(role);
        }
    }

    public override void OnBattleCreate()
    {
        //Debug.LogError("----HudManager Created");
        Clear();
        this.AddListner();
    }

    public override void OnDestroy()
    {
        Clear();
        GameEventCenter.RemoveListener(GameEvent.CreateHud_Success, this);
        GameEventCenter.RemoveListener(GameEvent.HudHide, this);

        //GameEventCenter.RemoveListener(GameEvent.UpdateHpMp, this);
        GameEventCenter.RemoveListener(GameEvent.EmotionChanged, this);
        GameEventCenter.RemoveListener(GameEvent.GameModeChanged, this);
        GameEventCenter.RemoveListener(GameEvent.SelectPlayer, this);
        GameEventCenter.RemoveListener(GameEvent.ShowHudName, this);
        //GameEventCenter.RemoveListener(GameEvent.ShowHudDamageEffect, this);
        EventManager.Instance.RemoveListener<CombatActorEntity>("BattleRefreshBuff", BufferRefresh);
        GameEventCenter.RemoveListener(GameEvent.SelectChanged, this);
        GameEventCenter.RemoveListener(GameEvent.ShowBreakDamageChanged, this);
        GameEventCenter.RemoveListener(GameEvent.ShowBreakDamage, this);
    }

    public void BufferRefresh(CombatActorEntity entity)
    {
        if (entity.isAtker)
            return;
        foreach (var VARIABLE in this.hud_list)
        {
            if (VARIABLE.info.ID == entity.UID)
            {
                VARIABLE.RefreshBuffer();
                break;
            }
        }
    }

    Hud CreateOneHud()
    {
        RecycledGameObject go = BucketManager.Stuff.Battle.Pool.Reuse<RecycledGameObject>(_hudPrefab);
        var hud = new Hud(go, hudRoot);
        return hud;
    }

    List<Hud> hud_list = new List<Hud>();

    public void Clear()
    {
        hud_list.Clear();
    }

    void CreateHud(object[] data)
    {
        var info = data[0] as Creature;
        if (info.sceneObjectType == SceneObjectType.Player /* || info.RoleItemData.IsBoss*/)
        {
            return;
        }
        Hud hud = null;
        hud = this.CreateOneHud();
        hud.SetData(info);
        hud.SetVisible(false);
        //this.SetData(hud);
        hud_list.Add(hud);
    }

    public override void OnUpdate()
    {
        var camera = CameraManager.Instance.MainCamera;
        foreach (var hud in this.hud_list)
        {
            float val = Vector3.Dot(camera.transform.forward, hud.info.transform.position - camera.transform.position);
            if (hud.durT <= 0)
            {
                //hud.SetVisible(false);
            }
            else
            {
                hud.SetVisible(Visible && val > 0 && Battle.Instance.BattleStarted);
                hud.durT -= Time.deltaTime;
            }
            var pos = hud.info.transform.position;
            var head_point = hud.info.HeadBone;
            if (head_point != null)
            {
                pos = head_point.transform.position;
            }
            pos = CameraManager.Instance.MainCamera.WorldToScreenPoint(pos);
            //pos = CameraManager.Instance.MainCamera.ScreenToWorldPoint(pos);
            hud.go.transform.position = pos;
            
            if(!NeatlyTimer.instance.timeStop)
                hud.Update();
        }
    }

    void ShowHudName(object[] data)
    {
        var create = data[0] as Creature;
        var vis = (bool)data[1];
        foreach (var hud in this.hud_list)
        {
            if (hud.info.ID == create.ID)
            {
                if (vis)
                {
                    hud.go.SetActive(true);
                }
                hud.name_back.SetActive(vis && Visible);
                break;
            }
        }
    }

    void HudHide(object[] data)
    {
        var create = data[0] as Creature;
        for (int i = 0; i < this.hud_list.Count; i++)
        {
            var hud = this.hud_list[i];
            if (hud.info.ID == create.ID)
            {
                hud.Destroy();
                this.hud_list.Remove(hud);
                break;
            }
        }
    }

    public void UpdateHero(string uid)
    {
        foreach (var hud in this.hud_list)
        {
            if (hud.info.ID == uid)
            {
                hud.UpdateHpMp();
                GameEventCenter.Broadcast(GameEvent.UpdateHpMp, hud.info);
                return;
            }
        }
    }

    /*void ShowHudDamageEffect(object[] data)
    {
        var id = (string)data[0];
        foreach (var hud in this.hud_list)
        {
            if (hud.info.ID == id)
            {
                this.ShowDamageEffect(hud);
                return;
            }
        }
    }*/

    /*
    void ShowDamageEffect(Hud hud)
    {
        Image image;
        Color color;
        if (hud.info.sceneObjectType == SceneObjectType.Monster)
        {
            image = hud.monsterHead;
            color = new Color(0.6f, 0.6f, 0.6f);
        }
        else
        {
            image = hud.head;
            color = new Color(1, 0, 0);
        }
        DOTween.Kill(hud.info.ID, false);
        var sequence = DOTween.Sequence();
        for (int i = 0; i <= 4; i++)
        {
            sequence.Append(image.DOColor(color, 0.4f));
            sequence.Append(image.DOColor(new Color(1, 1, 1), 0.4f));
        }
        sequence.SetId(hud.info.ID);
    }
    */

    void EmotionUpdate(object[] data)
    {
        var id = (string)data[0];
        var value = (StateEmotion)data[1];
        foreach (var hud in this.hud_list)
        {
            if (hud.info.ID == id)
            {
                hud.emoteRoot.UpdateEmote(value);
                return;
            }
        }
    }

    void GameModeChanged(object[] data)
    {
        foreach (var hud in this.hud_list)
        {
            hud.hp_back.SetActive(hud.info.IsEnemy);
            //hud.head.gameObject.SetActive(false && hud.info.IsHero);
            //hud.indicate.gameObject.SetActive(false && hud.info.IsHero);
            //hud.monsterHead.gameObject.SetActive(false && hud.info.IsEnemy);
            //hud.map_indicate.gameObject.SetActive(hud.info.sceneObjectType != SceneObjectType.Monster && hud.info.sceneObjectType != SceneObjectType.NPC);
            if (false)
            {
                hud.nameroot.localPosition = new Vector3(0, 35, 0);
            }
            else
            {
                hud.nameroot.localPosition = Vector3.zero;
            }
        }
    }
}