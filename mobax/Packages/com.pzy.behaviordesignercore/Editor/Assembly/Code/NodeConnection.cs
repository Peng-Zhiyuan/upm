namespace BehaviorDesigner.Editor
{
    using System;
    using UnityEditor;
    using UnityEngine;

    [Serializable]
    public class NodeConnection : ScriptableObject
    {
        [SerializeField]
        private NodeDesigner originatingNodeDesigner;
        [SerializeField]
        private NodeDesigner destinationNodeDesigner;
        [SerializeField]
        private BehaviorDesigner.Editor.NodeConnectionType nodeConnectionType;
        [SerializeField]
        private bool selected;
        [SerializeField]
        private float horizontalHeight;
        private readonly Color selectedDisabledProColor = new Color(0.1316f, 0.3212f, 0.4803f);
        private readonly Color selectedDisabledStandardColor = new Color(0.1701f, 0.3982f, 0.5873f);
        private readonly Color selectedEnabledProColor = new Color(0.188f, 0.4588f, 0.6862f);
        private readonly Color selectedEnabledStandardColor = new Color(0.243f, 0.5686f, 0.839f);
        private readonly Color taskRunningProColor = new Color(0f, 0.698f, 0.4f);
        private readonly Color taskRunningStandardColor = new Color(0f, 1f, 0.2784f);
        private bool horizontalDirty = true;
        private Vector2 startHorizontalBreak;
        private Vector2 endHorizontalBreak;
        private Vector3[] linePoints = new Vector3[4];

        public bool Contains(Vector2 point, Vector2 offset)
        {
            Vector2 vector = this.originatingNodeDesigner.OutgoingConnectionRect(offset).center;
            Vector2 vector2 = new Vector2(vector.x, this.horizontalHeight);
            if ((Mathf.Abs(point.x - vector.x) < 7f) && (((point.y >= vector.y) && (point.y <= vector2.y)) || ((point.y <= vector.y) && (point.y >= vector2.y))))
            {
                return true;
            }
            Rect rect2 = this.destinationNodeDesigner.IncomingConnectionRect(offset);
            Vector2 vector3 = new Vector2(rect2.center.x, rect2.y);
            Vector2 vector5 = new Vector2(vector3.x, this.horizontalHeight);
            return (((Mathf.Abs(point.y - this.horizontalHeight) >= 7f) || (((point.x > vector.x) || (point.x < vector5.x)) && ((point.x < vector.x) || (point.x > vector5.x)))) ? ((Mathf.Abs(point.x - vector3.x) < 7f) && (((point.y < vector3.y) || (point.y > vector5.y)) ? ((point.y <= vector3.y) && (point.y >= vector5.y)) : true)) : true);
        }

        public void deselect()
        {
            this.selected = false;
        }

        public void DrawConnection(Vector2 offset, bool disabled)
        {
            this.DrawConnection(this.OriginatingNodeDesigner.GetConnectionPosition(offset, BehaviorDesigner.Editor.NodeConnectionType.Outgoing), this.DestinationNodeDesigner.GetConnectionPosition(offset, BehaviorDesigner.Editor.NodeConnectionType.Incoming), disabled);
        }

        public unsafe void DrawConnection(Vector2 source, Vector2 destination, bool disabled)
        {
            Color color = !disabled ? Color.white : new Color(0.7f, 0.7f, 0.7f);
            bool flag = ((this.destinationNodeDesigner != null) && ((this.destinationNodeDesigner.Task != null) && (this.destinationNodeDesigner.Task.NodeData.PushTime != -1f))) && (this.destinationNodeDesigner.Task.NodeData.PushTime >= this.destinationNodeDesigner.Task.NodeData.PopTime);
            float num = !BehaviorDesignerPreferences.GetBool(BDPreferences.FadeNodes) ? 0.01f : 0.5f;
            if (this.selected)
            {
                color = !disabled ? (!UnityEditor.EditorGUIUtility.isProSkin ? this.selectedEnabledStandardColor : this.selectedEnabledProColor) : (!UnityEditor.EditorGUIUtility.isProSkin ? this.selectedDisabledStandardColor : this.selectedDisabledProColor);
            }
            else if (flag)
            {
                color = !UnityEditor.EditorGUIUtility.isProSkin ? this.taskRunningStandardColor : this.taskRunningProColor;
            }
            else if ((num != 0f) && ((this.destinationNodeDesigner != null) && ((this.destinationNodeDesigner.Task != null) && ((this.destinationNodeDesigner.Task.NodeData.PopTime != -1f) && ((this.destinationNodeDesigner.Task.NodeData.PopTime <= Time.realtimeSinceStartup) && ((Time.realtimeSinceStartup - this.destinationNodeDesigner.Task.NodeData.PopTime) < num))))))
            {
                float num2 = 1f - ((Time.realtimeSinceStartup - this.destinationNodeDesigner.Task.NodeData.PopTime) / num);
                Color color2 = Color.white;
                color2 = !UnityEditor.EditorGUIUtility.isProSkin ? this.taskRunningStandardColor : this.taskRunningProColor;
                color = Color.Lerp(Color.white, color2, num2);
            }
            UnityEditor.Handles.color = (color);
            if (this.horizontalDirty)
            {
                this.startHorizontalBreak = new Vector2(source.x, this.horizontalHeight);
                this.endHorizontalBreak = new Vector2(destination.x, this.horizontalHeight);
                this.horizontalDirty = false;
            }
            this.linePoints[0] = source;
            this.linePoints[1] = this.startHorizontalBreak;
            this.linePoints[2] = this.endHorizontalBreak;
            this.linePoints[3] = destination;
            UnityEditor.Handles.DrawPolyLine(this.linePoints);
            for (int i = 0; i < this.linePoints.Length; i++)
            {
                //Vector3* vectorPtr1 = &(this.linePoints[i]);
                //vectorPtr1->x++;
                //Vector3* vectorPtr2 = &(this.linePoints[i]);
                //vectorPtr2->y++;
                this.linePoints[i].x = this.linePoints[i].x + 1;
                this.linePoints[i].y = this.linePoints[i].y + 1;

            }
            UnityEditor.Handles.DrawPolyLine(this.linePoints);
        }

        public void LoadConnection(NodeDesigner nodeDesigner, BehaviorDesigner.Editor.NodeConnectionType nodeConnectionType)
        {
            this.originatingNodeDesigner = nodeDesigner;
            this.nodeConnectionType = nodeConnectionType;
            this.selected = false;
        }

        public void OnEnable()
        {
            base.hideFlags =(HideFlags)(0x3d);
        }

        public void select()
        {
            this.selected = true;
        }

        public NodeDesigner OriginatingNodeDesigner
        {
            get
            {
                return this.originatingNodeDesigner;
            }
            set
            {
                this.originatingNodeDesigner = value;
            }
        }

        public NodeDesigner DestinationNodeDesigner
        {
            get
            {
                return this.destinationNodeDesigner;
            }
            set
            {
                this.destinationNodeDesigner = value;
            }
        }

        public BehaviorDesigner.Editor.NodeConnectionType NodeConnectionType
        {
            get
            {
                return this.nodeConnectionType;
            }
            set
            {
                this.nodeConnectionType = value;
            }
        }

        public float HorizontalHeight
        {
            set
            {
                this.horizontalHeight = value;
                this.horizontalDirty = true;
            }
        }
    }
}

