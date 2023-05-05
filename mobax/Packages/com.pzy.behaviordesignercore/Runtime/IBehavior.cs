using UnityEngine;

namespace BehaviorDesigner.Runtime
{
    /// <summary>
    /// 行为树系统基础接口
    /// 行为树组件与行为树数据资产都实现这个接口
    /// </summary>
    public interface IBehavior
    {
        string GetOwnerName();
        int GetInstanceID();
        BehaviorSource GetBehaviorSource();
        void SetBehaviorSource(BehaviorSource behaviorSource);

        /// <summary>
        /// 将自身转换为 UnityEngine.Object 类型
        /// </summary>
        /// <returns></returns>
        object GetObject();
        SharedVariable GetVariable(string name);
        void SetVariable(string name, SharedVariable item);
        void SetVariableValue(string name, object value);
    }
}