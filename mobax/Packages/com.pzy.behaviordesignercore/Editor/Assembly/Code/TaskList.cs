namespace BehaviorDesigner.Editor
{
    using BehaviorDesigner.Runtime;
    using BehaviorDesigner.Runtime.Tasks;
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using UnityEditor;
    using UnityEngine;

    [Serializable]
    public class TaskList : ScriptableObject
    {
        private List<CategoryList> mCategoryList;
        private List<CategoryList> mQuickCategoryList;
        private Dictionary<System.Type, BehaviorDesigner.Runtime.Tasks.TaskNameAttribute[]> mTaskNameAttribute = new Dictionary<System.Type, BehaviorDesigner.Runtime.Tasks.TaskNameAttribute[]>();
        private Vector2 mScrollPosition = Vector2.zero;
        private string mSearchString = string.Empty;
        private bool mFocusSearch;
        private Vector2 mQuickScrollPosition = Vector2.zero;
        private string mQuickSearchString = string.Empty;
        private bool mFocusQuickSearch;
        private int mSelectedQuickIndex;
        private System.Type mSelectedQuickIndexType;
        private int mQuickIndexCount;

        private void AddCategoryTasksToMenu(ref UnityEditor.GenericMenu genericMenu, List<CategoryList> categoryList, System.Type selectedTaskType, string parentName, UnityEditor.GenericMenu.MenuFunction2 menuFunction)
        {
            for (int i = 0; i < categoryList.Count; i++)
            {
                if (categoryList[i].Subcategories != null)
                {
                    this.AddCategoryTasksToMenu(ref genericMenu, categoryList[i].Subcategories, selectedTaskType, parentName, menuFunction);
                }
                if (categoryList[i].Tasks != null)
                {
                    for (int j = 0; j < categoryList[i].Tasks.Count; j++)
                    {
                        if (parentName.Equals(string.Empty))
                        {
                            genericMenu.AddItem(new GUIContent(string.Format("{0}/{1}", categoryList[i].Fullpath, categoryList[i].Tasks[j].Name.ToString())), categoryList[i].Tasks[j].Type.Equals(selectedTaskType), menuFunction, categoryList[i].Tasks[j].Type);
                        }
                        else
                        {
                            genericMenu.AddItem(new GUIContent(string.Format("{0}/{1}/{2}", parentName, categoryList[i].Fullpath, categoryList[i].Tasks[j].Name.ToString())), categoryList[i].Tasks[j].Type.Equals(selectedTaskType), menuFunction, categoryList[i].Tasks[j].Type);
                        }
                    }
                }
            }
        }

        public void AddTasksToMenu(ref UnityEditor.GenericMenu genericMenu, System.Type selectedTaskType, string parentName, UnityEditor.GenericMenu.MenuFunction2 menuFunction)
        {
            this.AddCategoryTasksToMenu(ref genericMenu, this.mCategoryList, selectedTaskType, parentName, menuFunction);
        }

        public void AddTaskTypesToMenu(int typeIndex, ref UnityEditor.GenericMenu genericMenu, System.Type selectedTaskType, System.Type ignoredTaskType, string parentName, bool includeFullPath, UnityEditor.GenericMenu.MenuFunction2 menuFunction)
        {
            if ((typeIndex >= 0) && (typeIndex < this.mCategoryList.Count))
            {
                if (this.mCategoryList[typeIndex].Tasks != null)
                {
                    for (int i = 0; i < this.mCategoryList[typeIndex].Tasks.Count; i++)
                    {
                        if (this.mCategoryList[typeIndex].Tasks[i].Type != ignoredTaskType)
                        {
                            if (parentName.Equals(string.Empty))
                            {
                                if (includeFullPath)
                                {
                                    genericMenu.AddItem(new GUIContent(string.Format("{0}/{1}", this.mCategoryList[typeIndex].Fullpath, this.mCategoryList[typeIndex].Tasks[i].Name.ToString())), this.mCategoryList[typeIndex].Tasks[i].Type.Equals(selectedTaskType), menuFunction, this.mCategoryList[typeIndex].Tasks[i].Type);
                                }
                                else
                                {
                                    genericMenu.AddItem(new GUIContent(this.mCategoryList[typeIndex].Tasks[i].Name.ToString()), this.mCategoryList[typeIndex].Tasks[i].Type.Equals(selectedTaskType), menuFunction, this.mCategoryList[typeIndex].Tasks[i].Type);
                                }
                            }
                            else if (includeFullPath)
                            {
                                genericMenu.AddItem(new GUIContent(string.Format("{0}/{1}/{2}", parentName, this.mCategoryList[typeIndex].Fullpath, this.mCategoryList[typeIndex].Tasks[i].Name.ToString())), this.mCategoryList[typeIndex].Tasks[i].Type.Equals(selectedTaskType), menuFunction, this.mCategoryList[typeIndex].Tasks[i].Type);
                            }
                            else
                            {
                                genericMenu.AddItem(new GUIContent(string.Format("{0}/{1}", parentName, this.mCategoryList[typeIndex].Tasks[i].Name.ToString())), this.mCategoryList[typeIndex].Tasks[i].Type.Equals(selectedTaskType), menuFunction, this.mCategoryList[typeIndex].Tasks[i].Type);
                            }
                        }
                    }
                }
                this.AddCategoryTasksToMenu(ref genericMenu, this.mCategoryList[typeIndex].Subcategories, selectedTaskType, parentName, menuFunction);
            }
        }

        private void DrawCategory(BehaviorDesignerWindow window, CategoryList category, bool quickList)
        {
            if (category.Visible)
            {
                if (!quickList)
                {
                    category.Expanded = UnityEditor.EditorGUILayout.Foldout(category.Expanded, category.Name, BehaviorDesignerUtility.TaskFoldoutGUIStyle);
                    this.SetExpanded(category.ID, category.Expanded);
                }
                if (category.Expanded)
                {
                    if (!quickList)
                    {
                        UnityEditor.EditorGUI.indentLevel++;
                    }
                    string str = string.Empty;
                    if (category.Tasks != null)
                    {
                        for (int i = 0; i < category.Tasks.Count; i++)
                        {
                            if (category.Tasks[i].Visible)
                            {
                                if (quickList)
                                {
                                    string str2 = category.Fullpath.TrimEnd(BehaviorDesigner.Runtime.TaskUtility.TrimCharacters);
                                    if (str != str2)
                                    {
                                        char[] separator = new char[] { '/' };
                                        string[] strArray = str2.Split(separator);
                                        if (strArray.Length > 1)
                                        {
                                            GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
                                            GUILayout.Space(-2f);
                                            GUILayout.Label(strArray[strArray.Length - 1], Array.Empty<GUILayoutOption>());
                                            GUILayout.EndHorizontal();
                                        }
                                    }
                                }
                                GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
                                GUILayout.Space((float) (UnityEditor.EditorGUI.indentLevel * 0x10));
                                BehaviorDesigner.Runtime.Tasks.TaskNameAttribute[] customAttributes = null;
                                if (!this.mTaskNameAttribute.TryGetValue(category.Tasks[i].Type, out customAttributes))
                                {
                                    customAttributes = category.Tasks[i].Type.GetCustomAttributes(typeof(BehaviorDesigner.Runtime.Tasks.TaskNameAttribute), false) as BehaviorDesigner.Runtime.Tasks.TaskNameAttribute[];
                                    this.mTaskNameAttribute.Add(category.Tasks[i].Type, customAttributes);
                                }
                                string str3 = ((customAttributes == null) || (customAttributes.Length <= 0)) ? category.Tasks[i].Name : customAttributes[0].Name;
                                if (quickList && (this.mQuickIndexCount == this.mSelectedQuickIndex))
                                {
                                    GUI.backgroundColor =(new Color(1f, 0.64f, 0f));
                                    this.mSelectedQuickIndexType = category.Tasks[i].Type;
                                }
                                this.mQuickIndexCount++;
                                GUILayoutOption[] optionArray1 = new GUILayoutOption[] { GUILayout.MaxWidth((float) (((!quickList ? 300 : 200) - (UnityEditor.EditorGUI.indentLevel * 0x10)) - 0x18)) };
                                GUILayoutOption[] optionArray2 = new GUILayoutOption[] { GUILayout.MaxWidth((float) (((!quickList ? 300 : 200) - (UnityEditor.EditorGUI.indentLevel * 0x10)) - 0x18)) };
                                if (GUILayout.Button(str3, UnityEditor.EditorStyles.miniButton, optionArray2))
                                {
                                    window.AddTask(category.Tasks[i].Type, quickList);
                                }
                                GUI.backgroundColor =(Color.white);
                                GUILayout.Space(3f);
                                GUILayout.EndHorizontal();
                            }
                        }
                    }
                    if (category.Subcategories != null)
                    {
                        this.DrawCategoryTaskList(window, category.Subcategories, quickList);
                    }
                    if (!quickList)
                    {
                        UnityEditor.EditorGUI.indentLevel--;
                    }
                }
            }
        }

        private void DrawCategoryTaskList(BehaviorDesignerWindow window, List<CategoryList> categoryList, bool quickList)
        {
            for (int i = 0; i < categoryList.Count; i++)
            {
                this.DrawCategory(window, categoryList[i], quickList);
            }
        }

        public void DrawQuickTaskList(BehaviorDesignerWindow window, bool enabled)
        {
            GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
            GUI.SetNextControlName("QuickSearch");
            string str = GUILayout.TextField(this.mQuickSearchString, GUI.skin.FindStyle("ToolbarSeachTextField"), Array.Empty<GUILayoutOption>());
            if (this.mFocusQuickSearch)
            {
                GUI.FocusControl("QuickSearch");
                this.mFocusQuickSearch = false;
            }
            if (!this.mQuickSearchString.Equals(str))
            {
                this.mQuickSearchString = str;
                this.mSelectedQuickIndex = 0;
                this.Search(BehaviorDesignerUtility.SplitCamelCase(this.mQuickSearchString).ToLower().Replace(" ", string.Empty), this.mQuickCategoryList);
            }
            if (GUILayout.Button(string.Empty, !this.mSearchString.Equals(string.Empty) ? GUI.skin.FindStyle("ToolbarSeachCancelButton") : GUI.skin.FindStyle("ToolbarSeachCancelButtonEmpty"), Array.Empty<GUILayoutOption>()))
            {
                this.mQuickSearchString = string.Empty;
                this.Search(string.Empty, this.mQuickCategoryList);
                GUI.FocusControl(null);
            }
            GUILayout.EndHorizontal();
            BehaviorDesignerUtility.DrawContentSeperator(2);
            GUILayout.Space(4f);
            this.mQuickScrollPosition = GUILayout.BeginScrollView(this.mQuickScrollPosition, false, true, Array.Empty<GUILayoutOption>());
            GUI.enabled =(enabled);
            if (this.mQuickCategoryList.Count > 0)
            {
                this.mQuickIndexCount = 0;
                int num = 0;
                while (true)
                {
                    if (num >= this.mQuickCategoryList.Count)
                    {
                        if (this.mQuickIndexCount == 0)
                        {
                            GUILayout.Label("(No Tasks Found)", Array.Empty<GUILayoutOption>());
                        }
                        break;
                    }
                    this.DrawCategory(window, this.mQuickCategoryList[num], true);
                    num++;
                }
            }
            GUI.enabled =(true);
            GUILayout.EndScrollView();
        }

        public void DrawTaskList(BehaviorDesignerWindow window, bool enabled)
        {
            GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
            GUI.SetNextControlName("Search");
            string str = GUILayout.TextField(this.mSearchString, GUI.skin.FindStyle("ToolbarSeachTextField"), Array.Empty<GUILayoutOption>());
            if (this.mFocusSearch)
            {
                GUI.FocusControl("Search");
                this.mFocusSearch = false;
            }
            if (!this.mSearchString.Equals(str))
            {
                this.mSearchString = str;
                this.Search(BehaviorDesignerUtility.SplitCamelCase(this.mSearchString).ToLower().Replace(" ", string.Empty), this.mCategoryList);
            }
            if (GUILayout.Button(string.Empty, !this.mSearchString.Equals(string.Empty) ? GUI.skin.FindStyle("ToolbarSeachCancelButton") : GUI.skin.FindStyle("ToolbarSeachCancelButtonEmpty"), Array.Empty<GUILayoutOption>()))
            {
                this.mSearchString = string.Empty;
                this.Search(string.Empty, this.mCategoryList);
                GUI.FocusControl(null);
            }
            GUILayout.EndHorizontal();
            BehaviorDesignerUtility.DrawContentSeperator(2);
            GUILayout.Space(4f);
            this.mScrollPosition = GUILayout.BeginScrollView(this.mScrollPosition, Array.Empty<GUILayoutOption>());
            GUI.enabled =(enabled);
            if (this.mCategoryList.Count > 1)
            {
                this.DrawCategory(window, this.mCategoryList[1], false);
            }
            if (this.mCategoryList.Count > 3)
            {
                this.DrawCategory(window, this.mCategoryList[3], false);
            }
            if (this.mCategoryList.Count > 0)
            {
                this.DrawCategory(window, this.mCategoryList[0], false);
            }
            if (this.mCategoryList.Count > 2)
            {
                this.DrawCategory(window, this.mCategoryList[2], false);
            }
            GUI.enabled =(true);
            GUILayout.EndScrollView();
        }

        public void FocusSearchField(bool quickTaskList, bool clearQuickSearchString)
        {
            if (!quickTaskList)
            {
                this.mFocusSearch = true;
            }
            else
            {
                this.mFocusQuickSearch = true;
                if (clearQuickSearchString)
                {
                    this.mQuickSearchString = string.Empty;
                    this.mSelectedQuickIndex = 0;
                    this.mSelectedQuickIndexType = null;
                    this.Search(string.Empty, this.mQuickCategoryList);
                }
            }
        }

        public void Init()
        {
            this.mCategoryList = new List<CategoryList>();
            this.mQuickCategoryList = new List<CategoryList>();
            List<System.Type> list = new List<System.Type>();
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (int i = 0; i < assemblies.Length; i++)
            {
                try
                {
                    System.Type[] types = assemblies[i].GetTypes();
                    for (int j = 0; j < types.Length; j++)
                    {
                        if (typeof(BehaviorDesigner.Runtime.Tasks.Task).IsAssignableFrom(types[j]) && (!types[j].Equals(typeof(BehaviorDesigner.Runtime.Tasks.BehaviorReference)) && (!types[j].IsAbstract && (types[j].IsSubclassOf(typeof(BehaviorDesigner.Runtime.Tasks.Action)) || (types[j].IsSubclassOf(typeof(BehaviorDesigner.Runtime.Tasks.Composite)) || (types[j].IsSubclassOf(typeof(BehaviorDesigner.Runtime.Tasks.Conditional)) || types[j].IsSubclassOf(typeof(BehaviorDesigner.Runtime.Tasks.Decorator))))))))
                        {
                            list.Add(types[j]);
                        }
                    }
                }
                catch (Exception)
                {
                }
            }
            list.Sort(new AlphanumComparator<System.Type>());
            Dictionary<string, CategoryList> dictionary = new Dictionary<string, CategoryList>();
            Dictionary<string, CategoryList> dictionary2 = new Dictionary<string, CategoryList>();
            string str = string.Empty;
            BehaviorDesigner.Runtime.Tasks.TaskCategoryAttribute[] attributeArray = null;
            int id = 0;
            int num4 = 0;
            while (num4 < list.Count)
            {
                str = !list[num4].IsSubclassOf(typeof(BehaviorDesigner.Runtime.Tasks.Action)) ? (!list[num4].IsSubclassOf(typeof(BehaviorDesigner.Runtime.Tasks.Composite)) ? (!list[num4].IsSubclassOf(typeof(BehaviorDesigner.Runtime.Tasks.Conditional)) ? "Decorators" : "Conditionals") : "Composites") : "Actions";
                if ((attributeArray = list[num4].GetCustomAttributes(typeof(BehaviorDesigner.Runtime.Tasks.TaskCategoryAttribute), true) as BehaviorDesigner.Runtime.Tasks.TaskCategoryAttribute[]).Length > 0)
                {
                    str = str + "/" + attributeArray[0].Category.TrimEnd(BehaviorDesigner.Runtime.TaskUtility.TrimCharacters);
                }
                string key = string.Empty;
                CategoryList item = null;
                CategoryList list3 = null;
                char[] separator = new char[] { '/' };
                string[] strArray = str.Split(separator);
                CategoryList list4 = null;
                CategoryList list5 = null;
                int index = 0;
                while (true)
                {
                    if (index >= strArray.Length)
                    {
                        dictionary[key].AddTask(list[num4]);
                        dictionary2[key].AddTask(list[num4]);
                        num4++;
                        break;
                    }
                    if (index > 0)
                    {
                        key = key + "/";
                    }
                    key = key + strArray[index];
                    if (dictionary.ContainsKey(key))
                    {
                        item = dictionary[key];
                        list3 = dictionary2[key];
                    }
                    else
                    {
                        bool expanded = this.PreviouslyExpanded(id);
                        item = new CategoryList(strArray[index], key, expanded, id++);
                        list3 = new CategoryList(strArray[index], key, true, 0);
                        if (list4 == null)
                        {
                            this.mCategoryList.Add(item);
                            this.mQuickCategoryList.Add(list3);
                        }
                        else
                        {
                            list4.AddSubcategory(item);
                            list5.AddSubcategory(list3);
                        }
                        dictionary.Add(key, item);
                        dictionary2.Add(key, list3);
                    }
                    list4 = item;
                    list5 = list3;
                    index++;
                }
            }
            this.Search(BehaviorDesignerUtility.SplitCamelCase(this.mSearchString).ToLower().Replace(" ", string.Empty), this.mCategoryList);
            this.Search(BehaviorDesignerUtility.SplitCamelCase(this.mQuickSearchString).ToLower().Replace(" ", string.Empty), this.mQuickCategoryList);
        }

        private void MarkVisible(List<CategoryList> categoryList)
        {
            for (int i = 0; i < categoryList.Count; i++)
            {
                categoryList[i].Visible = true;
                if (categoryList[i].Subcategories != null)
                {
                    this.MarkVisible(categoryList[i].Subcategories);
                }
                if (categoryList[i].Tasks != null)
                {
                    for (int j = 0; j < categoryList[i].Tasks.Count; j++)
                    {
                        categoryList[i].Tasks[j].Visible = true;
                    }
                }
            }
        }

        public void MoveSelectedQuickTask(bool increase)
        {
            this.mSelectedQuickIndex = Mathf.Min(Mathf.Max(0, this.mSelectedQuickIndex + (!increase ? -1 : 1)), this.mQuickIndexCount);
        }

        public void OnEnable()
        {
            base.hideFlags = (HideFlags)(0x3d);
        }

        private bool PreviouslyExpanded(int id)
        {
            return UnityEditor.EditorPrefs.GetBool(string.Format("BehaviorDesignerTaskList{0}", id), true);
        }

        private bool Search(string searchString, List<CategoryList> categoryList)
        {
            bool flag = searchString.Equals(string.Empty);
            for (int i = 0; i < categoryList.Count; i++)
            {
                bool flag2 = false;
                categoryList[i].Visible = false;
                if ((categoryList[i].Subcategories != null) && this.Search(searchString, categoryList[i].Subcategories))
                {
                    categoryList[i].Visible = true;
                    flag = true;
                }
                if (BehaviorDesignerUtility.SplitCamelCase(categoryList[i].Name).ToLower().Replace(" ", string.Empty).Contains(searchString))
                {
                    flag = true;
                    flag2 = true;
                    categoryList[i].Visible = true;
                    if (categoryList[i].Subcategories != null)
                    {
                        this.MarkVisible(categoryList[i].Subcategories);
                    }
                }
                if (categoryList[i].Tasks != null)
                {
                    for (int j = 0; j < categoryList[i].Tasks.Count; j++)
                    {
                        categoryList[i].Tasks[j].Visible = searchString.Equals(string.Empty);
                        if (flag2 || categoryList[i].Tasks[j].Name.ToLower().Replace(" ", string.Empty).Contains(searchString))
                        {
                            categoryList[i].Tasks[j].Visible = true;
                            flag = true;
                            categoryList[i].Visible = true;
                        }
                    }
                }
            }
            return flag;
        }

        public void SelectQuickTask(BehaviorDesignerWindow window)
        {
            if (this.mSelectedQuickIndexType != null)
            {
                window.AddTask(this.mSelectedQuickIndexType, true);
            }
        }

        private void SetExpanded(int id, bool visible)
        {
            UnityEditor.EditorPrefs.SetBool(string.Format("BehaviorDesignerTaskList{0}", id), visible);
        }

        public void UpdateQuickTaskSearch()
        {
            this.Search(BehaviorDesignerUtility.SplitCamelCase(this.mQuickSearchString).ToLower().Replace(" ", string.Empty), this.mQuickCategoryList);
        }

        private class CategoryList
        {
            private string mName = string.Empty;
            private string mFullpath = string.Empty;
            private List<TaskList.CategoryList> mSubcategories;
            private List<TaskList.SearchableType> mTasks;
            private bool mExpanded = true;
            private bool mVisible = true;
            private int mID;

            public CategoryList(string name, string fullpath, bool expanded, int id)
            {
                this.mName = name;
                this.mFullpath = fullpath;
                this.mExpanded = expanded;
                this.mID = id;
            }

            public void AddSubcategory(TaskList.CategoryList category)
            {
                if (this.mSubcategories == null)
                {
                    this.mSubcategories = new List<TaskList.CategoryList>();
                }
                this.mSubcategories.Add(category);
            }

            public void AddTask(System.Type taskType)
            {
                if (this.mTasks == null)
                {
                    this.mTasks = new List<TaskList.SearchableType>();
                }
                this.mTasks.Add(new TaskList.SearchableType(taskType));
            }

            public string Name
            {
                get
                {
                    return this.mName;
                }
            }

            public string Fullpath
            {
                get
                {
                    return this.mFullpath;
                }
            }

            public List<TaskList.CategoryList> Subcategories
            {
                get
                {
                    return this.mSubcategories;
                }
            }

            public List<TaskList.SearchableType> Tasks
            {
                get
                {
                    return this.mTasks;
                }
            }

            public bool Expanded
            {
                get
                {
                    return this.mExpanded;
                }
                set
                {
                    this.mExpanded = value;
                }
            }

            public bool Visible
            {
                get
                {
                    return this.mVisible;
                }
                set
                {
                    this.mVisible = value;
                }
            }

            public int ID
            {
                get
                {
                    return this.mID;
                }
            }
        }

        private class SearchableType
        {
            private System.Type mType;
            private bool mVisible = true;
            private string mName;

            public SearchableType(System.Type type)
            {
                this.mType = type;
                this.mName = BehaviorDesignerUtility.SplitCamelCase(this.mType.Name);
            }

            public System.Type Type
            {
                get
                {
                    return this.mType;
                }
            }

            public bool Visible
            {
                get
                {
                    return this.mVisible;
                }
                set
                {
                    this.mVisible = value;
                }
            }

            public string Name
            {
                get
                {
                    return this.mName;
                }
            }
        }

        public enum TaskTypes
        {
            Action,
            Composite,
            Conditional,
            Decorator,
            Last
        }
    }
}

