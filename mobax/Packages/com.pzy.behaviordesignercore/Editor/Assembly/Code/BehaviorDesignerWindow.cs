namespace BehaviorDesigner.Editor
{
    using BehaviorDesigner.Runtime;
    using BehaviorDesigner.Runtime.Tasks;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.Networking;
    using Object = UnityEngine.Object;

    public class BehaviorDesignerWindow : UnityEditor.EditorWindow
    {
        [SerializeField]
        public static BehaviorDesignerWindow instance;
        private Rect mGraphRect;
        private Rect mGraphScrollRect;
        private Rect mFileToolBarRect;
        private Rect mDebugToolBarRect;
        private Rect mPropertyToolbarRect;
        private Rect mPropertyBoxRect;
        private Rect mPreferencesPaneRect;
        private Rect mFindDialogueRect;
        private Rect mQuickTaskListRect;
        private Vector2 mGraphScrollSize = new Vector2(20000f, 20000f);
        private bool mSizesInitialized;
        private float mPrevScreenWidth = -1f;
        private float mPrevScreenHeight = -1f;
        private bool mPropertiesPanelOnLeft = true;
        private Vector2 mCurrentMousePosition = Vector2.zero;
        private Vector2 mGraphScrollPosition = new Vector2(-1f, -1f);
        private Vector2 mGraphOffset = Vector2.zero;
        private float mGraphZoom = 1f;
        private float mGraphZoomMultiplier = 1f;
        private int mBehaviorToolbarSelection = 2;
        private string[] mBehaviorToolbarStrings = new string[] { "Behavior", "Tasks", "Variables", "Inspector" };
        private string mGraphStatus = string.Empty;
        private Material mGridMaterial;
        private Vector2 mSelectStartPosition = Vector2.zero;
        private Rect mSelectionArea;
        private bool mIsSelecting;
        private bool mIsDragging;
        private bool mKeepTasksSelected;
        private bool mNodeClicked;
        private Vector2 mDragDelta = Vector2.zero;
        private bool mCommandDown;
        private bool mUpdateNodeTaskMap;
        private bool mStepApplication;
        private Dictionary<NodeDesigner, BehaviorDesigner.Runtime.Tasks.Task> mNodeDesignerTaskMap;
        private bool mEditorAtBreakpoint;
        [SerializeField]
        private List<BehaviorDesigner.Editor.ErrorDetails> mErrorDetails;
        private bool mShowFindDialogue;
        private string mFindTaskValue;
        private BehaviorDesigner.Runtime.SharedVariable mFindSharedVariable;
        private bool mShowQuickTaskList;
        private UnityEditor.GenericMenu mRightClickMenu;
        [SerializeField]
        private UnityEditor.GenericMenu mBreadcrumbGameObjectBehaviorMenu;
        [SerializeField]
        private UnityEditor.GenericMenu mBreadcrumbGameObjectMenu;
        [SerializeField]
        private UnityEditor.GenericMenu mBreadcrumbBehaviorMenu;
        [SerializeField]
        private UnityEditor.GenericMenu mReferencedBehaviorsMenu;
        private bool mShowRightClickMenu;
        private bool mShowPrefPane;
        [SerializeField]
        private GraphDesigner mGraphDesigner;
        private TaskInspector mTaskInspector;
        private BehaviorDesigner.Editor.TaskList mTaskList;
        private VariableInspector mVariableInspector;
        [SerializeField]
        // 当前编辑器选中的项目,可能是资产，也可能是游戏对象
        private UnityEngine.Object mActiveObject;

        private UnityEngine.Object mPrevActiveObject;
        private BehaviorDesigner.Runtime.BehaviorSource mActiveBehaviorSource;
        private BehaviorDesigner.Runtime.BehaviorSource mExternalParent;
        private int mActiveBehaviorID = -1;
        [SerializeField]
        private List<IBehavior> mBehaviorSourceHistory = new List<IBehavior>();

        [SerializeField]
        int _mBehaviorSourceHistoryIndex = -1;
        //
        private int mBehaviorSourceHistoryIndex
        {
            get
            {
                return _mBehaviorSourceHistoryIndex;
            }
            set
            {
                _mBehaviorSourceHistoryIndex = value;
                //Debug.Log("mBehaviorSourceHistoryIndex set :" + value);
            }
        }
        private BehaviorDesigner.Runtime.BehaviorManager mBehaviorManager;
        private bool mLockActiveGameObject;
        private bool mLoadedFromInspector;
        [SerializeField]
        private bool mIsPlaying;
        private UnityWebRequest mUpdateCheckRequest;
        private DateTime mLastUpdateCheck = DateTime.MinValue;
        private string mLatestVersion;
        private bool mTakingScreenshot;
        private float mScreenshotStartGraphZoom;
        private Vector2 mScreenshotStartGraphOffset;
        private Texture2D mScreenshotTexture;
        private Rect mScreenshotGraphSize;
        private Vector2 mScreenshotGraphOffset;
        private string mScreenshotPath;
        public TaskCallbackHandler onAddTask;
        public TaskCallbackHandler onRemoveTask;
        private List<TaskSerializer> mCopiedTasks;

        //// 向编辑器中选中的游戏对象添加行为树组件
        //private void AddBehaviorComponentToEditorSelectedObject()
        //{
        //    // 没有运行，并且选中了任何资产
        //    if (!UnityEditor.EditorApplication.isPlaying && (UnityEditor.Selection.activeGameObject != null))
        //    {
        //        // 获取编辑器中选中的游戏对象
        //        GameObject undoObject = UnityEditor.Selection.activeGameObject;
        //        this.mActiveObject = UnityEditor.Selection.activeObject;
        //        this.mGraphDesigner = ScriptableObject.CreateInstance<GraphDesigner>();

        //        //BehaviorDesigner.Runtime.Behavior behavior = BehaviorUndo.AddComponent(undoObject, BehaviorDesigner.Runtime.TaskUtility.GetTypeWithinAssembly("BehaviorDesigner.Runtime.BehaviorTree")) as BehaviorDesigner.Runtime.Behavior;

        //        BehaviorDesigner.Runtime.Behavior[] components = undoObject.GetComponents<BehaviorDesigner.Runtime.Behavior>();
        //        HashSet<string> set = new HashSet<string>();
        //        string item = string.Empty;
        //        int index = 0;
        //        while (true)
        //        {
        //            if (index >= components.Length)
        //            {
        //                this.LoadBehavior(behavior.GetBehaviorSource(), false);
        //                base.Repaint();
        //                if (BehaviorDesignerPreferences.GetBool(BDPreferences.AddGameGUIComponent))
        //                {
        //                    BehaviorUndo.AddComponent(undoObject, BehaviorDesigner.Runtime.TaskUtility.GetTypeWithinAssembly("BehaviorDesigner.Runtime.BehaviorGameGUI"));
        //                }
        //                break;
        //            }
        //            item = components[index].GetBehaviorSource().behaviorName;
        //            int num2 = 2;
        //            while (true)
        //            {
        //                if (!set.Contains(item))
        //                {
        //                    components[index].GetBehaviorSource().behaviorName = item;
        //                    set.Add(components[index].GetBehaviorSource().behaviorName);
        //                    index++;
        //                    break;
        //                }
        //                item = string.Format("{0} {1}", components[index].GetBehaviorSource().behaviorName, num2);
        //                num2++;
        //            }
        //        }
        //    }
        //}

        /// <summary>
        /// 如果游戏对象是个 CoreObject 的代理调试对象，则尝试从对应 CoreObject 中获得组件
        /// 否则返回空数组
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public Behavior[] TryGetBehaviorComponentListFromDebbugerGameObject(GameObject obj)
        {
            var coreObjectDebbuger = obj.GetComponent<CoreObjectDebuger>();
            if(coreObjectDebbuger == null)
            {
                return new Behavior[] { };
            }
            var coObject = coreObjectDebbuger.co;
            var ret = coObject.GetComponents<Behavior>();
            return ret;
        }

        /// <summary>
        /// 如果游戏对象是个 CoreObject 的代理调试对象，则尝试从对应 CoreObject 中获得组件
        /// 否则返回空数组
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public Behavior TryGetBehaviorComponentFromDebbugerGameObject(GameObject obj)
        {
            var coreObjectDebbuger = obj.GetComponent<CoreObjectDebuger>();
            if (coreObjectDebbuger == null)
            {
                return null;
            }
            var coObject = coreObjectDebbuger.co;
            var ret = coObject.GetComponent<Behavior>();
            return ret;
        }

        public NodeDesigner AddTask(System.Type type, bool useMousePosition)
        {
            // 添加代码：
            // 仅在活动对象是行为树数据资产时生效
            if (!(this.mActiveObject is BehaviorDesigner.Runtime.ExternalBehavior))
            {
                return null;
            }

            // 没运行时：选中的对象必须是游戏对象或者行为树数据资产
            // 在运行时：选中的必须数据书资产
            if ((!(this.mActiveObject is GameObject) && !(this.mActiveObject is BehaviorDesigner.Runtime.ExternalBehavior)) || (UnityEditor.EditorApplication.isPlaying && !(this.mActiveObject as BehaviorDesigner.Runtime.ExternalBehavior)))
            {
                return null;
            }
            Vector2 mousePosition = new Vector2(this.mGraphRect.width / (2f * this.mGraphZoom), 150f);
            if (useMousePosition)
            {
                if (this.mShowQuickTaskList)
                {
                    mousePosition = (this.mQuickTaskListRect.position - new Vector2(this.mQuickTaskListRect.width, 0f)) / this.mGraphZoom;
                }
                else
                {
                    this.GetMousePositionInGraph(out mousePosition);
                }
            }
            mousePosition -= this.mGraphOffset;
            this.mShowQuickTaskList = false;
            GameObject mActiveObject = this.mActiveObject as GameObject;

            // 选中的是游戏对象，且没有行为树组件
            //if ((mActiveObject != null) && (mActiveObject.GetComponent<BehaviorDesigner.Runtime.Behavior>() == null))
            //{
            //    // 添加行为树组件
            //    this.AddBehaviorComponentToEditorSelectedObject();
            //}

            // 注册游戏对象的 undo 状态
            // BehaviorUndo.RegisterUndo("Add", this.mActiveBehaviorSource.Owner.GetObject());

            // 添加节点
            NodeDesigner designer = this.mGraphDesigner.AddNode(this.mActiveBehaviorSource, type, mousePosition);
            if (designer == null)
            {
                return null;
            }
            if (this.onAddTask != null)
            {
                this.onAddTask(this.mActiveBehaviorSource, designer.Task);
            }
            this.SaveBehavior();
            return designer;
        }

        private void AddTaskCallback(object obj)
        {
            this.AddTask((System.Type) obj, true);
        }

        private void BehaviorSelectionCallback(object obj)
        {
            BehaviorDesigner.Runtime.BehaviorSource behaviorSource = obj as BehaviorDesigner.Runtime.BehaviorSource;

            // 编辑器选中的对象
            //this.mActiveObject = !(behaviorSource.Owner is BehaviorDesigner.Runtime.Behavior) ? ((UnityEngine.Object) (behaviorSource.Owner as BehaviorDesigner.Runtime.ExternalBehavior)) : ((UnityEngine.Object) (behaviorSource.Owner as BehaviorDesigner.Runtime.Behavior).gameObject);
            
            // 不支持选中游戏对象，仅支持选中行为树数据
            this.mActiveObject = ((UnityEngine.Object)(behaviorSource.Owner as BehaviorDesigner.Runtime.ExternalBehavior));

            if (!this.mLockActiveGameObject)
            {
                UnityEditor.Selection.activeObject = (this.mActiveObject);
            }
            this.LoadBehavior(behaviorSource, false);
            this.UpdateGraphStatus();
            if (UnityEditor.EditorApplication.isPaused)
            {
                this.mUpdateNodeTaskMap = true;
                this.UpdateNodeTaskMap();
            }
        }

        private BehaviorDesigner.Runtime.BehaviorSource BehaviorSourceFromIBehaviorHistory(BehaviorDesigner.Runtime.IBehavior behavior)
        {
            if (behavior != null)
            {
                if (!(behavior.GetObject() is GameObject))
                {
                    return behavior.GetBehaviorSource();
                }

                // pzy: IBehavior 对应不可能是 GameObject
                // 不太明白这里要干什么
                //BehaviorDesigner.Runtime.Behavior[] components = (behavior.GetObject() as GameObject).GetComponents<BehaviorDesigner.Runtime.Behavior>();
                throw new Exception("[BehaviorDesignerWindow] not implement yet");
                BehaviorDesigner.Runtime.Behavior[] components;


                for (int i = 0; i < Enumerable.Count<BehaviorDesigner.Runtime.Behavior>(components); i++)
                {
                    if (components[i].GetBehaviorSource().BehaviorID == behavior.GetBehaviorSource().BehaviorID)
                    {
                        return components[i].GetBehaviorSource();
                    }
                }
            }
            return null;
        }

        private void BuildBreadcrumbMenus(BreadcrumbMenuType menuType)
        {
            int num3;
            Dictionary<BehaviorDesigner.Runtime.BehaviorSource, string> dictionary = new Dictionary<BehaviorDesigner.Runtime.BehaviorSource, string>();
            Dictionary<string, int> dictionary2 = new Dictionary<string, int>();
            //HashSet<Object> set = new HashSet<Object>();
            HashSet<object> set = new HashSet<object>();
            List<BehaviorDesigner.Runtime.BehaviorSource> list = new List<BehaviorDesigner.Runtime.BehaviorSource>();

            // 获得所有包含行为树插件的预制件
            //BehaviorDesigner.Runtime.Behavior[] behaviorArray = Resources.FindObjectsOfTypeAll(typeof(BehaviorDesigner.Runtime.Behavior)) as BehaviorDesigner.Runtime.Behavior[];
            
            // Core Engine 不支持预制件，使用空数组代替
            var behaviorArray = new Behavior[] { };

            int index = behaviorArray.Length - 1;
            while (true)
            {
                if (index <= -1)
                {
                    BehaviorDesigner.Runtime.ExternalBehavior[] behaviorArray2 = Resources.FindObjectsOfTypeAll(typeof(BehaviorDesigner.Runtime.ExternalBehavior)) as BehaviorDesigner.Runtime.ExternalBehavior[];
                    int num2 = behaviorArray2.Length - 1;
                    while (true)
                    {
                        if (num2 <= -1)
                        {
                            list.Sort(new AlphanumComparator<BehaviorDesigner.Runtime.BehaviorSource>());
                            num3 = 0;
                            break;
                        }
                        BehaviorDesigner.Runtime.BehaviorSource item = behaviorArray2[num2].GetBehaviorSource();
                        if (item.Owner == null)
                        {
                            item.Owner = behaviorArray2[num2];
                        }
                        list.Add(item);
                        num2--;
                    }
                    break;
                }
                BehaviorDesigner.Runtime.BehaviorSource behaviorSource = behaviorArray[index].GetBehaviorSource();
                if (behaviorSource.Owner == null)
                {
                    behaviorSource.Owner = behaviorArray[index];
                }
                list.Add(behaviorSource);
                index--;
            }
            while (true)
            {
                while (true)
                {
                    if (num3 >= list.Count)
                    {
                        if (menuType == BreadcrumbMenuType.GameObjectBehavior)
                        {
                            this.mBreadcrumbGameObjectBehaviorMenu = new UnityEditor.GenericMenu();
                        }
                        else if (menuType == BreadcrumbMenuType.GameObject)
                        {
                            this.mBreadcrumbGameObjectMenu = new UnityEditor.GenericMenu();
                        }
                        else if (menuType == BreadcrumbMenuType.Behavior)
                        {
                            this.mBreadcrumbBehaviorMenu = new UnityEditor.GenericMenu();
                        }
                        foreach (KeyValuePair<BehaviorDesigner.Runtime.BehaviorSource, string> pair in dictionary)
                        {
                            if (menuType == BreadcrumbMenuType.GameObjectBehavior)
                            {
                                this.mBreadcrumbGameObjectBehaviorMenu.AddItem(new GUIContent(pair.Value), pair.Key.Equals(this.mActiveBehaviorSource), new UnityEditor.GenericMenu.MenuFunction2(this.BehaviorSelectionCallback), pair.Key);
                                continue;
                            }
                            if (menuType == BreadcrumbMenuType.GameObject)
                            {
                                bool flag = false;
                                //flag = !(pair.Key.Owner.GetObject() is BehaviorDesigner.Runtime.ExternalBehavior) ? (pair.Key.Owner.GetObject() as BehaviorDesigner.Runtime.Behavior).gameObject.Equals(this.mActiveObject) : (pair.Key.Owner.GetObject() as BehaviorDesigner.Runtime.ExternalBehavior).GetObject().Equals(this.mActiveObject);
                                flag = !(pair.Key.Owner.GetObject() is BehaviorDesigner.Runtime.ExternalBehavior) ? (pair.Key.Owner.GetObject() as BehaviorDesigner.Runtime.Behavior).coreObject.debbugerGameObject.Equals(this.mActiveObject) : (pair.Key.Owner.GetObject() as BehaviorDesigner.Runtime.ExternalBehavior).GetObject().Equals(this.mActiveObject);
                                this.mBreadcrumbGameObjectMenu.AddItem(new GUIContent(pair.Value), flag, new UnityEditor.GenericMenu.MenuFunction2(this.BehaviorSelectionCallback), pair.Key);
                                continue;
                            }
                            if (menuType == BreadcrumbMenuType.Behavior)
                            {
                                this.mBreadcrumbBehaviorMenu.AddItem(new GUIContent(pair.Value), pair.Key.Equals(this.mActiveBehaviorSource), new UnityEditor.GenericMenu.MenuFunction2(this.BehaviorSelectionCallback), pair.Key);
                            }
                        }
                        return;
                    }
                    //Object obj2 = list[num3].Owner.GetObject();
                    var obj2 = list[num3].Owner;
                    if (menuType == BreadcrumbMenuType.Behavior)
                    {
                        if (obj2 is BehaviorDesigner.Runtime.Behavior)
                        {
                            if (!(obj2 as BehaviorDesigner.Runtime.Behavior).coreObject.Equals(this.mActiveObject))
                            {
                                break;
                            }
                        }
                        else if (!(obj2 as BehaviorDesigner.Runtime.ExternalBehavior).Equals(this.mActiveObject))
                        {
                            break;
                        }
                    }
                    if ((menuType == BreadcrumbMenuType.GameObject) && (obj2 is BehaviorDesigner.Runtime.Behavior))
                    {
                        if (set.Contains((obj2 as BehaviorDesigner.Runtime.Behavior).coreObject))
                        {
                            break;
                        }
                        set.Add((obj2 as BehaviorDesigner.Runtime.Behavior).coreObject);
                    }
                    string key = string.Empty;
                    if (obj2 is BehaviorDesigner.Runtime.Behavior)
                    {
                        if (menuType == BreadcrumbMenuType.GameObjectBehavior)
                        {
                            key = list[num3].ToString();
                        }
                        else if (menuType == BreadcrumbMenuType.GameObject)
                        {
                            key = (obj2 as BehaviorDesigner.Runtime.Behavior).coreObject.Name;
                        }
                        else if (menuType == BreadcrumbMenuType.Behavior)
                        {
                            key = list[num3].behaviorName;
                        }

                        // 行为树组件无法出现在预制件上
                        //if (!UnityEditor.AssetDatabase.GetAssetPath(obj2).Equals(string.Empty))
                        //{
                        //    key = key + " (prefab)";
                        //}
                    }
                    else
                    {
                        key = list[num3].ToString() + " (external)";
                    }
                    int num4 = 0;
                    if (!dictionary2.TryGetValue(key, out num4))
                    {
                        dictionary2.Add(key, 0);
                    }
                    else
                    {
                        dictionary2[key] = ++num4;
                        key = key + string.Format(" ({0})", num4 + 1);
                    }
                    dictionary.Add(list[num3], key);
                    break;
                }
                num3++;
            }
        }

        private void BuildRightClickMenu(NodeDesigner clickedNode)
        {
            if (this.mActiveObject != null)
            {
                this.mRightClickMenu = new UnityEditor.GenericMenu();
                if (((clickedNode == null) && (!UnityEditor.EditorApplication.isPlaying || (this.mActiveObject as BehaviorDesigner.Runtime.ExternalBehavior))) && !this.ViewOnlyMode())
                {
                    this.mTaskList.AddTasksToMenu(ref this.mRightClickMenu, null, "Add Task", new UnityEditor.GenericMenu.MenuFunction2(this.AddTaskCallback));
                    if ((this.mCopiedTasks != null) && (this.mCopiedTasks.Count > 0))
                    {
                        this.mRightClickMenu.AddItem(new GUIContent("Paste Tasks"), false, new UnityEditor.GenericMenu.MenuFunction(this.PasteNodes));
                    }
                    else
                    {
                        this.mRightClickMenu.AddDisabledItem(new GUIContent("Paste Tasks"));
                    }
                }
                if ((clickedNode != null) && !clickedNode.IsEntryDisplay)
                {
                    if (this.mGraphDesigner.SelectedNodes.Count == 1)
                    {
                        this.mRightClickMenu.AddItem(new GUIContent("Edit Script"), false, new UnityEditor.GenericMenu.MenuFunction2(this.OpenInFileEditor), clickedNode);
                        this.mRightClickMenu.AddItem(new GUIContent("Locate Script"), false, new UnityEditor.GenericMenu.MenuFunction2(this.SelectInProject), clickedNode);
                        if (!this.ViewOnlyMode())
                        {
                            this.mRightClickMenu.AddItem(new GUIContent(!clickedNode.Task.Disabled ? "Disable" : "Enable"), false, new UnityEditor.GenericMenu.MenuFunction2(this.ToggleEnableState), clickedNode);
                            if (clickedNode.IsParent)
                            {
                                this.mRightClickMenu.AddItem(new GUIContent(!clickedNode.Task.NodeData.Collapsed ? "Collapse" : "Expand"), false, new UnityEditor.GenericMenu.MenuFunction2(this.ToggleCollapseState), clickedNode);
                            }
                            this.mRightClickMenu.AddItem(new GUIContent(!clickedNode.Task.NodeData.IsBreakpoint ? "Set Breakpoint" : "Remove Breakpoint"), false, new UnityEditor.GenericMenu.MenuFunction2(this.ToggleBreakpoint), clickedNode);
                        }
                    }
                    if ((!UnityEditor.EditorApplication.isPlaying || (this.mActiveObject as BehaviorDesigner.Runtime.ExternalBehavior)) && !this.ViewOnlyMode())
                    {
                        this.mRightClickMenu.AddItem(new GUIContent(string.Format("Copy Task{0}", (this.mGraphDesigner.SelectedNodes.Count <= 1) ? string.Empty : "s")), false, new UnityEditor.GenericMenu.MenuFunction(this.CopyNodes));
                        if ((this.mCopiedTasks != null) && (this.mCopiedTasks.Count > 0))
                        {
                            this.mRightClickMenu.AddItem(new GUIContent(string.Format("Paste Task{0}", (this.mCopiedTasks.Count <= 1) ? string.Empty : "s")), false, new UnityEditor.GenericMenu.MenuFunction(this.PasteNodes));
                        }
                        else
                        {
                            this.mRightClickMenu.AddDisabledItem(new GUIContent("Paste Tasks"));
                        }
                        this.mRightClickMenu.AddItem(new GUIContent(string.Format("Duplicate Task{0}", (this.mGraphDesigner.SelectedNodes.Count <= 1) ? string.Empty : "s")), false, new UnityEditor.GenericMenu.MenuFunction(this.DuplicateNodes));
                        if (this.mGraphDesigner.SelectedNodes.Count > 0)
                        {
                            this.mTaskList.AddTasksToMenu(ref this.mRightClickMenu, (this.mGraphDesigner.SelectedNodes.Count != 1) ? null : this.mGraphDesigner.SelectedNodes[0].Task.GetType(), "Replace", new UnityEditor.GenericMenu.MenuFunction2(this.ReplaceTasksCallback));
                        }
                        this.mRightClickMenu.AddItem(new GUIContent(string.Format("Delete Task{0}", (this.mGraphDesigner.SelectedNodes.Count <= 1) ? string.Empty : "s")), false, new UnityEditor.GenericMenu.MenuFunction(this.DeleteNodes));
                    }
                }

                // 没有运行，且选中游戏对象
                //if ((!UnityEditor.EditorApplication.isPlaying || (this.mActiveObject as BehaviorDesigner.Runtime.ExternalBehavior)) && (this.mActiveObject is GameObject))
                //{
                //    if ((clickedNode != null) && !clickedNode.IsEntryDisplay)
                //    {
                //        this.mRightClickMenu.AddSeparator(string.Empty);
                //    }

                //    // 添加行为树组件
                //    this.mRightClickMenu.AddItem(new GUIContent("Add Behavior Tree"), false, new UnityEditor.GenericMenu.MenuFunction(this.AddBehaviorComponentToEditorSelectedObject));
                //    if (this.mActiveBehaviorSource != null)
                //    {
                //        // 移除行为树
                //        this.mRightClickMenu.AddItem(new GUIContent("Remove Behavior Tree"), false, new UnityEditor.GenericMenu.MenuFunction(this.RemoveBehavior));
                        
                //        // 导出行为树数据导资产
                //        this.mRightClickMenu.AddItem(new GUIContent("Save As External Behavior Tree"), false, new UnityEditor.GenericMenu.MenuFunction(this.SaveAsAsset));
                //    }
                //}
                this.mShowRightClickMenu = this.mRightClickMenu.GetItemCount() > 0;
            }
        }

        private bool CheckForAutoScroll()
        {
            Vector2 vector;
            if (!this.GetMousePositionInGraph(out vector))
            {
                return false;
            }
            if (this.mGraphScrollRect.Contains(this.mCurrentMousePosition))
            {
                return false;
            }
            if (!this.mIsDragging && (!this.mIsSelecting && (this.mGraphDesigner.ActiveNodeConnection == null)))
            {
                return false;
            }
            Vector2 amount = Vector2.zero;
            if (this.mCurrentMousePosition.y < (this.mGraphScrollRect.yMin + 15f))
            {
                amount.y = 3f;
            }
            else if (this.mCurrentMousePosition.y > (this.mGraphScrollRect.yMax - 15f))
            {
                amount.y = -3f;
            }
            if (this.mCurrentMousePosition.x < (this.mGraphScrollRect.xMin + 15f))
            {
                amount.x = 3f;
            }
            else if (this.mCurrentMousePosition.x > (this.mGraphScrollRect.xMax - 15f))
            {
                amount.x = -3f;
            }
            this.ScrollGraph(amount);
            if (this.mIsDragging)
            {
                this.mGraphDesigner.DragSelectedNodes(-amount / this.mGraphZoom, UnityEngine.Event.current.modifiers != (EventModifiers)4);
            }
            if (this.mIsSelecting)
            {
                this.mSelectStartPosition += amount / this.mGraphZoom;
            }
            return true;
        }

        private void CheckForErrors()
        {
            if (this.mErrorDetails != null)
            {
                for (int i = 0; i < this.mErrorDetails.Count; i++)
                {
                    if (this.mErrorDetails[i].NodeDesigner != null)
                    {
                        this.mErrorDetails[i].NodeDesigner.HasError = false;
                    }
                }
            }
            if (!BehaviorDesignerPreferences.GetBool(BDPreferences.ErrorChecking))
            {
                this.mErrorDetails = null;
            }
            else
            {
                BehaviorDesigner.Runtime.BehaviorSource behaviorSource = (this.mExternalParent == null) ? this.mActiveBehaviorSource : this.mExternalParent;
                this.mErrorDetails = ErrorCheck.CheckForErrors(behaviorSource);
                if (this.mErrorDetails != null)
                {
                    for (int i = 0; i < this.mErrorDetails.Count; i++)
                    {
                        if (this.mErrorDetails[i].NodeDesigner != null)
                        {
                            this.mErrorDetails[i].NodeDesigner.HasError = true;
                        }
                    }
                }
            }
            if (ErrorWindow.instance != null)
            {
                ErrorWindow.instance.ErrorDetails = this.mErrorDetails;
                ErrorWindow.instance.Repaint();
            }
        }

        private void ClearBreadcrumbMenu()
        {
            this.mBreadcrumbGameObjectBehaviorMenu = null;
            this.mBreadcrumbGameObjectMenu = null;
            this.mBreadcrumbBehaviorMenu = null;
        }

        private void ClearFindResults()
        {
            if (!string.IsNullOrEmpty(this.mFindTaskValue) || (this.mFindSharedVariable != null))
            {
                this.mFindTaskValue = string.Empty;
                this.mFindSharedVariable = null;
            }
        }

        public void ClearGraph()
        {
            this.mGraphDesigner.Clear(true);
            this.mActiveBehaviorSource = null;
            this.CheckForErrors();
            this.UpdateGraphStatus();
            base.Repaint();
        }

        public bool ContainsError(BehaviorDesigner.Runtime.Tasks.Task task, string fieldName)
        {
            if (this.mErrorDetails != null)
            {
                for (int i = 0; i < this.mErrorDetails.Count; i++)
                {
                    if (task == null)
                    {
                        if ((this.mErrorDetails[i].NodeDesigner == null) && (this.mErrorDetails[i].FieldName == fieldName))
                        {
                            return true;
                        }
                    }
                    else if ((this.mErrorDetails[i].NodeDesigner != null) && (ReferenceEquals(this.mErrorDetails[i].NodeDesigner.Task, task) && (this.mErrorDetails[i].FieldName == fieldName)))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private void CopyNodes()
        {
            this.mCopiedTasks = this.mGraphDesigner.Copy(this.mGraphOffset, this.mGraphZoom);
        }

        private void CutNodes()
        {
            this.mCopiedTasks = this.mGraphDesigner.Copy(this.mGraphOffset, this.mGraphZoom);

            // 不支持撤销
            //if ((this.mCopiedTasks != null) && (this.mCopiedTasks.Count > 0))
            //{
            //    BehaviorUndo.RegisterUndo("Cut", this.mActiveBehaviorSource.Owner.GetObject());
            //}

            this.mGraphDesigner.Delete(this.mActiveBehaviorSource, null);
            this.SaveBehavior();
        }

        private void DeleteNodes()
        {
            if (!this.ViewOnlyMode())
            {
                this.mGraphDesigner.Delete(this.mActiveBehaviorSource, this.onRemoveTask);
                this.SaveBehavior();
            }
        }

        private void DisableReferenceTasks()
        {
            if (this.IsReferencingTasks())
            {
                this.ToggleReferenceTasks();
            }
        }

        private bool Draw()
        {
            bool flag = false;
            Color color = GUI.color;
            Color color2 = GUI.backgroundColor;
            GUI.color = (Color.white);
            GUI.backgroundColor =(Color.white);
            this.DrawFileToolbar();
            this.DrawDebugToolbar();
            this.DrawPropertiesBox();
            if (this.DrawGraphArea())
            {
                flag = true;
            }
            this.DrawQuickTaskList();
            this.DrawFindDialogue();
            this.DrawPreferencesPane();
            if (this.mTakingScreenshot)
            {
                GUI.DrawTexture(new Rect(0f, 0f, base.position.width, base.position.height + 22f), BehaviorDesignerUtility.ScreenshotBackgroundTexture, 0, false);
            }
            GUI.color = (color);
            GUI.backgroundColor =(color2);
            return flag;
        }

        private void DrawDebugToolbar()
        {
            GUILayout.BeginArea(this.mDebugToolBarRect, UnityEditor.EditorStyles.toolbar);
            GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
            GUILayoutOption[] optionArray1 = new GUILayoutOption[] { GUILayout.Width(40f) };
            if (GUILayout.Button(BehaviorDesignerUtility.PlayTexture, !UnityEditor.EditorApplication.isPlaying ? UnityEditor.EditorStyles.toolbarButton : BehaviorDesignerUtility.ToolbarButtonSelectionGUIStyle, optionArray1))
            {
                UnityEditor.EditorApplication.isPlaying = !UnityEditor.EditorApplication.isPlaying;
            }
            GUILayoutOption[] optionArray2 = new GUILayoutOption[] { GUILayout.Width(40f) };
            if (GUILayout.Button(BehaviorDesignerUtility.PauseTexture, !UnityEditor.EditorApplication.isPaused ? UnityEditor.EditorStyles.toolbarButton : BehaviorDesignerUtility.ToolbarButtonSelectionGUIStyle, optionArray2))
            {
                UnityEditor.EditorApplication.isPaused = !UnityEditor.EditorApplication.isPaused;
            }
            GUILayoutOption[] optionArray3 = new GUILayoutOption[] { GUILayout.Width(40f) };
            if (GUILayout.Button(BehaviorDesignerUtility.StepTexture, UnityEditor.EditorStyles.toolbarButton, optionArray3) && UnityEditor.EditorApplication.isPlaying)
            {
                this.mStepApplication = true;
            }
            if ((this.mErrorDetails != null) && (this.mErrorDetails.Count > 0))
            {
                GUILayoutOption[] optionArray4 = new GUILayoutOption[] { GUILayout.Width(85f) };
                if (GUILayout.Button(new GUIContent(this.mErrorDetails.Count + " Error" + ((this.mErrorDetails.Count <= 1) ? string.Empty : "s"), BehaviorDesignerUtility.SmallErrorIconTexture), BehaviorDesignerUtility.ToolbarButtonLeftAlignGUIStyle, optionArray4))
                {
                    ErrorWindow.ShowWindow();
                }
            }
            GUILayout.FlexibleSpace();
            Version version = new Version("1.7.2");
            try
            {
                if (version.CompareTo(new Version(this.LatestVersion)) < 0)
                {
                    GUILayout.Label("Behavior Designer " + this.LatestVersion + " is now available.", BehaviorDesignerUtility.ToolbarLabelGUIStyle, Array.Empty<GUILayoutOption>());
                }
            }
            catch (Exception)
            {
            }
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

        private void DrawFileToolbar()
        {
            GUILayout.BeginArea(this.mFileToolBarRect, UnityEditor.EditorStyles.toolbar);
            GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
            if (GUILayout.Button(BehaviorDesignerUtility.HistoryBackwardTexture, UnityEditor.EditorStyles.toolbarButton, Array.Empty<GUILayoutOption>()) && ((this.mBehaviorSourceHistoryIndex > 0) || ((this.mActiveBehaviorSource == null) && (this.mBehaviorSourceHistoryIndex == 0))))
            {
                BehaviorDesigner.Runtime.BehaviorSource behaviorSource = null;
                if (this.mActiveBehaviorSource == null)
                {
                    this.mBehaviorSourceHistoryIndex++;
                }
                while (true)
                {
                    if ((behaviorSource != null) || ((this.mBehaviorSourceHistory.Count <= 0) || (this.mBehaviorSourceHistoryIndex <= 0)))
                    {
                        if (behaviorSource != null)
                        {
                            this.LoadBehavior(behaviorSource, false);
                        }
                        break;
                    }
                    this.mBehaviorSourceHistoryIndex--;
                    Debug.Log("mBehaviorSourceHistoryIndex: " + mBehaviorSourceHistoryIndex);
                    behaviorSource = this.BehaviorSourceFromIBehaviorHistory(this.mBehaviorSourceHistory[this.mBehaviorSourceHistoryIndex] as BehaviorDesigner.Runtime.IBehavior);
                    if ((behaviorSource == null) || ((behaviorSource.Owner == null) || (behaviorSource.Owner.GetObject() == null)))
                    {
                        this.mBehaviorSourceHistory.RemoveAt(this.mBehaviorSourceHistoryIndex);
                        behaviorSource = null;
                    }
                }
            }
            if (GUILayout.Button(BehaviorDesignerUtility.HistoryForwardTexture, UnityEditor.EditorStyles.toolbarButton, Array.Empty<GUILayoutOption>()))
            {
                BehaviorDesigner.Runtime.BehaviorSource behaviorSource = null;
                if (this.mBehaviorSourceHistoryIndex < (this.mBehaviorSourceHistory.Count - 1))
                {
                    this.mBehaviorSourceHistoryIndex++;
                    while ((behaviorSource == null) && ((this.mBehaviorSourceHistoryIndex < this.mBehaviorSourceHistory.Count) && (this.mBehaviorSourceHistoryIndex > 0)))
                    {
                        behaviorSource = this.BehaviorSourceFromIBehaviorHistory(this.mBehaviorSourceHistory[this.mBehaviorSourceHistoryIndex] as BehaviorDesigner.Runtime.IBehavior);
                        if ((behaviorSource == null) || ((behaviorSource.Owner == null) || (behaviorSource.Owner.GetObject() == null)))
                        {
                            this.mBehaviorSourceHistory.RemoveAt(this.mBehaviorSourceHistoryIndex);
                            behaviorSource = null;
                        }
                    }
                }
                if (behaviorSource != null)
                {
                    this.LoadBehavior(behaviorSource, false);
                }
            }
            GUILayoutOption[] optionArray1 = new GUILayoutOption[] { GUILayout.Width(22f) };
            if (GUILayout.Button("...", UnityEditor.EditorStyles.toolbarButton, optionArray1))
            {
                this.BuildBreadcrumbMenus(BreadcrumbMenuType.GameObjectBehavior);
                this.mBreadcrumbGameObjectBehaviorMenu.ShowAsContext();
            }
            GUILayoutOption[] optionArray2 = new GUILayoutOption[] { GUILayout.Width(140f) };
            if (GUILayout.Button(((this.mActiveObject is GameObject) || (this.mActiveObject is BehaviorDesigner.Runtime.ExternalBehavior)) ? this.mActiveObject.name : "(None Selected)", UnityEditor.EditorStyles.toolbarPopup, optionArray2))
            {
                this.BuildBreadcrumbMenus(BreadcrumbMenuType.GameObject);
                this.mBreadcrumbGameObjectMenu.ShowAsContext();
            }
            GUILayoutOption[] optionArray3 = new GUILayoutOption[] { GUILayout.Width(140f) };
            if (GUILayout.Button((this.mActiveBehaviorSource == null) ? "(None Selected)" : this.mActiveBehaviorSource.behaviorName, UnityEditor.EditorStyles.toolbarPopup, optionArray3) && (this.mActiveBehaviorSource != null))
            {
                this.BuildBreadcrumbMenus(BreadcrumbMenuType.Behavior);
                this.mBreadcrumbBehaviorMenu.ShowAsContext();
            }
            GUILayoutOption[] optionArray4 = new GUILayoutOption[] { GUILayout.Width(140f) };
            if (GUILayout.Button("Referenced Behaviors", UnityEditor.EditorStyles.toolbarPopup, optionArray4) && (this.mActiveBehaviorSource != null))
            {
                List<BehaviorDesigner.Runtime.BehaviorSource> list = this.mGraphDesigner.FindReferencedBehaviors();
                if (list.Count > 0)
                {
                    list.Sort(new AlphanumComparator<BehaviorDesigner.Runtime.BehaviorSource>());
                    this.mReferencedBehaviorsMenu = new UnityEditor.GenericMenu();
                    int num = 0;
                    while (true)
                    {
                        if (num >= list.Count)
                        {
                            this.mReferencedBehaviorsMenu.ShowAsContext();
                            break;
                        }
                        this.mReferencedBehaviorsMenu.AddItem(new GUIContent(list[num].ToString()), false, new UnityEditor.GenericMenu.MenuFunction2(this.BehaviorSelectionCallback), list[num]);
                        num++;
                    }
                }
            }
            GUILayoutOption[] optionArray5 = new GUILayoutOption[] { GUILayout.Width(22f) };
            if (GUILayout.Button("-", UnityEditor.EditorStyles.toolbarButton, optionArray5))
            {
                if (this.mActiveBehaviorSource != null)
                {
                    this.RemoveBehavior();
                }
                else
                {
                    UnityEditor.EditorUtility.DisplayDialog("Unable to Remove Behavior Tree", "No behavior tree selected.", "OK");
                }
            }

            // 绘制 + 按钮
            // 用于向选中的游戏对象或者预制件添加行为树组件
            // Core Engine 不支持预制件
            //GUILayoutOption[] optionArray6 = new GUILayoutOption[] { GUILayout.Width(22f) };
            //if (GUILayout.Button("+", UnityEditor.EditorStyles.toolbarButton, optionArray6))
            //{
            //    if (this.mActiveObject != null)
            //    {
            //        this.AddBehaviorToEditorSelectedObject();
            //    }
            //    else
            //    {
            //        UnityEditor.EditorUtility.DisplayDialog("Unable to Add Behavior Tree", "No GameObject is selected.", "OK");
            //    }
            //}

            GUILayoutOption[] optionArray7 = new GUILayoutOption[] { GUILayout.Width(42f) };
            if (GUILayout.Button("Lock", !this.mLockActiveGameObject ? UnityEditor.EditorStyles.toolbarButton : BehaviorDesignerUtility.ToolbarButtonSelectionGUIStyle, optionArray7))
            {
                if (this.mActiveObject != null)
                {
                    this.mLockActiveGameObject = !this.mLockActiveGameObject;
                    if (!this.mLockActiveGameObject)
                    {
                        this.UpdateTree(false);
                    }
                }
                else if (this.mLockActiveGameObject)
                {
                    this.mLockActiveGameObject = false;
                }
                else
                {
                    UnityEditor.EditorUtility.DisplayDialog("Unable to Lock GameObject", "No GameObject is selected.", "OK");
                }
            }
            GUI.enabled =((this.mActiveBehaviorSource == null) || ReferenceEquals(this.mExternalParent, null));
            GUILayoutOption[] optionArray8 = new GUILayoutOption[] { GUILayout.Width(46f) };
            if (GUILayout.Button("Export", UnityEditor.EditorStyles.toolbarButton, optionArray8))
            {
                if (this.mActiveBehaviorSource == null)
                {
                    UnityEditor.EditorUtility.DisplayDialog("Unable to Save Behavior Tree", "Select a behavior tree from within the scene.", "OK");
                }
                else if (this.mActiveBehaviorSource.Owner.GetObject() is BehaviorDesigner.Runtime.Behavior)
                {
                    this.SaveAsAsset();
                }
                else
                {
                    Debug.Log("[BehaviorDesignerWindow] core engine not support prefab");
                    // 不支持预制件
                    //this.SaveAsPrefab();
                }
            }
            GUI.enabled =(true);
            GUILayoutOption[] optionArray9 = new GUILayoutOption[] { GUILayout.Width(40f) };
            if (GUILayout.Button("Find", !this.mShowFindDialogue ? UnityEditor.EditorStyles.toolbarButton : BehaviorDesignerUtility.ToolbarButtonSelectionGUIStyle, optionArray9))
            {
                this.mShowFindDialogue = !this.mShowFindDialogue;
                if (this.mShowFindDialogue && this.mShowPrefPane)
                {
                    this.mShowPrefPane = false;
                }
                else if (!this.mShowFindDialogue)
                {
                    this.ClearFindResults();
                }
            }
            GUILayoutOption[] optionArray10 = new GUILayoutOption[] { GUILayout.Width(80f) };
            if (GUILayout.Button("Preferences", !this.mShowPrefPane ? UnityEditor.EditorStyles.toolbarButton : BehaviorDesignerUtility.ToolbarButtonSelectionGUIStyle, optionArray10))
            {
                this.mShowPrefPane = !this.mShowPrefPane;
                if (this.mShowPrefPane && this.mShowFindDialogue)
                {
                    this.mShowFindDialogue = false;
                    this.ClearFindResults();
                }
            }
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        private void DrawFindDialogue()
        {
            if (this.mShowFindDialogue)
            {
                GUILayout.BeginArea(this.mFindDialogueRect, BehaviorDesignerUtility.PreferencesPaneGUIStyle);
                UnityEditor.EditorGUILayout.LabelField("Find", BehaviorDesignerUtility.LabelTitleGUIStyle, Array.Empty<GUILayoutOption>());
                GUIContent content = new GUIContent("Task");
                Vector2 vector = GUI.skin.label.CalcSize(content);
                float labelWidth = UnityEditor.EditorGUIUtility.labelWidth;
                UnityEditor.EditorGUIUtility.labelWidth = vector.x + 50f;
                this.mFindTaskValue = UnityEditor.EditorGUILayout.TextField(content, this.mFindTaskValue, Array.Empty<GUILayoutOption>());
                UnityEditor.EditorGUIUtility.labelWidth = labelWidth;
                string[] names = null;
                int globalStartIndex = -1;
                int num3 = FieldInspector.GetVariablesOfType(null, (this.mFindSharedVariable != null) && this.mFindSharedVariable.IsGlobal, (this.mFindSharedVariable == null) ? string.Empty : this.mFindSharedVariable.Name, this.mActiveBehaviorSource, out names, ref globalStartIndex, true, false);
                if ((names == null) || (names.Length == 0))
                {
                    names = new string[] { "(None)" };
                }
                content.text =("Variable");
                vector = GUI.skin.label.CalcSize(content);
                UnityEditor.EditorGUIUtility.labelWidth = vector.x + 30f;
                int index = UnityEditor.EditorGUILayout.Popup("Variable", num3, names, BehaviorDesignerUtility.SharedVariableToolbarPopup, Array.Empty<GUILayoutOption>());
                UnityEditor.EditorGUIUtility.labelWidth = UnityEditor.EditorGUIUtility.labelWidth;
                if (index != num3)
                {
                    this.mFindSharedVariable = (index != 0) ? (((globalStartIndex == -1) || (index < globalStartIndex)) ? this.mActiveBehaviorSource.GetVariable(names[index]) : BehaviorDesigner.Runtime.GlobalVariables.Instance.GetVariable(names[index].Substring(8, names[index].Length - 8))) : null;
                }
                GUILayout.Space(6f);
                UnityEditor.EditorGUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
                GUILayout.FlexibleSpace();
                GUILayoutOption[] optionArray1 = new GUILayoutOption[] { GUILayout.Width(80f) };
                if (GUILayout.Button("Clear", UnityEditor.EditorStyles.toolbarButton, optionArray1))
                {
                    this.ClearFindResults();
                }
                GUILayout.FlexibleSpace();
                UnityEditor.EditorGUILayout.EndHorizontal();
                GUILayout.EndArea();
                if (GUI.changed)
                {
                    this.Find();
                }
            }
        }

        private bool DrawGraphArea()
        {
            Vector2 vector2;
            if ((UnityEngine.Event.current.type != (EventType)6) && !this.mTakingScreenshot)
            {
                var rect = new Rect(this.mGraphRect.x, this.mGraphRect.y, this.mGraphRect.width + 15f, this.mGraphRect.height + 15f);
                var position = this.mGraphScrollPosition;
                var rect2 = new Rect(0f, 0f, this.mGraphScrollSize.x, this.mGraphScrollSize.y);
                Vector2 vector = GUI.BeginScrollView(rect, position, rect2, true, true);
                if ((vector != this.mGraphScrollPosition) && ((UnityEngine.Event.current.type != (EventType)9) && (UnityEngine.Event.current.type != (EventType)11)))
                {
                    this.mGraphOffset -= (vector - this.mGraphScrollPosition) / this.mGraphZoom;
                    this.mGraphScrollPosition = vector;
                    this.mGraphDesigner.GraphDirty();
                }
                GUI.EndScrollView();
            }
            GUI.Box(this.mGraphRect, string.Empty, BehaviorDesignerUtility.GraphBackgroundGUIStyle);
            this.DrawGrid();

            //var tex3 = BehaviorDesignerUtility.TaskConnectionRunningTopTexture;
            //GUI.DrawTexture(new Rect(300, 18, 500, 500), tex3, ScaleMode.ScaleToFit);
             
            // 开始坐标变换
            EditorZoomArea.Begin(this.mGraphRect, this.mGraphZoom);
            if (!this.GetMousePositionInGraph(out vector2))
            {
                vector2 = new Vector2(-1f, -1f);
            }
            bool flag = false;



            var a = (this.mGraphDesigner != null);
            var bb = this.mGraphDesigner.DrawNodes(vector2, this.mGraphOffset);
            //Debug.Log("[BehaviorDesginerWindow] mGraphOffset: " + mGraphOffset);



            if (a && bb)
            {
                flag = true;
            }
            if (this.mTakingScreenshot && (UnityEngine.Event.current.type == (EventType)7))
            {
                this.RenderScreenshotTile();
            }
            if (this.mIsSelecting)
            {
                GUI.Box(this.GetSelectionArea(), string.Empty, BehaviorDesignerUtility.SelectionGUIStyle);
            }

            // pzy:
            // test
            //var tex2 = BehaviorDesignerUtility.TaskConnectionRunningTopTexture;
            //GUI.DrawTexture(new Rect(0, 0, 500, 500), tex2, ScaleMode.ScaleToFit);

            EditorZoomArea.End();



            this.DrawGraphStatus();



            this.DrawSelectedTaskDescription();

            // pzy:
            // text
            //var tex = BehaviorDesignerUtility.TaskConnectionRunningTopTexture;
            //GUI.DrawTexture(new Rect(0, 0, 200, 200), tex, ScaleMode.ScaleToFit);


            return flag;
        }

        private void DrawGraphStatus()
        {
            if (!this.mGraphStatus.Equals(string.Empty))
            {
                GUI.Label(new Rect(this.mGraphRect.x + 5f, this.mGraphRect.y + 5f, this.mGraphRect.width, 30f), this.mGraphStatus, BehaviorDesignerUtility.GraphStatusGUIStyle);
            }
        }

        private void DrawGrid()
        {
            if (BehaviorDesignerPreferences.GetBool(BDPreferences.SnapToGrid) && (UnityEngine.Event.current.type == (EventType)7))
            {
                this.mGridMaterial.SetPass(!UnityEditor.EditorGUIUtility.isProSkin ? 1 : 0);
                GL.PushMatrix();
                GL.Begin(1);
                this.DrawGridLines(10f * this.mGraphZoom, new Vector2((this.mGraphOffset.x % 10f) * this.mGraphZoom, (this.mGraphOffset.y % 10f) * this.mGraphZoom));
                GL.End();
                GL.PopMatrix();
                this.mGridMaterial.SetPass(!UnityEditor.EditorGUIUtility.isProSkin ? 3 : 2);
                GL.PushMatrix();
                GL.Begin(1);
                this.DrawGridLines(50f * this.mGraphZoom, new Vector2((this.mGraphOffset.x % 50f) * this.mGraphZoom, (this.mGraphOffset.y % 50f) * this.mGraphZoom));
                GL.End();
                GL.PopMatrix();
            }
        }

        private void DrawGridLines(float gridSize, Vector2 offset)
        {
            float num = this.mGraphRect.x + offset.x;
            if (offset.x < 0f)
            {
                num += gridSize;
            }
            for (float i = num; i < (this.mGraphRect.x + this.mGraphRect.width); i += gridSize)
            {
                this.DrawLine(new Vector2(i, this.mGraphRect.y), new Vector2(i, this.mGraphRect.y + this.mGraphRect.height));
            }
            float num3 = this.mGraphRect.y + offset.y;
            if (offset.y < 0f)
            {
                num3 += gridSize;
            }
            for (float j = num3; j < (this.mGraphRect.y + this.mGraphRect.height); j += gridSize)
            {
                this.DrawLine(new Vector2(this.mGraphRect.x, j), new Vector2(this.mGraphRect.x + this.mGraphRect.width, j));
            }
        }

        private void DrawLine(Vector2 p1, Vector2 p2)
        {
            GL.Vertex(p1);
            GL.Vertex(p2);
        }

        private void DrawPreferencesPane()
        {
            if (this.mShowPrefPane)
            {
                GUILayout.BeginArea(this.mPreferencesPaneRect, BehaviorDesignerUtility.PreferencesPaneGUIStyle);
                BehaviorDesignerPreferences.DrawPreferencesPane(new PreferenceChangeHandler(this.OnPreferenceChange));
                GUILayout.EndArea();
            }
        }

        private void DrawPropertiesBox()
        {
            GUILayout.BeginArea(this.mPropertyToolbarRect, UnityEditor.EditorStyles.toolbar);
            int mBehaviorToolbarSelection = this.mBehaviorToolbarSelection;
            this.mBehaviorToolbarSelection = GUILayout.Toolbar(this.mBehaviorToolbarSelection, this.mBehaviorToolbarStrings, UnityEditor.EditorStyles.toolbarButton, Array.Empty<GUILayoutOption>());
            GUILayout.EndArea();
            GUILayout.BeginArea(this.mPropertyBoxRect, BehaviorDesignerUtility.PropertyBoxGUIStyle);
            if (this.mBehaviorToolbarSelection == 0)
            {
                if (this.mActiveBehaviorSource == null)
                {
                    GUILayout.Space(5f);
                    GUILayoutOption[] optionArray1 = new GUILayoutOption[] { GUILayout.Width(285f) };
                    GUILayout.Label("No behavior tree selected. Create a new behavior tree or select one from the hierarchy.", BehaviorDesignerUtility.LabelWrapGUIStyle, optionArray1);
                }
                else
                {
                    GUILayout.Space(3f);
                    BehaviorDesigner.Runtime.BehaviorSource behaviorSource = (this.mExternalParent == null) ? this.mActiveBehaviorSource : this.mExternalParent;
                    if (!(behaviorSource.Owner is BehaviorDesigner.Runtime.Behavior))
                    {
                        bool showVariables = false;
                        ExternalBehaviorInspector.DrawInspectorGUI(behaviorSource, false, ref showVariables);
                    }
                    else
                    {
                        bool externalModification = false;
                        bool showOptions = false;


                        var behavior = behaviorSource.Owner as BehaviorDesigner.Runtime.Behavior;

                        // bahavior 不再是 UnityEngine.Object，无法支持撤销
                        //var serilizedObject = new UnityEditor.SerializedObject((UnityEngine.Object)(behaviorSource.Owner as BehaviorDesigner.Runtime.Behavior));
                        if (BehaviorInspector.DrawInspectorGUI(behavior, false, ref externalModification, ref showOptions, ref showOptions))
                        {
                            // 组件不会出现在预制件上
                            //BehaviorDesignerUtility.SetObjectDirty(behaviorSource.Owner.GetObject());
                            if (externalModification)
                            {
                                this.LoadBehavior(behaviorSource, false, false);
                            }
                        }
                    }
                }
            }
            else if (this.mBehaviorToolbarSelection == 1)
            {
                this.mTaskList.DrawTaskList(this, !this.ViewOnlyMode());
                if (mBehaviorToolbarSelection != 1)
                {
                    this.mTaskList.FocusSearchField(false, false);
                }
            }
            else if (this.mBehaviorToolbarSelection == 2)
            {
                if (this.mActiveBehaviorSource == null)
                {
                    GUILayout.Space(5f);
                    GUILayoutOption[] optionArray2 = new GUILayoutOption[] { GUILayout.Width(285f) };
                    GUILayout.Label("No behavior tree selected. Create a new behavior tree or select one from the hierarchy.", BehaviorDesignerUtility.LabelWrapGUIStyle, optionArray2);
                }
                else
                {
                    BehaviorDesigner.Runtime.BehaviorSource behaviorSource = (this.mExternalParent == null) ? this.mActiveBehaviorSource : this.mExternalParent;
                    if (this.mVariableInspector.DrawVariables(behaviorSource))
                    {
                        this.SaveBehavior();
                    }
                    if (mBehaviorToolbarSelection != 2)
                    {
                        this.mVariableInspector.FocusNameField();
                    }
                }
            }
            else if (this.mBehaviorToolbarSelection == 3)
            {
                if ((this.mGraphDesigner.SelectedNodes.Count == 1) && !this.mGraphDesigner.SelectedNodes[0].IsEntryDisplay)
                {
                    BehaviorDesigner.Runtime.Tasks.Task task = this.mGraphDesigner.SelectedNodes[0].Task;
                    if ((this.mNodeDesignerTaskMap != null) && (this.mNodeDesignerTaskMap.Count > 0))
                    {
                        NodeDesigner nodeDesigner = this.mGraphDesigner.SelectedNodes[0].Task.NodeData.NodeDesigner as NodeDesigner;
                        if ((nodeDesigner != null) && this.mNodeDesignerTaskMap.ContainsKey(nodeDesigner))
                        {
                            task = this.mNodeDesignerTaskMap[nodeDesigner];
                        }
                    }
                    if (this.mTaskInspector.DrawTaskInspector(this.mActiveBehaviorSource, this.mTaskList, task, !this.ViewOnlyMode()) && (!UnityEditor.EditorApplication.isPlaying || (this.mActiveObject as BehaviorDesigner.Runtime.ExternalBehavior)))
                    {
                        this.SaveBehavior();
                    }
                }
                else
                {
                    GUILayout.Space(5f);
                    if (this.mGraphDesigner.SelectedNodes.Count > 1)
                    {
                        GUILayoutOption[] optionArray3 = new GUILayoutOption[] { GUILayout.Width(285f) };
                        GUILayout.Label("Only one task can be selected at a time to\n view its properties.", BehaviorDesignerUtility.LabelWrapGUIStyle, optionArray3);
                    }
                    else
                    {
                        GUILayoutOption[] optionArray4 = new GUILayoutOption[] { GUILayout.Width(285f) };
                        GUILayout.Label("Select a task from the tree to\nview its properties.", BehaviorDesignerUtility.LabelWrapGUIStyle, optionArray4);
                    }
                }
            }
            GUILayout.EndArea();
        }

        private void DrawQuickTaskList()
        {
            if (this.mShowQuickTaskList)
            {
                GUILayout.BeginArea(this.mQuickTaskListRect, BehaviorDesignerUtility.PreferencesPaneGUIStyle);
                this.mTaskList.DrawQuickTaskList(this, !this.ViewOnlyMode());
                GUILayout.EndArea();
            }
        }

        private void DrawSelectedTaskDescription()
        {
            BehaviorDesigner.Runtime.Tasks.TaskDescriptionAttribute[] attributeArray;
            if (BehaviorDesignerPreferences.GetBool(BDPreferences.ShowTaskDescription) && ((this.mGraphDesigner.SelectedNodes.Count == 1) && ((attributeArray = this.mGraphDesigner.SelectedNodes[0].Task.GetType().GetCustomAttributes(typeof(BehaviorDesigner.Runtime.Tasks.TaskDescriptionAttribute), false) as BehaviorDesigner.Runtime.Tasks.TaskDescriptionAttribute[]).Length > 0)))
            {
                float num;
                float num2;
                BehaviorDesignerUtility.TaskCommentGUIStyle.CalcMinMaxWidth(new GUIContent(attributeArray[0].Description), out num, out num2);
                float num3 = Mathf.Min(400f, num2 + 20f);
                float num4 = Mathf.Min(300f, BehaviorDesignerUtility.TaskCommentGUIStyle.CalcHeight(new GUIContent(attributeArray[0].Description), num3)) + 3f;
                GUI.Box(new Rect(this.mGraphRect.x + 5f, (this.mGraphRect.yMax - num4) - 5f, num3, num4), string.Empty, BehaviorDesignerUtility.TaskDescriptionGUIStyle);
                GUI.Box(new Rect(this.mGraphRect.x + 2f, (this.mGraphRect.yMax - num4) - 5f, num3, num4), attributeArray[0].Description, BehaviorDesignerUtility.TaskCommentGUIStyle);
            }
        }

        private void DuplicateNodes()
        {
            List<TaskSerializer> copiedTasks = this.mGraphDesigner.Copy(this.mGraphOffset, this.mGraphZoom);

            // 不支持撤销
            //if ((copiedTasks != null) && (copiedTasks.Count > 0))
            //{
            //    BehaviorUndo.RegisterUndo("Duplicate", this.mActiveBehaviorSource.Owner.GetObject());
            //}

            this.mGraphDesigner.Paste(this.mActiveBehaviorSource, new Vector2((this.mGraphRect.width / (2f * this.mGraphZoom)) - this.mGraphOffset.x, 150f - this.mGraphOffset.y), copiedTasks, this.mGraphOffset, this.mGraphZoom);
            this.SaveBehavior();
        }

        private void Find()
        {
            this.mGraphDesigner.Find(this.mFindTaskValue, this.mFindSharedVariable);
        }

        private bool GetMousePositionInGraph(out Vector2 mousePosition)
        {
            mousePosition = this.mCurrentMousePosition;
            if (!this.mGraphRect.Contains(mousePosition))
            {
                return false;
            }
            if (this.mShowPrefPane && this.mPreferencesPaneRect.Contains(mousePosition))
            {
                return false;
            }
            if (this.mShowFindDialogue && this.mFindDialogueRect.Contains(mousePosition))
            {
                return false;
            }
            mousePosition -= new Vector2(this.mGraphRect.xMin, this.mGraphRect.yMin);
            mousePosition /= this.mGraphZoom;
            return true;
        }

        private bool GetMousePositionInPropertiesPane(out Vector2 mousePosition)
        {
            mousePosition = this.mCurrentMousePosition;
            if (!this.mPropertyBoxRect.Contains(mousePosition))
            {
                return false;
            }
            mousePosition.x -= this.mPropertyBoxRect.xMin;
            mousePosition.y -= this.mPropertyBoxRect.yMin;
            return true;
        }

        private Rect GetSelectionArea()
        {
            Vector2 vector;
            if (this.GetMousePositionInGraph(out vector))
            {
                float num = (this.mSelectStartPosition.x >= vector.x) ? vector.x : this.mSelectStartPosition.x;
                float num2 = (this.mSelectStartPosition.x <= vector.x) ? vector.x : this.mSelectStartPosition.x;
                float num3 = (this.mSelectStartPosition.y >= vector.y) ? vector.y : this.mSelectStartPosition.y;
                float num4 = (this.mSelectStartPosition.y <= vector.y) ? vector.y : this.mSelectStartPosition.y;
                this.mSelectionArea = new Rect(num, num3, num2 - num, num4 - num3);
            }
            return this.mSelectionArea;
        }

        private unsafe void HandleEvents()
        {
            if (!this.mTakingScreenshot)
            {
                if ((UnityEngine.Event.current.type != (EventType)1) && this.CheckForAutoScroll())
                {
                    base.Repaint();
                }
                else if ((UnityEngine.Event.current.type != (EventType)7) && (UnityEngine.Event.current.type != (EventType)8))
                {
                    switch (UnityEngine.Event.current.type)
                    {
                        case (EventType)0:
                            if (this.mShowQuickTaskList && !this.mQuickTaskListRect.Contains(this.mCurrentMousePosition))
                            {
                                this.mShowQuickTaskList = false;
                            }
                            if ((UnityEngine.Event.current.button != 0) || (UnityEngine.Event.current.modifiers == (EventModifiers)2))
                            {
                                if (((UnityEngine.Event.current.button == 1) || ((UnityEngine.Event.current.modifiers == (EventModifiers)2) && (UnityEngine.Event.current.button == 0))) && this.RightMouseDown())
                                {
                                    UnityEngine.Event.current.Use();
                                }
                            }
                            else
                            {
                                Vector2 vector;
                                if (this.GetMousePositionInGraph(out vector))
                                {
                                    if (this.LeftMouseDown(UnityEngine.Event.current.clickCount, vector))
                                    {
                                        UnityEngine.Event.current.Use();
                                    }
                                }
                                else if (this.GetMousePositionInPropertiesPane(out vector) && ((this.mBehaviorToolbarSelection == 2) && this.mVariableInspector.LeftMouseDown(this.mActiveBehaviorSource, this.mActiveBehaviorSource, vector)))
                                {
                                    UnityEngine.Event.current.Use();
                                    base.Repaint();
                                }
                            }
                            break;

                        case (EventType)1:
                            if ((UnityEngine.Event.current.button == 0) && (UnityEngine.Event.current.modifiers != (EventModifiers)2))
                            {
                                if (this.LeftMouseRelease())
                                {
                                    UnityEngine.Event.current.Use();
                                }
                            }
                            else if (((UnityEngine.Event.current.button == 1) || ((UnityEngine.Event.current.modifiers == (EventModifiers)2) && (UnityEngine.Event.current.button == 0))) && this.mShowRightClickMenu)
                            {
                                this.mShowRightClickMenu = false;
                                this.mRightClickMenu.ShowAsContext();
                                UnityEngine.Event.current.Use();
                            }
                            break;

                        case (EventType)2:
                            if (this.MouseMove())
                            {
                                UnityEngine.Event.current.Use();
                            }
                            break;

                        case (EventType)3:
                            if (UnityEngine.Event.current.button != 0)
                            {
                                if ((UnityEngine.Event.current.button == 2) && this.MousePan())
                                {
                                    UnityEngine.Event.current.Use();
                                }
                            }
                            else if (this.LeftMouseDragged())
                            {
                                UnityEngine.Event.current.Use();
                            }
                            else if ((UnityEngine.Event.current.modifiers == (EventModifiers)4) && this.MousePan())
                            {
                                UnityEngine.Event.current.Use();
                            }
                            break;

                        case (EventType)4:
                            if ((UnityEngine.Event.current.keyCode == (KeyCode)310) || (UnityEngine.Event.current.keyCode == (KeyCode)0x135))
                            {
                                this.mCommandDown = true;
                            }
                            break;

                        case (EventType)5:
                            if ((UnityEngine.Event.current.keyCode == (KeyCode)0x7f) || ((UnityEngine.Event.current.keyCode == (KeyCode)8) || UnityEngine.Event.current.commandName.Equals("Delete")))
                            {
                                if (this.PropertiesInspectorHasFocus() || (UnityEditor.EditorApplication.isPlaying && !(this.mActiveObject as BehaviorDesigner.Runtime.ExternalBehavior)))
                                {
                                    return;
                                }
                                this.DeleteNodes();
                                UnityEngine.Event.current.Use();
                            }
                            else if ((UnityEngine.Event.current.keyCode == (KeyCode)BehaviorDesignerPreferences.GetInt(BDPreferences.QuickSearchKeyCode)) && (UnityEngine.Event.current.modifiers == null))
                            {
                                Vector2 vector2;
                                if (!this.mShowQuickTaskList && this.GetMousePositionInGraph(out vector2))
                                {
                                    this.mShowQuickTaskList = true;
                                    vector2 *= this.mGraphZoom;
                                    this.mQuickTaskListRect = new Rect(vector2 + (new Vector2(200f, 0f) * 1.5f), new Vector2(200f, 200f));
                                    if (this.mQuickTaskListRect.xMax > this.mGraphRect.xMax)
                                    {
                             
                                        this.mQuickTaskListRect.x = (this.mQuickTaskListRect.x - (this.mQuickTaskListRect.xMax - this.mGraphRect.xMax));
                                    }
                                    if (this.mQuickTaskListRect.yMax > this.mGraphRect.yMax)
                                    {
                                        this.mQuickTaskListRect.y = (this.mQuickTaskListRect.y - (this.mQuickTaskListRect.yMax - this.mGraphRect.yMax));
                                    }
                                    if (this.mQuickTaskListRect.yMin < this.mGraphRect.yMin)
                                    {
                                        this.mQuickTaskListRect.y = (this.mQuickTaskListRect.y + (this.mGraphRect.yMin - this.mQuickTaskListRect.yMin));
                                    }
                                    this.mTaskList.FocusSearchField(true, true);
                                    UnityEngine.Event.current.Use();
                                    base.Repaint();
                                }
                            }
                            else if ((UnityEngine.Event.current.keyCode == (KeyCode)13) || (UnityEngine.Event.current.keyCode == (KeyCode)0x10f))
                            {
                                if ((this.mBehaviorToolbarSelection == 2) && this.mVariableInspector.HasFocus())
                                {
                                    if (this.mVariableInspector.ClearFocus(true, this.mActiveBehaviorSource))
                                    {
                                        this.SaveBehavior();
                                    }
                                    base.Repaint();
                                }
                                else
                                {
                                    this.DisableReferenceTasks();
                                    if (this.mShowQuickTaskList)
                                    {
                                        this.mTaskList.SelectQuickTask(this);
                                        base.Repaint();
                                    }
                                }
                                UnityEngine.Event.current.Use();
                            }
                            else if (UnityEngine.Event.current.keyCode == (KeyCode)0x1b)
                            {
                                this.DisableReferenceTasks();
                                if (this.mShowQuickTaskList)
                                {
                                    this.mShowQuickTaskList = false;
                                    UnityEngine.Event.current.Use();
                                    base.Repaint();
                                }
                            }
                            else if ((UnityEngine.Event.current.keyCode == (KeyCode)0x111) || (UnityEngine.Event.current.keyCode == (KeyCode)0x112))
                            {
                                if (this.mShowQuickTaskList)
                                {
                                    this.mTaskList.MoveSelectedQuickTask(UnityEngine.Event.current.keyCode == (KeyCode)0x112);
                                    UnityEngine.Event.current.Use();
                                    base.Repaint();
                                }
                            }
                            else if ((UnityEngine.Event.current.keyCode != (KeyCode)0x61) || (UnityEngine.Event.current.modifiers != (EventModifiers)2))
                            {
                                if ((UnityEngine.Event.current.keyCode == (KeyCode)310) || (UnityEngine.Event.current.keyCode == (KeyCode)0x135))
                                {
                                    this.mCommandDown = false;
                                }
                            }
                            else if (this.mShowQuickTaskList)
                            {
                                this.mTaskList.FocusSearchField(true, false);
                            }
                            else if ((this.mBehaviorToolbarSelection == 1) && (GUIUtility.keyboardControl != 0))
                            {
                                this.mTaskList.FocusSearchField(false, false);
                            }
                            break;

                        case (EventType)6:
                            if (BehaviorDesignerPreferences.GetBool(BDPreferences.MouseWhellScrolls) && !this.mCommandDown)
                            {
                                this.MousePan();
                            }
                            else if (this.MouseZoom())
                            {
                                UnityEngine.Event.current.Use();
                            }
                            break;

                        case (EventType)13:
                            if (UnityEditor.EditorApplication.isPlaying && !(this.mActiveObject as BehaviorDesigner.Runtime.ExternalBehavior))
                            {
                                return;
                            }
                            if (UnityEngine.Event.current.commandName.Equals("Copy") || (UnityEngine.Event.current.commandName.Equals("Paste") || (UnityEngine.Event.current.commandName.Equals("Cut") || (UnityEngine.Event.current.commandName.Equals("SelectAll") || UnityEngine.Event.current.commandName.Equals("Duplicate")))))
                            {
                                if (this.PropertiesInspectorHasFocus() || this.ViewOnlyMode())
                                {
                                    return;
                                }
                                UnityEngine.Event.current.Use();
                            }
                            break;

                        case (EventType)14:
                            if ((this.PropertiesInspectorHasFocus() || (UnityEditor.EditorApplication.isPlaying && !(this.mActiveObject as BehaviorDesigner.Runtime.ExternalBehavior))) || this.ViewOnlyMode())
                            {
                                return;
                            }
                            if (UnityEngine.Event.current.commandName.Equals("Copy"))
                            {
                                this.CopyNodes();
                                UnityEngine.Event.current.Use();
                            }
                            else if (UnityEngine.Event.current.commandName.Equals("Paste"))
                            {
                                this.PasteNodes();
                                UnityEngine.Event.current.Use();
                            }
                            else if (UnityEngine.Event.current.commandName.Equals("Cut"))
                            {
                                this.CutNodes();
                                UnityEngine.Event.current.Use();
                            }
                            else if (UnityEngine.Event.current.commandName.Equals("SelectAll"))
                            {
                                this.mGraphDesigner.SelectAll();
                                UnityEngine.Event.current.Use();
                            }
                            else if (UnityEngine.Event.current.commandName.Equals("Duplicate"))
                            {
                                this.DuplicateNodes();
                                UnityEngine.Event.current.Use();
                            }
                            break;

                        default:
                            break;
                    }
                }
            }
        }

        public void IdentifyNode(NodeDesigner nodeDesigner)
        {
            this.mGraphDesigner.IdentifyNode(nodeDesigner);
        }

        private int IndexForBehavior(BehaviorDesigner.Runtime.IBehavior behavior)
        {
            //if (!(behavior.GetObject() as BehaviorDesigner.Runtime.Behavior))
            if (!(behavior is BehaviorDesigner.Runtime.Behavior))
            {
                return 0;
            }
            BehaviorDesigner.Runtime.Behavior[] components = (behavior.GetObject() as BehaviorDesigner.Runtime.Behavior).coreObject.GetComponents<BehaviorDesigner.Runtime.Behavior>();
            for (int i = 0; i < components.Length; i++)
            {
                if (components[i].Equals(behavior))
                {
                    return i;
                }
            }
            return -1;
        }

        private void Init()
        {
            if (this.mTaskList == null)
            {
                this.mTaskList = ScriptableObject.CreateInstance<BehaviorDesigner.Editor.TaskList>();
            }
            if (this.mVariableInspector == null)
            {
                this.mVariableInspector = ScriptableObject.CreateInstance<VariableInspector>();
            }
            if (this.mGraphDesigner == null)
            {
                this.mGraphDesigner = ScriptableObject.CreateInstance<GraphDesigner>();
            }
            if (this.mTaskInspector == null)
            {
                this.mTaskInspector = ScriptableObject.CreateInstance<TaskInspector>();
            }
            if (this.mGridMaterial == null)
            {
                this.mGridMaterial = new Material(Shader.Find("Hidden/Behavior Designer/Grid"));
                this.mGridMaterial.hideFlags =(HideFlags)(0x3d);
                this.mGridMaterial.shader.hideFlags = (HideFlags)(0x3d);
            }
            this.mTaskList.Init();
            FieldInspector.Init();
            this.ClearBreadcrumbMenu();
        }

        public bool IsReferencingField(FieldInfo fieldInfo)
        {
            return fieldInfo.Equals(this.mTaskInspector.ActiveReferenceTaskFieldInfo);
        }

        public bool IsReferencingTasks()
        {
            return !ReferenceEquals(this.mTaskInspector.ActiveReferenceTask, null);
        }

        private bool LeftMouseDown(int clickCount, Vector2 mousePosition)
        {
            if (this.PropertiesInspectorHasFocus())
            {
                this.mTaskInspector.ClearFocus();
                this.mVariableInspector.ClearFocus(false, null);
                base.Repaint();
            }
            NodeDesigner nodeDesigner = this.mGraphDesigner.NodeAt(mousePosition, this.mGraphOffset);
            if (UnityEngine.Event.current.modifiers == (EventModifiers)4)
            {
                this.mNodeClicked = this.mGraphDesigner.IsSelected(nodeDesigner);
                return false;
            }
            if (this.IsReferencingTasks())
            {
                if (nodeDesigner == null)
                {
                    this.DisableReferenceTasks();
                }
                else
                {
                    this.ReferenceTask(nodeDesigner);
                }
                return true;
            }
            if (nodeDesigner != null)
            {
                if ((this.mGraphDesigner.HoverNode != null) && !nodeDesigner.Equals(this.mGraphDesigner.HoverNode))
                {
                    this.mGraphDesigner.ClearHover();
                    this.mGraphDesigner.Hover(nodeDesigner);
                }
                NodeConnection connection = null;
                if (!this.ViewOnlyMode() && ((connection = nodeDesigner.NodeConnectionRectContains(mousePosition, this.mGraphOffset)) != null))
                {
                    if (this.mGraphDesigner.NodeCanOriginateConnection(nodeDesigner, connection))
                    {
                        this.mGraphDesigner.ActiveNodeConnection = connection;
                    }
                    return true;
                }
                if (nodeDesigner.Contains(mousePosition, this.mGraphOffset, false))
                {
                    this.mKeepTasksSelected = false;
                    if (!this.mGraphDesigner.IsSelected(nodeDesigner))
                    {
                        if ((UnityEngine.Event.current.modifiers == (EventModifiers)1) || (UnityEngine.Event.current.modifiers == (EventModifiers)2))
                        {
                            this.mKeepTasksSelected = true;
                        }
                        else
                        {
                            this.mGraphDesigner.ClearNodeSelection();
                            this.mGraphDesigner.ClearConnectionSelection();
                            if (BehaviorDesignerPreferences.GetBool(BDPreferences.OpenInspectorOnTaskSelection))
                            {
                                this.mBehaviorToolbarSelection = 3;
                            }
                        }
                        this.mGraphDesigner.Select(nodeDesigner);
                    }
                    else if (UnityEngine.Event.current.modifiers == (EventModifiers)2)
                    {
                        this.mKeepTasksSelected = true;
                        this.mGraphDesigner.Deselect(nodeDesigner);
                    }
                    else if ((UnityEngine.Event.current.modifiers == (EventModifiers)1) && (nodeDesigner.Task is BehaviorDesigner.Runtime.Tasks.ParentTask))
                    {
                        nodeDesigner.Task.NodeData.Collapsed = !nodeDesigner.Task.NodeData.Collapsed;
                        this.mGraphDesigner.DeselectWithParent(nodeDesigner);
                    }
                    else if (clickCount == 2)
                    {
                        if ((this.mBehaviorToolbarSelection != 3) && BehaviorDesignerPreferences.GetBool(BDPreferences.OpenInspectorOnTaskDoubleClick))
                        {
                            this.mBehaviorToolbarSelection = 3;
                        }
                        else if (nodeDesigner.Task is BehaviorDesigner.Runtime.Tasks.BehaviorReference)
                        {
                            BehaviorDesigner.Runtime.Tasks.BehaviorReference task = nodeDesigner.Task as BehaviorDesigner.Runtime.Tasks.BehaviorReference;
                            if ((task.GetExternalBehaviors() != null) && ((task.GetExternalBehaviors().Length > 0) && (task.GetExternalBehaviors()[0] != null)))
                            {
                                if (this.mLockActiveGameObject)
                                {
                                    this.LoadBehavior(task.GetExternalBehaviors()[0].GetBehaviorSource(), false);
                                }
                                else
                                {
                                    UnityEditor.Selection.activeObject = ((UnityEngine.Object) task.GetExternalBehaviors()[0]);
                                }
                            }
                        }
                    }
                    this.mNodeClicked = this.mGraphDesigner.IsSelected(nodeDesigner);
                    return true;
                }
            }
            if (this.mGraphDesigner.HoverNode != null)
            {
                bool collapsedButtonClicked = false;
                if (this.mGraphDesigner.HoverNode.HoverBarButtonClick(mousePosition, this.mGraphOffset, ref collapsedButtonClicked))
                {
                    this.SaveBehavior();
                    if (collapsedButtonClicked && this.mGraphDesigner.HoverNode.Task.NodeData.Collapsed)
                    {
                        this.mGraphDesigner.DeselectWithParent(this.mGraphDesigner.HoverNode);
                    }
                    return true;
                }
            }
            List<NodeConnection> nodeConnections = new List<NodeConnection>();
            this.mGraphDesigner.NodeConnectionsAt(mousePosition, this.mGraphOffset, ref nodeConnections);
            if (nodeConnections.Count <= 0)
            {
                if (UnityEngine.Event.current.modifiers != (EventModifiers)1)
                {
                    this.mGraphDesigner.ClearNodeSelection();
                    this.mGraphDesigner.ClearConnectionSelection();
                }
                this.mSelectStartPosition = mousePosition;
                this.mIsSelecting = true;
                this.mIsDragging = false;
                this.mDragDelta = Vector2.zero;
                this.mNodeClicked = false;
                return true;
            }
            if ((UnityEngine.Event.current.modifiers != (EventModifiers)1) && (UnityEngine.Event.current.modifiers != (EventModifiers)2))
            {
                this.mGraphDesigner.ClearNodeSelection();
                this.mGraphDesigner.ClearConnectionSelection();
            }
            for (int i = 0; i < nodeConnections.Count; i++)
            {
                if (!this.mGraphDesigner.IsSelected(nodeConnections[i]))
                {
                    this.mGraphDesigner.Select(nodeConnections[i]);
                }
                else if (UnityEngine.Event.current.modifiers == (EventModifiers)2)
                {
                    this.mGraphDesigner.Deselect(nodeConnections[i]);
                }
            }
            return true;
        }

        private bool LeftMouseDragged()
        {
            Vector2 vector;
            if (!this.GetMousePositionInGraph(out vector))
            {
                return false;
            }
            if (UnityEngine.Event.current.modifiers != (EventModifiers)4)
            {
                if (this.IsReferencingTasks())
                {
                    return true;
                }
                if (this.mIsSelecting)
                {
                    this.mGraphDesigner.DeselectAll(null);
                    List<NodeDesigner> list = this.mGraphDesigner.NodesAt(this.GetSelectionArea(), this.mGraphOffset);
                    if (list != null)
                    {
                        for (int i = 0; i < list.Count; i++)
                        {
                            this.mGraphDesigner.Select(list[i]);
                        }
                    }
                    return true;
                }
                if (this.mGraphDesigner.ActiveNodeConnection != null)
                {
                    return true;
                }
            }
            if (!this.mNodeClicked || this.ViewOnlyMode())
            {
                return false;
            }
            Vector2 vector2 = Vector2.zero;
            if (!BehaviorDesignerPreferences.GetBool(BDPreferences.SnapToGrid))
            {
                vector2 = UnityEngine.Event.current.delta;
            }
            else
            {
                this.mDragDelta += UnityEngine.Event.current.delta;
                if (Mathf.Abs(this.mDragDelta.x) > 10f)
                {
                    float num2 = Mathf.Abs(this.mDragDelta.x) % 10f;
                    vector2.x = (Mathf.Abs(this.mDragDelta.x) - num2) * Mathf.Sign(this.mDragDelta.x);
                    this.mDragDelta.x = num2 * Mathf.Sign(this.mDragDelta.x);
                }
                if (Mathf.Abs(this.mDragDelta.y) > 10f)
                {
                    float num3 = Mathf.Abs(this.mDragDelta.y) % 10f;
                    vector2.y = (Mathf.Abs(this.mDragDelta.y) - num3) * Mathf.Sign(this.mDragDelta.y);
                    this.mDragDelta.y = num3 * Mathf.Sign(this.mDragDelta.y);
                }
            }
            bool flag = this.mGraphDesigner.DragSelectedNodes(vector2 / this.mGraphZoom, UnityEngine.Event.current.modifiers != (EventModifiers)4);
            if (flag)
            {
                this.mKeepTasksSelected = true;
            }
            this.mIsDragging = true;
            return flag;
        }

        private bool LeftMouseRelease()
        {
            Vector2 vector2;
            this.mNodeClicked = false;
            if (this.IsReferencingTasks())
            {
                Vector2 vector;
                if (!this.mTaskInspector.IsActiveTaskArray() && !this.mTaskInspector.IsActiveTaskNull())
                {
                    this.DisableReferenceTasks();
                    base.Repaint();
                }
                if (this.GetMousePositionInGraph(out vector))
                {
                    return true;
                }
                this.mGraphDesigner.ActiveNodeConnection = null;
                return false;
            }
            if (this.mIsSelecting)
            {
                this.mIsSelecting = false;
                return true;
            }
            if (this.mIsDragging)
            {
                // 不支持撤销
                //BehaviorUndo.RegisterUndo("Drag", this.mActiveBehaviorSource.Owner.GetObject());
                this.SaveBehavior();
                this.mIsDragging = false;
                this.mDragDelta = Vector3.zero;
                return true;
            }
            if (this.mGraphDesigner.ActiveNodeConnection == null)
            {
                Vector2 vector3;
                if ((UnityEngine.Event.current.modifiers == (EventModifiers)1) || this.mKeepTasksSelected)
                {
                    return false;
                }
                if (!this.GetMousePositionInGraph(out vector3))
                {
                    return false;
                }
                NodeDesigner designer2 = this.mGraphDesigner.NodeAt(vector3, this.mGraphOffset);
                if ((designer2 != null) && !this.mGraphDesigner.IsSelected(designer2))
                {
                    this.mGraphDesigner.DeselectAll(designer2);
                }
                return true;
            }
            if (!this.GetMousePositionInGraph(out vector2))
            {
                this.mGraphDesigner.ActiveNodeConnection = null;
                return false;
            }
            NodeDesigner nodeDesigner = this.mGraphDesigner.NodeAt(vector2, this.mGraphOffset);
            if ((nodeDesigner == null) || (nodeDesigner.Equals(this.mGraphDesigner.ActiveNodeConnection.OriginatingNodeDesigner) || !this.mGraphDesigner.NodeCanAcceptConnection(nodeDesigner, this.mGraphDesigner.ActiveNodeConnection)))
            {
                this.mGraphDesigner.ActiveNodeConnection = null;
            }
            else
            {
                this.mGraphDesigner.ConnectNodes(this.mActiveBehaviorSource, nodeDesigner);
                
                // 不支持撤销
                //BehaviorUndo.RegisterUndo("Task Connection", this.mActiveBehaviorSource.Owner.GetObject());
                this.SaveBehavior();
            }
            return true;
        }

        public void LoadBehavior(BehaviorDesigner.Runtime.BehaviorSource behaviorSource, bool loadPrevBehavior)
        {
            this.LoadBehavior(behaviorSource, loadPrevBehavior, false);
        }

        public void LoadBehavior(BehaviorDesigner.Runtime.BehaviorSource behaviorSource, bool loadPrevBehavior, bool inspectorLoad)
        {
            var a = behaviorSource != null;
            var b = !ReferenceEquals(behaviorSource.Owner, null);
            var c = !behaviorSource.Owner.Equals(null);

            if (a && b && c)
            {
                if (inspectorLoad && !this.mSizesInitialized)
                {
                    this.mActiveBehaviorID = behaviorSource.Owner.GetInstanceID();
                    this.mPrevActiveObject = UnityEditor.Selection.activeObject;
                    this.mLoadedFromInspector = true;
                }
                else if (this.mSizesInitialized)
                {
                    if (!loadPrevBehavior)
                    {
                        this.DisableReferenceTasks();
                        this.mVariableInspector.ResetSelectedVariableIndex();
                    }
                    this.mExternalParent = null;
                    this.mActiveBehaviorSource = behaviorSource;
                    if (behaviorSource.Owner is BehaviorDesigner.Runtime.Behavior)
                    {
                        //this.mActiveObject = (behaviorSource.Owner as BehaviorDesigner.Runtime.Behavior).gameObject;
                        this.mActiveObject = (behaviorSource.Owner as BehaviorDesigner.Runtime.Behavior).coreObject.debbugerGameObject as GameObject;
                        BehaviorDesigner.Runtime.ExternalBehavior externalBehavior = (behaviorSource.Owner as BehaviorDesigner.Runtime.Behavior).ExternalBehavior;
                        if ((externalBehavior != null) && !UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
                        {
                            this.mActiveBehaviorSource = externalBehavior.BehaviorSource;
                            this.mActiveBehaviorSource.Owner = externalBehavior;
                            this.mExternalParent = behaviorSource;
                            behaviorSource.CheckForSerialization(true, null);
                            if (VariableInspector.SyncVariables(behaviorSource, this.mActiveBehaviorSource.GetAllVariables()))
                            {
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
                    }
                    else
                    {
                        //this.mActiveObject = behaviorSource.Owner.GetObject();
                        this.mActiveObject = behaviorSource.OwnerAsBehaviorData;
                    }
                    this.mActiveBehaviorSource.BehaviorID = this.mActiveBehaviorSource.Owner.GetInstanceID();
                    this.mActiveBehaviorID = this.mActiveBehaviorSource.BehaviorID;
                    this.mPrevActiveObject = UnityEditor.Selection.activeObject;
                    if (((this.mBehaviorSourceHistory.Count == 0) || ((this.mBehaviorSourceHistoryIndex >= this.mBehaviorSourceHistory.Count) || (this.mBehaviorSourceHistory[this.mBehaviorSourceHistoryIndex] == null))) || (((this.mBehaviorSourceHistory[this.mBehaviorSourceHistoryIndex] as BehaviorDesigner.Runtime.IBehavior).GetBehaviorSource() != null) && !this.mActiveBehaviorSource.BehaviorID.Equals((this.mBehaviorSourceHistory[this.mBehaviorSourceHistoryIndex] as BehaviorDesigner.Runtime.IBehavior).GetBehaviorSource().BehaviorID)))
                    {
                        int index = this.mBehaviorSourceHistory.Count - 1;
                        while (true)
                        {
                            if (index <= this.mBehaviorSourceHistoryIndex)
                            {
                                this.mBehaviorSourceHistory.Add(this.mActiveBehaviorSource.Owner);
                                Debug.Log("mBehaviorSourceHistory.add  newCount: " + this.mBehaviorSourceHistory.Count);
                                this.mBehaviorSourceHistoryIndex++;
                                break;
                            }
                            this.mBehaviorSourceHistory.RemoveAt(index);
                            index--;
                        }
                    }
                    Vector2 nodePosition = new Vector2(this.mGraphRect.width / (2f * this.mGraphZoom), 150f);
                    nodePosition -= this.mGraphOffset;
                    if (this.mGraphDesigner.Load(this.mActiveBehaviorSource, loadPrevBehavior && !this.mLoadedFromInspector, nodePosition) && (this.mGraphDesigner.HasEntryNode() && (!loadPrevBehavior || this.mLoadedFromInspector)))
                    {
                        this.mGraphOffset = new Vector2(this.mGraphRect.width / (2f * this.mGraphZoom), 50f) - this.mGraphDesigner.EntryNodeOffset();
                        this.mGraphScrollPosition = ((this.mGraphScrollSize - new Vector2(this.mGraphRect.width, this.mGraphRect.height)) / 2f) - (2f * new Vector2(15f, 15f));
                    }
                    this.mLoadedFromInspector = false;
                    if (!this.mLockActiveGameObject)
                    {
                        UnityEditor.Selection.activeObject = (this.mActiveObject);
                    }
                    if (UnityEditor.EditorApplication.isPlaying && (this.mActiveBehaviorSource != null))
                    {
                        this.mRightClickMenu = null;
                        this.mUpdateNodeTaskMap = true;
                        this.UpdateNodeTaskMap();
                    }
                    this.CheckForErrors();
                    this.UpdateGraphStatus();
                    this.ClearBreadcrumbMenu();
                    this.Find();
                    base.Repaint();
                }
            }
        }

        private bool MouseMove()
        {
            Vector2 vector;
            if (!this.GetMousePositionInGraph(out vector))
            {
                return false;
            }
            NodeDesigner designer = this.mGraphDesigner.NodeAt(vector, this.mGraphOffset);
            if ((this.mGraphDesigner.HoverNode != null) && (((designer != null) && !this.mGraphDesigner.HoverNode.Equals(designer)) || !this.mGraphDesigner.HoverNode.HoverBarAreaContains(vector, this.mGraphOffset)))
            {
                this.mGraphDesigner.ClearHover();
                base.Repaint();
            }
            if (designer && (!designer.IsEntryDisplay && !this.ViewOnlyMode()))
            {
                this.mGraphDesigner.Hover(designer);
            }
            return (this.mGraphDesigner.HoverNode != null);
        }

        private bool MousePan()
        {
            Vector2 vector;
            if (!this.GetMousePositionInGraph(out vector))
            {
                return false;
            }
            Vector2 amount = UnityEngine.Event.current.delta;
            if (UnityEngine.Event.current.type == (EventType)6)
            {
                amount *= -1.5f;
                if (UnityEngine.Event.current.modifiers == (EventModifiers)2)
                {
                    amount.x = amount.y;
                    amount.y = 0f;
                }
            }
            this.ScrollGraph(amount);
            return true;
        }

        private bool MouseZoom()
        {
            Vector2 vector;
            Vector2 vector3;
            if (!this.GetMousePositionInGraph(out vector))
            {
                return false;
            }
            float num = -(UnityEngine.Event.current.delta.y * this.mGraphZoomMultiplier) / 150f;
            this.mGraphZoom += num;
            this.mGraphZoom = Mathf.Clamp(this.mGraphZoom, 0.2f, 1f);
            this.GetMousePositionInGraph(out vector3);
            this.mGraphOffset += vector3 - vector;
            this.mGraphScrollPosition += vector3 - vector;
            this.mGraphDesigner.GraphDirty();
            return true;
        }

        public void OnEnable()
        {
            this.mIsPlaying = UnityEditor.EditorApplication.isPlaying;
            this.mSizesInitialized = false;
            base.Repaint();
            UnityEditor.EditorApplication.projectChanged += this.OnProjectWindowChange;
            UnityEditor.EditorApplication.playModeStateChanged += new Action<UnityEditor.PlayModeStateChange>(this.OnPlaymodeStateChange);
            UnityEditor.Undo.undoRedoPerformed += new UnityEditor.Undo.UndoRedoCallback(this.OnUndoRedo);
            this.Init();
            this.SetBehaviorManager();
        }

        public void OnFocus()
        {
            instance = this;
            base.wantsMouseMove = true;
            this.Init();
            if (!this.mLockActiveGameObject)
            {
                this.mActiveObject = UnityEditor.Selection.activeObject;
                this.ReloadPreviousBehavior();
            }
            else if (this.mActiveBehaviorSource == null)
            {
                this.ReloadPreviousBehavior();
            }
            this.UpdateGraphStatus();
            if (this.mShowFindDialogue)
            {
                this.Find();
            }
        }

        public void OnGUI()
        {
            this.mCurrentMousePosition = UnityEngine.Event.current.mousePosition;
            this.SetupSizes();
            if (!this.mSizesInitialized)
            {
                this.mSizesInitialized = true;
                if (this.mLockActiveGameObject && (this.mActiveObject != null))
                {
                    this.ReloadPreviousBehavior();
                }
                else
                {
                    this.UpdateTree(true);
                }
            }
            this.Draw();
            this.HandleEvents();
        }

        public void OnInspectorUpdate()
        {
            if (this.mStepApplication)
            {
                UnityEditor.EditorApplication.Step();
                this.mStepApplication = false;
            }
            if (UnityEditor.EditorApplication.isPlaying && (!UnityEditor.EditorApplication.isPaused && ((this.mActiveBehaviorSource != null) && (this.mBehaviorManager != null))))
            {
                if (this.mUpdateNodeTaskMap)
                {
                    this.UpdateNodeTaskMap();
                }
                if (this.mBehaviorManager.BreakpointTree != null)
                {
                    this.mBehaviorManager.BreakpointTree = null;
                }
                base.Repaint();
            }
            if (Application.isPlaying && (this.mBehaviorManager == null))
            {
                this.SetBehaviorManager();
            }
            if ((this.mBehaviorManager != null) && this.mBehaviorManager.Dirty)
            {
                if (this.mActiveBehaviorSource != null)
                {
                    this.LoadBehavior(this.mActiveBehaviorSource, true, false);
                }
                this.mBehaviorManager.Dirty = false;
            }
            if (!UnityEditor.EditorApplication.isPlaying && this.mIsPlaying)
            {
                this.ReloadPreviousBehavior();
            }
            this.mIsPlaying = UnityEditor.EditorApplication.isPlaying;
            this.UpdateGraphStatus();
            this.UpdateCheck();
        }

        public void OnPlaymodeStateChange()
        {
            mBehaviorSourceHistoryIndex = -1;
            mBehaviorSourceHistory.Clear();

            if (UnityEditor.EditorApplication.isPlaying && !UnityEditor.EditorApplication.isPaused)
            {
                if (this.mBehaviorManager == null)
                {
                    this.SetBehaviorManager();
                    if (this.mBehaviorManager == null)
                    {
                        return;
                    }
                }
                if ((this.mBehaviorManager.BreakpointTree != null) && this.mEditorAtBreakpoint)
                {
                    this.mEditorAtBreakpoint = false;
                    this.mBehaviorManager.BreakpointTree = null;
                }
            }
            else if (!UnityEditor.EditorApplication.isPlaying || !UnityEditor.EditorApplication.isPaused)
            {
                if (!UnityEditor.EditorApplication.isPlaying)
                {
                    this.mBehaviorManager = null;
                }
            }
            else if ((this.mBehaviorManager != null) && (this.mBehaviorManager.BreakpointTree != null))
            {
                if (this.mEditorAtBreakpoint)
                {
                    this.mEditorAtBreakpoint = false;
                    this.mBehaviorManager.BreakpointTree = null;
                }
                else
                {
                    this.mEditorAtBreakpoint = true;
                    if (BehaviorDesignerPreferences.GetBool(BDPreferences.SelectOnBreakpoint) && !this.mLockActiveGameObject)
                    {
                        //UnityEditor.Selection.activeObject = ((UnityEngine.Object) this.mBehaviorManager.BreakpointTree);
                        var unityObj = GetAssociateUnityObject(this.mBehaviorManager.BreakpointTree);
                        UnityEditor.Selection.activeObject = unityObj;

                        this.LoadBehavior(this.mBehaviorManager.BreakpointTree.GetBehaviorSource(), ReferenceEquals(this.mActiveBehaviorSource, this.mBehaviorManager.BreakpointTree.GetBehaviorSource()), false);
                    }
                }
            }
        }

        /// <summary>
        /// 获得行为树组件或者行为树数据关联的 UnityEngine.Object
        /// 如果是行为树数据资产： 返回资产对象本身
        /// 如果是行为树组件：如果有关联调试游戏对象，返回该游戏对象，否则返回 null
        /// </summary>
        /// <param name="componentOrData"></param>
        /// <returns></returns>
        public UnityEngine.Object GetAssociateUnityObject(IBehavior componentOrData)
        {
            if(componentOrData is ExternalBehavior)
            {
                var e = componentOrData as ExternalBehavior;
                return e;
            }
            else if(componentOrData is Behavior)
            {
                var e = componentOrData as Behavior;
                var debuugerObject = e.coreObject.debbugerGameObject;
                if(debuugerObject == null)
                {
                    return null;
                }
                else
                {
                    return debuugerObject as GameObject;
                }
            }
            else
            {
                return null;
            }
        }

        public void OnPlaymodeStateChange(UnityEditor.PlayModeStateChange change)
        {
            this.OnPlaymodeStateChange();
        }

        private void OnPreferenceChange(BDPreferences pref, object value)
        {
            switch (pref)
            {
                case BDPreferences.CompactMode:
                    this.mGraphDesigner.GraphDirty();
                    break;

                case BDPreferences.BinarySerialization:
                    this.SaveBehavior();
                    break;

                case BDPreferences.ErrorChecking:
                    this.CheckForErrors();
                    break;

                default:
                    switch (pref)
                    {
                        case BDPreferences.GizmosViewMode:
                            break;

                        case BDPreferences.ZoomSpeedMultiplier:
                            this.mGraphZoomMultiplier = (float) value;
                            return;

                        default:
                            if (pref == BDPreferences.ShowSceneIcon)
                            {
                                break;
                            }
                            return;
                    }
                    GizmoManager.UpdateAllGizmos();
                    break;
            }
        }

        public void OnProjectWindowChange()
        {
            this.ReloadPreviousBehavior();
            this.ClearBreadcrumbMenu();
        }

        public void OnSelectionChange()
        {
            if (!this.mLockActiveGameObject)
            {
                this.UpdateTree(false);
            }
            else
            {
                this.ReloadPreviousBehavior();
            }
            this.UpdateGraphStatus();
        }

        public void OnTaskBreakpoint()
        {
            UnityEditor.EditorApplication.isPaused = true;
            base.Repaint();
        }

        private void OnUndoRedo()
        {
            if (this.mActiveBehaviorSource != null)
            {
                this.LoadBehavior(this.mActiveBehaviorSource, true, false);
            }
        }

        private void OpenInFileEditor(object obj)
        {
            TaskInspector.OpenInFileEditor((obj as NodeDesigner).Task);
        }

        private void PasteNodes()
        {
            if ((this.mActiveObject != null) && (!UnityEditor.EditorApplication.isPlaying || (this.mActiveObject as BehaviorDesigner.Runtime.ExternalBehavior)))
            {
                GameObject mActiveObject = this.mActiveObject as GameObject;
                //if ((mActiveObject != null) && (mActiveObject.GetComponent<BehaviorDesigner.Runtime.Behavior>() == null))
                if ((mActiveObject != null) && TryGetBehaviorComponentFromDebbugerGameObject(mActiveObject) == null)
                {
                    //this.AddBehaviorComponentToEditorSelectedObject();
                    throw new Exception("[BehaviorDesignerWindow] not support auto-add component to core object");
                }
                if ((this.mCopiedTasks != null) && (this.mCopiedTasks.Count > 0))
                {
                    // 不支持撤销
                    //BehaviorUndo.RegisterUndo("Paste", this.mActiveBehaviorSource.Owner.GetObject());
                }
                this.mGraphDesigner.Paste(this.mActiveBehaviorSource, new Vector2((this.mGraphRect.width / (2f * this.mGraphZoom)) - this.mGraphOffset.x, 150f - this.mGraphOffset.y), this.mCopiedTasks, this.mGraphOffset, this.mGraphZoom);
                this.SaveBehavior();
            }
        }

        private bool PropertiesInspectorHasFocus()
        {
            return (this.mTaskInspector.HasFocus() || this.mVariableInspector.HasFocus());
        }

        private void ReferenceTask(NodeDesigner nodeDesigner)
        {
            if ((nodeDesigner != null) && this.mTaskInspector.ReferenceTasks(nodeDesigner.Task))
            {
                this.SaveBehavior();
            }
        }

        private void ReloadPreviousBehavior()
        {
            if (this.mActiveObject == null)
            {
                if (this.mGraphDesigner != null)
                {
                    this.ClearGraph();
                    base.Repaint();
                }
            }
            else if (!(this.mActiveObject as GameObject))
            {
                if (!(this.mActiveObject is BehaviorDesigner.Runtime.ExternalBehavior))
                {
                    if (this.mGraphDesigner != null)
                    {
                        this.mActiveObject = null;
                        this.ClearGraph();
                    }
                }
                else
                {
                    BehaviorDesigner.Runtime.ExternalBehavior mActiveObject = this.mActiveObject as BehaviorDesigner.Runtime.ExternalBehavior;
                    BehaviorDesigner.Runtime.BehaviorSource behaviorSource = mActiveObject.BehaviorSource;
                    if (mActiveObject.BehaviorSource.Owner == null)
                    {
                        mActiveObject.BehaviorSource.Owner = mActiveObject;
                    }
                    this.LoadBehavior(behaviorSource, true, false);
                }
            }
            else
            {
                int index = -1;
                //BehaviorDesigner.Runtime.Behavior[] components = (this.mActiveObject as GameObject).GetComponents<BehaviorDesigner.Runtime.Behavior>();
                var components = TryGetBehaviorComponentListFromDebbugerGameObject(this.mActiveObject as GameObject);

                int num2 = 0;
                while (true)
                {
                    if (num2 < components.Length)
                    {
                   
                        if (components[num2].GetInstanceID() != this.mActiveBehaviorID)
                        {
                            num2++;
                            continue;
                        }
                        index = num2;
                    }
                    if (index != -1)
                    {
                        this.LoadBehavior(components[index].GetBehaviorSource(), true, false);
                    }
                    else if (Enumerable.Count<BehaviorDesigner.Runtime.Behavior>(components) > 0)
                    {
                        this.LoadBehavior(components[0].GetBehaviorSource(), true, false);
                    }
                    else if (this.mGraphDesigner != null)
                    {
                        this.ClearGraph();
                    }
                    break;
                }
            }
        }

        private void RemoveBehavior()
        {
            if (!UnityEditor.EditorApplication.isPlaying && ((this.mActiveObject is GameObject) && ((this.mActiveBehaviorSource.EntryTask == null) || ((this.mActiveBehaviorSource.EntryTask != null) && UnityEditor.EditorUtility.DisplayDialog("Remove Behavior Tree", "Are you sure you want to remove this behavior tree?", "Yes", "No")))))
            {
                GameObject mActiveObject = this.mActiveObject as GameObject;
                int index = this.IndexForBehavior(this.mActiveBehaviorSource.Owner);

                // 如果 source 是行为树数据资产时，删除资产
                // 如果是组件时，原来的行为应该是移除组件，但是 core engine 并不支持移除组件
                //BehaviorUndo.DestroyObject(this.mActiveBehaviorSource.Owner.GetObject(), true);
                var asset = this.mActiveBehaviorSource.OwnerAsBehaviorData;
                if(asset != null)
                {
                    BehaviorUndo.DestroyObject(asset, true);
                }
                var comp = this.mActiveBehaviorSource.OwnerAsComponent;
                if(comp != null)
                {
                    Debug.Log("[BehaviorDesignerWindow] core engine not support remove component");
                }

                index--;
                //if ((index == -1) && (mActiveObject.GetComponents<BehaviorDesigner.Runtime.Behavior>().Length > 0))
                if ((index == -1) && (TryGetBehaviorComponentListFromDebbugerGameObject(mActiveObject).Length > 0))
                {
                    index = 0;
                }
                if (index > -1)
                {
                    //var a = mActiveObject.GetComponents<BehaviorDesigner.Runtime.Behavior>()[index].GetBehaviorSource();
                    var a = TryGetBehaviorComponentListFromDebbugerGameObject(mActiveObject)[index].GetBehaviorSource();

                    this.LoadBehavior(a, true);
                   
                }
                else
                {
                    this.ClearGraph();
                }
                this.ClearBreadcrumbMenu();
                base.Repaint();
            }
        }

        public void RemoveSharedVariableReferences(BehaviorDesigner.Runtime.SharedVariable sharedVariable)
        {
            if (this.mGraphDesigner.RemoveSharedVariableReferences(sharedVariable))
            {
                this.SaveBehavior();
                base.Repaint();
            }
        }

        private unsafe void RenderScreenshotTile()
        {
            float num = Mathf.Min(this.mGraphRect.width, this.mScreenshotGraphSize.width - (this.mGraphOffset.x - this.mScreenshotGraphOffset.x));
            float num2 = Mathf.Min(this.mGraphRect.height, this.mScreenshotGraphSize.height + (this.mGraphOffset.y - this.mScreenshotGraphOffset.y));
            Rect rect = new Rect(this.mGraphRect.x, ((39f + this.mGraphRect.height) - num2) - 7f, num, num2);
            this.mScreenshotTexture.ReadPixels(rect, -((int) (this.mGraphOffset.x - this.mScreenshotGraphOffset.x)), (int) ((this.mScreenshotGraphSize.height - num2) + (this.mGraphOffset.y - this.mScreenshotGraphOffset.y)));
            this.mScreenshotTexture.Apply(false);
            if (((this.mScreenshotGraphSize.xMin + num) - (this.mGraphOffset.x - this.mScreenshotGraphOffset.x)) < this.mScreenshotGraphSize.xMax)
            {
                this.mGraphOffset.x -= num - 1f;
                this.mGraphDesigner.GraphDirty();
                base.Repaint();
            }
            else if (((this.mScreenshotGraphSize.yMin + num2) - (this.mGraphOffset.y - this.mScreenshotGraphOffset.y)) >= this.mScreenshotGraphSize.yMax)
            {
                this.SaveScreenshot();
            }
            else
            {
                this.mGraphOffset.y -= num2 - 1f;
                this.mGraphOffset.x = this.mScreenshotGraphOffset.x;
                this.mGraphDesigner.GraphDirty();
                base.Repaint();
            }
        }

        private void ReplaceTasksCallback(object obj)
        {
            if (this.mGraphDesigner.ReplaceSelectedNodes(this.mActiveBehaviorSource, (System.Type) obj))
            {
                this.SaveBehavior();
            }
        }

        private bool RightMouseDown()
        {
            Vector2 vector;
            if (this.IsReferencingTasks())
            {
                this.DisableReferenceTasks();
                return false;
            }
            if (!this.GetMousePositionInGraph(out vector))
            {
                return false;
            }
            NodeDesigner nodeDesigner = this.mGraphDesigner.NodeAt(vector, this.mGraphOffset);
            if ((nodeDesigner == null) || !this.mGraphDesigner.IsSelected(nodeDesigner))
            {
                this.mGraphDesigner.ClearNodeSelection();
                this.mGraphDesigner.ClearConnectionSelection();
                if (nodeDesigner != null)
                {
                    this.mGraphDesigner.Select(nodeDesigner);
                }
            }
            if (this.mGraphDesigner.HoverNode != null)
            {
                this.mGraphDesigner.ClearHover();
            }
            this.BuildRightClickMenu(nodeDesigner);
            return true;
        }

        private void SaveAsAsset()
        {
            if (this.mActiveBehaviorSource != null)
            {
                string path = UnityEditor.EditorUtility.SaveFilePanel("Save Behavior Tree", "Assets", this.mActiveBehaviorSource.behaviorName + ".asset", "asset");
                if ((path.Length == 0) || (Application.dataPath.Length >= path.Length))
                {
                    if (Path.GetExtension(path).Equals(".asset"))
                    {
                        Debug.LogError("Error: Unable to save external behavior tree. The save location must be within the Asset directory.");
                    }
                }
                else
                {
                    System.Type typeWithinAssembly = BehaviorDesigner.Runtime.TaskUtility.GetTypeWithinAssembly("BehaviorDesigner.Runtime.ExternalBehaviorTree");
                    if (BehaviorDesignerPreferences.GetBool(BDPreferences.BinarySerialization))
                    {
                        BinarySerialization.Save(this.mActiveBehaviorSource);
                    }
                    else
                    {
                        JSONSerialization.Save(this.mActiveBehaviorSource);
                    }
                    BehaviorDesigner.Runtime.ExternalBehavior owner = ScriptableObject.CreateInstance(typeWithinAssembly) as BehaviorDesigner.Runtime.ExternalBehavior;
                    BehaviorDesigner.Runtime.BehaviorSource behaviorSource = new BehaviorDesigner.Runtime.BehaviorSource(owner);
                    behaviorSource.behaviorName = this.mActiveBehaviorSource.behaviorName;
                    behaviorSource.behaviorDescription = this.mActiveBehaviorSource.behaviorDescription;
                    behaviorSource.TaskData = this.mActiveBehaviorSource.TaskData;
                    owner.SetBehaviorSource(behaviorSource);
                    path = string.Format("Assets/{0}", path.Substring(Application.dataPath.Length + 1));
                    UnityEditor.AssetDatabase.DeleteAsset(path);
                    UnityEditor.AssetDatabase.CreateAsset((UnityEngine.Object) owner, path);
                    UnityEditor.AssetDatabase.ImportAsset(path);
                    UnityEditor.Selection.activeObject = ((UnityEngine.Object) owner);
                }
            }
        }

        //private void SaveAsPrefab()
        //{
        //    if (this.mActiveBehaviorSource != null)
        //    {
        //        string path = UnityEditor.EditorUtility.SaveFilePanel("Save Behavior Tree", "Assets", this.mActiveBehaviorSource.behaviorName + ".prefab", "prefab");
        //        if ((path.Length == 0) || (Application.dataPath.Length >= path.Length))
        //        {
        //            if (Path.GetExtension(path).Equals(".prefab"))
        //            {
        //                Debug.LogError("Error: Unable to save prefab. The save location must be within the Asset directory.");
        //            }
        //        }
        //        else
        //        {
        //            GameObject obj2 = new GameObject();
        //            System.Type type = System.Type.GetType("BehaviorDesigner.Runtime.BehaviorTree, Assembly-CSharp");
        //            if (type == null)
        //            {
        //                type = System.Type.GetType("BehaviorDesigner.Runtime.BehaviorTree, Assembly-CSharp-firstpass");
        //            }
        //            BehaviorDesigner.Runtime.Behavior owner = obj2.AddComponent(type) as BehaviorDesigner.Runtime.Behavior;
        //            BehaviorDesigner.Runtime.BehaviorSource behaviorSource = new BehaviorDesigner.Runtime.BehaviorSource(owner);
        //            behaviorSource.behaviorName = this.mActiveBehaviorSource.behaviorName;
        //            behaviorSource.behaviorDescription = this.mActiveBehaviorSource.behaviorDescription;
        //            behaviorSource.TaskData = this.mActiveBehaviorSource.TaskData;
        //            owner.SetBehaviorSource(behaviorSource);
        //            path = string.Format("Assets/{0}", path.Substring(Application.dataPath.Length + 1));
        //            UnityEditor.AssetDatabase.DeleteAsset(path);
        //            GameObject obj3 = UnityEditor.PrefabUtility.SaveAsPrefabAsset(obj2, path);
        //            Object.DestroyImmediate(obj2, true);
        //            UnityEditor.AssetDatabase.ImportAsset(path);
        //            UnityEditor.Selection.activeObject = ((UnityEngine.Object) obj3);
        //        }
        //    }
        //}

        public void SaveBehavior()
        {
            if ((this.mActiveBehaviorSource != null) && (!this.ViewOnlyMode() && (!UnityEditor.EditorApplication.isPlaying || (this.mActiveObject as BehaviorDesigner.Runtime.ExternalBehavior))))
            {
                this.mGraphDesigner.Save(this.mActiveBehaviorSource);
                this.CheckForErrors();
            }
        }

        private void SaveScreenshot()
        {
            byte[] bytes = ImageConversion.EncodeToPNG(this.mScreenshotTexture);
            Object.DestroyImmediate(this.mScreenshotTexture, true);
            File.WriteAllBytes(this.mScreenshotPath, bytes);
            UnityEditor.AssetDatabase.ImportAsset(string.Format("Assets/{0}", this.mScreenshotPath.Substring(Application.dataPath.Length + 1)));
            this.mTakingScreenshot = false;
            this.mGraphZoom = this.mScreenshotStartGraphZoom;
            this.mGraphOffset = this.mScreenshotStartGraphOffset;
            this.mGraphDesigner.GraphDirty();
            base.Repaint();
        }

        private void ScrollGraph(Vector2 amount)
        {
            this.mGraphOffset += amount / this.mGraphZoom;
            this.mGraphScrollPosition -= amount;
            this.mGraphDesigner.GraphDirty();
            base.Repaint();
        }

        private void SelectInProject(object obj)
        {
            TaskInspector.SelectInProject((obj as NodeDesigner).Task);
        }

        private void SetBehaviorManager()
        {
            this.mBehaviorManager = BehaviorDesigner.Runtime.BehaviorManager.latestInstance;
            if (this.mBehaviorManager != null)
            {
                this.mBehaviorManager.OnTaskBreakpoint += new BehaviorDesigner.Runtime.BehaviorManager.BehaviorManagerHandler(this.OnTaskBreakpoint);
                this.mUpdateNodeTaskMap = true;
            }
        }

        private void SetupSizes()
        {
            float num = base.position.width;
            float num2 = base.position.height + 22f;
            if ((this.mPrevScreenWidth != num) || ((this.mPrevScreenHeight != num2) || (this.mPropertiesPanelOnLeft != BehaviorDesignerPreferences.GetBool(BDPreferences.PropertiesPanelOnLeft))))
            {
                if (BehaviorDesignerPreferences.GetBool(BDPreferences.PropertiesPanelOnLeft))
                {
                    this.mFileToolBarRect = new Rect(300f, 0f, num - 300f, 18f);
                    this.mPropertyToolbarRect = new Rect(0f, 0f, 300f, 18f);
                    this.mPropertyBoxRect = new Rect(0f, this.mPropertyToolbarRect.height, 300f, (num2 - this.mPropertyToolbarRect.height) - 21f);
                    this.mGraphRect = new Rect(300f, 18f, (num - 300f) - 15f, ((num2 - 36f) - 21f) - 15f);
                    this.mFindDialogueRect = new Rect((300f + this.mGraphRect.width) - 300f, (float) (0x12 + (!UnityEditor.EditorGUIUtility.isProSkin ? 2 : 1)), 300f, 88f);
                    this.mPreferencesPaneRect = new Rect((300f + this.mGraphRect.width) - 290f, (float) (0x12 + (!UnityEditor.EditorGUIUtility.isProSkin ? 2 : 1)), 290f, 414f);
                }
                else
                {
                    this.mFileToolBarRect = new Rect(0f, 0f, num - 300f, 18f);
                    this.mPropertyToolbarRect = new Rect(num - 300f, 0f, 300f, 18f);
                    this.mPropertyBoxRect = new Rect(num - 300f, this.mPropertyToolbarRect.height, 300f, (num2 - this.mPropertyToolbarRect.height) - 21f);
                    this.mGraphRect = new Rect(0f, 18f, (num - 300f) - 15f, ((num2 - 36f) - 21f) - 15f);
                    this.mFindDialogueRect = new Rect(this.mGraphRect.width - 300f, (float) (0x12 + (!UnityEditor.EditorGUIUtility.isProSkin ? 2 : 1)), 300f, 88f);
                    this.mPreferencesPaneRect = new Rect(this.mGraphRect.width - 290f, (float) (0x12 + (!UnityEditor.EditorGUIUtility.isProSkin ? 2 : 1)), 290f, 414f);
                }
                this.mDebugToolBarRect = new Rect(this.mGraphRect.x, (num2 - 18f) - 21f, this.mGraphRect.width + 15f, 18f);
                this.mGraphScrollRect.Set(this.mGraphRect.xMin + 15f, this.mGraphRect.yMin + 15f, this.mGraphRect.width - 30f, this.mGraphRect.height - 30f);
                if (this.mGraphScrollPosition == new Vector2(-1f, -1f))
                {
                    this.mGraphScrollPosition = ((this.mGraphScrollSize - new Vector2(this.mGraphRect.width, this.mGraphRect.height)) / 2f) - (2f * new Vector2(15f, 15f));
                }
                this.mPrevScreenWidth = num;
                this.mPrevScreenHeight = num2;
                this.mPropertiesPanelOnLeft = BehaviorDesignerPreferences.GetBool(BDPreferences.PropertiesPanelOnLeft);
            }
        }

        [UnityEditor.MenuItem("Tools/Behavior Designer/Editor", false, 0)]
        public static void ShowWindow()
        {
            BehaviorDesignerWindow window = GetWindow<BehaviorDesignerWindow>(false, "Behavior Designer");
            window.wantsMouseMove = true;
            window.minSize = new Vector2(700f, 100f);
            window.Init();
            BehaviorDesignerPreferences.InitPrefernces();
            if (BehaviorDesignerPreferences.GetBool(BDPreferences.ShowWelcomeScreen))
            {
                WelcomeScreen.ShowWindow();
            }
        }

        private unsafe void TakeScreenshot()
        {
            this.mScreenshotPath = UnityEditor.EditorUtility.SaveFilePanel("Save Screenshot", "Assets", this.mActiveBehaviorSource.behaviorName + "Screenshot.png", "png");
            if ((this.mScreenshotPath.Length == 0) || (Application.dataPath.Length >= this.mScreenshotPath.Length))
            {
                if (Path.GetExtension(this.mScreenshotPath).Equals(".png"))
                {
                    Debug.LogError("Error: Unable to save screenshot. The save location must be within the Asset directory.");
                }
            }
            else
            {
                this.mTakingScreenshot = true;
                this.mScreenshotGraphSize = this.mGraphDesigner.GraphSize(this.mGraphOffset);
                this.mGraphDesigner.GraphDirty();
                if ((this.mScreenshotGraphSize.width == 0f) || (this.mScreenshotGraphSize.height == 0f))
                {
                    this.mScreenshotGraphSize = new Rect(0f, 0f, 100f, 100f);
                }
                this.mScreenshotStartGraphZoom = this.mGraphZoom;
                this.mScreenshotStartGraphOffset = this.mGraphOffset;
                this.mGraphZoom = 1f;

                this.mGraphOffset.x -= this.mScreenshotGraphSize.xMin - 10f;
                this.mGraphOffset.y -= this.mScreenshotGraphSize.yMin - 10f;
                this.mScreenshotGraphOffset = this.mGraphOffset;
                this.mScreenshotGraphSize.Set(this.mScreenshotGraphSize.xMin - 9f, this.mScreenshotGraphSize.yMin, this.mScreenshotGraphSize.width + 18f, this.mScreenshotGraphSize.height + 18f);
                //this.mScreenshotTexture = new Texture2D((int) this.mScreenshotGraphSize.width, (int) this.mScreenshotGraphSize.height, 3, false);
                this.mScreenshotTexture = new Texture2D((int)this.mScreenshotGraphSize.width, (int)this.mScreenshotGraphSize.height, (TextureFormat)3, false);

                base.Repaint();
            }
        }

        private void ToggleBreakpoint(object obj)
        {
            (obj as NodeDesigner).ToggleBreakpoint();
            this.SaveBehavior();
            base.Repaint();
        }

        private void ToggleCollapseState(object obj)
        {
            NodeDesigner nodeDesigner = obj as NodeDesigner;
            if (nodeDesigner.ToggleCollapseState())
            {
                this.mGraphDesigner.DeselectWithParent(nodeDesigner);
            }
            this.SaveBehavior();
            base.Repaint();
        }

        private void ToggleEnableState(object obj)
        {
            (obj as NodeDesigner).ToggleEnableState();
            this.SaveBehavior();
            base.Repaint();
        }

        public void ToggleReferenceTasks()
        {
            this.ToggleReferenceTasks(null, null);
        }

        public void ToggleReferenceTasks(BehaviorDesigner.Runtime.Tasks.Task task, FieldInfo fieldInfo)
        {
            bool flag = !this.IsReferencingTasks();
            this.mTaskInspector.SetActiveReferencedTasks(!flag ? null : task, !flag ? null : fieldInfo);
            this.UpdateGraphStatus();
        }

        public void Update()
        {
            if (this.mTakingScreenshot)
            {
                base.Repaint();
            }
        }

        private bool UpdateCheck()
        {
            if ((this.mUpdateCheckRequest != null) && this.mUpdateCheckRequest.isDone)
            {
                if (!string.IsNullOrEmpty(this.mUpdateCheckRequest.error))
                {
                    this.mUpdateCheckRequest = null;
                    return false;
                }
                if (!"1.7.2".ToString().Equals(this.mUpdateCheckRequest.downloadHandler.text))
                {
                    this.LatestVersion = this.mUpdateCheckRequest.downloadHandler.text;
                }
                this.mUpdateCheckRequest = null;
            }
            if (BehaviorDesignerPreferences.GetBool(BDPreferences.UpdateCheck) && (DateTime.Compare(this.LastUpdateCheck.AddDays(1.0), DateTime.UtcNow) < 0))
            {
                string uri = string.Format("https://opsive.com/asset/UpdateCheck.php?asset=BehaviorDesigner&version={0}&unityversion={1}&devplatform={2}&targetplatform={3}", new object[] { "1.7.2", Application.unityVersion, Application.platform, UnityEditor.EditorUserBuildSettings.activeBuildTarget });
                this.mUpdateCheckRequest = UnityWebRequest.Get(uri);
                this.mUpdateCheckRequest.SendWebRequest();
                this.LastUpdateCheck = DateTime.UtcNow;
            }
            return !ReferenceEquals(this.mUpdateCheckRequest, null);
        }

        public void UpdateGraphStatus()
        {
            if ((this.mActiveObject == null) || ((this.mGraphDesigner == null) || (!(this.mActiveObject is GameObject) && !(this.mActiveObject is BehaviorDesigner.Runtime.ExternalBehavior))))
            {
                this.mGraphStatus = "Select a GameObject";
            }
            //else if ((this.mActiveObject is GameObject) && ReferenceEquals((this.mActiveObject as GameObject).GetComponent<BehaviorDesigner.Runtime.Behavior>(), null))
            else if ((this.mActiveObject is GameObject) && TryGetBehaviorComponentFromDebbugerGameObject(this.mActiveObject as GameObject) == null)
            {
                // 选中了游戏对象，但是没有行为树组件
                this.mGraphStatus = "Right Click, Add a Behavior Tree Component";
            }
            else if (this.ViewOnlyMode() && (this.mActiveBehaviorSource != null))
            {
                BehaviorDesigner.Runtime.ExternalBehavior externalBehavior = (this.mActiveBehaviorSource.Owner.GetObject() as BehaviorDesigner.Runtime.Behavior).ExternalBehavior;
                this.mGraphStatus = (externalBehavior == null) ? (this.mActiveBehaviorSource.ToString() + " (View Only Mode)") : (externalBehavior.BehaviorSource.ToString() + " (View Only Mode)");
            }
            else if (!this.mGraphDesigner.HasEntryNode())
            {
                this.mGraphStatus = "Add a Task";
            }
            else if (this.IsReferencingTasks())
            {
                this.mGraphStatus = "Select tasks to reference (right click to exit)";
            }
            else if ((this.mActiveBehaviorSource != null) && ((this.mActiveBehaviorSource.Owner != null) && (this.mActiveBehaviorSource.Owner.GetObject() != null)))
            {
                this.mGraphStatus = (this.mExternalParent == null) ? this.mActiveBehaviorSource.ToString() : (this.mExternalParent.ToString() + " (Editing External Behavior)");
            }
        }

        private void UpdateNodeTaskMap()
        {
            if (this.mUpdateNodeTaskMap && (this.mBehaviorManager != null))
            {
                BehaviorDesigner.Runtime.Behavior owner = this.mActiveBehaviorSource.Owner as BehaviorDesigner.Runtime.Behavior;
                List<BehaviorDesigner.Runtime.Tasks.Task> taskList = this.mBehaviorManager.GetTaskList(owner);
                if (taskList != null)
                {
                    this.mNodeDesignerTaskMap = new Dictionary<NodeDesigner, BehaviorDesigner.Runtime.Tasks.Task>();
                    int num = 0;
                    while (true)
                    {
                        if (num >= taskList.Count)
                        {
                            this.mUpdateNodeTaskMap = false;
                            break;
                        }
                        NodeDesigner nodeDesigner = taskList[num].NodeData.NodeDesigner as NodeDesigner;
                        if ((nodeDesigner != null) && !this.mNodeDesignerTaskMap.ContainsKey(nodeDesigner))
                        {
                            this.mNodeDesignerTaskMap.Add(nodeDesigner, taskList[num]);
                        }
                        num++;
                    }
                }
            }
        }

        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="firstLoad"></param>
        private void UpdateTree(bool firstLoad)
        {
            bool flag = firstLoad;

            
            if (UnityEditor.Selection.activeObject == null)
            {
                // 没有选中任何 Unity 对象
                if ((this.mActiveObject != null) && (this.mActiveBehaviorSource != null))
                {
                    this.mPrevActiveObject = this.mActiveObject;
                }
                this.mActiveObject = null;
                this.ClearGraph();
            }
            else
            {
                bool loadPrevBehavior = false;
                if (!UnityEditor.Selection.activeObject.Equals(this.mActiveObject))
                {
                    this.mActiveObject = UnityEditor.Selection.activeObject;
                    flag = true;
                }
                BehaviorDesigner.Runtime.BehaviorSource behaviorSource = null;
                GameObject mActiveObject = this.mActiveObject as GameObject;



                //if ((mActiveObject == null) || (mActiveObject.GetComponent<BehaviorDesigner.Runtime.Behavior>() == null))
                if (mActiveObject == null || TryGetBehaviorComponentFromDebbugerGameObject(mActiveObject) == null)
                {
                    // 如果选中的不是游戏对象
                    // 或者选中的是游戏对象，但是上面找不到关联组件

                    if (!(this.mActiveObject is BehaviorDesigner.Runtime.ExternalBehavior))
                    {
                        this.mPrevActiveObject = null;
                    }
                    else
                    {
                        BehaviorDesigner.Runtime.ExternalBehavior behavior = this.mActiveObject as BehaviorDesigner.Runtime.ExternalBehavior;
                        if (behavior.BehaviorSource.Owner == null)
                        {
                            behavior.BehaviorSource.Owner = behavior;
                        }
                        if (flag && this.mActiveObject.Equals(this.mPrevActiveObject))
                        {
                            loadPrevBehavior = true;
                        }
                        behaviorSource = behavior.BehaviorSource;
                    }
                }
                else if (!flag)
                {
                    //BehaviorDesigner.Runtime.Behavior[] components = mActiveObject.GetComponents<BehaviorDesigner.Runtime.Behavior>();
                    var components = TryGetBehaviorComponentListFromDebbugerGameObject(mActiveObject);
                    bool flag3 = false;
                    if (this.mActiveBehaviorSource != null)
                    {
                        for (int i = 0; i < components.Length; i++)
                        {
                            if (components[i].Equals(this.mActiveBehaviorSource.Owner))
                            {
                                flag3 = true;
                                break;
                            }
                        }
                    }
                    if (!flag3)
                    {
                        //behaviorSource = mActiveObject.GetComponents<BehaviorDesigner.Runtime.Behavior>()[0].GetBehaviorSource();
                        behaviorSource = TryGetBehaviorComponentListFromDebbugerGameObject(mActiveObject)[0].GetBehaviorSource();
                    }
                    else
                    {
                        behaviorSource = this.mActiveBehaviorSource;
                        loadPrevBehavior = true;
                    }
                }
                else if (!this.mActiveObject.Equals(this.mPrevActiveObject) || (this.mActiveBehaviorID == -1))
                {
                    //behaviorSource = mActiveObject.GetComponents<BehaviorDesigner.Runtime.Behavior>()[0].GetBehaviorSource();
                    behaviorSource = TryGetBehaviorComponentListFromDebbugerGameObject(mActiveObject)[0].GetBehaviorSource();
                }
                else
                {
                    loadPrevBehavior = true;
                    int index = -1;
                    //BehaviorDesigner.Runtime.Behavior[] components = (this.mActiveObject as GameObject).GetComponents<BehaviorDesigner.Runtime.Behavior>();
                    var components = TryGetBehaviorComponentListFromDebbugerGameObject(this.mActiveObject as GameObject);

                    int num2 = 0;
                    while (true)
                    {
                        if (num2 < components.Length)
                        {
                            if (components[num2].GetInstanceID() != this.mActiveBehaviorID)
                            {
                                num2++;
                                continue;
                            }
                            index = num2;
                        }
                        if (index != -1)
                        {
                            //behaviorSource = mActiveObject.GetComponents<BehaviorDesigner.Runtime.Behavior>()[index].GetBehaviorSource();
                            behaviorSource = TryGetBehaviorComponentListFromDebbugerGameObject(mActiveObject)[index].GetBehaviorSource();
                        }
                        else if (Enumerable.Count<BehaviorDesigner.Runtime.Behavior>(components) > 0)
                        {
                            //behaviorSource = mActiveObject.GetComponents<BehaviorDesigner.Runtime.Behavior>()[0].GetBehaviorSource();
                            behaviorSource = TryGetBehaviorComponentListFromDebbugerGameObject(mActiveObject)[0].GetBehaviorSource();
                        }
                        break;
                    }
                }
                if (behaviorSource != null)
                {
                    this.LoadBehavior(behaviorSource, loadPrevBehavior, false);
                }
                else if (behaviorSource == null)
                {
                    this.ClearGraph();
                }
            }
        }

        public bool ViewOnlyMode()
        {
            //return (!Application.isPlaying ? (((this.mActiveBehaviorSource != null) && ((this.mActiveBehaviorSource.Owner != null) && !this.mActiveBehaviorSource.Owner.Equals(null))) && ((this.mActiveBehaviorSource.Owner.GetObject() is BehaviorDesigner.Runtime.Behavior) && (!BehaviorDesignerPreferences.GetBool(BDPreferences.EditablePrefabInstances) && ((UnityEditor.PrefabUtility.GetPrefabAssetType(this.mActiveBehaviorSource.Owner.GetObject()) == UnityEditor.PrefabAssetType.Regular) || (UnityEditor.PrefabUtility.GetPrefabAssetType(this.mActiveBehaviorSource.Owner.GetObject()) == UnityEditor.PrefabAssetType.Variant))))) : false);
            return (!Application.isPlaying ? (((this.mActiveBehaviorSource != null) && ((this.mActiveBehaviorSource.Owner != null) && !this.mActiveBehaviorSource.Owner.Equals(null))) && ((this.mActiveBehaviorSource.Owner.GetObject() is BehaviorDesigner.Runtime.Behavior) && (!BehaviorDesignerPreferences.GetBool(BDPreferences.EditablePrefabInstances) ))) : false);
        }

        public List<BehaviorDesigner.Editor.ErrorDetails> ErrorDetails
        {
            get
            {
                return this.mErrorDetails;
            }
        }

        public BehaviorDesigner.Editor.TaskList TaskList
        {
            get
            {
                return this.mTaskList;
            }
        }

        public BehaviorDesigner.Runtime.BehaviorSource ActiveBehaviorSource
        {
            get
            {
                return this.mActiveBehaviorSource;
            }
        }

        public int ActiveBehaviorID
        {
            get
            {
                return this.mActiveBehaviorID;
            }
        }

        private DateTime LastUpdateCheck
        {
            get
            {
                try
                {
                    if (!(this.mLastUpdateCheck != DateTime.MinValue))
                    {
                        this.mLastUpdateCheck = DateTime.Parse(UnityEditor.EditorPrefs.GetString("BehaviorDesignerLastUpdateCheck", "1/1/1971 00:00:01"), CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        return this.mLastUpdateCheck;
                    }
                }
                catch (Exception)
                {
                    this.mLastUpdateCheck = DateTime.UtcNow;
                }
                return this.mLastUpdateCheck;
            }
            set
            {
                this.mLastUpdateCheck = value;
                UnityEditor.EditorPrefs.SetString("BehaviorDesignerLastUpdateCheck", this.mLastUpdateCheck.ToString(CultureInfo.InvariantCulture));
            }
        }

        public string LatestVersion
        {
            get
            {
                if (string.IsNullOrEmpty(this.mLatestVersion))
                {
                    this.mLatestVersion = UnityEditor.EditorPrefs.GetString("BehaviorDesignerLatestVersion", "1.7.2".ToString());
                }
                return this.mLatestVersion;
            }
            set
            {
                this.mLatestVersion = value;
                UnityEditor.EditorPrefs.SetString("BehaviorDesignerLatestVersion", this.mLatestVersion);
            }
        }

        public TaskCallbackHandler OnAddTask
        {
            get
            {
                return this.onAddTask;
            }
            set
            {
                this.onAddTask += value;
            }
        }

        public TaskCallbackHandler OnRemoveTask
        {
            get
            {
                return this.onRemoveTask;
            }
            set
            {
                this.onRemoveTask += value;
            }
        }

        private enum BreadcrumbMenuType
        {
            GameObjectBehavior,
            GameObject,
            Behavior
        }

        public delegate void TaskCallbackHandler(BehaviorDesigner.Runtime.BehaviorSource behaviorSource, BehaviorDesigner.Runtime.Tasks.Task task);
    }
}

