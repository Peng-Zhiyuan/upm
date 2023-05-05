using System;

namespace BattleEngine.View
{
    using System.Collections.Generic;
    using UnityEngine;
    using System.Threading.Tasks;
    using Logic;

    public sealed class BattleResManager : Singleton<BattleResManager>
    {
        //加载的英雄单位
        private List<string> avatarPrefab = new List<string>();
        //加载特效预制
        private Dictionary<string, GameObject> effectPrefabs = new Dictionary<string, GameObject>();
        private Dictionary<string, List<GameObject>> unUseEffectPrefabs = new Dictionary<string, List<GameObject>>();
        private Dictionary<string, List<GameObject>> usingEffectPrefabs = new Dictionary<string, List<GameObject>>();
        private Transform battlePool;

        // Start is called before the first frame update
        public async Task<GameObject> LoadAvatarModel(string AvatarPath)
        {
            var model = await BucketManager.Stuff.Battle.GetOrAquireAsync<GameObject>(AvatarPath);
            if (model == null)
            {
                BattleLog.LogWarning("Cant find model  " + AvatarPath);
                return null;
            }
            GameObject obj = GameObject.Instantiate(model);
            if (avatarPrefab.Contains(AvatarPath))
                avatarPrefab.Add(AvatarPath);
            return obj;
        }
        
        public async Task<GameObject> CreatorFx(string fxName, bool isLoop = false, int layer = 0)
        {
            if (string.IsNullOrEmpty(fxName))
                return null;
            GameObject fxObj = await CreatorFx(fxName, null, Vector3.zero, isLoop, layer);
            return fxObj;
        }

