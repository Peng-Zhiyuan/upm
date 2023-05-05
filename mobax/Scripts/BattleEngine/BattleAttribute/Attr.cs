namespace BattleEngine.Logic
{
    using System;
    using System.Collections.Generic;

    public sealed class Attr
    {
        public AttrType type;
        public int value;
        public string name;
    }

    public sealed class BaseAttr
    {
        public Dictionary<AttrType, Attr> Attrs = new Dictionary<AttrType, Attr>();

        public BaseAttr()
        {
            RegistAll();
        }

        public void RegistAll()
        {
            foreach (var VARIABLE in Enum.GetValues(typeof(AttrType)))
            {
                if ((AttrType)VARIABLE == AttrType.None)
                    continue;
                RegistAttr((AttrType)VARIABLE);
            }
        }

        public void RegistAttr(AttrType type)
        {
            Attr abli = new Attr();
            abli.type = type;
            abli.value = 0;
            abli.name = type.ToString();
            Attrs[type] = abli;
        }

        /// <summary>
        /// 获取一个基础属性
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public Attr GetAttr(AttrType type)
        {
            Attr abil = null;
            if (Attrs.TryGetValue(type, out abil))
            {
                return abil;
            }
            return null;
        }

        /// <summary>
        /// 获取基础属性值
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public int GetAttrValue(AttrType type)
        {
            Attr abil = null;
            if (Attrs.TryGetValue(type, out abil))
            {
                return abil.value;
            }
            return 0;
        }

        /// <summary>
        /// 属性变化
        /// </summary>
        /// <param name="type"></param>
        /// <param name="val"></param>
        public void MergeAblity(AttrType type, int val)
        {
            var abli = GetAttr(type);
            if (abli == null)
                return;
            Increse(abli, val);
        }

        /// <summary>
        /// 属性做加法计算
        /// </summary>
        /// <param name="abli"></param>
        /// <param name="val"></param>
        public void Increse(Attr abli, int val)
        {
            abli.value += val;
        }

        public void Clean()
        {
            var data = Attrs.GetEnumerator();
            while (data.MoveNext())
            {
                data.Current.Value.value = 0;
            }
        }
    }
}