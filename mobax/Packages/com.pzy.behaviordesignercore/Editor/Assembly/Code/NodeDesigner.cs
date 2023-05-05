namespace BehaviorDesigner.Editor
{
    using BehaviorDesigner.Runtime;
    using BehaviorDesigner.Runtime.Tasks;
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using UnityEngine;
    using Object = UnityEngine.Object;


    [Serializable]
    public class NodeDesigner : ScriptableObject
    {
        [SerializeField]
        private BehaviorDesigner.Runtime.Tasks.Task mTask;
        [SerializeField]
        private bool mSelected;
        private int mIdentifyUpdateCount = -1;
        private bool mFoundTask;
        [SerializeField]
        private bool mConnectionIsDirty;
        private bool mRectIsDirty = true;
        private bool mIncomingRectIsDirty = true;
        private bool mOutgoingRectIsDirty = true;
        [SerializeField]
        private bool isParent;
        [SerializeField]
        private bool isEntryDisplay;
        [SerializeField]
        private bool showReferenceIcon;
        private bool showHoverBar;
        private bool hasError;
        [SerializeField]
        private string taskName = string.Empty;
        private Rect mRectangle;
        private Rect mOutgoingRectangle;
        private Rect mIncomingRectangle;
        private bool prevRunningState;
        private int prevCommentLength = -1;
        private List<int> prevWatchedFieldsLength = new List<int>();
        private int prevFriendlyNameLength = -1;
        [SerializeField]
        private NodeDesigner parentNodeDesigner;
        [SerializeField]
        private List<NodeConnection> outgoingNodeConnections;
        private bool mCacheIsDirty = true;
        private readonly Color grayColor = new Color(0.7f, 0.7f, 0.7f);
        private Rect nodeCollapsedTextureRect;
        private Rect iconTextureRect;
        private Rect titleRect;
        private Rect breakpointTextureRect;
        private Rect errorTextureRect;
        private Rect referenceTextureRect;
        private Rect conditionalAbortTextureRect;
        private Rect conditionalAbortLowerPriorityTextureRect;
        private Rect disabledButtonTextureRect;
        private Rect collapseButtonTextureRect;
        private Rect incomingConnectionTextureRect;
        private Rect outgoingConnectionTextureRect;
        private Rect successReevaluatingExecutionStatusTextureRect;
        private Rect successExecutionStatusTextureRect;
        private Rect failureExecutionStatusTextureRect;
        private Rect iconBorderTextureRect;
        private Rect watchedFieldRect;
        private Rect watchedFieldNamesRect;
        private Rect watchedFieldValuesRect;
        private Rect commentRect;
        private Rect commentLabelRect;

        public void AddChildNode(NodeDesigner childNodeDesigner, NodeConnection nodeConnection, bool adjustOffset, bool replaceNode)
        {
            this.AddChildNode(childNodeDesigner, nodeConnection, adjustOffset, replaceNode, -1);
        }

        public void AddChildNode(NodeDesigner childNodeDesigner, NodeConnection nodeConnection, bool adjustOffset, bool replaceNode, int replaceNodeIndex)
        {
            if (replaceNode)
            {
                (this.mTask as BehaviorDesigner.Runtime.Tasks.ParentTask).Children[replaceNodeIndex] = childNodeDesigner.Task;
            }
            else
            {
                if (!this.isEntryDisplay)
                {
                    BehaviorDesigner.Runtime.Tasks.ParentTask mTask = this.mTask as BehaviorDesigner.Runtime.Tasks.ParentTask;
                    int index = 0;
                    if (mTask.Children != null)
                    {
                        index = 0;
                        while (index < mTask.Children.Count)
                        {
                            Vector2 absolutePosition = childNodeDesigner.GetAbsolutePosition();
                            Vector2 vector2 = (mTask.Children[index].NodeData.NodeDesigner as NodeDesigner).GetAbsolutePosition();
                            if (absolutePosition.x < vector2.x)
                            {
                                break;
                            }
                            index++;
                        }
                    }
                    mTask.AddChild(childNodeDesigner.Task, index);
                }
                if (adjustOffset)
                {
                    BehaviorDesigner.Runtime.NodeData nodeData = childNodeDesigner.Task.NodeData;
                    nodeData.Offset = (nodeData.Offset - this.GetAbsolutePosition());
                }
            }
            childNodeDesigner.ParentNodeDesigner = this;
            nodeConnection.DestinationNodeDesigner = childNodeDesigner;
            nodeConnection.NodeConnectionType = NodeConnectionType.Fixed;
            if (!nodeConnection.OriginatingNodeDesigner.Equals(this))
            {
                nodeConnection.OriginatingNodeDesigner = this;
            }
            this.outgoingNodeConnections.Add(nodeConnection);
            this.mConnectionIsDirty = true;
        }

        private void BringConnectionToFront(NodeDesigner nodeDesigner)
        {
            int num = 0;
            while (true)
            {
                if (num < this.outgoingNodeConnections.Count)
                {
                    if (!this.outgoingNodeConnections[num].DestinationNodeDesigner.Equals(nodeDesigner))
                    {
                        num++;
                        continue;
                    }
                    NodeConnection connection = this.outgoingNodeConnections[num];
                    this.outgoingNodeConnections[num] = this.outgoingNodeConnections[this.outgoingNodeConnections.Count - 1];
                    this.outgoingNodeConnections[this.outgoingNodeConnections.Count - 1] = connection;
                }
                return;
            }
        }

        private void CalculateNodeCommentRect(Rect nodeRect)
        {
            bool flag = false;
            if ((this.mTask.NodeData.WatchedFields != null) && (this.mTask.NodeData.WatchedFields.Count > 0))
            {
                string str = string.Empty;
                string str2 = string.Empty;
                int num = 0;
                while (true)
                {
                    if (num >= this.mTask.NodeData.WatchedFields.Count)
                    {
                        float num2;
                        float num3;
                        float num4;
                        BehaviorDesignerUtility.TaskCommentGUIStyle.CalcMinMaxWidth(new GUIContent(str), out num2, out num3);
                        BehaviorDesignerUtility.TaskCommentGUIStyle.CalcMinMaxWidth(new GUIContent(str2), out num2, out num4);
                        float num5 = num3;
                        float num6 = num4;
                        float num7 = Mathf.Min(220f, (num3 + num4) + 20f);
                        if (num7 == 220f)
                        {
                            num5 = (num3 / (num3 + num4)) * 220f;
                            num6 = (num4 / (num3 + num4)) * 220f;
                        }
                        this.watchedFieldRect = new Rect(nodeRect.xMax + 4f, nodeRect.y, num7 + 8f, nodeRect.height);
                        this.watchedFieldNamesRect = new Rect(nodeRect.xMax + 6f, nodeRect.y + 4f, num5, nodeRect.height - 8f);
                        this.watchedFieldValuesRect = new Rect((nodeRect.xMax + 6f) + num5, nodeRect.y + 4f, num6, nodeRect.height - 8f);
                        flag = true;
                        break;
                    }
                    FieldInfo info = this.mTask.NodeData.WatchedFields[num];
                    str = str + BehaviorDesignerUtility.SplitCamelCase(info.Name) + ": \n";
                    str2 = str2 + ((info.GetValue(this.mTask) == null) ? "null" : info.GetValue(this.mTask).ToString()) + "\n";
                    num++;
                }
            }
            string nodeComment = this.GetNodeComment();
            if (!nodeComment.Equals(string.Empty))
            {
                if (!this.isParent)
                {
                    float num11 = Mathf.Min(100f, BehaviorDesignerUtility.TaskCommentGUIStyle.CalcHeight(new GUIContent(nodeComment), nodeRect.width - 4f));
                    this.commentRect = new Rect(nodeRect.x, nodeRect.yMax + 4f, nodeRect.width, num11 + 4f);
                    this.commentLabelRect = new Rect(nodeRect.x, nodeRect.yMax + 4f, nodeRect.width - 4f, num11);
                }
                else
                {
                    float num8;
                    float num9;
                    BehaviorDesignerUtility.TaskCommentGUIStyle.CalcMinMaxWidth(new GUIContent(nodeComment), out num8, out num9);
                    float num10 = Mathf.Min(220f, num9 + 20f);
                    if (flag)
                    {
                        this.commentRect = new Rect((nodeRect.xMin - 12f) - num10, nodeRect.y, num10 + 8f, nodeRect.height);
                        this.commentLabelRect = new Rect((nodeRect.xMin - 6f) - num10, nodeRect.y + 4f, num10, nodeRect.height - 8f);
                    }
                    else
                    {
                        this.commentRect = new Rect(nodeRect.xMax + 4f, nodeRect.y, num10 + 8f, nodeRect.height);
                        this.commentLabelRect = new Rect(nodeRect.xMax + 6f, nodeRect.y + 4f, num10, nodeRect.height - 8f);
                    }
                }
            }
        }

        public void ChangeOffset(Vector2 delta)
        {
            Vector2 vector = this.mTask.NodeData.Offset + delta;
            this.mTask.NodeData.Offset = (vector);
            this.MarkDirty();
            if (this.parentNodeDesigner != null)
            {
                this.parentNodeDesigner.MarkDirty();
            }
        }

        public int ChildIndexForTask(BehaviorDesigner.Runtime.Tasks.Task childTask)
        {
            if (this.isParent)
            {
                BehaviorDesigner.Runtime.Tasks.ParentTask mTask = this.mTask as BehaviorDesigner.Runtime.Tasks.ParentTask;
                if (mTask.Children != null)
                {
                    for (int i = 0; i < mTask.Children.Count; i++)
                    {
                        if (mTask.Children[i].Equals(childTask))
                        {
                            return i;
                        }
                    }
                }
            }
            return -1;
        }

        public void ConnectionContains(Vector2 point, Vector2 offset, ref List<NodeConnection> nodeConnections)
        {
            if ((this.outgoingNodeConnections != null) && !this.isEntryDisplay)
            {
                for (int i = 0; i < this.outgoingNodeConnections.Count; i++)
                {
                    if (this.outgoingNodeConnections[i].Contains(point, offset))
                    {
                        nodeConnections.Add(this.outgoingNodeConnections[i]);
                    }
                }
            }
        }

        public bool Contains(Vector2 point, Vector2 offset, bool includeConnections)
        {
            return this.Rectangle(offset, includeConnections, false).Contains(point);
        }

        public NodeConnection CreateNodeConnection(bool incomingNodeConnection)
        {
            NodeConnection connection = ScriptableObject.CreateInstance<NodeConnection>();
            connection.LoadConnection(this, !incomingNodeConnection ? NodeConnectionType.Outgoing : NodeConnectionType.Incoming);
            return connection;
        }

        public void Deselect()
        {
            this.mSelected = false;
        }

        public void DestroyConnections()
        {
            if (this.outgoingNodeConnections != null)
            {
                for (int i = this.outgoingNodeConnections.Count - 1; i > -1; i--)
                {
                    Object.DestroyImmediate(this.outgoingNodeConnections[i], true);
                }
            }
        }

        private void DetermineConnectionHorizontalHeight(Rect nodeRect, Vector2 offset)
        {
            if (this.isParent)
            {
                float maxValue = float.MaxValue;
                float num2 = maxValue;
                int num3 = 0;
                while (true)
                {
                    if (num3 >= this.outgoingNodeConnections.Count)
                    {
                        maxValue = (maxValue * 0.75f) + (nodeRect.yMax * 0.25f);
                        if (maxValue < (nodeRect.yMax + 15f))
                        {
                            maxValue = nodeRect.yMax + 15f;
                        }
                        else if (maxValue > (num2 - 15f))
                        {
                            maxValue = num2 - 15f;
                        }
                        for (int i = 0; i < this.outgoingNodeConnections.Count; i++)
                        {
                            this.outgoingNodeConnections[i].HorizontalHeight = maxValue;
                        }
                        break;
                    }
                    Rect rect = this.outgoingNodeConnections[num3].DestinationNodeDesigner.Rectangle(offset, false, false);
                    if (rect.y < maxValue)
                    {
                        maxValue = rect.y;
                        num2 = rect.y;
                    }
                    num3++;
                }
            }
        }

        public bool DrawNode(Vector2 offset, bool drawSelected, bool disabled)
        {
            if (drawSelected != this.mSelected)
            {
                return false;
            }
            if (this.ToString().Length != this.prevFriendlyNameLength)
            {
                this.prevFriendlyNameLength = this.ToString().Length;
                this.mRectIsDirty = true;
            }
            Rect nodeRect = this.Rectangle(offset, false, false);
            this.UpdateCache(nodeRect);
            bool flag = ((this.mTask.NodeData.PushTime == -1f) || (this.mTask.NodeData.PushTime < this.mTask.NodeData.PopTime)) ? ((this.isEntryDisplay && (this.outgoingNodeConnections.Count > 0)) && (this.outgoingNodeConnections[0].DestinationNodeDesigner.Task.NodeData.PushTime != -1f)) : true;
            bool flag2 = (this.mIdentifyUpdateCount != -1) || this.mFoundTask;
            bool flag3 = this.prevRunningState != flag;
            float num = !BehaviorDesignerPreferences.GetBool(BDPreferences.FadeNodes) ? 0.01f : 0.5f;
            float num2 = 0f;
            if (flag2)
            {
                num2 = ((0x7d0 - this.mIdentifyUpdateCount) >= 500) ? 1f : (((float) (0x7d0 - this.mIdentifyUpdateCount)) / 500f);
                if (this.mIdentifyUpdateCount != -1)
                {
                    this.mIdentifyUpdateCount++;
                    if (this.mIdentifyUpdateCount > 0x7d0)
                    {
                        this.mIdentifyUpdateCount = -1;
                    }
                }
                flag3 = true;
            }
            else if (flag)
            {
                num2 = 1f;
            }
            else if (((this.mTask.NodeData.PopTime != -1f) && ((num != 0f) && ((this.mTask.NodeData.PopTime <= Time.realtimeSinceStartup) && ((Time.realtimeSinceStartup - this.mTask.NodeData.PopTime) < num)))) || (this.isEntryDisplay && ((this.outgoingNodeConnections.Count > 0) && ((this.outgoingNodeConnections[0].DestinationNodeDesigner.Task.NodeData.PopTime != -1f) && ((this.outgoingNodeConnections[0].DestinationNodeDesigner.Task.NodeData.PopTime <= Time.realtimeSinceStartup) && ((Time.realtimeSinceStartup - this.outgoingNodeConnections[0].DestinationNodeDesigner.Task.NodeData.PopTime) < num))))))
            {
                num2 = !this.isEntryDisplay ? (1f - ((Time.realtimeSinceStartup - this.mTask.NodeData.PopTime) / num)) : (1f - ((Time.realtimeSinceStartup - this.outgoingNodeConnections[0].DestinationNodeDesigner.Task.NodeData.PopTime) / num));
                flag3 = true;
            }
            if (!this.isEntryDisplay && (!this.prevRunningState && (this.parentNodeDesigner != null)))
            {
                this.parentNodeDesigner.BringConnectionToFront(this);
            }
            this.prevRunningState = flag;
            if (num2 != 1f)
            {
                GUI.color = ((disabled || this.mTask.Disabled) ? this.grayColor : Color.white);
                GUIStyle backgroundGUIStyle = null;
                backgroundGUIStyle = !BehaviorDesignerPreferences.GetBool(BDPreferences.CompactMode) ? (!this.mSelected ? BehaviorDesignerUtility.GetTaskGUIStyle(this.mTask.NodeData.ColorIndex) : BehaviorDesignerUtility.GetTaskSelectedGUIStyle(this.mTask.NodeData.ColorIndex)) : (!this.mSelected ? BehaviorDesignerUtility.GetTaskCompactGUIStyle(this.mTask.NodeData.ColorIndex) : BehaviorDesignerUtility.GetTaskSelectedCompactGUIStyle(this.mTask.NodeData.ColorIndex));
                this.DrawNodeTexture(nodeRect, BehaviorDesignerUtility.GetTaskConnectionTopTexture(this.mTask.NodeData.ColorIndex), BehaviorDesignerUtility.GetTaskConnectionBottomTexture(this.mTask.NodeData.ColorIndex), backgroundGUIStyle, BehaviorDesignerUtility.GetTaskBorderTexture(this.mTask.NodeData.ColorIndex));
            }
            if (num2 > 0f)
            {
                GUIStyle backgroundGUIStyle = null;
                Texture2D iconBorderTexture = null;
                if (flag2)
                {
                    backgroundGUIStyle = !BehaviorDesignerPreferences.GetBool(BDPreferences.CompactMode) ? (!this.mSelected ? BehaviorDesignerUtility.TaskIdentifyGUIStyle : BehaviorDesignerUtility.TaskIdentifySelectedGUIStyle) : (!this.mSelected ? BehaviorDesignerUtility.TaskIdentifyCompactGUIStyle : BehaviorDesignerUtility.TaskIdentifySelectedCompactGUIStyle);
                    iconBorderTexture = BehaviorDesignerUtility.TaskBorderIdentifyTexture;
                }
                else
                {
                    backgroundGUIStyle = !BehaviorDesignerPreferences.GetBool(BDPreferences.CompactMode) ? (!this.mSelected ? BehaviorDesignerUtility.TaskRunningGUIStyle : BehaviorDesignerUtility.TaskRunningSelectedGUIStyle) : (!this.mSelected ? BehaviorDesignerUtility.TaskRunningCompactGUIStyle : BehaviorDesignerUtility.TaskRunningSelectedCompactGUIStyle);
                    iconBorderTexture = BehaviorDesignerUtility.TaskBorderRunningTexture;
                }
                Color color = (disabled || this.mTask.Disabled) ? this.grayColor : Color.white;
                color.a = num2;
                GUI.color = (color);
                Texture2D connectionTopTexture = null;
                Texture2D connectionBottomTexture = null;
                if (!this.isEntryDisplay)
                {
                    connectionTopTexture = !flag2 ? BehaviorDesignerUtility.TaskConnectionRunningTopTexture : BehaviorDesignerUtility.TaskConnectionIdentifyTopTexture;
                }
                if (this.isParent)
                {
                    connectionBottomTexture = !flag2 ? BehaviorDesignerUtility.TaskConnectionRunningBottomTexture : BehaviorDesignerUtility.TaskConnectionIdentifyBottomTexture;
                }
                this.DrawNodeTexture(nodeRect, connectionTopTexture, connectionBottomTexture, backgroundGUIStyle, iconBorderTexture);
                GUI.color = (Color.white);
            }
            if (this.mTask.NodeData.Collapsed)
            {
                GUI.DrawTexture(this.nodeCollapsedTextureRect, BehaviorDesignerUtility.TaskConnectionCollapsedTexture);
            }
            if (!BehaviorDesignerPreferences.GetBool(BDPreferences.CompactMode))
            {
                GUI.DrawTexture(this.iconTextureRect, this.mTask.NodeData.Icon);
            }
            if ((this.mTask.NodeData.InterruptTime != -1f) && ((Time.realtimeSinceStartup - this.mTask.NodeData.InterruptTime) < (0.75f + num)))
            {
                float num3 = ((Time.realtimeSinceStartup - this.mTask.NodeData.InterruptTime) >= 0.75f) ? (1f - ((Time.realtimeSinceStartup - (this.mTask.NodeData.InterruptTime + 0.75f)) / num)) : 1f;
                Color color2 = Color.white;
                color2.a = num3;
                GUI.color = (color2);
                GUI.Label(nodeRect, string.Empty, BehaviorDesignerUtility.TaskHighlightGUIStyle);
                GUI.color = (Color.white);
            }
            GUI.Label(this.titleRect, this.ToString(), BehaviorDesignerUtility.TaskTitleGUIStyle);
            if (this.mTask.NodeData.IsBreakpoint)
            {
                GUI.DrawTexture(this.breakpointTextureRect, BehaviorDesignerUtility.BreakpointTexture);
            }
            if (this.showReferenceIcon)
            {
                GUI.DrawTexture(this.referenceTextureRect, BehaviorDesignerUtility.ReferencedTexture);
            }
            if (this.hasError)
            {
                GUI.DrawTexture(this.errorTextureRect, BehaviorDesignerUtility.ErrorIconTexture);
            }
            if ((this.mTask is BehaviorDesigner.Runtime.Tasks.Composite) && ((this.mTask as BehaviorDesigner.Runtime.Tasks.Composite).AbortType != BehaviorDesigner.Runtime.Tasks.AbortType.None))
            {
                BehaviorDesigner.Runtime.Tasks.AbortType abortType = (this.mTask as BehaviorDesigner.Runtime.Tasks.Composite).AbortType;
                if (abortType == BehaviorDesigner.Runtime.Tasks.AbortType.Self)
                {
                    GUI.DrawTexture(this.conditionalAbortTextureRect, BehaviorDesignerUtility.ConditionalAbortSelfTexture);
                }
                else if (abortType == BehaviorDesigner.Runtime.Tasks.AbortType.LowerPriority)
                {
                    GUI.DrawTexture(this.conditionalAbortLowerPriorityTextureRect, BehaviorDesignerUtility.ConditionalAbortLowerPriorityTexture);
                }
                else if (abortType == BehaviorDesigner.Runtime.Tasks.AbortType.Both)
                {
                    GUI.DrawTexture(this.conditionalAbortTextureRect, BehaviorDesignerUtility.ConditionalAbortBothTexture);
                }
            }
            GUI.color = (Color.white);
            if (this.showHoverBar)
            {
                //GUI.DrawTexture(this.disabledButtonTextureRect, !this.mTask.Disabled ? BehaviorDesignerUtility.DisableTaskTexture : BehaviorDesignerUtility.EnableTaskTexture, 2);
                GUI.DrawTexture(this.disabledButtonTextureRect, !this.mTask.Disabled ? BehaviorDesignerUtility.DisableTaskTexture : BehaviorDesignerUtility.EnableTaskTexture, ScaleMode.ScaleToFit);

                if (this.isParent || (this.mTask is BehaviorDesigner.Runtime.Tasks.BehaviorReference))
                {
                    bool collapsed = this.mTask.NodeData.Collapsed;
                    if (this.mTask is BehaviorDesigner.Runtime.Tasks.BehaviorReference)
                    {
                        collapsed = (this.mTask as BehaviorDesigner.Runtime.Tasks.BehaviorReference).collapsed;
                    }
                    //GUI.DrawTexture(this.collapseButtonTextureRect, !collapsed ? BehaviorDesignerUtility.CollapseTaskTexture : BehaviorDesignerUtility.ExpandTaskTexture, 2);
                    GUI.DrawTexture(this.collapseButtonTextureRect, !collapsed ? BehaviorDesignerUtility.CollapseTaskTexture : BehaviorDesignerUtility.ExpandTaskTexture, ScaleMode.ScaleToFit);
                }
            }
            return flag3;
        }

        public void DrawNodeComment(Vector2 offset)
        {
            int num2;
            string nodeComment = this.GetNodeComment();
            if (nodeComment.Length != this.prevCommentLength)
            {
                this.prevCommentLength = nodeComment.Length;
                this.mRectIsDirty = true;
            }
            if ((this.mTask.NodeData.WatchedFields == null) || (this.mTask.NodeData.WatchedFields.Count <= 0))
            {
                goto TR_000A;
            }
            else if (this.mTask.NodeData.WatchedFields.Count == this.prevWatchedFieldsLength.Count)
            {
                num2 = 0;
            }
            else
            {
                this.mRectIsDirty = true;
                this.prevWatchedFieldsLength.Clear();
                for (int i = 0; i < this.mTask.NodeData.WatchedFields.Count; i++)
                {
                    if (this.mTask.NodeData.WatchedFields[i] != null)
                    {
                        object obj2 = this.mTask.NodeData.WatchedFields[i].GetValue(this.mTask);
                        if (obj2 != null)
                        {
                            this.prevWatchedFieldsLength.Add(obj2.ToString().Length);
                        }
                        else
                        {
                            this.prevWatchedFieldsLength.Add(0);
                        }
                    }
                }
                goto TR_000A;
            }
            goto TR_001C;
        TR_000A:
            if (!nodeComment.Equals(string.Empty) || ((this.mTask.NodeData.WatchedFields != null) && (this.mTask.NodeData.WatchedFields.Count != 0)))
            {
                if ((this.mTask.NodeData.WatchedFields != null) && (this.mTask.NodeData.WatchedFields.Count > 0))
                {
                    string str2 = string.Empty;
                    string str3 = string.Empty;
                    int num4 = 0;
                    while (true)
                    {
                        if (num4 >= this.mTask.NodeData.WatchedFields.Count)
                        {
                            GUI.Box(this.watchedFieldRect, string.Empty, BehaviorDesignerUtility.TaskDescriptionGUIStyle);
                            GUI.Label(this.watchedFieldNamesRect, str2, BehaviorDesignerUtility.TaskCommentRightAlignGUIStyle);
                            GUI.Label(this.watchedFieldValuesRect, str3, BehaviorDesignerUtility.TaskCommentLeftAlignGUIStyle);
                            break;
                        }
                        FieldInfo info = this.mTask.NodeData.WatchedFields[num4];
                        str2 = str2 + BehaviorDesignerUtility.SplitCamelCase(info.Name) + ": \n";
                        str3 = str3 + ((info.GetValue(this.mTask) == null) ? "null" : info.GetValue(this.mTask).ToString()) + "\n";
                        num4++;
                    }
                }
                if (!nodeComment.Equals(string.Empty))
                {
                    GUI.Box(this.commentRect, string.Empty, BehaviorDesignerUtility.TaskDescriptionGUIStyle);
                    GUI.Label(this.commentLabelRect, nodeComment, BehaviorDesignerUtility.TaskCommentGUIStyle);
                }
            }
            return;
        TR_0014:
            num2++;
        TR_001C:
            while (true)
            {
                if (num2 >= this.mTask.NodeData.WatchedFields.Count)
                {
                    break;
                }
                if (this.mTask.NodeData.WatchedFields[num2] != null)
                {
                    object obj3 = this.mTask.NodeData.WatchedFields[num2].GetValue(this.mTask);
                    int length = 0;
                    if (obj3 != null)
                    {
                        length = obj3.ToString().Length;
                    }
                    if (length == this.prevWatchedFieldsLength[num2])
                    {
                        goto TR_0014;
                    }
                    else
                    {
                        this.mRectIsDirty = true;
                    }
                    break;
                }
                goto TR_0014;
            }
            goto TR_000A;
        }

        public void DrawNodeConnection(Vector2 offset, bool disabled)
        {
            if (this.mConnectionIsDirty)
            {
                this.DetermineConnectionHorizontalHeight(this.Rectangle(offset, false, false), offset);
                this.mConnectionIsDirty = false;
            }
            if (this.isParent)
            {
                for (int i = 0; i < this.outgoingNodeConnections.Count; i++)
                {
                    this.outgoingNodeConnections[i].DrawConnection(offset, disabled);
                }
            }
        }

        private void DrawNodeTexture(Rect nodeRect, Texture2D connectionTopTexture, Texture2D connectionBottomTexture, GUIStyle backgroundGUIStyle, Texture2D iconBorderTexture)
        {
            // pzy:
            // test
            //GUI.DrawTexture(new Rect(100, 100, 200, 200), connectionTopTexture, ScaleMode.ScaleToFit);

            if (!this.isEntryDisplay)
            {
                //GUI.DrawTexture(this.incomingConnectionTextureRect, connectionTopTexture, 2);
                GUI.DrawTexture(this.incomingConnectionTextureRect, connectionTopTexture, ScaleMode.ScaleToFit);
            }
            if (this.isParent)
            {
                //GUI.DrawTexture(this.outgoingConnectionTextureRect, connectionBottomTexture, 2);
                GUI.DrawTexture(this.outgoingConnectionTextureRect, connectionBottomTexture, ScaleMode.ScaleToFit);
            }
            GUI.Label(nodeRect, string.Empty, backgroundGUIStyle);
            if (this.mTask.NodeData.ExecutionStatus != BehaviorDesigner.Runtime.Tasks.TaskStatus.Success)
            {
                if (this.mTask.NodeData.ExecutionStatus == BehaviorDesigner.Runtime.Tasks.TaskStatus.Failure)
                {
                    GUI.DrawTexture(this.failureExecutionStatusTextureRect, !this.mTask.NodeData.IsReevaluating ? BehaviorDesignerUtility.ExecutionFailureTexture : BehaviorDesignerUtility.ExecutionFailureRepeatTexture);
                }
            }
            else if (this.mTask.NodeData.IsReevaluating)
            {
                GUI.DrawTexture(this.successReevaluatingExecutionStatusTextureRect, BehaviorDesignerUtility.ExecutionSuccessRepeatTexture);
            }
            else
            {
                GUI.DrawTexture(this.successExecutionStatusTextureRect, BehaviorDesignerUtility.ExecutionSuccessTexture);
            }
            if (!BehaviorDesignerPreferences.GetBool(BDPreferences.CompactMode))
            {
                GUI.DrawTexture(this.iconBorderTextureRect, iconBorderTexture);
            }
        }

        public override bool Equals(object obj)
        {
            return object.ReferenceEquals(this, obj);
        }

        public void FoundTask(bool found)
        {
            this.mFoundTask = found;
        }

        public Vector2 GetAbsolutePosition()
        {
            Vector2 vector = this.mTask.NodeData.Offset;
            if (this.parentNodeDesigner != null)
            {
                vector += this.parentNodeDesigner.GetAbsolutePosition();
            }
            if (BehaviorDesignerPreferences.GetBool(BDPreferences.SnapToGrid))
            {
                vector.Set(BehaviorDesignerUtility.RoundToNearest(vector.x, 10f), BehaviorDesignerUtility.RoundToNearest(vector.y, 10f));
            }
            return vector;
        }

        public Vector2 GetConnectionPosition(Vector2 offset, NodeConnectionType connectionType)
        {
            Vector2 vector;
            if (connectionType == NodeConnectionType.Incoming)
            {
                Rect rect = this.IncomingConnectionRect(offset);
                vector = new Vector2(rect.center.x, rect.y + 7f);
            }
            else
            {
                Rect rect2 = this.OutgoingConnectionRect(offset);
                vector = new Vector2(rect2.center.x, rect2.yMax - 8f);
            }
            return vector;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        private string GetNodeComment()
        {
            string str = string.Empty;
            if (!this.mTask.OnDrawNodeText().Equals(string.Empty))
            {
                str = this.mTask.OnDrawNodeText();
            }
            if (!this.mTask.NodeData.Comment.Equals(string.Empty))
            {
                if (!str.Equals(string.Empty))
                {
                    str = str + "\n";
                }
                str = str + this.mTask.NodeData.Comment;
            }
            return str;
        }

        public bool HasParent(NodeDesigner nodeDesigner)
        {
            return ((this.parentNodeDesigner != null) ? (!this.parentNodeDesigner.Equals(nodeDesigner) ? this.parentNodeDesigner.HasParent(nodeDesigner) : true) : false);
        }

        public unsafe bool HoverBarAreaContains(Vector2 point, Vector2 offset)
        {
            Rect rect = this.Rectangle(offset, false, false);
            Rect* rectPtr1 = &rect;
            rectPtr1->y = (rectPtr1->y - 24f);
            return rect.Contains(point);
        }

        public bool HoverBarButtonClick(Vector2 point, Vector2 offset, ref bool collapsedButtonClicked)
        {
            Rect rect = this.Rectangle(offset, false, false);
            Rect rect2 = new Rect(rect.x - 1f, rect.y - 17f, 14f, 14f);
            Rect rect3 = rect2;
            bool flag = false;
            if (rect2.Contains(point))
            {
                this.mTask.Disabled = !this.mTask.Disabled;
                flag = true;
            }
            if (!flag && (this.isParent || (this.mTask is BehaviorDesigner.Runtime.Tasks.BehaviorReference)))
            {
                Rect rect4 = new Rect(rect.x + 15f, rect.y - 17f, 14f, 14f);
                //rect3.xMax(rect4.xMax);
                rect3.xMax = rect4.xMax;
                if (rect4.Contains(point))
                {
                    if (this.mTask is BehaviorDesigner.Runtime.Tasks.BehaviorReference)
                    {
                        (this.mTask as BehaviorDesigner.Runtime.Tasks.BehaviorReference).collapsed = !(this.mTask as BehaviorDesigner.Runtime.Tasks.BehaviorReference).collapsed;
                    }
                    else
                    {
                        this.mTask.NodeData.Collapsed = !this.mTask.NodeData.Collapsed;
                    }
                    collapsedButtonClicked = true;
                    flag = true;
                }
            }
            if (!flag && rect3.Contains(point))
            {
                flag = true;
            }
            return flag;
        }

        public void IdentifyNode()
        {
            this.mIdentifyUpdateCount = 0;
        }

        public Rect IncomingConnectionRect(Vector2 offset)
        {
            if (this.mIncomingRectIsDirty)
            {
                Rect rect = this.Rectangle(offset, false, false);
                this.mIncomingRectangle = new Rect(rect.x + ((rect.width - 42f) / 2f), rect.y - 14f, 42f, 14f);
                this.mIncomingRectIsDirty = false;
            }
            return this.mIncomingRectangle;
        }

        private void Init()
        {
            this.taskName = BehaviorDesignerUtility.SplitCamelCase(this.mTask.GetType().Name.ToString());
            this.isParent = this.mTask.GetType().IsSubclassOf(typeof(BehaviorDesigner.Runtime.Tasks.ParentTask));
            if (this.isParent)
            {
                this.outgoingNodeConnections = new List<NodeConnection>();
            }
            this.mRectIsDirty = this.mCacheIsDirty = true;
            this.mIncomingRectIsDirty = true;
            this.mOutgoingRectIsDirty = true;
        }

        public bool Intersects(Rect rect, Vector2 offset)
        {
            Rect rect2 = this.Rectangle(offset, false, false);
            return (rect2.xMin < rect.xMax) && (rect2.xMax > rect.xMin) && (rect2.yMin < rect.yMax) && (rect2.yMax > rect.yMin);
        }

        public bool IsDisabled()
        {
            return (!this.mTask.Disabled ? ((this.parentNodeDesigner != null) && this.parentNodeDesigner.IsDisabled()) : true);
        }

        public void LoadNode(BehaviorDesigner.Runtime.Tasks.Task task, BehaviorDesigner.Runtime.BehaviorSource behaviorSource, Vector2 offset, ref int id)
        {
            int num;
            BehaviorDesigner.Runtime.Tasks.RequiredComponentAttribute[] attributeArray;
            this.mTask = task;
            this.mTask.Owner = behaviorSource.Owner as BehaviorDesigner.Runtime.Behavior;
            id = (num = id) + 1;
            this.mTask.ID = num;
            this.mTask.NodeData = new BehaviorDesigner.Runtime.NodeData();
            this.mTask.NodeData.Offset = (offset);
            this.mTask.NodeData.NodeDesigner = this;
            this.LoadTaskIcon();
            this.Init();
            this.mTask.FriendlyName = this.taskName;
            if ((this.mTask.Owner != null) && ((attributeArray = this.mTask.GetType().GetCustomAttributes(typeof(BehaviorDesigner.Runtime.Tasks.RequiredComponentAttribute), true) as BehaviorDesigner.Runtime.Tasks.RequiredComponentAttribute[]).Length > 0))
            {
                Type componentType = attributeArray[0].ComponentType;
                // 如果 task 标记了需要的组件，则自动添加需要的组件 
                //if (typeof(Component).IsAssignableFrom(componentType) && (this.mTask.Owner.gameObject.GetComponent(componentType) == null))
                //{
                //    this.mTask.Owner.gameObject.AddComponent(componentType);
                //}
                if (typeof(CoreComponent).IsAssignableFrom(componentType) && (this.mTask.Owner.coreObject.GetComponent(componentType) == null))
                {
                    this.mTask.Owner.coreObject.AddComponent(componentType);
                }
            }
            List<Type> baseClasses = FieldInspector.GetBaseClasses(this.mTask.GetType());
            BindingFlags bindingAttr = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly;
            int num2 = baseClasses.Count - 1;
            while (num2 > -1)
            {
                FieldInfo[] fields = baseClasses[num2].GetFields(bindingAttr);
                int index = 0;
                while (true)
                {
                    if (index >= fields.Length)
                    {
                        num2--;
                        break;
                    }
                    if (typeof(BehaviorDesigner.Runtime.SharedVariable).IsAssignableFrom(fields[index].FieldType) && !fields[index].FieldType.IsAbstract)
                    {
                        BehaviorDesigner.Runtime.SharedVariable variable = fields[index].GetValue(this.mTask) as BehaviorDesigner.Runtime.SharedVariable;
                        if (variable == null)
                        {
                            variable = Activator.CreateInstance(fields[index].FieldType) as BehaviorDesigner.Runtime.SharedVariable;
                        }
                        if (BehaviorDesigner.Runtime.TaskUtility.HasAttribute(fields[index], typeof(BehaviorDesigner.Runtime.Tasks.RequiredFieldAttribute)) || BehaviorDesigner.Runtime.TaskUtility.HasAttribute(fields[index], typeof(BehaviorDesigner.Runtime.Tasks.SharedRequiredAttribute)))
                        {
                            variable.IsShared = true;
                        }
                        fields[index].SetValue(this.mTask, variable);
                    }
                    index++;
                }
            }
        }

        public void LoadTask(BehaviorDesigner.Runtime.Tasks.Task task, BehaviorDesigner.Runtime.Behavior owner, ref int id)
        {
            if (task != null)
            {
                int num;
                BehaviorDesigner.Runtime.Tasks.RequiredComponentAttribute[] attributeArray;
                this.mTask = task;
                if (owner != null)
                {
                    this.mTask.Owner = owner;
                }
                id = (num = id) + 1;
                this.mTask.ID = num;
                this.mTask.NodeData.NodeDesigner = this;
                this.mTask.NodeData.InitWatchedFields(this.mTask);
                if (!this.mTask.NodeData.FriendlyName.Equals(string.Empty))
                {
                    this.mTask.FriendlyName = this.mTask.NodeData.FriendlyName;
                    this.mTask.NodeData.FriendlyName = string.Empty;
                }
                this.LoadTaskIcon();
                this.Init();
                if ((this.mTask.Owner != null) && ((attributeArray = this.mTask.GetType().GetCustomAttributes(typeof(BehaviorDesigner.Runtime.Tasks.RequiredComponentAttribute), true) as BehaviorDesigner.Runtime.Tasks.RequiredComponentAttribute[]).Length > 0))
                {
                    Type componentType = attributeArray[0].ComponentType;

                    // 如果 task 标记了需要的组件，自动添加
                    //if (typeof(Component).IsAssignableFrom(componentType) && (this.mTask.Owner.gameObject.GetComponent(componentType) == null))
                    //{
                    //    this.mTask.Owner.gameObject.AddComponent(componentType);
                    //}
                    if (typeof(CoreComponent).IsAssignableFrom(componentType) && (this.mTask.Owner.coreObject.GetComponent(componentType) == null))
                    {
                        this.mTask.Owner.coreObject.AddComponent(componentType);
                    }
                }
                List<Type> baseClasses = FieldInspector.GetBaseClasses(this.mTask.GetType());
                BindingFlags bindingAttr = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly;
                int num2 = baseClasses.Count - 1;
                while (num2 > -1)
                {
                    FieldInfo[] fields = baseClasses[num2].GetFields(bindingAttr);
                    int index = 0;
                    while (true)
                    {
                        if (index >= fields.Length)
                        {
                            num2--;
                            break;
                        }
                        if (typeof(BehaviorDesigner.Runtime.SharedVariable).IsAssignableFrom(fields[index].FieldType) && !fields[index].FieldType.IsAbstract)
                        {
                            BehaviorDesigner.Runtime.SharedVariable variable = fields[index].GetValue(this.mTask) as BehaviorDesigner.Runtime.SharedVariable;
                            if (variable == null)
                            {
                                variable = Activator.CreateInstance(fields[index].FieldType) as BehaviorDesigner.Runtime.SharedVariable;
                            }
                            if (BehaviorDesigner.Runtime.TaskUtility.HasAttribute(fields[index], typeof(BehaviorDesigner.Runtime.Tasks.RequiredFieldAttribute)) || BehaviorDesigner.Runtime.TaskUtility.HasAttribute(fields[index], typeof(BehaviorDesigner.Runtime.Tasks.SharedRequiredAttribute)))
                            {
                                variable.IsShared = true;
                            }
                            fields[index].SetValue(this.mTask, variable);
                        }
                        index++;
                    }
                }
                if (this.isParent)
                {
                    BehaviorDesigner.Runtime.Tasks.ParentTask mTask = this.mTask as BehaviorDesigner.Runtime.Tasks.ParentTask;
                    if (mTask.Children != null)
                    {
                        for (int i = 0; i < mTask.Children.Count; i++)
                        {
                            NodeDesigner childNodeDesigner = ScriptableObject.CreateInstance<NodeDesigner>();
                            childNodeDesigner.LoadTask(mTask.Children[i], owner, ref id);
                            NodeConnection nodeConnection = ScriptableObject.CreateInstance<NodeConnection>();
                            nodeConnection.LoadConnection(this, NodeConnectionType.Fixed);
                            this.AddChildNode(childNodeDesigner, nodeConnection, true, true, i);
                        }
                    }
                    this.mConnectionIsDirty = true;
                }
            }
        }

        private void LoadTaskIcon()
        {
            BehaviorDesigner.Runtime.Tasks.TaskIconAttribute[] attributeArray = null;
            this.mTask.NodeData.Icon =((Texture) null);
            if ((attributeArray = this.mTask.GetType().GetCustomAttributes(typeof(BehaviorDesigner.Runtime.Tasks.TaskIconAttribute), true) as BehaviorDesigner.Runtime.Tasks.TaskIconAttribute[]).Length > 0)
            {
                this.mTask.NodeData.Icon =((Texture) BehaviorDesignerUtility.LoadIcon(attributeArray[0].IconPath, null));
            }
            if (this.mTask.NodeData.Icon == null)
            {
                string iconName = string.Empty;
                iconName = !this.mTask.GetType().IsSubclassOf(typeof(BehaviorDesigner.Runtime.Tasks.Action)) ? (!this.mTask.GetType().IsSubclassOf(typeof(BehaviorDesigner.Runtime.Tasks.Conditional)) ? (!this.mTask.GetType().IsSubclassOf(typeof(BehaviorDesigner.Runtime.Tasks.Composite)) ? (!this.mTask.GetType().IsSubclassOf(typeof(BehaviorDesigner.Runtime.Tasks.Decorator)) ? "{SkinColor}EntryIcon.png" : "{SkinColor}DecoratorIcon.png") : "{SkinColor}CompositeIcon.png") : "{SkinColor}ConditionalIcon.png") : "{SkinColor}ActionIcon.png";
                this.mTask.NodeData.Icon =((Texture) BehaviorDesignerUtility.LoadIcon(iconName, null));
            }
        }

        public void MakeEntryDisplay()
        {
            this.isEntryDisplay = this.isParent = true;
            this.mTask.FriendlyName = this.taskName = "Entry";
            this.outgoingNodeConnections = new List<NodeConnection>();
        }

        public void MarkDirty()
        {
            this.mConnectionIsDirty = true;
            this.mRectIsDirty = true;
            this.mIncomingRectIsDirty = true;
            this.mOutgoingRectIsDirty = true;
        }

        public void MoveChildNode(int index, bool decreaseIndex)
        {
            int num = index + (!decreaseIndex ? 1 : -1);
            BehaviorDesigner.Runtime.Tasks.ParentTask mTask = this.mTask as BehaviorDesigner.Runtime.Tasks.ParentTask;
            mTask.Children[index] = mTask.Children[num];
            mTask.Children[num] = mTask.Children[index];
        }

        public NodeConnection NodeConnectionRectContains(Vector2 point, Vector2 offset)
        {
            bool incomingNodeConnection = false;
            return (((incomingNodeConnection = this.IncomingConnectionRect(offset).Contains(point)) || (this.isParent && this.OutgoingConnectionRect(offset).Contains(point))) ? this.CreateNodeConnection(incomingNodeConnection) : null);
        }

        public NodeDesigner NodeDesignerForChildIndex(int index)
        {
            if ((index >= 0) && this.isParent)
            {
                BehaviorDesigner.Runtime.Tasks.ParentTask mTask = this.mTask as BehaviorDesigner.Runtime.Tasks.ParentTask;
                if (mTask.Children != null)
                {
                    return (((index >= mTask.Children.Count) || (mTask.Children[index] == null)) ? null : (mTask.Children[index].NodeData.NodeDesigner as NodeDesigner));
                }
            }
            return null;
        }

        public void OnEnable()
        {
            base.hideFlags = (HideFlags)(0x3d);
        }

        public Rect OutgoingConnectionRect(Vector2 offset)
        {
            if (this.mOutgoingRectIsDirty)
            {
                Rect rect = this.Rectangle(offset, false, false);
                this.mOutgoingRectangle = new Rect(rect.x + ((rect.width - 42f) / 2f), rect.yMax, 42f, 16f);
                this.mOutgoingRectIsDirty = false;
            }
            return this.mOutgoingRectangle;
        }

        private Rect Rectangle(Vector2 offset)
        {
            if (this.mRectIsDirty)
            {
                this.mCacheIsDirty = true;
                if (this.mTask == null)
                {
                    return new Rect();
                }
                float num = BehaviorDesignerUtility.TaskTitleGUIStyle.CalcSize(new GUIContent(this.ToString())).x + 20f;
                if (!this.isParent)
                {
                    float num2;
                    float num3;
                    BehaviorDesignerUtility.TaskCommentGUIStyle.CalcMinMaxWidth(new GUIContent(this.GetNodeComment()), out num2, out num3);
                    num3 += 20f;
                    num = (num <= num3) ? num3 : num;
                }
                num = Mathf.Min(220f, Mathf.Max(100f, num));
                Vector2 absolutePosition = this.GetAbsolutePosition();
                this.mRectangle = new Rect((absolutePosition.x + offset.x) - (num / 2f), absolutePosition.y + offset.y, num, (float) (20 + (!BehaviorDesignerPreferences.GetBool(BDPreferences.CompactMode) ? 0x34 : 0x16)));
                this.mRectIsDirty = false;
            }
            return this.mRectangle;
        }

        public unsafe Rect Rectangle(Vector2 offset, bool includeConnections, bool includeComments)
        {
            Rect rect = this.Rectangle(offset);
            if (includeConnections)
            {
                if (!this.isEntryDisplay)
                {
                    Rect* rectPtr1 = &rect;
                    rectPtr1->yMin = rectPtr1->yMin - 14f;
                }
                if (this.isParent)
                {
                    Rect* rectPtr2 = &rect;
                    rectPtr2->yMax = rectPtr2->yMax + 16f;
                }
            }
            if (includeComments && (this.mTask != null))
            {
                if ((this.mTask.NodeData.WatchedFields != null) && ((this.mTask.NodeData.WatchedFields.Count > 0) && (rect.xMax < this.watchedFieldRect.xMax)))
                {
                    rect.xMax = this.watchedFieldRect.xMax;
                }
                if (!this.GetNodeComment().Equals(string.Empty))
                {
                    if (rect.xMax < this.commentRect.xMax)
                    {
                        rect.xMax = this.commentRect.xMax;
                    }
                    if (rect.yMax < this.commentRect.yMax)
                    {
                        rect.yMax = this.commentRect.yMax;
                    }
                }
            }
            return rect;
        }

        public void RemoveChildNode(NodeDesigner childNodeDesigner)
        {
            if (!this.isEntryDisplay)
            {
                (this.mTask as BehaviorDesigner.Runtime.Tasks.ParentTask).Children.Remove(childNodeDesigner.Task);
            }
            int index = 0;
            while (true)
            {
                if (index < this.outgoingNodeConnections.Count)
                {
                    NodeConnection connection = this.outgoingNodeConnections[index];
                    if (!connection.DestinationNodeDesigner.Equals(childNodeDesigner) && !connection.OriginatingNodeDesigner.Equals(childNodeDesigner))
                    {
                        index++;
                        continue;
                    }
                    this.outgoingNodeConnections.RemoveAt(index);
                }
                childNodeDesigner.ParentNodeDesigner = null;
                this.mConnectionIsDirty = true;
                return;
            }
        }

        public void Select()
        {
            if (!this.isEntryDisplay)
            {
                this.mSelected = true;
            }
        }

        public void SetID(ref int id)
        {
            int num;
            id = (num = id) + 1;
            this.mTask.ID = num;
            if (this.isParent)
            {
                BehaviorDesigner.Runtime.Tasks.ParentTask mTask = this.mTask as BehaviorDesigner.Runtime.Tasks.ParentTask;
                if (mTask.Children != null)
                {
                    for (int i = 0; i < mTask.Children.Count; i++)
                    {
                        (mTask.Children[i].NodeData.NodeDesigner as NodeDesigner).SetID(ref id);
                    }
                }
            }
        }

        public void ToggleBreakpoint()
        {
            this.mTask.NodeData.IsBreakpoint = !this.Task.NodeData.IsBreakpoint;
        }

        public bool ToggleCollapseState()
        {
            this.mTask.NodeData.Collapsed = !this.Task.NodeData.Collapsed;
            return this.mTask.NodeData.Collapsed;
        }

        public void ToggleEnableState()
        {
            this.mTask.Disabled = !this.Task.Disabled;
        }

        public override string ToString()
        {
            return ((this.mTask != null) ? (!this.mTask.FriendlyName.Equals(string.Empty) ? this.mTask.FriendlyName : this.taskName) : string.Empty);
        }

        private void UpdateCache(Rect nodeRect)
        {
            if (this.mCacheIsDirty)
            {
                this.nodeCollapsedTextureRect = new Rect((nodeRect.x + ((nodeRect.width - 26f) / 2f)) + 1f, nodeRect.yMax + 2f, 26f, 6f);
                this.iconTextureRect = new Rect(nodeRect.x + ((nodeRect.width - 44f) / 2f), (nodeRect.y + 4f) + 2f, 44f, 44f);
                this.titleRect = new Rect(nodeRect.x, (nodeRect.yMax - (!BehaviorDesignerPreferences.GetBool(BDPreferences.CompactMode) ? ((float) 20) : ((float) 0x1c))) - 1f, nodeRect.width, 20f);
                this.breakpointTextureRect = new Rect(nodeRect.xMax - 16f, nodeRect.y + 3f, 14f, 14f);
                this.errorTextureRect = new Rect(nodeRect.xMax - 12f, nodeRect.y - 8f, 20f, 20f);
                this.referenceTextureRect = new Rect(nodeRect.x + 2f, nodeRect.y + 3f, 14f, 14f);
                this.conditionalAbortTextureRect = new Rect(nodeRect.x + 3f, nodeRect.y + 3f, 16f, 16f);
                this.conditionalAbortLowerPriorityTextureRect = new Rect(nodeRect.x + 3f, nodeRect.y, 16f, 16f);
                this.disabledButtonTextureRect = new Rect(nodeRect.x - 1f, nodeRect.y - 17f, 14f, 14f);
                this.collapseButtonTextureRect = new Rect(nodeRect.x + 15f, nodeRect.y - 17f, 14f, 14f);
                this.incomingConnectionTextureRect = new Rect(nodeRect.x + ((nodeRect.width - 42f) / 2f), ((nodeRect.y - 14f) - 3f) + 3f, 42f, 17f);
                this.outgoingConnectionTextureRect = new Rect(nodeRect.x + ((nodeRect.width - 42f) / 2f), nodeRect.yMax - 3f, 42f, 19f);
                this.successReevaluatingExecutionStatusTextureRect = new Rect(nodeRect.xMax - 37f, nodeRect.yMax - 38f, 35f, 36f);
                this.successExecutionStatusTextureRect = new Rect(nodeRect.xMax - 37f, nodeRect.yMax - 33f, 35f, 31f);
                this.failureExecutionStatusTextureRect = new Rect(nodeRect.xMax - 37f, nodeRect.yMax - 38f, 35f, 36f);
                this.iconBorderTextureRect = new Rect(nodeRect.x + ((nodeRect.width - 46f) / 2f), (nodeRect.y + 3f) + 2f, 46f, 46f);
                this.CalculateNodeCommentRect(nodeRect);
                this.mCacheIsDirty = false;
            }
        }

        public BehaviorDesigner.Runtime.Tasks.Task Task
        {
            get
            {
                return this.mTask;
            }
            set
            {
                this.mTask = value;
                this.Init();
            }
        }

        public bool IsParent
        {
            get
            {
                return this.isParent;
            }
        }

        public bool IsEntryDisplay
        {
            get
            {
                return this.isEntryDisplay;
            }
        }

        public bool ShowReferenceIcon
        {
            set
            {
                this.showReferenceIcon = value;
            }
        }

        public bool ShowHoverBar
        {
            get
            {
                return this.showHoverBar;
            }
            set
            {
                this.showHoverBar = value;
            }
        }

        public bool HasError
        {
            set
            {
                this.hasError = value;
            }
        }

        public NodeDesigner ParentNodeDesigner
        {
            get
            {
                return this.parentNodeDesigner;
            }
            set
            {
                this.parentNodeDesigner = value;
            }
        }

        public List<NodeConnection> OutgoingNodeConnections
        {
            get
            {
                return this.outgoingNodeConnections;
            }
        }
    }
}

