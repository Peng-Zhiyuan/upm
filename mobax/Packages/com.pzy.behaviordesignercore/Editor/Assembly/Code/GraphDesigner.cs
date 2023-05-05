namespace BehaviorDesigner.Editor
{
    using BehaviorDesigner.Runtime;
    using BehaviorDesigner.Runtime.Tasks;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using UnityEditor;
    using UnityEngine;
    using Object = UnityEngine.Object;

    [Serializable]
    public class GraphDesigner : ScriptableObject
    {
        private NodeDesigner mEntryNode;
        private NodeDesigner mRootNode;
        private List<NodeDesigner> mDetachedNodes = new List<NodeDesigner>();
        [SerializeField]
        private List<NodeDesigner> mSelectedNodes = new List<NodeDesigner>();
        private NodeDesigner mHoverNode;
        private NodeConnection mActiveNodeConnection;
        [SerializeField]
        private List<NodeConnection> mSelectedNodeConnections = new List<NodeConnection>();
        [SerializeField]
        private int mNextTaskID;
        private List<int> mNodeSelectedID = new List<int>();
        [SerializeField]
        private int[] mPrevNodeSelectedID;

        private NodeDesigner AddNode(BehaviorDesigner.Runtime.BehaviorSource behaviorSource, BehaviorDesigner.Runtime.Tasks.Task task, Vector2 position)
        {
            if (this.mEntryNode == null)
            {
                BehaviorDesigner.Runtime.Tasks.Task task2 = Activator.CreateInstance(BehaviorDesigner.Runtime.TaskUtility.GetTypeWithinAssembly("BehaviorDesigner.Runtime.Tasks.EntryTask")) as BehaviorDesigner.Runtime.Tasks.Task;
                this.mEntryNode = ScriptableObject.CreateInstance<NodeDesigner>();
                this.mEntryNode.LoadNode(task2, behaviorSource, new Vector2(position.x, position.y - 120f), ref this.mNextTaskID);
                this.mEntryNode.MakeEntryDisplay();
            }
            NodeDesigner item = ScriptableObject.CreateInstance<NodeDesigner>();
            item.LoadNode(task, behaviorSource, position, ref this.mNextTaskID);
            BehaviorDesigner.Runtime.Tasks.TaskNameAttribute[] attributeArray = null;
            if ((attributeArray = task.GetType().GetCustomAttributes(typeof(BehaviorDesigner.Runtime.Tasks.TaskNameAttribute), false) as BehaviorDesigner.Runtime.Tasks.TaskNameAttribute[]).Length > 0)
            {
                task.FriendlyName = attributeArray[0].Name;
            }
            if (this.mEntryNode.OutgoingNodeConnections.Count != 0)
            {
                this.mDetachedNodes.Add(item);
            }
            else
            {
                this.mActiveNodeConnection = ScriptableObject.CreateInstance<NodeConnection>();
                this.mActiveNodeConnection.LoadConnection(this.mEntryNode, NodeConnectionType.Outgoing);
                this.ConnectNodes(behaviorSource, item);
            }
            return item;
        }

        public NodeDesigner AddNode(BehaviorDesigner.Runtime.BehaviorSource behaviorSource, System.Type type, Vector2 position)
        {
            BehaviorDesigner.Runtime.Tasks.Task task = Activator.CreateInstance(type, true) as BehaviorDesigner.Runtime.Tasks.Task;
            if (task == null)
            {
                UnityEditor.EditorUtility.DisplayDialog("Unable to Add Task", string.Format("Unable to create task of type {0}. Is the class name the same as the file name?", type), "OK");
                return null;
            }
            try
            {
                task.OnReset();
            }
            catch (Exception)
            {
            }
            return this.AddNode(behaviorSource, task, position);
        }

        private void CheckForLastConnectionRemoval(NodeDesigner nodeDesigner)
        {
            if (nodeDesigner.IsEntryDisplay)
            {
                if (nodeDesigner.OutgoingNodeConnections.Count == 1)
                {
                    this.RemoveConnection(nodeDesigner.OutgoingNodeConnections[0]);
                }
            }
            else
            {
                BehaviorDesigner.Runtime.Tasks.ParentTask task = nodeDesigner.Task as BehaviorDesigner.Runtime.Tasks.ParentTask;
                if ((task.Children != null) && ((task.Children.Count + 1) > task.MaxChildren()))
                {
                    NodeConnection nodeConnection = null;
                    int num = 0;
                    while (true)
                    {
                        if (num < nodeDesigner.OutgoingNodeConnections.Count)
                        {
                            if (!nodeDesigner.OutgoingNodeConnections[num].DestinationNodeDesigner.Equals(task.Children[task.Children.Count - 1].NodeData.NodeDesigner as NodeDesigner))
                            {
                                num++;
                                continue;
                            }
                            nodeConnection = nodeDesigner.OutgoingNodeConnections[num];
                        }
                        if (nodeConnection != null)
                        {
                            this.RemoveConnection(nodeConnection);
                        }
                        break;
                    }
                }
            }
        }

        private void Clear(NodeDesigner nodeDesigner)
        {
            if (nodeDesigner != null)
            {
                if (nodeDesigner.IsParent)
                {
                    BehaviorDesigner.Runtime.Tasks.ParentTask task = nodeDesigner.Task as BehaviorDesigner.Runtime.Tasks.ParentTask;
                    if ((task != null) && (task.Children != null))
                    {
                        for (int i = task.Children.Count - 1; i > -1; i--)
                        {
                            if (task.Children[i] != null)
                            {
                                this.Clear(task.Children[i].NodeData.NodeDesigner as NodeDesigner);
                            }
                        }
                    }
                }
                nodeDesigner.DestroyConnections();
                Object.DestroyImmediate(nodeDesigner, true);
            }
        }

        public void Clear(bool saveSelectedNodes)
        {
            if (!saveSelectedNodes)
            {
                this.mPrevNodeSelectedID = null;
            }
            else if (this.mNodeSelectedID.Count > 0)
            {
                this.mPrevNodeSelectedID = this.mNodeSelectedID.ToArray();
            }
            this.mNodeSelectedID.Clear();
            this.mSelectedNodes.Clear();
            this.mSelectedNodeConnections.Clear();
            this.DestroyNodeDesigners();
        }

        public void ClearConnectionSelection()
        {
            for (int i = 0; i < this.mSelectedNodeConnections.Count; i++)
            {
                this.mSelectedNodeConnections[i].deselect();
            }
            this.mSelectedNodeConnections.Clear();
        }

        public void ClearHover()
        {
            if (this.HoverNode)
            {
                this.HoverNode.ShowHoverBar = false;
                this.HoverNode = null;
            }
        }

        public void ClearNodeSelection()
        {
            if (this.mSelectedNodes.Count == 1)
            {
                this.IndicateReferencedTasks(this.mSelectedNodes[0].Task, false);
            }
            for (int i = 0; i < this.mSelectedNodes.Count; i++)
            {
                this.mSelectedNodes[i].Deselect();
            }
            this.mSelectedNodes.Clear();
            this.mNodeSelectedID.Clear();
        }

        public void ConnectNodes(BehaviorDesigner.Runtime.BehaviorSource behaviorSource, NodeDesigner nodeDesigner)
        {
            NodeConnection mActiveNodeConnection = this.mActiveNodeConnection;
            this.mActiveNodeConnection = null;
            if ((mActiveNodeConnection != null) && !mActiveNodeConnection.OriginatingNodeDesigner.Equals(nodeDesigner))
            {
                NodeDesigner originatingNodeDesigner = mActiveNodeConnection.OriginatingNodeDesigner;
                if (mActiveNodeConnection.NodeConnectionType == NodeConnectionType.Outgoing)
                {
                    this.RemoveParentConnection(nodeDesigner);
                    this.CheckForLastConnectionRemoval(originatingNodeDesigner);
                    originatingNodeDesigner.AddChildNode(nodeDesigner, mActiveNodeConnection, true, false);
                }
                else
                {
                    this.RemoveParentConnection(originatingNodeDesigner);
                    this.CheckForLastConnectionRemoval(nodeDesigner);
                    nodeDesigner.AddChildNode(originatingNodeDesigner, mActiveNodeConnection, true, false);
                }
                if (mActiveNodeConnection.OriginatingNodeDesigner.IsEntryDisplay)
                {
                    this.mRootNode = mActiveNodeConnection.DestinationNodeDesigner;
                }
                this.mDetachedNodes.Remove(mActiveNodeConnection.DestinationNodeDesigner);
            }
        }

        public List<TaskSerializer> Copy(Vector2 graphOffset, float graphZoom)
        {
            List<TaskSerializer> list = new List<TaskSerializer>();
            for (int i = 0; i < this.mSelectedNodes.Count; i++)
            {
                TaskSerializer item = TaskCopier.CopySerialized(this.mSelectedNodes[i].Task);
                if (item != null)
                {
                    if (this.mSelectedNodes[i].IsParent)
                    {
                        BehaviorDesigner.Runtime.Tasks.ParentTask task = this.mSelectedNodes[i].Task as BehaviorDesigner.Runtime.Tasks.ParentTask;
                        if (task.Children != null)
                        {
                            List<int> list2 = new List<int>();
                            int index = -1;
                            int num3 = 0;
                            while (true)
                            {
                                if (num3 >= task.Children.Count)
                                {
                                    item.childrenIndex = list2;
                                    break;
                                }
                                index = this.mSelectedNodes.IndexOf(task.Children[num3].NodeData.NodeDesigner as NodeDesigner);
                                if (index != -1)
                                {
                                    list2.Add(index);
                                }
                                num3++;
                            }
                        }
                    }
                    item.offset = (item.offset + graphOffset) * graphZoom;
                    list.Add(item);
                }
            }
            return ((list.Count <= 0) ? null : list);
        }

        private bool CycleExists(NodeDesigner nodeDesigner, ref HashSet<NodeDesigner> set)
        {
            if (set.Contains(nodeDesigner))
            {
                return true;
            }
            set.Add(nodeDesigner);
            if (nodeDesigner.IsParent)
            {
                BehaviorDesigner.Runtime.Tasks.ParentTask task = nodeDesigner.Task as BehaviorDesigner.Runtime.Tasks.ParentTask;
                if (task.Children != null)
                {
                    for (int i = 0; i < task.Children.Count; i++)
                    {
                        if (this.CycleExists(task.Children[i].NodeData.NodeDesigner as NodeDesigner, ref set))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public bool Delete(BehaviorDesigner.Runtime.BehaviorSource behaviorSource, BehaviorDesignerWindow.TaskCallbackHandler callback)
        {
            bool flag = false;
            if (this.mSelectedNodeConnections != null)
            {
                int num = 0;
                while (true)
                {
                    if (num >= this.mSelectedNodeConnections.Count)
                    {
                        this.mSelectedNodeConnections.Clear();
                        flag = true;
                        break;
                    }
                    this.RemoveConnection(this.mSelectedNodeConnections[num]);
                    num++;
                }
            }
            if (this.mSelectedNodes != null)
            {
                int num2 = 0;
                while (true)
                {
                    if (num2 >= this.mSelectedNodes.Count)
                    {
                        this.mSelectedNodes.Clear();
                        flag = true;
                        break;
                    }
                    if (callback != null)
                    {
                        callback(behaviorSource, this.mSelectedNodes[num2].Task);
                    }
                    this.RemoveNode(this.mSelectedNodes[num2]);
                    num2++;
                }
            }
            if (flag)
            {
                // 不支持撤销
                //BehaviorUndo.RegisterUndo("Delete", behaviorSource.Owner.GetObject());
                TaskReferences.CheckReferences(behaviorSource);
                this.Save(behaviorSource);
            }
            return flag;
        }

        public void Deselect(NodeConnection nodeConnection)
        {
            this.mSelectedNodeConnections.Remove(nodeConnection);
            nodeConnection.deselect();
        }

        public void Deselect(NodeDesigner nodeDesigner)
        {
            this.mSelectedNodes.Remove(nodeDesigner);
            this.mNodeSelectedID.Remove(nodeDesigner.Task.ID);
            nodeDesigner.Deselect();
            this.IndicateReferencedTasks(nodeDesigner.Task, false);
        }

        public void DeselectAll(NodeDesigner exceptionNodeDesigner)
        {
            for (int i = this.mSelectedNodes.Count - 1; i >= 0; i--)
            {
                if ((exceptionNodeDesigner == null) || !this.mSelectedNodes[i].Equals(exceptionNodeDesigner))
                {
                    this.mSelectedNodes[i].Deselect();
                    this.mSelectedNodes.RemoveAt(i);
                    this.mNodeSelectedID.RemoveAt(i);
                }
            }
            if (exceptionNodeDesigner != null)
            {
                this.IndicateReferencedTasks(exceptionNodeDesigner.Task, false);
            }
        }

        public void DeselectWithParent(NodeDesigner nodeDesigner)
        {
            for (int i = this.mSelectedNodes.Count - 1; i >= 0; i--)
            {
                if (this.mSelectedNodes[i].HasParent(nodeDesigner))
                {
                    this.Deselect(this.mSelectedNodes[i]);
                }
            }
        }

        public void DestroyNodeDesigners()
        {
            if (this.mEntryNode != null)
            {
                this.Clear(this.mEntryNode);
            }
            if (this.mRootNode != null)
            {
                this.Clear(this.mRootNode);
            }
            for (int i = this.mDetachedNodes.Count - 1; i > -1; i--)
            {
                this.Clear(this.mDetachedNodes[i]);
            }
            this.mEntryNode = null;
            this.mRootNode = null;
            this.mDetachedNodes = new List<NodeDesigner>();
        }

        private void DragNode(NodeDesigner nodeDesigner, Vector2 delta, bool dragChildren)
        {
            if (!this.IsParentSelected(nodeDesigner) || !dragChildren)
            {
                nodeDesigner.ChangeOffset(delta);
                if (nodeDesigner.ParentNodeDesigner != null)
                {
                    int index = nodeDesigner.ParentNodeDesigner.ChildIndexForTask(nodeDesigner.Task);
                    if (index != -1)
                    {
                        int num2 = index - 1;
                        bool flag = false;
                        NodeDesigner designer = nodeDesigner.ParentNodeDesigner.NodeDesignerForChildIndex(num2);
                        if ((designer != null) && (nodeDesigner.Task.NodeData.Offset.x < designer.Task.NodeData.Offset.x))
                        {
                            nodeDesigner.ParentNodeDesigner.MoveChildNode(index, true);
                            flag = true;
                        }
                        if (!flag)
                        {
                            num2 = index + 1;
                            designer = nodeDesigner.ParentNodeDesigner.NodeDesignerForChildIndex(num2);
                            if ((designer != null) && (nodeDesigner.Task.NodeData.Offset.x > designer.Task.NodeData.Offset.x))
                            {
                                nodeDesigner.ParentNodeDesigner.MoveChildNode(index, false);
                            }
                        }
                    }
                }
                if (nodeDesigner.IsParent && !dragChildren)
                {
                    BehaviorDesigner.Runtime.Tasks.ParentTask task = nodeDesigner.Task as BehaviorDesigner.Runtime.Tasks.ParentTask;
                    if (task.Children != null)
                    {
                        for (int i = 0; i < task.Children.Count; i++)
                        {
                            (task.Children[i].NodeData.NodeDesigner as NodeDesigner).ChangeOffset(-delta);
                        }
                    }
                }
                this.MarkNodeDirty(nodeDesigner);
            }
        }

        public bool DragSelectedNodes(Vector2 delta, bool dragChildren)
        {
            if (this.mSelectedNodes.Count == 0)
            {
                return false;
            }
            bool flag = this.mSelectedNodes.Count == 1;
            for (int i = 0; i < this.mSelectedNodes.Count; i++)
            {
                this.DragNode(this.mSelectedNodes[i], delta, dragChildren);
            }
            if (flag && (dragChildren && (this.mSelectedNodes[0].IsEntryDisplay && (this.mRootNode != null))))
            {
                this.DragNode(this.mRootNode, delta, dragChildren);
            }
            return true;
        }

        private bool DrawNodeChildren(NodeDesigner nodeDesigner, Vector2 offset, bool disabledNode)
        {
            if (nodeDesigner == null)
            {
                return false;
            }
            bool flag = false;
            if (nodeDesigner.DrawNode(offset, false, disabledNode))
            {
                flag = true;
            }
            if (nodeDesigner.IsParent)
            {
                BehaviorDesigner.Runtime.Tasks.ParentTask task = nodeDesigner.Task as BehaviorDesigner.Runtime.Tasks.ParentTask;
                if (!task.NodeData.Collapsed && (task.Children != null))
                {
                    for (int i = task.Children.Count - 1; i > -1; i--)
                    {
                        if ((task.Children[i] != null) && this.DrawNodeChildren(task.Children[i].NodeData.NodeDesigner as NodeDesigner, offset, task.Disabled || disabledNode))
                        {
                            flag = true;
                        }
                    }
                }
            }
            return flag;
        }

        private void DrawNodeCommentChildren(NodeDesigner nodeDesigner, Vector2 offset)
        {
            if (nodeDesigner != null)
            {
                nodeDesigner.DrawNodeComment(offset);
                if (nodeDesigner.IsParent)
                {
                    BehaviorDesigner.Runtime.Tasks.ParentTask task = nodeDesigner.Task as BehaviorDesigner.Runtime.Tasks.ParentTask;
                    if (!task.NodeData.Collapsed && (task.Children != null))
                    {
                        for (int i = 0; i < task.Children.Count; i++)
                        {
                            if (task.Children[i] != null)
                            {
                                this.DrawNodeCommentChildren(task.Children[i].NodeData.NodeDesigner as NodeDesigner, offset);
                            }
                        }
                    }
                }
            }
        }

        private void DrawNodeConnectionChildren(NodeDesigner nodeDesigner, Vector2 offset, bool disabledNode)
        {
            if ((nodeDesigner != null) && !nodeDesigner.Task.NodeData.Collapsed)
            {
                nodeDesigner.DrawNodeConnection(offset, nodeDesigner.Task.Disabled || disabledNode);
                if (nodeDesigner.IsParent)
                {
                    BehaviorDesigner.Runtime.Tasks.ParentTask task = nodeDesigner.Task as BehaviorDesigner.Runtime.Tasks.ParentTask;
                    if (task.Children != null)
                    {
                        for (int i = 0; i < task.Children.Count; i++)
                        {
                            if (task.Children[i] != null)
                            {
                                this.DrawNodeConnectionChildren(task.Children[i].NodeData.NodeDesigner as NodeDesigner, offset, task.Disabled || disabledNode);
                            }
                        }
                    }
                }
            }
        }

        // TODO:
        // 没有正确绘制
        public bool DrawNodes(Vector2 mousePosition, Vector2 offset)
        {

            if (this.mEntryNode == null)
            {
                return false;
            }
            this.mEntryNode.DrawNodeConnection(offset, false);
            if (this.mRootNode != null)
            {
                this.DrawNodeConnectionChildren(this.mRootNode, offset, this.mRootNode.Task.Disabled);
             }
            for (int i = 0; i < this.mDetachedNodes.Count; i++)
            {
                this.DrawNodeConnectionChildren(this.mDetachedNodes[i], offset, this.mDetachedNodes[i].Task.Disabled);
            }
            for (int j = 0; j < this.mSelectedNodeConnections.Count; j++)
            {
                this.mSelectedNodeConnections[j].DrawConnection(offset, this.mSelectedNodeConnections[j].OriginatingNodeDesigner.IsDisabled());
            }
            if ((mousePosition != new Vector2(-1f, -1f)) && (this.mActiveNodeConnection != null))
            {
                this.mActiveNodeConnection.HorizontalHeight = (this.mActiveNodeConnection.OriginatingNodeDesigner.GetConnectionPosition(offset, this.mActiveNodeConnection.NodeConnectionType).y + mousePosition.y) / 2f;
                this.mActiveNodeConnection.DrawConnection(this.mActiveNodeConnection.OriginatingNodeDesigner.GetConnectionPosition(offset, this.mActiveNodeConnection.NodeConnectionType), mousePosition, (this.mActiveNodeConnection.NodeConnectionType == NodeConnectionType.Outgoing) && this.mActiveNodeConnection.OriginatingNodeDesigner.IsDisabled());
            }
            this.mEntryNode.DrawNode(offset, false, false);
            bool flag = false;
            if ((this.mRootNode != null) && this.DrawNodeChildren(this.mRootNode, offset, this.mRootNode.Task.Disabled))
            {
                flag = true;
            }
            for (int k = 0; k < this.mDetachedNodes.Count; k++)
            {
                if (this.DrawNodeChildren(this.mDetachedNodes[k], offset, this.mDetachedNodes[k].Task.Disabled))
                {
                    flag = true;
                }
            }
            for (int m = 0; m < this.mSelectedNodes.Count; m++)
            {
                if (this.mSelectedNodes[m].DrawNode(offset, true, this.mSelectedNodes[m].IsDisabled()))
                {
                    flag = true;
                }
            }
            if (this.mRootNode != null)
            {
                this.DrawNodeCommentChildren(this.mRootNode, offset);
            }
            for (int n = 0; n < this.mDetachedNodes.Count; n++)
            {
                this.DrawNodeCommentChildren(this.mDetachedNodes[n], offset);
            }





            return flag;
        }

        public Vector2 EntryNodeOffset()
        {
            return this.mEntryNode.Task.NodeData.Offset;
        }

        public void Find(string findTaskValue, BehaviorDesigner.Runtime.SharedVariable findSharedVariable)
        {
            if (findTaskValue != null)
            {
                findTaskValue = findTaskValue.ToLower();
            }
            if (this.mRootNode != null)
            {
                this.Find(this.mRootNode, findTaskValue, findSharedVariable);
            }
            for (int i = 0; i < this.mDetachedNodes.Count; i++)
            {
                this.Find(this.mDetachedNodes[i], findTaskValue, findSharedVariable);
            }
        }

        private void Find(NodeDesigner nodeDesigner, string findTaskValue, BehaviorDesigner.Runtime.SharedVariable findSharedVariable)
        {
            if ((nodeDesigner != null) && (nodeDesigner.Task != null))
            {
                bool found = false;
                if (!string.IsNullOrEmpty(findTaskValue) && (findTaskValue.Length > 2))
                {
                    if (nodeDesigner.Task.FriendlyName.ToLower().Contains(findTaskValue))
                    {
                        found = true;
                    }
                    else if (nodeDesigner.Task.GetType().FullName.ToLower().Contains(findTaskValue))
                    {
                        found = true;
                    }
                }
                if (!found)
                {
                    FieldInfo[] serializableFields = BehaviorDesigner.Runtime.TaskUtility.GetSerializableFields(nodeDesigner.Task.GetType());
                    for (int i = 0; i < serializableFields.Length; i++)
                    {
                        System.Type fieldType = serializableFields[i].FieldType;
                        if ((findSharedVariable != null) && typeof(BehaviorDesigner.Runtime.SharedVariable).IsAssignableFrom(fieldType))
                        {
                            BehaviorDesigner.Runtime.SharedVariable variable = serializableFields[i].GetValue(nodeDesigner.Task) as BehaviorDesigner.Runtime.SharedVariable;
                            if ((variable != null) && ((variable.Name == findSharedVariable.Name) && (variable.IsGlobal == findSharedVariable.IsGlobal)))
                            {
                                found = true;
                                break;
                            }
                        }
                        else if (BehaviorDesignerUtility.HasAttribute(serializableFields[i], typeof(BehaviorDesigner.Runtime.Tasks.InspectTaskAttribute)))
                        {
                            if (!typeof(IList).IsAssignableFrom(serializableFields[i].FieldType))
                            {
                                found = this.FindInspectedTask(serializableFields[i].GetValue(nodeDesigner.Task) as BehaviorDesigner.Runtime.Tasks.Task, findTaskValue, findSharedVariable);
                            }
                            else
                            {
                                IList list = serializableFields[i].GetValue(nodeDesigner.Task) as IList;
                                if (list != null)
                                {
                                    for (int j = 0; j < list.Count; j++)
                                    {
                                        if (this.FindInspectedTask(list[j] as BehaviorDesigner.Runtime.Tasks.Task, findTaskValue, findSharedVariable))
                                        {
                                            found = true;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                nodeDesigner.FoundTask(found);
                if (nodeDesigner.IsParent)
                {
                    BehaviorDesigner.Runtime.Tasks.ParentTask task = nodeDesigner.Task as BehaviorDesigner.Runtime.Tasks.ParentTask;
                    if (task.Children != null)
                    {
                        for (int i = 0; i < task.Children.Count; i++)
                        {
                            if (task.Children[i] != null)
                            {
                                this.Find(task.Children[i].NodeData.NodeDesigner as NodeDesigner, findTaskValue, findSharedVariable);
                            }
                        }
                    }
                }
            }
        }

        private bool FindInspectedTask(BehaviorDesigner.Runtime.Tasks.Task inspectedTask, string findTaskValue, BehaviorDesigner.Runtime.SharedVariable findSharedVariable)
        {
            if (inspectedTask != null)
            {
                if (!string.IsNullOrEmpty(findTaskValue) && ((findTaskValue.Length > 2) && (inspectedTask.FriendlyName.ToLower().Contains(findTaskValue) || inspectedTask.GetType().FullName.ToLower().Contains(findTaskValue))))
                {
                    return true;
                }
                FieldInfo[] publicFields = BehaviorDesigner.Runtime.TaskUtility.GetPublicFields(inspectedTask.GetType());
                for (int i = 0; i < publicFields.Length; i++)
                {
                    if ((findSharedVariable != null) && typeof(BehaviorDesigner.Runtime.SharedVariable).IsAssignableFrom(publicFields[i].FieldType))
                    {
                        BehaviorDesigner.Runtime.SharedVariable variable = publicFields[i].GetValue(inspectedTask) as BehaviorDesigner.Runtime.SharedVariable;
                        if ((variable != null) && ((variable.Name == findSharedVariable.Name) && (variable.IsGlobal == findSharedVariable.IsGlobal)))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public List<BehaviorDesigner.Runtime.BehaviorSource> FindReferencedBehaviors()
        {
            List<BehaviorDesigner.Runtime.BehaviorSource> behaviors = new List<BehaviorDesigner.Runtime.BehaviorSource>();
            if (this.mRootNode != null)
            {
                this.FindReferencedBehaviors(this.mRootNode, ref behaviors);
            }
            for (int i = 0; i < this.mDetachedNodes.Count; i++)
            {
                this.FindReferencedBehaviors(this.mDetachedNodes[i], ref behaviors);
            }
            return behaviors;
        }

        public void FindReferencedBehaviors(NodeDesigner nodeDesigner, ref List<BehaviorDesigner.Runtime.BehaviorSource> behaviors)
        {
            FieldInfo[] publicFields = BehaviorDesigner.Runtime.TaskUtility.GetPublicFields(nodeDesigner.Task.GetType());
            for (int i = 0; i < publicFields.Length; i++)
            {
                System.Type fieldType = publicFields[i].FieldType;
                if (!typeof(IList).IsAssignableFrom(fieldType))
                {
                    if (typeof(BehaviorDesigner.Runtime.ExternalBehavior).IsAssignableFrom(fieldType) || typeof(BehaviorDesigner.Runtime.Behavior).IsAssignableFrom(fieldType))
                    {
                        object obj2 = publicFields[i].GetValue(nodeDesigner.Task);
                        if (obj2 != null)
                        {
                            BehaviorDesigner.Runtime.BehaviorSource item = null;
                            if (obj2 is BehaviorDesigner.Runtime.ExternalBehavior)
                            {
                                item = (obj2 as BehaviorDesigner.Runtime.ExternalBehavior).BehaviorSource;
                                if (item.Owner == null)
                                {
                                    item.Owner = obj2 as BehaviorDesigner.Runtime.ExternalBehavior;
                                }
                                behaviors.Add(item);
                            }
                            else
                            {
                                item = (obj2 as BehaviorDesigner.Runtime.Behavior).GetBehaviorSource();
                                if (item.Owner == null)
                                {
                                    item.Owner = obj2 as BehaviorDesigner.Runtime.Behavior;
                                }
                            }
                            behaviors.Add(item);
                        }
                    }
                }
                else
                {
                    System.Type c = fieldType;
                    if (!fieldType.IsGenericType)
                    {
                        c = fieldType.GetElementType();
                    }
                    else
                    {
                        while (true)
                        {
                            if (c.IsGenericType)
                            {
                                c = fieldType.GetGenericArguments()[0];
                                break;
                            }
                            c = c.BaseType;
                        }
                    }
                    if (c != null)
                    {
                        if (!typeof(BehaviorDesigner.Runtime.ExternalBehavior).IsAssignableFrom(c) && !typeof(BehaviorDesigner.Runtime.Behavior).IsAssignableFrom(c))
                        {
                            if (typeof(BehaviorDesigner.Runtime.Behavior).IsAssignableFrom(c))
                            {
                            }
                        }
                        else
                        {
                            IList list = publicFields[i].GetValue(nodeDesigner.Task) as IList;
                            if (list != null)
                            {
                                for (int j = 0; j < list.Count; j++)
                                {
                                    if (list[j] != null)
                                    {
                                        BehaviorDesigner.Runtime.BehaviorSource item = null;
                                        if (list[j] is BehaviorDesigner.Runtime.ExternalBehavior)
                                        {
                                            item = (list[j] as BehaviorDesigner.Runtime.ExternalBehavior).BehaviorSource;
                                            if (item.Owner == null)
                                            {
                                                item.Owner = list[j] as BehaviorDesigner.Runtime.ExternalBehavior;
                                            }
                                        }
                                        else
                                        {
                                            item = (list[j] as BehaviorDesigner.Runtime.Behavior).GetBehaviorSource();
                                            if (item.Owner == null)
                                            {
                                                item.Owner = list[j] as BehaviorDesigner.Runtime.Behavior;
                                            }
                                        }
                                        behaviors.Add(item);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            if (nodeDesigner.IsParent)
            {
                BehaviorDesigner.Runtime.Tasks.ParentTask task = nodeDesigner.Task as BehaviorDesigner.Runtime.Tasks.ParentTask;
                if (task.Children != null)
                {
                    for (int j = 0; j < task.Children.Count; j++)
                    {
                        if (task.Children[j] != null)
                        {
                            this.FindReferencedBehaviors(task.Children[j].NodeData.NodeDesigner as NodeDesigner, ref behaviors);
                        }
                    }
                }
            }
        }

        private void GetNodeMinMax(Vector2 offset, NodeDesigner nodeDesigner, ref Rect minMaxRect)
        {
            Rect rect = nodeDesigner.Rectangle(offset, true, true);
            if (rect.xMin < minMaxRect.xMin)
            {
                minMaxRect.xMin = (rect.xMin);
            }
            if (rect.yMin < minMaxRect.yMin)
            {
                minMaxRect.yMin = (rect.yMin);
            }
            if (rect.xMax > minMaxRect.xMax)
            {
                minMaxRect.xMax = (rect.xMax);
            }
            if (rect.yMax > minMaxRect.yMax)
            {
                minMaxRect.yMax = (rect.yMax);
            }
            if (nodeDesigner.IsParent)
            {
                BehaviorDesigner.Runtime.Tasks.ParentTask task = nodeDesigner.Task as BehaviorDesigner.Runtime.Tasks.ParentTask;
                if (task.Children != null)
                {
                    for (int i = 0; i < task.Children.Count; i++)
                    {
                        this.GetNodeMinMax(offset, task.Children[i].NodeData.NodeDesigner as NodeDesigner, ref minMaxRect);
                    }
                }
            }
        }

        public int GetTaskCount()
        {
            int count = this.mDetachedNodes.Count;
            if (this.mRootNode != null)
            {
                count += this.GetTaskCount(this.mRootNode);
            }
            return count;
        }

        private int GetTaskCount(NodeDesigner nodeDesigner)
        {
            int num = 1;
            if (nodeDesigner.Task.GetType().IsSubclassOf(typeof(BehaviorDesigner.Runtime.Tasks.ParentTask)))
            {
                BehaviorDesigner.Runtime.Tasks.ParentTask task = nodeDesigner.Task as BehaviorDesigner.Runtime.Tasks.ParentTask;
                if (task.Children != null)
                {
                    for (int i = 0; i < task.Children.Count; i++)
                    {
                        num += this.GetTaskCount(task.Children[i].NodeData.NodeDesigner as NodeDesigner);
                    }
                }
            }
            return num;
        }

        public void GraphDirty()
        {
            if (this.mEntryNode != null)
            {
                this.mEntryNode.MarkDirty();
                if (this.mRootNode != null)
                {
                    this.MarkNodeDirty(this.mRootNode);
                }
                for (int i = this.mDetachedNodes.Count - 1; i > -1; i--)
                {
                    this.MarkNodeDirty(this.mDetachedNodes[i]);
                }
            }
        }

        public Rect GraphSize(Vector3 offset)
        {
            if (this.mEntryNode == null)
            {
                return new Rect();
            }
            Rect minMaxRect = new Rect();
            minMaxRect.xMin = (float.MaxValue);
            minMaxRect.xMax = (float.MinValue);
            minMaxRect.yMin = (float.MaxValue);
            minMaxRect.yMax = (float.MinValue);
            this.GetNodeMinMax(offset, this.mEntryNode, ref minMaxRect);
            if (this.mRootNode != null)
            {
                this.GetNodeMinMax(offset, this.mRootNode, ref minMaxRect);
            }
            for (int i = 0; i < this.mDetachedNodes.Count; i++)
            {
                this.GetNodeMinMax(offset, this.mDetachedNodes[i], ref minMaxRect);
            }
            return minMaxRect;
        }

        public bool HasEntryNode()
        {
            return ((this.mEntryNode != null) && !object.ReferenceEquals(this.mEntryNode.Task, null));
        }

        public void Hover(NodeDesigner nodeDesigner)
        {
            if (!nodeDesigner.ShowHoverBar)
            {
                nodeDesigner.ShowHoverBar = true;
                this.HoverNode = nodeDesigner;
            }
        }

        public void IdentifyNode(NodeDesigner nodeDesigner)
        {
            nodeDesigner.IdentifyNode();
        }

        private void IndicateReferencedTasks(BehaviorDesigner.Runtime.Tasks.Task task, bool indicate)
        {
            List<BehaviorDesigner.Runtime.Tasks.Task> referencedTasks = TaskInspector.GetReferencedTasks(task);
            NodeDesigner nodeDesigner = null;
            if ((referencedTasks != null) && (referencedTasks.Count > 0))
            {
                for (int i = 0; i < referencedTasks.Count; i++)
                {
                    if ((referencedTasks[i] != null) && (referencedTasks[i].NodeData != null))
                    {
                        nodeDesigner = referencedTasks[i].NodeData.NodeDesigner as NodeDesigner;
                        if (nodeDesigner != null)
                        {
                            nodeDesigner.ShowReferenceIcon = indicate;
                        }
                    }
                }
            }
        }

        public bool IsParentSelected(NodeDesigner nodeDesigner)
        {
            return ((nodeDesigner.ParentNodeDesigner != null) && (!this.IsSelected(nodeDesigner.ParentNodeDesigner) ? this.IsParentSelected(nodeDesigner.ParentNodeDesigner) : true));
        }

        public bool IsSelected(NodeConnection nodeConnection)
        {
            for (int i = 0; i < this.mSelectedNodeConnections.Count; i++)
            {
                if (this.mSelectedNodeConnections[i].Equals(nodeConnection))
                {
                    return true;
                }
            }
            return false;
        }

        public bool IsSelected(NodeDesigner nodeDesigner)
        {
            return this.mSelectedNodes.Contains(nodeDesigner);
        }

        public bool Load(BehaviorDesigner.Runtime.BehaviorSource behaviorSource, bool loadPrevBehavior, Vector2 nodePosition)
        {
            BehaviorDesigner.Runtime.Tasks.Task task;
            BehaviorDesigner.Runtime.Tasks.Task task2;
            List<BehaviorDesigner.Runtime.Tasks.Task> list2;
            if (behaviorSource == null)
            {
                this.Clear(false);
                return false;
            }
            this.DestroyNodeDesigners();
            if ((behaviorSource.Owner == null) || (!(behaviorSource.Owner is BehaviorDesigner.Runtime.Behavior) || ((behaviorSource.Owner as BehaviorDesigner.Runtime.Behavior).ExternalBehavior == null)))
            {
                behaviorSource.CheckForSerialization(!Application.isPlaying, null);
            }
            else
            {
                List<BehaviorDesigner.Runtime.SharedVariable> allVariables = null;
                bool force = !Application.isPlaying;
                if (Application.isPlaying && !(behaviorSource.Owner as BehaviorDesigner.Runtime.Behavior).HasInheritedVariables)
                {
                    behaviorSource.CheckForSerialization(true, null);
                    allVariables = behaviorSource.GetAllVariables();
                    (behaviorSource.Owner as BehaviorDesigner.Runtime.Behavior).HasInheritedVariables = true;
                    force = true;
                }
                BehaviorDesigner.Runtime.ExternalBehavior externalBehavior = (behaviorSource.Owner as BehaviorDesigner.Runtime.Behavior).ExternalBehavior;
                externalBehavior.BehaviorSource.Owner = externalBehavior;
                externalBehavior.BehaviorSource.CheckForSerialization(force, behaviorSource);
                if (allVariables != null)
                {
                    for (int i = 0; i < allVariables.Count; i++)
                    {
                        behaviorSource.SetVariable(allVariables[i].Name, allVariables[i]);
                    }
                }
            }
            if ((behaviorSource.EntryTask == null) && ((behaviorSource.RootTask == null) && (behaviorSource.DetachedTasks == null)))
            {
                this.Clear(false);
                return false;
            }
            if (!loadPrevBehavior)
            {
                this.Clear(false);
            }
            else
            {
                this.mSelectedNodes.Clear();
                this.mSelectedNodeConnections.Clear();
                if (this.mPrevNodeSelectedID != null)
                {
                    int index = 0;
                    while (true)
                    {
                        if (index >= this.mPrevNodeSelectedID.Length)
                        {
                            this.mPrevNodeSelectedID = null;
                            break;
                        }
                        this.mNodeSelectedID.Add(this.mPrevNodeSelectedID[index]);
                        index++;
                    }
                }
            }
            this.mNextTaskID = 0;
            this.mEntryNode = null;
            this.mRootNode = null;
            this.mDetachedNodes.Clear();
            behaviorSource.Load(out task, out task2, out list2);
            if (BehaviorDesignerUtility.AnyNullTasks(behaviorSource) || ((behaviorSource.TaskData != null) && (BehaviorDesignerUtility.HasRootTask(behaviorSource.TaskData.JSONSerialization) && (behaviorSource.RootTask == null))))
            {
                behaviorSource.CheckForSerialization(true, null);
                behaviorSource.Load(out task, out task2, out list2);
            }
            if (task != null)
            {
                this.mEntryNode = ScriptableObject.CreateInstance<NodeDesigner>();
                this.mEntryNode.LoadTask(task, (behaviorSource.Owner == null) ? null : (behaviorSource.Owner.GetObject() as BehaviorDesigner.Runtime.Behavior), ref this.mNextTaskID);
                this.mEntryNode.MakeEntryDisplay();
            }
            else if ((task2 != null) || ((list2 != null) && (list2.Count > 0)))
            {
                behaviorSource.EntryTask = task = Activator.CreateInstance(BehaviorDesigner.Runtime.TaskUtility.GetTypeWithinAssembly("BehaviorDesigner.Runtime.Tasks.EntryTask"), true) as BehaviorDesigner.Runtime.Tasks.Task;
                this.mEntryNode = ScriptableObject.CreateInstance<NodeDesigner>();
                if (task2 != null)
                {
                    this.mEntryNode.LoadNode(task, behaviorSource, new Vector2(task2.NodeData.Offset.x, task2.NodeData.Offset.y - 120f), ref this.mNextTaskID);
                }
                else
                {
                    this.mEntryNode.LoadNode(task, behaviorSource, new Vector2(nodePosition.x, nodePosition.y - 120f), ref this.mNextTaskID);
                }
                this.mEntryNode.MakeEntryDisplay();
            }
            if (task2 != null)
            {
                this.mRootNode = ScriptableObject.CreateInstance<NodeDesigner>();
                this.mRootNode.LoadTask(task2, (behaviorSource.Owner == null) ? null : (behaviorSource.Owner.GetObject() as BehaviorDesigner.Runtime.Behavior), ref this.mNextTaskID);
                NodeConnection nodeConnection = ScriptableObject.CreateInstance<NodeConnection>();
                nodeConnection.LoadConnection(this.mEntryNode, NodeConnectionType.Fixed);
                this.mEntryNode.AddChildNode(this.mRootNode, nodeConnection, false, false);
                this.LoadNodeSelection(this.mRootNode);
                if (this.mEntryNode.OutgoingNodeConnections.Count == 0)
                {
                    this.mActiveNodeConnection = ScriptableObject.CreateInstance<NodeConnection>();
                    this.mActiveNodeConnection.LoadConnection(this.mEntryNode, NodeConnectionType.Outgoing);
                    this.ConnectNodes(behaviorSource, this.mRootNode);
                }
            }
            if (list2 != null)
            {
                for (int i = 0; i < list2.Count; i++)
                {
                    if (list2[i] != null)
                    {
                        NodeDesigner item = ScriptableObject.CreateInstance<NodeDesigner>();
                        item.LoadTask(list2[i], (behaviorSource.Owner == null) ? null : (behaviorSource.Owner.GetObject() as BehaviorDesigner.Runtime.Behavior), ref this.mNextTaskID);
                        this.mDetachedNodes.Add(item);
                        this.LoadNodeSelection(item);
                    }
                }
            }
            return true;
        }

        private void LoadNodeSelection(NodeDesigner nodeDesigner)
        {
            if (nodeDesigner != null)
            {
                if ((this.mNodeSelectedID != null) && this.mNodeSelectedID.Contains(nodeDesigner.Task.ID))
                {
                    this.Select(nodeDesigner, false);
                }
                if (nodeDesigner.IsParent)
                {
                    BehaviorDesigner.Runtime.Tasks.ParentTask task = nodeDesigner.Task as BehaviorDesigner.Runtime.Tasks.ParentTask;
                    if (task.Children != null)
                    {
                        for (int i = 0; i < task.Children.Count; i++)
                        {
                            if ((task.Children[i] != null) && (task.Children[i].NodeData != null))
                            {
                                this.LoadNodeSelection(task.Children[i].NodeData.NodeDesigner as NodeDesigner);
                            }
                        }
                    }
                }
            }
        }

        private void MarkNodeDirty(NodeDesigner nodeDesigner)
        {
            nodeDesigner.MarkDirty();
            if (nodeDesigner.IsEntryDisplay)
            {
                if ((nodeDesigner.OutgoingNodeConnections.Count > 0) && (nodeDesigner.OutgoingNodeConnections[0].DestinationNodeDesigner != null))
                {
                    this.MarkNodeDirty(nodeDesigner.OutgoingNodeConnections[0].DestinationNodeDesigner);
                }
            }
            else if (nodeDesigner.IsParent)
            {
                BehaviorDesigner.Runtime.Tasks.ParentTask task = nodeDesigner.Task as BehaviorDesigner.Runtime.Tasks.ParentTask;
                if (task.Children != null)
                {
                    for (int i = 0; i < task.Children.Count; i++)
                    {
                        if (task.Children[i] != null)
                        {
                            this.MarkNodeDirty(task.Children[i].NodeData.NodeDesigner as NodeDesigner);
                        }
                    }
                }
            }
        }

        public NodeDesigner NodeAt(Vector2 point, Vector2 offset)
        {
            if (this.mEntryNode == null)
            {
                return null;
            }
            for (int i = 0; i < this.mSelectedNodes.Count; i++)
            {
                if (this.mSelectedNodes[i].Contains(point, offset, false))
                {
                    return this.mSelectedNodes[i];
                }
            }
            NodeDesigner designer = null;
            for (int j = this.mDetachedNodes.Count - 1; j > -1; j--)
            {
                if ((this.mDetachedNodes[j] != null) && ((designer = this.NodeChildrenAt(this.mDetachedNodes[j], point, offset)) != null))
                {
                    return designer;
                }
            }
            return (((this.mRootNode == null) || ((designer = this.NodeChildrenAt(this.mRootNode, point, offset)) == null)) ? (!this.mEntryNode.Contains(point, offset, true) ? null : this.mEntryNode) : designer);
        }

        public bool NodeCanAcceptConnection(NodeDesigner nodeDesigner, NodeConnection connection)
        {
            if (!((!nodeDesigner.IsEntryDisplay || (connection.NodeConnectionType != NodeConnectionType.Incoming)) ? (!nodeDesigner.IsEntryDisplay && (nodeDesigner.IsParent || (!nodeDesigner.IsParent && (connection.NodeConnectionType == NodeConnectionType.Outgoing)))) : true))
            {
                return false;
            }
            if (nodeDesigner.IsEntryDisplay || connection.OriginatingNodeDesigner.IsEntryDisplay)
            {
                return true;
            }
            HashSet<NodeDesigner> set = new HashSet<NodeDesigner>();
            NodeDesigner designer = (connection.NodeConnectionType != NodeConnectionType.Outgoing) ? connection.OriginatingNodeDesigner : nodeDesigner;
            NodeDesigner item = (connection.NodeConnectionType != NodeConnectionType.Outgoing) ? nodeDesigner : connection.OriginatingNodeDesigner;
            return (!this.CycleExists(designer, ref set) ? !set.Contains(item) : false);
        }

        public bool NodeCanOriginateConnection(NodeDesigner nodeDesigner, NodeConnection connection)
        {
            return (!nodeDesigner.IsEntryDisplay || (nodeDesigner.IsEntryDisplay && (connection.NodeConnectionType == NodeConnectionType.Outgoing)));
        }

        private NodeDesigner NodeChildrenAt(NodeDesigner nodeDesigner, Vector2 point, Vector2 offset)
        {
            if (nodeDesigner != null)
            {
                if (nodeDesigner.Contains(point, offset, true))
                {
                    return nodeDesigner;
                }
                if (nodeDesigner.IsParent)
                {
                    BehaviorDesigner.Runtime.Tasks.ParentTask task = nodeDesigner.Task as BehaviorDesigner.Runtime.Tasks.ParentTask;
                    NodeDesigner designer = null;
                    if (!task.NodeData.Collapsed && (task.Children != null))
                    {
                        for (int i = 0; i < task.Children.Count; i++)
                        {
                            if ((task.Children[i] != null) && ((designer = this.NodeChildrenAt(task.Children[i].NodeData.NodeDesigner as NodeDesigner, point, offset)) != null))
                            {
                                return designer;
                            }
                        }
                    }
                }
            }
            return null;
        }

        private void NodeChildrenConnectionsAt(NodeDesigner nodeDesigner, Vector2 point, Vector2 offset, ref List<NodeConnection> nodeConnections)
        {
            if (!nodeDesigner.Task.NodeData.Collapsed)
            {
                nodeDesigner.ConnectionContains(point, offset, ref nodeConnections);
                if (nodeDesigner.IsParent)
                {
                    BehaviorDesigner.Runtime.Tasks.ParentTask task = nodeDesigner.Task as BehaviorDesigner.Runtime.Tasks.ParentTask;
                    if ((task != null) && (task.Children != null))
                    {
                        for (int i = 0; i < task.Children.Count; i++)
                        {
                            if (task.Children[i] != null)
                            {
                                this.NodeChildrenConnectionsAt(task.Children[i].NodeData.NodeDesigner as NodeDesigner, point, offset, ref nodeConnections);
                            }
                        }
                    }
                }
            }
        }

        public void NodeConnectionsAt(Vector2 point, Vector2 offset, ref List<NodeConnection> nodeConnections)
        {
            if (this.mEntryNode != null)
            {
                this.NodeChildrenConnectionsAt(this.mEntryNode, point, offset, ref nodeConnections);
                if (this.mRootNode != null)
                {
                    this.NodeChildrenConnectionsAt(this.mRootNode, point, offset, ref nodeConnections);
                }
                for (int i = 0; i < this.mDetachedNodes.Count; i++)
                {
                    this.NodeChildrenConnectionsAt(this.mDetachedNodes[i], point, offset, ref nodeConnections);
                }
            }
        }

        public List<NodeDesigner> NodesAt(Rect rect, Vector2 offset)
        {
            List<NodeDesigner> nodes = new List<NodeDesigner>();
            if (this.mRootNode != null)
            {
                this.NodesChildrenAt(this.mRootNode, rect, offset, ref nodes);
            }
            for (int i = 0; i < this.mDetachedNodes.Count; i++)
            {
                this.NodesChildrenAt(this.mDetachedNodes[i], rect, offset, ref nodes);
            }
            return ((nodes.Count <= 0) ? null : nodes);
        }

        private void NodesChildrenAt(NodeDesigner nodeDesigner, Rect rect, Vector2 offset, ref List<NodeDesigner> nodes)
        {
            if (nodeDesigner.Intersects(rect, offset))
            {
                nodes.Add(nodeDesigner);
            }
            if (nodeDesigner.IsParent)
            {
                BehaviorDesigner.Runtime.Tasks.ParentTask task = nodeDesigner.Task as BehaviorDesigner.Runtime.Tasks.ParentTask;
                if (!task.NodeData.Collapsed && (task.Children != null))
                {
                    for (int i = 0; i < task.Children.Count; i++)
                    {
                        if (task.Children[i] != null)
                        {
                            this.NodesChildrenAt(task.Children[i].NodeData.NodeDesigner as NodeDesigner, rect, offset, ref nodes);
                        }
                    }
                }
            }
        }

        public void OnEnable()
        {
            base.hideFlags =(HideFlags)(0x3d);
        }

        public bool Paste(BehaviorDesigner.Runtime.BehaviorSource behaviorSource, Vector3 position, List<TaskSerializer> copiedTasks, Vector2 graphOffset, float graphZoom)
        {
            if ((copiedTasks == null) || (copiedTasks.Count == 0))
            {
                return false;
            }
            this.ClearNodeSelection();
            this.ClearConnectionSelection();
            this.RemapIDs();
            List<NodeDesigner> list = new List<NodeDesigner>();
            for (int i = 0; i < copiedTasks.Count; i++)
            {
                TaskSerializer serializer = copiedTasks[i];
                BehaviorDesigner.Runtime.Tasks.Task task = TaskCopier.PasteTask(behaviorSource, serializer);
                NodeDesigner item = ScriptableObject.CreateInstance<NodeDesigner>();
                item.LoadTask(task, (behaviorSource.Owner == null) ? null : (behaviorSource.Owner.GetObject() as BehaviorDesigner.Runtime.Behavior), ref this.mNextTaskID);
                item.Task.NodeData.Offset = ((serializer.offset / graphZoom) - graphOffset);
                list.Add(item);
                this.mDetachedNodes.Add(item);
                this.Select(item);
            }
            for (int j = 0; j < copiedTasks.Count; j++)
            {
                TaskSerializer serializer2 = copiedTasks[j];
                if (serializer2.childrenIndex != null)
                {
                    for (int k = 0; k < serializer2.childrenIndex.Count; k++)
                    {
                        NodeDesigner nodeDesigner = list[j];
                        NodeConnection nodeConnection = ScriptableObject.CreateInstance<NodeConnection>();
                        nodeConnection.LoadConnection(nodeDesigner, NodeConnectionType.Outgoing);
                        nodeDesigner.AddChildNode(list[serializer2.childrenIndex[k]], nodeConnection, true, false);
                        this.mDetachedNodes.Remove(list[serializer2.childrenIndex[k]]);
                    }
                }
            }
            if (this.mEntryNode == null)
            {
                BehaviorDesigner.Runtime.Tasks.Task task = Activator.CreateInstance(BehaviorDesigner.Runtime.TaskUtility.GetTypeWithinAssembly("BehaviorDesigner.Runtime.Tasks.EntryTask")) as BehaviorDesigner.Runtime.Tasks.Task;
                this.mEntryNode = ScriptableObject.CreateInstance<NodeDesigner>();
                this.mEntryNode.LoadNode(task, behaviorSource, new Vector2(position.x, position.y - 120f), ref this.mNextTaskID);
                this.mEntryNode.MakeEntryDisplay();
                if (this.mDetachedNodes.Count > 0)
                {
                    this.mActiveNodeConnection = ScriptableObject.CreateInstance<NodeConnection>();
                    this.mActiveNodeConnection.LoadConnection(this.mEntryNode, NodeConnectionType.Outgoing);
                    this.ConnectNodes(behaviorSource, this.mDetachedNodes[0]);
                }
            }
            this.Save(behaviorSource);
            return true;
        }

        private void RemapIDs()
        {
            if (this.mEntryNode != null)
            {
                this.mNextTaskID = 0;
                this.mEntryNode.SetID(ref this.mNextTaskID);
                if (this.mRootNode != null)
                {
                    this.mRootNode.SetID(ref this.mNextTaskID);
                }
                for (int i = 0; i < this.mDetachedNodes.Count; i++)
                {
                    this.mDetachedNodes[i].SetID(ref this.mNextTaskID);
                }
                this.mNodeSelectedID.Clear();
                for (int j = 0; j < this.mSelectedNodes.Count; j++)
                {
                    this.mNodeSelectedID.Add(this.mSelectedNodes[j].Task.ID);
                }
            }
        }

        public void RemoveConnection(NodeConnection nodeConnection)
        {
            nodeConnection.DestinationNodeDesigner.Task.NodeData.Offset = (nodeConnection.DestinationNodeDesigner.GetAbsolutePosition());
            this.mDetachedNodes.Add(nodeConnection.DestinationNodeDesigner);
            nodeConnection.OriginatingNodeDesigner.RemoveChildNode(nodeConnection.DestinationNodeDesigner);
            if (nodeConnection.OriginatingNodeDesigner.IsEntryDisplay)
            {
                this.mRootNode = null;
            }
        }

        private void RemoveNode(NodeDesigner nodeDesigner)
        {
            if (!nodeDesigner.IsEntryDisplay)
            {
                if (nodeDesigner.IsParent)
                {
                    for (int i = 0; i < nodeDesigner.OutgoingNodeConnections.Count; i++)
                    {
                        NodeDesigner destinationNodeDesigner = nodeDesigner.OutgoingNodeConnections[i].DestinationNodeDesigner;
                        this.mDetachedNodes.Add(destinationNodeDesigner);
                        destinationNodeDesigner.Task.NodeData.Offset = (destinationNodeDesigner.GetAbsolutePosition());
                        destinationNodeDesigner.ParentNodeDesigner = null;
                    }
                }
                if (nodeDesigner.ParentNodeDesigner != null)
                {
                    nodeDesigner.ParentNodeDesigner.RemoveChildNode(nodeDesigner);
                }
                if ((this.mRootNode != null) && this.mRootNode.Equals(nodeDesigner))
                {
                    this.mEntryNode.RemoveChildNode(nodeDesigner);
                    this.mRootNode = null;
                }
                if (this.mRootNode != null)
                {
                    this.RemoveReferencedTasks(this.mRootNode, nodeDesigner.Task);
                }
                if (this.mDetachedNodes != null)
                {
                    for (int i = 0; i < this.mDetachedNodes.Count; i++)
                    {
                        this.RemoveReferencedTasks(this.mDetachedNodes[i], nodeDesigner.Task);
                    }
                }
                this.mDetachedNodes.Remove(nodeDesigner);
                BehaviorUndo.DestroyObject(nodeDesigner, false);
            }
        }

        private void RemoveParentConnection(NodeDesigner nodeDesigner)
        {
            if (nodeDesigner.ParentNodeDesigner != null)
            {
                NodeDesigner parentNodeDesigner = nodeDesigner.ParentNodeDesigner;
                NodeConnection nodeConnection = null;
                int num = 0;
                while (true)
                {
                    if (num < parentNodeDesigner.OutgoingNodeConnections.Count)
                    {
                        if (!parentNodeDesigner.OutgoingNodeConnections[num].DestinationNodeDesigner.Equals(nodeDesigner))
                        {
                            num++;
                            continue;
                        }
                        nodeConnection = parentNodeDesigner.OutgoingNodeConnections[num];
                    }
                    if (nodeConnection != null)
                    {
                        this.RemoveConnection(nodeConnection);
                    }
                    break;
                }
            }
        }

        private void RemoveReferencedTasks(NodeDesigner nodeDesigner, BehaviorDesigner.Runtime.Tasks.Task task)
        {
            bool fullSync = false;
            bool doReference = false;
            FieldInfo[] serializableFields = BehaviorDesigner.Runtime.TaskUtility.GetSerializableFields(nodeDesigner.Task.GetType());
            for (int i = 0; i < serializableFields.Length; i++)
            {
                if ((!serializableFields[i].IsPrivate && !serializableFields[i].IsFamily) || BehaviorDesignerUtility.HasAttribute(serializableFields[i], typeof(SerializeField)))
                {
                    if (!typeof(IList).IsAssignableFrom(serializableFields[i].FieldType))
                    {
                        if (typeof(BehaviorDesigner.Runtime.Tasks.Task).IsAssignableFrom(serializableFields[i].FieldType))
                        {
                            BehaviorDesigner.Runtime.Tasks.Task task2 = serializableFields[i].GetValue(nodeDesigner.Task) as BehaviorDesigner.Runtime.Tasks.Task;
                            if ((task2 != null) && (nodeDesigner.Task.Equals(task) || task2.Equals(task)))
                            {
                                TaskInspector.ReferenceTasks(nodeDesigner.Task, task, serializableFields[i], ref fullSync, ref doReference, false, false);
                            }
                        }
                    }
                    else if (typeof(BehaviorDesigner.Runtime.Tasks.Task).IsAssignableFrom(serializableFields[i].FieldType.GetElementType()) || (serializableFields[i].FieldType.IsGenericType && typeof(BehaviorDesigner.Runtime.Tasks.Task).IsAssignableFrom(serializableFields[i].FieldType.GetGenericArguments()[0])))
                    {
                        BehaviorDesigner.Runtime.Tasks.Task[] taskArray = serializableFields[i].GetValue(nodeDesigner.Task) as BehaviorDesigner.Runtime.Tasks.Task[];
                        if (taskArray != null)
                        {
                            for (int j = taskArray.Length - 1; j > -1; j--)
                            {
                                if ((taskArray[j] != null) && (nodeDesigner.Task.Equals(task) || taskArray[j].Equals(task)))
                                {
                                    TaskInspector.ReferenceTasks(nodeDesigner.Task, task, serializableFields[i], ref fullSync, ref doReference, false, false);
                                }
                            }
                        }
                    }
                }
            }
            if (nodeDesigner.IsParent)
            {
                BehaviorDesigner.Runtime.Tasks.ParentTask task3 = nodeDesigner.Task as BehaviorDesigner.Runtime.Tasks.ParentTask;
                if (task3.Children != null)
                {
                    for (int j = 0; j < task3.Children.Count; j++)
                    {
                        if (task3.Children[j] != null)
                        {
                            this.RemoveReferencedTasks(task3.Children[j].NodeData.NodeDesigner as NodeDesigner, task);
                        }
                    }
                }
            }
        }

        private bool RemoveSharedVariableReference(NodeDesigner nodeDesigner, BehaviorDesigner.Runtime.SharedVariable sharedVariable)
        {
            bool flag = false;
            FieldInfo[] serializableFields = BehaviorDesigner.Runtime.TaskUtility.GetSerializableFields(nodeDesigner.Task.GetType());
            for (int i = 0; i < serializableFields.Length; i++)
            {
                if (typeof(BehaviorDesigner.Runtime.SharedVariable).IsAssignableFrom(serializableFields[i].FieldType))
                {
                    BehaviorDesigner.Runtime.SharedVariable variable = serializableFields[i].GetValue(nodeDesigner.Task) as BehaviorDesigner.Runtime.SharedVariable;
                    if ((variable != null) && (!string.IsNullOrEmpty(variable.Name) && ((variable.IsGlobal == sharedVariable.IsGlobal) && variable.Name.Equals(sharedVariable.Name))))
                    {
                        if (!serializableFields[i].FieldType.IsAbstract)
                        {
                            variable = Activator.CreateInstance(serializableFields[i].FieldType) as BehaviorDesigner.Runtime.SharedVariable;
                            variable.IsShared = true;
                            serializableFields[i].SetValue(nodeDesigner.Task, variable);
                        }
                        flag = true;
                    }
                }
            }
            if (nodeDesigner.IsParent)
            {
                BehaviorDesigner.Runtime.Tasks.ParentTask task = nodeDesigner.Task as BehaviorDesigner.Runtime.Tasks.ParentTask;
                if (task.Children != null)
                {
                    for (int j = 0; j < task.Children.Count; j++)
                    {
                        if ((task.Children[j] != null) && this.RemoveSharedVariableReference(task.Children[j].NodeData.NodeDesigner as NodeDesigner, sharedVariable))
                        {
                            flag = true;
                        }
                    }
                }
            }
            return flag;
        }

        public bool RemoveSharedVariableReferences(BehaviorDesigner.Runtime.SharedVariable sharedVariable)
        {
            if (this.mEntryNode == null)
            {
                return false;
            }
            bool flag = false;
            if ((this.mRootNode != null) && this.RemoveSharedVariableReference(this.mRootNode, sharedVariable))
            {
                flag = true;
            }
            if (this.mDetachedNodes != null)
            {
                for (int i = 0; i < this.mDetachedNodes.Count; i++)
                {
                    if (this.RemoveSharedVariableReference(this.mDetachedNodes[i], sharedVariable))
                    {
                        flag = true;
                    }
                }
            }
            return flag;
        }

        public bool ReplaceSelectedNodes(BehaviorDesigner.Runtime.BehaviorSource behaviorSource, System.Type taskType)
        {
            if (this.SelectedNodes.Count == 0)
            {
                return false;
            }
            for (int i = this.SelectedNodes.Count - 1; i > -1; i--)
            {
                Vector2 absolutePosition = this.SelectedNodes[i].GetAbsolutePosition();
                NodeDesigner parentNodeDesigner = this.SelectedNodes[i].ParentNodeDesigner;
                List<BehaviorDesigner.Runtime.Tasks.Task> list = !this.SelectedNodes[i].IsParent ? null : (this.SelectedNodes[i].Task as BehaviorDesigner.Runtime.Tasks.ParentTask).Children;
                BehaviorDesigner.Runtime.Tasks.UnknownTask task = this.SelectedNodes[i].Task as BehaviorDesigner.Runtime.Tasks.UnknownTask;
                this.RemoveNode(this.SelectedNodes[i]);
                this.mSelectedNodes.RemoveAt(i);
                TaskReferences.CheckReferences(behaviorSource);
                NodeDesigner nodeDesigner = null;
                if (task == null)
                {
                    nodeDesigner = this.AddNode(behaviorSource, taskType, absolutePosition);
                }
                else
                {
                    BehaviorDesigner.Runtime.Tasks.Task task2 = null;
                    if (!string.IsNullOrEmpty(task.JSONSerialization))
                    {
                        Dictionary<int, BehaviorDesigner.Runtime.Tasks.Task> dictionary = new Dictionary<int, BehaviorDesigner.Runtime.Tasks.Task>();
                        Dictionary<string, object> dictionary2 = BehaviorDesigner.Runtime.MiniJSON.Deserialize(task.JSONSerialization) as Dictionary<string, object>;
                        if (dictionary2.ContainsKey("Type"))
                        {
                            dictionary2["Type"] = taskType.ToString();
                        }
                        task2 = BehaviorDesigner.Runtime.JSONDeserialization.DeserializeTask(behaviorSource, dictionary2, ref dictionary,  null);
                    }
                    else
                    {
                        BehaviorDesigner.Runtime.TaskSerializationData taskSerializationData = new BehaviorDesigner.Runtime.TaskSerializationData();
                        taskSerializationData.types.Add(taskType.ToString());
                        taskSerializationData.startIndex.Add(0);
                        BehaviorDesigner.Runtime.FieldSerializationData fieldSerializationData = new BehaviorDesigner.Runtime.FieldSerializationData();
                        fieldSerializationData.fieldNameHash = task.fieldNameHash;
                        fieldSerializationData.startIndex = task.startIndex;
                        fieldSerializationData.dataPosition = task.dataPosition;
                        fieldSerializationData.unityObjects = task.unityObjects;
                        fieldSerializationData.byteDataArray = task.byteData.ToArray();
                        List<BehaviorDesigner.Runtime.Tasks.Task> taskList = new List<BehaviorDesigner.Runtime.Tasks.Task>();
                        BinaryDeserialization.LoadTask(taskSerializationData, fieldSerializationData, ref taskList, ref behaviorSource);
                        if (taskList.Count > 0)
                        {
                            task2 = taskList[0];
                        }
                    }
                    if (task2 != null)
                    {
                        nodeDesigner = this.AddNode(behaviorSource, task2, absolutePosition);
                    }
                }
                if (nodeDesigner != null)
                {
                    if (parentNodeDesigner != null)
                    {
                        this.ActiveNodeConnection = parentNodeDesigner.CreateNodeConnection(false);
                        this.ConnectNodes(behaviorSource, nodeDesigner);
                    }
                    if (nodeDesigner.IsParent && (list != null))
                    {
                        for (int j = 0; j < list.Count; j++)
                        {
                            this.ActiveNodeConnection = nodeDesigner.CreateNodeConnection(false);
                            this.ConnectNodes(behaviorSource, list[j].NodeData.NodeDesigner as NodeDesigner);
                            if (j >= (nodeDesigner.Task as BehaviorDesigner.Runtime.Tasks.ParentTask).MaxChildren())
                            {
                                break;
                            }
                        }
                    }
                    this.Select(nodeDesigner);
                }
            }

            // 不支持撤销
            //BehaviorUndo.RegisterUndo("Replace", behaviorSource.Owner.GetObject());
            return true;
        }

        public void Save(BehaviorDesigner.Runtime.BehaviorSource behaviorSource)
        {
            if (!object.ReferenceEquals(behaviorSource.Owner.GetObject(), null))
            {
                this.RemapIDs();
                List<BehaviorDesigner.Runtime.Tasks.Task> detachedTasks = new List<BehaviorDesigner.Runtime.Tasks.Task>();
                for (int i = 0; i < this.mDetachedNodes.Count; i++)
                {
                    detachedTasks.Add(this.mDetachedNodes[i].Task);
                }
                behaviorSource.Save((this.mEntryNode == null) ? null : this.mEntryNode.Task, (this.mRootNode == null) ? null : this.mRootNode.Task, detachedTasks);
                if (BehaviorDesignerPreferences.GetBool(BDPreferences.BinarySerialization))
                {
                    BinarySerialization.Save(behaviorSource);
                }
                else
                {
                    JSONSerialization.Save(behaviorSource);
                }
            }
        }

        public void Select(NodeConnection nodeConnection)
        {
            this.mSelectedNodeConnections.Add(nodeConnection);
            nodeConnection.select();
        }

        public void Select(NodeDesigner nodeDesigner)
        {
            this.Select(nodeDesigner, true);
        }

        public void Select(NodeDesigner nodeDesigner, bool addHash)
        {
            if (!this.mSelectedNodes.Contains(nodeDesigner))
            {
                if (this.mSelectedNodes.Count == 1)
                {
                    this.IndicateReferencedTasks(this.mSelectedNodes[0].Task, false);
                }
                this.mSelectedNodes.Add(nodeDesigner);
                if (addHash)
                {
                    this.mNodeSelectedID.Add(nodeDesigner.Task.ID);
                }
                nodeDesigner.Select();
                if (this.mSelectedNodes.Count == 1)
                {
                    this.IndicateReferencedTasks(this.mSelectedNodes[0].Task, true);
                }
            }
        }

        public void SelectAll()
        {
            for (int i = this.mSelectedNodes.Count - 1; i > -1; i--)
            {
                this.Deselect(this.mSelectedNodes[i]);
            }
            if (this.mRootNode != null)
            {
                this.SelectAll(this.mRootNode);
            }
            for (int j = this.mDetachedNodes.Count - 1; j > -1; j--)
            {
                this.SelectAll(this.mDetachedNodes[j]);
            }
        }

        private void SelectAll(NodeDesigner nodeDesigner)
        {
            this.Select(nodeDesigner);
            if (nodeDesigner.Task.GetType().IsSubclassOf(typeof(BehaviorDesigner.Runtime.Tasks.ParentTask)))
            {
                BehaviorDesigner.Runtime.Tasks.ParentTask task = nodeDesigner.Task as BehaviorDesigner.Runtime.Tasks.ParentTask;
                if (task.Children != null)
                {
                    for (int i = 0; i < task.Children.Count; i++)
                    {
                        this.SelectAll(task.Children[i].NodeData.NodeDesigner as NodeDesigner);
                    }
                }
            }
        }

        public void SetStartOffset(Vector2 offset)
        {
            Vector2 vector = offset - this.mEntryNode.Task.NodeData.Offset;
            this.mEntryNode.Task.NodeData.Offset = (offset);
            for (int i = 0; i < this.mDetachedNodes.Count; i++)
            {
                BehaviorDesigner.Runtime.NodeData nodeData = this.mDetachedNodes[i].Task.NodeData;
                nodeData.Offset = (nodeData.Offset + vector);
            }
        }

        public NodeDesigner RootNode
        {
            get
            {
                return this.mRootNode;
            }
        }

        public List<NodeDesigner> DetachedNodes
        {
            get
            {
                return this.mDetachedNodes;
            }
        }

        public List<NodeDesigner> SelectedNodes
        {
            get
            {
                return this.mSelectedNodes;
            }
        }

        public NodeDesigner HoverNode
        {
            get
            {
                return this.mHoverNode;
            }
            set
            {
                this.mHoverNode = value;
            }
        }

        public NodeConnection ActiveNodeConnection
        {
            get
            {
                return this.mActiveNodeConnection;
            }
            set
            {
                this.mActiveNodeConnection = value;
            }
        }

        public List<NodeConnection> SelectedNodeConnections
        {
            get
            {
                return this.mSelectedNodeConnections;
            }
        }
    }
}

