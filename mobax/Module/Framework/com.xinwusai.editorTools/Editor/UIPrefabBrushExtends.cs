using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

/// <summary>
///  用来做prefab的扩展工具
/// </summary>
public class UIPrefabBrushExtends
{
    static List<string> withoutExtensions = new List<string>() {".prefab"};
    private static int _curTime;

    static void UIPrefabBrush()
    {
        EditorSettings.serializationMode = SerializationMode.ForceText;
        string path = "";
        var choose = Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.Assets);
        FindAllPrefab(path, choose, 0);
    }
    
    [MenuItem("Assets/BrushTools/一键刷新九宫格预制体")]
    static void FindAllPrefabs()
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
                GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(CommonHelper.GetRelativeAssetsPath(file));
                Debug.Log(string.Format("<color=#6495ED>{0}</color>", file) + "引用到该资源", go);
                // 刷新当前prefab内存在该图片的image
                if (go == null) return;
                var imageList = go.GetComponentsInChildren<Image>(true);
                var textureName = path.Split('/').Last().Split('.').First();
                //替换sprite引用，并修正填充方式
                foreach (Image image in imageList)
                {
                    if (image == null || image.sprite == null || image.sprite.texture.name != textureName) continue;
                    var targetSprite = image.sprite;
                    if (image.type == Image.Type.Sliced) continue;
                    if (targetSprite.border.x + targetSprite.border.y + targetSprite.border.z +
                        targetSprite.border.w > 0)
                    {
                        if (image.type == Image.Type.Simple)
                        {
                            image.type = Image.Type.Sliced;
                        }
                    }
                    else
                    {
                        if (image.type == Image.Type.Sliced)
                        {
                            image.type = Image.Type.Simple;
                        }
                    }

                    Debug.Log(string.Format("<color=#3CB371>{0}</color>", file) + "已被正常修改", go);
                    EditorUtility.SetDirty(go);
                }
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

                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    Debug.Log(string.Format("<color=#3CB371>{0}</color>",
                        $"Search Finished, Total Time Consuming ---> {timeStr}"));
                    return;
                }

                FindNext(path, choose, parseDic, next);
            }
        };
    }

    static void FindAllPrefab(string path, Object[] choose, int index)
    {
        var obj = choose[index];
        path = AssetDatabase.GetAssetPath(obj);
        if (string.IsNullOrEmpty(path)) return;
        string[] exDirectories = new string[] {"Arts"};
        Debug.Log("开始查找引用资源:" + path + "的<<Prefab>>资源");
        string guid = AssetDatabase.AssetPathToGUID(path);
        //string guid = FindReferences.GetFileID(Selection.activeObject).ToString();
        // Debug.Log("guid : " + guid);
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
            // 解析file获取他的上层
            var parentPaths = path.Split('/');
            parentPaths = parentPaths.Take(parentPaths.Length - 1).ToArray();
            var parentPath = parentPaths.Last().Split('.').First();
            bool isCancel = EditorUtility.DisplayCancelableProgressBar($"匹配资源中:{index + 1}/{choose.Length}", file,
                (float) startIndex / (float) files.Length);

            // 这里需要判断是否是自己的父文件夹
            if (Regex.IsMatch(File.ReadAllText(file), guid))
            {
                var self = file.Split('\\').Last().Split('.').First();
                if (string.IsNullOrEmpty(parentPath) || self != parentPath)
                {
                    GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(CommonHelper.GetRelativeAssetsPath(file));
                    Debug.Log(string.Format("<color=#6495ED>{0}</color>", file) + "引用到该资源", go);
                    // 刷新当前prefab内存在该图片的image
                    if (go == null) return;
                    var imageList = go.GetComponentsInChildren<Image>(true);
                    var textureName = path.Split('/').Last().Split('.').First();
                    //替换sprite引用，并修正填充方式
                    foreach (Image image in imageList)
                    {
                        if (image == null || image.sprite == null || image.sprite.texture.name != textureName) continue;
                        var targetSprite = image.sprite;
                        if (image.type == Image.Type.Sliced) continue;
                        if (targetSprite.border.x + targetSprite.border.y + targetSprite.border.z +
                            targetSprite.border.w > 0)
                        {
                            if (image.type == Image.Type.Simple)
                            {
                                image.type = Image.Type.Sliced;
                            }
                        }
                        else
                        {
                            if (image.type == Image.Type.Sliced)
                            {
                                image.type = Image.Type.Simple;
                            }
                        }

                        Debug.Log(string.Format("<color=#3CB371>{0}</color>", file) + "已被正常修改", go);
                        EditorUtility.SetDirty(go);
                    }
                }
            }

            startIndex++;
            if (isCancel || startIndex >= files.Length)
            {
                EditorUtility.ClearProgressBar();
                EditorApplication.update = null;
                startIndex = 0;
                var next = index + 1;
                if (next > choose.Length - 1)
                {
                    Debug.Log("已查找结束");
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    return;
                }

                FindAllPrefab(path, choose, next);
            }
        };
    }

    /// <summary>
    /// 刷新用到过该图片的prefab变成slice
    /// </summary>
    static void BrushPrefabSprite2Slice()
    {
    }
}