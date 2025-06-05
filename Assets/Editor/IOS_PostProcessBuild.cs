#if UNITY_EDITOR && UNITY_IOS

using System;
using System.IO;
using JerryMouse.Controller;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using UnityEngine;

namespace PostProcessBuild
{
    public class IOS_PostProcessBuild
    {
        //配置Xcode选项
        //[PostProcessBuild]
        [PostProcessBuild(88)]
        public static void OnPostProcessBuild(BuildTarget target, string targetPath)
        {
            // if (target != BuildTarget.iOS)
            // {
            //     Debug.LogWarning("Target is not iOS. XCodePostProcess will not run");
            //     return;
            // }
            //
            string xcodePath = Path.GetFullPath(targetPath);
            string projPath = xcodePath + "/Unity-iPhone.xcodeproj/project.pbxproj";
            
            PBXProject proj = new PBXProject();
            proj.ReadFromFile(projPath);
            
            // string UnityMainTargetGuid = proj.GetUnityMainTargetGuid();
            string unityFrameworkTargetGuid = proj.GetUnityFrameworkTargetGuid();
            
            //Build Property
            proj.SetBuildProperty(unityFrameworkTargetGuid, "ENABLE_BITCODE", "NO"); //BitCode  NO
            proj.SetBuildProperty(unityFrameworkTargetGuid, "GCC_ENABLE_OBJC_EXCEPTIONS", "YES"); //Enable Objective-C Exceptions
            proj.AddBuildProperty(unityFrameworkTargetGuid, "OTHER_LDFLAGS", "-ObjC");
            proj.SetBuildProperty(unityFrameworkTargetGuid, "ALWAYS_EMBED_SWIFT_STANDARD_LIBRARIES", "false");
            
            //
            // string[] headerSearchPathsToAdd =
            // {
            //     "$(SRCROOT)/Libraries/Plugins/iOS/ThinkingSDK/Source/main",
            //     "$(SRCROOT)/Libraries/Plugins/iOS/ThinkingSDK/Source/common"
            // };
            // proj.UpdateBuildProperty(unityFrameworkTargetGuid, "HEADER_SEARCH_PATHS", headerSearchPathsToAdd,
            //     null); // Header Search Paths
            //
            // //Add Frameworks
            // proj.AddFrameworkToProject(unityFrameworkTargetGuid, "WebKit.framework", true);
            // proj.AddFrameworkToProject(unityFrameworkTargetGuid, "CoreTelephony.framework", true);
            // proj.AddFrameworkToProject(unityFrameworkTargetGuid, "SystemConfiguration.framework", true);
            // proj.AddFrameworkToProject(unityFrameworkTargetGuid, "Security.framework", true);
            // proj.AddFrameworkToProject(unityFrameworkTargetGuid, "UserNotifications.framework", true);
            //
            // //Add Lib
            // proj.AddFileToBuild(unityFrameworkTargetGuid,
            //     proj.AddFile("usr/lib/libsqlite3.tbd", "libsqlite3.tbd", PBXSourceTree.Sdk));
            // proj.AddFileToBuild(unityFrameworkTargetGuid,
            //     proj.AddFile("usr/lib/libz.tbd", "libz.tbd", PBXSourceTree.Sdk));
            //
            // //Add xib
            // string sourceStoryboardPath = Application.dataPath + "/iOS/Xib/BRRaider.storyboard";
            // string targetStoryboardPath = xcodePath + "/BRRaider.storyboard";
            // try
            // {
            //     File.Copy(sourceStoryboardPath, targetStoryboardPath, true);
            //     //proj.AddFileToBuild(UnityMainTargetGuid, proj.AddFile(targetStoryboardPath, "BRRaider.storyboard"));
            // }
            // catch (Exception exp)
            // {
            //     Debug.LogError("Failed to copy BRRaider.storyboard : " + exp.ToString());
            // }
            //
            // //Add Image
            // string sourceLoadingBgPath = Application.dataPath + "/iOS/Images/img_loading_bg.png";
            // string targetLoadingBgPath = xcodePath + "/img_loading_bg.png";
            // try
            // {
            //     File.Copy(sourceLoadingBgPath, targetLoadingBgPath, true);
            // }
            // catch (Exception exp)
            // {
            //     Debug.LogError("Failed to copy img_loading_bg.png : " + exp.ToString());
            // }
            //
            // proj.WriteToFile(projPath);
            
            // //Info.plist
            string plistPath = Path.Combine(targetPath, "Info.plist");
            PlistDocument plist = new PlistDocument();
            plist.ReadFromFile(plistPath);
            plist.root.SetInteger("AppsFlyerShouldSwizzle", 1);
            plist.root.SetString("NSUserTrackingUsageDescription", "This setting is utilized to identify the device and safeguard your account security.");
            // plist.root.SetString("NSLocationAlwaysAndWhenInUseUsageDescription", "We need your location to determine eligibility for skill-based cash gaming in your region.");
            // plist.root.SetString("NSLocationWhenInUseUsageDescription", "We need your location to determine eligibility for skill-based cash gaming in your region.");
            // plist.root.SetString("NSCameraUsageDescription", "Bingo Charm used your camera library to upload avatar.");
            plist.root.SetString("NSPhotoLibraryUsageDescription", "We need access your photos for you to send it and get help.");
            plist.root.SetBoolean("ITSAppUsesNonExemptEncryption", false);

#if debug_build
               PlistElementDict plistElementDic = plist.root.CreateDict("NSAppTransportSecurity");
                plistElementDic.SetBoolean("NSAllowsArbitraryLoads", true);
                // plistElementDic.SetBoolean("NSAllowsArbitraryLoadsInWebContent", true);
                plistElementDic = plistElementDic.CreateDict("NSExceptionDomains");
                plistElementDic = plistElementDic.CreateDict("app.caslotsclub.com");
                plistElementDic.SetBoolean("NSIncludesSubdomains", true);
                plistElementDic.SetBoolean("NSTemporaryExceptionAllowsInsecureHTTPLoads", true);
                plistElementDic.SetString("NSTemporaryExceptionMinimumTLSVersion", "1.0");
                plistElementDic.SetBoolean("NSTemporaryExceptionRequiresForwardSecrecy", false);
#endif
            
            // AF归因 被广告覆盖了
            plist.root.SetString("NSAdvertisingAttributionReportEndpoint", "https://appsflyer-skadnetwork.com/");
            
            plist.WriteToFile(plistPath);

            //修正头文件路径
            XcodeFile xMainClass =
                new XcodeFile(xcodePath + "/MainApp/main.mm");
            xMainClass.Replace("<UnityFramework/UnityFramework.h>", "\"../UnityFramework/UnityFramework.h\"");

            XcodeFile unityFramework =
                new XcodeFile(xcodePath + "/UnityFramework/UnityFramework.h");
            
            unityFramework.Replace("<UnityFramework/UnityAppController.h>", "\"../Classes/UnityAppController.h\"");
            
            //复制Podfile
            // string sourcePodfilePath = Application.dataPath + "/iOS/Build/Podfile";
            // string targetPodfilePath = xcodePath + "/Podfile";
            // try
            // {
            //     File.Copy(sourcePodfilePath, targetPodfilePath, true);
            // }
            // catch (Exception exp)
            // {
            //     Debug.LogError("Failed to copy Podfile : " + exp.ToString());
            // }
        }
    }
}
#endif