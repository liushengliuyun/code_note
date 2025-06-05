using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using JerryMouse.Model.JsonConfig;
using SlotX.Utils;
using UnityEditor;
using UnityEngine;

public static class TestInEditor
{
    [MenuItem("Tools/PrintConfig")]
    public static void PrintConfig()
    {
        // AdmobConfig.Print();
        Debug.Log(XJsonUtil.Serialize(AdmobConfig.Instance));
    }
}
