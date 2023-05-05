//using UnityEngine;
//using UnityEditor;
//using UnityEditor.Callbacks;
//using System.Collections;
//#if UNITY_IOS
//using UnityEditor.iOS.Xcode;
//#endif
//using System.IO;

//public class NativeBridgePostProcess
//{
//    [PostProcessBuild]
//    public static void ChangeXcodePlist(BuildTarget buildTarget, string pathToBuiltProject)
//    {
//        #if UNITY_IOS
//        if (buildTarget == BuildTarget.iOS)
//        {
//            // set swift compiler version
//            var path = $"{pathToBuiltProject}/Unity-iPhone.xcodeproj/project.pbxproj";
//            var pbx = new PBXProject();
//            pbx.ReadFromFile(path);
//            var targetGuid = pbx.TargetGuidByName("Unity-iPhone");
//            pbx.SetBuildProperty(targetGuid, "SWIFT_VERSION", "5.0");

//            // add swift to oc bridge
//            var content = "#import \"Gate2.h\"";
//            File.WriteAllText($"{pathToBuiltProject}/Unity-iPhone-Bridging-Header.h", content);
//            pbx.AddFile($"Unity-iPhone-Bridging-Header.h", "Unity-iPhone-Bridging-Header.h");
//            pbx.SetBuildProperty(targetGuid, "SWIFT_OBJC_BRIDGING_HEADER", "Unity-iPhone-Bridging-Header.h");
//            pbx.WriteToFile(path);
//        }
//        #endif
//    }

//}
