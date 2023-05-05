using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BattleEngine.Logic;
using Random = UnityEngine.Random;

public class DamageManager : BattleComponent<DamageManager>
{
    private Transform _damageRoot = null;
    private GameObject _damagePrefab = null;
    //private GameObjectPool _pool = null;

    private Vector2[] allArea = { new Vector2(1, 1), new Vector2(1, -1), new Vector2(-1, 1), new Vector2(-1, -1) };
    private List<Vector2> allPos = new List<Vector2>();

    public bool Vis
    {
        get;
        set;
    } = true;

    public override void OnBattleCreate()
    {
        if (_damageRoot == null)
        {
            _damageRoot = GameObject.Find("Damages").transform;
            //_damageRoot = UIEngine.Stuff.transform.Find("Custom").transform;
        }
        for (int i = 0; i < 4; i++)
        {
            Vector2 pos = Random.insideUnitCircle * 50;
            pos.x = 50;
            pos.y = 50;
            float x = Mathf.Abs((pos.x));
            float y = Mathf.Abs(pos.y);
            int index = i % allArea.Length;
            Vector2 point = new Vector2(x, y) * allArea[index];
            allPos.Add(point);
        }
    }

    public Vector2 GetNextOffset()
    {
        Vector2 pos = allPos.Last();
        allPos.RemoveAt(allPos.Count - 1);
        allPos.Insert(UnityEngine.Random.Range(0, allPos.Count - 1), pos);
        return pos;
    }

    public void SetVisible(bool vis)
    {
        if (_damageRoot != null)
        {
            _damageRoot.gameObject.SetActive(vis);
        }
    }

    public override async Task OnLoadResourcesAsync()
    {
        //GameObject prefab = await AddressableRes.loadAddressableResAsync<UnityEngine.Object>($"Assets/AddressableRes/UI/Hud/Prefab/DamagePanel.prefab") as GameObject;
        var address = "DamagePanel.prefab";
        //var prefab = await AddressableRes.AquireAsync<GameObject>(address);
        var prefab = await BucketManager.Stuff.Battle.GetOrAquireAsync<GameObject>(address);
        if (prefab == null)
            return;
        _damagePrefab = prefab as GameObject;
    }

    public async void ShowDamagePanel(Creature target, float damage, bool isCrit, bool isLeftAnim, BehitData data)
    {
        //没有跟踪的角色不显示伤害(治疗技能除外)
        if (target.IsSelf())
        {
            if (!data.HasState(HitType.Cure)
                && !target.Selected)
                return;
        }
        var go = await GameObjectPoolUtil.ReuseAddressableObjectAsync<RecycledGameObject>(BucketManager.Stuff.Battle, "DamagePanel.prefab");
        go.transform.SetParent(_damageRoot);

        //设置位置
        Vector3 pos = CameraManager.Instance.MainCamera.WorldToScreenPoint(target.GetPosition() + new Vector3(0, 0.5f, 0));
        //pos = CameraManager.Instance.UICamera.ScreenToWorldPoint((pos));
        DamageUnit du = go.transform.GetComponent<DamageUnit>();
        bool isHero = (target.sceneObjectType == SceneObjectType.Player || target.sceneObjectType == SceneObjectType.NPC);
        go.transform.position = pos;
        go.transform.localScale = Vector3.one;
        /*await du.PlayAnim( isHero, data, target);
        if (go != null)
        {
            GameObjectPool.Stuff.Recycle(go);
        }*/
    }

    private List<BDamageAction> list = new List<BDamageAction>();
    private float remTime = 0f;

    public override void OnUpdate()
    {
        /*if (remTime > 0)
        {
            remTime -= Time.deltaTime;
            if (remTime > 0)
                return;
        }
        if (list.Count == 0)
            return;
        ShowDamageAnim(list[0]);
        list.RemoveAt(0);
        remTime = 0.05f;*/

        /*foreach (var VARIABLE in list)
        {
            ShowDamageAnim(VARIABLE);
        }
        list.Clear();*/
    }

