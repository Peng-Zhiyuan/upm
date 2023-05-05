using System;
using System.Collections.Generic;
using UnityEngine;

public enum ReminderType
{
    None = 0, // 一般模式
    ShowNum, // 显示数字
    AllDone, // 子节点需全部满足条件，该点才能算完成（比如同时几个道具的数量都要满足， 即其子节点是关注几个道具数量的）
}

/// <summary>
/// 提醒系统之单结点
/// </summary>
public class ReminderNode
{
    public Transform Comp
    {
        set
        {
            if (_comp == value) return;
            // 原先的移除
            // _comp?.onRemovedFromStage.Remove(_OnCompRemoved);
            // 如果已经绑定了其他组件，需要把那个给移除掉
            var newComp = value;
            if (newComp == null)
            {
                // 如果新结点指向null，那么原来的comp要清掉红点显示
                _SetCompFlag(0);
                _comp = null;
                return;
            }

            if (_compMap.TryGetValue(newComp, out var node))
            {
                node.Comp = null;
                _compMap.Remove(newComp);
            }

            // newComp.onRemovedFromStage.Add(_OnCompRemoved);
            _compMap.Add(newComp, this);
            _comp = newComp;
            UpdateUI();
        }
    }

    // 是否提醒了红点
    public bool Reminding => _value > 0;
    public string Key => _key;

    private static Dictionary<string, ReminderNode> _nodeMap; // key对应node的字典
    private static Dictionary<Transform, ReminderNode> _compMap; // comp对应node的字典
    private string _key;
    private Transform _comp;
    private int _value;
    private int _size; // size of children
    private int _capability; // capability of the array
    private ReminderNode[] _children;
    private ReminderNode _parent;
    private ReminderType _reminderType;
    private Action<bool> _changeHandler;
    private ReminderNode _alias; // 别名
    private ReminderNode _self; // 本尊（即如果只是别名的node， 那它有一个本尊）

    static ReminderNode()
    {
        _nodeMap = new Dictionary<string, ReminderNode>(256);
        _compMap = new Dictionary<Transform, ReminderNode>(256);
    }

    public static ReminderNode Get(string key)
    {
        _nodeMap.TryGetValue(key, out var node);
        if (null == node)
        {
            node = new ReminderNode
            {
                _key = key,
                _reminderType = ReminderType.None,
            };
            _nodeMap[key] = node;
        }

        return node;
    }

    public static bool Has(string key)
    {
        return _nodeMap.ContainsKey(key);
    }

    /// <summary>
    /// 取消一个组件的红点关联
    /// </summary>
    /// <param name="comp"></param>
    public static void CancelComp(Transform comp)
    {
        if (_compMap.TryGetValue(comp, out var node))
        {
            node.Comp = null;
            _compMap.Remove(comp);
        }
    }

    /// <summary>
    /// 保证要增加的个数的容量
    /// </summary>
    /// <param name="addedNum">增加的数量</param>
    public void EnsureAddedCapability(int addedNum)
    {
        EnsureCapability(_size + addedNum);
    }

    /// <summary>
    /// 保证容量充足
    /// </summary>
    /// <param name="num"></param>
    public void EnsureCapability(int num)
    {
        if (null == _children)
        {
            _capability = num;
            _children = new ReminderNode[_capability];
        }
        else
        {
            var newCapability = num;
            if (newCapability > _capability)
            {
                // 创建新数组， 来承载新的数据， 尽可能让这个逻辑少跑到
                var newNodes = new ReminderNode[newCapability];
                if (_size > 0)
                {
                    Array.Copy(_children, 0, newNodes, 0, _size);
                }

                _capability = newCapability;
                _children = newNodes;
            }
        }
    }

    /// <summary>
    /// 目前支持直接加ReminderNode或者是string类型的key
    /// </summary>
    /// <param name="vals"></param>
    /// <returns></returns>
    public ReminderNode Add<T>(params T[] vals)
    {
        // 保证容量
        EnsureAddedCapability(vals.Length);
        // 把新数据加进去
        var addedNum = 0;
        for (var i = 0; i < vals.Length; i++)
        {
            var val = vals[i];
            var node = _GetNode(val);
            if (node._parent == this)
            {
                // 这种情况是已经加到里面的，就不处理了
            }
            else if (node._parent != null)
            {
                throw new Exception($"[Fetal] Reminder: Node({node._key}) already has parent node, Please Check!");
            }
            else
            {
                _children[_size + addedNum] = node;
                node._parent = this;
                ++addedNum;
            }
        }

        if (addedNum > 0)
        {
            _size = _size + addedNum;
            // 最后还要更新一下结点value
            _RefreshNodeValue();
        }

        return this;
    }