        public async Task<GameObject> CreatorFx(string fxName, Transform parent, Vector3 offset, bool isLoop = false, int layer = 0)
        {
            if (string.IsNullOrEmpty(fxName))
                return null;
            try
            {
                GameObject fxObj = null;
                if (unUseEffectPrefabs.ContainsKey(fxName)
                    && unUseEffectPrefabs[fxName].Count > 0)
                {
                    fxObj = unUseEffectPrefabs[fxName][0];
                    unUseEffectPrefabs[fxName].RemoveAt(0);
                }
                if (fxObj == null && isLoop)
                {
                    if (usingEffectPrefabs.ContainsKey(fxName)
                        && usingEffectPrefabs[fxName].Count > 0)
                    {
                        fxObj = usingEffectPrefabs[fxName][0];
                    }
                }
                if (fxObj == null)
                {
                    if (effectPrefabs.ContainsKey(fxName)
                        && effectPrefabs[fxName] != null)
                    {
                        fxObj = GameObject.Instantiate(effectPrefabs[fxName]);
                    }
                    else
                    {
                        var fx = await BucketManager.Stuff.Battle.GetOrAquireAsync<GameObject>(fxName);
                        if (fx == null)
                        {
                            return null;
                        }
                        if (!effectPrefabs.ContainsKey(fxName))
                        {
                            effectPrefabs.Add(fxName, fx);
                        }
                        else
                        {
                            effectPrefabs[fxName] = fx;
                        }
                        fxObj = GameObject.Instantiate(fx);
                        ParticleSystemPlayCtr ctr = fxObj.GetComponent<ParticleSystemPlayCtr>();
                        if (ctr == null)
                        {
                            fxObj.AddComponent<ParticleSystemPlayCtr>();
                        }
                    }
                }
                if (usingEffectPrefabs.ContainsKey(fxName))
                {
                    usingEffectPrefabs[fxName].Add(fxObj);
                }
                else
                {
                    List<GameObject> lst = new List<GameObject>() { fxObj };
                    usingEffectPrefabs.Add(fxName, lst);
                }
                fxObj.transform.parent = parent;
                if (parent == null)
                {
                    fxObj.transform.position = offset;
                }
                else
                {
                    fxObj.transform.localPosition = offset;
                }
                fxObj.transform.localScale = Vector3.one;
                fxObj.SetActive(true);
                ParticleSystemPlayCtr psCtr = fxObj.GetComponent<ParticleSystemPlayCtr>();
                if (psCtr != null)
                {
                    psCtr.Play();
                }
                else
                {
                    psCtr = fxObj.AddComponent<ParticleSystemPlayCtr>();
                    psCtr.Play();
                }
                psCtr.effectPrefabName = fxName;
                ChangeLayer(fxObj, 0);
                return fxObj;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public void RecycleAllEffect()
        {
            var data = usingEffectPrefabs.GetEnumerator();
            while (data.MoveNext())
            {
                for (int i = 0; i < data.Current.Value.Count; i++)
                {
                    if (data.Current.Value != null)
                    {
                        RecycleEffect(data.Current.Key, data.Current.Value[i]);
                    }
                    else
                    {
                        data.Current.Value.RemoveAt(i);
                        i--;
                    }
                }
            }
        }

        public void RecycleEffect(string fxName, GameObject fxObj)
        {
            if (fxObj == null)
                return;
            if (!Battle.Instance.IsFight)
            {
                GameObject.Destroy(fxObj);
                return;
            }
            ParticleSystemPlayCtr psCtr = fxObj.GetComponent<ParticleSystemPlayCtr>();
            if (psCtr != null)
            {
                psCtr.Stop();
            }
            else
            {
                psCtr = fxObj.AddComponent<ParticleSystemPlayCtr>();
                psCtr.Stop();
            }
            if (usingEffectPrefabs.ContainsKey(fxName))
            {
                if (usingEffectPrefabs[fxName].Count == 1)
                {
                    usingEffectPrefabs[fxName].Remove(fxObj);
                    usingEffectPrefabs[fxName].Clear();
                }
                else
                {
                    usingEffectPrefabs[fxName].Remove(fxObj);
                }
            }
            if (unUseEffectPrefabs.ContainsKey(fxName))
            {
                unUseEffectPrefabs[fxName].Add(fxObj);
            }
            else
            {
                List<GameObject> lst = new List<GameObject>() { fxObj };
                unUseEffectPrefabs.Add(fxName, lst);
            }
            if (battlePool == null)
            {
                GameObject pool = GameObject.Find("BattlePool");
                if (pool != null)
                    battlePool = pool.transform;
            }
            fxObj.transform.parent = battlePool;
            fxObj.SetActive(false);
        }

        public void ReleaseEffect(string fxName)
        {
            if (usingEffectPrefabs.ContainsKey(fxName))
            {
                for (int i = 0; i < usingEffectPrefabs[fxName].Count; i++)
                {
                    if (usingEffectPrefabs[fxName][i] != null)
                    {
                        GameObject.DestroyImmediate(usingEffectPrefabs[fxName][i]);
                    }
                }
                usingEffectPrefabs.Remove(fxName);
            }
            if (unUseEffectPrefabs.ContainsKey(fxName))
            {
                for (int i = 0; i < unUseEffectPrefabs[fxName].Count; i++)
                {
                    if (unUseEffectPrefabs[fxName][i] != null)
                    {
                        GameObject.DestroyImmediate(unUseEffectPrefabs[fxName][i]);
                    }
                }
                unUseEffectPrefabs.Remove(fxName);
            }
            if (effectPrefabs.ContainsKey(fxName))
            {
                effectPrefabs.Remove(fxName);
            }
        }

        public void ChangeLayer(GameObject go, int targetLayer)
        {
            if (go == null) return;
            go.layer = targetLayer;
            //遍历更改所有子物体layer
            Transform[] child = go.GetComponentsInChildren<Transform>();
            for (int i = 0; i < child.Length; i++)
            {
                child[i].gameObject.layer = targetLayer;
            }
        }

        public void ChangeSelectFXStartColor(GameObject go, Color color)
        {
            if (go == null) return;
            //遍历更改所有子物体
            ParticleSystem[] ps = go.transform.GetComponentsInChildren<ParticleSystem>();
            if (ps.Length > 0)
            {
                for (int i = 0; i < ps.Length; i++)
                {
                    ParticleSystem.MainModule main = ps[i].main;
                    float colorA = main.startColor.color.a;
                    main.startColor = new Color(color.r, color.g, color.b, colorA);
                }
            }
        }

        public void RemoveNullEffect(string fxName)
        {
            if (usingEffectPrefabs.ContainsKey(fxName))
            {
                for (int i = 0; i < usingEffectPrefabs[fxName].Count; i++)
                {
                    if (usingEffectPrefabs[fxName][i] == null)
                    {
                        usingEffectPrefabs[fxName].RemoveAt(i);
                        i -= 1;
                    }
                }
                if (usingEffectPrefabs[fxName].Count <= 0)
                {
                    usingEffectPrefabs.Remove(fxName);
                }
            }
            if (unUseEffectPrefabs.ContainsKey(fxName))
            {
                for (int i = 0; i < unUseEffectPrefabs[fxName].Count; i++)
                {
                    if (unUseEffectPrefabs[fxName][i] == null)
                    {
                        unUseEffectPrefabs[fxName].RemoveAt(i);
                        i -= 1;
                    }
                }
                if (unUseEffectPrefabs[fxName].Count <= 0)
                {
                    unUseEffectPrefabs.Remove(fxName);
                }
            }
        }

        public void releaseAll()
        {
            BattleLog.Log("Battle release all!!!");
            releaseModelPrefabs();
            releaseEffects();
            BucketManager.Stuff.Battle.ReleaseAll();
        }

        public void releaseEffects()
        {
            effectPrefabs.Clear();
            unUseEffectPrefabs.Clear();
            usingEffectPrefabs.Clear();
        }

        public void releaseModelPrefabs()
        {
            avatarPrefab.Clear();
        }

        public void PauseALLUsingEffect()
        {
            var data = usingEffectPrefabs.GetEnumerator();
            List<GameObject> lst = new List<GameObject>();
            while (data.MoveNext())
            {
                lst = data.Current.Value;
                for (int i = 0; i < lst.Count; i++)
                {
                    if (lst[i] == null)
                    {
                        lst.RemoveAt(i);
                        i--;
                        continue;
                    }
                    if (lst[i].GetComponent<ParticleSystemPlayCtr>() == null)
                    {
                        continue;
                    }
                    lst[i].GetComponent<ParticleSystemPlayCtr>().Pause();
                }
            }
        }

        public void ResumeALLUsingEffect()
        {
            var data = usingEffectPrefabs.GetEnumerator();
            List<GameObject> lst = new List<GameObject>();
            while (data.MoveNext())
            {
                lst = data.Current.Value;
                for (int i = 0; i < lst.Count; i++)
                {
                    if (lst[i] == null)
                    {
                        lst.RemoveAt(i);
                        i--;
                        continue;
                    }
                    if (lst[i].GetComponent<ParticleSystemPlayCtr>() == null)
                    {
                        continue;
                    }
                    lst[i].GetComponent<ParticleSystemPlayCtr>().Resume();
                }
            }
        }

        // /// <summary>
        // /// 加载锚点组
        // /// </summary>
        // /// <param name="anchorGroupName"></param>
        // /// <returns></returns>
        // public async Task<GameObject> LoadAnchorGroup(string anchorGroupName) {
        //     string loadKey = string.Format("Assets/AddressableRes/Prefab/Anchors/{0}.prefab", anchorGroupName);
        //     Debug.Log($"loadKey: {loadKey}");
        //     GameObject model = await AddressableRes.loadAddressableResAsync<GameObject>(loadKey);
        //     return GameObject.Instantiate(model);
        // }
        //
        // public void LoadAnchorGroup2(string anchorGroupName, UnityAction<GameObject> onSuccess) {
        //     string loadKey = string.Format("Assets/AddressableRes/Prefab/Anchors/{0}.prefab", anchorGroupName);
        //     AsyncOperationHandle<GameObject> handler = Addressables.InstantiateAsync(loadKey);
        //     handler.Completed += (operation) => { onSuccess?.Invoke(operation.Result); };
        // }

        public GameObject targetFx = null;

        public async void CreateTargetPosFx()
        {
            targetFx = await CreatorFx("BattleOperator/TargetPosEffect");
            HideTargetPos();
        }

        /// <summary>
        /// 改变目标点的位置
        /// </summary>
        public void ShowTargetPos(Vector3 pos)
        {
            if (targetFx == null) return;
            if (targetFx.layer != 0)
            {
                targetFx.SetActive(true);
            }
            targetFx.transform.position = pos;
        }

        /// <summary>
        /// 消除目标点
        /// </summary>
        public void HideTargetPos()
        {
            if (targetFx != null)
            {
                targetFx.SetActive(false);
            }
        }
    }
}