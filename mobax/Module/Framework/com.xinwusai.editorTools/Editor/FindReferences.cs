using System;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Reflection;
using Object = UnityEngine.Object;

//TODO: 可以扩展之后使用一键刷子工具
public static class FindReferences
{
    private static PropertyInfo inspectorMode =
        typeof(SerializedObject).GetProperty("inspectorMode", BindingFlags.NonPublic | BindingFlags.Instance);

    private static int _curTime;

    public static long GetFileID(this Object target)
    {
        SerializedObject serializedObject = new SerializedObject(target);
        inspectorMode.SetValue(serializedObject, InspectorMode.Debug, null);
        SerializedProperty localIdProp = serializedObject.FindProperty("m_LocalIdentfierInFile");
        return localIdProp.longValue;
    }

    /// <summary>
    /// 找到所有的废弃引用
    /// </summary>
    [MenuItem("Assets/BrushTools/查找UI资源引用并清理", false)]
    static void FindAll()
    {
        EditorSettings.serializationMode = SerializationMode.ForceText;
        string path = "";
        _curTime = (int) (EditorApplication.timeSinceStartup);
        // 查找文件的所有guid的map合集
        // key = file的path, value = file所有的guid的数组存储
        var parseDic = new Dictionary<string, string>();
        EditorSettings.serializationMode = SerializationMode.ForceText;

        // 自定义需要过滤的资源类型和资源后缀列表
        string[] filterTypes = new string[] {"t:Prefab", "t:Material", "t:Asset"};
        // string[] filterExtensions = new string[] {".png", ".jpg", ".jpeg"}; // 以 .png、.jpg、.jpeg 结尾的图片资源

        // string searchFilter = string.Join(" ", filterTypes) + " " + string.Join(" ", filterExtensions);
        string searchFilter = string.Join(" ", filterTypes);
        List<string> guids = AssetDatabase.FindAssets(searchFilter).ToList();

        // 自定义需要过滤的目录名称列表
        string[] filterDirectories = new string[] {"Arts/", "Editor/"};
        guids = guids.FindAll(val =>
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(val);
            // 判断文件路径是否包含需要过滤的目录名称，如果包含则跳过该文件
            bool shouldFilter = false;
            foreach (string directory in filterDirectories)
            {
                if (assetPath.Contains(directory))
                {
                    shouldFilter = true;
                    break;
                }
            }

            return !shouldFilter;
        });

