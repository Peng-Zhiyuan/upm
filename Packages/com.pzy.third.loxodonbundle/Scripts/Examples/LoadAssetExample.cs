using System;
using System.Collections;
using System.Diagnostics;
using UnityEngine;

using Loxodon.Framework.Bundles;
using Loxodon.Framework.Asynchronous;
using Loxodon.Framework.Contexts;
using Debug = UnityEngine.Debug;

namespace Loxodon.Framework.Examples.Bundle
{

    public class LoadAssetExample : MonoBehaviour
    {
        private IResources resources;
        public Transform canvas;

        void Start()
        {
            ApplicationContext context = Context.GetApplicationContext();
            this.resources = context.GetService<IResources>();

            // this.Load(new string[] { "LoxodonFramework/BundleExamples/Models/Red/Red.prefab", "LoxodonFramework/BundleExamples/Models/Green/Green.prefab" });
            // this.StartCoroutine(Load2("Bundles/UI/Dynamic/WarAsset.prefab"));
        }

        private void Update() {
            if(Input.GetKeyDown(KeyCode.C)) {
            }
        }

        private void OnGUI() {
            if(GUI.Button(new Rect(Vector2.zero, new Vector2(300, 300)),  "a")) {
               this.StartCoroutine(Load2("Bundles/BeUnit/GamePlayer/BaseC.prefab"));
            }
            
            if(GUI.Button(new Rect(Vector2.one * 200, new Vector2(300, 300)),  "b")) {
                this.StartCoroutine(Load2("Bundles/BeUnit/GamePlayer/Pre_Character_02_01.prefab"));
            }
            
            if(GUI.Button(new Rect(Vector2.one * 500, new Vector2(300, 300)),  "c")) {
                this.StartCoroutine(Load2("Bundles/UI/Dynamic/WarAsset.prefab"));
            }
            
            if(GUI.Button(new Rect(Vector2.one * 800, new Vector2(300, 300)),  "d")) {
                this.StartCoroutine(Load2("Res/Code/Hotfix.dll"));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        void Load(string[] names)
        {
            IProgressResult<float, GameObject[]> result = resources.LoadAssetsAsync<GameObject>(names);
            result.Callbackable().OnProgressCallback(p =>
            {
                Debug.LogFormat("Progress:{0}%", p * 100);
            });
            result.Callbackable().OnCallback((r) =>
            {
                try
                {
                    if (r.Exception != null)
                        throw r.Exception;

                    foreach (GameObject template in r.Result)
                    {
                        GameObject.Instantiate(template);
                    }

                }
                catch (Exception e)
                {
                    Debug.LogErrorFormat("Load failure.Error:{0}", e);
                }
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        IEnumerator Load2(string name)
        {
            IProgressResult<float, GameObject> result = resources.LoadAssetAsync<GameObject>(name);
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            while (!result.IsDone)
            {
                Debug.LogFormat("Progress:{0}%", result.Progress * 100);
                yield return null;
            }

            try
            {
                if (result.Exception != null)
                    throw result.Exception;
                var go = GameObject.Instantiate(result.Result);
                if(name.Contains("UI")) {
                    go.transform.SetParent(canvas);
                    go.transform.localPosition = Vector3.zero;
                }
            }
            catch (Exception e)
            {
                Debug.LogErrorFormat("Load failure.Error:{0}", e);
            }
            Debug.Log(stopwatch.ElapsedMilliseconds);
        }
    }
}
