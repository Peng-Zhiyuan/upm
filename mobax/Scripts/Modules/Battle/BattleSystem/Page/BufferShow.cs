using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BattleEngine.Logic;
using UnityEngine;
using UnityEngine.UI;

namespace Modules.Battle.BattleSystem.Page
{
    public class BufferBar
    {
        public Transform Root;
        public string prefab;
        public List<BufferShow> Buffers = new List<BufferShow>();

        public BufferBar(Transform root, string prefab)
        {
            this.Root = root;
            this.prefab = prefab;

            TimerMgr.Instance.BattleSchedulerTimer(0.1f, () =>
            {
                foreach (var VARIABLE in Buffers)
                {
                    VARIABLE.Update(0.1f);
                }
            }, true);
        }

        public void SetData(CombatActorEntity entity)
        {
            foreach (var VARIABLE in Buffers)
            {
                VARIABLE.Destroyed();
            }
            Buffers.Clear();
            
            if(entity == null)
                return;
            foreach (var VARIABLE in entity.TypeIdBuffs)
            {
                if(string.IsNullOrEmpty(VARIABLE.Value.buffRow.Icon))
                    continue;
                
                Buffers.Add(new BufferShow(prefab, Root, VARIABLE.Key, VARIABLE.Value, VARIABLE.Value.StackNum));
            }
        }

        public void SetVisible(bool vis)
        {
            //Root.SetActive(vis);
            UiUtil.SetActive(Root.gameObject, vis);
        }
    }
    public class BufferShow
    {
        public GameObject root;
        public RecycledGameObject go;
        public Image icon;
        public Text count;
        public Text time;
        public Transform parent;

        public int buf_id;
        public BuffAbility buf;
        public int buf_count;
        public float t;

        public BufferShow(string prefab, Transform parent, int bufferID, BuffAbility buf, int count)
        {
            this.parent = parent;
            this.buf = buf;
            this.buf_count = count;
            this.t = buf.GetBuffMaxTime() - buf.GetBuffCurrentTime();
            Init(prefab);
            
            //Debug.LogError($"{buf.SelfActorEntity.UID}加个buffer：{buf.buffRow.BuffID} 时间：{t}");
        }

        public async Task Init(string prefab)
        {
            root = new GameObject("root");
            root.AddComponent<RectTransform>();
            root.transform.SetParent(this.parent);
            root.transform.localPosition = Vector3.zero;
            go = await GameObjectPoolUtil.ReuseAddressableObjectAsync<RecycledGameObject>(BucketManager.Stuff.Battle, prefab);
            icon = go.transform.Find("Icon").GetComponent<Image>();
            count = go.transform.Find("Count").GetComponent<Text>();
            time = go.transform.Find("Time").GetComponent<Text>();
            go.transform.SetParent(root.transform);
            go.transform.localPosition = Vector3.zero;
            go.transform.localScale = Vector3.one;
            
            Refresh();
        }

        public void Refresh()
        {
            if (icon != null)
            {
                if(buf == null)
                    return;
                
                UiUtil.SetSpriteInBackground(icon, ()=> buf.buffRow.Icon + ".png", null, 1f, null, false, false );
                count.text = buf_count.ToString();
                //Debug.LogError("Buffer icon = " + buf.buffRow.Icon);
            }
        }

        public void Update(float deltime)
        {
            if(time == null)
                return;
            
            //t -= deltime;
            //time.text = Math.Round(t, 1) + "s";
        }

        public void Destroyed()
        {
            if(go != null)
                go.Recycle();
            
            if(root != null)
                GameObject.Destroy(root);
        }
    }
}