using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
 
public class VersionWriter : IPreprocessBuildWithReport
{
    private const string targetFile = "GameVersion.cs";
 
    public int callbackOrder => 0;
 
    public void OnPreprocessBuild(BuildReport report)
    {
        WriteVersion();
    }
 
    // [DidReloadScripts]
    public static void WriteVersion()
    {
        string finalPath = Path.Combine(Application.dataPath, targetFile);
        
        string newText = $"public static class GameVersion\r\n" +
            "{\r\n" +
            FormatVar("iOS_BuildNumber", PlayerSettings.iOS.buildNumber) +
            FormatVar("Android_BuildNumber", PlayerSettings.Android.bundleVersionCode.ToString()) +
            FormatVar("build_time_stamp", System.DateTime.Now.ToString()) +
            "}";
        
        string currentText = File.ReadAllText(finalPath);
        
        if (currentText != newText)
        {
            Debug.Log("Updated GameVersion.cs");
        
            File.WriteAllText(finalPath, newText);
            AssetDatabase.Refresh();
        }
    }
 
    private static string FormatVar(string varName, string varValue)
    {
        return $"    public const string {varName} = \"{varValue}\";\r\n";
    }
}