        int startIndex = 0;
        EditorApplication.update = delegate()
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guids[startIndex]);

            // string file = guids[startIndex];
            bool isCancel = EditorUtility.DisplayCancelableProgressBar($"解析资源中:{startIndex + 1}/{guids.Count}",
                assetPath,
                (float) startIndex / (float) guids.Count);
            parseDic.Add(assetPath, File.ReadAllText(assetPath));

            startIndex++;
            if (isCancel)
            {
                EditorUtility.ClearProgressBar();
                EditorApplication.update = null;
                startIndex = 0;
                parseDic.Clear();
            }

            if (startIndex >= guids.Count)
            {
                EditorUtility.ClearProgressBar();
                EditorApplication.update = null;
                startIndex = 0;

                // 执行下一个
                var choose = Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.Assets);
                FindNext(path, choose, parseDic, 0);
            }
        };
    }

    /// <summary>
    /// 找到所有的废弃引用
    /// </summary>
    [MenuItem("Assets/BrushTools/查找Art资源引用并清理", false)]
    static void FindAllArt()
    {
        EditorSettings.serializationMode = SerializationMode.ForceText;
        string path = "";
        _curTime = (int)(EditorApplication.timeSinceStartup);
        // 查找文件的所有guid的map合集
        // key = file的path, value = file所有的guid的数组存储
        var parseDic = new Dictionary<string, string>();
        EditorSettings.serializationMode = SerializationMode.ForceText;

        // 自定义需要过滤的资源类型和资源后缀列表
        string[] filterTypes = new string[] { "t:Prefab", "t:Material", "t:Asset" };
        // string[] filterExtensions = new string[] {".png", ".jpg", ".jpeg"}; // 以 .png、.jpg、.jpeg 结尾的图片资源

        // string searchFilter = string.Join(" ", filterTypes) + " " + string.Join(" ", filterExtensions);
        string searchFilter = string.Join(" ", filterTypes);
        List<string> guids = AssetDatabase.FindAssets(searchFilter).ToList();

        // 自定义需要过滤的目录名称列表
        string[] selectedDirectories = new string[] { "Arts/Env", "Arts/Plots", "Arts/Roguelike", "Arts/LevelScene" };
        guids = guids.FindAll(val =>
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(val);
            // 判断文件路径是否包含需要过滤的目录名称，如果包含则跳过该文件
            bool shouldSelect = false;
            foreach (string directory in selectedDirectories)
            {
                if (assetPath.Contains(directory))
                {
                    shouldSelect = true;
                    break;
                }
            }
            return shouldSelect;
        });

        int startIndex = 0;
        EditorApplication.update = delegate ()
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guids[startIndex]);

            // string file = guids[startIndex];
            bool isCancel = EditorUtility.DisplayCancelableProgressBar($"解析资源中:{startIndex + 1}/{guids.Count}",
                assetPath,
                (float)startIndex / (float)guids.Count);
            parseDic.Add(assetPath, File.ReadAllText(assetPath));

            startIndex++;
            if (isCancel)
            {
                EditorUtility.ClearProgressBar();
                EditorApplication.update = null;
                startIndex = 0;
                parseDic.Clear();
            }

            if (startIndex >= guids.Count)
            {
                EditorUtility.ClearProgressBar();
                EditorApplication.update = null;
                startIndex = 0;

                // 执行下一个
                var choose = Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.Assets);
                FindNext(path, choose, parseDic, 0);
            }
        };
    }

    static void FindNext(string path, Object[] choose, Dictionary<string, string> parseDic, int index)
    {
        var obj = choose[index];
        path = AssetDatabase.GetAssetPath(obj);
        if (string.IsNullOrEmpty(path)) return;
        Debug.Log("开始查找哪里引用到资源:" + path);
        string guid = AssetDatabase.AssetPathToGUID(path);
        int startIndex = 0;
        List<string> usedTemp = new List<string>();
        var fileKeys = parseDic.Keys.ToList();
        EditorApplication.update = delegate()
        {
            string file = fileKeys[startIndex];
            bool isCancel = EditorUtility.DisplayCancelableProgressBar($"匹配资源中:{index + 1}/{choose.Length}", file,
                (float) startIndex / (float) fileKeys.Count);
            // 这里需要判断是否是自己的父文件夹
            if (Regex.IsMatch(parseDic[file], guid))
            {
                Debug.LogError("检测到:"+path+" => "+ file);
                usedTemp.Add(file);
            }

            startIndex++;
            if (isCancel)
            {
                EditorUtility.ClearProgressBar();
                EditorApplication.update = null;
                startIndex = 0;
            }

            if (startIndex >= fileKeys.Count)
            {
                EditorUtility.ClearProgressBar();
                EditorApplication.update = null;
                startIndex = 0;
                if (usedTemp.Count <= 0)
                {
                    var result = AssetDatabase.DeleteAsset(path);
                    if (result)
                    {
                        Debug.Log(string.Format("<color=#F08080>{0}</color>", path) + "已删除成功~ 请确认是否代码内无任何引用");
                    }
                }

                var next = index + 1;
                if (next > choose.Length - 1)
                {
                    var tm = (int) (EditorApplication.timeSinceStartup) - _curTime;
                    var timeSpan = new TimeSpan(tm * TimeSpan.TicksPerSecond);
                    var timeStr = "";
                    if (timeSpan.Hours > 0)
                    {
                        timeStr = $"{timeSpan.Hours:00}时{timeSpan.Minutes:00}分{timeSpan.Seconds:00}秒";
                    }
                    else if (timeSpan.Minutes > 0)
                    {
                        timeStr = $"{timeSpan.Minutes:00}分{timeSpan.Seconds:00}秒";
                    }
                    else
                    {
                        timeStr = $"{timeSpan.Seconds:00}秒";
                    }

                    Debug.Log(string.Format("<color=#3CB371>{0}</color>",
                        $"Search Finished, Total Time Consuming ---> {timeStr}"));
                    return;
                }

                FindNext(path, choose, parseDic, next);
            }
        };
    }

    [MenuItem("Assets/BrushTools/查找资源引用", false)]
    static void Find()
    {
        string[] exDirectories = new string[] {"Arts"};
        EditorSettings.serializationMode = SerializationMode.ForceText;
        string path = AssetDatabase.GetAssetPath(Selection.activeObject);
        if (!string.IsNullOrEmpty(path))
        {
            Debug.Log("开始查找哪里引用到资源:" + path);
            string guid = AssetDatabase.AssetPathToGUID(path);
            //string guid = FindReferences.GetFileID(Selection.activeObject).ToString();
            // Debug.Log("guid : " + guid);
            List<string> withoutExtensions = new List<string>() {".prefab", ".unity", ".mat"};
            var temp = Directory.GetFiles(Application.dataPath, "*.*", SearchOption.AllDirectories);
            string[] files = temp.ToList().FindAll(val =>
            {
                var v = val.Split('\\');
                return v.ToList().Find(v => exDirectories.ToList().IndexOf(v) >= 0) == null;
            }).Where(s => withoutExtensions.Contains(Path.GetExtension(s).ToLower())).ToArray();

            int startIndex = 0;
            EditorApplication.update = delegate()
            {
                string file = files[startIndex];
                bool isCancel =
                    EditorUtility.DisplayCancelableProgressBar($"匹配资源中:{startIndex + 1}/{files.Length}", file,
                        (float) startIndex / (float) files.Length);

                if (Regex.IsMatch(File.ReadAllText(file), guid))
                {
                    Object find_obj = AssetDatabase.LoadAssetAtPath<Object>(CommonHelper.GetRelativeAssetsPath(file));
                    Debug.Log(file + "引用到该资源", find_obj);
                    string extension = Path.GetExtension(file);
                    // Debug.Log("extension "+extension);
                    if (extension == ".prefab")
                    {
                        int select_index =
                            EditorUtility.DisplayDialogComplex("找到了", file + "引用到该资源", "关闭", "继续查找", "打开界面");
                        // Debug.Log("select index "+select_index);
                        isCancel = (select_index == 0 || select_index == 2);
                        if (select_index == 2)
                        {
                            Debug.Log($"{file}");
                            // U3DExtends.UIEditorHelper.LoadLayoutByPath(file);
                        }
                    }
                    else
                    {
                        isCancel = EditorUtility.DisplayDialog("找到了", file + "引用到该资源", "关闭", "继续查找");
                    }
                }

                startIndex++;
                if (isCancel || startIndex >= files.Length)
                {
                    EditorUtility.ClearProgressBar();
                    EditorApplication.update = null;
                    startIndex = 0;
                    Debug.Log("匹配结束");
                }
            };
        }
    }

    [MenuItem("Assets/BrushTools/查找资源引用", true)]
    static private bool VFind()
    {
        string path = AssetDatabase.GetAssetPath(Selection.activeObject);
        return (!string.IsNullOrEmpty(path));
    }
}