    public async void ShowDamage(BDamageAction data)
    {
        Creature target = BattleManager.Instance.ActorMgr.GetActor(data.targetID);
        if (target == null)
            return;
        Creature caster = BattleManager.Instance.ActorMgr.GetActor(data.casterID);

        //没有跟踪的角色不显示伤害(治疗技能除外)
        if (target.IsSelf())
        {
            //if(!data.HasState(HitType.Cure) && !target.Selected)
            //return;
        }
        else
        {
            if (caster != null)
            {
                if (caster.IsSelf())
                {
                    if (caster.IsPlayer)
                    {
                        
                    }
                    else
                    {
                        if (!data.HasState(HitType.Cure)
                            && !caster.Selected && !data.HasState(HitType.SPSKLDamage))
                            return;
                    }
                }
            }
        }
        //list.Add(data);
        
        ShowDamageAnim(data);
    }

    private static GameObject prefabUnit = null;
    public async void ShowDamageAnim(BDamageAction data)
    {
        Creature target = BattleManager.Instance.ActorMgr.GetActor(data.targetID);
        if (target == null)
            return;
        
        if (prefabUnit == null)
        {
            var address = "DamageUnit.prefab";
            prefabUnit = await BucketManager.Stuff.Battle.GetOrAquireAsync<GameObject>(address);
        }
        
        RecycledGameObject go = BucketManager.Stuff.Battle.Pool.Reuse<RecycledGameObject>(prefabUnit);
        go.transform.SetParent(_damageRoot);

        //if (target.IsSelf())
        //prefab = "DamageBar.prefab";
        //var go = await GameObjectPoolUtil.ReuseAddressableObjectAsync<RecycledGameObject>(BuketManager.Stuff.Battle, address);

        //var v1 = target.GetPosition() - CameraManager.Instance.MainCamera.transform.position;
        //var v2 = caster.GetPosition() - CameraManager.Instance.MainCamera.transform.position;
        //bool isLeft = Vector3.Dot(v1, v2) < 0;
        Vector3 offset = Vector3.zero;
        /*if (!isLeft)
            offset.x = UnityEngine.Random.Range(-1.2f, -0.5f);
        else
        {
            offset.x = UnityEngine.Random.Range(1.2f, 0.5f);
        }
        
        offset.y = UnityEngine.Random.Range(-0.5f, 0.5f);*/
        /*var vec = UnityEngine.Random.insideUnitCircle * 1f;
        offset.x = vec.x;
        offset.y = vec.y;*/
        if (!target.IsSelf()
            && !data.HasState(HitType.Cure))
        {
            target.BeHit();
        }
        Vector2 vec;

        //设置位置
        Transform bone;
        if (target.IsSelf())
        {
            bone = target.GetBone("root");
            offset.x = UnityEngine.Random.Range(-1f, 1f);
            offset.y = 0;
        }
        else
        {
            bone = target.GetBone("body_hit");
            offset.y = 0.6f;
        }
        Vector3 pos = CameraManager.Instance.MainCamera.WorldToScreenPoint(bone.position + offset);
        //pos = CameraManager.Instance.UICamera.ScreenToWorldPoint((pos));
        DamageUnit du = go.transform.GetComponent<DamageUnit>();
        bool isHero = (target.sceneObjectType == SceneObjectType.Player || target.sceneObjectType == SceneObjectType.NPC);
        go.transform.position = pos;
        go.transform.localScale = Vector3.one;
        if (target.IsSelf())
        {
            GameEventCenter.Broadcast(GameEvent.ShowDamageBar, target.mData.UID, go);
            await du.PlayAnim(isHero, data, target, target.IsLeft());
            if (go != null)
            {
                BucketManager.Stuff.Battle.Pool.Recycle(go);
            }
            return;
        }
        await du.PlayAnim(isHero, data, target, target.IsLeft());
        if (go != null)
        {
            BucketManager.Stuff.Battle.Pool.Recycle(go);
        }
    }
}