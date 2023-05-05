using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

public class FindMissingWindow : EditorWindow{
        [MenuItem("Tools/检查/检查MissingReference资源")]
        public static void FindMissing()
        {
            GetWindow<FindMissingWindow>().titleContent = new GUIContent("查找Missing资源");
            GetWindow<FindMissingWindow>().Show();
            Find();
        }
        private static Dictionary<UnityEngine.Object, List<UnityEngine.Object>> prefabs = new Dictionary<UnityEngine.Object, List<UnityEngine.Object>>();
        private static Dictionary<UnityEngine.Object, string> refPaths = new Dictionary<UnityEngine.Object, string>();
        private static void Find(){
            prefabs.Clear();
            string[] allassetpaths = AssetDatabase.GetAllAssetPaths();//获取所有资源路径
            var gos = allassetpaths
                .Where(a => a.EndsWith("prefab"))//筛选 是以prefab为后缀的 预设体资源
                .Select(a => AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(a));//加载这个预设体
                //gos拿到的是所有加载好的预设体
            foreach (var item in gos){
                GameObject go = item as GameObject;
                if (go){
                    Component[] cps = go.GetComponentsInChildren<Component>(true);//获取这个物体身上所有的组件
                    foreach (var cp in cps)//遍历每一个组件
                    {
                        if (!cp)
                        {
                            if (!prefabs.ContainsKey(go))
                            {
                                prefabs.Add(go, new List<UnityEngine.Object>() { cp });
                            }
                            else
                            {
                                prefabs[go].Add(cp);
                            }
                            continue;
                        }
                        SerializedObject so = new SerializedObject(cp);//生成一个组件对应的S俄日阿里则对Object对象 用于遍历这个组件的所有属性
                        var iter = so.GetIterator();//拿到迭代器
                        while (iter.NextVisible(true))//如果有下一个属性
                        {
                        //如果这个属性类型是引用类型的
                            if (iter.propertyType == SerializedPropertyType.ObjectReference)
                            {
                            //引用对象是null 并且 引用ID不是0 说明丢失了引用
                                if (iter.objectReferenceValue == null && iter.objectReferenceInstanceIDValue != 0)
                                {
                                    if (!refPaths.ContainsKey(cp)) refPaths.Add(cp, iter.propertyPath);
                                    else refPaths[cp] += " | " + iter.propertyPath;
                                    if (prefabs.ContainsKey(go))
                                    {
                                        if(!prefabs[go].Contains(cp))prefabs[go].Add(cp);
                                    }
                                    else
                                    {
                                        prefabs.Add(go, new List<UnityEngine.Object>() { cp});
                                    }
                                }
                            }
                       }
                    }
                }
            }
            EditorUtility.DisplayDialog("", "就绪", "OK");
        }
         //以下只是将查找结果显示
        private Vector3 scroll = Vector3.zero;
        private void OnGUI()
        {
            scroll = EditorGUILayout.BeginScrollView(scroll);
            EditorGUILayout.BeginVertical();
            if (GUILayout.Button("移除丢失引用的Animator组件")) RemoveAnimatorWithMissReference();
            foreach (var item in prefabs)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.ObjectField(item.Key, typeof(GameObject), true, GUILayout.Width(200));
                EditorGUILayout.BeginVertical();
                foreach (var cp in item.Value)
                {
                    if (cp)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.ObjectField(cp, cp.GetType(), true, GUILayout.Width(200));
                        if (refPaths.ContainsKey(cp))
                        {
                            GUILayout.Label(refPaths[cp]);
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndScrollView();
       }
        private void RemoveAnimatorWithMissReference()
        {
            int count = 0;
            foreach (var cps in prefabs.Values)
            {
                foreach (var item in cps)
                {
                    Animator a = item as Animator;
                    if (a)
                    {
                        count++;
                        EditorUtility.SetDirty(a.gameObject);
                        DestroyImmediate(a, true);
                    }
                }
            }
            if (count > 0)
            {
                EditorUtility.DisplayDialog("", "一共移除" + count + "个Animator组件", "OK");
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Find();
            }
        }
   }
