using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System;
using Object = UnityEngine.Object;

[InitializeOnLoad]
public class ModuleWindow : EditorWindow
{
    public enum Location
    {
        Framework,
        Game,
        LauncherFramework,
        LauncherGame,
    }

    static Dictionary<Location, string> locationToPathDic = new Dictionary<Location, string>();

    static ModuleWindow()
    {
        locationToPathDic[Location.Framework] = "Assets/$Module/Framework";
        locationToPathDic[Location.Game] = "Assets/$Module/Game";
        locationToPathDic[Location.LauncherFramework] = "Assets/LauncherAssembly/Module/Framework";
        locationToPathDic[Location.LauncherGame] = "Assets/LauncherAssembly/Module/Game";

        EditorApplication.projectWindowItemOnGUI += OnProjectWindowItemGUI;
    }

    static string GetLocationDir(Location location)
    {
        return locationToPathDic[location];
    }

    static Texture2D _icon_location;
    static Texture2D Icon_Location
    {
        get
        {
            if (_icon_location == null)
            {
                var guidList = AssetDatabase.FindAssets("module_location_icon");
                if (guidList.Length > 0)
                {
                    var guid = guidList[0];
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    _icon_location = AssetDatabase.LoadAllAssetsAtPath(path)[0] as Texture2D;
                }
            }
            return _icon_location;
        }
    }

    static Texture2D _icon_module;
    static Texture2D Icon_Module
    {
        get
        {
            if(_icon_module == null)
            {
                var guidList = AssetDatabase.FindAssets("module_icon");
                if(guidList.Length > 0)
                {
                    var guid = guidList[0];
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    _icon_module = AssetDatabase.LoadAllAssetsAtPath(path)[0] as Texture2D;
                }
            }
            return _icon_module;
        }
    }

    static HashSet<string> _locationPathSet;
    static HashSet<string> LocationPathSet
    {
        get
        {
            if(_locationPathSet == null)
            {
                _locationPathSet = new HashSet<string>();
                foreach(Location location in Enum.GetValues(typeof(Location)))
                {
                    var path = GetLocationDir(location);
                    _locationPathSet.Add(path);
                }
            }
            return _locationPathSet;
        }
    }


    public static List<string> ModulePathList
    {
        get
        {
            var list = new List<string>();
            var locationSet = LocationPathSet;
            foreach(var location in locationSet)
            {
                if(!Directory.Exists(location))
                {
                    continue;
                }
                var moduleList = Directory.GetDirectories(location, "*", SearchOption.TopDirectoryOnly);
                list.AddRange(moduleList);
            }
            return list;
        }
    }

    private static void OnProjectWindowItemGUI(string guid, Rect rect)
    {
        var path = AssetDatabase.GUIDToAssetPath(guid);
        if (!AssetDatabase.IsValidFolder(path)) return;

        // 指定要更改图标的文件夹名称
        if (LocationPathSet.Contains(path))
        {
            // 更改为要使用的自定义图标的路径
            var icon = Icon_Location;
          
            // rect 是从图标开始到右边所有区域
            if (icon != null)
            {
                rect.x += rect.width - 18;
                rect.width = 16;
                rect.y += 2;
                rect.height = 16;

                GUI.DrawTexture(rect, icon);
                
            }
        }
        else
        {
            var parent = Path.GetDirectoryName(path).Replace("\\", "/");
            if(LocationPathSet.Contains(parent))
            {
                // 更改为要使用的自定义图标的路径
                var icon = Icon_Module;

                // rect 是从图标开始到右边所有区域
                if (icon != null)
                {
                    rect.x += rect.width - 18;
                    rect.width = 16;
                    rect.y += 2;
                    rect.height = 16;

                    GUI.DrawTexture(rect, icon);

                }
            }
        }
    }


    [MenuItem("pzy.com.*/Module/Window")]
    static void ShowWinsow()
    {
        ModuleWindow window = (ModuleWindow)EditorWindow.GetWindow(typeof(ModuleWindow));
        window.Show();
    }

    [MenuItem("Assets/Move To Module/Game", false, 1001)]
    static void Menu_MoveToGame()
    {
        SelectionMoveFolder(Location.Game);
    }


