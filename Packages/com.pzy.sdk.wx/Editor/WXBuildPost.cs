// using UnityEngine;
// using UnityEditor;
// using UnityEditor.Callbacks;
// using System.Collections;
// #if UNITY_IOS
// using UnityEditor.iOS.Xcode;
// #endif
// using System.IO;
// using BuildPost;

// public class WXBuildPost
// {
//     [PostProcessBuild]
//     public static void ChangeXcodePlist(BuildTarget buildTarget, string pathToBuiltProject)
//     {
//         if(buildTarget == BuildTarget.iOS)
//         {
//             //IOSBuildPost(pathToBuiltProject);
//         }
//         else if(buildTarget == BuildTarget.Android)
//         {
//             AndroidBuildPost(pathToBuiltProject);
//         }
//     }

//     private static void AndroidBuildPost(string pathToBuiltProject)
//     {
//         #if UNITY_ANDROID
//         if(ProjectSetter._lastSetType != "bilibili")
//         {
//             BuildGradle.AddDependencies(pathToBuiltProject, "api 'com.tencent.mm.opensdk:wechat-sdk-android-with-mta:+'");
//         }
//         #endif
//     }

//     // private static void IOSBuildPost(string pathToBuiltProject)
//     // {
//     //     #if UNITY_IOS
//     //     if (buildTarget == BuildTarget.iOS)
//     //     {
//     //         // // edit main
//     //         // string main = FindFile(pathToBuiltProject, "main.mm");
//     //         // var code = File.ReadAllText(main);
//     //         // code = code.Replace("UnityAppController", "GameController");
//     //         // File.WriteAllText(main, code);
            
//     //         // Get plist
//     //         PlistDocument plist = new PlistDocument();
//     //         string plistPath = FindFile(pathToBuiltProject, "Info.plist");
//     //         plist.ReadFromString(File.ReadAllText(plistPath));
           
//     //         // Get root
//     //         PlistElementDict rootDict = plist.root;
//     //         // rootDict.SetString("NSPhotoLibraryAddUsageDescription", "即将保存到相册");

//     //         // add plist key
//     //         {
//     //             var arry = rootDict.CreateArray("LSApplicationQueriesSchemes");
//     //             arry.AddString("weixin");
//     //         }

//     //         {
//     //             var arry = rootDict.CreateArray("CFBundleURLTypes");
//     //             var dic = arry.AddDict();
//     //             dic.SetString("CFBundleTypeRole", "Editor");
//     //             dic.SetString("CFBundleURLName", "weixin");
//     //             var a = dic.CreateArray("CFBundleURLSchemes");
//     //             a.AddString("wx7c36eb92fbc40a24");
//     //         }

//     //         // Write to file
//     //         File.WriteAllText(plistPath, plist.WriteToString());

//     //         // // close bit code
//     //         // {
//     //         //     var path = $"{pathToBuiltProject}/Unity-iPhone.xcodeproj/project.pbxproj";
//     //         //     var text = File.ReadAllText(path);
//     //         //     text = text.Replace("ENABLE_BITCODE = YES", "ENABLE_BITCODE = NO");
//     //         //     File.WriteAllText(path, text);
//     //         // }

//     //         // // remove armv7
//     //         // {
//     //         //     var path = $"{pathToBuiltProject}/Unity-iPhone.xcodeproj/project.pbxproj";
//     //         //     var text = File.ReadAllText(path);
//     //         //     text = text.Replace("ARCHS = \"armv7 arm64\";", "ARCHS = arm64;");
//     //         //     File.WriteAllText(path, text);
//     //         // }

//     //         // // chnage sign to manuel
//     //         // {
//     //         //     var path = $"{pathToBuiltProject}/Unity-iPhone.xcodeproj/project.pbxproj";
//     //         //     var text = File.ReadAllText(path);
//     //         //     text = text.Replace("ProvisioningStyle = Automatic;", "ProvisioningStyle = Manual;");
//     //         //     File.WriteAllText(path, text);
                
//     //         // }

//     //         // // background modes
//     //         // PlistElementArray bgModes = rootDict.CreateArray("UIBackgroundModes");
//     //         // bgModes.AddString("location");
//     //         // bgModes.AddString("fetch");
           

//     //     }
//     //     #endif
//     // }

//     private static string FindFile(string dir, string filename)
//     {
//         var list = Directory.GetFiles(dir, filename, SearchOption.AllDirectories);
//         if(list.Length != 0)
//         {
//             return list[0];
//         }
//         return "";
//     }
// }