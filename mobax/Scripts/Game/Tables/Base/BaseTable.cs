using UnityEngine;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;

namespace Table
{
    public abstract class BaseTable
    {
        protected bool m_isLoaded;
        public bool IsLoaded { get { return m_isLoaded; } }
    }
    public abstract class TBaseTable<TKey, TItem, TTable> : BaseTable
        where TTable : TBaseTable<TKey, TItem, TTable>, new()
        where TItem : ITableItem<TKey>, new()
    {
        public Dictionary<TKey, TItem> m_items;

        static private TTable m_instance;
        static public TTable Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new TTable();
                    m_instance.initialize();
                }
                return m_instance;
            }
        }
        public static TItem GetItem(TKey key)
        {
            return Instance.getItem(key);
        }

        public static Dictionary<TKey, TItem> Items
        {
            get { return Instance.items; }
        }

        public static TItem GetIndexItem(int index)
        {
            return Instance.getItem(Instance.items.Keys.ToArray()[index -1]);
        }

        public TItem getItem(TKey key)
        {
            TItem ret;
            m_items.TryGetValue(key, out ret);
            return ret;
        }

        public Dictionary<TKey, TItem> items
        {
            get { return m_items; }
        }

        protected abstract void initialize();
        protected async void load(string name)
        {
            try
            {
                //TextAsset ta = await AddressableRes.loadAddressableResAsync<UnityEngine.Object>($"Assets/AddressableRes/Config/{name}.json", false) as TextAsset;
                var address = $"{name}.json";
                //var ta = await AddressableRes.AquireAsync<TextAsset>(address);
                var bucket = BucketManager.Stuff.Main;
                var ta = await bucket.GetOrAquireAsync<TextAsset>(address);

                if (ta == null)
                    return;

                string data = ta.text;

                int start = data.IndexOf("[");
                int end = data.LastIndexOf("]");
                data = data.Substring(start, end + 1 - start);
                loadData(data);
            }
            catch (System.Exception ex)
            {
                Debug.Assert(false, ex.Message);
            }
            m_isLoaded = true;
            return;
        }

    public virtual void loadData(string stream)
    {
            TItem[] items = JsonConvert.DeserializeObject<TItem[]>(stream);
            m_items = new Dictionary<TKey, TItem>((int)items.Length);
            foreach(TItem item in items)
            {
                item.initialize();
#if UNITY_EDITOR
                if (m_items.ContainsKey(item.GetKey()))
                    Debug.LogErrorFormat("ID == {0} 重复!", item.GetKey());
#endif
                m_items[item.GetKey()] = item;
            }
    }
}
    public interface ISerializable
    {
        bool initialize();
    }

    public interface ITableItem<TKey>:ISerializable
    {
        TKey GetKey();
    }
}

public partial class ConfigLoader
{
    public static List<Table.BaseTable> m_tables = new List<Table.BaseTable>();
    public static Table.BaseTable AddTable(Table.BaseTable table)
    {
        m_tables.Add(table);
        return table;
    }
    public bool IsTableLoadedAll()
    {
        foreach (var table in m_tables)
        {
            if (!table.IsLoaded) return false;
        }
        return true;
    }
}