    [MenuItem("Assets/Move To Module/Framework", false, 1002)]
    static void Menu_MoveToFramework()
    {
        SelectionMoveFolder(Location.Framework);
    }


    [MenuItem("Assets/Move To Module/LauncherGame", false, 2001)]
    static void Menu_MoveToLauncherGame()
    {
        SelectionMoveFolder(Location.LauncherGame);
    }


    [MenuItem("Assets/Move To Module/LauncherFramework", false, 2002)]
    static void Menu_MoveToLauncherFramework()
    {
        SelectionMoveFolder(Location.LauncherFramework);
    }

    private static int GetPathDepth(string path)
    {
        return path.Split('/').Length;
    }
    static int GetMinDepthInSelection()
    {
        Object[] selectedObjects = Selection.objects;
        int minDepth = int.MaxValue;
        foreach (Object obj in selectedObjects)
        {
            string sourcePath = AssetDatabase.GetAssetPath(obj);
            int depth = GetPathDepth(sourcePath);
            if (depth < minDepth)
            {
                minDepth = depth;
            }
        }
        return minDepth;
    }

    static void CreateFoldAssetsIfNeed(string path)
    {
        if (!AssetDatabase.IsValidFolder(path))
        {
            Directory.CreateDirectory(path);
            AssetDatabase.Refresh();
        }
    }

    /// <summary>
    /// 把选中的文件夹移动到模块目录
    /// </summary>
    /// <param name="category">模块分类</param>
    private static void SelectionMoveFolder(Location location)
    {
        var minDepth = GetMinDepthInSelection();

        Object[] selectedObjects = Selection.objects;
        foreach (Object obj in selectedObjects)
        {
            string sourcePath = AssetDatabase.GetAssetPath(obj);
            if (AssetDatabase.IsValidFolder(sourcePath))
            {
                int depth = GetPathDepth(sourcePath);

                // 如果当前文件夹是层级最高的文件夹，则移动它和它的子文件夹
                if (depth == minDepth)
                {
                    var dir = GetLocationDir(location);
                    string targetPath = dir + "/" + obj.name;
                    var parentDir = Path.GetDirectoryName(targetPath);
                    CreateFoldAssetsIfNeed(parentDir);
                    var errorMsg = AssetDatabase.MoveAsset(sourcePath, targetPath);
                    if(errorMsg == "")
                    {
                        Debug.Log($"{sourcePath} -> {targetPath}");
                    }
                    else
                    {
                        Debug.LogError(errorMsg);
                    }
                }
            }
        }
    }

    private Dictionary<Location, List<string>> locationToModuleList = new Dictionary<Location, List<string>>();
    private Dictionary<string, bool> categoryFoldouts = new Dictionary<string, bool>();
    
    void RecreateData()
    {
        locationToModuleList.Clear();
        int moduleCount = 0;
        var locationList = Enum.GetValues(typeof(Location));
        foreach(Location location in locationList)
        {
            var dir = GetLocationDir(location);
            if (!Directory.Exists(dir))
            {
                continue;
            }
            List<string> moduleList = new List<string>();

            // 获取此分类文件夹下的所有子文件夹

            string[] childDirList = Directory.GetDirectories(dir);

            // 将所有子文件夹添加到列表中
            foreach (string childDir in childDirList)
            {
                string folderName = Path.GetFileName(childDir);
                moduleList.Add(folderName);
            }

            // 将此分类和对应的文件夹列表添加到字典中
            if (moduleList.Count > 0)
            {
                locationToModuleList[location] = moduleList;
                moduleCount += moduleList.Count;
            }
        }

        Debug.Log($"[ModuleWindow] {moduleCount} module(s) found");
    }

    void OnEnable()
    {
        this.RecreateData();
    }

    void OnGUI()
    {
        this.DrawRefreshButton();
        GUILayout.Label("Module Folders", EditorStyles.boldLabel);

        // 如果没有分类文件夹，显示“未找到”消息
        if (locationToModuleList.Count == 0)
        {
            GUILayout.Label("No module folders found.");
        }

        // 其他情况下，显示分类和文件夹列表
        foreach (var kv in locationToModuleList)
        {
            var category = kv.Key;
            var moduelList = kv.Value;
            DrawLocation(category, moduelList);
        }
    }

