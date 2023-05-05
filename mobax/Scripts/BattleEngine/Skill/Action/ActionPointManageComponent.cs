namespace BattleEngine.Logic
{
    using System.Collections.Generic;
    using System;

    /// <summary>
    /// 行动点，一次战斗行动<see cref="ActionExecution"/>会触发战斗实体一系列的行动点
    /// </summary>
    public sealed class ActionPoint
    {
        public List<Action<ActionExecution>> Listeners { get; set; } = new List<Action<ActionExecution>>();
    }

    /// <summary>
    /// 行动点管理器，在这里管理一个战斗实体所有行动点的添加监听、移除监听、触发流程
    /// </summary>
    public sealed class ActionPointManageComponent : Component
    {
        private Dictionary<ACTION_POINT_TYPE, ActionPoint> ActionPoints { get; set; } = new Dictionary<ACTION_POINT_TYPE, ActionPoint>();

        public override void Setup()
        {
            base.Setup();
        }

        public void AddListener(ACTION_POINT_TYPE actionPointType, Action<ActionExecution> action)
        {
            if (!ActionPoints.ContainsKey(actionPointType))
            {
                ActionPoints.Add(actionPointType, new ActionPoint());
            }
            ActionPoints[actionPointType].Listeners.Add(action);
        }

        public void RemoveListener(ACTION_POINT_TYPE actionPointType, Action<ActionExecution> action)
        {
            if (ActionPoints.ContainsKey(actionPointType))
            {
                ActionPoints[actionPointType].Listeners.Remove(action);
            }
        }

        public void TriggerActionPoint(ACTION_POINT_TYPE actionPointType, ActionExecution action)
        {
            if (ActionPoints.ContainsKey(actionPointType)
                && ActionPoints[actionPointType].Listeners.Count > 0)
            {
                for (int i = ActionPoints[actionPointType].Listeners.Count - 1; i >= 0; i--)
                {
                    var item = ActionPoints[actionPointType].Listeners[i];
                    item.Invoke(action);
                }
            }
        }
    }
}