namespace BattleEngine.Logic
{
    using System.Collections.Generic;
    using System;

    /// <summary>
    /// 条件管理组件，在这里管理一个战斗实体所有条件达成事件的添加监听、移除监听、触发流程
    /// </summary>
    public sealed class ConditionManageComponent : Component
    {
        private Dictionary<Action, ConditionEntity> Conditions { get; set; } = new Dictionary<Action, ConditionEntity>();

        public override void Setup()
        {
            base.Setup();
        }

        public void AddListener(ConditionType conditionType, Action action, object paramObj = null)
        {
            ConditionEntity conditionEntity = null;
            switch (conditionType)
            {
                case ConditionType.WhenInTimeNoDamage:
                    conditionEntity = Entity.CreateWithParent<WhenInTimeNoDamageCondition>(Entity, paramObj);
                    break;
                case ConditionType.WhenHPLower:
                    var condition = Entity.CreateWithParent<WhenHPLowerCondition>(Entity, paramObj);
                    Conditions.Add(action, condition);
                    condition.StartListen(action);
                    break;
                case ConditionType.WhenHPPctLower:
                    WhenHPPctLowerCondition HPLowerPctCondition = Entity.CreateWithParent<WhenHPPctLowerCondition>(Entity, paramObj);
                    Conditions.Add(action, HPLowerPctCondition);
                    HPLowerPctCondition.StartListen(action);
                    break;
            }
            if (conditionEntity != null)
            {
                Conditions.Add(action, conditionEntity);
                conditionEntity.StartListen(action);
            }
        }

        public void RemoveListener(ConditionType conditionType, Action action)
        {
            if (Conditions.ContainsKey(action))
            {
                Entity.Destroy(Conditions[action]);
                Conditions.Remove(action);
            }
        }
    }
}