    public ReminderNode Bind(MonoBehaviour comp)
    {
        Comp = comp.transform;
        return this;
    }

    /// <summary>
    /// 改变时的回调
    /// </summary>
    /// <param name="changeHandler"></param>
    public ReminderNode AddChange(Action<bool> changeHandler)
    {
        _changeHandler -= changeHandler;
        _changeHandler += changeHandler;
        return this;
    }

    /// <summary>
    /// 清除某一条回调
    /// </summary>
    /// <param name="changeHandler"></param>
    public ReminderNode RemoveChange(Action<bool> changeHandler)
    {
        _changeHandler -= changeHandler;
        return this;
    }

    public ReminderNode ClearChange()
    {
        _changeHandler = null;
        return this;
    }

    // 设置别名
    public ReminderNode Alias<T>(T val)
    {
        var node = _GetNode(val);

        // 先要清掉原来的本尊
        if (null != node._self)
        {
            node._self._alias = null;
        }

        _alias = node;
        node._self = this;
        // 然后还要把本体的value也设给它
        node.SetValue(_value);
        return node;
    }

    public ReminderNode ClearAlias()
    {
        _alias = null;
        return this;
    }

    /** 单个node移除 **/
    public bool Remove<T>(T val)
    {
        var node = _GetNode(val);
        var index = Array.IndexOf(_children, node);

        if (index >= 0)
        {
            --_size;
            if (index < _size)
            {
                Array.Copy(_children, index + 1, _children, index, _size - index);
            }

            _children[_size] = null;

            // 最后还要更新一下结点value
            _RefreshNodeValue();
            node.Clear();
            return true;
        }

        return false;
    }

    public void Dispose()
    {
        // 如果有父节点
        if (_parent != null)
        {
            _parent.Remove(this);
            _parent = null;
        }
        else
        {
            Clear();
        }

        ClearChildren();
    }

    public virtual void Clear()
    {
        // 字典的关联也要删掉
        if (null != _key)
        {
            _nodeMap?.Remove(_key);
        }
        
        // 清空数据
        _value = 0;
        _key = null;
        _parent = null;
        _alias = null;
        _reminderType = 0;
    }

    public void RemoveChildren()
    {
        if (null != _children)
        {
            for (var i = 0; i < _size; i++)
            {
                var nodeChild = _children[i];
                nodeChild._parent = null;
            }

            Array.Clear(_children, 0, _size);
            _size = 0;

            // 结点重置为0
            SetValue(0);
        }
    }

    public void ClearChildren()
    {
        if (null != _children)
        {
            for (var i = 0; i < _size; i++)
            {
                var nodeChild = _children[i];
                nodeChild.Clear();
                nodeChild.ClearChildren();
            }

            Array.Clear(_children, 0, _size);
            _size = 0;

            // 结点重置为0
            SetValue(0);
        }
    }

    public int ChildrenSumValue()
    {
        int sum = 0;
        int doneNum = 0;
        for (var i = 0; i < _size; i++)
        {
            var node = _children[i];
            sum += node._value;
            // 只有大于0的才需要加进去
            if (node._value > 0)
            {
                ++doneNum;
            }
        }

        switch (_reminderType)
        {
            case ReminderType.None:
                return sum > 0 ? 1 : 0;
            case ReminderType.ShowNum:
                return sum;
            case ReminderType.AllDone:
                return doneNum >= _size ? 1 : 0;
        }

        return 0;
    }

