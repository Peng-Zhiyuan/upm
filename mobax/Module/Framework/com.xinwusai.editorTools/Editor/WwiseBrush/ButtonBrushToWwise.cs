using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using Object = UnityEngine.Object;

/// <summary>
/// 右键选中某图片找到其在按钮中的引用
/// </summary>
public class ButtonBrushToWwise
{
    private static int _curTime;

    [MenuItem("Assets/BrushTools/刷新确认按钮音效")]
    static void BrushConfirmButtonWwise()
    {
        FindAllPrefabs(ButtonAction.Confirm);
    }

    [MenuItem("Assets/BrushTools/刷新取消按钮音效")]
    static void BrushCancelButtonWwise()
    {
        FindAllPrefabs(ButtonAction.Cancel);
    }
    [MenuItem("Assets/BrushTools/刷新按钮音效无")]
    static void BrushNoneButtonWwise()
    {
        FindAllPrefabs(ButtonAction.None);
    }
    static void FindAllPrefabs(ButtonAction buttonAction)
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
                FindNext(path, choose, parseDic, 0, buttonAction);
            }
        };
    }

    static void FindNext(string path, Object[] choose, Dictionary<string, string> parseDic, int index,
        ButtonAction buttonAction)
    {
        var obj = choose[index];
        path = AssetDatabase.GetAssetPath(obj);
        if (string.IsNullOrEmpty(path)) return;
        Debug.Log("开始查找哪里引用到资源:" + path);
        string guid = AssetDatabase.AssetPathToGUID(path);
        int startIndex = 0;
        var fileKeys = parseDic.Keys.ToList();
        var str = "";
        switch (buttonAction)
        {
            case ButtonAction.Confirm:
                str = "ButtonAction.Confirm";
                break;
            case ButtonAction.Cancel:
                str = "ButtonAction.Cancel";
                break;
        }

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
                var buttonList = go.GetComponentsInChildren<Button>(true);
                var textureName = path.Split('/').Last().Split('.').First();
                // 这里需要注意的是button本身自带的image上是这个图片 或者是button的子节点内存在这个图片都算是
                foreach (var button in buttonList)
                {
                    // button自身存在的image
                    var i = button.GetComponent<Image>();
                    if (i == null || i.sprite == null)
                    {
                        var ii = button.GetComponentsInChildren<Image>(true);
                        foreach (var iii in ii)
                        {
                            if (iii == null || iii.sprite == null || iii.sprite.texture.name != textureName) continue;
                            var buttonExtra = button.gameObject.GetOrAddComponent<ButtonExtra>();
                            if (buttonExtra.action == buttonAction) continue;
                            buttonExtra.action = buttonAction;
                            Debug.Log(
                                string.Format("<color=#3CB371>{0}</color>", $"{file}--->{button.name}") + "已被正常修改为:" +
                                str, go);
                            break;
                        }
                    }
                    else
                    {
                        if (i.sprite.texture.name != textureName)
                        {
                            var ii = button.GetComponentsInChildren<Image>(true);
                            foreach (var iii in ii)
                            {
                                if (iii == null || iii.sprite == null || iii.sprite.texture.name != textureName) continue;
                                var buttonExtra = button.gameObject.GetOrAddComponent<ButtonExtra>();
                                if (buttonExtra.action == buttonAction) continue;
                                buttonExtra.action = buttonAction;
                                Debug.Log(
                                    string.Format("<color=#3CB371>{0}</color>", $"{file}--->{button.name}") +
                                    "已被正常修改为:" + str, go);
                                break;
                            }
                        }
                        else
                        {
                            var buttonExtra = button.gameObject.GetOrAddComponent<ButtonExtra>();
                            if (buttonExtra.action == buttonAction) continue;
                            buttonExtra.action = buttonAction;
                            Debug.Log(
                                string.Format("<color=#3CB371>{0}</color>", $"{file}--->{button.name}") + "已被正常修改为:" +
                                str, go);
                        }
                    }

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

                FindNext(path, choose, parseDic, next, buttonAction);
            }
        };
    }
}