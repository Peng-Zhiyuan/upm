using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading.Tasks;

/// <summary>
/// 战斗组件，单例
/// 此单例会被视作战斗实例的一部分
/// 一场战斗只能创建一个此类型实例
/// 当战斗实例未被创建时，此单例无法被创建
/// 当战斗实例被销毁时，此实例也会被销毁
/// 仅在一场战斗中，可以访问此类型单例
/// 
/// 使用静态字段'Instance'来访问此类型单例
/// 使用静态字段'IsAccessable'来判断此类型单例是否可被访问
/// </summary>
/// <typeparam name="T"></typeparam>
public class BattleComponent<T> : IBattleComponent where T : BattleComponent<T>, new()
{
    public static T Instance
    {
        get
        {
            var type = typeof(T);
            object ret = TrtGet(type);
            if (ret == null)
            {
                //BattleLog.LogError($"[BattleComponent] instance of '{type.Name}' not created.");
                return null;
                //ret = new T();
                //Set(type, ret);
                //var count = Count;
                //Debug.Log($"<color=#AAAAFF>[BattleSingleton] typeof {typeof(T).Name} created (total: {count})</color>");
            }
            return ret as T;
        }
    }

    public static object TrtGet(Type type)
    {
        var battle = Battle.Instance;
        if (battle != null)
        {
            IBattleComponent ret;
            battle.typeToBattleSingleInstanceDic.TryGetValue(type, out ret);
            return ret;
        }
        else
        {
            BattleLog.LogError("[BattleComponent] battle instace not exsits");
            return null;
        }
    }

    public static bool IsAccessable
    {
        get
        {
            var battle = Battle.Instance;
            if (battle == null)
            {
                return false;
            }
            if(!battle.isEnterCompelted)
            {
                return false;
            }
            return true;
        }
    }

    public Battle battle;

    public void Setup(Battle battle)
    {
        this.battle = battle;
    }

    public virtual void OnBattleCreate()
    {

    }

    public virtual Task OnLoadResourcesAsync()
    {
        var tcs = new TaskCompletionSource<bool>();
        tcs.SetResult(true);
        return tcs.Task;
    }

    public virtual void OnUpdate()
    {

    }

    public virtual void LateUpdate()
    {

    }

    public virtual void FixedUpdate()
    {

    }

    public virtual void OnDestroy()
    {

    }

    public virtual void OnCoreCreated()
    {

    }
}