    /// <summary>
    /// 设置新值
    /// </summary>
    /// <param name="val">要更改的新值</param>
    /// <returns></returns>
    public bool SetValue(int val)
    {
        // 如果不一样才需要更新
        if (_value != val)
        {
            // 显示数字或者是有一个是0
            if (ReminderType.ShowNum == _reminderType || _value * val == 0)
            {
                _value = val;
                // 更新自己的红点显示
                UpdateUI();
                // 派发数值改变
                _changeHandler?.Invoke(val > 0);
                // 更新父节点的红点
                var node = this;
                while ((node = node._parent) != null)
                {
                    int newValue = node.ChildrenSumValue();
                    // 如果值没有更新就不需要继续向上通知了
                    if (!node.SetValue(newValue)) break;
                }

                // 然后还需要更新别名红点的值
                _alias?.SetValue(val);

                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 设置是否显示数字，
    /// 设置
    /// </summary>
    /// <returns></returns>
    public ReminderNode SetShowNumMode()
    {
        _reminderType = ReminderType.ShowNum;
        return this;
    }

    /// <summary>
    /// 设置是否子节点需要全部完成才能算该结点完成
    /// </summary>
    /// <returns></returns>
    public ReminderNode SetAllDoneMode()
    {
        _reminderType = ReminderType.AllDone;
        return this;
    }

    public bool SetValue(bool val)
    {
        return SetValue(val ? 1 : 0);
    }

    public bool HasChildren()
    {
        return _size > 0;
    }

    public void UpdateUI()
    {
        if (null != _comp)
        {
            _SetCompFlag(_value);
        }
    }

    private ReminderNode _GetNode<T>(T val)
    {
        if (val is ReminderNode node)
        {
            return node;
        }
        else if (val is string nodeKey)
        {
            return Get(nodeKey);
        }

        return null;
    }

    private bool _RefreshNodeValue()
    {
        return SetValue(ChildrenSumValue());
    }

    private void _SetCompFlag(int value)
    {
        var reminderUi = _GetReminderView();
        if (null == reminderUi) return;

        // 如果显示数字，则逻辑如下
        if (ReminderType.ShowNum == _reminderType)
        {
            // 暂时没这种需求
            // 有这种需求了，在这里把逻辑实现即可
        }
        else
        {
            // 只显示红点的逻辑
            reminderUi.SetActive(value > 0);
        }
    }

    private void _OnCompRemoved()
    {
        if (_comp != null)
        {
            // 关闭后也得删除
            _compMap.Remove(_comp);
            // 这个也得移除
            // _comp.onRemovedFromStage.Remove(_OnCompRemoved);
            // 同时要把红点也清掉
            _SetCompFlag(0);
            // 如果界面被关闭了，那么不需要再去响应变化了
            _comp = null;
        }
    }

    private Transform _GetReminderView()
    {
        var reminderComp = _comp.GetComponent<IReminderNodeView>();
        var reminderUi = reminderComp != null ? reminderComp.Reminder : _comp.Find("reminder");

        if (null == reminderUi)
        {
            Debug.LogError("[Reminder]Component named \"reminder\" needed in this comp.");
        }

        return reminderUi;
    }
}

/// <summary>
/// 道具相关的提醒结点
/// </summary>
public class ItemReminderNode : ReminderNode
{
    public int itemId;

    // 提醒的界限，比如是一个道具的需求数值。到达这个数值，就满足
    public int remindLine;
    private int _itemType;

    public ItemReminderNode(int itemId, int remindLine)
    {
        this.itemId = itemId;
        this.remindLine = remindLine;

        var has = Database.Stuff.itemDatabase.GetHoldCount(itemId);
        SetValue(has >= remindLine);
        // 增加监听
        Database.Stuff.itemDatabase.AddItemChange(itemId, _OnItemUpdate);
    }

    public override void Clear()
    {
        base.Clear();
        Database.Stuff.itemDatabase.RemoveItemChange(itemId, _OnItemUpdate);
    }

    private void _OnItemUpdate(int changedItemId)
    {
        var num = Database.Stuff.itemDatabase.GetHoldCount(changedItemId);
        SetValue(num >= remindLine);
    }
    
    ~ItemReminderNode()
    {
    }
}

/// <summary>
/// 提醒系统（红点系统）
/// </summary>
public static class Reminder
{
    public static ReminderNode GetNode(string key)
    {
        return ReminderNode.Get(key);
    }

    public static bool HasNode(string key)
    {
        return ReminderNode.Has(key);
    }

    /// <summary>
    /// 监听道具改变
    /// </summary>
    /// <param name="key"></param>
    /// <param name="itemInfos"></param>
    public static void ListenItems<T>(string key, List<T> itemInfos) where T : ICostItem
    {
        var node = GetNode(key);
        ListenItems(node, itemInfos);
    }
    
    public static void ListenItems<T>(ReminderNode node, List<T> itemInfos) where T : ICostItem
    {
        // 先清掉之前的节点
        UnListenItems(node);
        var itemNodes = new ItemReminderNode[itemInfos.Count];
        for (var i = 0; i < itemInfos.Count; i++)
        {
            var itemInfo = itemInfos[i];
            itemNodes[i] = new ItemReminderNode(itemInfo.Id, itemInfo.Num);
        }

        node.SetAllDoneMode().Add(itemNodes);
    }

    /// <summary>
    /// 取消监听道具改变
    /// </summary>
    /// <param name="key"></param>
    public static void UnListenItems(string key)
    {
        if (!HasNode(key)) return;

        GetNode(key).ClearChildren();
    }
    
    public static void UnListenItems(ReminderNode node)
    {
        node.ClearChildren();
    }

    /// <summary>
    /// 取消一个组件的红点关联
    /// </summary>
    /// <param name="comp"></param>
    public static void CancelComp(Component comp)
    {
        ReminderNode.CancelComp(comp.transform);
    }

    /// <summary>
    /// 取消一个组件的红点关联
    /// </summary>
    /// <param name="comp"></param>
    public static void CancelComp(Transform comp)
    {
        ReminderNode.CancelComp(comp);
    }

    /// <summary>
    /// 清掉结点
    /// </summary>
    /// <param name="key"></param>
    public static void Clear(string key)
    {
        GetNode(key).Dispose();
    }

    /// <summary>
    /// 将key绑定到组件上
    /// </summary>
    /// <param name="key"></param>
    /// <param name="comp">绑定的组件里面必须有reminder的子组件</param>
    public static ReminderNode Bind(string key, Component comp)
    {
        var node = ReminderNode.Get(key);
        node.Comp = comp.transform;
        return node;
    }

    /// <summary>
    /// 将key绑定到组件上
    /// </summary>
    /// <param name="key"></param>
    /// <param name="comp">绑定的组件里面必须有reminder的子组件</param>
    public static ReminderNode Bind(string key, Transform comp)
    {
        var node = ReminderNode.Get(key);
        node.Comp = comp;
        return node;
    }

    /// <summary>
    /// 清除绑定
    /// </summary>
    /// <param name="key"></param>
    public static void ClearBind(string key)
    {
        var node = ReminderNode.Get(key);
        node.Comp = null;
    }

    /// <summary>
    /// 添加对应key值改变时的回调
    /// </summary>
    /// <param name="key"></param>
    /// <param name="changeHandler"></param>
    /// <returns></returns>
    public static ReminderNode AddChange(string key, Action<bool> changeHandler)
    {
        return ReminderNode.Get(key).AddChange(changeHandler);
    }

    public static ReminderNode RemoveChange(string key, Action<bool> changeHandler)
    {
        return ReminderNode.Get(key).RemoveChange(changeHandler);
    }

    public static ReminderNode ClearChange(string key)
    {
        return ReminderNode.Get(key).ClearChange();
    }

    public static void SetValue(string key, bool value)
    {
        SetValue(key, value ? 1 : 0);
    }

    /// <summary>
    /// 设置key对应的值
    /// </summary>
    /// <param name="key"></param>
    /// <param name="val"></param>
    /// <returns></returns>
    public static bool SetValue(string key, int val)
    {
        var node = GetNode(key);
        return SetValue(node, val);
    }

    public static bool SetValue(ReminderNode node, int val)
    {
        // 有子节点的不允许更新
        if (node.HasChildren())
        {
            throw new Exception($"[Fetal] Node({node.Key}) that has child nodes is not allowed set value manually");
        }

        return node.SetValue(val);
    }

    /// <summary>
    /// 单独显示红点（跟这一套系统无关）
    /// </summary>
    /// <param name="comp"></param>
    public static void On(Transform comp)
    {
        _Display(comp, true);
    }


    /// <summary>
    /// 单独隐藏红点（跟这一套系统无关）
    /// </summary>
    /// <param name="comp"></param>
    public static void Off(Transform comp)
    {
        _Display(comp, false);
    }

    /// <summary>
    /// 对一个组件使用红点（这里是额外提供一个这样的功能）
    /// </summary>
    /// <param name="comp"></param>
    private static void _Display(Transform comp, bool on)
    {
        var reminderUi = comp.Find("reminder");

        // 只显示红点的逻辑
        reminderUi.SetActive(on);
    }
}