namespace BehaviorDesigner.Editor
{
    using BehaviorDesigner.Runtime.Tasks;
    using System;
    using UnityEngine;

    [Serializable]
    public class ErrorDetails
    {
        [SerializeField]
        private ErrorType mType;
        [SerializeField]
        private BehaviorDesigner.Editor.NodeDesigner mNodeDesigner;
        [SerializeField]
        private string mTaskFriendlyName;
        [SerializeField]
        private string mTaskType;
        [SerializeField]
        private string mFieldName;

        public ErrorDetails(ErrorType type, BehaviorDesigner.Runtime.Tasks.Task task, string fieldName)
        {
            this.mType = type;
            if (task != null)
            {
                this.mNodeDesigner = task.NodeData.NodeDesigner as BehaviorDesigner.Editor.NodeDesigner;
                this.mTaskFriendlyName = task.FriendlyName;
                this.mTaskType = task.GetType().ToString();
            }
            this.mFieldName = fieldName;
        }

        public ErrorType Type
        {
            get
            {
                return this.mType;
            }
        }

        public BehaviorDesigner.Editor.NodeDesigner NodeDesigner
        {
            get
            {
                return this.mNodeDesigner;
            }
        }

        public string TaskFriendlyName
        {
            get
            {
                return this.mTaskFriendlyName;
            }
        }

        public string TaskType
        {
            get
            {
                return this.mTaskType;
            }
        }

        public string FieldName
        {
            get
            {
                return this.mFieldName;
            }
        }

        public enum ErrorType
        {
            RequiredField,
            SharedVariable,
            NonUniqueDynamicVariable,
            MissingChildren,
            UnknownTask,
            InvalidTaskReference,
            InvalidVariableReference
        }
    }
}

