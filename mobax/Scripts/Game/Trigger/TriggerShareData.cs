using UnityEngine;
using System.Collections.Generic;
using BattleEngine.Logic;
using BattleSystem.Core;

public class TriggerShareData
{
    public Creature sceneObj; //场景对象

    public CharacterActionController actionCtrl;
    public int HitIndex = 0;
    public HurtType hurtType;

    private List<int> _listEffectIndex = new List<int>(); //trigger配置中创建的特效索引列表

    public void AddEffectIndex(int index)
    {
        _listEffectIndex.Add(index);
    }

    public virtual void Uninitialize()
    {
        sceneObj = null;
        actionCtrl = null;
    }

    //public void RemoveEffects()
    //{
    //    for (int index = _listEffectIndex.Count - 1; index >= 0; index--)
    //    {
    //        EffectManager.Instance.RemoveEffect(_listEffectIndex[index]);
    //    }
    //}
}

public class TriggerShareDataSkill : TriggerShareData
{
    public List<GameObject> targets = new List<GameObject>(); //trigger配置中创建的特效索引列表
    public List<GameObject> objects = new List<GameObject>();
    public Vector3 destinationPos; //施法后位置
    public GameObject live_spine = null; //立绘
    public Vector3 targetPos = Vector3.zero;
    public List<string> attackIDs = new List<string>();
    public int SkillID;
    //受击数据
    public BehitData hitData;

    public List<GameObject> CreateObjects()
    {
        /*foreach (var go in targets)
        {
            if (go != null)
            {
                var spine = GameObject.Instantiate(go) as GameObject;
                spine.transform.position = go.transform.position;
                objects.Add(spine);
            }
        }*/
        return objects;
    }

    public override void Uninitialize()
    {
        base.Uninitialize();
        targets.Clear();
    }

    public void CleanObjects()
    {
        foreach (var go in objects)
        {
            GameObject.Destroy(go);
        }
        targets.Clear();
    }

    public async void CreateSpine(string name)
    {
        var address = name + ".prefab";
        //var obj = await AddressableRes.AquireAsync<Object>(address);
        var bucket = BucketManager.Stuff.Battle;
        var obj = await bucket.GetOrAquireAsync<Object>(address);
        live_spine = GameObject.Instantiate(obj) as GameObject;
    }
}