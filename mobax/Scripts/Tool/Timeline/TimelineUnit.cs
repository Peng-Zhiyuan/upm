using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class TimelineUnit : RecycledGameObject
{
    public List<GameObject> EnemyRootNodes = new List<GameObject>();
    public List<Transform> EnemyTrans = new List<Transform>();
    public PlayableDirector Director;

    List<GameObject> Targets = new List<GameObject>();

    private void Awake()
    {
        Director = gameObject.GetComponentInChildren<PlayableDirector>();

        //var rootNode = transform.Find ("Enemy");
        var trans = transform.GetComponentsInChildren<Transform>(true);
        if (trans != null)
        {
            foreach (var tran in trans)
            {
                if (tran.name == "Enemy")
                {
                    EnemyRootNodes.Add(tran.gameObject);
                    //var enemyTrans = new List<Transform>();
                    int i = 0;
                    foreach (Transform e in tran)
                    {
                        if (i == 0)
                        {
                            EnemyTrans.Add(e);
                        }
                        e.gameObject.SetActive(false);
                    }
                }
            }
        }
    }

    public void SetEnemies(List<GameObject> targets)
    {
        if (targets == null)
            return;
        Targets = targets;
        for (int i = 0; i < targets.Count; i++)
        {
            var target = targets[i].transform;
            ResetEnemy(target.gameObject);
            if (0 < EnemyRootNodes.Count)
            {
                var attachParent = EnemyRootNodes[0].transform;
                if (i < EnemyTrans.Count)
                {
                    var tranInfo = EnemyTrans[i];
                    target.parent = attachParent;
                    target.transform.localPosition = tranInfo.localPosition;
                    target.transform.localRotation = tranInfo.localRotation;
                    target.transform.localScale = tranInfo.localScale;
                }
            }
        }
    }

    void ResetEnemy(GameObject go)
    {
        go.layer = LayerMask.NameToLayer("Cutscene");
        var roleRender = go.GetComponentInChildren<RoleRender>();
        if (roleRender == null)
            return;
        roleRender.SwitchPlayMode(false);
        roleRender.gameObject.layer = go.layer;
        roleRender.transform.localPosition = Vector3.zero;
        roleRender.transform.localRotation = Quaternion.identity;
        roleRender.transform.localScale = Vector3.one;
    }

    public void Release()
    {
        BucketManager.Stuff.Battle.Pool.Recycle(this);
        foreach (var go in Targets)
        {
            Destroy(go);
        }
        Targets.Clear();
    }
}