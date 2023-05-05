using System.Collections.Generic;

namespace BattleEngine.Logic
{
    /// <summary>
    /// 战斗行动管理组件
    /// </summary>
    public class CombatActionManageComponent : Component
    {
        private List<ActionExecution> CombatActions = new List<ActionExecution>();

        public T CreateAction<T>(CombatActorEntity combatEntity) where T : ActionExecution
        {
            var action = Entity.CreateWithParent<T>(combatEntity) as T;
            action.Creator = combatEntity;
            CombatActions.Add(action);
            return action;
        }

        public void DestoryAction(ActionExecution action)
        {
            CombatActions.Remove(action);
            Entity.Destroy(action);
        }

        public void ClearAllActions()
        {
            for (int i = 0; i < CombatActions.Count; i++)
            {
                Entity.Destroy(CombatActions[i]);
            }
            CombatActions.Clear();
        }
    }
}