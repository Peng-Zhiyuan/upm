using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 安装：
/// 每一帧调用 UpdateManager.Update()
/// </summary>
public class UpdateManager : StuffObject<UpdateManager> {
    private readonly List<IUpdatable> list = new List<IUpdatable>();
    private readonly List<IUpdatable> newAdd = new List<IUpdatable>();
    private readonly List<IUpdatable> newRemove = new List<IUpdatable>();

    public void Add(IUpdatable updatable) {
        newAdd.Add(updatable);
    }

    public void Remove(IUpdatable updatable) {
        newRemove.Add(updatable);
    }

    void Update() {
        if (newAdd.Count > 0) {
            foreach (var r in newAdd) {
                if (!list.Contains(r)) {
                    list.Add(r);
                }
            }
            newAdd.Clear();
        }
        foreach (var u in list) {
            u.OnUpdate();
        }
        if (newRemove.Count > 0) {
            foreach (var r in newRemove) {
                list.Remove(r);
            }
            newRemove.Clear();
        }
    }
}

public interface IUpdatable {
    void OnUpdate();
}