    void DrawRefreshButton()
    {
        if(GUILayout.Button("Refresh"))
        {
            this.RecreateData();
        }
    }

    bool IsFoldout(string catogory)
    {
        if(!categoryFoldouts.ContainsKey(catogory))
        {
            return false;
        }

        return categoryFoldouts[catogory];
    }

    void SetIsFoldout(string catogory, bool b)
    {
        categoryFoldouts[catogory] = b;
    }

    /// <summary>
    /// 绘制卷轴，如果没有子项目，则绘制成标签, 返回是否展开
    /// </summary>
    /// <param name="name">文字</param>
    /// <param name="subItemList">子项目列表，用于检测数目</param>
    /// <param name="keyToFoldoutDic">展开记录字典</param>
    /// <param name="foldoutKey">展开的 key</param>
    static bool DrawFoldoutOrLabel(string name, IList subItemList, Dictionary<string, bool> keyToFoldoutDic, string foldoutKey)
    {
        // 绘制分类名称，并将其作为可折叠控件
        if (subItemList.Count > 0)
        {
            GUIStyle style = new GUIStyle(EditorStyles.foldout);
            var isFoldout = keyToFoldoutDic.TryGet(foldoutKey, false);
            var b = EditorGUILayout.Foldout(isFoldout, name, style);
            keyToFoldoutDic[foldoutKey] = b;
            return b;
        }
        else
        {
            GUILayout.Label(name, EditorStyles.label);
            return false;
        }
    }

    /// <summary>
    /// 在资产或者打开场景中选中资产或者游戏对象
    /// </summary>
    /// <param name="asset"></param>
    static void SelectAsset(Object asset)
    {
        ProjectWindowUtil.ShowCreatedAsset(asset);
        EditorGUIUtility.PingObject(asset);
    }

    static string GetModulePath(Location location, string module)
    {
        var dir = GetLocationDir(location);
        var path = $"{dir}/{module}";
        return path;
    }

    void DrawModuleListInLocation(Location location, List<string> moduleList)
    {
        // 遍历所有模块文件夹并显示名称
        foreach (string module in moduleList)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(EditorGUI.indentLevel * 15);
            if (GUILayout.Button(module, EditorStyles.label))
            {
                var path = GetModulePath(location, module);
                // 获取对应的资产对象
                var asset = AssetDatabase.LoadAssetAtPath(path, typeof(Object));
                SelectAsset(asset);
            }

            GUILayout.FlexibleSpace();
            // 绘制移动到按钮
            if (GUILayout.Button("移动文件到"))
            {
                var dir = GetModulePath(location, module);
                MoveSelectedAssets(dir);
            }

            GUILayout.EndHorizontal();
        }
    }

    /// <summary>
    ///  把选中的资产移动到指定目录中
    /// </summary>
    /// <param name="targetDirPath"></param>
    static void MoveSelectedAssets(string targetDirPath)
    {
        // 获取选中的资产
        Object[] selectedAssets = Selection.GetFiltered(typeof(Object), SelectionMode.Assets);

        // 移动每个选定的资产到目标文件夹中
        foreach (Object asset in selectedAssets)
        {
            string assetPath = AssetDatabase.GetAssetPath(asset);
            string assetName = Path.GetFileName(assetPath);
            string targetPath = Path.Combine(targetDirPath, assetName);

            AssetDatabase.MoveAsset(assetPath, targetPath);
            Debug.Log($"{assetPath} -> {targetPath}");
        }

        // 刷新 AssetDatabase
        AssetDatabase.Refresh();
    }

    void DrawLocation(Location location, List<string> moduleList)
    {
        // 绘制分类名称，并将其作为可折叠控件
        var drawMsg = $"{location} ({moduleList.Count} modules)";
        var isFoldout = DrawFoldoutOrLabel(drawMsg, moduleList, categoryFoldouts, location.ToString());


        // 只有在展开时才绘制该分类下的模块文件夹列表
        if (isFoldout)
        {
            EditorGUI.indentLevel++;

            this.DrawModuleListInLocation(location, moduleList);

            EditorGUI.indentLevel--;
        }
    }


}
