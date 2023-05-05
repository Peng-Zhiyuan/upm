namespace BehaviorDesigner.Editor
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;


    [Serializable]

    // 看起来是用作复制节点时的数据
    public class TaskSerializer
    {
        public string serialization;
        public Vector2 offset;

        // 保存其中对外部对象的引用，这部分仅拷贝引用，不拷贝数据
        //public List<UnityEngine.Object> unityObjects;

        public List<ICoreEngineSystemObject> unityObjects;

        public List<int> childrenIndex;
    }
}

