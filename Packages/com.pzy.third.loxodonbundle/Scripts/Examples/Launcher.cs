// using System;
// using System.Text;
// using UnityEngine;

// using Loxodon.Framework.Bundles;
// using Loxodon.Framework.Contexts;
// using Loxodon.Framework.Security.Cryptography;

// namespace Loxodon.Framework.Examples.Bundle
// {
//     public class Launcher : MonoBehaviour
//     {
//         private Rect position = new Rect(5, 5, 150, 40);
//         private GUIContent contentON = new GUIContent("Simulation Mode: ON");
//         private GUIContent contentOFF = new GUIContent("Simulation Mode: OFF");
//         private GUIStyle style;

//         private string iv = "5Hh2390dQlVh0AqC";
//         private string key = "E4YZgiGQ0aqe5LEJ";


//         void Awake()
//         {           
//             DontDestroyOnLoad(this.gameObject);
//             var resources = CreateResources();

//             ApplicationContext context = Context.GetApplicationContext();
//             context.GetContainer().Register<IResources>(resources);
//         }

//         IResources CreateResources()
//         {
//             IResources resources = null;
// #if UNITY_EDITOR
//             if (SimulationSetting.IsSimulationMode)
//             {
//                 Debug.Log("Use SimulationResources. Run In Editor");

//                 /* Create a PathInfoParser. */
//                 //IPathInfoParser pathInfoParser = new SimplePathInfoParser("@");
//                 IPathInfoParser pathInfoParser = new SimulationAutoMappingPathInfoParser();

//                 /* Create a BundleManager */
//                 IBundleManager manager = new SimulationBundleManager();

//                 /* Create a BundleResources */
//                 resources = new SimulationResources(pathInfoParser, manager);
//             }
//             else
// #endif
//             {
//                 /* Create a BundleManifestLoader. */
//                 IBundleManifestLoader manifestLoader = new BundleManifestLoader();
//                 /* Loads BundleManifest. */
//                 BundleManifest manifest = null;
//                 var readonlyDirectory = BundleUtil.GetReadOnlyDirectory();
//                 var path = readonlyDirectory + BundleSetting.ManifestFilename;
//                 BundleManifest streamingBundleManifest = manifestLoader.Load(path);
//                 var storableDiretory = BundleUtil.GetStorableDirectory();
//                 var storageBundleManifestPath = storableDiretory + BundleSetting.ManifestFilename;
//                 if(System.IO.File.Exists(storageBundleManifestPath)) 
//                 {
//                     BundleManifest storableManifest = manifestLoader.Load(storageBundleManifestPath);
//                     var temp = storableManifest.Version.Split('.');
//                     string ver = temp[temp.Length-1];
//                     int storableVersion = int.Parse(ver);

//                     temp = streamingBundleManifest.Version.Split('.');
//                     ver = temp[temp.Length-1];
//                     int readOnlyVersion = int.Parse(ver);

//                     if(storableVersion>=readOnlyVersion) {
//                         manifest = storableManifest;
//                     } else {
//                         manifest = streamingBundleManifest;
//                     }
//                 } else { 
//                     manifest = streamingBundleManifest;
//                 }

//                 //manifest.ActiveVariants = new string[] { "", "sd" };
//                 manifest.ActiveVariants = new string[] { "", "hd" };

//                 /* Create a PathInfoParser. */
//                 //IPathInfoParser pathInfoParser = new SimplePathInfoParser("@");
//                 IPathInfoParser pathInfoParser = new AutoMappingPathInfoParser(manifest);

//                 /* Create a BundleLoaderBuilder */
//                 //ILoaderBuilder builder = new WWWBundleLoaderBuilder(new Uri(BundleUtil.GetReadOnlyDirectory()), false);

//                 /* AES128_CBC_PKCS7 */
//                 //RijndaelCryptograph rijndaelCryptograph = new RijndaelCryptograph(128, Encoding.ASCII.GetBytes(this.key), Encoding.ASCII.GetBytes(this.iv));
//                 IStreamDecryptor decryptor = CryptographUtil.GetDecryptor(Algorithm.AES128_CBC_PKCS7, Encoding.ASCII.GetBytes(this.key), Encoding.ASCII.GetBytes(this.iv));

//                 /* Use a custom BundleLoaderBuilder */
//                 ILoaderBuilder builder = new CustomBundleLoaderBuilder(new Uri(BundleUtil.GetReadOnlyDirectory()), false, decryptor);

//                 /* Create a BundleManager */
//                 IBundleManager manager = new BundleManager(manifest, builder);

//                 /* Create a BundleResources */
//                 resources = new BundleResources(pathInfoParser, manager);
//             }
//             return resources;
//         }

// #if UNITY_EDITOR
//         void OnGUI()
//         {
//             if (style == null)
//             {
//                 style = new GUIStyle();
//                 style.normal.textColor = new Color(1,0,0);
//             }

//             if (SimulationSetting.IsSimulationMode)
//                 GUI.Label(position, contentON, style);
//             else
//                 GUI.Label(position, contentOFF, style);
//         }
// #endif
//     }
// }
