using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;
using Sirenix.OdinInspector;

public class CoreObjectDebuger : SerializedMonoBehaviour
{
    // 此方法会被 CoreObjectUtil 反射调用
    [Preserve]
    public static void Attach(CoreObject co)
    {
        var go = new GameObject();
        go.name = co.Name;

        co.OnNameChnaged += (name) =>
        {
            go.name = name;
        };

        co.OnDestoryed += () =>
        {
            GameObject.Destroy(go);
        };

        co.OnActivedChnaged += (b) =>
        {
            go.SetActive(b);
        };

        var instance = go.AddComponent<CoreObjectDebuger>();
        instance.co = co;

        co.debbugerGameObject = go;

        GameObject.DontDestroyOnLoad(go);


        instance.isAttachComplete = true;

    }

    public CoreObject co;
    void Update()
    {
        if(this.isAttachComplete)
        {
            var coreTransform = co.Transform;

            this.transform.position = coreTransform.position.ToVector3();
            this.transform.rotation = coreTransform.rotation.ToQuaternion();
            this.transform.localScale = coreTransform.scale.ToVector3();
        }

    }

    bool isAttachComplete;
    void OnEnable()
    {
        if(isAttachComplete)
        {
            this.co.IsActived = true;
        }
    }

    private void OnDisable()
    {
        if(isAttachComplete)
        {
            this.co.IsActived = false;
        }
    }

    [Button]
    void PrintComponentList()
    {
        var index = 0;
        foreach(var one in this.co.componentList)
        {
            var type = one.GetType();
            Debug.Log($"[{index}] {type.Name}");
            index++;
        }
    }

    void OnDrawGizmos()
    {
        var list = co.componentList;
        foreach (var comp in list)
        {
            comp.OnCoreDrawGizmos();
        }
    }

    void OnDrawGizmosSelected()
    {
        var list = co.componentList;
        foreach (var comp in list)
        {
            comp.OnCoreDrawGizmosSelected();
        }
    }

    void OnGUI()
    {
        var list = co.componentList;
        foreach (var comp in list)
        {
            comp.OnGUI();
        }
    }
}
